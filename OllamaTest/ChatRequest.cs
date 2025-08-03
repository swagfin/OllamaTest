using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OllamaTest
{
    public class ChatRequest
    {
        [Required, MinLength(1), DefaultValue("user_001")]
        public string? SessionId { get; set; }
        [Required, MinLength(2)]
        public string Message { get; set; } = "";
    }

    public class ChatResponse
    {
        public string SessionId { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public string EmbeddingQueryTime { get; set; } = string.Empty;
        public string VectorDbQueryTime { get; set; } = string.Empty;
        public string LLMQueryTime { get; set; } = string.Empty;
    }

    public class ModelResponse
    {
        public string Response { get; set; } = "";
    }
}
