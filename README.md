# fsEnsemble

fsEnsemble is an F# library designed to simplify the integration and use of multiple language models (LLMs) in a composable and functional way. This library currently supports two major LLM providers: OpenAI and Google Gemini, with plans to include other providers such as Anthropic Claude in the future.

## Purpose

The primary goal of fsEnsemble is to provide a simple and unified interface for making one-shot queries to different LLMs. This library abstracts the complexities involved in interacting with various LLM providers, allowing developers to focus on building applications. While fsEnsemble opens the door to the world of LLMs, it is up to you, fellow developers, to pave the rest of the way once these doors are open.

## Features

- **Unified Interface**: Provides a common interface to interact with different LLM providers.
- **One-shot Queries**: Designed for one-shot queries to keep the library simple and easy to use.
- **Composable Functions**: Supports functional composition of LLM queries for flexible and reusable code.
- **Extensible**: Future plans to include support for additional LLM providers such as Anthropic Claude.

## Installation

You can install the `fsEnsemble` package from NuGet:

## Usage
```bash
dotnet add package fsEnsemble
```
### Adding a Client

To use fsEnsemble, you'll need to set up clients for the supported LLM providers. Here’s an example of how to set up and use OpenAI and Google Gemini clients.

#### Example Setup

1. **Define API Keys**: Ensure you have the necessary API keys for OpenAI and Google Gemini.
2. **Initialize Clients**: Set up the clients using the provided interfaces.

```fsharp
open fsEnsemble

// Read API keys from configuration file
let openAiApiKey, googleGeminiApiKey, claudeApiKey = readApiKeys "config.json"

// Create instances of the LLM clients
let chatGptClient = ChatGptClient(openAiApiKey) :> ILanguageModelClient
let googleGeminiClient = GoogleGeminiClient(googleGeminiApiKey) :> ILanguageModelClient
let claudeClient = ClaudeClient(claudeApiKey) :> ILanguageModelClient

// Define the temperature for the queries
let temperature = Some 0.5

// Create LLM query functions
let chatGptQuery = createQueryFunction chatGptClient temperature
let googleGeminiQuery = createQueryFunction googleGeminiClient temperature
let claudeQuery = createQueryFunction claudeClient temperature

// Define a sample prompt
let prompt = "Tell me about the weather today."

// Run the query
let result = runLLMQuery chatGptClient prompt temperature |> Async.RunSynchronously

// Print the result
printfn "Response: %s" result
```

### Composing Queries Using the `>>>` Operator

fsEnsemble allows you to compose multiple LLM queries using functional composition with the `>>>` operator. This operator helps in creating a pipeline of LLM queries where the output of one function becomes the input for the next.

#### Example: Generating, Reviewing, and Revising Code

Here’s an example of how to chain three LLM functions: generating code, reviewing the generated code, and revising the code based on feedback.

```fsharp
open System

// Define the custom operator for chaining LLM functions
let (>>>) (firstFunction: string -> Async<string>) (nextFunction: string -> Async<string>) =
    fun input -> async {
        let! intermediateResult = firstFunction input
        return! nextFunction intermediateResult
    }

// Function to simulate generating C# code from a prompt
let generateCode (prompt: string) : Async<string> = async {
    // Simulate LLM response
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

// Function to simulate reviewing C# code and providing feedback
let reviewCode (code: string) : Async<string> = async {
    // Simulate LLM review feedback
    return """
    // Feedback: The code is correct, but it can be improved by adding error handling and comments.
    // Consider adding exception handling for potential runtime errors.
    """
}

// Function to simulate revising C# code based on feedback
let reviseCode (input: string) : Async<string> = async {
    // Split the input into original code and feedback
    let parts = input.Split([|"\n// Feedback:"|], StringSplitOptions.RemoveEmptyEntries)
    let originalCode = parts.[0]
    let feedback = if parts.Length > 1 then parts.[1] else ""

    // Simulate LLM revision based on feedback
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

// Example chaining of LLM functions
let chainExample = generateCode >>> reviewCode >>> reviseCode

// Define a prompt for generating code
let prompt = "Write a simple C# program that prints 'Hello, World!'"

// Run the chained LLM functions with an input
let result = chainExample prompt |> Async.RunSynchronously

// Print the result
printfn "%s" result
```