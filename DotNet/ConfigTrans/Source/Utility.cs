//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="Utility.cs" company="LiXinXu">
//     Copyright LiXinXu.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------------------------------------------------------------------

namespace ConfigurationTransformation
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using System.Globalization;

    /// <summary>
    /// Shared utility
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Get XML attribute. If attribute does not exist or is empty, default value returned.
        /// </summary>
        /// <param name="xml">XML element</param>
        /// <param name="attributeName">attribute name</param>
        /// <param name="defaultValue">default value</param>
        /// <returns>attribute or default value</returns>
        public static string GetAttribute(XmlElement xml, string attributeName, string defaultValue = null)
        {
            if (xml == null)
            {
                throw new ArgumentNullException(nameof(xml));
            }
            if (string.IsNullOrWhiteSpace(attributeName))
            {
                var message = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} should not be null or blank",
                    nameof(attributeName));
                throw new ArgumentException(message);
            }
            var attribute = xml.GetAttribute(attributeName);
            return string.IsNullOrEmpty(attribute) ? defaultValue : attribute;
        }

        /// <summary>
        /// Get required XML attribute
        /// </summary>
        /// <param name="xml">XML element</param>
        /// <param name="attributeName">name of the attribute</param>
        /// <returns>attribute value</returns>
        /// <remarks>
        /// if attribute does not exist, or value is null or blank, argument exception will be thrown.
        /// </remarks>
        public static string GetRequiredAttribute(XmlElement xml, string attributeName)
        {
            var value = GetAttribute(xml, attributeName);
            if (value == null)
            {
                var message = string.Format(
                    CultureInfo.InvariantCulture,
                    "Attribute \"{0}\" is required and should not be empty or blank. XML:{1}",
                    attributeName,
                    xml.OuterXml);
                throw new ArgumentException(message);
            }
            return value;
        }

        /// <summary>
        /// Compare two XML and return true if they are same
        /// </summary>
        /// <remarks>
        /// white-spaces and comments are ignored. order of attribute does not matter. 
        /// XML name-space is not considered yet. Will update later.
        /// </remarks>
        /// <param name="first">first XML</param>
        /// <param name="second">second XML</param>
        /// <returns>true if save. False if not</returns>
        public static bool XmlAreSame(XmlElement first, XmlElement second)
        {
            if (first.HasAttributes)
            {
                if (!second.HasAttributes || (first.Attributes.Count != second.Attributes.Count))
                {
                    return false;
                }

                for (var idx = 0; idx < first.Attributes.Count; idx++)
                {
                    var firstAttribute = first.Attributes[idx];
                    var secondAttribute = second.Attributes[firstAttribute.Name];
                    if ((secondAttribute == null) || (firstAttribute.Value != secondAttribute.Value))
                    {
                        return false;
                    }
                }
            }
            else if (second.HasAttributes)
            {
                return false;
            }

            var firstNodeList = GetNodes(first);
            var secondNodeList = GetNodes(second);

            if (firstNodeList != null)
            {
                if ((secondNodeList == null) || (firstNodeList.Count != secondNodeList.Count))
                {
                    return false;
                }

                for (var i = 0; i < firstNodeList.Count; i++)
                {
                    var firstNode = firstNodeList[i];
                    var secondNode = secondNodeList[i];
                    if (firstNode.NodeType != secondNode.NodeType)
                    {
                        return false;
                    }

                    if (firstNode.NodeType == XmlNodeType.Element)
                    {
                        var firstElement = firstNode as XmlElement;
                        var secondElement = secondNode as XmlElement;

                        if ((firstElement.Name != secondElement.Name) || !XmlAreSame(firstElement, secondElement))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (firstNode.Value != secondNode.Value)
                        {
                            return false;
                        }
                    }
                }
            }
            else if (secondNodeList != null)
            {
                return false;
            }

            return true;
        }

        #region helper
        /// <summary>
        /// Get XML children
        /// </summary>
        /// <param name="xml">root XML</param>
        /// <returns>child list</returns>
        private static IReadOnlyList<XmlNode> GetNodes(XmlElement xml)
        {
            List<XmlNode> list = null;
            if (xml.HasChildNodes)
            {
                var children = xml.ChildNodes;
                for (var i = 0; i < children.Count; i++)
                {
                    var node = children[i];
                    switch (node.NodeType)
                    {
                        case XmlNodeType.Element:
                        case XmlNodeType.CDATA:
                        case XmlNodeType.Text:
                            if (list == null)
                            {
                                list = new List<XmlNode>();
                            }

                            list.Add(node);
                            break;
                    }
                }
            }

            return list;
        }
        #endregion helper
    }
}
