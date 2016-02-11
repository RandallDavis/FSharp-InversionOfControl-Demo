namespace IocDemo

open System
open System.Net
open System.Text
open IocDemo.Prelude

(*
    Changes:
    - registerDependenciesAndMatch() is changed to create a WebClientWrapper rather than receive an IWebClientWrapper.
*)

module E_InvertedControlWithRegistration =

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

    let getPairs (interns:Intern list) (seniors:Senior list) : Lazy<Pair list * Senior list> =
        let rec getPairs' (acc:Pair list) (interns':Intern list) (seniors':Senior list) : Lazy<Pair list * Senior list> =
            match (acc, interns', seniors') with
            | (acc, iL, []) when iL.Length > 0 -> failwith "No senior to match an intern."
            | (acc, [], sn) -> lazy(acc, sn)
            | (acc, i::iL, s::sL) -> getPairs' ((i, s)::acc) iL sL
            | (_, _, _) -> failwith "Uncovered case"
        getPairs'[] interns seniors

    let matchInterns (getPairs':Intern list -> Senior list -> Lazy<Pair list * Senior list>)
                     (postDataPair:Pair -> Async<unit>) (postDataSenior:Senior -> Async<unit>)
                     (interns:Intern list) (seniors:Senior list) : Async<unit> = async {
        let pairsResult = getPairs' interns seniors
        let (pairs, remainingSeniors) = pairsResult.Force()
        do! pairs |> List.map (fun p -> postDataPair p ) |> Async.Parallel |> Async.Ignore
        do! remainingSeniors |> List.map (fun s -> postDataSenior s) |> Async.Parallel |> Async.Ignore
    }

    let registerDependenciesAndMatch (postUri:Uri) (interns:Intern list) (seniors:Senior list) : Async<unit> = async {

        let webClient = WebClientWrapper()

        //partial application:
        let postDataPair = postData<Intern * Senior> webClient postUri
        let postDataSenior = postData<Senior> webClient postUri
        
        do! matchInterns getPairs postDataPair postDataSenior interns seniors
    }
