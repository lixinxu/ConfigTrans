//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationXmlUtility.cs" company="LiXinXu">
//     Copyright LiXinXu.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------------------------------------------------------------------

namespace ConfigurationTransformation.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Xml;

    using static TestUtility;

    /// <summary>
    /// Configuration XML generator utility
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ConfigurationXmlUtility
    {
        /// <summary>
        /// root element name
        /// </summary>
        public const string ConfigRootXmlElementName = "configuration";

        /// <summary>
        /// Application Settings element name
        /// </summary>
        public const string ConfigAppSettingsElementName = "appSettings";

        /// <summary>
        /// configuration item element name
        /// </summary>
        public const string ConfigAddItemElementName = "add";

        /// <summary>
        /// configuration key attribute name
        /// </summary>
        public const string ConfigKeyAttributeName = "key";

        /// <summary>
        /// configuration value attribute name
        /// </summary>
        public const string ConfigValueAttributeName = "value";

        /// <summary>
        /// configuration key prefix
        /// </summary>
        public const string ConfigKeyPrefix = "myKey";

        /// <summary>
        /// configuration value prefix
        /// </summary>
        public const string ConfigValuePrefix = "myValue";

        /// <summary>
        /// Create a configuration XML
        /// </summary>
        /// <param name="settings">configuration settings</param>
        /// <param name="xmlDoc">XML documentation</param>
        /// <returns>configuration XML instance</returns>
        public static XmlElement CreateAppSettingsXml(IDictionary<string, string> settings, XmlDocument xmlDoc = null)
        {
            return CreateAppSettingsXml(
                appSettingsXml =>
                {
                    foreach (var pair in settings)
                    {
                        var itemXml = appSettingsXml.OwnerDocument.CreateElement(ConfigAddItemElementName);
                        AddAttribute(itemXml, ConfigKeyAttributeName, pair.Key);
                        AddAttribute(itemXml, ConfigValueAttributeName, pair.Value);
                    }
                },
                xmlDoc);
        }

        /// <summary>
        /// Get configuration key by position
        /// </summary>
        /// <param name="id">key position</param>
        /// <returns>configuration key name</returns>
        public static string GetKeyById(int id)
        {
            return $"{ConfigKeyPrefix}{id}".ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Get configuration value by position
        /// </summary>
        /// <param name="id">value position</param>
        /// <returns>configuration value</returns>
        public static string GetValueById(int id)
        {
            return $"{ConfigValuePrefix}{id}".ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Create application setting XML
        /// </summary>
        /// <param name="count">number of configurations</param>
        /// <param name="xmlDoc">XML doc instance (optional)</param>
        /// <returns>configuration XML instance</returns>
        public static XmlElement CreateAppSettingsXml(int count, XmlDocument xmlDoc = null)
        {
            return CreateAppSettingsXml(
                appSettingsXml =>
                {
                    for (int i = 0; i < count; i++)
                    {
                        var key = GetKeyById(i);
                        var value = GetValueById(i);
                        var itemXml = appSettingsXml.OwnerDocument.CreateElement(ConfigAddItemElementName);
                        appSettingsXml.AppendChild(itemXml);
                        AddAttribute(itemXml, ConfigKeyAttributeName, key);
                        AddAttribute(itemXml, ConfigValueAttributeName, value);
                    }
                },
                xmlDoc);
        }

        /// <summary>
        /// Create configuration XML with application settings
        /// </summary>
        /// <param name="addItems">delegate on add new configuration items</param>
        /// <param name="xmlDoc">XML doc instance (optional)</param>
        /// <returns>configuration XML instance</returns>
        public static XmlElement CreateAppSettingsXml(Action<XmlElement> addItems, XmlDocument xmlDoc  = null)
        {
            if (xmlDoc == null)
            {
                xmlDoc = new XmlDocument();
            }

            var rootXml = xmlDoc.CreateElement(ConfigRootXmlElementName);
            xmlDoc.AppendChild(rootXml);
            var appSettingsXml = xmlDoc.CreateElement(ConfigAppSettingsElementName);
            rootXml.AppendChild(appSettingsXml);
            addItems(appSettingsXml);
            return rootXml;
        }
    }
}
