namespace OllamaTest.Services
{
    public interface ILLModelQueryService
    {
        Task<string> AskAsync(string collectionName, string sessionId, string question, string context, bool includeHistory = true);
    }
}
