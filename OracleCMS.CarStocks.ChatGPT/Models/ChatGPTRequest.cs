using Newtonsoft.Json;
namespace OracleCMS.CarStocks.ChatGPT.Models
{
    public class ChatGPTRequest
    {
        [JsonProperty("messages")]
        public List<Messages> Messages { get; set; } = new();
        [JsonProperty("max_tokens")]
        public int MaxTokens { get; set; }
        [JsonProperty("temperature")]
        public decimal Temparature { get; set; }
        [JsonProperty("top_p")]
        public decimal TopP { get; set; }
        [JsonProperty("n")]
        public int N { get; set; }
        [JsonProperty("model")]
        public string Model { get; set; } = "";
        [JsonProperty("frequency_penalty")]
        public decimal FrequencyPenalty { get; set; }
        [JsonProperty("presence_penalty")]
        public decimal PresencePenalty { get; set; } 
    }
    public class Messages
    {
        [JsonProperty("role")]
        public string Role { get; set; } = "";
        [JsonProperty("content")]
        public string Content { get; set; } = "";
    }
}
