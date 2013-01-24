using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;

namespace ermeX.Tests.AcceptanceTester.Base.TestExecution
{
    public abstract class TesterBase
    {
        protected static Guid _currentComponentId;

        public virtual void Dispose()
        {
            try
            {
                WorldGate.Reset();//hides errors when the db is deleted
            }catch
            {
                
            }
        }

        protected void UpdateLogConnectionStringSettings(string connStr)
        {
            string readAllText = File.ReadAllText(LogConfigurationPath);
            var content = XDocument.Parse(readAllText);

            content.Descendants("log4net").Descendants("appender").
                Descendants("connectionString").Attributes().First().Value = connStr;
            content.Save(LogConfigurationPath);

            Thread.Sleep(TimeSpan.FromSeconds(1));//ensure changes are taken
        }
        string _logConfigurationPath;
        private readonly object _syncLock=new object();

        private string LogConfigurationPath
        {
            get
            {
                if (string.IsNullOrEmpty(_logConfigurationPath))
                    lock (_syncLock)
                        if (string.IsNullOrEmpty(_logConfigurationPath))
                        {
                            Assembly executingAssembly = Assembly.GetExecutingAssembly();
                            string path = new Uri(executingAssembly.CodeBase).LocalPath;
                            string localPath = Path.GetDirectoryName(path);
                            _logConfigurationPath = Path.Combine(localPath, "ermeX.Tests.AcceptanceTester.exe.config");
                        }

                return _logConfigurationPath;
            }
        }
    }
}