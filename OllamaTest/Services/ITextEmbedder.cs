namespace OllamaTest.Services
{
    public interface ITextEmbedder
    {
        Task<float[]> EmbedAsync(string text);
    }
}
