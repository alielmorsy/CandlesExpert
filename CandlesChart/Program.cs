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
    //var result = await File.ReadAllTextAsync("json");

    var service = new CandleService();
    service.CreateCandlesFromOldData(result);
    service.CreateLines();
}