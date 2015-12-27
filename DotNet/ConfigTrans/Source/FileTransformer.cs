//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="FileTransformer.cs" company="LiXinXu">
//     Copyright LiXinXu.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------------------------------------------------------------------

namespace ConfigurationTransformation
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Xml;
    using static Utility;

    /// <summary>
    /// File transformer
    /// </summary>
    public static class FileTransformer
    {
        /// <summary>
        /// Transform XML file
        /// </summary>
        /// <param name="masterXmlFilePath">master XML file path</param>
        /// <param name="manifestXmlFilePath">manifest XML file path</param>
        /// <param name="outputDirectly">output directly</param>
        /// <param name="parallel">parallel process</param>
        public static void Transform(
            string masterXmlFilePath, 
            string manifestXmlFilePath, 
            string outputDirectly, 
            bool parallel)
        {
            if (string.IsNullOrEmpty(masterXmlFilePath))
            {
                var error = $"{nameof(masterXmlFilePath)} is required";
                throw new ArgumentException(error.ToString(CultureInfo.InvariantCulture));
            }

            masterXmlFilePath = Path.GetFullPath(masterXmlFilePath);
            if (!File.Exists(masterXmlFilePath))
            {
                throw new FileNotFoundException("Master XML file was not found", masterXmlFilePath);
            }

            if (string.IsNullOrEmpty(manifestXmlFilePath))
            {
                var error = $"{nameof(manifestXmlFilePath)} is required";
                throw new ArgumentException(error.ToString(CultureInfo.InvariantCulture));
            }

            manifestXmlFilePath = Path.GetFullPath(manifestXmlFilePath);
            if (!File.Exists(manifestXmlFilePath))
            {
                throw new FileNotFoundException("Manifest XML file was not found", manifestXmlFilePath);
            }

            if (string.IsNullOrEmpty(outputDirectly))
            {
                var error = $"{nameof(outputDirectly)} is required";
                throw new ArgumentException(error.ToString(CultureInfo.InvariantCulture));
            }

            outputDirectly = Path.GetFullPath(outputDirectly);

            var masterDoc = new XmlDocument();
            masterDoc.Load(masterXmlFilePath);

            var manifestDoc = new XmlDocument();
            manifestDoc.Load(manifestXmlFilePath);

            var transformer = new Transformer(manifestDoc.DocumentElement, parallel);
            transformer.Transform(
                masterDoc.DocumentElement,
                (outputFormat, scopes, newXml) =>
                {
                    var outputName = outputFormat;
                    foreach (var pair in scopes)
                    {
                        var key = $"{{{pair.Key}}}".ToString(CultureInfo.InvariantCulture);
                        outputName = outputName.Replace(key, pair.Value);
                    }

                    var path = Path.Combine(outputDirectly, outputName);
                    var folder = Path.GetDirectoryName(path);
                    var writeFile = true;
                    if (Directory.Exists(folder))
                    {
                        if (File.Exists(path))
                        {
                            var oldDoc = new XmlDocument();
                            try
                            {
                                oldDoc.Load(path);
                                if (XmlAreSame(newXml, oldDoc.DocumentElement))
                                {
                                    writeFile = false;
                                }
                            }
                            catch (XmlException)
                            {
                            }
                        }
                    }
                    else
                    {
                        Directory.CreateDirectory(folder);
                    }

                    if (writeFile)
                    {
                        var settings = new XmlWriterSettings()
                        {
                            Indent = true,
                            Encoding = Encoding.UTF8,
                        };
                        using (var writer = XmlTextWriter.Create(path, settings))
                        {
                            newXml.WriteTo(writer);
                        }
                    }
                },
                parallel);
        }
    }
}
