using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NotDivan
{
    public class Program
    {
        public static void Main()
        {
            Task.Run(async () =>
            {
                // create DBs if they don't exist
                if (!await CouchDbHelper.GetDbInformation())
                {
                    await CouchDbHelper.CreateDbs();
                }

                // create a basic master document with some dummy metadata
                var reference =
                    await CouchDbHelper.InsertNewDocument(metadata: @"<xml><metadata value=""2""></metadata></xml>");

                // reget the newly created document - with no rev
                var regetNewDocument = await CouchDbHelper.FindDocumentById(reference.Id);
                Console.WriteLine($"Reget new document: {regetNewDocument.DocumentId} {regetNewDocument.RevisionId} " +
                    $"{regetNewDocument.DatabaseName} {regetNewDocument.Metadata} {regetNewDocument.Status}");

                // reget the newly created document - with the revision
                var regetNewDocumentWithRev = await CouchDbHelper.FindDocumentById(reference.Id, reference.RevisionId);
                Console.WriteLine($"Reget new document with rev: {regetNewDocumentWithRev.DocumentId} {regetNewDocumentWithRev.RevisionId} " +
                    $"{regetNewDocumentWithRev.DatabaseName} {regetNewDocumentWithRev.Metadata} {regetNewDocumentWithRev.Status}");

                // populate a random buffer
                Random rand = new Random();
                var buffer = new byte[100 * 100];
                rand.NextBytes(buffer);

                // attach the buffer to the document
                var attachmentReference =
                    await CouchDbHelper.AddAttachment(reference.Id, reference.RevisionId, 
                        "image", buffer);
                Console.WriteLine($"Attachment reference: {attachmentReference.Id} {attachmentReference.RevisionId}");

                // reget the attached buffer
                var regetAttachmentBuffer = 
                    await CouchDbHelper.GetAttachmentForDoc(reference.Id, "image");
                Console.WriteLine($"Reget attachment length: {regetAttachmentBuffer.Length}");

                // compare reget buffer with original
                for (int n = 0; n < buffer.Length; n++)
                {
                    System.Diagnostics.Trace.Assert(buffer[n] == regetAttachmentBuffer[n]);
                }
                Console.WriteLine("Successfully compared reget buffer with original");

            }).Wait();

            Console.ReadLine();
        }
    }
}
