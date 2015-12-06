//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="XPathCollection.cs" company="LiXinXu">
//     Copyright LiXinXu.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------------------------------------------------------------------

namespace ConfigurationTransformation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Xml;
    using static Utility;

    /// <summary>
    /// XPath collection
    /// </summary>
    public class XPathCollection
    {
        /// <summary>
        /// Default path alias indicator
        /// </summary>
        private const string DefaultAliasIndicator = "#";

        /// <summary>
        /// Default path parameter placeholder
        /// </summary>
        private const string DefaultParameterPlaceholder = "{parameter}";

        /// <summary>
        /// Gets alias indicator
        /// </summary>
        private string aliasIndicator;

        /// <summary>
        /// Parameter placeholder
        /// </summary>
        private string parameterPlaceholder;

        /// <summary>
        /// Path collection
        /// </summary>
        private IReadOnlyDictionary<string, XPathInformation> pathCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="XPathCollection" /> class.
        /// </summary>
        /// <param name="manifest">manifest XML</param>
        /// <param name="names">XML names</param>
        public XPathCollection(XmlElement manifest, XmlNames names)
        {
            if (manifest == null)
            {
                throw new ArgumentNullException(nameof(manifest));
            }

            if (names == null)
            {
                throw new ArgumentNullException(nameof(names));
            }

            var collection = new Dictionary<string, XPathInformation>();
            this.pathCollection = collection;
            var collectionElement = manifest.SelectSingleNode(names.PathElementName) as XmlElement;
            if (collectionElement == null)
            {
                this.aliasIndicator = DefaultAliasIndicator;
                this.parameterPlaceholder = DefaultParameterPlaceholder;
            }
            else
            {
                this.aliasIndicator = GetAttribute(
                    collectionElement, 
                    names.PathAliasIndicatorAttribute, 
                    DefaultAliasIndicator);
                this.parameterPlaceholder = GetAttribute(
                    collectionElement, 
                    names.PathParameterPlaceHolderAttribute, 
                    DefaultParameterPlaceholder);
                foreach (XmlElement pathElement in collectionElement.SelectNodes(names.PathAddElementName))
                {
                    if (pathElement != null)
                    {
                        var pathInformation = new XPathInformation(pathElement, names, this.parameterPlaceholder);
                        if (collection.ContainsKey(pathInformation.Name))
                        {
                            var message = string.Format(
                                CultureInfo.InvariantCulture,
                                "The alias \"{0}\" has been used. XML:{1}",
                                pathInformation.Name,
                                pathElement.OuterXml);
                            throw new ArgumentException(message);
                        }

                        collection.Add(pathInformation.Name, pathInformation);
                    }
                }
            }
        }

        /// <summary>
        /// Get XPath
        /// </summary>
        /// <param name="path">the XPath string</param>
        /// <param name="parameter">XPath parameter</param>
        /// <returns>final XPath</returns>
        /// <remarks>
        /// if path starts with alias indicator, it will get the actual XPath from path collection.
        /// if path contains parameter, it will replace parameter placeholder by the parameter value
        /// </remarks>
        public string GetPath(string path, string parameter)
        {
            if (string.IsNullOrEmpty(path))
            {
                var message = $"{nameof(path)} should not be empty or blank";
                throw new ArgumentException(message.ToString(CultureInfo.InvariantCulture));
            }

            var finalPath = path;
            bool hasParameterPlaceholder = true;
            if (path.StartsWith(this.aliasIndicator, false, CultureInfo.InvariantCulture))
            {
                var alias = path.Substring(this.aliasIndicator.Length);
                XPathInformation pathInformation;
                if (!this.pathCollection.TryGetValue(alias, out pathInformation))
                {
                    var message = string.Format(
                        CultureInfo.InvariantCulture,
                        "\"{0}\" is path alias. However it was not defined in path collection",
                        path);
                    throw new ArgumentException(message);
                }

                finalPath = pathInformation.Path;
                hasParameterPlaceholder = pathInformation.HasParameterPlaceholder;
            }

            if (hasParameterPlaceholder)
            {
                finalPath = finalPath.Replace(this.parameterPlaceholder, parameter);
            }
            else if (!string.IsNullOrEmpty(parameter))
            {
                var message = string.Format(
                    CultureInfo.InvariantCulture,
                    "XPath does not accept parameter but parameter is provided. Parameter:\"{1}\". XPath:{1}",
                    parameter,
                    finalPath);
                throw new ArgumentException(message);
            }

            return finalPath;
        }
    }
}
