using CemesMultiplayerSudoku.GameSession.Services;
using CemesMultiplayerSudoku.GameSession.Services.Components;
using Microsoft.AspNetCore.Mvc;

namespace CemesMultiplayerSudoku.GameSession.Controllers;

[ApiController]
public class GameSessionController : ControllerBase
{
    private readonly PlayerConnectionsService _playerConnectionsService;
    private readonly GamesManagerService _gamesManagerService;

    public GameSessionController(
        PlayerConnectionsService playerConnectionsService,
        GamesManagerService gamesManagerService)
    {
        _playerConnectionsService = playerConnectionsService;
        _gamesManagerService = gamesManagerService;
    }

    [HttpPost("game-session/set-user")]
    public async Task<IActionResult> SetUser([FromHeader(Name = "x-session-token")] string sessionToken, [FromQuery] string name, [FromQuery] string? color)
    {
        var errorMessage = await _playerConnectionsService.SetUser(sessionToken, name, color);
        return !string.IsNullOrEmpty(errorMessage) ? BadRequest(errorMessage) : Ok();
    }

    [HttpPost("game-session/create-game")]
    public async Task<IActionResult> CreateGame([FromHeader(Name = "x-session-token")] string sessionToken, [FromQuery] string name)
    {
        if (string.IsNullOrEmpty(name))
            return BadRequest("Bitte einen Spielnamen angeben.");

        var player = GetInitializedPlayer(sessionToken, out var errorMessage);
        if (player is null)
            return BadRequest(errorMessage);

        errorMessage = await _gamesManagerService.CreateGame(player, name);
        return !string.IsNullOrEmpty(errorMessage) ? BadRequest(errorMessage) : Ok();
    }

    [HttpPost("game-session/join-game")]
    public async Task<IActionResult> JoinGame([FromHeader(Name = "x-session-token")] string sessionToken, [FromQuery] string code)
    {
        if (string.IsNullOrEmpty(code))
            return BadRequest("Bitte einen Spiel-Code angeben.");

        var player = GetInitializedPlayer(sessionToken, out var errorMessage);
        if (player is null)
            return BadRequest(errorMessage);

        errorMessage = await _gamesManagerService.JoinGame(player, code);
        return !string.IsNullOrEmpty(errorMessage) ? BadRequest(errorMessage) : Ok();
    }

    [HttpPost("game-session/leave-game")]
    public async Task<IActionResult> LeaveGame([FromHeader(Name = "x-session-token")] string sessionToken)
    {
        var player = GetInitializedPlayer(sessionToken, out var errorMessage);
        if (player is null)
            return BadRequest(errorMessage);

        errorMessage = await _gamesManagerService.LeaveGame(player);
        return !string.IsNullOrEmpty(errorMessage) ? BadRequest(errorMessage) : Ok();
    }

    [HttpPost("game-session/select-field")]
    public async Task<IActionResult> SelectField([FromHeader(Name = "x-session-token")] string sessionToken, [FromQuery] byte x, [FromQuery] byte y)
    {
        var player = GetInitializedPlayer(sessionToken, out var errorMessage);
        if (player is null)
            return BadRequest(errorMessage);

        if (x >= 9 || y >= 9)
            return BadRequest("Ungültige Positionen auf dem Spielfeld.");

        errorMessage = await _gamesManagerService.SelectField(player, x, y);
        return !string.IsNullOrEmpty(errorMessage) ? BadRequest(errorMessage) : Ok();
    }

    [HttpPost("game-session/deselect-field")]
    public async Task<IActionResult> DeselectField([FromHeader(Name = "x-session-token")] string sessionToken)
    {
        var player = GetInitializedPlayer(sessionToken, out var errorMessage);
        if (player is null)
            return BadRequest(errorMessage);

        errorMessage = await _gamesManagerService.DeselectField(player);
        return !string.IsNullOrEmpty(errorMessage) ? BadRequest(errorMessage) : Ok();
    }

    [HttpPost("game-session/set-number")]
    public async Task<IActionResult> SetNumber([FromHeader(Name = "x-session-token")] string sessionToken, [FromQuery] byte value, [FromQuery] byte x, [FromQuery] byte y)
    {
        var player = GetInitializedPlayer(sessionToken, out var errorMessage);
        if (player is null)
            return BadRequest(errorMessage);

        if (x >= 9 || y >= 9)
            return BadRequest("Ungültige Positionen auf dem Spielfeld.");

        if (value is 0 or > 9)
            return BadRequest("Ungültiger Wert für ein Sudoku Feld.");

        errorMessage = await _gamesManagerService.SetNumber(player, value, x, y);
        return !string.IsNullOrEmpty(errorMessage) ? BadRequest(errorMessage) : Ok();
    }

    [HttpPost("game-session/erase-number")]
    public async Task<IActionResult> EraseNumber([FromHeader(Name = "x-session-token")] string sessionToken, [FromQuery] byte x, [FromQuery] byte y)
    {
        var player = GetInitializedPlayer(sessionToken, out var errorMessage);
        if (player is null)
            return BadRequest(errorMessage);

        if (x >= 9 || y >= 9)
            return BadRequest("Ungültige Positionen auf dem Spielfeld.");

        errorMessage = await _gamesManagerService.EraseNumber(player, x, y);
        return !string.IsNullOrEmpty(errorMessage) ? BadRequest(errorMessage) : Ok();
    }

    [HttpPost("game-session/set-note")]
    public async Task<IActionResult> SetNote([FromHeader(Name = "x-session-token")] string sessionToken, [FromQuery] byte value, [FromQuery] byte x, [FromQuery] byte y)
    {
        var player = GetInitializedPlayer(sessionToken, out var errorMessage);
        if (player is null)
            return BadRequest(errorMessage);

        if (x >= 9 || y >= 9)
            return BadRequest("Ungültige Positionen auf dem Spielfeld.");

        if (value is 0 or > 9)
            return BadRequest("Ungültiger Wert für ein Sudoku Feld.");

        errorMessage = await _gamesManagerService.SetNote(player, value, x, y);
        return !string.IsNullOrEmpty(errorMessage) ? BadRequest(errorMessage) : Ok();
    }

    [HttpPost("game-session/erase-note")]
    public async Task<IActionResult> EraseNote([FromHeader(Name = "x-session-token")] string sessionToken, [FromQuery] byte value, [FromQuery] byte x, [FromQuery] byte y)
    {
        var player = GetInitializedPlayer(sessionToken, out var errorMessage);
        if (player is null)
            return BadRequest(errorMessage);

        if (x >= 9 || y >= 9)
            return BadRequest("Ungültige Positionen auf dem Spielfeld.");

        if (value is 0 or > 9)
            return BadRequest("Ungültiger Wert für ein Sudoku Feld.");

        errorMessage = await _gamesManagerService.EraseNote(player, value, x, y);
        return !string.IsNullOrEmpty(errorMessage) ? BadRequest(errorMessage) : Ok();
    }

    [HttpPost("game-session/get-hint")]
    public async Task<IActionResult> GetHint([FromHeader(Name = "x-session-token")] string sessionToken, [FromQuery] byte x, [FromQuery] byte y)
    {
        var player = GetInitializedPlayer(sessionToken, out var errorMessage);
        if (player is null)
            return BadRequest(errorMessage);

        if (x >= 9 || y >= 9)
            return BadRequest("Ungültige Positionen auf dem Spielfeld.");

        errorMessage = await _gamesManagerService.GetHint(player, x, y);
        return !string.IsNullOrEmpty(errorMessage) ? BadRequest(errorMessage) : Ok();
    }

    [HttpPost("game-session/new-board")]
    public async Task<IActionResult> NewBoard([FromHeader(Name = "x-session-token")] string sessionToken)
    {
        var player = GetInitializedPlayer(sessionToken, out var errorMessage);
        if (player is null)
            return BadRequest(errorMessage);

        errorMessage = await _gamesManagerService.NewBoard(player);
        return !string.IsNullOrEmpty(errorMessage) ? BadRequest(errorMessage) : Ok();
    }

    [HttpPost("game-session/undo-last")]
    public IActionResult UndoLast([FromHeader(Name = "x-session-token")] string sessionToken) => BadRequest("Not implemented");

    [HttpGet("game-session/all-players")]
    public IActionResult GetAllPlayers() => Ok(_playerConnectionsService.GetConnectedPlayers());

    //[HttpGet("game-session/all-games")]
    //public IActionResult GetAllGames() => Ok(_gamesManagerService.Ga);

    private Player? GetInitializedPlayer(string sessionToken, out string errorMessage)
    {
        var player = _playerConnectionsService.GetPlayer(sessionToken);
        if (player is null)
        {
            errorMessage = "Ungültiger SessionToken.";
            return null;
        }

        if (!player.IsInitialized())
        {
            errorMessage = "Bitte vorerst einen Namen und Farbe auswählen.";
            return null;
        }

        errorMessage = string.Empty;
        return player;
    }
}