using System.Text.Json;
namespace OllamaTest.Services.Implementations
{
    public class QdrantDbContext : IVectorDbContext
    {
        private readonly HttpClient _httpClient;

        public QdrantDbContext(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task CreateCollectionIfNotExistsAsync(string collectionName, int vectorSize, string distance = "Cosine")
        {
            HttpResponseMessage checkResponse = await _httpClient.GetAsync($"collections/{collectionName}");
            if (checkResponse.IsSuccessStatusCode)
            {
                return;
            }

            var request = new
            {
                vectors = new
                {
                    size = vectorSize,
                    distance
                }
            };

            HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"collections/{collectionName}", request);
            if (response.IsSuccessStatusCode)
                return;
            throw new HttpRequestException(await response.Content.ReadAsStringAsync());
        }

        public async Task AddPointsAsync(string collectionName, string id, float[] vector, object payload)
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

            HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"collections/{collectionName}/points?wait=true", request);
            if (response.IsSuccessStatusCode)
                return;
            throw new HttpRequestException(await response.Content.ReadAsStringAsync());
        }

        public async Task<string> PointsSearchAsync(string collectionName, float[] vector, int limit = 3)
        {
            var request = new
            {
                vector,
                limit = limit,
                with_payload = true
            };

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"collections/{collectionName}/points/search", request);
            string rawResponse = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(rawResponse);
            //proceed
            response.EnsureSuccessStatusCode();
            //format to string
            string? formattedResponse = ExtractTextFromTopHits(rawResponse, limit);
            //return response
            return formattedResponse ?? string.Empty;
        }

        private static string ExtractTextFromTopHits(string qdrantResponseJson, int limit)
        {
            using JsonDocument doc = JsonDocument.Parse(qdrantResponseJson);
            JsonElement hits = doc.RootElement.GetProperty("result");

            if (hits.GetArrayLength() == 0)
                return "No relevant information found.";

            List<string> texts = [];

            int count = Math.Min(limit, hits.GetArrayLength());
            for (int i = 0; i < count; i++)
            {
                JsonElement hit = hits[i];

                if (hit.TryGetProperty("payload", out JsonElement payload) && payload.TryGetProperty("text", out JsonElement textElement))
                {
                    string? text = textElement.GetString();
                    if (!string.IsNullOrEmpty(text))
                    {
                        texts.Add(text);
                    }
                }
            }

            if (texts.Count == 0)
                return "Relevant data found, but no text content available.";

            // Join all extracted texts with a separator (customize if needed)
            return string.Join(Environment.NewLine, texts);
        }
    }
}