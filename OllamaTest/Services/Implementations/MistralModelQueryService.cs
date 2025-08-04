namespace OllamaTest.Services.Implementations
{
    public class MistralModelQueryService : ILLModelQueryService
    {
        private readonly HttpClient _httpClient;

        public MistralModelQueryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> AskAsync(string collectionName, string sessionId, string question, string context, bool includeHistory = true)
        {
            List<string> history = SessionData._chatHistory.GetOrAdd(sessionId, _ => []);
            string historyChatStr = !includeHistory ? "" : $"\nCONVERSATION HISTORY:\n{string.Join("\n", history)}";
            string systemPrompt = @$"
You are a helpful assistant that strictly answers based on the provided context below.

RULES:
- Only use the information in the context to answer the user's question.
- Do NOT use any external or prior knowledge, even if asked.
- If the answer is not in the context, say: 'I don't know based on the given information.'
- Do not speculate, assume, or fabricate any answers.
- Do not repeat or reference the word 'context' in your answer.
- Do not explain where the answer came from — just give a direct, confident response when possible.

CONTEXT:
{context}
{historyChatStr}

USER QUESTION:
{question}

ASSISTANT:";

            var response = await _httpClient.PostAsJsonAsync("api/generate", new
            {
                model = "Mistral:7b",
                prompt = systemPrompt,
                stream = false
            });

            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());

            ModelResponse? result = await response.Content.ReadFromJsonAsync<ModelResponse>();
            string? reply = result?.Response?.Trim();

            history.Add($"User: {question}");
            history.Add($"Bot: {reply}");

            return reply ?? string.Empty;
        }
    }
}