using OracleCMS.CarStocks.ChatGPT.Exceptions;
using OracleCMS.CarStocks.ChatGPT.Models;
using OracleCMS.CarStocks.ChatGPT.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
namespace OracleCMS.CarStocks.ChatGPT.Services
{
    public class ChatGPTService(IOptions<OpenAI> openAISettings, HttpClient client)
    {
        private readonly OpenAI _openAISettings = openAISettings.Value;
        public async Task<Message?> AskChatGPT(string chatGptPrompt, CancellationToken token = new CancellationToken())
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAISettings.ApiKey);          
            var chatGPTRequest = new ChatGPTRequest
            {
                Messages = [new Messages() { Role = "user", Content = chatGptPrompt }],
                MaxTokens = _openAISettings.MaxTokens,
                Temparature = _openAISettings.Temparature,
                TopP = _openAISettings.TopP,
                N = _openAISettings.N,
                FrequencyPenalty = _openAISettings.FrequencyPenalty,
                PresencePenalty = _openAISettings.PresencePenalty,
                Model = _openAISettings.Model
            };
            var content = new StringContent(JsonConvert.SerializeObject(chatGPTRequest), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(_openAISettings.ApiUrl, content, token);
            var result = await response.Content.ReadAsStringAsync(token);
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch
            {
                var error = JsonConvert.DeserializeObject<ErrorModel>(result);
                throw new ApiResponseException(error?.Error?.Message, response, result);
            }
            return JsonConvert.DeserializeObject<ChatGPTResult>(result)?.Choices?.FirstOrDefault()?.Message;
        }
      
    }
}
