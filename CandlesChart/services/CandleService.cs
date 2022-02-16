using System.Text.Json;
using CandlesChart.models;
using CandlesChart.services.interfaces;

namespace CandlesChart.services;

public class CandleService : ICandleService
{
    private static ICandleService? _instance;

    public static ICandleService GetInstance()
    {
        return _instance ??= new CandleService();
    }

    private List<Candle> _candles = new();
    private List<Line> _lines = new();

    public List<Candle> Candles => _candles;

    public List<Line> Lines => _lines;

    public void CreateCandlesFromOldData(string candlesJson)
    {
        var candlesList = JsonSerializer.Deserialize<List<List<object>>>(candlesJson);
        _candles = new List<Candle>();
        foreach (var candle in candlesList)
        {
            _candles.Add(ListToCandle(candle));
        }
    }

    public void CreateLines()
    {
        var candles = _candles;
        var lastCandle = candles[^1];
        var count = candles.Count;
        if (lastCandle.EndTime < DateTime.Now.Millisecond)
        {
            count--;
        }

        for (int i = 0; i < count; i++)
        {
            var mainCandle = candles[i];
            var newLine = new Line();
            newLine.Candles.Add(mainCandle);
            Console.WriteLine("Main Candle: " + mainCandle);
            for (int j = i + 1; j < count; j++)
            {
                var childCandle = candles[j];

                if (Math.Round(mainCandle.OpenPrice) == Math.Round(childCandle.OpenPrice))
                {
                    Console.WriteLine("\t\t Match On Open Price, Candle: " + childCandle);
                    if (mainCandle.CandleType == CandleType.HighCandle &&
                        childCandle.CandleType == CandleType.HighCandle)
                    {
                        newLine.IsBroken = newLine.LineType != null && newLine.LineType != LineType.SupportLine;
                        if (newLine.IsBroken) newLine.BrokenCount++;
                        newLine.LineType = LineType.SupportLine;
                        newLine.Price = mainCandle.OpenPrice;
                        newLine.Candles.Add(childCandle);
                    }
                    else if (mainCandle.CandleType == CandleType.LowCandle &&
                             childCandle.CandleType == CandleType.LowCandle)
                    {
                        newLine.IsBroken = newLine.LineType != null && newLine.LineType != LineType.SupportResistance;
                        newLine.LineType = LineType.SupportResistance;
                        if (newLine.IsBroken) newLine.BrokenCount++;
                        newLine.Price = mainCandle.OpenPrice;
                        newLine.Candles.Add(childCandle);
                    }
                }
                else if (Math.Round(mainCandle.OpenPrice) == Math.Round(childCandle.ClosePrice))
                {
                    Console.WriteLine("\t\t Match On open in main and open in close, Candle: " + childCandle);
                    if (mainCandle.CandleType == CandleType.HighCandle &&
                        childCandle.CandleType == CandleType.LowCandle)
                    {
                        newLine.IsBroken = newLine.LineType != null && newLine.LineType != LineType.SupportLine;
                        newLine.LineType = LineType.SupportLine;
                        newLine.Price = mainCandle.OpenPrice;
                        newLine.Candles.Add(childCandle);
                        if (newLine.IsBroken) newLine.BrokenCount++;
                    }
                }
                else if (Math.Round(mainCandle.ClosePrice) == Math.Round(childCandle.OpenPrice))
                {
                    Console.WriteLine("\t\t Match On Close in main and open in child, Candle: " + childCandle);
                    if (mainCandle.CandleType == CandleType.LowCandle &&
                        childCandle.CandleType == CandleType.HighCandle)
                    {
                        newLine.IsBroken = newLine.LineType != null && newLine.LineType != LineType.SupportLine;
                        newLine.LineType = LineType.SupportLine;
                        newLine.Price = mainCandle.ClosePrice;
                        newLine.Candles.Add(childCandle);
                        if (newLine.IsBroken) newLine.BrokenCount++;
                    }
                }
                else if (Math.Round(mainCandle.ClosePrice) == Math.Round(childCandle.ClosePrice))
                {
                    Console.WriteLine("\t\t Match On Close Price, Candle: " + childCandle);
                    if (mainCandle.CandleType == CandleType.HighCandle &&
                        childCandle.CandleType == CandleType.LowCandle)
                    {
                        newLine.IsBroken = newLine.LineType != null && newLine.LineType != LineType.SupportLine;
                        newLine.LineType = LineType.SupportLine;
                        newLine.Price = mainCandle.ClosePrice;
                        newLine.Candles.Add(childCandle);
                        if (newLine.IsBroken) newLine.BrokenCount++;
                    }
                    else if (mainCandle.CandleType == CandleType.HighCandle &&
                             childCandle.CandleType == CandleType.HighCandle)
                    {
                        newLine.IsBroken = newLine.LineType != null && newLine.LineType != LineType.SupportResistance;
                        newLine.LineType = LineType.SupportResistance;
                        newLine.Price = mainCandle.ClosePrice;
                        newLine.Candles.Add(childCandle);
                        if (newLine.IsBroken) newLine.BrokenCount++;
                    }
                }
                else
                {
                    if (newLine.Price != null)
                    {
                        if (newLine.LineType == LineType.SupportResistance)
                        {
                            if (childCandle.CandleType == CandleType.HighCandle)
                            {
                                if (childCandle.ClosePrice > newLine.Price)
                                {
                                    newLine.LineType = LineType.SupportLine;
                                    newLine.IsBroken = true;
                                }
                            }
                        }
                        else if (newLine.LineType == LineType.SupportLine)
                        {
                            if (childCandle.CandleType == CandleType.LowCandle)
                            {
                                if (childCandle.ClosePrice < newLine.Price)
                                {
                                    newLine.LineType = LineType.SupportResistance;
                                    newLine.IsBroken = true;
                                }
                            }
                        }
                    }
                }
            }

            if (newLine.Candles.Count > 1)
            {
                var oldLine = _lines.SingleOrDefault(oldLine =>
                    Math.Round((float) newLine.Price) == Math.Round((float) oldLine.Price));
                if (oldLine == null)
                {
                    _lines.Add(newLine);
                }
                else
                {
                    oldLine.IsBroken = true;
                    oldLine.LineType = newLine.LineType;
                    var list = newLine.Candles.Where(c => oldLine.Candles.All(cc => c.StartTime != cc.StartTime))
                        .ToList();
                    oldLine.BrokenCount += oldLine.BrokenCount - newLine.BrokenCount;
                    oldLine.Candles.AddRange(list);
                }
            }

            Console.WriteLine();
        }
    }

    public Candle? CreateCandleBasedOnNewData(string json)
    {
        var c = JsonSerializer.Deserialize<Candle>(json);
        if (c == null) return null;

        c.CandleType = c.OpenPrice < c.ClosePrice ? CandleType.HighCandle : CandleType.LowCandle;
        return c;
    }

    private Candle ListToCandle(List<object> candle)
    {
        var c = new Candle
        {
            StartTime = long.Parse(((JsonElement) candle[0]).ToString()),
            OpenPrice = float.Parse(((JsonElement) candle[1]).ToString()),
            HighestPrice = float.Parse(((JsonElement) candle[2]).ToString()),
            LowestPrice = float.Parse(((JsonElement) candle[3]).ToString()),
            ClosePrice = float.Parse(((JsonElement) candle[4]).ToString()),
            EndTime = ((JsonElement) candle[6]).GetInt64(),
            NumberOfTrades = ((JsonElement) candle[8]).GetInt32(),
        };
        c.CandleType = c.OpenPrice < c.ClosePrice ? CandleType.HighCandle : CandleType.LowCandle;

        return c;
    }
}