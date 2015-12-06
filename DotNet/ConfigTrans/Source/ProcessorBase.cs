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

    public class ProcessorBase
    {
        private static readonly XmlNames names = new XmlNames();

        protected void Process(XmlElement masterXml, XmlElement manifest, string outputLocation, object state = null)
        {
            if (masterXml == null)
            {
                throw new ArgumentNullException(nameof(masterXml));
            }

            if (manifest == null)
            {
                throw new ArgumentNullException(nameof(manifest));
            }
        }

        #region helper
        #endregion helper
    }
}
