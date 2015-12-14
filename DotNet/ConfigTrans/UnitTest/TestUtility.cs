//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="TestUtility.cs" company="LiXinXu">
//     Copyright LiXinXu.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------------------------------------------------------------------

namespace ConfigurationTransformation.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Xml;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Utility for unit test
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class TestUtility
    {
        /// <summary>
        /// XML names which can be shared by all unit tests
        /// </summary>
        private static readonly XmlNames DefaultXmlNames = new XmlNames();

        /// <summary>
        /// All possible booleans
        /// </summary>
        private static readonly bool[] AllBooleans = new bool[] { true, false };

        /// <summary>
        /// Get XML names
        /// </summary>
        /// <returns>XML names instance</returns>
        public static XmlNames GetXmlNames()
        {
            return DefaultXmlNames;
        }

        /// <summary>
        /// Get all possible boolean values
        /// </summary>
        /// <returns>boolean values</returns>
        public static IReadOnlyList<bool> GetAllBooleans()
        {
            return AllBooleans;
        }

        /// <summary>
        /// Create random string
        /// </summary>
        /// <param name="prefix">string prefix</param>
        /// <returns>random string with GUID appended</returns>
        public static string CreateRandomName(string prefix)
        {
            return prefix + Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// Load XML from assembly
        /// </summary>
        /// <param name="name">file name</param>
        /// <param name="folder">folder related to type location</param>
        /// <param name="type">type which at same location of the file. Use utility type of type is not specified</param>
        /// <returns>XML from assembly</returns>
        public static XmlElement LoadXml(string name, string folder = null, Type type = null)
        {
            if (type == null)
            {
                type = typeof(TestUtility);
            }

            XmlElement xml;
            using (var stream = type.Assembly.GetManifestResourceStream(type, folder + name))
            {
                var doc = new XmlDocument();
                doc.Load(stream);
                xml = doc.DocumentElement;
            }

            return xml;
        }

        /// <summary>
        /// Get constructor or method parameter information
        /// </summary>
        /// <param name="type">type which defines the method of constructor</param>
        /// <param name="methodName">method name. if it is null, then constructor</param>
        /// <param name="bindingFlags">binding flags</param>
        /// <param name="parameterIndex">index of the method</param>
        /// <param name="methodTypes">method parameter types</param>
        /// <returns>parameter information</returns>
        public static ParameterInfo GetParameterInformation(
            Type type, 
            string methodName, 
            BindingFlags bindingFlags, 
            int parameterIndex, 
            Type[] methodTypes = null)
        {
            MethodBase methodInformation;
            if (methodTypes == null)
            {
                if (string.IsNullOrEmpty(methodName))
                {
                    var constructors = type.GetConstructors(bindingFlags | BindingFlags.Instance);
                    Assert.AreEqual(1, constructors.Length);
                    methodInformation = constructors[0];
                }
                else
                {
                    methodInformation = type.GetMethod(methodName, bindingFlags);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(methodName))
                {
                    methodInformation = type.GetConstructor(bindingFlags | BindingFlags.Instance, null, methodTypes, null);
                }
                else
                {
                    methodInformation = type.GetMethod(methodName, bindingFlags, null, methodTypes, null);
                }
            }

            Assert.IsNotNull(methodInformation);
            var parameterInformationList = methodInformation.GetParameters();
            Assert.IsNotNull(parameterInformationList);
            Assert.IsTrue(parameterInformationList.Length >= parameterIndex);
            return parameterInformationList[parameterIndex];
        }

        /// <summary>
        /// Create random XML element/attribute names
        /// </summary>
        /// <returns>new XmlNames instances with random names</returns>
        public static XmlNames GetRandomXmlNames()
        {
            var defaultNames = GetXmlNames();
            var propertyInformationList = defaultNames.GetType().GetProperties();
            var newXmlNameValues = new NameValueCollection(propertyInformationList.Length);
            foreach (var propertyInformation in propertyInformationList)
            {
                var attribute = propertyInformation.GetCustomAttribute<XmlNames.ConfigurationItemAttribute>(true);
                if (attribute != null)
                {
                    var value = propertyInformation.GetValue(defaultNames) as string;
                    newXmlNameValues.Add(propertyInformation.Name, CreateRandomName(value));
                }
            }

            return new XmlNames(newXmlNameValues);
        }

        /// <summary>
        /// Add attribute to element
        /// </summary>
        /// <param name="element">XML element to host the attribute</param>
        /// <param name="name">attribute name</param>
        /// <param name="value">attribute value</param>
        /// <remarks>
        /// if name is null, the attribute will not be added.
        /// </remarks>
        public static void AddAttribute(XmlElement element, string name, string value)
        {
            if (value != null)
            {
                var attribute = element.OwnerDocument.CreateAttribute(name);
                attribute.Value = value;
                element.Attributes.Append(attribute);
            }
        }

        /// <summary>
        /// Gets default XPath alias indicator
        /// </summary>
        /// <returns>indicator name</returns>
        public static string GetDefaultXPathAliasIndicator()
        {
            return XPathCollectionForUtility.GetDefaultAliasIndicator();
        }

        /// <summary>
        /// Gets default XPath parameter placeholder
        /// </summary>
        /// <returns>placeholder string</returns>
        public static string GetDefaultXPathParameterPlaceholder()
        {
            return XPathCollectionForUtility.GetDefaultParameterPlaceholder();
        }

        /// <summary>
        /// Dummy class to retrieve internal information
        /// </summary>
        [ExcludeFromCodeCoverage]
        private class XPathCollectionForUtility : XPathCollection
        {
            /// <summary>
            /// Initializes a new instance of the<see cref="XPathCollectionForUtility" /> class.
            /// </summary>
            /// <param name="pathRootElement">path root element</param>
            /// <param name="names">XML names</param>
            internal XPathCollectionForUtility(XmlElement pathRootElement, XmlNames names) : base(pathRootElement, names)
            {
            }

            /// <summary>
            /// Gets default indicator
            /// </summary>
            /// <returns>indicator name</returns>
            internal static string GetDefaultAliasIndicator()
            {
                return XPathCollectionForUtility.DefaultAliasIndicator;
            }

            /// <summary>
            /// Gets default parameter placeholder
            /// </summary>
            /// <returns>placeholder string</returns>
            internal static string GetDefaultParameterPlaceholder()
            {
                return XPathCollectionForUtility.DefaultParameterPlaceholder;
            }
        }
    }
}
