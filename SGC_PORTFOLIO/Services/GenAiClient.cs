using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SGC_PORTFOLIO.Services
{

    public class GenAiClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _openAiApiKey;
        private const string OpenAiChatEndpoint = "https://api.openai.com/v1/chat/completions";
        private const string OpenAiEmbeddingEndpoint = "https://api.openai.com/v1/embeddings";
        private const string EmbeddingModel = "text-embedding-3-small";
        private const string TagsFilePath = "globalTags.json";
        private const string EmbeddingsCachePath = "tagEmbeddings.json";

        public GenAiClient(IConfiguration configuration)
        {
            _openAiApiKey = configuration["OpenAI:ApiKey"];
            if (string.IsNullOrWhiteSpace(_openAiApiKey))
                throw new ArgumentException("OpenAI:ApiKey is missing in configuration.");

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAiApiKey);
        }

        public async Task<(string summary, List<string> ageTags, List<string> expTags, List<string> topicTags)> AnalyzePdfTextAsync(string rawText)
        {
            // Call GPT to get summary, ageTags, and expTags
            var systemPrompt = "You are a PDF analysis assistant. Given raw extracted text from a PDF, produce a JSON object with exactly these fields: \"summary\" (a concise paragraph describing what the pdf is talking about and what you'll get by reading it, i'd preffer if the summary is written in an intersting way so that reader will open the pdf and read it), \"ageTags\" (give int, min age reader can be (min is 5 always), tag to check if pdf is 18), and \"expTags\" (Beginner or Intermediate or Advanced). Respond with JUST the JSON.";
            var userPrompt = $"Here is the extracted text:\n\n{rawText}";

            var payload = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
                temperature = 0.5,
                max_tokens = 800
            };

            var requestJson = JsonConvert.SerializeObject(payload);
            using var request = new HttpRequestMessage(HttpMethod.Post, OpenAiChatEndpoint)
            {
                Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
            };

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();

            var root = JObject.Parse(jsonResponse);
            var content = root["choices"]?[0]?["message"]?["content"]?.ToString().Trim();
            if (string.IsNullOrWhiteSpace(content))
                throw new Exception("OpenAI returned no content.");

            var parsed = JObject.Parse(content);
            var summary = parsed["summary"]?.ToString() ?? "";

            // Fix: handle both string and array for ageTags and expTags
            List<string> ageTags = new List<string>();
            var ageToken = parsed["ageTags"];
            if (ageToken != null)
            {
                if (ageToken.Type == JTokenType.String)
                    ageTags.Add(ageToken.ToString());
                else if (ageToken.Type == JTokenType.Array)
                    ageTags = ageToken.ToObject<List<string>>() ?? new List<string>();
                else if (ageToken.Type == JTokenType.Integer || ageToken.Type == JTokenType.Float)
                    ageTags.Add(ageToken.ToString());
            }

            List<string> expTags = new List<string>();
            var expToken = parsed["expTags"];
            if (expToken != null)
            {
                if (expToken.Type == JTokenType.String)
                    expTags.Add(expToken.ToString());
                else if (expToken.Type == JTokenType.Array)
                    expTags = expToken.ToObject<List<string>>() ?? new List<string>();
            }

            // Using embedding-based tag matching instead of GPT-predicted tags
            var topicTags = await PickTop13TagsFromTextAsync(rawText);

            return (summary, ageTags, expTags, topicTags);
        }

        private async Task<List<string>> PickTop13TagsFromTextAsync(string rawText)
        {
            var tagEmbeddings = await LoadOrCreateTagEmbeddingsAsync();
            var docEmbedding = await GetEmbeddingAsync(rawText);

            return tagEmbeddings
                .Select(tag => new
                {
                    tag.Tag,
                    Similarity = CosineSimilarity(docEmbedding, tag.Embedding)
                })
                .OrderByDescending(t => t.Similarity)
                .Take(13)
                .Select(t => t.Tag)
                .ToList();
        }

        private async Task<List<TagEmbedding>> LoadOrCreateTagEmbeddingsAsync()
        {
            if (File.Exists(EmbeddingsCachePath))
            {
                var cachedJson = await File.ReadAllTextAsync(EmbeddingsCachePath);
                var cached = JsonConvert.DeserializeObject<List<TagEmbedding>>(cachedJson);
                if (cached?.Any() == true)
                    return cached;
            }

            var tagsJson = await File.ReadAllTextAsync(TagsFilePath);
            var tags = JsonConvert.DeserializeObject<List<string>>(tagsJson);
            if (tags == null || tags.Count == 0)
                throw new Exception("Tag list in globalTags.json is empty or invalid.");

            var embeddings = new List<TagEmbedding>();
            foreach (var tag in tags)
            {
                var vector = await GetEmbeddingAsync(tag);
                embeddings.Add(new TagEmbedding { Tag = tag, Embedding = vector });
            }

            var jsonToSave = JsonConvert.SerializeObject(embeddings, Formatting.Indented);
            await File.WriteAllTextAsync(EmbeddingsCachePath, jsonToSave);
            return embeddings;
        }

        private async Task<List<float>> GetEmbeddingAsync(string text)
        {
            var payload = new
            {
                model = EmbeddingModel,
                input = text
            };

            var requestJson = JsonConvert.SerializeObject(payload);
            using var request = new HttpRequestMessage(HttpMethod.Post, OpenAiEmbeddingEndpoint)
            {
                Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
            };

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var root = JObject.Parse(jsonResponse);

            var vector = root["data"]?[0]?["embedding"]?.ToObject<List<float>>();
            if (vector == null)
                throw new Exception("Failed to extract embedding.");

            return vector;
        }

        private float CosineSimilarity(List<float> v1, List<float> v2)
        {
            float dot = 0, mag1 = 0, mag2 = 0;
            for (int i = 0; i < v1.Count; i++)
            {
                dot += v1[i] * v2[i];
                mag1 += v1[i] * v1[i];
                mag2 += v2[i] * v2[i];
            }

            return (float)(dot / (Math.Sqrt(mag1) * Math.Sqrt(mag2)));
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        private class TagEmbedding
        {
            public string Tag { get; set; }
            public List<float> Embedding { get; set; }
        }
    }



    /*
    
    */
}
