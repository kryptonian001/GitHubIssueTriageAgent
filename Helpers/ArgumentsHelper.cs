namespace GitHubIssueTriageAgent.Helpers;

internal static class ArgumentsHelper
{
    public static bool TryGetArgument(this string[] args, string arg, out string argValue)
    {
        argValue = string.Empty;
        
        for(int i = 0; i < args.Length; i++)
        {
            if (args[i] == arg)
            {
                argValue = args[i + 1];

                if (string.IsNullOrEmpty(argValue))
                    return false;

                return true;
            }
        }

        return false;
    }
}
