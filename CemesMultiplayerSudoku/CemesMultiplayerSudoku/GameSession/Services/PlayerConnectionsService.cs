using System.Security.Cryptography;
using CemesMultiplayerSudoku.Contract.GameSession.Dtos;
using CemesMultiplayerSudoku.Contract.GameSession.Hubs;
using CemesMultiplayerSudoku.GameSession.Hubs;
using CemesMultiplayerSudoku.GameSession.Mappings;
using CemesMultiplayerSudoku.GameSession.Services.Components;
using Microsoft.AspNetCore.SignalR;

namespace CemesMultiplayerSudoku.GameSession.Services;

public class PlayerConnectionsService
{
    private readonly IdentificationGenerator _identificationGenerator;
    private readonly GamesManagerService _gamesManagerService;
    private readonly IHubContext<GameSessionHub, IGameSessionHubClient> _gameSessionHubContext;

    private readonly List<Player> _connectedPlayers = new();

    public PlayerConnectionsService(
        IdentificationGenerator identificationGenerator,
        GamesManagerService gamesManagerService,
        IHubContext<GameSessionHub, IGameSessionHubClient> gameSessionHubContext)
    {
        _identificationGenerator = identificationGenerator;
        _gamesManagerService = gamesManagerService;
        _gameSessionHubContext = gameSessionHubContext;
    }

    public async Task PlayerConnected(string connectionId)
    {
        var player = new Player
        {
            Id = await _identificationGenerator.GeneratePlayerId(),
            ConnectionId = connectionId
        };

        player.SessionToken = await _identificationGenerator.GenerateSessionToken(player.Id);
        _connectedPlayers.Add(player);
        await _gameSessionHubContext.Clients.Client(connectionId).SessionCreated(player.SessionToken, player.Id);
    }

    public async Task PlayerDisconnected(string connectionId)
    {
        var playerHit = _connectedPlayers.FirstOrDefault(x => x.ConnectionId == connectionId);
        if (playerHit is null)
            return;

        await _gamesManagerService.DisconnectPlayer(playerHit);
    }

    private static readonly char[] HexCharacters = new[] { 'A', 'B', 'C', 'D', 'E', 'F', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };
    public async Task<string?> SetUser(string sessionToken, string name, string? color)
    {
        if (string.IsNullOrEmpty(name))
            return "Bitte einen Namen angeben.";

        var playerHit = _connectedPlayers.FirstOrDefault(x => x.SessionToken == sessionToken);
        if (playerHit is null)
            return "SessionToken ist ungültig.";

        playerHit.Color = color ?? $"#{RandomNumberGenerator.GetString(HexCharacters, 6)}";
        playerHit.Name = name;
        return null;
    }

    public IEnumerable<PlayerDto> GetConnectedPlayers() => _connectedPlayers.Select(x => x.MapToDto());

    internal Player? GetPlayer(string sessionToken) => _connectedPlayers.FirstOrDefault(x => x.SessionToken == sessionToken);
}