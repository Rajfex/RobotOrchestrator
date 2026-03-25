using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Playwright;
using Robot.Flashscore;
using Robot.Models;

namespace Robot.Automations
{
    public class Automations
    {
        private readonly string _notificationHubUrl;
        private HubConnection? _hubConnection;

        public Automations(string notificationHubUrl)
        {
            _notificationHubUrl = notificationHubUrl;
        }

        public async Task<List<LeagueResult>> GetLeaguesInfoAsync(List<FootballLeagueInfo> leagueInfos)
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions{});

            var email = new Emails();

            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();

            var allLeaguesData = new List<LeagueResult>();

            try
            {
                await SendNotificationAsync("Flashscore robot fetching data");
                foreach (var leagueInfo in leagueInfos)
                {
                    try
                    {
                        string country = leagueInfo.Country.ToLower();
                        string leagueName = leagueInfo.LeaguseName.ToLower().Replace(" ", "-");
                        var url = $"https://www.flashscore.com/football/{country}/{leagueName}/standings/";

                        await page.GotoAsync(url);

                        var cookieButton = page.Locator("#onetrust-accept-btn-handler");
                        if (await cookieButton.IsVisibleAsync()) await cookieButton.ClickAsync();

                        await page.WaitForSelectorAsync(".ui-table__row");

                        var rows = page.Locator(".ui-table__row");
                        int rowCount = await rows.CountAsync();

                        var leagueResult = new LeagueResult { LeagueName = leagueInfo.LeaguseName };

                        for (int j = 0; j < rowCount; j++)
                        {
                            var currentRow = rows.Nth(j);
                            var teamName = await currentRow.Locator(".tableCellParticipant__name").InnerTextAsync();
                            var cells = currentRow.Locator(".table__cell--value");

                            var team = new TeamStanding
                            {
                                TeamName = teamName,
                                MP = int.Parse(await cells.Nth(0).InnerTextAsync()),
                                W = int.Parse(await cells.Nth(1).InnerTextAsync()),
                                D = int.Parse(await cells.Nth(2).InnerTextAsync()),
                                L = int.Parse(await cells.Nth(3).InnerTextAsync()),
                                Goals = await cells.Nth(4).InnerTextAsync(),
                                RB = int.Parse(await cells.Nth(5).InnerTextAsync()),
                                Pts = int.Parse(await cells.Nth(6).InnerTextAsync())
                            };

                            leagueResult.Teams.Add(team);
                        }

                        if (leagueResult.Teams.Count > 0)
                        {
                            allLeaguesData.Add(leagueResult);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to scrape league '{leagueInfo.LeaguseName}': {ex.Message}");
                    }
                }
                try
                {
                    email.SendEmail("mail@local.com", "Fetched data", "Your data is ready visit orkiestrator robot webiste.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send email: {ex.Message}");
                }

                return allLeaguesData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch leagues data: {ex.Message}");
                return allLeaguesData;
            }
            finally
            {
                await SendNotificationAsync("Flashscore robot finished fetching data");

                if (_hubConnection != null)
                {
                    await _hubConnection.DisposeAsync();
                }

                await browser.CloseAsync();
            }
        }

        private async Task SendNotificationAsync(string message)
        {
            try
            {
                if (_hubConnection == null)
                {
                    _hubConnection = new HubConnectionBuilder()
                        .WithUrl(_notificationHubUrl, options =>
                        {
                            options.HttpMessageHandlerFactory = _ => new HttpClientHandler
                            {
                                ServerCertificateCustomValidationCallback =
                                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                            };
                        })
                        .WithAutomaticReconnect()
                        .Build();
                }

                if (_hubConnection.State == HubConnectionState.Disconnected)
                {
                    await _hubConnection.StartAsync();
                }

                await _hubConnection.InvokeAsync("SendNotification", message);
            }
            catch
            {
            }
        }
    }
}