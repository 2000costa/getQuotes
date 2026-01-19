using Microsoft.VisualBasic;
using Restless.Tiingo.Client;
using Restless.Tiingo.Core;
using Restless.Tiingo.Data;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Uso:");
                Console.WriteLine("dotnet run AAPL MSFT TSLA --start=2022-01-01 --end=2024-01-01");
                return;
            }

            // Extrair argumentos de data
            DateTime? startDate = null;
            DateTime? endDate = null;

            var tickers = args
                .Where(a => !a.StartsWith("--"))
                .Select(t => t.ToUpper())
                .ToList();

            foreach (var arg in args.Where(a => a.StartsWith("--")))
            {
                if (arg.StartsWith("--start="))
                {
                    if (DateTime.TryParse(arg.Replace("--start=", ""), out var dt))
                        startDate = dt;
                }
                else if (arg.StartsWith("--end="))
                {
                    if (DateTime.TryParse(arg.Replace("--end=", ""), out var dt))
                        endDate = dt;
                }
            }

            if (!startDate.HasValue || !endDate.HasValue)
            {
                Console.WriteLine("Intervalo não informado. Usando últimos 2 anos.");
                endDate = DateTime.UtcNow;
                startDate = endDate.Value.AddYears(-2);
            }

            // API Key
            string apiKey = "668563308cd9d663955bf4c2a7a85bc9dc20c0f2";

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Console.WriteLine("Erro: API Key não configurada.");
                return;
            }

            var client =  TiingoClient.Create(apiKey);

            foreach (var ticker in tickers)
            {
                Console.WriteLine($"Buscando dados para {ticker}...");

                try
                {

                    // Get price data points for a ticker
                    TickerDataPointCollection tickerData = await client.Ticker.GetDataPointsAsync(new TickerParameters()
                    {
                        Ticker = "msft",
                        StartDate = startDate,
                        EndDate = endDate,
                        Frequency = FrequencyUnit.Week,
                        FrequencyValue = 1
                    });

                    if (tickerData == null || !tickerData.Any())
                    {
                        Console.WriteLine($"Nenhum dado encontrado para {ticker}.");
                        continue;
                    }

                    string fileName = $"{ticker}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv";
                    SaveToCsv(fileName, tickerData);

                    Console.WriteLine($"CSV gerado: {fileName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro inesperado para {ticker}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro geral: {ex.Message}");
        }
    }

    static void SaveToCsv(string fileName, TickerDataPointCollection prices)
    {
        using (var writer = new StreamWriter(fileName))
        {
            writer.WriteLine("Date;Open;High;Low;Close;Volume");

            foreach (var p in prices)
            {
                writer.WriteLine(
                    $"{p.Date:yyyy-MM-dd};" +
                    $"{p.Open.ToString(CultureInfo.InvariantCulture)};" +
                    $"{p.High.ToString(CultureInfo.InvariantCulture)};" +
                    $"{p.Low.ToString(CultureInfo.InvariantCulture)};" +
                    $"{p.Close.ToString(CultureInfo.InvariantCulture)};" +
                    $"{p.Volume}"
                );
            }
        }
    }
}

