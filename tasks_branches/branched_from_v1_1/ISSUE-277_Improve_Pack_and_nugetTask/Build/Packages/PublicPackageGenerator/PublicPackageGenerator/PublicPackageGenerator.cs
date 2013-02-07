using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace PublicPackageGenerator
{
    /// <summary>
    /// This task generates a public publisheable package when all the builds in the matrix have been done for a given version
    /// </summary>
    public class PublicPackageGenerator:Task
    {
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
        /// the current version that is being built
        /// </summary>
        [Required]
        public virtual string CheckVersion { get; set; }

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
                //TODO: NAMED MUTEX FOR THIS
                EnsureDbExists();

                if(SaveCurrent())
                {
                    GeneratePackage();
                }
            }
            catch (Exception e)
            {
                Log.LogErrorFromException(e);
                return false;
            }

            return true;
        }

        private void GeneratePackage()
        {
            Log.LogMessage(MessageImportance.Normal, "PublicPackageGenerator: Generating public package", TODO);
        }


        /// <summary>
        /// Returns true if is the last an the package must be generated
        /// </summary>
        /// <returns></returns>
        private bool SaveCurrent()
        {
            Log.LogMessage(MessageImportance.Normal, "PublicPackageGenerator: Saving current build data, version: {0} Platform: {1} Framework:{2}", TODO);
        }

        //Creates the db if it doesnt exists
        private void EnsureDbExists()
        {
            Log.LogMessage(MessageImportance.Normal, "PublicPackageGenerator: ensure db exists at {0}", this.DbPath);
            throw new NotImplementedException();
        }
    }
}
