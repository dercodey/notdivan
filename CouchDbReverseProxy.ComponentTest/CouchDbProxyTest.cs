using System;
using System.Linq;
using CouchDbClient;
using Microsoft.Owin.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CouchDbReverseProxy.ComponentTest
{
    [TestClass]
    public class CouchDbProxyTest
    {
        static Random random = new Random();
        static IDisposable webAppInstance = null;

        [ClassInitialize]
        public static void Initialize(TestContext ctx)
        {
            // start the web server
            string baseAddress = "http://localhost:55465/";

            // Start OWIN host 
            webAppInstance = WebApp.Start<Startup>(url: baseAddress);

            // create the db, if needed
            if (!CouchDbHelper.GetDbInformation().Result)
            {
                var bResult = CouchDbHelper.CreateDbs().Result;
            }
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            var bResult = CouchDbHelper.GetDbInformation().Result;

            // stop the web host
            webAppInstance.Dispose();
        }

        [TestMethod]
        public void CreateAndGetMasterDocument()
        {
            // create a basic master document with some dummy metadata
            var randomBytes = new byte[25];
            random.NextBytes(randomBytes);
            var randomByteString = string.Join("|", randomBytes.Select(byt => byt.ToString()).ToArray());
            var initMetadata = $"<xml><metadata value=\"{random.Next(10)}\">{randomByteString}</metadata></xml>";
            var reference =
                CouchDbHelper.InsertNewDocument(metadata: initMetadata).Result;
            Assert.IsNotNull(reference.Id);
            Assert.IsNotNull(reference.RevisionId);

            // reget the newly created document - with no rev
            var regetNewDocument = CouchDbHelper.FindDocumentById(reference.Id).Result;
            Console.WriteLine($"Reget new document: {regetNewDocument.DocumentId} {regetNewDocument.RevisionId} " +
                $"{regetNewDocument.DatabaseName} {regetNewDocument.Metadata} {regetNewDocument.Status}");
            Assert.AreEqual(reference.Id, regetNewDocument.DocumentId);
            Assert.AreEqual(reference.RevisionId, regetNewDocument.RevisionId);
            Assert.AreEqual(initMetadata, regetNewDocument.Metadata);
        }

        [TestMethod]
        public void CreateAndGetMasterDocumentWithRev()
        {
            // create a basic master document with some dummy metadata
            var randomBytes = new byte[25];
            random.NextBytes(randomBytes);
            var randomByteString = string.Join("|", randomBytes.Select(byt => byt.ToString()).ToArray());
            var initMetadata = $"<xml><metadata value=\"{random.Next(10)}\">{randomByteString}</metadata></xml>";
            var reference =
                CouchDbHelper.InsertNewDocument(metadata: initMetadata).Result;
            Assert.IsNotNull(reference.Id);
            Assert.IsNotNull(reference.RevisionId);

            // reget the newly created document - with the revision
            var regetNewDocumentWithRev = CouchDbHelper.FindDocumentById(reference.Id, reference.RevisionId).Result;
            Console.WriteLine($"Reget new document with rev: {regetNewDocumentWithRev.DocumentId} {regetNewDocumentWithRev.RevisionId} " +
                $"{regetNewDocumentWithRev.DatabaseName} {regetNewDocumentWithRev.Metadata} {regetNewDocumentWithRev.Status}");
            Assert.AreEqual(reference.Id, regetNewDocumentWithRev.DocumentId);
            Assert.AreEqual(reference.RevisionId, regetNewDocumentWithRev.RevisionId);
            Assert.AreEqual(initMetadata, regetNewDocumentWithRev.Metadata);
        }
    }
}
