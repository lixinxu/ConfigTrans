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
        /// Gets container name
        /// </summary>
        protected readonly string Name;

        /// <summary>
        /// Parent collection
        /// </summary>
        protected readonly TransformationCommandCollection Host;

        /// <summary>
        /// Transformation commends
        /// </summary>
        protected readonly IReadOnlyList<TransformationCommand> Commands;

        /// <summary>
        /// scope collection. for example: {"environment"="production", region="west us"}
        /// </summary>
        protected readonly IReadOnlyDictionary<string, string> Scopes;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformationCommandCollection" /> class.
        /// </summary>
        /// <param name="containerXml">root XML element which contains the container information</param>
        /// <param name="host">parent host</param>
        /// <param name="nameAttribute">name of the attribute which can get container name from</param>
        /// <param name="pathCollection">XPath collection</param>
        /// <param name="names">XML names</param>
        /// <param name="isScopeXml">is XML for scope</param>
        public TransformationCommandCollection(
            XmlElement containerXml, 
            TransformationCommandCollection host, 
            string nameAttribute, 
            XPathCollection pathCollection, 
            XmlNames names,
            bool isScopeXml = true)
        {
            if (containerXml == null)
            {
                throw new ArgumentNullException(nameof(containerXml));
            }

            if (string.IsNullOrEmpty(nameAttribute))
            {
                var message = $"parameter {nameof(nameAttribute)} is required and should not be blank.";
                throw new ArgumentException(message.ToString(CultureInfo.InvariantCulture));
            }

            var name = GetAttribute(containerXml, nameAttribute);
            if (string.IsNullOrEmpty(name))
            {
                var message = $"Attribute '{nameAttribute}' in XML is required and should not be empty. XML: {containerXml.OuterXml}";
                throw new ArgumentException(message.ToString(CultureInfo.InvariantCulture));
            }

            this.Scopes = null;
            if (!isScopeXml)
            {
                if (host == null)
                {
                    var message = $"If XML is not scope XML, host should be provided because it needs scope name. XML:{containerXml.OuterXml}";
                    throw new ArgumentException(message.ToString(CultureInfo.InvariantCulture));
                }

                var currentScopes = new Dictionary<string, string>();
                if (host.Host != null)
                {
                    foreach (var pair in host.Host.Scopes)
                    {
                        currentScopes.Add(pair.Key, pair.Value);
                    }
                }

                currentScopes.Add(host.Name, name);
                this.Scopes = currentScopes;
            }

            this.Name = name;
            this.Host = host;

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

            this.Commands = commandList;
        }

        /// <summary>
        /// Transform XML
        /// </summary>
        /// <param name="xml">XML to transform</param>
        /// <param name="namespaceManager">XML namespace manager</param>
        /// <returns>scope collection</returns>
        public IReadOnlyDictionary<string, string> Transform(XmlElement xml, XmlNamespaceManager namespaceManager)
        {
            if (this.Host != null)
            {
                this.Host.Transform(xml, namespaceManager);
            }

            foreach (var commnand in this.Commands)
            {
                commnand.Transform(xml, namespaceManager);
            }

            return this.Scopes;
        }
    }
}
