using System.Text.Json;
using CandlesChart.models;
using CandlesChart.models.enums;
using CandlesChart.services.interfaces;

namespace CandlesChart.services;

public class CandleService : ICandleService
{
    private static ICandleService? _instance;

    public static ICandleService GetInstance()
    {
        return _instance ??= new CandleService();
    }

    public List<Candle> Candles { get; private set; } = new();

    public List<Line> Lines { get; private set; } = new();

    public void CreateCandlesFromOldData(string candlesJson)
    {
        var candlesList = JsonSerializer.Deserialize<List<List<object>>>(candlesJson);
        Candles = new List<Candle>();
        foreach (var candle in candlesList)
        {
            Candles.Add(ListToCandle(candle));
        }
    }

    public void Analyse()
    {
        var candles = Candles;
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
            var shouldAdd = true;
            for (var j = i + 1; j < count; j++)
            {
                if (newLine != null && shouldAdd && newLine.Price != 0)
                {
                    var tmpLines = Lines.Where(l =>
                            Math.Abs(Math.Round(l.Price) - Math.Abs(newLine.Price)) < 150 &&
                            l.LineType == newLine.LineType)
                        .ToList();
                    switch (tmpLines.Count)
                    {
                        case 0:
                            break;
                        case 1:

                            shouldAdd = false;
                            newLine = tmpLines[0];
                            break;
                        default:
                            shouldAdd = false;
                            newLine = tmpLines.MinBy(l => l.Price);
                            foreach (var tmpLine in tmpLines.Where(tmpLine => tmpLine != newLine))
                            {
                                //Can't Add Old Breaks Of All Lines Bcz it will leak of data
                                newLine?.Candles.AddRange(tmpLine.Candles);
                                
                                Lines.RemoveAt(Lines.IndexOf(tmpLine));
                            }

                            break;
                    }
                }

                m++;
                var childCandle = candles[j];

                if (Math.Abs(Math.Round(mainCandle.OpenPrice) - Math.Round(childCandle.OpenPrice)) < 100)
                {
                    newLine?.Candles.Add(childCandle);

                    switch (mainCandle.CandleType)
                    {
                        case CandleType.HighCandle when childCandle.CandleType == CandleType.HighCandle:
                        {
                            newLine.IsBroken = newLine.LineType != null && newLine.LineType != LineType.SupportLine;
                            if (newLine.IsBroken)
                            {
                                newLine.BrokenCount++;
                                points.CurrentLineType = newLine.LineType;
                                points.NumberOfCandles = m - 1;
                                newLine.BreakDataList.Add(points);
                                m = 0;
                                points = new BreakData();
                            }

                            newLine.LineType = LineType.SupportLine;
                            if (shouldAdd)
                                newLine.Price = mainCandle.OpenPrice;
                            break;
                        }
                        case CandleType.LowCandle when childCandle.CandleType == CandleType.LowCandle:
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
                            if (shouldAdd)
                                newLine.Price = mainCandle.OpenPrice;
                            break;
                        }
                    }
                }
                else if (Math.Abs(Math.Round(mainCandle.OpenPrice) - Math.Round(childCandle.ClosePrice)) < 100)
                {
                    newLine.Candles.Add(childCandle);

                    if (mainCandle.CandleType == CandleType.HighCandle &&
                        childCandle.CandleType == CandleType.LowCandle)
                    {
                        newLine.IsBroken = newLine.LineType != null && newLine.LineType != LineType.SupportLine;
                        if (newLine.IsBroken)
                        {
                           
                            newLine.BrokenCount++;
                            points.CurrentLineType = newLine.LineType;
                            points.NumberOfCandles = m - 1;
                            newLine.BreakDataList.Add(points);
                            points = new BreakData();
                            m = 0;
                        }

                        newLine.LineType = LineType.SupportLine;
                        if (shouldAdd)
                            newLine.Price = mainCandle.OpenPrice;
                    }

                    points.CurrentLineType = newLine.LineType;
                }
                else if (Math.Abs(Math.Round(mainCandle.ClosePrice) - Math.Round(childCandle.OpenPrice)) < 100)
                {
                    newLine.Candles.Add(childCandle);

                    if (mainCandle.CandleType == CandleType.LowCandle &&
                        childCandle.CandleType == CandleType.HighCandle)
                    {
                        newLine.IsBroken = newLine.LineType != null && newLine.LineType != LineType.SupportLine;
                        if (newLine.IsBroken)
                        {
                           
                            newLine.BrokenCount++;
                            points.CurrentLineType = newLine.LineType;
                            points.NumberOfCandles = m - 1;
                            newLine.BreakDataList.Add(points);
                            m = 0;
                            points = new BreakData();
                        }

                        newLine.LineType = LineType.SupportLine;
                        if (shouldAdd)
                            newLine.Price = mainCandle.ClosePrice;
                    }

                    points.CurrentLineType = newLine.LineType;
                }
                else if (Math.Abs(Math.Round(mainCandle.ClosePrice) - Math.Round(childCandle.ClosePrice)) < 100)
                {
                    newLine.Candles.Add(childCandle);

                    switch (mainCandle.CandleType)
                    {
                        case CandleType.HighCandle when childCandle.CandleType == CandleType.LowCandle:
                        {
                            newLine.IsBroken = newLine.LineType != null && newLine.LineType != LineType.SupportLine;
                            if (newLine.IsBroken)
                            {
                               
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
                            if (shouldAdd)
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

            if (!shouldAdd)
                continue;

            if (newLine.Price == 0) continue;
            if (Lines.Count != 0)
            {
                var oldLine =
                    Lines.SingleOrDefault(o =>
                        Math.Abs(Math.Round(newLine.Price) - Math.Round(o.Price)) < 150 &&
                        newLine.LineType == o.LineType);


                if (oldLine == null)
                {
                    Lines.Add(newLine);
                }
                else
                {
                    MigrateLines(newLine, oldLine);
                }
            }
            else
            {
                Lines.Add(newLine);
            }
        }

        CalculatePricesBetweenLineBreak();
        // FilterDoubleLines();
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
        foreach (var line in Lines)
        {
            var firstCandleIndex = Candles.IndexOf(line.Candles[0]);
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
            var currentCandle = Candles[i];
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
        Console.WriteLine(Lines.Count);
        for (int i = 0; i < Lines.Count; i++)
        {
            var line = Lines[i];
            for (int j = i + 1; j < Lines.Count; j++)
            {
                bool shouldBreak = false;
                var childLine = Lines[j];


                if (Math.Abs(line.Price - childLine.Price) > 150)

                    continue;

                switch (line.LineType)
                {
                    case LineType.SupportLine when childLine.LineType == LineType.SupportLine:
                    case LineType.SupportResistance when childLine.LineType == LineType.SupportResistance:
                        Console.WriteLine("IN");
                        if (line.Price < childLine.Price)
                        {
                            Lines.RemoveAt(j);
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
                            Lines.RemoveAt(i);
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

        Lines = lines.ToList();
    }
}