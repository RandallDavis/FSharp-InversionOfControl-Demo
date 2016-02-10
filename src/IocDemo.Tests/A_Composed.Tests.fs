namespace IocDemoTests

open System
open System.Net
open System.Text
open Xunit
open IocDemo.Prelude
open IocDemo.A_Composed
open IocDemoTests.Prelude

module A_Composed =

    //Note: this is in no away an adequate set of unit tests - only ones that are interesting to the demo are implemented


    let [<Fact>] ``getPairs fails if less seniors than interns`` () =
        Assert.Throws<Exception> (fun _ -> getPairs (generateInternList 2) (generateSeniorList 1) |> ignore)


    // the only way to get matchInterns() to receive an exception from getPairs is to trigger a known error condition
    let [<Fact>] ``matchInterns bubbles exceptions from getPairs`` () =
        Assert.Throws<Exception> (fun _ -> matchInterns uri (generateInternList 2) (generateSeniorList 1) |> Async.RunSynchronously |> ignore)


    // don't test success conditions, because it will do real web posts!
