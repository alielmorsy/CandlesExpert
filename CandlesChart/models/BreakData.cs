using CandlesChart.models.enums;

namespace CandlesChart.models;

public class BreakData
{
    public float HighestPrice { get; set; }
    public float LowestPrice { get; set; }

    public LineType? CurrentLineType { get; set; }

    public int NumberOfCandles { get; set; }
}