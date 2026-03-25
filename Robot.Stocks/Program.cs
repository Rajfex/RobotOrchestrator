using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Orchestrator.Core.Data;
using Robot.Stocks.Models;

namespace Robot.Stocks
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var baseUrl = "https://localhost:7028/";
            var notificationHubUrl = "https://localhost:7006/notifications";
            var apiKey = "DWNvDCd4jUiWLe2v3MA9tA";

            var connectionString = "Server=(localdb)\\mssqllocaldb;Database=OrchestratorDb;Trusted_Connection=True;";
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            using var context = new AppDbContext(optionsBuilder.Options);
            var _logger = new Orchestrator.Core.Logger(context);

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            using var http = new HttpClient(handler)
            {
                BaseAddress = new Uri(baseUrl)
            };

            _logger.Log(Guid.Empty, "[ROBOT] Robot started", 1);

            while (true)
            {
                try
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, "api/robots/next-task");
                    request.Headers.Add("X-ApiKey", apiKey);
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    using var resp = await http.SendAsync(request);

                    if (resp.StatusCode == HttpStatusCode.NoContent)
                    {
                        Console.WriteLine($"{DateTime.Now}: No tasks");
                    }
                    else if (resp.IsSuccessStatusCode)
                    {
                        var body = await resp.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(body);
                        var root = doc.RootElement;
                        var taskId = root.GetProperty("id").GetGuid();
                        var inputData = root.GetProperty("inputData");
                        string stocksJson;

                        if (inputData.ValueKind == JsonValueKind.String)
                        {
                            stocksJson = inputData.GetString();
                        }
                        else
                        {
                            stocksJson = inputData.GetRawText();
                        }
                        var data = JsonSerializer.Deserialize<Dictionary<string, List<Stock>>>(stocksJson);
                        Stock[] stockArray = data["StocksInfo"].ToArray();
                        var automations = new Automations(notificationHubUrl);
                        var output = await automations.GetStockData(stockArray.ToList());

                        var outputJson = JsonNode.Parse(JsonSerializer.Serialize(output));
                        var resultBody = new JsonObject
                        {
                            ["outputData"] = outputJson,
                            ["statusId"] = 3
                        };

                        using var resultRequest = new HttpRequestMessage(HttpMethod.Post, $"api/tasks/{taskId}/results")
                        {
                            Content = new StringContent(resultBody.ToJsonString(), Encoding.UTF8, "application/json")
                        };

                        using var resultResponse = await http.SendAsync(resultRequest);

                        var statusBody = JsonSerializer.Serialize(new { statusId = 3 });
                        using var statusRequest = new HttpRequestMessage(HttpMethod.Patch, $"api/tasks/{taskId}/status")
                        {
                            Content = new StringContent(statusBody, Encoding.UTF8, "application/json")
                        };
                        using var statusResponse = await http.SendAsync(statusRequest);

                        Console.WriteLine($"Task {taskId} completed");
                        _logger.Log(taskId, "[Robot] Task completed", 1);

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                    _logger.Log(Guid.Empty, $"[Robot] Error: {ex.Message}", 3);
                }

                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }
}
