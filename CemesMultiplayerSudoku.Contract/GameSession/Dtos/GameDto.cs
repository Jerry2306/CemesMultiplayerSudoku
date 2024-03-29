namespace CemesMultiplayerSudoku.Contract.GameSession.Dtos;

public class GameDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long HostPlayerId { get; set; }
    public PlayerDto[] Players { get; set; } = Array.Empty<PlayerDto>();
    public DateTime StartedAt { get; set; }
    public CellStateDto[][] BoardState { get; set; } = Array.Empty<CellStateDto[]>();
}