//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="TestUtility.cs" company="LiXinXu">
//     Copyright LiXinXu.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------------------------------------------------------------------

namespace ConfigurationTransformation.UnitTest
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml;

    /// <summary>
    /// Utility for unit test
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class TestUtility
    {
        /// <summary>
        /// Load XML from assembly
        /// </summary>
        /// <param name="name">file name</param>
        /// <param name="folder">folder related to type location</param>
        /// <param name="type">type which at same location of the file. Use utility type of type is not specified</param>
        /// <returns>XML from assembly</returns>
        public static XmlElement LoadXml(string name, string folder = null, Type type=null)
        {
            if (type == null)
            {
                type = typeof(TestUtility);
            }
            XmlElement xml;
            using (var stream = type.Assembly.GetManifestResourceStream(type, folder + name))
            {
                var doc = new XmlDocument();
                doc.Load(stream);
                xml = doc.DocumentElement;
            }
            return xml;
        }
    }
}
