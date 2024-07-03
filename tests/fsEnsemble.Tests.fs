module fsEnsemble.Tests
open Expecto
open fsEnsemble

[<Tests>]
let tests =
    testList "fsEnsemble tests" [
        testCase "Simple test" <| fun _ ->
            let result = "Hello" |> composeFunctions (fun s -> async { return s + " World" }) (fun s -> async { return s + "!" })
            Expect.equal (Async.RunSynchronously result) "Hello World!" "Simple composition should work"
    ]

[<EntryPoint>]
let main args =
    runTestsInAssembly defaultConfig args
