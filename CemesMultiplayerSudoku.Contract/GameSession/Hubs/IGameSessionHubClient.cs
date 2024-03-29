using CemesMultiplayerSudoku.Contract.GameSession.Dtos;

namespace CemesMultiplayerSudoku.Contract.GameSession.Hubs;

public interface IGameSessionHubClient
{
    Task SessionCreated(string sessionToken, long playerId);
    Task JoinedGame(GameDto game);
    Task LeftGame();
    Task UpdateBoard(CellStateDto[][] boardState);
    Task PlayerJoined(PlayerDto player);
    Task PlayerLeft(PlayerDto player);
    Task PlayerSelectedField(PlayerDto player, byte x, byte y);
    Task PlayerDeselectedField(PlayerDto player);
    Task PlayerSetNumber(PlayerDto player, byte value, byte x, byte y, bool correct);
    Task PlayerErasedNumber(PlayerDto player, byte x, byte y);
    Task PlayerSetNote(PlayerDto player, byte value, byte x, byte y);
    Task PlayerErasedNote(PlayerDto player, byte value, byte x, byte y);
    Task PlayerHint(PlayerDto player, byte value, byte x, byte y);
    Task PlayerUndo(PlayerDto player, byte x, byte y);
}