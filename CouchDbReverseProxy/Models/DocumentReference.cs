using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CouchDbReverseProxy.Models
{
    public class DocumentReference
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

    }
}