namespace FsCouchDbReverseProxy.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open System.Net.Http
open Microsoft.AspNetCore.Mvc
open Newtonsoft.Json


type AttachmentDocument = {Id:string; Revision:string; MasterDocumentId:string}
type AttachmentReference = {Id:string; Revision:string}
type MasterDocument = {Id:string; Revision:string; Metadata:string; Attachments:AttachmentReference[]}

[<ApiController>]
type BlobStorageController () =
    inherit ControllerBase()

    let client = new HttpClient(BaseAddress = Uri("http://localhost:5984"))
    let get (url:string) = client.GetAsync(url)
    let put (content) (url:string) = client.PutAsync(url, content)
    let delete (url:string) = client.DeleteAsync(url)
    let empty = new StringContent(System.String.Empty)
    let requestAsync (action:string->Task<HttpResponseMessage>) (url:string) =
        async {
            let! response = action(url) |> Async.AwaitTask
            response.EnsureSuccessStatusCode () |> ignore
            let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            let json = JsonConvert.DeserializeObject<'t>(content)
            return ActionResult<string>(content)
        }
        
    [<HttpPut("masterdb/{docid}")>]
    member this.CreateMasterDocument(docid:string, ver:string, [<FromBody>] doc:MasterDocument) = 
        let content = new StringContent(JsonConvert.SerializeObject(doc))
        requestAsync (put content) "masterdb/{docid}"

    [<HttpPut("masterdb/{docid}?ver={ver}")>]
    member this.UpdateMasterDocument(docid:string, ver:string, [<FromBody>] doc:MasterDocument) = 
        let content = new StringContent(JsonConvert.SerializeObject(doc))
        requestAsync (put content) "masterdb/{docid}?ver={ver}"

    [<HttpGet("masterdb/{docid}?ver={ver}")>]
    member this.GetMasterDocument(dbname:string, docid:string, ver:string) =
        requestAsync get (sprintf "%s/%s" dbname docid)

    [<HttpPut("{attachmentdb}/{docid}")>]
    member this.Attach(attachmentdb:string, docid:string, [<FromBody>] doc:AttachmentDocument) = 
        let content = new StreamContent(this.Request.Body)
        requestAsync (put content) "masterdb/{docid}"

    [<HttpGet("{attachmentdb}/{docid}")>]
    member this.GetAttachment(attachmentdb:string, docid:string) =
        requestAsync get (sprintf "%s/%s" attachmentdb docid)

