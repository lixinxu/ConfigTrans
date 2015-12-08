﻿//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="XPathCollectionUnitTest.cs" company="LiXinXu">
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

    /// <summary>
    /// XPath collection unit test
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class XPathCollectionUnitTest
    {
        /// <summary>
        /// Test constructor when passing null as path root XML
        /// </summary>
        [TestMethod]
        public void XPathCollection_Constructor_NullPathXml()
        {
            var collection = new XPathCollectionForTest(null, GetXmlNames());
            Assert.IsNull(collection.GetAliasIndicator());
            Assert.IsNull(collection.GetParameterPlaceholder());
            var paths = collection.GetPathCollection();
            Assert.IsTrue((paths == null) || (paths.Count == 0));
        }

        /// <summary>
        /// Test constructor by passing null as XmlNames
        /// </summary>
        [TestMethod]
        public void XPathCollection_Constructor_NullXmlNames()
        {
            string message = null;
            var pathXml = CreatePathXml(
                null,
                null,
                new Dictionary<string, string>()
                {
                    { "AppSettings", "//configuration/appSettings" },
                });
            try
            {
                var collection = new XPathCollection(pathXml, null);
            }
            catch (ArgumentNullException exception)
            {
                message = exception.Message;
            }

            Assert.IsNotNull(message);
            var parameter = GetMethodParameterName(null, 1);
            Assert.IsTrue(message.Contains(parameter));
        }

        /// <summary>
        /// Test GetPath()
        /// </summary>
        [TestMethod]
        public void XPathCollection_Constructor_HasDuplicatedAliasName()
        {
            var key = "key12";
            var pathAlias = new Dictionary<string, string>()
            {
                { key, "value123" },
            };

            var indicator = "!!";
            var parameterPlaceholder = "{PathParam}";
            var names = GetRandomXmlNames();
            var pathXml = CreatePathXml(indicator, parameterPlaceholder, pathAlias, names);
            AddPath(pathXml, key, "anotherPath", names);

            string message = null;
            try
            {
                var collection = new XPathCollection(pathXml, names);
            }
            catch (ArgumentException exception)
            {
                message = exception.Message;
            }

            Assert.IsNotNull(message);
            Assert.IsTrue(message.Contains(key));
        }

        /// <summary>
        /// Test GetPath()
        /// </summary>
        [TestMethod]
        public void XPathCollection_GetPath_Normal()
        {
            var pathCount = 10;
            var pathAlias = new Dictionary<string, string>(pathCount);
            for (var i = 0; i < pathCount; i++)
            {
                var name = $"Path{i}".ToString(CultureInfo.InvariantCulture);
                var path = Guid.NewGuid().ToString("N");
                pathAlias.Add(name, path);
            }

            var indicator = "!!";
            var parameterPlaceholder = "{PathParam}";
            var names = GetRandomXmlNames();
            var pathXml = CreatePathXml(indicator, parameterPlaceholder, pathAlias, names);
            var collection = new XPathCollection(pathXml, names);

            foreach (var pair in pathAlias)
            {
                var key = indicator + pair.Key;
                var actual = collection.GetPath(key, null);
                Assert.AreEqual(pair.Value, actual);
            }
        }

        /// <summary>
        /// Test GetPath() by pass null as alias name
        /// </summary>
        [TestMethod]
        public void XPathCollection_GetPath_WithNullName()
        {
            var pathXml = CreatePathXml(
                null,
                null,
                new Dictionary<string, string>()
                {
                    { "AppSettings", "//configuration/appSettings" },
                });
            var collection = new XPathCollection(pathXml, GetXmlNames());

            string message = null;
            try
            {
                collection.GetPath(null, null);
            }
            catch (ArgumentException exception)
            {
                message = exception.Message;
            }

            Assert.IsNotNull(message);
            var parameter = GetMethodParameterName(nameof(XPathCollection.GetPath), 0);
            Assert.IsTrue(message.Contains(parameter));
        }

        /// <summary>
        /// Test GetPath() by pass empty as alias name
        /// </summary>
        [TestMethod]
        public void XPathCollection_GetPath_WithEmptyName()
        {
            var pathXml = CreatePathXml(
                null,
                null,
                new Dictionary<string, string>()
                {
                    { "AppSettings", "//configuration/appSettings" },
                });
            var collection = new XPathCollection(pathXml, GetXmlNames());

            string message = null;
            try
            {
                collection.GetPath(string.Empty, null);
            }
            catch (ArgumentException exception)
            {
                message = exception.Message;
            }

            Assert.IsNotNull(message);
            var parameter = GetMethodParameterName(nameof(XPathCollection.GetPath), 0);
            Assert.IsTrue(message.Contains(parameter));
        }

        /// <summary>
        /// Test GetPath() use or not use alias
        /// </summary>
        [TestMethod]
        public void XPathCollection_GetPath_Alias()
        {
            var key = "AppSettings";
            var value = "//configuration/appSettings";
            var aliasIndicator = "##";
            var pathXml = CreatePathXml(
                aliasIndicator,
                null,
                new Dictionary<string, string>()
                {
                    { key, value },
                });
            var collection = new XPathCollection(pathXml, GetXmlNames());

            var actual = collection.GetPath(key, null);
            Assert.AreEqual(key, actual);

            actual = collection.GetPath(aliasIndicator + key, null);
            Assert.AreEqual(value, actual);
        }

        /// <summary>
        /// Test GetPath() not use parameter
        /// </summary>
        [TestMethod]
        public void XPathCollection_GetPath_Parameter()
        {
            var aliasIndicator = "##";
            var placeholder = "{data}";

            var key1 = "AppSettings1";
            var value1 = "//configuration/appSettings";
            var key2 = "AppSettings2";
            var value2 = $"//configuration/appSettings/add[@key='{placeholder}']/@value".ToString(CultureInfo.InvariantCulture);
            var pathXml = CreatePathXml(
                aliasIndicator,
                null,
                new Dictionary<string, string>()
                {
                    { key1, value1 },
                    { key2, value2 },
                });
            var collection = new XPathCollection(pathXml, GetXmlNames());

            var actual = collection.GetPath(aliasIndicator + key1, null);
            Assert.AreEqual(value1, actual);
        }

        /// <summary>
        /// Test GetPath() use parameter
        /// </summary>
        [TestMethod]
        public void XPathCollection_GetPath_UseParameter()
        {
            var aliasIndicator = "##";
            var placeholder = "{data}";

            var key1 = "AppSettings1";
            var value1 = "//configuration/appSettings";
            var key2 = "AppSettings2";
            var format = "//configuration/appSettings/add[@key='{0}']/@value";
            var value2 = string.Format(CultureInfo.InvariantCulture, format, placeholder);
            var names = GetXmlNames();
            var pathXml = CreatePathXml(
                aliasIndicator,
                placeholder,
                new Dictionary<string, string>()
                {
                    { key1, value1 },
                    { key2, value2 },
                },
                names);
            var collection = new XPathCollection(pathXml, names);

            var parameter = "XPathData";
            var expected = string.Format(CultureInfo.InvariantCulture, format, parameter);
            var actual = collection.GetPath(aliasIndicator + key2, parameter);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test GetPath() use parameter
        /// </summary>
        [TestMethod]
        public void XPathCollection_GetPath_NotParameterRequiredButProvidedOne()
        {
            var aliasIndicator = "##";
            var placeholder = "{data}";

            var key1 = "AppSettings1";
            var value1 = "//configuration/appSettings";
            var key2 = "AppSettings2";
            var value2 = $"//configuration/appSettings/add[@key='{placeholder}']/@value".ToString(CultureInfo.InvariantCulture);
            var pathXml = CreatePathXml(
                aliasIndicator,
                null,
                new Dictionary<string, string>()
                {
                    { key1, value1 },
                    { key2, value2 },
                });
            var collection = new XPathCollection(pathXml, GetXmlNames());

            string message = null;
            var alias = aliasIndicator + key1;
            try
            {
                var actual = collection.GetPath(alias, "Unused parameter");
            }
            catch (ArgumentException exception)
            {
                message = exception.Message;
            }

            Assert.IsNotNull(message);
            Assert.IsTrue(message.Contains(alias));
        }

        #region helper
        /// <summary>
        /// create path XML for test
        /// </summary>
        /// <param name="aliasIndicator">alias indicator</param>
        /// <param name="parameterPlaceholder">path parameter placeholder</param>
        /// <param name="collection">path name/value collection</param>
        /// <param name="names">XML names</param>
        /// <returns>path XML</returns>
        private static XmlElement CreatePathXml(
            string aliasIndicator,
            string parameterPlaceholder,
            IReadOnlyDictionary<string, string> collection,
            XmlNames names = null)
        {
            if (names == null)
            {
                names = GetXmlNames();
            }

            var document = new XmlDocument();
            var collectionElement = document.CreateElement(names.PathElementName);
            document.AppendChild(collectionElement);
            AddAttribute(collectionElement, names.PathAliasIndicatorAttribute, aliasIndicator);
            AddAttribute(collectionElement, names.PathParameterPlaceholderAttribute, parameterPlaceholder);

            if (collection != null)
            {
                foreach (var pair in collection)
                {
                    AddPath(collectionElement, pair.Key, pair.Value, names);
                }
            }

            return collectionElement;
        }

        /// <summary>
        /// Add a new alias to path XML
        /// </summary>
        /// <param name="element">path root XML</param>
        /// <param name="name">alias name</param>
        /// <param name="path">alias XPath</param>
        /// <param name="names">XML names</param>
        private static void AddPath(XmlElement element, string name, string path, XmlNames names)
        {
            var pathElement = element.OwnerDocument.CreateElement(names.PathAddElementName);
            element.AppendChild(pathElement);
            AddAttribute(pathElement, names.PathNameAttribute, name);
            AddAttribute(pathElement, names.PathValueAttribute, path);
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
                typeof(XPathCollection),
                methodName,
                BindingFlags.Public | BindingFlags.Instance,
                parameterIndex).Name;
        }
        #endregion helper

        /// <summary>
        /// XPath collection wrapper so we can get values
        /// </summary>
        [ExcludeFromCodeCoverage]
        private class XPathCollectionForTest : XPathCollection
        {
            /// <summary>
            /// Initializes a new instance of the<see cref="XPathCollectionForTest" /> class.
            /// </summary>
            /// <param name="pathRootElement">path root element</param>
            /// <param name="names">XML names</param>
            internal XPathCollectionForTest(XmlElement pathRootElement, XmlNames names) : base(pathRootElement, names)
            {
            }

            /// <summary>
            /// Gets default indicator
            /// </summary>
            /// <returns>indicator name</returns>
            internal string GetDefaultAliasIndicator()
            {
                return XPathCollectionForTest.DefaultAliasIndicator;
            }

            /// <summary>
            /// Gets alias indicator
            /// </summary>
            /// <returns>indicator string</returns>
            internal string GetAliasIndicator()
            {
                return this.AliasIndicator;
            }

            /// <summary>
            /// Gets default parameter placeholder
            /// </summary>
            /// <returns>placeholder string</returns>
            internal string GetDefaultParameterPlaceholder()
            {
                return XPathCollectionForTest.DefaultParameterPlaceholder;
            }

            /// <summary>
            /// Gets current parameter placeholder
            /// </summary>
            /// <returns>placeholder string</returns>
            internal string GetParameterPlaceholder()
            {
                return this.ParameterPlaceholder;
            }

            /// <summary>
            /// Get alias collection
            /// </summary>
            /// <returns>path alias dictionary</returns>
            internal IReadOnlyDictionary<string, XPathInformation> GetPathCollection()
            {
                return this.PathCollection;
            }
        }
    }
}