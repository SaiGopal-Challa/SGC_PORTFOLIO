using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace SGC_PORTFOLIO.Services
{
    public class GenAiClient_Old : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _openAiApiKey;
        private const string OpenAiEndpoint = "https://api.openai.com/v1/chat/completions";

        public GenAiClient_Old(IConfiguration configuration)
        {
            _openAiApiKey = configuration["OpenAI:ApiKey"];
            if (string.IsNullOrWhiteSpace(_openAiApiKey))
                throw new ArgumentException("OpenAI:ApiKey is missing in configuration.");

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAiApiKey);
        }

        public async Task<(string summary, List<string> ageTags, List<string> expTags, List<string> topicTags)> AnalyzePdfTextAsync(string rawText)
        {
            var systemPrompt = "You are a PDF analysis assistant. Given raw extracted text from a PDF, produce a JSON object with exactly these fields: \"summary\" (a concise paragraph), \"ageTags\" (array of strings), \"expTags\" (array of strings), \"topicTags\" (array of strings). Respond with JUST the JSON. Note: ageTags - min age to read pdf, to filter 18+, topicTags must be standard topic names which the pdf is talking about, 12 tags must be given";
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
            using var request = new HttpRequestMessage(HttpMethod.Post, OpenAiEndpoint)
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
            var ageTags = parsed["ageTags"]?.ToObject<List<string>>() ?? new List<string>();
            var expTags = parsed["expTags"]?.ToObject<List<string>>() ?? new List<string>();
            var topicTags = parsed["topicTags"]?.ToObject<List<string>>() ?? new List<string>();

            return (summary, ageTags, expTags, topicTags);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
