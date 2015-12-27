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
        public void FileTransformer_Geneal()
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

                    FileTransformer.Transform(
                        Path.Combine(masterFolder, MasterFileName),
                        Path.Combine(manifestFolder, ManifestFileName),
                        outputFolder,
                        parallel);

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
        public void FileTransformer_Skip()
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

                    FileTransformer.Transform(
                        Path.Combine(masterFolder, MasterFileName),
                        Path.Combine(manifestFolder, ManifestFileName),
                        outputFolder,
                        parallel);

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
        public void FileTransformer_Overwrite()
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

                    FileTransformer.Transform(
                        Path.Combine(masterFolder, MasterFileName),
                        Path.Combine(manifestFolder, ManifestFileName),
                        outputFolder,
                        parallel);

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
        /// Get temp folder path
        /// </summary>
        /// <returns>temp folder path</returns>
        private static string GetTempFolder()
        {
            return Path.Combine(Path.GetTempPath(), "FileTransformation_" + Guid.NewGuid().ToString("N"));
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
    }
}
