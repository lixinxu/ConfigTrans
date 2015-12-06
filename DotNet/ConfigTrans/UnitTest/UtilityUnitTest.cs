//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="UtilityUnitTest.cs" company="LiXinXu">
//     Copyright LiXinXu.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------------------------------------------------------------------

namespace ConfigurationTransformation.UnitTest
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml;
    using static Utility;
    using System.Reflection;
    using System.Collections.Generic;

    /// <summary>
    /// Utility unit test
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class UtilityUnitTest
    {
        #region GetAttribute()
        [TestMethod]
        public void Utility_GetAttribute_NullXml()
        {
            var hadException = false;
            try
            {
                var value = GetAttribute(null, "name");
            }
            catch (ArgumentNullException exception)
            {
                hadException = true;
                var parameterName = GetMethodParameterName(nameof(Utility.GetAttribute), 0);
                Assert.IsTrue(exception.Message.Contains(parameterName));
            }
            Assert.IsTrue(hadException);
        }

        [TestMethod]
        public void Utility_GetAttribute_NullAttributeName()
        {
            var document = new XmlDocument();
            document.LoadXml("<a b=\"123\" />");
            var elemet = document.DocumentElement;
            var hadException = false;
            try
            {
                var value = GetAttribute(elemet, null);
            }
            catch (ArgumentException exception)
            {
                hadException = true;
                var parameterName = GetMethodParameterName(nameof(Utility.GetAttribute), 1);
                Assert.IsTrue(exception.Message.Contains(parameterName));
            }
            Assert.IsTrue(hadException);
        }

        [TestMethod]
        public void Utility_GetAttribute_Normal()
        {
            var name = "key";
            var value = Guid.NewGuid().ToString();
            var element = CreateElementForGetAttributeTest(
                new Dictionary<string, string>()
                {
                    { name, value },
                });

            var actual = GetAttribute(element, name);
            Assert.IsNotNull(actual);
            Assert.AreEqual(value, actual);
        }

        [TestMethod]
        public void Utility_GetAttribute_EmptyValue()
        {
            var name = "key";
            var element = CreateElementForGetAttributeTest(
                new Dictionary<string, string>()
                {
                    { name, string.Empty },
                });
            var defaultValue = Guid.NewGuid().ToString();
            var actual = GetAttribute(element, name, defaultValue);
            Assert.IsNotNull(actual);
            Assert.AreEqual(defaultValue, actual);
        }

        [TestMethod]
        public void Utility_GetAttribute_AttributeDoesNotExist()
        {
            var element = CreateElementForGetAttributeTest(
                new Dictionary<string, string>()
                {
                    { "name", "value" },
                });
            var defaultValue = Guid.NewGuid().ToString();
            var actual = GetAttribute(element, "NotExist", defaultValue);
            Assert.IsNotNull(actual);
            Assert.AreEqual(defaultValue, actual);
        }
        #endregion GetAttribute()

        #region XmlAreSame()
        [TestMethod]
        public void Utility_XmlAreSame_SameAttrSameOrderTest()
        {
            Assert.IsTrue(Test("SameAttrSameOrder"));
        }

        [TestMethod]
        public void Utility_XmlAreSame_SameAttrDiffOrderTest()
        {
            Assert.IsTrue(Test("SameAttrDiffOrder"));
        }

        [TestMethod]
        public void Utility_XmlAreSame_DiffAttrTest()
        {
            Assert.IsFalse(Test("DiffAttr"));
        }

        [TestMethod]
        public void Utility_XmlAreSame_DiffAttrDiffValueTest()
        {
            Assert.IsFalse(Test("DiffAttrDiffValue"));
        }

        [TestMethod]
        public void Utility_XmlAreSame_SameElementTest()
        {
            Assert.IsTrue(Test("SameElement"));
        }

        [TestMethod]
        public void Utility_XmlAreSame_DiffElementAddChildTest()
        {
            Assert.IsFalse(Test("DiffElementAddChild"));
        }

        [TestMethod]
        public void Utility_XmlAreSame_DiffElementNameChangedTest()
        {
            Assert.IsFalse(Test("DiffElementNameChanged"));
        }

        [TestMethod]
        public void Utility_XmlAreSame_DiffElementDiffValueTest()
        {
            Assert.IsFalse(Test("DiffElementDiffValue"));
        }
        #endregion XmlAreSame()

        #region helper
        /// <summary>
        /// Get method parameter name
        /// </summary>
        /// <param name="methodName">name of the method in utility class</param>
        /// <param name="parameterId">the index of the method parameter</param>
        /// <returns>parameter name</returns>
        private static string GetMethodParameterName(string methodName, int parameterId)
        {
            var type = typeof(Utility);
            var methodInformation = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
            Assert.IsNotNull(methodInformation);
            var parameterInformationList = methodInformation.GetParameters();
            return parameterInformationList[parameterId].Name;
        }

        /// <summary>
        /// create an element for GetAttribute() test
        /// </summary>
        /// <param name="attributes">attribute name/value collection</param>
        /// <returns>element which contains those attributes</returns>
        private XmlElement CreateElementForGetAttributeTest(IReadOnlyDictionary<string, string> attributes)
        {
            var document = new XmlDocument();
            var element = document.CreateElement("root");
            foreach (var pair in attributes)
            {
                var attribute = document.CreateAttribute(pair.Key);
                attribute.Value = pair.Value;
                element.Attributes.Append(attribute);
            }
            document.AppendChild(element);
            return element;
        }

        private bool Test(string name)
        {
            var x = LoadXml(name, 1);
            var y = LoadXml(name, 2);
            return XmlAreSame(x, y);
        }

        private XmlElement LoadXml(string name, int id)
        {
            return LoadXml(string.Format("{0}{1}.xml", name, id));
        }

        private XmlElement LoadXml(string name)
        {
            return TestUtility.LoadXml(name, "XmlCompareData.");
        }
        #endregion helper
    }
}
