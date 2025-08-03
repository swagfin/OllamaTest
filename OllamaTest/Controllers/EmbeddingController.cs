using Microsoft.AspNetCore.Mvc;
using OllamaTest.Services;

namespace OllamaTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmbeddingController : ControllerBase
    {
        private const string COLLECTION_NAME = "test-local";
        private readonly IVectorDbContext _vectorDbContext;
        private readonly ITextEmbedder _textEmbedder;

        public EmbeddingController(IVectorDbContext vectorDbContext, ITextEmbedder textEmbedder)
        {
            _vectorDbContext = vectorDbContext;
            _textEmbedder = textEmbedder;
        }

        [HttpGet("train")]
        public async Task<ActionResult<string>> Train(string fact = "George is a Senior software engineer at BET Software.")
        {
            try
            {
                // ensure collection exists
                await _vectorDbContext.CreateCollectionIfNotExistsAsync(COLLECTION_NAME, 384);
                // local text embedding
                float[] rawVector = await _textEmbedder.EmbedAsync(fact);
                // save vector
                bool success = await _vectorDbContext.AddPointsAsync(COLLECTION_NAME, Guid.NewGuid().ToString(), rawVector, new
                {
                    text = fact
                });
                return Ok(success);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("query")]
        public async Task<ActionResult<string>> GetAskRag(string question = "Who is George?", int limit = 1)
        {
            // local text embedding
            float[] questionEmbedding = await _textEmbedder.EmbedAsync(question);
            // Qdrant search request
            string response = await _vectorDbContext.PointsSearchAsync(COLLECTION_NAME, questionEmbedding, limit);
            //format response
            return Ok(response);
        }
    }
}
