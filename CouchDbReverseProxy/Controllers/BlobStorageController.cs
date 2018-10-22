using CouchDbReverseProxy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace CouchDbReverseProxy.Controllers
{
    [Route("bsi")]
    public class BlobStorageController : ApiController
    {
        //[Authorize]
        [Route("bsi/_master/{docid}")]
        [HttpPut]
        public Task<DocumentReference>
            CreateOrUpdateMasterDocument([FromBody] MasterDocument masterDocument, string rev = null)
        {
            return Task.FromResult(new DocumentReference()
            {
                DocumentId = masterDocument.DocumentId,
                Revisionid = masterDocument.Revisionid,
            });
        }

        //[Authorize]
        [Route("bsi/_master/{docid}")]
        [HttpGet]
        public Task<MasterDocument>
            GetMasterDocument(string docid, string rev = null)
        {
            return Task.FromResult(new MasterDocument()
            {
                Attachments = 
                    new AttachmentReference[] { },
                DocumentId = "12u4872489",
                Revisionid = "2",
                MetaData = "<xml></xml>",
            });
        }

    }
}
