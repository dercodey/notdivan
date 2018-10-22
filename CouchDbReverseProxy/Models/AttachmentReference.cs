using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CouchDbReverseProxy.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class AttachmentReference
    {
        /// <summary>
        /// 
        /// </summary>
        public string AttachmentDbName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AttachmentDocumentId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AttachmentRevisionId { get; set; }
    }
}