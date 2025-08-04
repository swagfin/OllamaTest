using Microsoft.AspNetCore.Mvc;
using OllamaTest.Services;
using System.Text.Json;

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

        [HttpPost("train/with-text")]
        public async Task<ActionResult<string>> Train([FromBody] string fact = "George is a Senior software engineer at BET Software.")
        {
            try
            {
                // ensure collection exists
                await _vectorDbContext.CreateCollectionIfNotExistsAsync(COLLECTION_NAME, 384);
                // local text embedding
                float[] rawVector = await _textEmbedder.EmbedAsync(fact);
                // save vector
                await _vectorDbContext.AddPointsAsync(COLLECTION_NAME, Guid.NewGuid().ToString(), rawVector, new
                {
                    text = fact
                });
                return Ok("success");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPost("train/with-payload")]
        public async Task<ActionResult<string>> TrainWithPayload([FromBody] object payload)
        {
            try
            {
                /*  Payloads with meta-data Example
                 * 
                    {
                        "text": "Rauvoun made 15 sales totaling KES 6,200...",
                        "date": "2025-08-03",
                        "staff": "Rauvoun",
                        "type": "staff-performance"
                    }
                 */

                if (payload is not JsonElement jsonElement) throw new ArgumentException("Invalid payload format.");
                if (jsonElement.ValueKind != JsonValueKind.Object) throw new ArgumentException("Payload must be a JSON object.");
                //proceed
                if (jsonElement.TryGetProperty("text", out var textElement))
                {
                    string? text = textElement.GetString() ?? throw new Exception("[text] property can not be NULL");
                    // ensure collection exists
                    await _vectorDbContext.CreateCollectionIfNotExistsAsync(COLLECTION_NAME, 384);
                    // local text embedding
                    float[] rawVector = await _textEmbedder.EmbedAsync(text);
                    // save vector
                    await _vectorDbContext.AddPointsAsync(COLLECTION_NAME, Guid.NewGuid().ToString(), rawVector, payload);
                    return Ok("success");
                }
                else
                    return BadRequest("[text] property was not found!");
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
