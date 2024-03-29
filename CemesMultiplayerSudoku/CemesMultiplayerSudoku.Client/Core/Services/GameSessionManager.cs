using System.Buffers;
using CemesMultiplayerSudoku.Client.Core.Models;
using CemesMultiplayerSudoku.Client.Core.Services.Handlers;
using CemesMultiplayerSudoku.Contract.GameSession.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace CemesMultiplayerSudoku.Client.Core.Services;

public class GameSessionManager
{
    public GameStateModel? GameState { get; set; }
    public bool IsInitialized { get; private set; }
    public bool IsUserSet => _apiService.IsUserSet;

    public long PlayerId { get; private set; }
    public string? SessionToken { get; private set; }

    public List<GameEventModel> Events { get; } = new();

    private readonly List<StateChangedHandler> _stateChangedHandlers = new();

    private readonly ILogger<GameSessionManager> _logger;
    private readonly NavigationManager _navigationManager;
    private readonly ApiService _apiService;

    public GameSessionManager(
        ILogger<GameSessionManager> logger,
        NavigationManager navigationManager,
        ApiService apiService)
    {
        _logger = logger;
        _navigationManager = navigationManager;
        _apiService = apiService;
    }

    public IDisposable RegisterStateChangedHandler(Func<Task> func)
    {
        var handler = new StateChangedHandler(func, this);
        _stateChangedHandlers.Add(handler);
        return handler;
    }

    public Task InitializeSignalR()
    {
        var notifyHubUrl = _navigationManager.BaseUri.TrimEnd('/') + "/hub/game-session";
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(notifyHubUrl)
            .WithAutomaticReconnect()
            .Build();

        hubConnection.On<string, long>("SessionCreated", SessionCreated);
        hubConnection.On<GameDto>("JoinedGame", JoinedGame);
        hubConnection.On("LeftGame", LeftGame);
        hubConnection.On<CellStateDto[][]>("UpdateBoard", UpdateBoard);
        hubConnection.On<PlayerDto>("PlayerJoined", PlayerJoined);
        hubConnection.On<PlayerDto>("PlayerLeft", PlayerLeft);
        hubConnection.On<PlayerDto, byte, byte>("PlayerSelectedField", PlayerSelectedField);
        hubConnection.On<PlayerDto>("PlayerDeselectedField", PlayerDeselectedField);
        hubConnection.On<PlayerDto, byte, byte, byte, bool>("PlayerSetNumber", PlayerSetNumber);
        hubConnection.On<PlayerDto, byte, byte>("PlayerErasedNumber", PlayerErasedNumber);
        hubConnection.On<PlayerDto, byte, byte, byte>("PlayerSetNote", PlayerSetNote);
        hubConnection.On<PlayerDto, byte, byte, byte>("PlayerErasedNote", PlayerErasedNote);
        hubConnection.On<PlayerDto, byte, byte, byte>("PlayerHint", PlayerHint);

        IsInitialized = true;
        return hubConnection.StartAsync();
    }

    internal void RemoveHandler(StateChangedHandler handler) => _stateChangedHandlers.Remove(handler);

    private async Task InvokeStateChangedHandlers()
    {
        var handlerCount = _stateChangedHandlers.Count;
        var stateChangedHandlersCopy = ArrayPool<StateChangedHandler>.Shared.Rent(handlerCount);
        _stateChangedHandlers.CopyTo(stateChangedHandlersCopy);

        var tasks = new Task[handlerCount];
        try
        {
            for (var i = 0; i < handlerCount; i++)
                tasks[i] = stateChangedHandlersCopy[i].Func();

            await Task.WhenAll(tasks);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception while invocation of a [StateChangedHandler].");
        }
    }

    private Task SessionCreated(string sessionToken, long playerId)
    {
        SessionToken = sessionToken;
        PlayerId = playerId;
        _apiService.SetSessionToken(SessionToken);
        return InvokeStateChangedHandlers();
    }

    private Task JoinedGame(GameDto game)
    {
        Events.Clear();
        AddEvent(null, $"Spiel '{game.Name}' mit Code '{game.Code}' beigetreten!");
        
        GameState = new GameStateModel(game);
        return InvokeStateChangedHandlers();
    }

    private Task LeftGame()
    {
        GameState = null;
        return InvokeStateChangedHandlers();
    }

    private Task UpdateBoard(CellStateDto[][] boardState)
    {
        AddEvent(null, "Board wurde aktualisiert!");
        GameState?.SetBoardState(boardState);
        return InvokeStateChangedHandlers();
    }

    private Task PlayerJoined(PlayerDto player)
    {
        AddEvent(null, $"Der Spieler '{player.Name}' ist beigetreten.");
        GameState?.AddPlayer(player);
        return InvokeStateChangedHandlers();
    }

    private Task PlayerLeft(PlayerDto player)
    {
        AddEvent(null, $"Der Spieler '{player.Name}' hat das Spiel verlassen.");
        GameState?.DeselectPlayerFields(player);
        GameState?.RemovePlayer(player);
        return InvokeStateChangedHandlers();
    }

    private Task PlayerSelectedField(PlayerDto player, byte x, byte y)
    {
        if (GameState is null)
            return InvokeStateChangedHandlers();

        GameState.DeselectPlayerFields(player);

        var players = GameState.BoardState[x][y].SelectedByPlayers;
        if (players.Any(c => c.Id == player.Id))
            return Task.CompletedTask;

        players.Add(player);
        return InvokeStateChangedHandlers();
    }

    private Task PlayerDeselectedField(PlayerDto player)
    {
        var gotHit = GameState?.DeselectPlayerFields(player) ?? false;
        return gotHit ? InvokeStateChangedHandlers() : Task.CompletedTask;
    }

    private Task PlayerSetNumber(PlayerDto player, byte value, byte x, byte y, bool wasCorrect)
    {
        AddEvent(null, $"'{player.Name}' hat die Zahl {value} in Feld {x}/{y} gesetzt.");
        
        if (GameState is null)
            return InvokeStateChangedHandlers();

        var cell = GameState.BoardState[x][y];
        cell.Dto.Value = value;
        cell.Dto.Notes.Clear();
        cell.Dto.IsHint = false;
        cell.Dto.IsCorrect = wasCorrect;

        return InvokeStateChangedHandlers();
    }

    private Task PlayerErasedNumber(PlayerDto player, byte x, byte y)
    {
        AddEvent(null, $"'{player.Name}' hat das Feld {x}/{y} geleert.");
        
        if (GameState is null)
            return InvokeStateChangedHandlers();

        var cell = GameState.BoardState[x][y];
        cell.Dto.Value = 0;
        cell.Dto.Notes.Clear();
        cell.Dto.IsHint = false;
        cell.Dto.IsCorrect = true;

        return InvokeStateChangedHandlers();
    }

    private Task PlayerSetNote(PlayerDto player, byte value, byte x, byte y)
    {
        if (GameState is null)
            return InvokeStateChangedHandlers();

        var cell = GameState.BoardState[x][y];
        cell.Dto.Value = 0;
        cell.Dto.Notes.Add(value);
        cell.Dto.IsHint = false;
        cell.Dto.IsCorrect = true;

        return InvokeStateChangedHandlers();
    }

    private Task PlayerErasedNote(PlayerDto player, byte value, byte x, byte y)
    {
        if (GameState is null)
            return InvokeStateChangedHandlers();

        var cell = GameState.BoardState[x][y];
        cell.Dto.Value = 0;
        cell.Dto.Notes.Remove(value);
        cell.Dto.IsHint = false;
        cell.Dto.IsCorrect = true;

        return InvokeStateChangedHandlers();
    }

    private Task PlayerHint(PlayerDto player, byte value, byte x, byte y)
    {
        AddEvent(null, $"'{player.Name}' hat einen Tip für das Feld {x}/{y} gefordert.");
        
        if (GameState is null)
            return InvokeStateChangedHandlers();

        var cell = GameState.BoardState[x][y];
        cell.Dto.Value = value;
        cell.Dto.Notes.Clear();
        cell.Dto.IsHint = true;
        cell.Dto.IsCorrect = true;

        return InvokeStateChangedHandlers();
    }

    private void AddEvent(PlayerDto? player, string message)
    {
        if (Events.Count > 20)
        {
            var toRemove = Events.Take(Events.Count - 20).ToList();
            toRemove.ForEach(x => Events.Remove(x));
        }

        Events.Add(new GameEventModel(player, message));
    }
}