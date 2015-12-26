//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="Transformer.cs" company="LiXinXu">
//     Copyright LiXinXu.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------------------------------------------------------------------

namespace ConfigurationTransformation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;
    using System.Xml;
    using static Utility;

    /// <summary>
    /// configuration transformer
    /// </summary>
    public class Transformer
    {
        /// <summary>
        /// output format
        /// </summary>
        private readonly string outputFormat;

        /// <summary>
        /// transformation command collections
        /// </summary>
        private readonly IReadOnlyList<TransformationCommandCollection> transformations;

        /// <summary>
        /// Initializes a new instance of the <see cref="Transformer" /> class.
        /// </summary>
        /// <param name="manifestXml">manifest XML instance</param>
        /// <param name="parallel">parallel processing</param>
        public Transformer(XmlElement manifestXml, bool parallel)
        {
            var names = new XmlNames();

            if (manifestXml == null)
            {
                throw new ArgumentNullException(nameof(manifestXml));
            }

            this.outputFormat = GetAttribute(manifestXml, names.OutputFormatAttribute);

            var pathXml = manifestXml.SelectSingleNode(names.PathElementName) as XmlElement;
            var pathCollection = new XPathCollection(pathXml, names);
            var transformationCollection = new List<TransformationCommandCollection>();

            ProcessSections(null, manifestXml, pathCollection, transformationCollection, names, parallel);
            this.transformations = transformationCollection;
        }

        /// <summary>
        /// perform transformation
        /// </summary>
        /// <param name="xml">XML to be transformed</param>
        /// <param name="saveTransformedXml">action when transformation is done</param>
        /// <param name="parallel">parallel processing</param>
        public void Transform(
            XmlElement xml, 
            Action<string, IReadOnlyDictionary<string, string>, XmlElement> saveTransformedXml, 
            bool parallel)
        {
            if (xml == null)
            {
                throw new ArgumentNullException(nameof(xml));
            }

            if (saveTransformedXml == null)
            {
                throw new ArgumentNullException(nameof(saveTransformedXml));
            }

            if (parallel)
            {
                Parallel.ForEach(
                    this.transformations,
                    transformer =>
                    {
                        this.Transform(transformer, xml, saveTransformedXml);
                    });
            }
            else
            {
                foreach (var transformer in this.transformations)
                {
                    this.Transform(transformer, xml, saveTransformedXml);
                }
            }
        }

        /// <summary>
        /// Load sections from manifest XML
        /// </summary>
        /// <param name="host">parent transformer</param>
        /// <param name="xml">manifest XML instance</param>
        /// <param name="pathCollection">XPath collection</param>
        /// <param name="collection">transformer collection</param>
        /// <param name="names">XML names</param>
        /// <param name="parallel">process in parallel</param>
        private static void ProcessSections(
            TransformationCommandCollection host,
            XmlElement xml,
            XPathCollection pathCollection,
            List<TransformationCommandCollection> collection,
            XmlNames names,
            bool parallel)
        {
            ProcessSections(host, xml.SelectNodes(names.SectionsElementName), pathCollection, collection, names, parallel);
        }

        /// <summary>
        /// Load sections from manifest node list
        /// </summary>
        /// <param name="host">parent transformer</param>
        /// <param name="nodeList">scope section XML node list</param>
        /// <param name="pathCollection">XPath collection</param>
        /// <param name="collection">transformer collection</param>
        /// <param name="names">XML names</param>
        /// <param name="parallel">process in parallel</param>
        private static void ProcessSections(
            TransformationCommandCollection host,
            XmlNodeList nodeList,
            XPathCollection pathCollection,
            List<TransformationCommandCollection> collection,
            XmlNames names,
            bool parallel)
        {
            if (parallel)
            {
                var options = new ParallelOptions();
                Parallel.For(
                    0,
                    nodeList.Count,
                    options,
                    (id, state) =>
                    {
                        var rootSectionXml = nodeList[id] as XmlElement;
                        if (rootSectionXml != null)
                        {
                            ProcessRootSection(host, rootSectionXml, pathCollection, collection, names, true);
                        }
                    });
            }
            else
            {
                foreach (XmlElement rootSectionXml in nodeList)
                {
                    ProcessRootSection(host, rootSectionXml, pathCollection, collection, names, false);
                }
            }
        }

        /// <summary>
        /// Process scope root item
        /// </summary>
        /// <param name="host">parent transformer</param>
        /// <param name="xml">scope section XML instance</param>
        /// <param name="pathCollection">XPath collection</param>
        /// <param name="collection">transformer collection</param>
        /// <param name="names">XML names</param>
        /// <param name="parallel">process in parallel</param>
        private static void ProcessRootSection(
            TransformationCommandCollection host,
            XmlElement xml,
            XPathCollection pathCollection,
            List<TransformationCommandCollection> collection,
            XmlNames names,
            bool parallel)
        {
            var scope = new TransformationCommandCollection(xml, host, names.SectionsScopeAttributeName, pathCollection, names, true);
            var sections = xml.SelectNodes(names.SectionElementName);
            if (sections.Count < 1)
            {
                var message = $"Scope section needs child section(s). XML:{xml.OuterXml}";
                throw new ArgumentException(message.ToString(CultureInfo.InvariantCulture));
            }

            if (parallel)
            {
                Parallel.For(
                    0,
                    sections.Count,
                    id =>
                    {
                        var targetSectionXml = sections[id] as XmlElement;
                        if (targetSectionXml != null)
                        {
                            ProcessChildSection(scope, targetSectionXml, pathCollection, collection, names, true);
                        }
                    });
            }
            else
            {
                foreach (XmlElement targetSectionXml in sections)
                {
                    ProcessChildSection(scope, targetSectionXml, pathCollection, collection, names, false);
                }
            }
        }

        /// <summary>
        /// process child section
        /// </summary>
        /// <param name="host">section parent</param>
        /// <param name="xml">child section XML instance</param>
        /// <param name="pathCollection">XPath collection</param>
        /// <param name="collection">transformer collection</param>
        /// <param name="names">XML names</param>
        /// <param name="parallel">process in parallel</param>
        private static void ProcessChildSection(
            TransformationCommandCollection host,
            XmlElement xml,
            XPathCollection pathCollection,
            List<TransformationCommandCollection> collection,
            XmlNames names,
            bool parallel)
        {
            var target = new TransformationCommandCollection(xml, host, names.SectionNameAttribute, pathCollection, names, false);

            var innerScopes = xml.SelectNodes(names.SectionsElementName);
            if (innerScopes.Count < 1)
            {
                lock (collection)
                {
                    collection.Add(target);
                }
            }
            else
            {
                ProcessSections(target, innerScopes, pathCollection, collection, names, parallel);
            }
        }

        /// <summary>
        /// Copy XML
        /// </summary>
        /// <param name="source">source XML instance</param>
        /// <returns>copied XML instance</returns>
        private static XmlElement CopyXml(XmlElement source)
        {
            var doc = new XmlDocument();
            doc.LoadXml(source.OuterXml);
            return doc.DocumentElement;
        }

        /// <summary>
        /// Perform transformation by given transformer
        /// </summary>
        /// <param name="transformer">transformer which will take action</param>
        /// <param name="xml">XML which will be transformed</param>
        /// <param name="saveTransformedXml">the action to save transformed XML</param>
        private void Transform(
            TransformationCommandCollection transformer,
            XmlElement xml,
            Action<string, IReadOnlyDictionary<string, string>, XmlElement> saveTransformedXml)
        {
            var newXml = CopyXml(xml);
            var scopes = transformer.Transform(newXml);
            saveTransformedXml(this.outputFormat, scopes, newXml);
        }
    }
}
