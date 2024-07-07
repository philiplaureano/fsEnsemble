module fsEnsemble

open System
open System.IO
open System.Threading.Tasks
open Claudia
open SharpToken
open Newtonsoft.Json.Linq
open OpenAI_API
open OpenAI_API.Chat
open Mscc.GenerativeAI

// Define types for content request and response
type ContentRequest = { Prompt: string; Temperature: float }

type ContentResponse = { Response: string option }

// Define the ILanguageModelClient interface
type ILanguageModelClient =
    abstract member GenerateContentAsync: ContentRequest -> Task<Result<ContentResponse, string>>
    abstract member CountTokens: string -> int

// ChatGPT client implementation
type ChatGptClient(apiKey: string, chatModel: OpenAI_API.Models.Model) =

    interface ILanguageModelClient with
        member _.GenerateContentAsync(request: ContentRequest) =
            task {
                try
                    let api = new OpenAIAPI(apiKey)
                    let chat = api.Chat.CreateConversation()
                    chat.Model <- chatModel
                    chat.RequestParameters.Temperature <- request.Temperature

                    chat.AppendUserInput(request.Prompt)

                    let! response = chat.GetResponseFromChatbotAsync() |> Async.AwaitTask
                    return Ok { Response = Some response }
                with ex ->
                    return Error(sprintf "ChatGptClient error: %s" ex.Message)
            }

        member _.CountTokens(input: string) =
            let encoding = GptEncoding.GetEncodingForModel("gpt-4")
            encoding.CountTokens(input)


// Google Gemini client implementation
type GoogleGeminiClient(apiKey: string) =
    interface ILanguageModelClient with
        member _.GenerateContentAsync(request: ContentRequest) =
            task {
                try
                    let googleAi = new GoogleAI(apiKey)
                    let model = googleAi.GenerativeModel(model = Model.GeminiProLatest)

                    let prompt = request.Prompt
                    let temperature = Convert.ToSingle request.Temperature

                    let generateMessageRequest =
                        new GenerateMessageRequest(
                            Prompt = new MessagePrompt(Context = prompt),
                            Temperature = new Nullable<float32>(temperature)
                        )

                    let! response = model.GenerateMessage(generateMessageRequest) |> Async.AwaitTask
                    return Ok { Response = Some response.Text }
                with ex ->
                    return Error(sprintf "GoogleGeminiClient error: %s" ex.Message)
            }

        member _.CountTokens(input: string) =
            let googleAi = new GoogleAI(apiKey)
            let model = googleAi.GenerativeModel(model = Model.GeminiProLatest)
            let response = model.CountTokens(input).Result
            response.TokenCount

// Anthropic Claude Sonnet 3.5 client implementation
type ClaudeClient(apiKey: string) =
    let anthropic = new Anthropic(ApiKey = apiKey)

    interface ILanguageModelClient with
        member _.GenerateContentAsync(request: ContentRequest) =
            task {
                try
                    let messages = [| new Claudia.Message(Role= Role.User, Content = request.Prompt) |]
                    let messageRequest = new MessageRequest(Model=Claudia.Models.Claude3_5Sonnet, MaxTokens=4096, Messages=messages, Temperature = request.Temperature)                    

                    let! response = anthropic.Messages.CreateAsync(messageRequest) |> Async.AwaitTask
                    let responseText = response.Content.ToString()
                    return Ok { Response = Some responseText }
                with ex ->
                    return Error(sprintf "ClaudeClient error: %s" ex.Message)
            }

        member _.CountTokens(input: string) =
            // TODO: Implement Claude token counting logic here
            input.Length / 4 // Approximate token count (not accurate)

// Function to read API keys from configuration file
let readApiKeys (filePath: string) =
    let config = JObject.Parse(File.ReadAllText(filePath))
    let openAiApiKey = config.["OpenAI"].["ApiKey"].ToString()
    let googleGeminiApiKey = config.["GoogleGemini"].["ApiKey"].ToString()
    let claudeApiKey = config.["Claude"].["ApiKey"].ToString()
    openAiApiKey, googleGeminiApiKey, claudeApiKey

// Function to run an LLM query
let runLLMQuery (client: ILanguageModelClient) (prompt: string) (temperature: float) : Task<string> =
    task {
        let request =
            { Prompt = prompt
              Temperature = temperature }

        let! response = client.GenerateContentAsync(request)

        match response with
        | Ok content ->
            match content.Response with
            | Some res -> return res
            | None -> return ""
        | Error errMsg -> return errMsg
    }

// Function to compose two LLM query functions
let (>>>) (firstFunction: string -> Async<string>) (nextFunction: string -> Async<string>) =
    fun input ->
        async {
            let! intermediateResult = firstFunction input
            return! nextFunction intermediateResult
        }
