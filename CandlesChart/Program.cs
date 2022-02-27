// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using CandlesChart;
using CandlesChart.models;
using CandlesChart.services;


//Get the data  
{
    var client = new HttpClient(new HttpClientHandler());
    HttpResponseMessage response =
        await client.GetAsync("https://api.binance.com/api/v3/klines?symbol=BTCUSDT&interval=4h");
    var result = await response.Content.ReadAsStringAsync();


    var service = new CandleService();
    service.CreateCandlesFromOldData(result);
    service.Analyse();
    new WriteDataToStream(new FileStream("../../../candles.json", FileMode.Truncate, FileAccess.Write), service.Candles,
        service.Lines).WriteData(null);
    int count = 1;
    foreach (var line in service.Lines.Where(l => l.BrokenCount < 10))
    {
        Console.WriteLine($"Lin {count++}: {line}");
    }
}