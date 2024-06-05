partial class Program
{
    private static void WriteInColor(string text, ConsoleColor color){
        ConsoleColor prev = ForegroundColor;
        ForegroundColor = color;
        Write(text);
        ForegroundColor = prev;
    }
}
