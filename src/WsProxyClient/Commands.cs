namespace WsProxyClient;

public static class Commands
{
    public const string KillAll = "kill_all";
    public const string AddPrefix = "add";

    public static string CreateAdd(int count) => $"{AddPrefix}_{count}";

    public static int ExtractCountToAdd(string addCommand) => int.Parse(addCommand.Split('_')[1]);
}