//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="LiXinXu">
//     Copyright LiXinXu.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------------------------------------------------------------------

namespace ConfigurationTransformation
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// application entry
    /// </summary>
    public class Program
    {
        /// <summary>
        /// configuration key
        /// </summary>
        public const string ParallelConfigurationKey = "parallel";

        /// <summary>
        /// default parallel value
        /// </summary>
        public const bool DefaultParallelValue = false;

        /// <summary>
        /// Main entry
        /// </summary>
        /// <param name="args">application arguments</param>
        /// <returns>exit code</returns>
        public static int Main(string[] args)
        {
            return Transform(args, Console.Out);
        }

        /// <summary>
        /// Main entry
        /// </summary>
        /// <param name="args">application arguments</param>
        /// <param name="writer">information writer</param>
        /// <returns>exit code</returns>
        public static int Transform(string[] args, TextWriter writer)
        { 
            if (args.Length < 2)
            {
                var filename = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
                var usage = $"{filename} MasterFile Manifest [output folder] [true|false]";
                writer.WriteLine(usage);
                return 1;
            }

            var masterFile = args[0];
            var manifestFile = args[1];
            string outputFolder;
            if (args.Length > 2)
            {
                outputFolder = args[2];
            }
            else
            {
                outputFolder = Path.GetDirectoryName(masterFile);
            }

            var parallel = GetParallelFlag(args.Length > 3 ? args[3] : null, ConfigurationManager.AppSettings);

            FileTransformer.Transform(masterFile, manifestFile, outputFolder, writer, parallel);

            return 0;
        }

        /// <summary>
        /// get parallel flag
        /// </summary>
        /// <param name="flag">flag from command line</param>
        /// <param name="configurations">application configurations</param>
        /// <returns>allow parallel processing</returns>
        public static bool GetParallelFlag(string flag, NameValueCollection configurations)
        {
            bool data;
            if (!string.IsNullOrEmpty(flag) && bool.TryParse(flag, out data))
            {
                return data;
            }

            var config = configurations[ParallelConfigurationKey];
            if (!string.IsNullOrEmpty(config) && bool.TryParse(config, out data))
            {
                return data;
            }

            return false;
        }
    }
}
