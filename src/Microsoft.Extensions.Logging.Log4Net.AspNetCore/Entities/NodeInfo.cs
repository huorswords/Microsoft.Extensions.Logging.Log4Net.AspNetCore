namespace Microsoft.Extensions.Logging.Log4Net.AspNetCore.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    ///  Class to store information of a log4net xml config file node.
    /// </summary>
    public class NodeInfo
    {
        /// <summary>
        /// Gets or sets the x path to find the node to override.
        /// </summary>
        /// <value>
        /// The x path.
        /// </value>
        public string XPath { get; set; }

        /// <summary>
        /// Gets or sets the content of the node.
        /// </summary>
        /// <value>
        /// The content of the node.
        /// </value>
        public string NodeContent { get; set; }

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public Dictionary<string, string> Attributes { get; set; }
    }
}