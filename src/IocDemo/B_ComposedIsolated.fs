namespace IocDemo

open System
open System.Net
open System.Text
open IocDemo.Prelude

(*
    Changes:
    - IWebClientWrapper was added.
    - WebClientWrapper was added.
    - postData() now receives an IWebClientWrapper as a parameter and has string to byte array conversions migrated into WebClientWrapper.
    - matchInterns() now receives an IWebClientWrapper, which it passes to postData().
*)

module B_ComposedIsolated =

    type IWebClientWrapper =
        abstract member UploadStringAsync : Uri -> string -> Async<unit>

    type WebClientWrapper () =
        interface IWebClientWrapper with
            member this.UploadStringAsync (postUri:Uri) (data:string) : Async<unit> = async {
                use client = new WebClient()
                let dataBytes = Encoding.ASCII.GetBytes data
                do! client.UploadDataTaskAsync(postUri, dataBytes) |> Async.AwaitTask |> Async.Ignore
            }

    let postData<'a> (webClient:IWebClientWrapper) (postUri:Uri) (data:'a) : Async<unit> = async {
        let dataString = sprintf "DataToPost: %A" data
        do! webClient.UploadStringAsync postUri dataString
    }

    let getPairs (interns:Intern list) (seniors:Senior list) : Pair list * Senior list =
        let rec getPairs' (acc:Pair list) (interns':Intern list) (seniors':Senior list) : Pair list * Senior list =
            match (acc, interns', seniors') with
            | (acc, iL, []) when iL.Length > 0 -> failwith "No senior to match an intern."
            | (acc, [], sn) -> (acc, sn)
            | (acc, i::iL, s::sL) -> getPairs' ((i, s)::acc) iL sL
            | (_, _, _) -> failwith "Uncovered case"
        getPairs'[] interns seniors

    let matchInterns (webClient:IWebClientWrapper) (postUri:Uri) (interns:Intern list) (seniors:Senior list) : Async<unit> = async {
        let (pairs, remainingSeniors) = getPairs interns seniors
        do! pairs |> List.map (fun p -> postData webClient postUri p ) |> Async.Parallel |> Async.Ignore
        do! remainingSeniors |> List.map (fun s -> postData webClient postUri s) |> Async.Parallel |> Async.Ignore
    }
