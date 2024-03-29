using CemesMultiplayerSudoku.Contract.GameSession.Dtos;

namespace CemesMultiplayerSudoku.Client.Core.Models;

public class GameEventModel
{
    public DateTime Timestamp { get; set; }
    public PlayerDto? Player { get; set; }
    public string Message { get; set; }

    public GameEventModel(PlayerDto? player, string message)
    {
        Timestamp = DateTime.Now;
        Player = player;
        Message = message;
    }
}