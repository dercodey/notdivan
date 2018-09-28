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
                if (!await CouchDbHelper.GetDbInformation())
                {
                    await CouchDbHelper.CreateDbs();
                }

                var reference =
                    await CouchDbHelper.InsertNewDocument(metadata: @"<xml></xml>");
                await CouchDbHelper.FindDocumentById(reference.Id);

                await CouchDbHelper.AddAttachment(reference.Id, reference.RevisionId, "image", new byte[100 * 100]);
                var attachment = await CouchDbHelper.GetAttachmentForDoc(reference.Id, "image");

            }).Wait();

            Console.ReadLine();
        }
    }
}
