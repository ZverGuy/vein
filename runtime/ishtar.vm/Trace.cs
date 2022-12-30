namespace ishtar;

internal static class Trace
{
    private static bool useConsole;
    private static bool useFile;

    public static void init()
    {
        useConsole = !Environment.GetCommandLineArgs().Contains("--sys::log::use-console=0");
        useFile = Environment.GetCommandLineArgs().Contains("--sys::log::use-file=1"); // TODO
    }
    public static void println(string s)
    {
        if (useConsole)
        {
            var t = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(s);
            Console.ForegroundColor = t;
        }
    }
}
