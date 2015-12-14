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

            var name = GetAttribute(pathElement, names.PathNameAttribute);
            if (name == null)
            {
                var message = $"{names.PathElementName} element requires '{names.PathNameAttribute}' attribute. These attribute value should not be empty or blank. XML:{pathElement.OuterXml}";
                throw new ArgumentException(message.ToString(CultureInfo.InvariantCulture));
            }

            var path = GetAttribute(pathElement, names.PathValueAttribute);
            if (path == null)
            {
                var message = $"{names.PathElementName} element requires '{names.PathValueAttribute}' attribute. These attribute value should not be empty or blank. XML:{pathElement.OuterXml}";
                throw new ArgumentException(message.ToString(CultureInfo.InvariantCulture));
            }

            this.Name = name;
            this.Path = path;
            this.HasParameterPlaceholder = path.Contains(parameterPlaceholder);
        }

        /// <summary>
        /// Gets  path alias
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
