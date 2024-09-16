namespace OracleCMS.CarStocks.ChatGPT.Settings
{   
    public class OpenAI
    {
        public string ApiKey { get; set; } = "";
        public string ApiUrl { get; set; } = "";
        public int MaxTokens { get; set; } 
        public decimal Temparature { get; set; }
        public decimal TopP { get; set; } 
        public int N { get; set; }
        public string Model { get; set; }
        public decimal FrequencyPenalty { get; set; }
        public decimal PresencePenalty { get; set; }
    }
}
