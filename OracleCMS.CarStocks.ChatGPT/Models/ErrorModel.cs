using Newtonsoft.Json;
namespace OracleCMS.CarStocks.ChatGPT.Models
{
    public class ErrorModel
    {
        [JsonProperty("error")]
        public ErrorDetail? Error { get; set;} 
    }
    public class ErrorDetail
    {
        [JsonProperty("message")]
        public string Message { get; set; } = "";
        [JsonProperty("type")]
        public decimal Type { get; set; }
        [JsonProperty("param")]
        public decimal Parameter { get; set; }
        [JsonProperty("code")]
        public decimal Code { get; set; }
    }
}
