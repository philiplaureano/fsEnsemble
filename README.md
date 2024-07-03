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

TODO: Instructions for installation

## Usage

### Adding a Client

To use fsEnsemble, you'll need to set up clients for the supported LLM providers. Here’s an example of how to set up and use OpenAI and Google Gemini clients.

#### Example Setup

1. **Define API Keys**: Ensure you have the necessary API keys for OpenAI and Google Gemini.
2. **Initialize Clients**: Set up the clients using the provided interfaces.

```fsharp
open fsEnsemble

// Read API keys from configuration file
let openAiApiKey, googleGeminiApiKey = readApiKeys "config.json"

// Create instances of the LLM clients
let chatGptClient = ChatGptClient(openAiApiKey, OpenAI_API.Models.Model.ChatGPT3_5Turbo) :> ILanguageModelClient
let googleGeminiClient = GoogleGeminiClient(googleGeminiApiKey) :> ILanguageModelClient

// Define the temperature for the queries
let temperature = Some 0.5

// Create LLM query functions
let chatGptQuery = createQueryFunction chatGptClient temperature
let googleGeminiQuery = createQueryFunction googleGeminiClient temperature

// Define a sample prompt
let prompt = "Tell me about the weather today."

// Run the query
let result = runLLMQuery chatGptClient prompt temperature |> Async.RunSynchronously

// Print the result
printfn "Response: %s" result
