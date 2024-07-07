module fsEnsemble.Tests
open Expecto
open fsEnsemble
open System

// Mock functions to simulate LLM operations
let mockGenerateCode (prompt: string) : Async<string> = async {
    return $"""
    // Generated C# code based on the prompt
    public class HelloWorld
    {{
        public static void Main(string[] args)
        {{
            Console.WriteLine("Hello, World!");
        }}
    }}
    """
}

let mockReviewCode (code: string) : Async<string> = async {
    return """
    // Feedback: The code is correct, but it can be improved by adding error handling and comments.
    // Consider adding exception handling for potential runtime errors.
    """
}

let mockReviseCode (input: string) : Async<string> = async {
    let parts = input.Split([|"\n// Feedback:"|], StringSplitOptions.RemoveEmptyEntries)
    let originalCode = parts.[0]
    let feedback = if parts.Length > 1 then parts.[1] else ""

    return $"""
    // Revised C# code with feedback incorporated
    public class HelloWorld
    {{
        public static void Main(string[] args)
        {{
            try
            {{
                Console.WriteLine("Hello, World!");
            }}
            catch (Exception ex)
            {{
                Console.WriteLine("An error occurred: " + ex.Message);
            }}
        }}
    }}
    // Feedback: {feedback.Trim()}
    """
}

[<Tests>]
let tests =
    testList "fsEnsemble tests" [
        testCase "Simple composition test using >>>" <| fun _ ->
            let result = 
                "Write a simple C# program that prints 'Hello, World!'"
                |> (mockGenerateCode >>> mockReviewCode >>> mockReviseCode)
                |> Async.RunSynchronously

            let expected = """
            // Revised C# code with feedback incorporated
            public class HelloWorld
            {
                public static void Main(string[] args)
                {
                    try
                    {
                        Console.WriteLine("Hello, World!");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("An error occurred: " + ex.Message);
                    }
                }
            }
            // Feedback: The code is correct, but it can be improved by adding error handling and comments.
            // Consider adding exception handling for potential runtime errors.
            """

            Expect.equal result expected "Composition should work correctly using >>>"
    ]

[<EntryPoint>]
let main args =
    runTestsInAssembly defaultConfig args
