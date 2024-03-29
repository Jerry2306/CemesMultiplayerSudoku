namespace CemesMultiplayerSudoku.Client.Core.Services.Handlers;

public class StateChangedHandler : IDisposable
{
    public Func<Task> Func { get; }

    private readonly GameSessionManager _parent;

    public StateChangedHandler(Func<Task> func, GameSessionManager parent)
    {
        _parent = parent;
        Func = func;
    }

    public void Dispose() => _parent.RemoveHandler(this);
}