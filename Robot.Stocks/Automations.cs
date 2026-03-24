using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Playwright;
using Robot.Flashscore;
using Robot.Stocks.Models;

namespace Robot.Stocks
{
    public class Automations
    {
        private readonly string _notificationHubUrl;
        private HubConnection? _hubConnection;

        public Automations(string notificationHubUrl)
        {
            _notificationHubUrl = notificationHubUrl;
        }
        public async Task<List<StockData>> GetStockData(List<Stock> stockInfo)
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions{});

            var mail = new Emails();

            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();
            var stockInfoData = new List<StockData>();

            try
            {
                await SendNotificationAsync("Stocks robot fetching data");
                await page.GotoAsync("https://www.bankier.pl/gielda/notowania/");
                await AcceptCookiesIfVisible(page);

                foreach (var stock in stockInfo)
                {
                    var listUrl = GetListUrl(stock.Type);

                    await page.GotoAsync(listUrl);

                    var stockButton = page.Locator($"a:has-text('{stock.Name}')").First;
                    await stockButton.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 8000 });
                    await stockButton.ClickAsync();

                    var periodButton = page.GetByText(stock.Period).First;
                    if (await periodButton.CountAsync() > 0)
                    {
                        await periodButton.ClickAsync();
                    }

                    await page.Locator("div.m-quotes-metric-table").First
                        .WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });

                    var stockData = await ParseMetricTable(page, stock.Type, stock.Name);
                    if (stockData != null)
                    {
                        stockInfoData.Add(stockData);
                    }
                }
                mail.SendEmail("mail@local.html", "Fetched data", "Your data is ready visit orkiestrator robot webiste.");
                await SendNotificationAsync("Stocks robot finished fetching data");
                return stockInfoData;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return stockInfoData;
            }


        }

        private static async Task AcceptCookiesIfVisible(IPage page)
        {
            var cookies = page.Locator("span:has-text('Zaakceptuj')").First;
            if (await cookies.CountAsync() > 0)
            {
                try
                {
                    await cookies.ClickAsync(new LocatorClickOptions { Timeout = 3000 });
                }
                catch
                {
                }
            }
        }

        private static string GetListUrl(string type)
        {
            if (string.Equals(type, "akcje", StringComparison.OrdinalIgnoreCase))
            {
                return "https://www.bankier.pl/gielda/notowania/akcje";
            }

            if (string.Equals(type, "indeksy", StringComparison.OrdinalIgnoreCase))
            {
                return "https://www.bankier.pl/gielda/notowania/indeksy";
            }

            return null;
        }

        private static async Task<StockData> ParseMetricTable(IPage page, string type, string name)
        {
            var stockData = new StockData
            {
                Type = type,
                Name = name
            };

            var items = page.Locator("div.m-quotes-metric-table ul.m-quotes-data-list li.m-quotes-data-list__item");
            var count = await items.CountAsync();

            for (int i = 0; i < count; i++)
            {
                var item = items.Nth(i);

                var nameLocator = item.Locator("span.m-quotes-data-list__name");
                var contentLocator = item.Locator("span.m-quotes-data-list__content");
                var dateLocator = item.Locator("span.m-quotes-data-list__date");

                if (await nameLocator.CountAsync() == 0 || await contentLocator.CountAsync() == 0)
                {
                    continue;
                }

                var metricName = (await nameLocator.InnerTextAsync()).Trim();
                var metricValue = (await contentLocator.InnerTextAsync()).Trim();
                var metricDate = await dateLocator.CountAsync() > 0
                    ? (await dateLocator.InnerTextAsync()).Trim()
                    : null;

                switch (metricName)
                {
                    case "Kurs odniesienia:":
                    case "Kurs odniesienia":
                        stockData.ReferencePrice = metricValue;
                        stockData.ReferencePriceDate = metricDate;
                        break;
                    case "Data początkowa":
                        stockData.StartDate = metricValue;
                        break;
                    case "Data końcowa":
                        stockData.EndDate = metricValue;
                        break;
                    case "Zmiana proc.":
                        stockData.ChangePercent = metricValue;
                        break;
                    case "Zmiana":
                        stockData.Change = metricValue;
                        break;
                    case "Minimum":
                        stockData.Min = metricValue;
                        stockData.MinDate = metricDate;
                        break;
                    case "Maksimum":
                        stockData.Max = metricValue;
                        stockData.MaxDate = metricDate;
                        break;
                    case "Średni kurs":
                        stockData.AveragePrice = metricValue;
                        break;
                    case "Wolumen obrotu":
                        stockData.Volume = metricValue;
                        break;
                    case "Średni wolumen":
                        stockData.AverageVolume = metricValue;
                        break;
                    case "Wartość obrotu":
                        stockData.Turnover = metricValue;
                        break;
                    case "Średnie obroty":
                        stockData.AverageTurnover = metricValue;
                        break;
                }
            }

            return stockData;
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