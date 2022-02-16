using System.Text.Json;
using CandlesChart.models;

namespace CandlesChart;

public class Utils
{
    public static Candle ListToCandle(List<object> candle)
    {
        var c = new Candle()
        {
            StartTime = long.Parse(((JsonElement)candle[0]).ToString()),
            OpenPrice = float.Parse(((JsonElement)candle[1]).ToString()),
            HighestPrice = float.Parse(((JsonElement)candle[2]).ToString()),
            LowestPrice = float.Parse(((JsonElement)candle[3]).ToString()),
            ClosePrice = float.Parse(((JsonElement)candle[4]).ToString()),
            EndTime = ((JsonElement)candle[6]).GetInt64(),
            NumberOfTrades = ((JsonElement)candle[8]).GetInt32(),
        };
        c.CandleType = c.OpenPrice < c.ClosePrice ? CandleType.HighCandle : CandleType.LowCandle;
        return c;
    }
}