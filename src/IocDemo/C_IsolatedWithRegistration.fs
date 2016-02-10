namespace IocDemo

open System
open System.Net
open System.Text
open IocDemo.Prelude

module C_IsolatedWithRegistration =

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
        

    let matchInterns (postDataPair:Pair -> Async<unit>) (postDataSenior:Senior -> Async<unit>)
                     (interns:Intern list) (seniors:Senior list) : Async<unit> = async {
        let (pairs, remainingSeniors) = getPairs interns seniors
        do! pairs |> List.map (fun p -> postDataPair p ) |> Async.Parallel |> Async.Ignore
        do! remainingSeniors |> List.map (fun s -> postDataSenior s) |> Async.Parallel |> Async.Ignore
    }

    let registerDependenciesAndMatch (webClient:IWebClientWrapper) (postUri:Uri) (interns:Intern list) (seniors:Senior list) : Async<unit> = async {

        //partial application:
        let postDataPair = postData<Intern * Senior> webClient postUri
        let postDataSenior = postData<Senior> webClient postUri
        
        do! matchInterns postDataPair postDataSenior interns seniors
    }

    // this code has to be commented out because the generics get resolved in a way that we don't want
//    let matchInterns postData' (interns:Intern list) (seniors:Senior list) : Async<unit> = async {
//        let (pairs, remainingSeniors) = getPairs interns seniors
//        do! pairs |> List.map (fun p -> postData' p ) |> Async.Parallel |> Async.Ignore
//        do! remainingSeniors |> List.map (fun s -> postData' s) |> Async.Parallel |> Async.Ignore
//    }
//
//    let wireUpAndMatch (webClient:IWebClientWrapper) (postUri:Uri) (interns:Intern list) (seniors:Senior list) : Async<unit> = async {
//        let postData' = postData webClient postUri
//
//        do! matchInterns postData' interns seniors
//    }
