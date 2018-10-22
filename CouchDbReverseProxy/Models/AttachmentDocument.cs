using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CouchDbReverseProxy.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class AttachmentDocument
    {
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string DocumentId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Revisionid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string MasterDocumentId { get; set; }
    }
}