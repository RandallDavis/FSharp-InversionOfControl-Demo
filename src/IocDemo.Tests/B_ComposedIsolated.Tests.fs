namespace IocDemoTests

open System
open System.Net
open System.Text
open Xunit
open IocDemo.Prelude
open IocDemo.B_ComposedIsolated
open IocDemoTests.Prelude

module B_ComposedIsolated =

    //Note: this is in no away an adequate set of unit tests - only ones that are interesting to the demo are implemented


    type WebClientFake () =
        interface IWebClientWrapper with
            member this.UploadStringAsync (uri:Uri) (data:string) : Async<unit> = async.Return ()
    type WebClientFake_Throw () =
        interface IWebClientWrapper with
            member this.UploadStringAsync (uri:Uri) (data:string) : Async<unit> =
                failwith "forced exception" |> async.Return

    let webClient = WebClientFake ()
    let webClient_Throw = WebClientFake_Throw ()


    let [<Fact>] ``getPairs fails if less seniors than interns`` () =
        Assert.Throws<Exception> (fun _ -> getPairs (generateInternList 2) (generateSeniorList 1) |> ignore)


    let [<Fact>] ``postData bubbles exceptions from webClient`` () =
        Assert.Throws<Exception> (fun _ -> postData webClient_Throw uri "some data" |> Async.RunSynchronously |> ignore )


    let [<Fact>] ``matchInterns succeeds`` () =
        let result = matchInterns webClient uri (generateInternList 2) (generateSeniorList 3) |> Async.RunSynchronously
        Assert.Equal((), result)

    // the only way to get matchInterns() to receive an exception from getPairs is to trigger a known error condition
    let [<Fact>] ``matchInterns bubbles exceptions from getPairs`` () =
        Assert.Throws<Exception> (fun _ -> matchInterns webClient uri (generateInternList 2) (generateSeniorList 1) |> Async.RunSynchronously |> ignore)

    let [<Fact>] ``matchInterns bubbles exceptions from postData`` () =
        Assert.Throws<Exception> (fun _ -> matchInterns webClient_Throw uri (generateInternList 2) (generateSeniorList 3) |> Async.RunSynchronously |> ignore)
