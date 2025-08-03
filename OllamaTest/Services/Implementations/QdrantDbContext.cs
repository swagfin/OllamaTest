using System.Text;
using System.Text.Json;
namespace OllamaTest.Services.Implementations
{
    public class QdrantDbContext : IVectorDbContext
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public QdrantDbContext(HttpClient httpClient, string baseUrl = "http://localhost:6333")
        {
            _httpClient = httpClient;
            _baseUrl = baseUrl.TrimEnd('/');
        }

        public async Task<bool> CreateCollectionIfNotExistsAsync(string collectionName, int vectorSize, string distance = "Cosine")
        {
            string collectionUrl = $"{_baseUrl}/collections/{collectionName}";

            HttpResponseMessage checkResponse = await _httpClient.GetAsync(collectionUrl);
            if (checkResponse.IsSuccessStatusCode)
            {
                // Debug
                Console.WriteLine(await checkResponse.Content.ReadAsStringAsync());
                // Already exists
                return false;
            }

            var request = new
            {
                vectors = new
                {
                    size = vectorSize,
                    distance
                }
            };

            HttpResponseMessage response = await _httpClient.PutAsJsonAsync(collectionUrl, request);
            response.EnsureSuccessStatusCode();
            // Debug
            Console.WriteLine(await response.Content.ReadAsStringAsync());
            return true;
        }

        public async Task<bool> AddPointsAsync(string collectionName, string id, float[] vector, object payload)
        {
            var request = new
            {
                points = new[]
                {
                new
                {
                    id,
                    vector,
                    payload
                }
            }
            };

            HttpResponseMessage response = await _httpClient.PostAsync($"{_baseUrl}/collections/{collectionName}/points?wait=true", new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            // debug
            Console.WriteLine(await response.Content.ReadAsStringAsync());
            return true;
        }

        public async Task<string> PointsSearchAsync(string collectionName, float[] vector, int limit = 3)
        {
            var request = new
            {
                vector,
                top = limit,
                with_payload = true
            };

            HttpResponseMessage response = await _httpClient.PostAsync($"{_baseUrl}/collections/{collectionName}/points/search", new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            string rawResponse = await response.Content.ReadAsStringAsync();
            //format to string
            string? formattedResponse = ExtractTextFromTopHit(rawResponse);
            //return response
            return formattedResponse ?? string.Empty;
        }

        private static string? ExtractTextFromTopHit(string qdrantResponseJson)
        {
            using JsonDocument doc = JsonDocument.Parse(qdrantResponseJson);
            JsonElement hits = doc.RootElement.GetProperty("result");

            if (hits.GetArrayLength() == 0)
                return "No relevant information found.";

            JsonElement firstHit = hits[0];

            if (firstHit.TryGetProperty("payload", out JsonElement payload) &&
                payload.TryGetProperty("text", out JsonElement textElement))
            {
                return textElement.GetString();
            }

            return "Relevant data found, but no text content available.";
        }
    }
}