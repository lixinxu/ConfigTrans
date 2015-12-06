﻿//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="XPathInformationUnitTest.cs" company="LiXinXu">
//     Copyright LiXinXu.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------------------------------------------------------------------

namespace ConfigurationTransformation.UnitTest
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Xml;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static TestUtility;

    /// <summary>
    /// XPathInformation unit test
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class XPathInformationUnitTest
    {
        /// <summary>
        /// Test constructor when pass null path XML
        /// </summary>
        [TestMethod]
        public void XPathInformation_Constructor_NullXml()
        {
            string message = null;
            try
            {
                var information = new XPathInformation(null, GetXmlNames(), "placeholder");
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
        /// Test constructor when pass null XML names
        /// </summary>
        [TestMethod]
        public void XPathInformation_Constructor_NullNames()
        {
            string message = null;
            var document = new XmlDocument();
            var element = document.CreateElement("manifest");
            document.AppendChild(element);
            try
            {
                var information = new XPathInformation(element, null, "placeholder");
            }
            catch (ArgumentNullException exception)
            {
                message = exception.Message;
            }

            Assert.IsNotNull(message);
            var parameterName = GetParameterName(null, 1);
            Assert.IsTrue(message.Contains(parameterName));
        }

        /// <summary>
        /// Test constructor when pass null placeholder
        /// </summary>
        [TestMethod]
        public void XPathInformation_Constructor_NullPlaceholder()
        {
            string message = null;
            var element = CreatePathXml("abc", "path");
            try
            {
                var information = new XPathInformation(element, GetXmlNames(), null);
            }
            catch (ArgumentException exception)
            {
                message = exception.Message;
            }

            Assert.IsNotNull(message);
            var parameterName = GetParameterName(null, 2);
            Assert.IsTrue(message.Contains(parameterName));
        }

        /// <summary>
        /// Test constructor when pass empty placeholder
        /// </summary>
        [TestMethod]
        public void XPathInformation_Constructor_EmptyPlaceholder()
        {
            string message = null;
            var element = CreatePathXml("abc", "path");
            try
            {
                var information = new XPathInformation(element, GetXmlNames(), string.Empty);
            }
            catch (ArgumentException exception)
            {
                message = exception.Message;
            }

            Assert.IsNotNull(message);
            var parameterName = GetParameterName(null, 2);
            Assert.IsTrue(message.Contains(parameterName));
        }

        /// <summary>
        /// Test constructor no name attribute
        /// </summary>
        [TestMethod]
        public void XPathInformation_Constructor_NullAliasName()
        {
            string message = null;
            try
            {
                TestAlias(null);
            }
            catch (ArgumentException exception)
            {
                message = exception.Message;
            }

            Assert.IsNotNull(message);
            Assert.IsTrue(message.Contains(GetXmlNames().PathNameAttribute));
        }

        /// <summary>
        /// Test constructor empty name attribute
        /// </summary>
        [TestMethod]
        public void XPathInformation_Constructor_EmptyAliasName()
        {
            string message = null;
            try
            {
                TestAlias(string.Empty);
            }
            catch (ArgumentException exception)
            {
                message = exception.Message;
            }

            Assert.IsNotNull(message);
            Assert.IsTrue(message.Contains(GetXmlNames().PathNameAttribute));
        }

        /// <summary>
        /// Test constructor normal name attribute
        /// </summary>
        [TestMethod]
        public void XPathInformation_Constructor_NormalAliasName()
        {
            TestAlias("name" + Guid.NewGuid().ToString("N"));
        }

        /// <summary>
        /// Test constructor no null path
        /// </summary>
        [TestMethod]
        public void XPathInformation_Constructor_NullPath()
        {
            string message = null;
            try
            {
                TestPath(null);
            }
            catch (ArgumentException exception)
            {
                message = exception.Message;
            }

            Assert.IsNotNull(message);
            Assert.IsTrue(message.Contains(GetXmlNames().PathValueAttribute));
        }

        /// <summary>
        /// Test constructor empty path
        /// </summary>
        [TestMethod]
        public void XPathInformation_Constructor_EmptyPath()
        {
            string message = null;
            try
            {
                TestPath(string.Empty);
            }
            catch (ArgumentException exception)
            {
                message = exception.Message;
            }

            Assert.IsNotNull(message);
            Assert.IsTrue(message.Contains(GetXmlNames().PathValueAttribute));
        }

        /// <summary>
        /// Test constructor normal path
        /// </summary>
        [TestMethod]
        public void XPathInformation_Constructor_NormalPath()
        {
            TestPath("name" + Guid.NewGuid().ToString("N"));
        }

        /// <summary>
        /// Test constructor path does not have placeholder
        /// </summary>
        [TestMethod]
        public void XPathInformation_Constructor_NoPlaceholder()
        {
            var placeholder = "{Placeholder" + Guid.NewGuid().ToString("N") + "}";
            var path = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            TestPlaceholder(path, placeholder);
        }

        /// <summary>
        /// Test constructor path has placeholder
        /// </summary>
        [TestMethod]
        public void XPathInformation_Constructor_HasPlaceholder()
        {
            var placeholder = "{Placeholder" + Guid.NewGuid().ToString("N") + "}";
            var path = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", Guid.NewGuid(), placeholder, Guid.NewGuid());
            TestPlaceholder(path, placeholder);
        }

        /// <summary>
        /// Get parameter name
        /// </summary>
        /// <param name="name">method name. null for constructor</param>
        /// <param name="parameterIndex">parameter position</param>
        /// <param name="bindingFlags">binding flag</param>
        /// <returns>parameter name</returns>
        private static string GetParameterName(
            string name, 
            int parameterIndex,
            BindingFlags bindingFlags = BindingFlags.Public)
        {
            return GetParameterInformation(typeof(XPathInformation), name, bindingFlags, parameterIndex).Name;
        }

        /// <summary>
        /// Create path XML
        /// </summary>
        /// <param name="name">name attribute</param>
        /// <param name="path">path attribute</param>
        /// <returns>path XML</returns>
        private static XmlElement CreatePathXml(string name, string path)
        {
            var names = GetXmlNames();
            var document = new XmlDocument();
            var element = document.CreateElement(names.PathElementName);
            document.AppendChild(element);
            AddAttribute(element, names.PathNameAttribute, name);
            AddAttribute(element, names.PathValueAttribute, path);
            return element;
        }

        /// <summary>
        /// Test path name
        /// </summary>
        /// <param name="alias">path name</param>
        private static void TestAlias(string alias)
        {
            var element = CreatePathXml(alias, "path" + Guid.NewGuid().ToString("N"));
            var pathInformation = new XPathInformation(element, GetXmlNames(), "placeholder");
            Assert.AreEqual(alias, pathInformation.Name);
        }

        /// <summary>
        /// Test path value
        /// </summary>
        /// <param name="path">XPath to test</param>
        private static void TestPath(string path)
        {
            var element = CreatePathXml("alias" + Guid.NewGuid().ToString("N"), path);
            var pathInformation = new XPathInformation(element, GetXmlNames(), "placeholder");
            Assert.AreEqual(path, pathInformation.Path);
        }

        /// <summary>
        /// Test placeholder
        /// </summary>
        /// <param name="path">XPath value</param>
        /// <param name="placeholder">parameter placeholder</param>
        private static void TestPlaceholder(string path, string placeholder)
        {
            var element = CreatePathXml("alias" + Guid.NewGuid().ToString("N"), path);
            var pathInformation = new XPathInformation(element, GetXmlNames(), placeholder);
            Assert.AreEqual(path.Contains(placeholder), pathInformation.HasParameterPlaceholder);
        }
    }
}
