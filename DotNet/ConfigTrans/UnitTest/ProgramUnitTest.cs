//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="ProgramUnitTest.cs" company="LiXinXu">
//     Copyright LiXinXu.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------------------------------------------------------------------

namespace ConfigurationTransformation.UnitTest
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Xml;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static TestUtility;
    using static Utility;

    /// <summary>
    /// Unit tests for program class
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ProgramUnitTest
    {
        /// <summary>
        /// test data folder
        /// </summary>
        private const string TestDataFolder = "ProgramTestData.";

        /// <summary>
        /// manifest file name
        /// </summary>
        private const string ManifestFileName = "Manifest.xml";

        /// <summary>
        /// master file name
        /// </summary>
        private const string MasterFileName = "ServiceConfiguration.Cloud.cscfg";

        /// <summary>
        /// invalid flags
        /// </summary>
        private static readonly IReadOnlyList<string> InvalidFlags = new string[]
        {
            null,
            string.Empty,
            "Invalid"
        };

        /// <summary>
        /// expected files
        /// </summary>
        private static readonly ISet<string> ExpectedFiles = new HashSet<string>()
        {
            "web.dev-eastus.config",
            "web.dev-westus.config",
            "web.prod-eastus.config",
            "web.prod-westus.config",
            "web.uat-eastus.config",
            "web.uat-westus.config",
        };

        #region GetParallelFlag
        /// <summary>
        /// Test bad flag without configuration defined
        /// </summary>
        [TestMethod]
        public void Porgram_GetParallelFlag_BadFlagNullConfig()
        {
            var configurations = new NameValueCollection();
            foreach (var flag in InvalidFlags)
            {
                var parallel = Program.GetParallelFlag(flag, configurations);
                Assert.AreEqual(Program.DefaultParallelValue, parallel);
            }
        }

        /// <summary>
        /// Test bad flag without valid configuration defined
        /// </summary>
        [TestMethod]
        public void Porgram_GetParallelFlag_BadFlagInvalidConfig()
        {
            var configurations = new NameValueCollection();
            configurations.Add(Program.ParallelConfigurationKey, "InvalidBoolean");
            foreach (var flag in InvalidFlags)
            {
                var parallel = Program.GetParallelFlag(flag, configurations);
                Assert.AreEqual(Program.DefaultParallelValue, parallel);
            }
        }

        /// <summary>
        /// Test bad flag with configuration defined
        /// </summary>
        [TestMethod]
        public void Porgram_GetParallelFlag_BagFlag()
        {
            foreach (var flag in InvalidFlags)
            {
                foreach (var configuration in GetAllBooleans())
                {
                    var configurations = new NameValueCollection();
                    configurations.Add(Program.ParallelConfigurationKey, configuration.ToString(CultureInfo.InvariantCulture));
                    var parallel = Program.GetParallelFlag(flag, configurations);
                    Assert.AreEqual(configuration, parallel);
                }
            }
        }

        /// <summary>
        /// Test valid flags
        /// </summary>
        [TestMethod]
        public void Porgram_GetParallelFlag_ValidFlags()
        {
            foreach (var flag in GetAllBooleans())
            {
                foreach (var configuration in GetAllBooleans())
                {
                    var configurations = new NameValueCollection();
                    configurations.Add(Program.ParallelConfigurationKey, configuration.ToString(CultureInfo.InvariantCulture));
                    var parallel = Program.GetParallelFlag(flag.ToString(CultureInfo.InvariantCulture), configurations);
                    Assert.AreEqual(flag, parallel);
                }
            }
        }
        #endregion GetParallelFlag

        #region main()
        /// <summary>
        /// Test main with less parameter
        /// </summary>
        [TestMethod]
        public void Porgram_Main_LessParameter()
        {
            var masterFolder = GetTempFolder();
            var masterFile = Path.Combine(masterFolder, MasterFileName);
            var parameters = new string[] { masterFile };

            var exitCode = Program.Main(parameters);
            Assert.AreEqual(1, exitCode);
        }

        /// <summary>
        /// Test main with less parameter
        /// </summary>
        [TestMethod]
        public void Porgram_Main_GiveOutputFolder()
        {
            foreach (var parallel in GetAllBooleans())
            {
                string masterFolder = GetTempFolder();
                string manifestFolder = GetTempFolder();
                string outputFolder = GetTempFolder();

                try
                {
                    var masterFilename = Path.Combine(masterFolder, MasterFileName);
                    var manifestFileName = Path.Combine(manifestFolder, ManifestFileName);

                    var parameters = new string[]
                    {
                        masterFilename,
                        manifestFileName,
                        outputFolder,
                        parallel.ToString(CultureInfo.InvariantCulture)
                    };

                    this.CopyFile(MasterFileName, masterFolder);
                    this.CopyFile(ManifestFileName, manifestFolder);

                    var exitCode = Program.Main(parameters);
                    Assert.AreEqual(0, exitCode);

                    var files = Directory.GetFiles(outputFolder);
                    Assert.AreEqual(ExpectedFiles.Count, files.Length);
                    foreach (var file in files)
                    {
                        var name = Path.GetFileName(file);
                        Assert.IsTrue(ExpectedFiles.Contains(name));
                        var expectedXml = this.LoadEmbeddedXml(name);
                        var actualDoc = new XmlDocument();
                        actualDoc.Load(file);
                        Assert.IsTrue(XmlAreSame(expectedXml, actualDoc.DocumentElement));
                    }
                }
                finally
                {
                    if (Directory.Exists(manifestFolder))
                    {
                        Directory.Delete(manifestFolder, true);
                    }

                    if (Directory.Exists(masterFolder))
                    {
                        Directory.Delete(masterFolder, true);
                    }

                    if (Directory.Exists(outputFolder))
                    {
                        Directory.Delete(outputFolder, true);
                    }
                }
            }
        }

        /// <summary>
        /// Test main with less parameter
        /// </summary>
        [TestMethod]
        public void Porgram_Main_NoOutputFolder()
        {
            string masterFolder = GetTempFolder();
            string manifestFolder = GetTempFolder();

            try
            {
                var masterFilename = Path.Combine(masterFolder, MasterFileName);
                var manifestFileName = Path.Combine(manifestFolder, ManifestFileName);

                var parameters = new string[]
                {
                    masterFilename,
                    manifestFileName
                };

                this.CopyFile(MasterFileName, masterFolder);
                this.CopyFile(ManifestFileName, manifestFolder);

                var exitCode = Program.Main(parameters);
                Assert.AreEqual(0, exitCode);

                var outputFolder = masterFolder;
                foreach (var name in ExpectedFiles)
                {
                    var file = Path.Combine(outputFolder, name);
                    Assert.IsTrue(File.Exists(file));

                    var expectedXml = this.LoadEmbeddedXml(name);
                    var actualDoc = new XmlDocument();
                    actualDoc.Load(file);
                    Assert.IsTrue(XmlAreSame(expectedXml, actualDoc.DocumentElement));
                }
            }
            finally
            {
                if (Directory.Exists(manifestFolder))
                {
                    Directory.Delete(manifestFolder, true);
                }

                if (Directory.Exists(masterFolder))
                {
                    Directory.Delete(masterFolder, true);
                }
            }
        }
        #endregion main()

        #region helper
        /// <summary>
        /// Load embedded XML
        /// </summary>
        /// <param name="name">base file name</param>
        /// <returns>XML from assembly</returns>
        private XmlElement LoadEmbeddedXml(string name)
        {
            return LoadXml(name, TestDataFolder, this.GetType());
        }

        /// <summary>
        /// Copy file from embedded to output folder
        /// </summary>
        /// <param name="name">file name</param>
        /// <param name="outputFolder">output folder</param>
        private void CopyFile(string name, string outputFolder)
        {
            var type = this.GetType();
            string content;
            using (var stream = type.Assembly.GetManifestResourceStream(type, TestDataFolder + name))
            {
                using (var reader = new StreamReader(stream))
                {
                    content = reader.ReadToEnd();
                }
            }

            var path = Path.Combine(outputFolder, name);
            var folder = Path.GetDirectoryName(path);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(path, content);
        }
        #endregion helper
    }
}
