namespace IocDemoTests

open System
open IocDemo.Prelude

module Prelude =

    let intern = { Intern.name = "some intern" }
    let senior = { Senior.name = "some senior"; Senior.department = "some department" }
    let pair = (intern, senior)
    let uri = new Uri("http://localhost")

    let generateInternList (count:int) = List.replicate count intern
    let generateSeniorList (count:int) = List.replicate count senior
    let generatePairList (count:int) = List.replicate count pair
