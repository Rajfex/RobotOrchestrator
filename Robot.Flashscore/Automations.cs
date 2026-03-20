using Robot.Models;
using Microsoft.Playwright;
using Robot.Models;
using static Robot.Flashscore.Program;

namespace Robot.Automations
{
    public static class Automations
    {
        public static async Task<List<LeagueResult>> GetLeaguesInfoAsync(List<FootballLeagueInfo> leagueInfos)
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions{});

            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();

            var allLeaguesData = new List<LeagueResult>();

            try
            {
                foreach (var leagueInfo in leagueInfos)
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

                    allLeaguesData.Add(leagueResult);
                }

                return allLeaguesData;
            }
            catch (Exception ex)
            {
                return new List<LeagueResult>();
            }
            finally
            {
                await browser.CloseAsync();
            }
        }
    }
}