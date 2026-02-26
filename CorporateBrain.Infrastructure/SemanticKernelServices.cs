using CorporateBrain.Application.Common.Interfaces;
using CorporateBrain.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Embeddings;

namespace CorporateBrain.Infrastructure;

public sealed class SemanticKernelServices : IAiChatServices
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly ITextEmbeddingGenerationService _embeddingServices;
    private readonly VectorStoreCollection<Guid, DocumentChunk> _vectorCollection;

    public SemanticKernelServices(IConfiguration configuration)
    {
        var apiKey = configuration["AI:GemniApiKey"]!;

        // 1. Build the Kernel with OpenAI settings
        var builder = Kernel.CreateBuilder();

        builder.AddGoogleAIGeminiChatCompletion("gemini-2.5-flash", apiKey);
        builder.AddGoogleAIEmbeddingGeneration("gemini-embedding-001", apiKey);

        builder.Services.AddInMemoryVectorStore();
        //builder.AddOpenAIChatCompletion(
        //   modelId: configuration["AI:ModelId"]!,
        //   apiKey: configuration["AI:GemniApiKey"]!
        //);
        _kernel = builder.Build();
        _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        _embeddingServices = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();

        // Fix 2. Interface renamed to VecotorStore
        var vectorStore = _kernel.GetRequiredService<VectorStore>();
        _vectorCollection = vectorStore.GetCollection<Guid, DocumentChunk>("corporate_documents");

        // Fix 3: Method renamed to EnsureCollectionExistsAsync
        _vectorCollection.EnsureCollectionExistsAsync().GetAwaiter().GetResult();
    }

    public async Task IngestDocumentAsync(string text)
    {
        var embedding = await _embeddingServices.GenerateEmbeddingAsync(text);

        var chunk = new DocumentChunk
        {
            Id = Guid.NewGuid(),
            Text = text,
            Embedding = embedding
        };

        await _vectorCollection.UpsertAsync(chunk);
    }

    public async Task<string> ChatAsync(string userMessage)
    {
        // 1. Create a history object (this is where we can add "System Prompts" or previous messages in the future)
        //var history = new ChatHistory();

        //// System Prompt: Give the AI a personality!
        //history.AddSystemMessage("You are 'Corporate Brain', a helpful and slightly sarcastic AI assistant for this company. You answer questions briefly.");

        //// User Message: The actual question from the user
        //history.AddUserMessage(userMessage);

        //// 2. Get the response
        //var result = await _chatCompletionService.GetChatMessageContentAsync(
        //    history,
        //    kernel: _kernel
        //);

        //// 3. Return the text
        //return result.ToString();

        var questionEmbedding = await _embeddingServices.GenerateEmbeddingAsync(userMessage);

        // Fix 4. Method renamed to SearchAsync
        var searchResults = _vectorCollection.SearchAsync(questionEmbedding, top: 2);

        var contextText = "";

        // Fix 5 We can now loop through the results directly!
        await foreach (var result in searchResults)
        {
            contextText += result.Record.Text + "\n";
        }

        var history = new ChatHistory();
        history.AddSystemMessage($@"You are 'CorporateBrain'. Use the following context to answer the user's question. If the answer is not in the context, say 'I don't know based on the company policy.'

             --- CONTEXT ---
            {contextText}
            ----------------
            ");

        history.AddUserMessage(userMessage);

        var result1 = await _chatCompletionService.GetChatMessageContentAsync(
            history,
            kernel: _kernel
        );

        return result1.ToString();
    }
}
