namespace IocDemo

open System
open System.Net
open System.Text
open IocDemo.Prelude

module A_Composed =

    let postData<'a> (postUri:Uri) (data:'a) : Async<unit> = async {
        let client = new WebClient()
        let dataBytes = sprintf "DataToPost: %A" data |> Encoding.ASCII.GetBytes
        do! client.UploadDataTaskAsync(postUri, dataBytes) |> Async.AwaitTask |> Async.Ignore
    }

    let getPairs (interns:Intern list) (seniors:Senior list) : Pair list * Senior list =
        let rec getPairs' (acc:Pair list) (interns':Intern list) (seniors':Senior list) : Pair list * Senior list =
            match (acc, interns', seniors') with
            | (acc, iL, []) when iL.Length > 0 -> failwith "No senior to match an intern."
            | (acc, [], sn) -> (acc, sn)
            | (acc, i::iL, s::sL) -> getPairs' ((i, s)::acc) iL sL
            | (_, _, _) -> failwith "Uncovered case"
        getPairs'[] interns seniors

    let matchInterns (postUri:Uri) (interns:Intern list) (seniors:Senior list) : Async<unit> = async {
        let (pairs, remainingSeniors) = getPairs interns seniors
        do! pairs |> List.map (fun p -> postData postUri p ) |> Async.Parallel |> Async.Ignore
        do! remainingSeniors |> List.map (fun s -> postData postUri s) |> Async.Parallel |> Async.Ignore
    }
