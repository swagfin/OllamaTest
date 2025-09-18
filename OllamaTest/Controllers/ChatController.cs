using Microsoft.AspNetCore.Mvc;
using OllamaTest.Services;
using System.Diagnostics;

namespace OllamaTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private const string COLLECTION_NAME = "test-local";
        private readonly IVectorDbContext _vectorDbContext;
        private readonly ITextEmbedder _textEmbedder;
        private readonly ILLModelQueryService _llModelQueryService;

        public ChatController(IVectorDbContext vectorDbContext, ITextEmbedder textEmbedder, ILLModelQueryService llModelQueryService)
        {
            _vectorDbContext = vectorDbContext;
            _textEmbedder = textEmbedder;
            _llModelQueryService = llModelQueryService;
        }

        [HttpPost]
        public async Task<IActionResult> ChatAsync([FromBody] ChatRequest request, bool includeHistory = true)
        {
            try
            {

                string sessionId = request.SessionId ?? Guid.NewGuid().ToString();
                // local text embedding
                Stopwatch embeddingTimer = Stopwatch.StartNew();
                float[] questionEmbedding = await _textEmbedder.EmbedAsync(request.Message);
                embeddingTimer.Stop();
                // Qdrant search request
                Stopwatch vectorTimer = Stopwatch.StartNew();
                string llmContext = await _vectorDbContext.PointsSearchAsync(COLLECTION_NAME, vector: questionEmbedding, limit: 3);
                vectorTimer.Stop();
                //prompt
                Stopwatch llmTimer = Stopwatch.StartNew();
                string botReply = await _llModelQueryService.AskAsync(COLLECTION_NAME, sessionId, request.Message, llmContext, includeHistory);
                //reply
                llmTimer.Stop();
                return Ok(new ChatResponse
                {
                    SessionId = sessionId,
                    Response = botReply,
                    EmbeddingQueryTime = $"{embeddingTimer.ElapsedMilliseconds:N0} ms",
                    VectorDbQueryTime = $"{vectorTimer.ElapsedMilliseconds:N0} ms",
                    LLMQueryTime = $"{llmTimer.ElapsedMilliseconds:N0} ms",
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }
}
