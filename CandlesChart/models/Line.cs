using CandlesChart.models.enums;

namespace CandlesChart.models;

public class Line
{
    public float Price { get; set; } = 0;

    public List<Candle> Candles { get; } = new();

    public LineType? LineType { get; set; }

    public bool IsBroken { get; set; }

    public int BrokenCount { get; set; } = 0;

    public List<BreakData> BreakDataList { get; } = new();

    public override string ToString()
    {
        return
            $"Price: {Price}, Number Of Candles On It: {Candles.Count}, Line Type: {LineType}, Number Of Breaks : {BrokenCount}";
    }
}