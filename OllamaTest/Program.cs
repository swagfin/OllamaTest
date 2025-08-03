using OllamaTest.Services;
using OllamaTest.Services.Implementations;

namespace OllamaTest
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // text embedding
            builder.Services.AddSingleton<ITextEmbedder, LocalTextEmbedder>();
            // Vector Db, using Qdrant
            builder.Services.AddHttpClient<IVectorDbContext, QdrantDbContext>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:6333");
            });
            //Large Language Model Query (Using Ollama and Mistral) 
            builder.Services.AddHttpClient<ILLModelQueryService, MistralModelQueryService>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:11434");
            });


            WebApplication app = builder.Build();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            await app.RunAsync();
        }
    }
}
