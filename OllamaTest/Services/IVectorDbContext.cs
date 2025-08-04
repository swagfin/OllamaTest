
namespace OllamaTest.Services
{
    public interface IVectorDbContext
    {
        Task AddPointsAsync(string collectionName, string id, float[] vector, object payload);
        Task CreateCollectionIfNotExistsAsync(string collectionName, int vectorSize, string distance = "Cosine");
        Task<string> PointsSearchAsync(string collectionName, float[] vector, int limit = 3);
    }
}