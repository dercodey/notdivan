namespace FsCouchDbReverseProxy.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open System.Net.Http
open Microsoft.AspNetCore.Mvc


[<Route("api/[controller]")>]
[<ApiController>]
type CouchDbApiController () =
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
            return ActionResult<string>(content)
        }

    [<HttpPut("{dbname}")>]
    member this.CreateDb(dbname:string) = 
        requestAsync (put empty) dbname

    [<HttpGet("{dbname}")>]
    member this.GetDbInfo(dbname:string) =
        requestAsync get dbname

    [<HttpDelete("{dbname}")>]
    member this.DeletDb(dbname:string) =
        requestAsync delete dbname

    [<HttpPut("{dbname}/{docid}")>]
    member this.CreateOrUpdateDocument(dbname:string, docid:string, ver:string) = 
        let content = new StreamContent(this.Request.Body)
        requestAsync (put content) dbname

    [<HttpGet("{dbname}/{docid}")>]
    member this.GetDocument(dbname:string, docid:string, ver:string) =
        requestAsync get (sprintf "%s/%s" dbname docid)