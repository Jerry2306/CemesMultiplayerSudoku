using CemesMultiplayerSudoku.Contract.GameSession.Dtos;
using CemesMultiplayerSudoku.Contract.GameSession.Hubs;
using CemesMultiplayerSudoku.GameSession.Hubs;
using CemesMultiplayerSudoku.GameSession.Mappings;
using CemesMultiplayerSudoku.GameSession.Services.Components;
using CemesMultiplayerSudoku.GameSession.Services.Database;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CemesMultiplayerSudoku.GameSession.Services;

public class GamesManagerService
{
    private readonly IdentificationGenerator _identificationGenerator;
    private readonly IHubContext<GameSessionHub, IGameSessionHubClient> _gameSessionHubContext;

    private readonly List<GameContainer> _activeGames = new();

    public GamesManagerService(
        IdentificationGenerator identificationGenerator,
        IHubContext<GameSessionHub, IGameSessionHubClient> gameSessionHubContext)
    {
        _identificationGenerator = identificationGenerator;
        _gameSessionHubContext = gameSessionHubContext;
    }

    public async Task DisconnectPlayer(Player player)
    {
        var currentPlayerGames = _activeGames.Where(x => x.Players.Contains(player));
        foreach (var currentPlayerGame in currentPlayerGames)
        {
            currentPlayerGame.Players.Remove(player);
            var otherPlayers = currentPlayerGame.Players.Select(x => x.ConnectionId).ToList();
            if (otherPlayers.Count > 0)
                await _gameSessionHubContext.Clients.Clients(otherPlayers).PlayerLeft(player.MapToDto());
        }

        var emptyGames = _activeGames.Where(x => x.Players.Count == 0).ToList();
        emptyGames.ForEach(x => _activeGames.Remove(x));
    }

    public async Task<string?> CreateGame(Player player, string name)
    {
        var newGame = new GameContainer
        {
            Name = name,
            Code = await _identificationGenerator.GenerateGameCode(),
            HostPlayerId = player.Id,
            Players = new() { player },
            StartedAt = DateTime.Now
        };

        await using var context = new SudokuContext();
        var sudokuHit = await context.Sudokus.Where(x => x.Missing >= 25 && x.Missing <= 61).OrderBy(_ => Guid.NewGuid()).FirstOrDefaultAsync();
        if (sudokuHit is null)
            return "Keine Sudokus gefunden... :(";

        try
        {
            for (var i = 0; i < 9; i++)
            {
                var solvedRow = sudokuHit.Solved.Substring(i * 9, 9);
                var unsolvedRow = sudokuHit.Unsolved.Substring(i * 9, 9);

                for (var j = 0; j < 9; j++)
                {
                    newGame.SolvedBoard[i][j] = byte.Parse(solvedRow[j].ToString());
                    var unsolvedValue = byte.Parse(unsolvedRow[j].ToString());
                    newGame.BoardState[i][j] = new CellStateDto(unsolvedValue, unsolvedValue != 0);
                }
            }
        }
        catch (Exception e)
        {
            return $"Ungültige Sudoku-Definition für Sudoku mit der ID [{sudokuHit.Id}]. Message: {e.Message}";
        }

        _activeGames.Add(newGame);
        await _gameSessionHubContext.Clients.Client(player.ConnectionId).JoinedGame(newGame.MapToDto());
        return null;
    }

    public async Task<string?> JoinGame(Player player, string code)
    {
        var gameHit = _activeGames.FirstOrDefault(x => x.Code == code);
        if (gameHit is null)
            return $"Spiel mit Code [{code}] nicht gefunden.";

        await LeaveGame(player);

        await _gameSessionHubContext.Clients.Clients(gameHit.Players.Select(x => x.ConnectionId))
            .PlayerJoined(player.MapToDto());
        gameHit.Players.Add(player);
        await _gameSessionHubContext.Clients.Client(player.ConnectionId).JoinedGame(gameHit.MapToDto());
        return null;
    }

    public async Task<string?> LeaveGame(Player player)
    {
        var currentPlayerGames = _activeGames.Where(x => x.Players.Contains(player));
        foreach (var currentPlayerGame in currentPlayerGames)
        {
            currentPlayerGame.Players.Remove(player);
            var otherPlayers = currentPlayerGame.Players.Select(x => x.ConnectionId).ToList();
            if (otherPlayers.Count > 0)
                await _gameSessionHubContext.Clients.Clients(otherPlayers)
                    .PlayerLeft(player.MapToDto());

            await _gameSessionHubContext.Clients.Client(player.ConnectionId).LeftGame();
        }

        var emptyGames = _activeGames.Where(x => x.Players.Count == 0).ToList();
        emptyGames.ForEach(x => _activeGames.Remove(x));
        return null;
    }

    public async Task<string?> SelectField(Player player, byte x, byte y)
    {
        var currentPlayerGames = _activeGames.Where(c => c.Players.Contains(player)).ToList();
        if (currentPlayerGames.Count != 1)
            return "Der Benutzer befindet sich in mehreren Spielen oder keinem Spiel.";

        var gameHit = currentPlayerGames.First();
        await _gameSessionHubContext.Clients.Clients(gameHit.Players.Select(c => c.ConnectionId))
            .PlayerSelectedField(player.MapToDto(), x, y);
        return null;
    }

    public async Task<string?> DeselectField(Player player)
    {
        var currentPlayerGames = _activeGames.Where(c => c.Players.Contains(player)).ToList();
        if (currentPlayerGames.Count != 1)
            return "Der Benutzer befindet sich in mehreren Spielen oder keinem Spiel.";

        var gameHit = currentPlayerGames.First();
        await _gameSessionHubContext.Clients.Clients(gameHit.Players.Select(c => c.ConnectionId))
            .PlayerDeselectedField(player.MapToDto());
        return null;
    }

    public async Task<string?> SetNumber(Player player, byte value, byte x, byte y)
    {
        var currentPlayerGames = _activeGames.Where(c => c.Players.Contains(player)).ToList();
        if (currentPlayerGames.Count != 1)
            return "Der Benutzer befindet sich in mehreren Spielen oder keinem Spiel.";

        var gameHit = currentPlayerGames.First();
        var cell = gameHit.BoardState[x][y];
        if (cell.WasPrefilled)
            return null;

        var isCorrect = gameHit.SolvedBoard[x][y] == value;
        cell.Notes.Clear();
        cell.Value = value;
        cell.IsCorrect = isCorrect;

        await _gameSessionHubContext.Clients.Clients(gameHit.Players.Select(c => c.ConnectionId))
            .PlayerSetNumber(player.MapToDto(), value, x, y, isCorrect);
        return null;
    }

    public async Task<string?> EraseNumber(Player player, byte x, byte y)
    {
        var currentPlayerGames = _activeGames.Where(c => c.Players.Contains(player)).ToList();
        if (currentPlayerGames.Count != 1)
            return "Der Benutzer befindet sich in mehreren Spielen oder keinem Spiel.";

        var gameHit = currentPlayerGames.First();
        var cell = gameHit.BoardState[x][y];
        if (cell.WasPrefilled)
            return null;

        cell.Notes.Clear();
        cell.Value = 0;
        cell.IsCorrect = true;

        await _gameSessionHubContext.Clients.Clients(gameHit.Players.Select(c => c.ConnectionId))
            .PlayerErasedNumber(player.MapToDto(), x, y);
        return null;
    }

    public async Task<string?> SetNote(Player player, byte value, byte x, byte y)
    {
        var currentPlayerGames = _activeGames.Where(c => c.Players.Contains(player)).ToList();
        if (currentPlayerGames.Count != 1)
            return "Der Benutzer befindet sich in mehreren Spielen oder keinem Spiel.";

        var gameHit = currentPlayerGames.First();
        var cell = gameHit.BoardState[x][y];
        if (cell.Value != 0)
            return null;

        if (cell.Notes.Contains(value))
            return null;

        cell.Notes.Add(value);

        await _gameSessionHubContext.Clients.Clients(gameHit.Players.Select(c => c.ConnectionId))
            .PlayerSetNote(player.MapToDto(), value, x, y);
        return null;
    }

    public async Task<string?> EraseNote(Player player, byte value, byte x, byte y)
    {
        var currentPlayerGames = _activeGames.Where(c => c.Players.Contains(player)).ToList();
        if (currentPlayerGames.Count != 1)
            return "Der Benutzer befindet sich in mehreren Spielen oder keinem Spiel.";

        var gameHit = currentPlayerGames.First();
        var cell = gameHit.BoardState[x][y];
        if (cell.Value != 0)
            return null;

        if (!cell.Notes.Contains(value))
            return null;

        cell.Notes.Remove(value);

        await _gameSessionHubContext.Clients.Clients(gameHit.Players.Select(c => c.ConnectionId))
            .PlayerErasedNote(player.MapToDto(), value, x, y);
        return null;
    }

    public async Task<string?> GetHint(Player player, byte x, byte y)
    {
        var currentPlayerGames = _activeGames.Where(c => c.Players.Contains(player)).ToList();
        if (currentPlayerGames.Count != 1)
            return "Der Benutzer befindet sich in mehreren Spielen oder keinem Spiel.";

        var gameHit = currentPlayerGames.First();
        var cell = gameHit.BoardState[x][y];
        if (cell.Value != 0)
            return null;

        var correctValue = gameHit.SolvedBoard[x][y];
        cell.Notes.Clear();
        cell.Value = correctValue;
        cell.IsHint = true;

        await _gameSessionHubContext.Clients.Clients(gameHit.Players.Select(c => c.ConnectionId))
            .PlayerHint(player.MapToDto(), correctValue, x, y);
        return null;
    }

    public async Task<string?> NewBoard(Player player)
    {
        var currentPlayerGames = _activeGames.Where(c => c.Players.Contains(player)).ToList();
        if (currentPlayerGames.Count != 1)
            return "Der Benutzer befindet sich in mehreren Spielen oder keinem Spiel.";

        var gameHit = currentPlayerGames.First();
        if (gameHit.HostPlayerId != player.Id)
            return "Nur der Host darf ein neues Board anfordern.";

        await using var context = new SudokuContext();
        var sudokuHit = await context.Sudokus.Where(x => x.Missing >= 25 && x.Missing <= 61).OrderBy(_ => Guid.NewGuid()).FirstOrDefaultAsync();
        if (sudokuHit is null)
            return "Keine Sudokus gefunden... :(";

        try
        {
            for (var i = 0; i < 9; i++)
            {
                var solvedRow = sudokuHit.Solved.Substring(i * 9, 9);
                var unsolvedRow = sudokuHit.Unsolved.Substring(i * 9, 9);

                for (var j = 0; j < 9; j++)
                {
                    gameHit.SolvedBoard[i][j] = byte.Parse(solvedRow[j].ToString());
                    var unsolvedValue = byte.Parse(unsolvedRow[j].ToString());
                    gameHit.BoardState[i][j] = new CellStateDto(unsolvedValue, unsolvedValue != 0);
                }
            }
        }
        catch (Exception e)
        {
            return $"Ungültige Sudoku-Definition für Sudoku mit der ID [{sudokuHit.Id}]. Message: {e.Message}";
        }

        await _gameSessionHubContext.Clients.Clients(gameHit.Players.Select(x => x.ConnectionId)).UpdateBoard(gameHit.BoardState);
        return null;
    }
}