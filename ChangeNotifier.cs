public class ChangeNotifier
{
    public event Action? Changed;
    public void Publish()
    {
        if (Changed is null) return;
        foreach (var handler in Changed.GetInvocationList().Cast<Action>())
        {
            try { handler(); } catch { /* a disposed circuit; ignore */ }
        }
    }
}
