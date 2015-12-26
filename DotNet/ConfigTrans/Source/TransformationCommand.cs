//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="TransformationCommand.cs" company="LiXinXu">
//     Copyright LiXinXu.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------------------------------------------------------------------

namespace ConfigurationTransformation
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Xml;
    using static Utility;

    /// <summary>
    /// Transformation command
    /// </summary>
    public class TransformationCommand
    {
        /// <summary>
        /// transformation command type
        /// </summary>
        private Action<XmlElement> transformationAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformationCommand" /> class.
        /// </summary>
        /// <param name="commandXml">command XML element</param>
        /// <param name="names">XML names</param>
        /// <param name="pathCollection">predefined XPaths</param>
        public TransformationCommand(XmlElement commandXml, XmlNames names, XPathCollection pathCollection)
        {
            if (commandXml == null)
            {
                throw new ArgumentNullException(nameof(commandXml));
            }

            if (names == null)
            {
                throw new ArgumentNullException(nameof(names));
            }

            if (pathCollection == null)
            {
                throw new ArgumentNullException(nameof(pathCollection));
            }

            var path = GetAttribute(commandXml, names.TransformPathAttribute);
            if (string.IsNullOrEmpty(path))
            {
                var message = $"Attribute '{names.TransformPathAttribute}' is required and it should not be empty. XML:{commandXml.OuterXml}";
                throw new ArgumentException(message.ToString(CultureInfo.InvariantCulture));
            }

            var parameter = GetAttribute(commandXml, names.TransformParameterAttribute);
            try
            {
                path = pathCollection.GetPath(path, parameter);
            }
            catch (ArgumentException exception)
            {
                var message = $"Failed to get path. Error:'{exception.Message}'. XML:{commandXml.OuterXml}";
                throw new ArgumentException(message.ToString(CultureInfo.InvariantCulture), exception);
            }

            var name = GetAttribute(commandXml, names.TransformNameAttribute);

            var value = GetAttribute(commandXml, names.TransformValueAttribute);
            if ((value == null) && commandXml.HasChildNodes)
            {
                value = commandXml.InnerText;
            }

            if (commandXml.Name == names.TransformAddElement)
            {
                this.transformationAction = xml => this.Add(xml, path, name, value);
            }
            else if (commandXml.Name == names.TransformRemoveElement)
            {
                this.transformationAction = xml => this.Remove(xml, path);
            }
            else if (commandXml.Name == names.TransformUpdateElement)
            {
                this.transformationAction = xml => this.Update(xml, path, value);
            }
            else
            {
                var message = $"'{commandXml.Name}' is not a supported transformation command. XML: {commandXml.OuterXml}";
                throw new ArgumentException(message.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Transform XML
        /// </summary>
        /// <param name="xml">XML to transform</param>
        public void Transform(XmlElement xml)
        {
            this.transformationAction(xml);
        }

        /// <summary>
        /// Add attribute or element
        /// </summary>
        /// <param name="xml">root XML</param>
        /// <param name="path">XPath value</param>
        /// <param name="name">name for attribute</param>
        /// <param name="value">value for attribute or element</param>
        private void Add(XmlElement xml, string path, string name, string value)
        {
            var nodes = xml.SelectNodes(path);
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    if (node.NodeType != XmlNodeType.Element)
                    {
                        var message = $"Only element can add attribute or child. XPath:{path}, XML:{xml.OuterXml}";
                        throw new ArgumentException(message.ToString(CultureInfo.InvariantCulture));
                    }

                    var element = node as XmlElement;
                    if (string.IsNullOrEmpty(name))
                    {
                        element.InnerXml += value;
                    }
                    else
                    {
                        var attribute = xml.OwnerDocument.CreateAttribute(name);
                        attribute.Value = value;
                        element.Attributes.Append(attribute);
                    }
                }
            }
        }

        /// <summary>
        /// Remove attribute or element
        /// </summary>
        /// <param name="xml">root XML</param>
        /// <param name="path">XPath to find the node to remove</param>
        private void Remove(XmlElement xml, string path)
        {
            var nodes = xml.SelectNodes(path);
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    if (node.NodeType == XmlNodeType.Element)
                    {
                        node.ParentNode.RemoveChild(node);
                    }
                    else
                    {
                        var attribute = node as XmlAttribute;
                        var element = attribute.OwnerElement;
                        element.Attributes.Remove(attribute);
                    }
                }
            }
        }

        /// <summary>
        /// Update attribute or element
        /// </summary>
        /// <param name="xml">root XML</param>
        /// <param name="path">XPath to find the root</param>
        /// <param name="value">value to replace</param>
        private void Update(XmlElement xml, string path, string value)
        {
            var nodes = xml.SelectNodes(path);
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    if (node.NodeType == XmlNodeType.Element)
                    {
                        var builder = new StringBuilder();
                        var referenceXml = builder.Append("<root>").Append(value).Append("</root>").ToString();
                        var referenceDoc = new XmlDocument();
                        referenceDoc.LoadXml(builder.ToString());

                        var parent = node.ParentNode;
                        var referenceRoot = referenceDoc.DocumentElement;
                        var referenceChildren = referenceRoot.ChildNodes;
                        for (var i = 0; i < referenceChildren.Count; i++)
                        {
                            var referenceChild = referenceChildren[i];
                            XmlNode newNode;
                            switch (referenceChild.NodeType)
                            {
                                case XmlNodeType.Text:
                                    newNode = node.OwnerDocument.CreateTextNode(referenceChild.InnerText);
                                    break;
                                case XmlNodeType.CDATA:
                                    newNode = node.OwnerDocument.CreateCDataSection(referenceChild.InnerText);
                                    break;
                                case XmlNodeType.Element:
                                    {
                                        var referenceElement = referenceChild as XmlElement;
                                        var newElement = node.OwnerDocument.CreateElement(referenceElement.Name);
                                        newNode = newElement;
                                        if (referenceElement.HasAttributes)
                                        {
                                            foreach (XmlAttribute referenceAttribute in referenceElement.Attributes)
                                            {
                                                var newAttribute = node.OwnerDocument.CreateAttribute(referenceAttribute.Name);
                                                newAttribute.Value = referenceAttribute.Value;
                                                newElement.Attributes.Append(newAttribute);
                                            }
                                        }

                                        if (referenceElement.HasChildNodes)
                                        {
                                            newElement.InnerXml = referenceElement.InnerXml;
                                        }
                                    }

                                    break;
                                default:
                                    newNode = null;
                                    break;
                            }

                            if (newNode != null)
                            {
                                parent.InsertAfter(newNode, node);
                            }
                        }

                        parent.RemoveChild(node);
                    }
                    else if (node.NodeType == XmlNodeType.Attribute)
                    {
                        var attribute = node as XmlAttribute;
                        attribute.Value = value;
                    }
                }
            }
        }
    }
}
