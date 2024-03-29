using CemesMultiplayerSudoku.Contract.GameSession.Hubs;
using CemesMultiplayerSudoku.GameSession.Services;
using Microsoft.AspNetCore.SignalR;

namespace CemesMultiplayerSudoku.GameSession.Hubs;

public class GameSessionHub : Hub<IGameSessionHubClient>
{
    private readonly ILogger<GameSessionHub> _logger;
    private readonly PlayerConnectionsService _connectionsService;

    public GameSessionHub(
        ILogger<GameSessionHub> logger,
        PlayerConnectionsService connectionsService)
    {
        _logger = logger;
        _connectionsService = connectionsService;
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogInformation("Player with ConnectionId [{ConnectionId}] connected.", Context.ConnectionId);
        return _connectionsService.PlayerConnected(Context.ConnectionId);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception is not null)
            _logger.LogError(exception, "The client [{ConnectionId}] disconnected due to an exception.", Context.ConnectionId);

        return _connectionsService.PlayerDisconnected(Context.ConnectionId);
    }
}