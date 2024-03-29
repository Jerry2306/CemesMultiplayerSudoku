using CemesMultiplayerSudoku.Contract.GameSession.Dtos;

namespace CemesMultiplayerSudoku.Client.Core.Models;

public class GameStateModel
{
    public string Code { get; set; }
    public string Name { get; set; }
    public long HostPlayerId { get; set; }
    public DateTime StartedAt { get; set; }
    public List<PlayerDto> Players { get; set; }
    public CellStateModel[][] BoardState { get; set; } = null!;

    public GameStateModel(GameDto dto)
    {
        Code = dto.Code;
        Name = dto.Name;
        HostPlayerId = dto.HostPlayerId;
        StartedAt = dto.StartedAt;
        Players = new List<PlayerDto>(dto.Players);

        SetBoardState(dto.BoardState);
    }

    public void SetBoardState(CellStateDto[][] boardState)
    {
        BoardState = new CellStateModel[boardState.Length][];
        for (var i = 0; i < BoardState.Length; i++)
        {
            BoardState[i] = new CellStateModel[boardState[i].Length];

            for (var j = 0; j < BoardState[i].Length; j++)
                BoardState[i][j] = new CellStateModel(boardState[i][j]);
        }
    }

    public void AddPlayer(PlayerDto player)
    {
        var playerHit = Players.FirstOrDefault(x => x.Id == player.Id);
        if (playerHit is not null)
            return;

        Players.Add(player);
    }

    public void RemovePlayer(PlayerDto player)
    {
        var playerHit = Players.FirstOrDefault(x => x.Id == player.Id);
        if (playerHit is null)
            return;

        Players.Remove(playerHit);
    }

    public bool DeselectPlayerFields(PlayerDto player)
    {
        var gotHit = false;
        for (var i = 0; i < BoardState.Length; i++)
        for (var j = 0; j < BoardState[i].Length; j++)
        {
            var players = BoardState[i][j].SelectedByPlayers;
            var playerHit = players.FirstOrDefault(x => x.Id == player.Id);
            if (playerHit is null)
                continue;

            gotHit = true;
            players.Remove(playerHit);
        }

        return gotHit;
    }
}