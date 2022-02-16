using System.Text.Json.Serialization;

namespace CandlesChart.models;

public class Candle
{
    [JsonPropertyName("t")] public long StartTime { get; set; }
    [JsonPropertyName("T")] public long EndTime { get; set; }
    [JsonPropertyName("h")] public float HighestPrice { get; set; }
    [JsonPropertyName("l")] public float LowestPrice { get; set; }
    [JsonPropertyName("o")] public float OpenPrice { get; set; }
    [JsonPropertyName("c")] public float ClosePrice { get; set; }
    [JsonPropertyName("n")] public long NumberOfTrades { get; set; }

    [JsonPropertyName("x")] public bool IsCandleClosed { get; set; } = true;

    public CandleType CandleType { get; set; }

    public override string ToString()
    {
        string text =
            $"Start Time: {new DateTime(1970, 1, 1).AddMilliseconds(StartTime)}, End Time: {new DateTime(1970, 1, 1).AddMilliseconds(EndTime)}, Highest Price: {HighestPrice}, Lowest Price: {LowestPrice}, Open Price: {OpenPrice}, Close Price: {ClosePrice}, Candle Type: {CandleType}";
        return text;
    }
}