// /*---------------------------------------------------------------------------------------*/
//        Licensed to the Apache Software Foundation (ASF) under one
//        or more contributor license agreements.  See the NOTICE file
//        distributed with this work for additional information
//        regarding copyright ownership.  The ASF licenses this file
//        to you under the Apache License, Version 2.0 (the
//        "License"); you may not use this file except in compliance
//        with the License.  You may obtain a copy of the License at
// 
//          http://www.apache.org/licenses/LICENSE-2.0
// 
//        Unless required by applicable law or agreed to in writing,
//        software distributed under the License is distributed on an
//        "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//        KIND, either express or implied.  See the License for the
//        specific language governing permissions and limitations
//        under the License.
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