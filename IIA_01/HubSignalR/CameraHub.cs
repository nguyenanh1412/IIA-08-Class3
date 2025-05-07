using Microsoft.AspNetCore.SignalR;
using System.Drawing.Imaging;
using System.Drawing;
using MvCameraControl;

public class CameraHub : Hub
{
    private bool _isStreaming = false;
    private IDevice device;
    private IHubContext<CameraHub> hubContext;
    public CameraHub(IDevice _device)
    {
        this.device = _device;
    }
    // Gửi hình ảnh đến tất cả client
    public async Task SendImage(string base64Image)
    {
        await Clients.All.SendAsync("ReceiveImage", base64Image);
    }

    // Bắt đầu gửi frame ảnh liên tục
    public async Task StartStreaming()
    {
        if (_isStreaming) return;  // Nếu đang stream thì không khởi động lại

        _isStreaming = true;

        // Lấy ảnh từ camera và gửi tới client liên tục
        while (_isStreaming)
        {
            using (Bitmap frame = GetFrameFromCamera())
            using (MemoryStream ms = new MemoryStream())
            {
                frame.Save(ms, ImageFormat.Jpeg);
                string base64 = "data:image/jpeg;base64," + Convert.ToBase64String(ms.ToArray());
                await hubContext.Clients.All.SendAsync("ReceiveImage", base64);
            }

            await Task.Delay(100);  // Gửi ảnh mỗi 100ms (10fps)
        }
    }

    // Dừng quá trình gửi ảnh
    public void StopStreaming()
    {
        _isStreaming = false;
    }

    public Bitmap GetFrameFromCamera()
    {
        // Thay thế bằng SDK camera thực tế
        Bitmap bmp = new Bitmap(640, 480);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.Clear(Color.Black);
            g.DrawString(DateTime.Now.ToString("HH:mm:ss"), new Font("Arial", 20), Brushes.White, new PointF(10, 10));
        }
        int nRet;
        IFrameOut frameOut;
        nRet = device.StreamGrabber.GetImageBuffer(1000, out frameOut);
        bmp = frameOut.Image.ToBitmap();
        return bmp;
    }

}
