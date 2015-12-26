//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="TransformerUnitTest.cs" company="LiXinXu">
//     Copyright LiXinXu.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------------------------------------------------------------------

namespace ConfigurationTransformation.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Xml;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static TestUtility;
    using static Utility;

    /// <summary>
    /// Transformer unit tests
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TransformerUnitTest
    {
        /// <summary>
        /// Test transformation general
        /// </summary>
        [TestMethod]
        public void Transformer_Constructor_NullManifestXml()
        {
            var parameterName = GetMethodParameterName(null, 0);
            foreach (var parallel in GetAllBooleans())
            {
                string error = null;
                try
                {
                    var transformer = new Transformer(null, parallel);
                }
                catch (ArgumentNullException exception)
                {
                    error = exception.Message;
                }

                Assert.IsNotNull(error);
                Assert.IsTrue(error.Contains(parameterName));
            }
        }

        /// <summary>
        /// Test transformation general
        /// </summary>
        [TestMethod]
        public void Transformer_Generic()
        {
            var expectedFiles = new HashSet<string>()
            {
                "web.dev-eastus.config",
                "web.dev-westus.config",
                "web.prod-eastus.config",
                "web.prod-westus.config",
                "web.uat-eastus.config",
                "web.uat-westus.config",
            };

            foreach (var parallel in GetAllBooleans())
            {
                this.TestTransformation("MasterConfig.xml", "DiffConfig.xml", expectedFiles, parallel);
            }
        }

        /// <summary>
        /// Test transformation passing master XML as null
        /// </summary>
        [TestMethod]
        public void Transformer_Transform_NullXml()
        {
            var manifestXml = this.LoadEmbeddedXml("DiffConfig.xml");
            var transformer = new Transformer(manifestXml, false);
            var parameterName = GetMethodParameterName(nameof(Transformer.Transform), 0);

            foreach (var parallel in GetAllBooleans())
            {
                string error = null;
                try
                {
                    transformer.Transform(
                        null,
                        (outputFormat, scopes, newXml) =>
                        {
                        },
                        parallel);
                }
                catch (ArgumentNullException exception)
                {
                    error = exception.Message;
                }

                Assert.IsNotNull(error);
                Assert.IsTrue(error.Contains(parameterName));
            }
        }

        /// <summary>
        /// Test transformation passing master XML as null
        /// </summary>
        [TestMethod]
        public void Transformer_Transform_NullAction()
        {
            var manifestXml = this.LoadEmbeddedXml("DiffConfig.xml");
            var transformer = new Transformer(manifestXml, false);
            var parameterName = GetMethodParameterName(nameof(Transformer.Transform), 1);

            foreach (var parallel in GetAllBooleans())
            {
                string error = null;
                var masterXml = this.LoadEmbeddedXml("MasterConfig.xml");
                try
                {
                    transformer.Transform(masterXml, null, parallel);
                }
                catch (ArgumentNullException exception)
                {
                    error = exception.Message;
                }

                Assert.IsNotNull(error);
                Assert.IsTrue(error.Contains(parameterName));
            }
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
                typeof(Transformer),
                methodName,
                BindingFlags.Public | BindingFlags.Instance,
                parameterIndex).Name;
        }

        /// <summary>
        /// Test transformation
        /// </summary>
        /// <param name="masterXmlName">master XML file name</param>
        /// <param name="manifestXmlName">manifest XML file name</param>
        /// <param name="expectedFiles">expected files</param>
        /// <param name="parallel">parallel process</param>
        private void TestTransformation(string masterXmlName, string manifestXmlName, ISet<string> expectedFiles, bool parallel)
        {
            var manifestXml = this.LoadEmbeddedXml(manifestXmlName);
            var transformer = new Transformer(manifestXml, parallel);

            var masterXml = this.LoadEmbeddedXml(masterXmlName);

            var outputs = new Dictionary<string, XmlElement>();

            transformer.Transform(
                masterXml,
                (outputFormat, scopes, newXml) =>
                {
                    var outputName = outputFormat;
                    foreach (var pair in scopes)
                    {
                        var key = $"{{{pair.Key}}}".ToString(CultureInfo.InvariantCulture);
                        outputName = outputName.Replace(key, pair.Value);
                    }

                    lock(outputs)
                    {
                        outputs.Add(outputName, newXml);
                    }
                },
                parallel);

            Assert.AreEqual(expectedFiles.Count, outputs.Count);
            foreach (var name in expectedFiles)
            {
                XmlElement actualXml;
                Assert.IsTrue(outputs.TryGetValue(name, out actualXml));
                Assert.IsNotNull(actualXml);

                var expectedXml = this.LoadEmbeddedXml(name);
                Assert.IsTrue(XmlAreSame(expectedXml, actualXml));
            }
        }

        /// <summary>
        /// Load embedded XML
        /// </summary>
        /// <param name="name">base file name</param>
        /// <returns>XML from assembly</returns>
        private XmlElement LoadEmbeddedXml(string name)
        {
            return LoadXml(name, "TransformerTestData.", this.GetType());
        }
    }
}
