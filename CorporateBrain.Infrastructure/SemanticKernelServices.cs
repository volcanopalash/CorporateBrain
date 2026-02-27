using CorporateBrain.Application.Common.Interfaces;
using CorporateBrain.Domain.Entities;
using CorporateBrain.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
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
    private readonly ApplicationDbContext _context;

    public SemanticKernelServices(IConfiguration configuration, ApplicationDbContext context)
    {
        _context = context;
        var apiKey = configuration["AI:GemniApiKey"]!;

        // 1. Build the Kernel with OpenAI settings
        var builder = Kernel.CreateBuilder();

        builder.AddGoogleAIGeminiChatCompletion("gemini-2.5-flash", apiKey);
        builder.AddGoogleAIEmbeddingGeneration("gemini-embedding-001", apiKey);

        // now we don't need to add the vector store here because we will directly use the PostgreSQL vector extension through our DbContext
        //builder.Services.AddInMemoryVectorStore();
        //builder.AddOpenAIChatCompletion(
        //   modelId: configuration["AI:ModelId"]!,
        //   apiKey: configuration["AI:GemniApiKey"]!
        //);
        _kernel = builder.Build();
        _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        _embeddingServices = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();

        // Fix 2. Interface renamed to VecotorStore
        // var vectorStore = _kernel.GetRequiredService<VectorStore>();
        // _vectorCollection = vectorStore.GetCollection<Guid, DocumentChunk>("corporate_documents");

        // // Fix 3: Method renamed to EnsureCollectionExistsAsync
        // _vectorCollection.EnsureCollectionExistsAsync().GetAwaiter().GetResult();
    }

    public async Task IngestDocumentAsync(string text)
    {
        Console.WriteLine($"\n[POSTGRES INGESTION] Storing text: {text}\n");
        var embedding = await _embeddingServices.GenerateEmbeddingAsync(text);

        var chunk = new DocumentChunk
        {
            Id = Guid.NewGuid(),
            Text = text,
            Embedding = embedding
        };

        // Since we're now using PostgreSQL directly, we can just add the chunk to our DbContext and save changes. The embedding will be stored in the vector column.
        // await _vectorCollection.UpsertAsync(chunk);

        _context.DocumentChunks.Add(chunk);
        await _context.SaveChangesAsync();

        Console.WriteLine($"[POSTGRES INGESTION] Successfully stored document chunk with ID: {chunk.Id}\n");
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


        Console.WriteLine($"\n[POSTGRES SEARCH] User asked: {userMessage}\n");

        var questionEmbedding = await _embeddingServices.GenerateEmbeddingAsync(userMessage);

        // Fix 4. Method renamed to SearchAsync
        // we will directly query our PostgreSQL vector column using the DbContext instead of an in-memory vector store. This is a simplified example and may need adjustments based on your actual implementation.
        // var searchResults = _vectorCollection.SearchAsync(questionEmbedding, top: 2);

        // Convert the c# array into a Pgvector object so the database understands it
        var pgVector = new Pgvector.Vector(questionEmbedding);

        // The Magic SQL: Using pgvector's cosine distance operator (<=>) to find the most similar document chunks
        var topChunks = await _context.DocumentChunks
            .FromSqlInterpolated($"SELECT * FROM \"DocumentChunks\" ORDER BY \" Embedding\" <=> {pgVector} LIMIT 2")
            .ToListAsync();

        var contextText = "";

        // Fix 5 We can now loop through the results directly!
        // await foreach (var result in searchResults)
        // {
        //     contextText += result.Record.Text + "\n";
        // }

        foreach (var chunk in topChunks)
        {
            contextText += chunk.Text + "\n";
            Console.WriteLine($"[POSTGRES SEARCH] Found memory: {chunk.Id} and text: {chunk.Text}\n");
        }

        var history = new ChatHistory();
        history.AddSystemMessage($@"You are 'CorporateBrain'. Use the following context to answer the user's question. If the answer is not in the context, say 'I don't know based on the company policy.'

             --- CONTEXT ---
            {contextText}
            ----------------
            ");

        history.AddUserMessage(userMessage);

        // var result1 = await _chatCompletionService.GetChatMessageContentAsync(
        //     history,
        //     kernel: _kernel
        // );

            var result1 = await _chatCompletionService.GetChatMessageContentAsync(
                history,
                kernel: _kernel
            );
    
            Console.WriteLine($"[AI RESPONSE] {result1}\n");
        return result1.ToString();
    }
}
