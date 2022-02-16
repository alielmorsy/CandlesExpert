using System.Net.WebSockets;
using System.Text;

namespace CandlesChart;

public class WS
{
    public async Task test()
    {
        
        var ws = new ClientWebSocket();

        ws.Options.RemoteCertificateValidationCallback = (_, _, _, _) => true;

        var cts = new CancellationTokenRegistration();
        await ws.ConnectAsync(new Uri("wss://stream.binance.com:9443/ws/btcusdt@kline_4h"), cts.Token);
// var a = "{\"method\": \"LIST_SUBSCRIPTIONS\",\"params\": [\"btcusdt@trade\",\"btcusdt@depth\"],\"id\": 1}";
//
// await ws.SendAsync(new ReadOnlyMemory<byte>(Encoding.Default.GetBytes(a)), WebSocketMessageType.Text, false, cts.Token);


        while (!cts.Token.IsCancellationRequested)
        {
            byte[] v = new byte[1024];
            var r = await ws.ReceiveAsync(v, cts.Token);
            Console.WriteLine(Encoding.Default.GetString(v, 0, r.Count));
        }
    }
}