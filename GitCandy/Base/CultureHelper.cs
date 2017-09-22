using System;
using System.Collections.Generic;
using System.Globalization;

namespace GitCandy.Base
{
    public static class CultureHelper
    {
        private static Dictionary<string, CultureInfo> ciCache = new Dictionary<string, CultureInfo>(StringComparer.OrdinalIgnoreCase);

        public static string CultureToDisplayCache(string culture = null)
        {
            if (culture == null)
                culture = CultureInfo.CurrentUICulture.Name;

            var ci = NameToCultureInfoCache(culture);
            var displayName = ci.Name.StartsWith("en")
                ? ci.NativeName
                : ci.EnglishName + " - " + ci.NativeName;

            return displayName;
        }

        public static CultureInfo NameToCultureInfoCache(string culture)
        {
            if (ciCache.ContainsKey(culture))
                return ciCache[culture];

            var ci = CultureInfo.CreateSpecificCulture(culture);
            if (ci.Equals(CultureInfo.InvariantCulture))
                throw new CultureNotFoundException();

            ciCache.Add(culture, ci);

            return ci;
        }
    }
}
