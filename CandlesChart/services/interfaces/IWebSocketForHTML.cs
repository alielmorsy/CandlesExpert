namespace CandlesChart.services.interfaces;

public interface IWebSocketForHTML
{
    /// <summary>
    /// To Bind Connection With Html when when page start
    /// </summary>
    /// <returns></returns>
    bool bind();

    /// <summary>
    /// To Send Reload Request to html page to load new data
    /// </summary>
    /// <returns></returns>
    bool SendReload();
}