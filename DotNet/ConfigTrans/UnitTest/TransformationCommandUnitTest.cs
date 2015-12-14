//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="TransformationCommandUnitTest.cs" company="LiXinXu">
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

    using static ConfigurationXmlUtility;
    using static ManifestXmlUtility;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static TestUtility;
    using static Utility;

    /// <summary>
    /// TransformationCommand UnitTest
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TransformationCommandUnitTest
    {
        /// <summary>
        /// Default path collection
        /// </summary>
        private static readonly IReadOnlyDictionary<string, string> DefaultPathCollection = new Dictionary<string, string>()
        {
                { "AppSettings1", "//configuration/appSettings" },
                { "AppSettings2", $"//configuration/appSettings/add[@key='{GetDefaultXPathParameterPlaceholder()}']/@value".ToString(CultureInfo.InvariantCulture) }
        };

        #region constructor test
        /// <summary>
        /// Test constructor by passing null transformation XML
        /// </summary>
        [TestMethod]
        public void TransformationCommand_Constructor_NullXml()
        {
            var names = GetXmlNames();
            var pathCollectionXml = CreatePathCollectionXml(null, null, DefaultPathCollection);
            var pathCollection = new XPathCollection(pathCollectionXml, names);
            string message = null;
            try
            {
                var instance = new TransformationCommand(null, names, pathCollection);
            }
            catch (ArgumentNullException exception)
            {
                message = exception.Message;
            }

            Assert.IsNotNull(message);
            var parameterName = GetParameterName(null, 0);
            Assert.IsTrue(message.Contains(parameterName));
        }

        /// <summary>
        /// Test constructor by passing null as XML names
        /// </summary>
        [TestMethod]
        public void TransformationCommand_Constructor_NullNames()
        {
            var names = GetXmlNames();
            foreach (var valueInAttribute in GetAllBooleans())
            {
                var pathCollectionXml = CreatePathCollectionXml(null, null, DefaultPathCollection);
                var pathCollection = new XPathCollection(pathCollectionXml, names);
                var commmandXml = CreateCommandXml(names.TransformAddElement, "path", null, "name", "value", valueInAttribute, names);
                string message = null;
                try
                {
                    var instance = new TransformationCommand(commmandXml, null, pathCollection);
                }
                catch (ArgumentNullException exception)
                {
                    message = exception.Message;
                }

                Assert.IsNotNull(message);
                var parameterName = GetParameterName(null, 1);
                Assert.IsTrue(message.Contains(parameterName));
            }
        }

        /// <summary>
        /// Test constructor by passing null as XPath collection
        /// </summary>
        [TestMethod]
        public void TransformationCommand_Constructor_NullCollection()
        {
            var names = GetXmlNames();
            foreach (var valueInAttribute in GetAllBooleans())
            {
                var commmandXml = CreateCommandXml(names.TransformAddElement, "path", null, "name", "value", valueInAttribute, names);
                string message = null;
                try
                {
                    var instance = new TransformationCommand(commmandXml, names, null);
                }
                catch (ArgumentNullException exception)
                {
                    message = exception.Message;
                }

                Assert.IsNotNull(message);
                var parameterName = GetParameterName(null, 2);
                Assert.IsTrue(message.Contains(parameterName));
            }
        }
        #endregion constructor test

        #region add attribute test
        /// <summary>
        /// Test adding new attribute using XPath in XML directly
        /// </summary>
        [TestMethod]
        public void TransformationCommand_AddAttribute_UsePath()
        {
            var names = GetXmlNames();

            var key = "item3";
            var path = $"//{ConfigRootXmlElementName}/{ConfigAppSettingsElementName}/{ConfigAddItemElementName}[@{ConfigKeyAttributeName}='{key}']"
                .ToString(CultureInfo.InvariantCulture);

            var pathCollection = new XPathCollection(null, names);

            this.TestAddAttribute(pathCollection, path, null, names);
        }

        /// <summary>
        /// Test adding new attribute by using XPath alias but no parameter
        /// </summary>
        [TestMethod]
        public void TransformationCommand_AddAttribute_UseAliasWithoutParameter()
        {
            var names = GetXmlNames();
            var indicator = GetDefaultXPathAliasIndicator();

            var key = "item3";
            var alias = key + "Path";
            var path = $"//{ConfigRootXmlElementName}/{ConfigAppSettingsElementName}/{ConfigAddItemElementName}[@{ConfigKeyAttributeName}='{key}']"
                .ToString(CultureInfo.InvariantCulture);
            var pathCollectionXml = CreatePathCollectionXml(
                null,
                null,
                new Dictionary<string, string>()
                {
                    { alias, path }
                },
                names);
            var pathCollection = new XPathCollection(pathCollectionXml, names);

            this.TestAddAttribute(pathCollection, indicator + alias, null, names);
        }

        /// <summary>
        /// Test adding new attribute by using XPath alias with parameter
        /// </summary>
        [TestMethod]
        public void TransformationCommand_AddAttribute_UseAliasWithParameter()
        {
            var names = GetXmlNames();
            var indicator = GetDefaultXPathAliasIndicator();
            var placeholder = GetDefaultXPathParameterPlaceholder();

            var key = "item3";
            var alias = key + "Path";
            var path = $"//{ConfigRootXmlElementName}/{ConfigAppSettingsElementName}/{ConfigAddItemElementName}[@{ConfigKeyAttributeName}='{placeholder}']"
                .ToString(CultureInfo.InvariantCulture);
            var pathCollectionXml = CreatePathCollectionXml(
                null,
                null,
                new Dictionary<string, string>()
                {
                    { alias, path }
                },
                names);
            var pathCollection = new XPathCollection(pathCollectionXml, names);

            this.TestAddAttribute(pathCollection, indicator + alias, key, names);
        }
        #endregion add attribute test

        #region helper
        /// <summary>
        /// Get method parameter name
        /// </summary>
        /// <param name="methodName">method name. if null, constructor will be used</param>
        /// <param name="parameterIndex">position of parameter</param>
        /// <param name="bindingFlags">binding flags</param>
        /// <returns>parameter name</returns>
        private static string GetParameterName(
            string methodName,
            int parameterIndex,
            BindingFlags bindingFlags = BindingFlags.Public)
        {
            return GetParameterInformation(typeof(TransformationCommand), methodName, bindingFlags, parameterIndex).Name;
        }

        /// <summary>
        /// Load embedded XML
        /// </summary>
        /// <param name="name">base file name</param>
        /// <param name="id">file id</param>
        /// <returns>XML from assembly</returns>
        private XmlElement LoadEmbeddedXml(string name, int id)
        {
            var fullName = $"{name}{id}.xml".ToString(CultureInfo.InvariantCulture);
            return LoadXml(fullName, "TransformationCommandTestData.", this.GetType());
        }

        /// <summary>
        /// Test transformation adding attribute
        /// </summary>
        /// <param name="pathCollection">XPath collection</param>
        /// <param name="path">XPath string</param>
        /// <param name="parameter">path parameter</param>
        /// <param name="names">XML names</param>
        private void TestAddAttribute(XPathCollection pathCollection, string path, string parameter, XmlNames names)
        {
            const string XmlFileBaseName = "AddAttribute";
            foreach (var valueInAttribute in GetAllBooleans())
            {
                var configurationXml = this.LoadEmbeddedXml(XmlFileBaseName, 1);

                var commandXml = CreateCommandXml(names.TransformAddElement, path, parameter, "lockAttributes", "true", valueInAttribute, names);
                var command = new TransformationCommand(commandXml, names, pathCollection);

                command.Transform(configurationXml);

                var expected = this.LoadEmbeddedXml(XmlFileBaseName, 2);
                Assert.IsTrue(XmlAreSame(expected, configurationXml));
            }
        }
        #endregion helper
    }
}
