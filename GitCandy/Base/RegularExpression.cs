using System.Text.RegularExpressions;

namespace GitCandy.Base
{
    public static class RegularExpression
    {
        public const string Email = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";
        public const string Username = @"(?i)^[a-z][a-z0-9\-_]+$";
        public const string Teamname = @"(?i)^[a-z][a-z0-9\-_]+$";
        public const string Repositoryname = @"(?i)^[a-z][a-z0-9\-\._]+(?<!\.git)$";

        public static readonly Regex ReplaceNewline = new Regex(@"\r\n|\r|\n", RegexOptions.Compiled);
    }
}
