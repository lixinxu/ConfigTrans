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
        protected const string DefaultAliasIndicator = "#";

        /// <summary>
        /// Default path parameter placeholder
        /// </summary>
        protected const string DefaultParameterPlaceholder = "{parameter}";

        /// <summary>
        /// Initializes a new instance of the <see cref="XPathCollection" /> class.
        /// </summary>
        /// <param name="pathRootElement">path collection XML root element</param>
        /// <param name="names">XML names</param>
        public XPathCollection(XmlElement pathRootElement, XmlNames names)
        {
            if (names == null)
            {
                throw new ArgumentNullException(nameof(names));
            }

            var collection = new Dictionary<string, XPathInformation>();
            this.PathCollection = collection;
            if (pathRootElement == null)
            {
                this.AliasIndicator = null;
                this.ParameterPlaceholder = null;
            }
            else
            {
                this.AliasIndicator = GetAttribute(
                    pathRootElement,
                    names.PathAliasIndicatorAttribute,
                    DefaultAliasIndicator);
                this.ParameterPlaceholder = GetAttribute(
                    pathRootElement,
                    names.PathParameterPlaceholderAttribute,
                    DefaultParameterPlaceholder);
                foreach (XmlElement pathElement in pathRootElement.SelectNodes(names.PathAddElementName))
                {
                    if (pathElement != null)
                    {
                        var pathInformation = new XPathInformation(pathElement, names, this.ParameterPlaceholder);
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
        /// Gets alias indicator
        /// </summary>
        protected string AliasIndicator { get; private set; }

        /// <summary>
        /// Gets parameter placeholder
        /// </summary>
        protected string ParameterPlaceholder { get; private set; }

        /// <summary>
        /// Gets path collection
        /// </summary>
        protected IReadOnlyDictionary<string, XPathInformation> PathCollection { get; private set; }

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
            bool hasParameterPlaceholder = this.ParameterPlaceholder != null;
            if ((this.AliasIndicator != null) && path.StartsWith(this.AliasIndicator, false, CultureInfo.InvariantCulture))
            {
                var alias = path.Substring(this.AliasIndicator.Length);
                XPathInformation pathInformation;
                if (!this.PathCollection.TryGetValue(alias, out pathInformation))
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
                finalPath = finalPath.Replace(this.ParameterPlaceholder, parameter);
            }
            else if (!string.IsNullOrEmpty(parameter))
            {
                var message = string.Format(
                    CultureInfo.InvariantCulture,
                    "XPath does not accept parameter but parameter is provided. Parameter:\"{1}\". XPath:{1}",
                    parameter,
                    path);
                throw new ArgumentException(message);
            }

            return finalPath;
        }
    }
}
