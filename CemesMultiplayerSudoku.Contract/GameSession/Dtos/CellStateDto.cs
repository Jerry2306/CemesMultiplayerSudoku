namespace CemesMultiplayerSudoku.Contract.GameSession.Dtos;

public class CellStateDto
{
    public byte Value { get; set; }
    public bool WasPrefilled { get; set; }
    public bool IsHint { get; set; }
    public bool IsCorrect { get; set; } = true;
    public List<byte> Notes { get; set; } = new();

    public CellStateDto()
    {
    }

    public CellStateDto(byte value, bool wasPrefilled)
    {
        Value = value;
        WasPrefilled = wasPrefilled;
    }
}