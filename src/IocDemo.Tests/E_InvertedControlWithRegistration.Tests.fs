﻿namespace IocDemoTests

open System
open System.Net
open System.Text
open Xunit
open IocDemo.Prelude
open IocDemo.E_InvertedControlWithRegistration
open IocDemoTests.Prelude

// Changes: no changes

module E_InvertedControlWithRegistration =

    //Note: this is in no away an adequate set of unit tests - only ones that are interesting to the demo are implemented


    type WebClientFake () =
        interface IWebClientWrapper with
            member this.UploadStringAsync (uri:Uri) (data:string) : Async<unit> = async.Return ()
    type WebClientFake_Throw () =
        interface IWebClientWrapper with
            member this.UploadStringAsync (uri:Uri) (data:string) : Async<unit> =
                failwith "forced exception" |> async.Return

    let webClientFake = WebClientFake ()
    let webClientFake_Throw = WebClientFake_Throw ()

    let postDataPairFake (_:Pair) = async.Return ()
    let postDataPairFake_Throw (_:Pair) = failwith "forced exception" |> async.Return

    let postDataSeniorFake (_:Senior) = async.Return ()
    let postDataSeniorFake_Throw (_:Senior) = failwith "forced exception" |> async.Return

    let getPairsFake (_:Intern list) (_:Senior list) : Lazy<Pair list * Senior list> = lazy(generatePairList 2, generateSeniorList 3)
    let getPairsFake_Throw (_:Intern list) (_:Senior list) : Lazy<Pair list * Senior list> = lazy(failwith "forced exception")


    let [<Fact>] ``getPairs fails if less seniors than interns`` () =
        Assert.Throws<Exception> (fun _ -> getPairs (generateInternList 2) (generateSeniorList 1) |> ignore)


    let [<Fact>] ``postData bubbles exceptions from webClient`` () =
        Assert.Throws<Exception> (fun _ -> postData webClientFake_Throw uri "some data" |> Async.RunSynchronously |> ignore )


    let [<Fact>] ``matchInterns succeeds`` () =
        let result = matchInterns getPairsFake postDataPairFake postDataSeniorFake (generateInternList 2) (generateSeniorList 3) |> Async.RunSynchronously
        Assert.Equal((), result)

    let [<Fact>] ``matchInterns bubbles exceptions from getPairs`` () =
        Assert.Throws<Exception> (fun _ -> matchInterns getPairsFake_Throw postDataPairFake postDataSeniorFake (generateInternList 1) (generateSeniorList 1) |> Async.RunSynchronously |> ignore)

    let [<Fact>] ``matchInterns bubbles exceptions from postData`` () =
        Assert.Throws<Exception> (fun _ -> matchInterns getPairsFake postDataPairFake_Throw postDataSeniorFake (generateInternList 2) (generateSeniorList 3) |> Async.RunSynchronously |> ignore)
