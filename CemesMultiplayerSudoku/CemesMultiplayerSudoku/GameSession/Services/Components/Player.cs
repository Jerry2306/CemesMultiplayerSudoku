namespace CemesMultiplayerSudoku.GameSession.Services.Components;

public class Player
{
    public string ConnectionId { get; set; } = string.Empty;
    public string SessionToken { get; set; } = string.Empty;
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Color { get; set; }

    public bool IsInitialized() => !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Color);
}