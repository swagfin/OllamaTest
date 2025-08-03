using SmartComponents.LocalEmbeddings;

namespace OllamaTest.Services.Implementations
{
    public class LocalTextEmbedder : ITextEmbedder
    {
        //local embedder
        //see::https://github.com/dotnet/smartcomponents/blob/main/docs/local-embeddings.md
        private readonly LocalEmbedder _localEmbedder;

        public LocalTextEmbedder()
        {
            _localEmbedder = new LocalEmbedder();
        }

        public Task<float[]> EmbedAsync(string text)
        {
            float[] result = _localEmbedder.Embed<EmbeddingF32>(text).Values.ToArray();
            return Task.FromResult(result);
        }
    }
}
