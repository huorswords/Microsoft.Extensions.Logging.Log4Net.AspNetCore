namespace Microsoft.Extensions.Logging.Log4Net.AspNetCore.Extensions
{
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;

    /// <summary>
    /// Class with XmlDocument and XDocument extensions.
    /// </summary>
    internal static class DocumentExtensions
    {
        /// <summary>
        /// Converts a XmlDocument object into xDocument.
        /// </summary>
        /// <param name="xmlDocument">The XML document.</param>
        /// <returns>The XmlDocument converted to XDocument</returns>
        public static XDocument ToXDocument(this XmlDocument xmlDocument)
        {
            using (var memoryStream = new MemoryStream())
            {
                xmlDocument.Save(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return XDocument.Load(memoryStream);
            }
        }

        /// <summary>
        /// Converts a XDocument object into XmlDocument
        /// </summary>
        /// <param name="xDocument">The x document.</param>
        /// <returns>The XDocument converted to XmlDocument</returns>
        public static XmlDocument ToXmlDocument(this XDocument xDocument)
        {
            using (var memoryStream = new MemoryStream())
            {
                xDocument.Save(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(memoryStream);
                return xmlDoc;
            }
        }
    }
}