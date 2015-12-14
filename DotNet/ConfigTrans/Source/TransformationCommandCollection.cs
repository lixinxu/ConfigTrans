//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="TransformationCommandCollection.cs" company="LiXinXu">
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
    /// Transformation command collection
    /// </summary>
    public class TransformationCommandCollection
    {
        /// <summary>
        /// Parent collection
        /// </summary>
        private readonly TransformationCommandCollection host;

        /// <summary>
        /// Transformation commends
        /// </summary>
        private readonly IReadOnlyList<TransformationCommand> commands;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformationCommandCollection" /> class.
        /// </summary>
        /// <param name="containerXml">root XML element which contains the container information</param>
        /// <param name="host">parent host</param>
        /// <param name="nameAttribute">name of the attribute which can get container name from</param>
        /// <param name="pathCollection">XPath collection</param>
        /// <param name="names">XML names</param>
        public TransformationCommandCollection(
            XmlElement containerXml, 
            TransformationCommandCollection host, 
            string nameAttribute, 
            XPathCollection pathCollection, 
            XmlNames names)
        {
            if (containerXml == null)
            {
                throw new ArgumentNullException(nameof(containerXml));
            }

            if (string.IsNullOrEmpty(nameAttribute))
            {
                var message = $"parameter {nameof(nameAttribute)} is required and should not be blank";
                throw new ArgumentException(message.ToString(CultureInfo.InvariantCulture));
            }

            var name = GetAttribute(containerXml, nameAttribute);
            if (string.IsNullOrEmpty(name))
            {
                var message = $"Attribute '{nameAttribute}' in XML is required and should not be empty. XML: {containerXml.OuterXml}";
                throw new ArgumentException(message.ToString(CultureInfo.InvariantCulture));
            }

            this.Name = name;
            this.host = host;

            var commandList = new List<TransformationCommand>();
            foreach (XmlElement commandRootXml in containerXml.SelectNodes(names.TransformElementName))
            {
                foreach (XmlNode childXml in commandRootXml.ChildNodes)
                {
                    if (childXml.NodeType == XmlNodeType.Element)
                    {
                        var command = new TransformationCommand(childXml as XmlElement, names, pathCollection);
                        commandList.Add(command);
                    }
                }
            }

            this.commands = commandList;
        }

        /// <summary>
        /// Gets container name
        /// </summary>
        public string Name { get; private set; }
    }
}
