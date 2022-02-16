namespace CandlesChart.models;

public class Line
{
    public float? Price { get; set; }

    public List<Candle> Candles { get; } = new();

    public LineType? LineType { get; set; }

    public bool IsBroken { get; set; }

    public int BrokenCount { get; set; } = 0;

    public float HighestPrice { get; set; }

    public float LowestPrice { get; set; }
}