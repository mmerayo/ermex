// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ermeX.Versioning.Schema.Scripts
{
    internal static class ScriptUtils
    {
        private static readonly Regex _splitRegex = new Regex("\\bgo\\s*[\\r\\n|\\n|$]", RegexOptions.IgnoreCase);

        public static string[] SplitScriptsByGoKeyword(string sqlScript)
        {
            return FilterOutEmpty(_splitRegex.Split(sqlScript).Select(s => s.Trim())).ToArray();
        }

        //Using this instead of where because where doesn't guarantee the order apparently 

        public static IEnumerable<string> FilterOutEmpty(this IEnumerable<string> input)
        {
            foreach (var value in input)
            {
                if (!String.IsNullOrEmpty(value))
                {
                    yield return value;
                }
            }
        }

        public static string GetSqlFromResource(string resourceName)
        {
            string result;
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new ApplicationException(string.Format("The embedded script {0} is missing.", resourceName));
                using (var reader = new StreamReader(stream))
                    result = reader.ReadToEnd();
            }
            return result;
        }
    }
}