using System.Text;
using System.Text.Json;
using CandlesChart.models;
using CandlesChart.models.enums;

namespace CandlesChart;

public class WriteDataToStream
{
    private readonly List<Candle> _candles;

    private readonly List<Line> _lines;

    private readonly Stream _stream;

    public WriteDataToStream(Stream stream, List<Candle> candles, List<Line> lines)
    {
        _stream = stream;
        _candles = candles;
    
        _lines = lines.FindAll(l => l.BrokenCount < 10);
    }

    public bool WriteData(List<float> floats)
    {
        int j = 0;
        var foundLines = new bool[_lines.Count];
        Array.Fill(foundLines, false);
        List<List<object>> data = new List<List<object>>();
        for (int m = 0; m < _candles.Count; m++)
        {
            j = 0;
            bool startLine = false;
            Candle candle = _candles[m];
            List<object> l = new List<object>();
            l.Add(candle.StartTime);
            // l.Add(new DateTime(1970, 1, 1).AddMilliseconds(candle.StartTime).ToString());
            l.Add(candle.LowestPrice);
            l.Add(candle.ClosePrice);
            l.Add(candle.OpenPrice);
            l.Add(candle.HighestPrice);

            for (int i = 0; i < _lines.Count; i++)
            {
                var line = _lines[i];
                var firstCandle = line.Candles[0];
                if (firstCandle == candle)
                {
                   
                    foundLines[i] = true;
                }

                if (foundLines[i])
                    l.Add(line.Price);
                else

                    l.Add(null);
            }

           

            data.Add(l);
        }

        var minCandle = _candles.MinBy(c => c.CandleType == CandleType.HighCandle ? c.OpenPrice : c.ClosePrice);
        var minPrice = (minCandle.CandleType == CandleType.HighCandle ? minCandle.OpenPrice : minCandle.ClosePrice) -
                       5000;

        var maxCandle = _candles.MaxBy(c => c.CandleType == CandleType.LowCandle ? c.OpenPrice : c.ClosePrice);
        var maxPrice = maxCandle.CandleType == CandleType.HighCandle ? maxCandle.OpenPrice : maxCandle.ClosePrice;

        var dict = new Dictionary<string, object>()

        {
            {"candles", data},
            {"lines", _lines.Count},
            {"min", minPrice},
            {"max", maxPrice}
        };

        var json = JsonSerializer.Serialize(dict);
        _stream.Write(Encoding.Default.GetBytes(json));
        return true;
    }
}