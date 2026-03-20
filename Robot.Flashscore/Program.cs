using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Orchestrator.Core;
using Orchestrator.Core.Data;
using Robot.Models;

namespace Robot.Flashscore
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            var baseUrl = "https://localhost:7028/";
            var apiKey = "NicutsVs0aF8Tm7szH8vA";

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

                        string leaguesJson;

                        if (inputData.ValueKind == JsonValueKind.String)
                        {
                            using var inputDataDoc = JsonDocument.Parse(inputData.GetString());
                            leaguesJson = inputDataDoc.RootElement.GetProperty("FootballLeagueInfo").GetRawText();
                        }
                        else
                        {
                            leaguesJson = inputData.GetProperty("FootballLeagueInfo").GetRawText();
                        }

                        var leagues = JsonSerializer.Deserialize<List<FootballLeagueInfo>>(leaguesJson,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                        if (leagues == null || leagues.Count == 0)
                        {
                            Console.WriteLine("No data");
                            _logger.Log(taskId, "No data provided for the task", 2);
                            continue;
                        }

                        Console.WriteLine($"{DateTime.Now}: Data collected:");
                        _logger.Log(taskId, $"[ROBOT] Data collected", 1);
                        var result = await Automations.Automations.GetLeaguesInfoAsync(leagues);

                        var output = JsonNode.Parse(JsonSerializer.Serialize(result));
                        var resultBody = new JsonObject
                        {
                            ["outputData"] = output,
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
                        _logger.Log(taskId, "[ROBOT] Task completed", 1);

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                    _logger.Log(Guid.Empty, $"[ROBOT] Exception: {ex.Message}", 3);
                }

                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }
}