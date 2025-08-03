# RAG Demo with Local Embeddings and Qdrant

This project demonstrates how to build a simple Retrieval-Augmented Generation (RAG) system using:

- **Local Text Embeddings** via [SmartComponents.LocalEmbeddings](https://github.com/dotnet/smartcomponents)
- **Vector Storage & Search** via [Qdrant](https://qdrant.tech/)
- **Ollama** to serve a local language model (e.g., `mistral:7b`)

## 💡 Why this project?

The biggest challenge in RAG systems is **text embedding** — converting meaningful text into high-dimensional vectors for retrieval. While many solutions rely on cloud APIs or heavy frameworks like ONNX, this project demonstrates how to:

- Use **local embeddings** with minimal setup
- Achieve **fast and accurate semantic search**
- Keep everything **offline and privacy-preserving**

## ⚙️ Technologies Used

| Component         | Tool/Library                                 |
|-------------------|----------------------------------------------|
| Embeddings        | [SmartComponents.LocalEmbeddings](https://www.nuget.org/packages/SmartComponents.LocalEmbeddings/) |
| Vector DB         | [Qdrant](https://qdrant.tech) (running locally) |
| LLM               | [`Ollama`](https://ollama.com/) (`mistral:7b`) |
| API Layer         | ASP.NET Core Web API                         |

## 🧠 How It Works

```
[User Question]
     ↓
Local Embedder (Microsoft's smartcomponents)
     ↓
Qdrant Search (Vector DB)
     ↓
Top Document Retrieved
     ↓
Mistral via Ollama (LLM)
     ↓
Final Answer
```

## Setup

1. Clone the repo.
2. Install Qdrant locally or run via Docker.
3. Run Ollama with:  
   ```bash
   ollama run mistral
   ```
4. Launch the ASP.NET API project.

## License

MIT License.