using System.Text.RegularExpressions;

namespace GitCandy.Base
{
    public static class RegularExpressions
    {
        public const string Email = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";
        public const string UserName = @"(?i)^[a-z][a-z0-9\-_]+$";
        public const string TeamName = @"(?i)^[a-z][a-z0-9\-_]+$";
        public const string RepoName = @"(?i)^[a-z][a-z0-9\-\._]+(?<!\.git)$";

        public static readonly Regex ReplaceNewline = new Regex(@"\r\n|\r|\n", RegexOptions.Compiled);
    }
}
