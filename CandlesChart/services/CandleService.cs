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

        for (var i = 0; i < count; i++)
        {
            var mainCandle = candles[i];
            var newLine = new Line();
            newLine.Candles.Add(mainCandle);

            var points = new BreakData() { };
            var m = 0;
            for (var j = i + 1; j < count; j++)
            {
                m++;
                var childCandle = candles[j];

                if (Math.Round(mainCandle.OpenPrice) == Math.Round(childCandle.OpenPrice))
                {
                    newLine.Candles.Add(childCandle);

                    switch (mainCandle.CandleType)
                    {
                        case CandleType.HighCandle when childCandle.CandleType == CandleType.HighCandle:
                        {
                            newLine.IsBroken = newLine.LineType != null && newLine.LineType != LineType.SupportLine;
                            if (newLine.IsBroken)
                            {
                                Console.WriteLine("Weird State");
                                newLine.BrokenCount++;
                                points.CurrentLineType = newLine.LineType;
                                points.NumberOfCandles = m - 1;
                                newLine.BreakDataList.Add(points);
                                m = 0;
                                points = new BreakData();
                            }

                            newLine.LineType = LineType.SupportLine;
                            newLine.Price = mainCandle.OpenPrice;
                            break;
                        }
                        case CandleType.LowCandle when childCandle.CandleType == CandleType.LowCandle:
                        {
                            newLine.IsBroken = newLine.LineType != null &&
                                               newLine.LineType != LineType.SupportResistance;
                            if (newLine.IsBroken)
                            {
                                Console.WriteLine("Weird State");
                                newLine.BrokenCount++;
                                points.CurrentLineType = newLine.LineType;
                                points.NumberOfCandles = m - 1;
                                newLine.BreakDataList.Add(points);
                                points = new BreakData();
                                m = 0;
                            }

                            newLine.LineType = LineType.SupportResistance;

                            newLine.Price = mainCandle.OpenPrice;
                            break;
                        }
                    }
                }
                else if (Math.Round(mainCandle.OpenPrice) == Math.Round(childCandle.ClosePrice))
                {
                    newLine.Candles.Add(childCandle);

                    if (mainCandle.CandleType == CandleType.HighCandle &&
                        childCandle.CandleType == CandleType.LowCandle)
                    {
                        newLine.IsBroken = newLine.LineType != null && newLine.LineType != LineType.SupportLine;
                        if (newLine.IsBroken)
                        {
                            Console.WriteLine("Weird State");
                            newLine.BrokenCount++;
                            points.CurrentLineType = newLine.LineType;
                            points.NumberOfCandles = m - 1;
                            newLine.BreakDataList.Add(points);
                            points = new BreakData();
                            m = 0;
                        }

                        newLine.LineType = LineType.SupportLine;
                        newLine.Price = mainCandle.OpenPrice;
                    }

                    points.CurrentLineType = newLine.LineType;
                }
                else if (Math.Round(mainCandle.ClosePrice) == Math.Round(childCandle.OpenPrice))
                {
                    newLine.Candles.Add(childCandle);

                    if (mainCandle.CandleType == CandleType.LowCandle &&
                        childCandle.CandleType == CandleType.HighCandle)
                    {
                        newLine.IsBroken = newLine.LineType != null && newLine.LineType != LineType.SupportLine;
                        if (newLine.IsBroken)
                        {
                            Console.WriteLine("Weird State");
                            newLine.BrokenCount++;
                            points.CurrentLineType = newLine.LineType;
                            points.NumberOfCandles = m - 1;
                            newLine.BreakDataList.Add(points);
                            m = 0;
                            points = new BreakData();
                        }

                        newLine.LineType = LineType.SupportLine;
                        newLine.Price = mainCandle.ClosePrice;
                    }

                    points.CurrentLineType = newLine.LineType;
                }
                else if (Math.Round(mainCandle.ClosePrice) == Math.Round(childCandle.ClosePrice))
                {
                    newLine.Candles.Add(childCandle);

                    switch (mainCandle.CandleType)
                    {
                        case CandleType.HighCandle when childCandle.CandleType == CandleType.LowCandle:
                        {
                            newLine.IsBroken = newLine.LineType != null && newLine.LineType != LineType.SupportLine;
                            if (newLine.IsBroken)
                            {
                                Console.WriteLine("Weird State");
                                newLine.BrokenCount++;
                                points.CurrentLineType = newLine.LineType;
                                points.NumberOfCandles = m - 1;
                                newLine.BreakDataList.Add(points);
                                points = new BreakData();
                                m = 0;
                            }

                            newLine.LineType = LineType.SupportLine;
                            newLine.Price = mainCandle.ClosePrice;
                            break;
                        }
                        case CandleType.HighCandle when childCandle.CandleType == CandleType.HighCandle:
                        {
                            newLine.IsBroken = newLine.LineType != null &&
                                               newLine.LineType != LineType.SupportResistance;
                            if (newLine.IsBroken)
                            {
                                newLine.BrokenCount++;
                                points.CurrentLineType = newLine.LineType;
                                points.NumberOfCandles = m - 1;
                                newLine.BreakDataList.Add(points);
                                points = new BreakData();
                                m = 0;
                            }

                            newLine.LineType = LineType.SupportResistance;
                            newLine.Price = mainCandle.ClosePrice;
                            break;
                        }
                    }

                    points.CurrentLineType = newLine.LineType;
                }
                else
                {
                    if (newLine.Price == 0) continue;
                    switch (newLine.LineType)
                    {
                        case LineType.SupportResistance:
                        {
                            if (childCandle.CandleType == CandleType.HighCandle)
                            {
                                if (childCandle.ClosePrice > newLine.Price)
                                {
                                    newLine.BrokenCount++;
                                    newLine.IsBroken = true;
                                    points.CurrentLineType = newLine.LineType;
                                    points.NumberOfCandles = m - 1;
                                    newLine.BreakDataList.Add(points);
                                    m = 0;
                                    points = new BreakData();

                                    newLine.LineType = LineType.SupportLine;
                                }
                            }

                            break;
                        }
                        case LineType.SupportLine when childCandle.CandleType != CandleType.LowCandle:
                        case LineType.SupportLine when !(childCandle.ClosePrice < newLine.Price):
                            continue;
                        case LineType.SupportLine:
                        {
                            newLine.BrokenCount++;
                            newLine.IsBroken = true;

                            points.CurrentLineType = newLine.LineType;
                            points.NumberOfCandles = m - 1;
                            newLine.BreakDataList.Add(points);
                            points = new BreakData();
                            m = 0;
                            newLine.LineType = LineType.SupportResistance;

                            break;
                        }
                    }
                }
            }

            if (newLine.Price == 0) continue;
            if (_lines.Count != 0)
            {
                var oldLine = _lines.SingleOrDefault(o => Math.Round(newLine.Price) == Math.Round(o.Price));


                if (oldLine == null)
                {
                    _lines.Add(newLine);
                }
                else
                {
                    MigrateLines(newLine, oldLine);
                }
            }
            else
            {
                _lines.Add(newLine);
            }
        }

        CalculatePricesBetweenLineBreak();
        FilterDoubleLines();
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

    private void MigrateLines(Line newLine, Line oldLine)
    {
        oldLine.IsBroken = true;
        oldLine.LineType = newLine.LineType;
        var list = newLine.Candles.Where(c => oldLine.Candles.All(cc => c.StartTime != cc.StartTime))
            .ToList();

        oldLine.BrokenCount += oldLine.BrokenCount - newLine.BrokenCount;
        oldLine.Candles.AddRange(list);
    }

    private void CalculatePricesBetweenLineBreak()
    {
        foreach (var line in _lines)
        {
            var firstCandleIndex = _candles.IndexOf(line.Candles[0]);
            for (int i = 0; i < line.BreakDataList.Count; i++)
            {
                var breakData = line.BreakDataList[i];
                CalculatePricesBetweenLineBreak(firstCandleIndex, breakData);
            }

            Console.WriteLine($"Line: {line}");
        }
    }

    private void CalculatePricesBetweenLineBreak(int firstCandleIndex, BreakData breakData)
    {
        for (int i = firstCandleIndex; i < breakData.NumberOfCandles; i++)
        {
            var currentCandle = _candles[i];
            switch (breakData.CurrentLineType)
            {
                case LineType.SupportLine when currentCandle.CandleType == CandleType.HighCandle:
                {
                    var price = currentCandle.ClosePrice;
                    breakData.HighestPrice = price > breakData.HighestPrice
                        ? price
                        : breakData.HighestPrice;
                    break;
                }
                case LineType.SupportResistance when currentCandle.CandleType == CandleType.LowCandle:
                {
                    var price = currentCandle.ClosePrice;
                    breakData.LowestPrice = price > breakData.LowestPrice
                        ? price
                        : breakData.LowestPrice;
                    break;
                }
            }
        }
    }
     private void FilterDoubleLines()
    {
        var lines = new HashSet<Line>();
        Console.WriteLine(_lines.Count);
        for (int i = 0; i < _lines.Count; i++)
        {
            var line = _lines[i];
            for (int j = i + 1; j < _lines.Count; j++)
            {
                bool shouldBreak = false;
                var childLine = _lines[j];


                if (Math.Abs(line.Price - childLine.Price) > 150)

                    continue;

                switch (line.LineType)
                {
                    case LineType.SupportLine when childLine.LineType == LineType.SupportLine:
                    case LineType.SupportResistance when childLine.LineType == LineType.SupportResistance:
                        Console.WriteLine("IN");
                        if (line.Price < childLine.Price)
                        {
                            _lines.RemoveAt(j);
                            foreach (var childCandle in childLine.Candles)
                            {
                                var alreadyAdded = line.Candles.Any(c => c == childCandle);
                                if (alreadyAdded)
                                {
                                    line.Candles.Add(childCandle);
                                }
                            }

                            line.BreakDataList.AddRange(childLine.BreakDataList);
                            lines.Add(line);
                        }
                        else if (line.Price > childLine.Price)
                        {
                            _lines.RemoveAt(i);
                            foreach (var childCandle in line.Candles)
                            {
                                var alreadyAdded = childLine.Candles.Any(c => c == childCandle);
                                if (alreadyAdded)
                                {
                                    childLine.Candles.Add(childCandle);
                                }
                            }

                            lines.Add(childLine);
                            shouldBreak = true;
                        }

                        break;
                }

                if (shouldBreak) break;
            }
        }

        _lines = lines.ToList();
    }
}