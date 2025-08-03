using System.Collections.Concurrent;

namespace OllamaTest
{
    public static class SessionData
    {
        // Store chat history per session
        public static readonly ConcurrentDictionary<string, List<string>> _chatHistory = new();
    }
}
