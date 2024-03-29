using CemesMultiplayerSudoku.Contract.GameSession.Dtos;

namespace CemesMultiplayerSudoku.GameSession.Services.Components;

public class GameContainer
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long HostPlayerId { get; set; }
    public List<Player> Players { get; set; } = new();
    public DateTime StartedAt { get; set; }
    public CellStateDto[][] BoardState { get; set; }
    public byte[][] SolvedBoard { get; set; }

    public GameContainer()
    {
        BoardState = new CellStateDto[9][];
        for (var i = 0; i < 9; i++)
            BoardState[i] = new CellStateDto[9];

        SolvedBoard = new byte[9][];
        for (var i = 0; i < 9; i++)
            SolvedBoard[i] = new byte[9];
    }
}