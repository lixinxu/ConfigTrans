﻿//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="TestUtility.cs" company="LiXinXu">
//     Copyright LiXinXu.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------------------------------------------------------------------

namespace ConfigurationTransformation.UnitTest
{
    using System;
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
        private static readonly XmlNames names = new XmlNames();

        /// <summary>
        /// Load XML from assembly
        /// </summary>
        /// <param name="name">file name</param>
        /// <param name="folder">folder related to type location</param>
        /// <param name="type">type which at same location of the file. Use utility type of type is not specified</param>
        /// <returns>XML from assembly</returns>
        public static XmlElement LoadXml(string name, string folder = null, Type type=null)
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
        /// Get XML names
        /// </summary>
        /// <returns>XML names instance</returns>
        public static XmlNames GetXmlNames()
        {
            return names;
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
    }
}