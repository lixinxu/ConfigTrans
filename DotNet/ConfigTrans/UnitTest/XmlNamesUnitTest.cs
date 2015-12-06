//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="XmlNamesUnitTest.cs" company="LiXinXu">
//     Copyright LiXinXu.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------------------------------------------------------------------

namespace ConfigurationTransformation.UnitTest
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    /// <summary>
    /// Test XML name configuration
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class XmlNamesUnitTest
    {
        /// <summary>
        /// Test XML names if pass null as configuration
        /// </summary>
        [TestMethod]
        public void XmlNames_NullConfiguration()
        {
            TestNullOrEmptyConfig(null);
        }

        /// <summary>
        /// Test XML names if pass empty configuration collection
        /// </summary>
        [TestMethod]
        public void XmlNames_EmptyConfiguration()
        {
            TestNullOrEmptyConfig(new NameValueCollection());
        }

        /// <summary>
        /// Test XML names if pass configuration which change default
        /// </summary>
        [TestMethod]
        public void XmlNames_Updated()
        {
            var configuration = new NameValueCollection();
            var expected = Guid.NewGuid().ToString("N");
            configuration[nameof(XmlNames.OutputFormatAttribute)] = expected;
            var instance = new XmlNames(configuration);
            var updated = GetUpdatedItems(instance);
            AssertNameValueCollectionAreEqual(configuration, updated);
        }

        /// <summary>
        /// Test default XML names constructor
        /// </summary>
        [TestMethod]
        public void XmlNames_Default()
        {
            var configuration = new NameValueCollection();
            ProcessProperties(
                (propertyInformation, configurationAttribute) =>
                {
                    var value = ConfigurationManager.AppSettings[propertyInformation.Name];
                    if (value != null)
                    {
                        configuration[propertyInformation.Name] = value;
                    }
                });
            Assert.IsTrue(configuration.Count > 0);
            var instance = new XmlNames();
            var updated = GetUpdatedItems(instance);
            AssertNameValueCollectionAreEqual(configuration, updated);
        }

        private void TestNullOrEmptyConfig(NameValueCollection configuration)
        {
            var instance = new XmlNames(configuration);
            var updated = GetUpdatedItems(instance);
            Assert.AreEqual(0, updated.Count);
        }

        private NameValueCollection GetUpdatedItems(XmlNames instance)
        {
            var valueCollection = new NameValueCollection();
            ProcessProperties(
                (propertyInformation, configurationAttribute) =>
                {
                    var actual = propertyInformation.GetValue(instance) as string;
                    Assert.IsFalse(string.IsNullOrWhiteSpace(actual));
                    if (actual != configurationAttribute.DefaultValue)
                    {
                        valueCollection[propertyInformation.Name] = actual;
                    }
                });
            return valueCollection;
        }

        private NameValueCollection GetDefaultValues()
        {
            var valueCollection = new NameValueCollection();
            ProcessProperties(
                (propertyInformation, configurationAttribute)=>
                {
                    valueCollection[propertyInformation.Name] = configurationAttribute.DefaultValue;
                });
            Assert.IsTrue(valueCollection.Count > 0);
            return valueCollection;
        }

        private void ProcessProperties(Action<PropertyInfo, XmlNames.ConfigurationItemAttribute> processItem)
        {
            var type = typeof(XmlNames);
            var propertyInformationCollection = type.GetProperties();
            foreach (var propertyInformation in propertyInformationCollection)
            {
                var attribute = propertyInformation.GetCustomAttribute<XmlNames.ConfigurationItemAttribute>();
                if (attribute != null)
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(attribute.DefaultValue));
                    processItem(propertyInformation, attribute);
                }
            }
        }

        private static void AssertNameValueCollectionAreEqual(NameValueCollection expected, NameValueCollection actual)
        {
            Assert.AreEqual(expected.Count, actual.Count);
            foreach (var key in expected.AllKeys)
            {
                var value = actual[key];
                Assert.IsNotNull(value);
                Assert.AreEqual(expected[key], value);
            }
        }
    }
}
