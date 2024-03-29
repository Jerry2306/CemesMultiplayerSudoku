using System.Drawing;
using CemesMultiplayerSudoku.Contract.GameSession.Dtos;

namespace CemesMultiplayerSudoku.Client.Core.Models;

public class CellStateModel
{
    public CellStateDto Dto { get; set; }
    public Color? ActiveColor { get; set; }
    public Dictionary<byte, Color[]>? SelectedNumbers { get; set; }
    public List<PlayerDto> SelectedByPlayers { get; set; } = new();

    public CellStateModel(CellStateDto dto)
    {
        Dto = dto;
    }
}