using System.Drawing.Imaging;
using System.Drawing;
using System.Net.WebSockets;
using System.Text;

public static class WebSocketManager
{
    public static List<WebSocket> Clients = new();

    public static async Task SendToAllClientsAsync(Bitmap bitmap)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            bitmap.Save(ms, ImageFormat.Jpeg);
            string base64 = "data:image/jpeg;base64," + Convert.ToBase64String(ms.ToArray());
            //frameOut.Image.ToBitmap().Save(ms, ImageFormat.Jpeg);
            //string base64 = "data:image/jpeg;base64," + Convert.ToBase64String(ms.ToArray());
            byte[] buffer = Encoding.UTF8.GetBytes(base64);

            foreach (var socket in Clients.ToList())
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(
                        new ArraySegment<byte>(buffer),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None
                    );
                }
                else
                {
                    Clients.Remove(socket);
                }
            }
        }
    }
}
