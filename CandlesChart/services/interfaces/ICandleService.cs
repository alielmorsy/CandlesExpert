using CandlesChart.models;

namespace CandlesChart.services.interfaces;

public interface ICandleService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="candleJson">A json text contains Data about </param>
    /// <returns>Return List Contains Candles list</returns>
 void CreateCandlesFromOldData(string candleJson);

    /// <summary>
    /// TO Create List Contains the all lines are found between candles
    /// </summary>
    
    /// <returns>Returns List Contains All Lines Found</returns>
    void CreateLines();


    /// <summary>
    /// 
    /// </summary>
    /// <param name="json">The Candle Json</param>
    /// <returns>Return the new Candle</returns>
    Candle? CreateCandleBasedOnNewData(string json);
}