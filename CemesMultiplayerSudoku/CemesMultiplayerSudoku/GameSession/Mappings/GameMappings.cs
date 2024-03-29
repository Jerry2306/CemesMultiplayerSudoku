using CemesMultiplayerSudoku.Contract.GameSession.Dtos;
using CemesMultiplayerSudoku.GameSession.Services.Components;

namespace CemesMultiplayerSudoku.GameSession.Mappings;

public static class GameMappings
{
    public static GameDto MapToDto(this GameContainer container)
    {
        return new GameDto
        {
            Code = container.Code,
            Name = container.Name,
            HostPlayerId = container.HostPlayerId,
            Players = container.Players.Select(x => x.MapToDto()).ToArray(),
            StartedAt = container.StartedAt,
            BoardState = container.BoardState
        };
    }
}