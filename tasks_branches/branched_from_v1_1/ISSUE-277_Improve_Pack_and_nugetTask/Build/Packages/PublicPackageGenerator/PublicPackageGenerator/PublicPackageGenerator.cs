using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace PublicPackageGenerator
{
    /// <summary>
    /// This task generates a public publisheable package when all the builds in the matrix have been done for a given version
    /// </summary>
    public class PublicPackageGenerator:Task
    {
        private readonly string[] CreateTableDDL =new string[]{
            "CREATE TABLE [Builds] (Version text,Platform text,Framework text,OutputPath text)",
            "CREATE UNIQUE INDEX IDX_BUILD ON Builds(Version,Platform,Framework)"};

        private const string DbName = "PublicPackageGenerator.db";

        /// <summary>
        /// X86,x64,..
        /// </summary>
        [Required]
        public virtual ITaskItem[] CheckPlatforms { get; set; }

        /// <summary>
        /// 4.0,..
        /// </summary>
        [Required]
        public virtual ITaskItem[] CheckFrameworks { get; set; }

       
        /// <summary>
        /// The path where the db is stored
        /// </summary>
        [Required]
        public virtual string DbPath { get; set; }

        /// <summary>
        /// The path where the result is stored
        /// </summary>
        [Required]
        public virtual string OutputPath { get; set; }

        /// <summary>
        /// the current platform to save
        /// </summary>
        [Required]
        public virtual string CurrentPlatform { get; set; }

        /// <summary>
        /// the current fr to save
        /// </summary>
        [Required]
        public virtual string CurrentFramework { get; set; }

        /// <summary>
        /// the current version that is being built
        /// </summary>
        [Required]
        public virtual string CurrentVersion { get; set; }
        
        public override bool Execute()
        {
            try
            {
                ValidateParameters();
                using (var mutex = new Mutex(false, "PublicPackageGenerator"))
                {
                    mutex.WaitOne(TimeSpan.FromSeconds(10));

                    EnsureDbExists();

                    IEnumerable<string> outputPaths = SaveCurrent();
                    if (outputPaths!=null)
                    {
                        GeneratePackage(outputPaths);
                    }
                }
            }
            catch (Exception e)
            {
                Log.LogErrorFromException(e);
                return false;
            }

            return true;
        }

        private void ValidateParameters()
        {
            if(string.IsNullOrEmpty(DbPath) || string.IsNullOrWhiteSpace(DbPath) ) throw new ArgumentException("Please provide DbPath");
            if (string.IsNullOrEmpty(CurrentVersion) || string.IsNullOrWhiteSpace(CurrentVersion)) throw new ArgumentException("Please provide CurrentVersion");
            if (string.IsNullOrEmpty(CurrentPlatform) || string.IsNullOrWhiteSpace(CurrentPlatform)) throw new ArgumentException("Please provide CurrentPlatform");
            if (string.IsNullOrEmpty(CurrentFramework) || string.IsNullOrWhiteSpace(CurrentFramework)) throw new ArgumentException("Please provide CurrentFramework");
            if (string.IsNullOrEmpty(OutputPath)) throw new ArgumentException("Please provide OutputPath");
            if (CheckPlatforms == null || CheckPlatforms.Length == 0) throw new ArgumentException("Please provide CheckPlatforms");
            if (CheckFrameworks == null || CheckFrameworks.Length == 0) throw new ArgumentException("Please provide CheckFrameworks");

        }

        private void GeneratePackage(IEnumerable<string> outputPaths)
        {
            Log.LogMessage(MessageImportance.Normal, "PublicPackageGenerator: Generating public package");
            throw new NotImplementedException();
        }


        /// <summary>
        /// Returns all the output paths if is the last and the package must be generated
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> SaveCurrent()
        {
            Log.LogMessage(MessageImportance.Normal, "PublicPackageGenerator: Saving current build data, version: {0} Platform: {1} Framework:{2}", CurrentVersion,CurrentPlatform,CurrentFramework);
            var result = new List<string>();
            string dbFullPath = Path.Combine(DbPath, DbName);
            var connStr = string.Format("Data Source={0};Version=3;BinaryGuid=False;", dbFullPath);
            using (var conn = new SQLiteConnection(connStr))
            {
                //save current
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    var txt = string.Format("INSERT INTO Builds VALUES ('{0}', '{1}','{2}','{3}')",
                                            CurrentVersion, CurrentPlatform, CurrentFramework, OutputPath);
                    cmd.CommandText = txt;
                    cmd.ExecuteNonQuery();
                }
                //check all are there
                using (var cmd = conn.CreateCommand())
                {
                    
                    foreach (var checkPlatform in CheckPlatforms)
                        foreach (var checkFramework in CheckFrameworks)
                        {
                            cmd.CommandText =
                                string.Format(
                                    "select OutputPath from Builds where Version= '{0}' and Platform='{1}' and Framework='{2}'",CurrentVersion,checkPlatform.ItemSpec,checkFramework.ItemSpec);
                            object executeScalar = cmd.ExecuteScalar();
                            if (executeScalar == null) return null;
                            result.Add(executeScalar.ToString());
                        }
                }

            }

            return result;
        }

        //Creates the db if it doesnt exists
        private void EnsureDbExists()
        {
            Log.LogMessage(MessageImportance.Normal, "PublicPackageGenerator: ensure db exists at {0}", this.DbPath);

            string dbFullPath = Path.Combine(DbPath, DbName);
            if (!Directory.Exists(DbPath)) Directory.CreateDirectory(DbPath);
            if(!File.Exists(dbFullPath))
            {
               var connStr = string.Format("Data Source={0};Version=3;BinaryGuid=False;", dbFullPath);
               using( var conn = new SQLiteConnection(connStr))
               {
                   conn.Open();
                   using(var cmd = conn.CreateCommand())
                   {
                       cmd.CommandText = CreateTableDDL[0];
                       cmd.ExecuteNonQuery();
                       cmd.CommandText = CreateTableDDL[1];
                       cmd.ExecuteNonQuery();
                   }
               }
            }
        }
    }
}
