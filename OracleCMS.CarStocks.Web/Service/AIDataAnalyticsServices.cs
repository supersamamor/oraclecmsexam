using OracleCMS.CarStocks.ChatGPT.Services;
using Newtonsoft.Json;
namespace OracleCMS.CarStocks.Web.Service
{
    public class AIDataAnalyticsServices(ChatGPTService chatGPTService)
    {
        private static readonly Dictionary<string, string> GPTAssistanceWays = new()
        {
            { "Data Analysis", "Identify trends, patterns, and correlations in your data." },
            { "Predictive Modeling", "Forecast future trends and outcomes based on historical data." },
            { "Segmentation Analysis", "Segment your customer base for targeted marketing strategies." },
            { "Performance Evaluation", "Assess the performance of various aspects of your business." },
            { "Competitive Analysis", "Analyze competitors' data to gain insights and competitive advantage." },
            { "Optimization Strategies", "Develop strategies to optimize processes and resource allocation." },
            { "Customer Insights", "Gain insights into customer preferences, behaviors, and satisfaction." },
            { "Risk Assessment", "Identify potential risks and develop mitigation strategies." },
            { "Decision Support", "Provide data-driven insights to support decision-making." },
            { "Customer Lifetime Value Analysis", "Determine the value of your customers over time." },
            { "Churn Analysis", "Analyze customer churn patterns and implement retention strategies." },
            { "Market Basket Analysis", "Identify product associations for targeted promotions." },
            { "Sentiment Analysis", "Analyze customer feedback to gauge sentiment." },
            { "Supply Chain Optimization", "Optimize inventory management and supply chain operations." },
            { "Quality Control Analysis", "Identify and address quality issues in products or services." },
            { "Marketing ROI Analysis", "Evaluate the effectiveness of marketing campaigns." },
            { "Employee Performance Analysis", "Assess employee productivity and satisfaction." },
            { "Geospatial Analysis", "Analyze geographic trends and regional differences." },
            { "Benchmarking", "Compare your business performance against industry benchmarks." },
            { "Recommendation of Marketing Strategies and Campaigns", "Recommendation of Marketing Strategies and Campaigns." },
            { "Proposed Data Adjustments for Enhanced Insights", "Proposed Data Adjustments for Enhanced Insights." }
        };
        private static readonly string CommaSeparatedGPTAssistanceWays = string.Join(", ", GPTAssistanceWays.Keys);
        public async Task<string?> AIDrivenAnalysis(string reportName, string reportData, string reportColumns, string? customInstruction = null, CancellationToken token = new CancellationToken())
        {
            string minifiedReportData = MinifyJson(reportData);
            string minifiedReportColumns = MinifyJson(reportColumns);
            string chatGptPrompt = $"The report `{reportName}` has the following data in json format: {minifiedReportData}. " +
                $" and report column: {minifiedReportColumns}, respectively. " +
                $" Based on the report details, try to perform each analysis if applicable and give your insights (skip item that are not applicable to the data, disregard column headers' colors): " +
                $": ({CommaSeparatedGPTAssistanceWays}). " +
                $"The output should be itemized and in json format. " +
                $"Do not include additional explanation or descriptions. Only the JSON is needed. ";
            if (!string.IsNullOrEmpty(customInstruction))
            {
                chatGptPrompt += $" And also, {customInstruction}.";
            }
            var messageResult = await chatGPTService.AskChatGPT(chatGptPrompt, token);
            return messageResult?.SanitizedContent;
        }
        public static string MinifyJson(string json)
        {
            var parsedObject = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedObject, Formatting.None);
        }
    }
}
