using System.Security.Cryptography;
using System.Text;

namespace CemesMultiplayerSudoku.GameSession.Services;

public class IdentificationGenerator
{
    private static readonly char[] AllowedGameCodeCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890".ToArray();

    private readonly ILogger<IdentificationGenerator> _logger;

    private long _lastPlayerId;
    private readonly SemaphoreSlim _playerSemaphore = new(1);
    private readonly SemaphoreSlim _gameCodeSemaphore = new(1);

    public IdentificationGenerator(ILogger<IdentificationGenerator> logger) => _logger = logger;

    public async Task<long> GeneratePlayerId()
    {
        await _playerSemaphore.WaitAsync();

        try
        {
            _logger.LogInformation("Generated new player id. Last id: {lastPlayerId} - New id {newPlayerId}", _lastPlayerId, ++_lastPlayerId);
            return _lastPlayerId;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while generating a new player id.");
            throw;
        }
        finally
        {
            _playerSemaphore.Release();
        }
    }

    public async Task<string> GenerateGameCode()
    {
        await _gameCodeSemaphore.WaitAsync();

        try
        {
            var gameCode = $"APO-{RandomNumberGenerator.GetString(AllowedGameCodeCharacters, 28)}";
            _logger.LogInformation("Generated new game code. Code: {gameCode}", gameCode);
            return gameCode;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while generating a new game code.");
            throw;
        }
        finally
        {
            _gameCodeSemaphore.Release();
        }
    }

    public Task<string> GenerateSessionToken(long playerId)
    {
        var salt = new byte[512];
        RandomNumberGenerator.Fill(salt);
        var playerIdBytes = BitConverter.GetBytes(playerId);
        for (var i = 0; i < playerIdBytes.Length; i++)
            salt[255 + i] = playerIdBytes[i];
        return Task.FromResult(Convert.ToBase64String(salt));
    }
}