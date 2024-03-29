using CemesMultiplayerSudoku.Contract.GameSession.Dtos;
using CemesMultiplayerSudoku.GameSession.Services.Components;

namespace CemesMultiplayerSudoku.GameSession.Mappings;

public static class PlayerMappings
{
    public static PlayerDto MapToDto(this Player player)
    {
        return new PlayerDto
        {
            ConnectionId = player.ConnectionId,
            Id = player.Id,
            Name = player.Name,
            Color = player.Color
        };
    }
}