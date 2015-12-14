//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="ManifestXmlUtility.cs" company="LiXinXu">
//     Copyright LiXinXu.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------------------------------------------------------------------

namespace ConfigurationTransformation.UnitTest
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml;

    using static TestUtility;

    /// <summary>
    /// Manifest XML utility
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ManifestXmlUtility
    {
        /// <summary>
        /// Create path XML
        /// </summary>
        /// <param name="name">name attribute</param>
        /// <param name="path">path attribute</param>
        /// <param name="names">XML names</param>
        /// <returns>path XML</returns>
        public static XmlElement CreatePathInformationXml(string name, string path, XmlNames names = null)
        {
            return CreatePathInformationXml(new XmlDocument(), name, path, names);
        }

        /// <summary>
        /// Create XPath collection XML
        /// </summary>
        /// <param name="document">XML document</param>
        /// <param name="name">alias name</param>
        /// <param name="path">XPath value</param>
        /// <param name="names">XML names</param>
        /// <returns>XPath collection root element</returns>
        public static XmlElement CreatePathInformationXml(XmlDocument document, string name, string path, XmlNames names = null)
        {
            if (names == null)
            {
                names = GetXmlNames();
            }

            var element = document.CreateElement(names.PathElementName);
            document.AppendChild(element);
            AddAttribute(element, names.PathNameAttribute, name);
            AddAttribute(element, names.PathValueAttribute, path);
            return element;
        }

        /// <summary>
        /// create path XML for test
        /// </summary>
        /// <param name="aliasIndicator">alias indicator</param>
        /// <param name="parameterPlaceholder">path parameter placeholder</param>
        /// <param name="collection">path name/value collection</param>
        /// <param name="names">XML names</param>
        /// <returns>path XML</returns>
        public static XmlElement CreatePathCollectionXml(
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
                    AddPathToCollectionXml(collectionElement, pair.Key, pair.Value, names);
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
        public static void AddPathToCollectionXml(XmlElement element, string name, string path, XmlNames names)
        {
            var pathElement = element.OwnerDocument.CreateElement(names.PathAddElementName);
            element.AppendChild(pathElement);
            AddAttribute(pathElement, names.PathNameAttribute, name);
            AddAttribute(pathElement, names.PathValueAttribute, path);
        }

        /// <summary>
        /// Create Transformation Command XML
        /// </summary>
        /// <param name="command">command type</param>
        /// <param name="path">XPath to located XML node</param>
        /// <param name="parameter">parameter for generate XPath, it necessary</param>
        /// <param name="name">name of new attribute</param>
        /// <param name="value">value for creating/updating node</param>
        /// <param name="valueInAttribute">save value in attribute. if false, save a CDATA</param>
        /// <param name="names">XML names</param>
        /// <param name="document">XML document</param>
        /// <returns>Transformation XML instance</returns>
        public static XmlElement CreateCommandXml(
            string command,
            string path,
            string parameter,
            string name,
            string value,
            bool valueInAttribute,
            XmlNames names,
            XmlDocument document = null)
        {
            if (document == null)
            {
                document = new XmlDocument();
            }

            var element = document.CreateElement(command);
            AddAttribute(element, names.TransformPathAttribute, path);
            AddAttribute(element, names.TransformParameterAttribute, parameter);
            AddAttribute(element, names.TransformNameAttribute, name);
            if (valueInAttribute)
            {
                AddAttribute(element, names.TransformValueAttribute, value);
            }
            else if (!string.IsNullOrEmpty(value))
            {
                var cdata = document.CreateCDataSection(value);
                element.AppendChild(cdata);
            }

            return element;
        }
    }
}
