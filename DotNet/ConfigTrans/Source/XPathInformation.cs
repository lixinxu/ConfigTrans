//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="XPathInformation.cs" company="LiXinXu">
//     Copyright LiXinXu.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------------------------------------------------------------------

namespace ConfigurationTransformation
{
    using System;
    using System.Globalization;
    using System.Xml;
    using static Utility;

    /// <summary>
    /// XPath information
    /// </summary>
    public class XPathInformation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XPathInformation" /> class.
        /// </summary>
        /// <param name="pathElement">path collection XML element</param>
        /// <param name="names">XML names</param>
        /// <param name="parameterPlaceholder">path parameter placeholder</param>
        public XPathInformation(XmlElement pathElement, XmlNames names, string parameterPlaceholder)
        {
            if (pathElement == null)
            {
                throw new ArgumentNullException(nameof(pathElement));
            }

            if (names == null)
            {
                throw new ArgumentNullException(nameof(names));
            }

            if (string.IsNullOrEmpty(parameterPlaceholder))
            {
                var message = $"{nameof(parameterPlaceholder)} is required. It should not be null or blank."
                    .ToString(CultureInfo.InvariantCulture);
                throw new ArgumentException(message);
            }

            const string MessageFormat = "{0} element requires \"{1}\" attribute. These attribute value should not be empty or blank. XML:{2}";
            var name = GetAttribute(pathElement, names.PathNameAttribute);
            if (name == null)
            {
                var message = string.Format(
                    CultureInfo.InvariantCulture,
                    MessageFormat,
                    names.PathElementName,
                    names.PathNameAttribute,
                    pathElement.OuterXml);
                throw new ArgumentException(message);
            }

            var path = GetAttribute(pathElement, names.PathValueAttribute);
            if (path == null)
            {
                var message = string.Format(
                    CultureInfo.InvariantCulture,
                    MessageFormat,
                    names.PathElementName,
                    names.PathValueAttribute,
                    pathElement.OuterXml);
                throw new ArgumentException(message);
            }

            this.Name = name;
            this.Path = path;
            this.HasParameterPlaceholder = path.Contains(parameterPlaceholder);
        }

        /// <summary>
        /// Gets path alias
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets path 
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Gets a value indicating whether path contains parameter placeholder
        /// </summary>
        public bool HasParameterPlaceholder { get; private set; }
    }
}
