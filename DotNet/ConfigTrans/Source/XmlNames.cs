//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="XmlNames.cs" company="LiXinXu">
//     Copyright LiXinXu.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------------------------------------------------------------------

namespace ConfigurationTransformation
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Reflection;

    /// <summary>
    /// XML element/attribute names
    /// </summary>
    /// <remarks>
    /// <para>This names are used by processor when loading manifest XML. Users can customize the names by changing configuration. For example, 
    /// to change OutputFormatAttribute, use can add/update entry in app.Config:</para>
    /// <![CDATA[
    /// <configuration>
    ///   <appSettings>
    ///     ...
    ///     <add key = "OutputFormatAttribute" value="fileFormat"/>
    ///     ...
    ///   </appSettings>
    ///   ...
    /// </configuration>
    /// ]]>
    /// <para>In the manifest XML file, user can use "fileFormat" instead of the default "outputFormat":</para>
    /// <![CDATA[
    /// <manifest fileFormat="web.{env}-{region}.config" parameterPlacehold="{parameter}">
    /// </manifest>
    /// ]]>
    /// </remarks>
    public class XmlNames
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlNames" /> class.
        /// </summary>
        /// <remarks>
        /// It loads value from application configuration
        /// </remarks>
        public XmlNames() : this(ConfigurationManager.AppSettings)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlNames" /> class.
        /// </summary>
        /// <param name="configurations">It load values from given configuration collection</param>
        /// <remarks>
        /// This will be used by unit test
        /// </remarks>
        public XmlNames(NameValueCollection configurations)
        {
            var propertyInformationCollection = this.GetType().GetProperties();
            foreach (var propertyInformation in propertyInformationCollection)
            {
                var attribute = propertyInformation.GetCustomAttribute<ConfigurationItemAttribute>(true);
                if (attribute != null)
                {
                    string value = null;
                    if (configurations != null)
                    {
                        value = configurations[propertyInformation.Name];
                    }

                    if (value == null)
                    {
                        value = attribute.DefaultValue;
                    }

                    propertyInformation.SetValue(this, value);
                }
            }
        }

        /// <summary>
        /// Gets the output file format/pattern
        /// </summary>
        /// <remarks>
        /// Finial output name depends on sections. People can specify how the output name looks like.
        /// </remarks>
        /// <example>
        /// For example, you have sections below:
        /// <![CDATA[
        /// <sections scope="environment">
        ///     <section name="Production">
        ///         ...
        ///         <sections scope="region">
        ///             <section name="Europe">
        ///                 ...
        ///             </section>
        ///         </sections>
        ///     </section>
        /// </sections>
        /// ]]>
        /// <para>The section finial section indicates following scope:{"environment" : "production", "region" : "Europe"}</para>
        /// <para>If we defined the output format in the manifest as below:</para>
        /// <![CDATA[
        /// <manifest outputFormat="web.{environment}-{region}.config" ...">
        ///     ...
        /// </manifest>
        /// ]]>
        /// <para>The output name will be "web.Production-Europe.config"</para>
        /// </example>
        [ConfigurationItem("outputFormat")]
        public string OutputFormatAttribute { get; private set; }

        #region Predefined XPath collection
        /// <summary>
        /// Gets the XPath dictionary element name
        /// </summary>
        /// <remarks>
        /// <para>To avoid create too many XPath in manifest, people can create alias here.</para>
        /// </remarks>
        /// <example>
        /// <![CDATA[
        /// <manifest outputFormat = "web.{env}-{region}.config">
        ///   <path>
        ///     ...
        ///     <add name="AppSettingAttr" path="//configuration/appSettings/add[@key='{parameter}']/@value" />
        ///     ...
        ///   </path>
        ///   <sections scope = "env" >
        ///     <transform>
        ///       <update path="#AppSettingAttr" param="item1" value="item1-global" />
        ///       <update path="/configuration/connectionStrings/add[@name='database']/@value" value="my connection string" />
        ///     </transform>
        ///     ...
        ///   </sections>
        ///   ...
        /// </manifest> 
        /// ]]>
        /// <para>In "transform" section, the first one uses alias. The second uses XPath.</para>
        /// </example>
        [ConfigurationItem("path")]
        public string PathElementName { get; private set; }

        /// <summary>
        /// Gets the name of XPath alias indicator attribute. The indicator tells the processor a string is not a XPath but an alias
        /// </summary>
        /// <remarks>
        /// <para>It gives an flexibility to use different indicator based on users preference. It also can void potential problem if the 
        /// indicator used by future version XPath.</para>
        /// </remarks>
        /// <example>
        /// <![CDATA[
        /// <manifest outputFormat = "web.{env}-{region}.config">
        ///   <path indicator="$$"">
        ///     ...
        ///     <add name = "AppSettingAttr" path="//configuration/appSettings/add[@key='{parameter}']/@value" />
        ///     ...
        ///   </path>
        ///   <sections scope = "env" >
        ///     < transform >
        ///       <update path="$$AppSettingAttr" param="item1" value="item1-global" />
        ///     </transform>
        ///     ...
        ///   </sections>
        ///   ...
        /// </manifest> 
        /// ]]>
        /// </example>
        [ConfigurationItem("indicator")]
        public string PathAliasIndicatorAttribute { get; private set; }

        /// <summary>
        /// Gets the name of XPath parameter attribute. 
        /// </summary>
        /// <remarks>
        /// <para>It will help the process to find the parameter placeholder so it can replace it by actual value.</para>
        /// <para>Using alias is not good enough. For example, if you need to find a item in AppSettings you need to provide key: 
        /// //configuration/appSettings/add[@key='MyKey']/value
        /// the key is very dynamic so you cannot predefine it unless you use the same key in multiple places. The reduce the complexity,
        /// people can use XPath "with parameter placeholder". The predefined placeholder may be used in some scenarios so we have to use
        ///  alternative one. By adding/changed the attribute, people can use their own placeholder rather than the predefined one.</para>
        /// </remarks>
        /// <example>
        /// <![CDATA[
        /// <manifest outputFormat = "web.{env}-{region}.config">
        ///   <path parameter="argument">
        ///     ...
        ///     <add name="AppSettingAttr" path="//configuration/appSettings/add[@key='{argument}']/@value" />
        ///     ...
        ///   </path>
        ///   <sections scope = "env" >
        ///     <transform>
        ///       <update path="#AppSettingAttr" param="item1" value="item1-global" />
        ///       <update path="/configuration/connectionStrings/add[@name='database']/@value" value="my connection string" />
        ///     </transform>
        ///     ...
        ///   </sections>
        ///   ...
        /// </manifest> 
        /// ]]>
        /// <para>The sample above shows how to use "argument" as XPath parameter</para>
        /// </example>
        [ConfigurationItem("parameter")]
        public string PathParameterPlaceholderAttribute { get; private set; }

        /// <summary>
        /// Gets the attribute name of XPath alias
        /// </summary>
        [ConfigurationItem("name")]
        public string PathNameAttribute { get; private set; }

        /// <summary>
        /// Gets the name of attribute which saves XPath value
        /// </summary>
        [ConfigurationItem("path")]
        public string PathValueAttribute { get; private set; }

        /// <summary>
        /// Gets the element name of adding a new alias
        /// </summary>
        [ConfigurationItem("add")]
        public string PathAddElementName { get; private set; }

        /// <summary>
        /// Gets namespace element name
        /// </summary>
        /// <example>
        /// <![CDATA[
        /// <transfor fileFormat="web.{env}-{region}.config" parameterPlacehold="{parameter}">
        ///   <path>
        ///     <namespace prefix="x">http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration</namespace>
        ///     <namespace prefix="y">http://schemas.microsoft.com/ServiceHosting/2008/11/ServiceConfiguration</namespace>
        ///     <add name = "SharedCertificateThumbprint" path="..." />
        ///     ...
        ///   </path>
        ///   ...
        /// </transfor>
        /// ]]>
        /// </example>
        [ConfigurationItem("namespace")]
        public string PathNamespaceElementName { get; private set; }

        /// <summary>
        /// Gets namespace prefix attribute name
        /// </summary>
        [ConfigurationItem("prefix")]
        public string PathNamespacePrefixAttributeName { get; private set; }
        #endregion Predefined XPath collection

        #region Transformation command
        /// <summary>
        /// Gets the transformation collection element name
        /// </summary>
        /// <remarks>
        /// Transformation element can be used in "sections" and "section".
        /// </remarks>
        /// <example>
        /// <![CDATA[
        /// <manifest outputFormat="web.{env}-{region}.config">
        ///   ...
        ///   <sections scope="env" >
        ///     <transform>
        ///       ...
        ///     </transform>
        ///     <section name="dev">
        ///       <transform>
        ///         ...
        ///       </transform>
        ///     </section>
        ///   </sections>
        ///   ...
        /// </manifest> 
        /// ]]>
        /// </example>
        [ConfigurationItem("transform")]
        public string TransformElementName { get; private set; }

        /// <summary>
        /// Gets element name of transformation updating operation
        /// </summary>
        [ConfigurationItem("update")]
        public string TransformUpdateElement { get; private set; }

        /// <summary>
        /// Gets element name of transformation removing operation
        /// </summary>
        [ConfigurationItem("remove")]
        public string TransformRemoveElement { get; private set; }

        /// <summary>
        /// Gets element name of transformation adding operation
        /// </summary>
        [ConfigurationItem("add")]
        public string TransformAddElement { get; private set; }

        /// <summary>
        /// Gets attribute name of transformation XPath. The attribute is required.
        /// </summary>
        /// <remarks>
        /// If the value of path starts with the alias indicator, then it is a alias.
        /// </remarks>
        [ConfigurationItem("path")]
        public string TransformPathAttribute { get; private set; }

        /// <summary>
        /// Gets the name of the attribute which stores XPath parameter. it is optional.
        /// </summary>
        [ConfigurationItem("param")]
        public string TransformParameterAttribute { get; private set; }

        /// <summary>
        /// Gets the name of the attribute which used to create new attribute.
        /// </summary>
        [ConfigurationItem("name")]
        public string TransformNameAttribute { get; private set; }

        /// <summary>
        /// Gets the attribute name of value. it is used for update or add. If this value is not specified, the element value may be used.
        /// </summary>
        [ConfigurationItem("value")]
        public string TransformValueAttribute { get; private set; }
        #endregion Transformation command

        /// <summary>
        /// Gets the name of sections element
        /// </summary>
        [ConfigurationItem("sections")]
        public string SectionsElementName { get; private set; }

        /// <summary>
        /// Gets the name of scope attribute
        /// </summary>
        [ConfigurationItem("name")]
        public string SectionsScopeAttributeName { get; private set; }

        /// <summary>
        /// Gets the name of section element
        /// </summary>
        [ConfigurationItem("section")]
        public string SectionElementName { get; private set; }

        /// <summary>
        /// Gets the name of section name attribute
        /// </summary>
        [ConfigurationItem("name")]
        public string SectionNameAttribute { get; private set; }

        /// <summary>
        /// Attribute to describe property
        /// </summary>
        /// <remarks>
        /// It will help the XML names class to load configuration by reflection. It also cover default value if configuration entry was
        /// not created in the configuration file.
        /// </remarks>
        [AttributeUsage(AttributeTargets.Property)]
        public class ConfigurationItemAttribute : Attribute
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ConfigurationItemAttribute" /> class.
            /// </summary>
            /// <param name="defaultValue">default value</param>
            public ConfigurationItemAttribute(string defaultValue)
            {
                this.DefaultValue = defaultValue;
            }

            /// <summary>
            /// Gets property default value
            /// </summary>
            public string DefaultValue { get; private set; }
        }
    }
}
