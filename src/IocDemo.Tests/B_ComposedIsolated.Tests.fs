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


    // Changes: these are new
    type WebClientFake () =
        interface IWebClientWrapper with
            member this.UploadStringAsync (uri:Uri) (data:string) : Async<unit> = async.Return ()
    type WebClientFake_Throw () =
        interface IWebClientWrapper with
            member this.UploadStringAsync (uri:Uri) (data:string) : Async<unit> =
                failwith "forced exception" |> async.Return

    // Changes: these are new
    let webClientFake = WebClientFake ()
    let webClientFake_Throw = WebClientFake_Throw ()


    let [<Fact>] ``getPairs fails if less seniors than interns`` () =
        Assert.Throws<Exception> (fun _ -> getPairs (generateInternList 2) (generateSeniorList 1) |> ignore)


    // Changes: this is new
    let [<Fact>] ``postData bubbles exceptions from webClient`` () =
        Assert.Throws<Exception> (fun _ -> postData webClientFake_Throw uri "some data" |> Async.RunSynchronously |> ignore )


    // Changes: this is new
    let [<Fact>] ``matchInterns succeeds`` () =
        let result = matchInterns webClientFake uri (generateInternList 2) (generateSeniorList 3) |> Async.RunSynchronously
        Assert.Equal((), result)

    // the only way to get matchInterns() to receive an exception from getPairs is to trigger a known error condition
    let [<Fact>] ``matchInterns bubbles exceptions from getPairs`` () =
        Assert.Throws<Exception> (fun _ -> matchInterns webClientFake uri (generateInternList 2) (generateSeniorList 1) |> Async.RunSynchronously |> ignore)

    // Changes: this is new
    let [<Fact>] ``matchInterns bubbles exceptions from postData`` () =
        Assert.Throws<Exception> (fun _ -> matchInterns webClientFake_Throw uri (generateInternList 2) (generateSeniorList 3) |> Async.RunSynchronously |> ignore)
