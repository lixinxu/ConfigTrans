//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="XmlNames.cs" company="LiXinXu">
//     Copyright LiXinXu.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------------------------------------------------------------------

namespace ConfigurationTransformation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;
    using static Utility;

    public class ProcessorBase
    {
        private static readonly XmlNames names = new XmlNames();

        protected void Process(XmlElement masterXml, XmlElement manifest, string outputLocation = null, object state = null)
        {
            if (masterXml == null)
            {
                throw new ArgumentNullException(nameof(masterXml));
            }

            if (manifest == null)
            {
                throw new ArgumentNullException(nameof(manifest));
            }

            var outputFormat = GetRequiredAttribute(manifest, names.OutputFormatAttribute).Trim();
            var pathCollection = new XPathCollection(manifest.SelectSingleNode(names.PathElementName) as XmlElement, names);
            //var sectionsToProcess = GetSections(diffConfig);
        }

        #region helper
        #endregion helper
    }
}
