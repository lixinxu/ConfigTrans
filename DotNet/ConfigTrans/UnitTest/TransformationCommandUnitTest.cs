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
            TestAdd_UsePath(this.TestAddAttribute);
        }

        /// <summary>
        /// Test adding new attribute by using XPath alias but no parameter
        /// </summary>
        [TestMethod]
        public void TransformationCommand_AddAttribute_UseAliasWithoutParameter()
        {
            this.TestAdd_UseAliasWithoutParameter(this.TestAddAttribute);
        }

        /// <summary>
        /// Test adding new attribute by using XPath alias with parameter
        /// </summary>
        [TestMethod]
        public void TransformationCommand_AddAttribute_UseAliasWithParameter()
        {
            this.TestAdd_UseAliasWithParameter(this.TestAddAttribute);
        }
        #endregion add attribute test

        #region add element
        /// <summary>
        /// Test adding new attribute using XPath in XML directly
        /// </summary>
        [TestMethod]
        public void TransformationCommand_AddElement_UsePath()
        {
            this.TestAdd_UsePath(this.TestAddElement);
        }

        /// <summary>
        /// Test adding new element by using XPath alias but no parameter
        /// </summary>
        [TestMethod]
        public void TransformationCommand_AddElement_UseAliasWithoutParameter()
        {
            this.TestAdd_UseAliasWithoutParameter(this.TestAddElement);
        }

        /// <summary>
        /// Test adding new attribute by using XPath alias with parameter
        /// </summary>
        [TestMethod]
        public void TransformationCommand_AddElement_UseAliasWithParameter()
        {
            this.TestAdd_UseAliasWithParameter(this.TestAddElement);
        }
        #endregion add element

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

        /// <summary>
        /// Test transformation adding new element
        /// </summary>
        /// <param name="pathCollection">XPath collection</param>
        /// <param name="path">XPath string</param>
        /// <param name="parameter">path parameter</param>
        /// <param name="names">XML names</param>
        private void TestAddElement(XPathCollection pathCollection, string path, string parameter, XmlNames names)
        {
            const string XmlFileBaseName = "AddElement";
            const string newElement = "<locator type=\"database\"><connection>connection string</connection></locator>";
            foreach (var valueInAttribute in GetAllBooleans())
            {
                var configurationXml = this.LoadEmbeddedXml(XmlFileBaseName, 1);

                var commandXml = CreateCommandXml(names.TransformAddElement, path, parameter, null, newElement, valueInAttribute, names);
                var command = new TransformationCommand(commandXml, names, pathCollection);

                command.Transform(configurationXml);

                var expected = this.LoadEmbeddedXml(XmlFileBaseName, 2);
                Assert.IsTrue(XmlAreSame(expected, configurationXml));
            }
        }

        /// <summary>
        /// Test add attribute/element use XPath directly
        /// </summary>
        /// <param name="tester">delegate to test adding attribute or element</param>
        private void TestAdd_UsePath(Action<XPathCollection, string, string, XmlNames> tester)
        {
            var names = GetXmlNames();

            var key = "item3";
            var path = $"//{ConfigRootXmlElementName}/{ConfigAppSettingsElementName}/{ConfigAddItemElementName}[@{ConfigKeyAttributeName}='{key}']"
                .ToString(CultureInfo.InvariantCulture);

            var pathCollection = new XPathCollection(null, names);

            tester(pathCollection, path, null, names);
        }

        /// <summary>
        /// Test adding attribute/element use XPath alias without parameter
        /// </summary>
        /// <param name="tester">delegate to test adding attribute or element</param>
        private void TestAdd_UseAliasWithoutParameter(Action<XPathCollection, string, string, XmlNames> tester)
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

            tester(pathCollection, indicator + alias, null, names);
        }

        /// <summary>
        /// Test adding new attribute/element by using XPath alias with parameter
        /// </summary>
        /// <param name="tester">delegate to test adding attribute or element</param>
        private void TestAdd_UseAliasWithParameter(Action<XPathCollection, string, string, XmlNames> tester)
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

            tester(pathCollection, indicator + alias, key, names);
        }
        #endregion helper
    }
}
