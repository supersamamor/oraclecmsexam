using OracleCMS.CarStocks.ChatGPT.Services;
using OracleCMS.CarStocks.Core.Identity;
using OracleCMS.CarStocks.Core.Oidc;
using OracleCMS.CarStocks.Core.CarStocks;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Text.RegularExpressions;
namespace OracleCMS.CarStocks.Web.Service
{
    public class AIReportQueryGenerationServices(ChatGPTService chatGPTService, IConfiguration configuration)
    {
        private static readonly List<string> ExcludedTables =
		[
			SanitizeTableName(nameof(ReportState)),
			SanitizeTableName(nameof(ReportQueryFilterState)),
			SanitizeTableName(nameof(ReportAIIntegrationState)),
			SanitizeTableName(nameof(ReportRoleAssignmentState)),
			SanitizeTableName(nameof(UploadProcessorState)),
			SanitizeTableName(nameof(ApprovalState)),
			SanitizeTableName(nameof(ApprovalRecordState)),
			SanitizeTableName(nameof(ApproverSetupState)),
			SanitizeTableName(nameof(ApproverAssignmentState)),
			"__EFMigrationsHistory",
			"AspNetRoleClaims",
			"AspNetUserClaims",
			"AspNetUserLogins",
			"AspNetUserTokens",
			"AuditLogs",
			"Entities",
			"OpenIddictApplications",
			"OpenIddictAuthorizations",
			"OpenIddictScopes",
			"OpenIddictTokens",
		];
		private static string SanitizeTableName(string input)
		{
			const string suffix = "State";
			if (input.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
			{
				return input.Substring(0, input.Length - suffix.Length);
			}
			return input;
		}
        public async Task<string?> SQLReportQueryGeneration(string reportDescription, string reportOrChartType, CancellationToken token = new CancellationToken())
        {
            string minifiedDatabaseStructure = MinifyJson(await GetDatabaseStructureInJsonAsync());
            var chatGptPrompt = $"The ff. is the description of the report; {reportDescription}. " +
                 $" Based on the description that I provided and the ff. database structure in json format : {minifiedDatabaseStructure}," +
                 $" please create a ms sql query. ";
            switch (reportOrChartType)
            {
                case Core.Constants.ReportChartType.HorizontalBar:
                    chatGptPrompt += Core.Constants.ReportChartType.HorizontalBarQueryFormat;
                    break;
                case Core.Constants.ReportChartType.Pie:
                    chatGptPrompt += Core.Constants.ReportChartType.PieQueryFormat;
                    break;
            }
            chatGptPrompt += $" Do not include additional explanation or descriptions. Only the SQL Script is needed. ";
            var messageResult = await chatGPTService.AskChatGPT(chatGptPrompt, token);
            return BeautifySQL(messageResult?.SanitizedContent);
        }
        private static string MinifyJson(string json)
        {
            var parsedObject = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedObject, Formatting.None);
        }
        private static string BeautifySQL(string? sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return "";
            }
            // Define SQL keywords
            string[] keywords = { "SELECT", "FROM", "WHERE", "ORDER BY", "GROUP BY", "INSERT INTO", "VALUES", "UPDATE", "SET", "DELETE FROM" };
            // Capitalize keywords and handle line breaks
            string result = sql;
            foreach (string keyword in keywords)
            {
                string regex = "\\b" + keyword + "\\b";
                result = Regex.Replace(result, regex, keyword, RegexOptions.IgnoreCase);
                result = Regex.Replace(result, regex, "\n" + keyword);
            }
            // Handle indentation (basic example)
            int indentLevel = 0;
            string[] lines = result.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.StartsWith("FROM") || line.StartsWith("WHERE") || line.StartsWith("ORDER BY") || line.StartsWith("GROUP BY"))
                {
                    lines[i] = new String(' ', 4 * indentLevel) + line;
                    indentLevel++;
                }
                else if (line.StartsWith("SELECT") || line.StartsWith("UPDATE") || line.StartsWith("INSERT INTO") || line.StartsWith("DELETE FROM"))
                {
                    indentLevel = 1;
                    lines[i] = line;
                }
                else
                {
                    lines[i] = new String(' ', 4 * indentLevel) + line;
                }
            }
            return String.Join("\n", lines);
        }
        public async Task<string> GetDatabaseStructureInJsonAsync()
        {
            using SqlConnection connection = new(configuration.GetConnectionString("ReportContext"));
            connection.Open();
            DataTable tables = connection.GetSchema("Tables");
            List<object> databaseSchema = [];
            foreach (DataRow row in tables.Rows)
            {
                string tableName = (string)row["TABLE_NAME"];
                //ignore case sensitivity   
                if (ExcludedTables.Contains(tableName)) 
                {
                    continue;
                }
                using SqlCommand command = new($"SELECT TOP 0 * FROM [{tableName}]", connection);
                using SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.KeyInfo);
                DataTable? schemaTable = await reader.GetSchemaTableAsync();
                var tableSchema = new
                {
                    TableName = tableName,
                    Columns = new List<object>()
                };
                if (schemaTable != null)
                {
                    foreach (DataRow schemaRow in schemaTable.Rows)
                    {
                        tableSchema.Columns.Add(new
                        {
                            ColumnName = schemaRow["ColumnName"],
                            DataType = schemaRow["DataType"].ToString()
                        });
                    }
                    databaseSchema.Add(tableSchema);
                }
            }
            return JsonConvert.SerializeObject(databaseSchema, Formatting.Indented);
        }
    }
}
