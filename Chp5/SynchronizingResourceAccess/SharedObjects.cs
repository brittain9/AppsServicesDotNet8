namespace SynchronizingResourceAccess;

public static class SharedObjects
{
    public static string? Message;
    public static object Conch = new(); // shared object to lock
    public static int Counter;
}
