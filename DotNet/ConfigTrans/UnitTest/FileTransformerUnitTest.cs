//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="FileTransformerUnitTest.cs" company="LiXinXu">
//     Copyright LiXinXu.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------------------------------------------------------------------

namespace ConfigurationTransformation.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static TestUtility;
    using static Utility;

    /// <summary>
    /// File transformer unit tests
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class FileTransformerUnitTest
    {
        /// <summary>
        /// test data folder
        /// </summary>
        private const string TestDataFolder = "FileTransformerTestData.";

        /// <summary>
        /// manifest file name
        /// </summary>
        private const string ManifestFileName = "DiffConfig.xml";

        /// <summary>
        /// master file name
        /// </summary>
        private const string MasterFileName = "MasterConfig.xml";

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

        /// <summary>
        /// File transformer general test
        /// </summary>
        [TestMethod]
        public void FileTransformer_Transform_Geneal()
        {
            foreach (var parallel in GetAllBooleans())
            {
                string manifestFolder = GetTempFolder();
                string masterFolder = GetTempFolder();
                string outputFolder = GetTempFolder();

                try
                {
                    this.CopyFile(ManifestFileName, manifestFolder);
                    this.CopyFile(MasterFileName, masterFolder);

                    using (var inforWriter = new StringWriter())
                    {
                        FileTransformer.Transform(
                        Path.Combine(masterFolder, MasterFileName),
                        Path.Combine(manifestFolder, ManifestFileName),
                        outputFolder,
                        inforWriter,
                        parallel);
                    }

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
        /// File transformer skip to write existing XML file
        /// </summary>
        [TestMethod]
        public void FileTransformer_Transform_Skip()
        {
            foreach (var parallel in GetAllBooleans())
            {
                string manifestFolder = GetTempFolder();
                string masterFolder = GetTempFolder();
                string outputFolder = GetTempFolder();

                try
                {
                    this.CopyFile(ManifestFileName, manifestFolder);
                    this.CopyFile(MasterFileName, masterFolder);

                    if (!Directory.Exists(outputFolder))
                    {
                        Directory.CreateDirectory(outputFolder);
                    }

                    foreach (var name in ExpectedFiles)
                    {
                        var xml = this.LoadEmbeddedXml(name);
                        var comment = xml.OwnerDocument.CreateComment(name);
                        xml.AppendChild(comment);
                        using (var writer = XmlTextWriter.Create(Path.Combine(outputFolder, name)))
                        {
                            xml.WriteTo(writer);
                        }
                    }

                    using (var inforWriter = new StringWriter())
                    {
                        FileTransformer.Transform(
                        Path.Combine(masterFolder, MasterFileName),
                        Path.Combine(manifestFolder, ManifestFileName),
                        outputFolder,
                        inforWriter,
                        parallel);
                    }

                    var files = Directory.GetFiles(outputFolder);
                    Assert.AreEqual(ExpectedFiles.Count, files.Length);
                    foreach (var file in files)
                    {
                        var name = Path.GetFileName(file);
                        Assert.IsTrue(ExpectedFiles.Contains(name));
                        var doc = new XmlDocument();
                        doc.Load(file);
                        var root = doc.DocumentElement;
                        string comment = null;
                        foreach (XmlNode node in root.ChildNodes)
                        {
                            if (node.NodeType == XmlNodeType.Comment)
                            {
                                comment = (node as XmlComment).Data;
                            }
                        }

                        Assert.IsNotNull(comment);
                        Assert.AreEqual(name, comment);
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
        /// File transformer overwrite existing XML file
        /// </summary>
        [TestMethod]
        public void FileTransformer_Transform_Overwrite()
        {
            foreach (var parallel in GetAllBooleans())
            {
                string manifestFolder = GetTempFolder();
                string masterFolder = GetTempFolder();
                string outputFolder = GetTempFolder();
                var attributeName = "additional";

                try
                {
                    this.CopyFile(ManifestFileName, manifestFolder);
                    this.CopyFile(MasterFileName, masterFolder);

                    if (!Directory.Exists(outputFolder))
                    {
                        Directory.CreateDirectory(outputFolder);
                    }

                    foreach (var name in ExpectedFiles)
                    {
                        var xml = this.LoadEmbeddedXml(name);
                        var attribute = xml.OwnerDocument.CreateAttribute(attributeName);
                        attribute.Value = name;
                        xml.Attributes.Append(attribute);
                        using (var writer = XmlTextWriter.Create(Path.Combine(outputFolder, name)))
                        {
                            xml.WriteTo(writer);
                        }
                    }

                    using (var inforWriter = new StringWriter())
                    {
                        FileTransformer.Transform(
                        Path.Combine(masterFolder, MasterFileName),
                        Path.Combine(manifestFolder, ManifestFileName),
                        outputFolder,
                        inforWriter,
                        parallel);
                    }

                    var files = Directory.GetFiles(outputFolder);
                    Assert.AreEqual(ExpectedFiles.Count, files.Length);
                    foreach (var file in files)
                    {
                        var name = Path.GetFileName(file);
                        Assert.IsTrue(ExpectedFiles.Contains(name));
                        var doc = new XmlDocument();
                        doc.Load(file);
                        var root = doc.DocumentElement;
                        var attribute = GetAttribute(root, attributeName);
                        Assert.IsTrue(string.IsNullOrEmpty(attribute));
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
        /// File transformer overwrite invalid existing XML file
        /// </summary>
        [TestMethod]
        public void FileTransformer_InvalidTargetFile()
        {
            foreach (var parallel in GetAllBooleans())
            {
                string manifestFolder = GetTempFolder();
                string masterFolder = GetTempFolder();
                string outputFolder = GetTempFolder();

                try
                {
                    this.CopyFile(ManifestFileName, manifestFolder);
                    this.CopyFile(MasterFileName, masterFolder);

                    foreach (var name in ExpectedFiles)
                    {
                        this.CopyFile(name, outputFolder);
                    }

                    var invalidFile = ExpectedFiles.ToArray()[ExpectedFiles.Count / 2];
                    File.WriteAllText(Path.Combine(outputFolder, invalidFile), "<invalidXml>");

                    using (var inforWriter = new StringWriter())
                    {
                        FileTransformer.Transform(
                        Path.Combine(masterFolder, MasterFileName),
                        Path.Combine(manifestFolder, ManifestFileName),
                        outputFolder,
                        inforWriter,
                        parallel);
                    }

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
        /// File transformer pass null as master XML file path
        /// </summary>
        [TestMethod]
        public void FileTransformer_Transform_NullMasterXmlFilePath()
        {
            var parameterName = GetParameterInformation(
                typeof(FileTransformer),
                nameof(FileTransformer.Transform),
                BindingFlags.Public | BindingFlags.Static,
                0).Name;
            this.TestTransformNullParameter(null, GetTempFolder(), GetTempFolder(), parameterName);
        }

        /// <summary>
        /// File transformer pass empty string as master XML file path
        /// </summary>
        [TestMethod]
        public void FileTransformer_Transform_EmptyMasterXmlFilePath()
        {
            var parameterName = GetParameterInformation(
                typeof(FileTransformer),
                nameof(FileTransformer.Transform),
                BindingFlags.Public | BindingFlags.Static,
                0).Name;
            this.TestTransformNullParameter(string.Empty, GetTempFolder(), GetTempFolder(), parameterName);
        }

        /// <summary>
        /// File transformer pass null as master XML file path
        /// </summary>
        [TestMethod]
        public void FileTransformer_Transform_NullManifestXmlFilePath()
        {
            var parameterName = GetParameterInformation(
                typeof(FileTransformer),
                nameof(FileTransformer.Transform),
                BindingFlags.Public | BindingFlags.Static,
                1).Name;
            this.TestTransformNullParameter(GetTempFolder(), null, GetTempFolder(), parameterName);
        }

        /// <summary>
        /// File transformer pass empty string as master XML file path
        /// </summary>
        [TestMethod]
        public void FileTransformer_Transform_EmptyManifestXmlFilePath()
        {
            var parameterName = GetParameterInformation(
                typeof(FileTransformer),
                nameof(FileTransformer.Transform),
                BindingFlags.Public | BindingFlags.Static,
                1).Name;
            this.TestTransformNullParameter(GetTempFolder(), string.Empty, GetTempFolder(), parameterName);
        }

        /// <summary>
        /// File transformer pass null as output folder
        /// </summary>
        [TestMethod]
        public void FileTransformer_Transform_NullOutputFolder()
        {
            var parameterName = GetParameterInformation(
                typeof(FileTransformer),
                nameof(FileTransformer.Transform),
                BindingFlags.Public | BindingFlags.Static,
                2).Name;
            this.TestTransformNullParameter(GetTempFolder(), GetTempFolder(), null, parameterName);
        }

        /// <summary>
        /// File transformer pass empty string as output folder
        /// </summary>
        [TestMethod]
        public void FileTransformer_Transform_EmptyOutputFolder()
        {
            var parameterName = GetParameterInformation(
                typeof(FileTransformer),
                nameof(FileTransformer.Transform),
                BindingFlags.Public | BindingFlags.Static,
                2).Name;
            this.TestTransformNullParameter(GetTempFolder(), GetTempFolder(), string.Empty, parameterName);
        }

        /// <summary>
        /// File transformer when master file does not exist
        /// </summary>
        [TestMethod]
        public void FileTransformer_Transform_MasterFileNotExist()
        {
            this.TestRequiredFileNotExist(false, true);
        }

        /// <summary>
        /// File transformer when master file does not exist
        /// </summary>
        [TestMethod]
        public void FileTransformer_Transform_ManifestFileNotExist()
        {
            this.TestRequiredFileNotExist(true, false);
        }

        /// <summary>
        /// Get method parameter name
        /// </summary>
        /// <param name="methodName">method name, if null, return constructor</param>
        /// <param name="parameterIndex">parameter index</param>
        /// <returns>parameter name</returns>
        private static string GetMethodParameterName(string methodName, int parameterIndex)
        {
            return GetParameterInformation(
                typeof(FileTransformer),
                methodName,
                BindingFlags.Public | BindingFlags.Instance,
                parameterIndex).Name;
        }

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

        /// <summary>
        /// Test transform pass null or empty parameters
        /// </summary>
        /// <param name="masterFolder">master folder</param>
        /// <param name="manifestFolder">manifest folder</param>
        /// <param name="outputFolder">output folder</param>
        /// <param name="parameterName">parameter name to check</param>
        private void TestTransformNullParameter(string masterFolder, string manifestFolder, string outputFolder, string parameterName)
        {
            foreach (var parallel in GetAllBooleans())
            {
                string error = null;
                try
                {
                    string manifestFileName = null;
                    if (!string.IsNullOrEmpty(manifestFolder))
                    {
                        this.CopyFile(ManifestFileName, manifestFolder);
                        manifestFileName = Path.Combine(manifestFolder, ManifestFileName);
                    }

                    string masterFilename = null;
                    if (!string.IsNullOrEmpty(masterFolder))
                    {
                        this.CopyFile(MasterFileName, masterFolder);
                        masterFilename = Path.Combine(masterFolder, MasterFileName);
                    }

                    using (var inforWriter = new StringWriter())
                    {
                        FileTransformer.Transform(masterFilename, manifestFileName, outputFolder, inforWriter, parallel);
                    }
                }
                catch (ArgumentException exception)
                {
                    error = exception.Message;
                }
                finally
                {
                    if (!string.IsNullOrEmpty(manifestFolder) && Directory.Exists(manifestFolder))
                    {
                        Directory.Delete(manifestFolder, true);
                    }

                    if (!string.IsNullOrEmpty(masterFolder) && Directory.Exists(masterFolder))
                    {
                        Directory.Delete(masterFolder, true);
                    }

                    if (!string.IsNullOrEmpty(outputFolder) && Directory.Exists(outputFolder))
                    {
                        Directory.Delete(outputFolder, true);
                    }
                }

                Assert.IsNotNull(error);
                Assert.IsTrue(error.Contains(parameterName));
            }
        }

        /// <summary>
        /// Test when required file was not provided
        /// </summary>
        /// <param name="copyMasterFile">copy master to folder</param>
        /// <param name="copyManifestFile">copy manifest file to folder</param>
        private void TestRequiredFileNotExist(bool copyMasterFile, bool copyManifestFile)
        {
            foreach (var parallel in GetAllBooleans())
            {
                string manifestFolder = GetTempFolder();
                string masterFolder = GetTempFolder();
                string outputFolder = GetTempFolder();

                var manifestFileName = Path.Combine(manifestFolder, ManifestFileName);
                var masterFilename = Path.Combine(masterFolder, MasterFileName);

                string filename = null;
                try
                {
                    if (copyMasterFile)
                    {
                        this.CopyFile(MasterFileName, masterFolder);
                    }

                    if (copyManifestFile)
                    {
                        this.CopyFile(ManifestFileName, manifestFolder);
                    }

                    using (var inforWriter = new StringWriter())
                    {
                        FileTransformer.Transform(masterFilename, manifestFileName, outputFolder, inforWriter, parallel);
                    }
                }
                catch (FileNotFoundException exception)
                {
                    filename = exception.FileName;
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

                Assert.IsNotNull(filename);
                if (!copyMasterFile)
                {
                    Assert.AreEqual(masterFilename, filename);
                }
                else if (!copyManifestFile)
                {
                    Assert.AreEqual(manifestFileName, filename);
                }
            }
        }
    }
}
