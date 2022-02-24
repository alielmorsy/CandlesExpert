using System.Text;
using System.Text.Json;
using CandlesChart.models;

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
        _lines = lines;
    }

    public bool WriteData()
    {
        List<List<object>> data = new List<List<object>>();
        for (int m = 0; m < _candles.Count; m++)
        {
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
                l.Add(line.Price);
            }

            data.Add(l);
        }

        var dict = new Dictionary<string, object>()
        {
            {"candles", data},
            {"lines", _lines.Count}
        };
        var json = JsonSerializer.Serialize(dict);
        _stream.Write(Encoding.Default.GetBytes(json));
        return true;
    }
}