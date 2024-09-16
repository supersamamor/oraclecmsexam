using Newtonsoft.Json;
namespace OracleCMS.CarStocks.ChatGPT.Models
{
    public class Choice
    {
        [JsonProperty("index")]
        public int Index { get; set; }
        [JsonProperty("message")]
        public Message Message { get; set; } = new();
    }
    public class Message
    {
        [JsonProperty("role")]
        public string Role { get; set; } = "";
        [JsonProperty("content")]
        public string Content { get; set; } = "";
        public string SanitizedContent {
            get {
                return Content.Replace("```sql", "").Replace("```", "")
                    .Replace("```json", "").Replace("```", "").Replace("json\n", "");
            }        
        }
    }
    public class ChatGPTResult
    {
        [JsonProperty("id")]
        public string Id { get; set; } = "";
        [JsonProperty("object")]
        public string Object { get; set; } = "";
        [JsonProperty("created")]
        public int Created { get; set; }
        [JsonProperty("model")]
        public string Model { get; set; } = "";
        [JsonProperty("choices")]
        public List<Choice>? Choices { get; set; }
        [JsonProperty("usage")]
        public Usage? Usage { get; set; }
    }

    public class Usage
    {
        [JsonProperty("prompt_tokens")]
        public int PromptTokens { get; set; }
        [JsonProperty("completion_tokens")]
        public int CompletionToken { get; set; }
        [JsonProperty("total_tokens")]
        public int TotalTokens { get; set; }
    }
}
