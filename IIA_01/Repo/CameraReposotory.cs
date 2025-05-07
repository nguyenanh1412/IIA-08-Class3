using System.Data;
using System.Data.SqlClient;
using Dapper;
using NCalc;
using IIA_01.Models.LogModel;
using IIA_02_Server_scanner.Models.ScannerModel;
using System.Text.RegularExpressions;
using OfficeOpenXml;
using System;
using MvCameraControl;
using System.Drawing;
using System.Reflection.Emit;
using IIA_01.HubSignalR;
using Microsoft.AspNetCore.SignalR;
using System.Drawing.Imaging;

public class CameraReposotory
{
    private readonly IHubContext<LogHub> _hubContext;
    private readonly string _connectionString;
    readonly DeviceTLayerType enumTLayerType = DeviceTLayerType.MvGigEDevice | DeviceTLayerType.MvUsbDevice
            | DeviceTLayerType.MvGenTLGigEDevice | DeviceTLayerType.MvGenTLCXPDevice | DeviceTLayerType.MvGenTLCameraLinkDevice | DeviceTLayerType.MvGenTLXoFDevice;

    List<IDeviceInfo> deviceInfoList = new List<IDeviceInfo>();
    IDevice device = null;

    bool isGrabbing = false;        // ch:是否正在取图 | en: Grabbing flag
    bool isRecord = false;          // ch:是否正在录像 | en: Video record flag
    Thread receiveThread = null;    // ch:接收图像线程 | en: Receive image thread
    public bool isBursting = false;

    private IFrameOut frameForSave;                         // ch:获取到的帧信息, 用于保存图像 | en:Frame for save image
    private readonly object saveImageLock = new object();
    private CancellationTokenSource captureTokenSource;
    public CameraReposotory(IConfiguration configuration, IHubContext<LogHub> hubContext) // Inject IConfiguration
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
        _hubContext = hubContext;
    }
    public List<IDeviceInfo> RefreshDeviceList()
    {
        int nRet = DeviceEnumerator.EnumDevices(enumTLayerType, out deviceInfoList);
        if (nRet != MvError.MV_OK)
        {
            //ShowErrorMsg("Enumerate devices fail!", nRet);
            return null;
        }
        return deviceInfoList;
    }
    public bool bnOpen_Click(IDevice devices )
    {
        logging("Open camera...");
        device = devices;
        int result = device.Open();
        if (result != MvError.MV_OK)
        {
            return false;
        }

        if (device is IGigEDevice)
        {
            IGigEDevice gigEDevice = device as IGigEDevice;

            int optionPacketSize;
            result = gigEDevice.GetOptimalPacketSize(out optionPacketSize);
            if (result != MvError.MV_OK)
            {
                logging("Get optimal packet size fail!");
                return false;
            }
            else
            {
                result = device.Parameters.SetIntValue("GevSCPSPacketSize", (long)optionPacketSize);
                if (result != MvError.MV_OK)
                {
                    logging("SetIntValue fail!");
                    return false;
                }
            }
        }

        device.Parameters.SetEnumValueByString("AcquisitionMode", "Continuous");
        device.Parameters.SetEnumValueByString("TriggerMode", "Off");
        // Bắt đầu bật cam
        try
        {
            isGrabbing = true;

            receiveThread = new Thread(ReceiveThreadProcess);
            receiveThread.Start();
        }
        catch (Exception ex)
        {
            throw;
        }

        int res = device.StreamGrabber.StartGrabbing();
        if (res != MvError.MV_OK)
        {
            logging("Start Grabbing fail!");
            return false;
        }
        int rs = device.Parameters.SetFloatValue("ExposureTime", 150000);
        if (rs != MvError.MV_OK)
        {
            logging("Set Exposure Time fail!");
            return false;
        }

        device.Parameters.SetEnumValue("GainAuto", 0);
        rs = device.Parameters.SetFloatValue("Gain", 4);
        if (result != MvError.MV_OK)
        {
            logging("Set Gain fail!");
            return false;
        }
        logging("Open camera success!");
        return true;
    }
    public void ReceiveThreadProcess()
    {
        int nRet;

        Graphics graphics;   // ch:使用GDI在pictureBox上绘制图像 | en:Display frame using a graphics

        while (isGrabbing)
        {
            IFrameOut frameOut;

            nRet = device.StreamGrabber.GetImageBuffer(1000, out frameOut);
            if (MvError.MV_OK == nRet)
            {
                if (isRecord)
                {
                    device.VideoRecorder.InputOneFrame(frameOut.Image);
                }

                lock (saveImageLock)
                {
                    try
                    {
                        frameForSave = frameOut.Clone() as IFrameOut;
                    }
                    catch (Exception e)
                    {
                        return;
                    }
                }

#if !GDI_RENDER
#else
                    // 使用GDI绘制图像
                    try
                    {
                        using (Bitmap bitmap = frameOut.Image.ToBitmap())
                        {
                            if (graphics == null)
                            {
                                graphics = pictureBox1.CreateGraphics();
                            }

                            Rectangle srcRect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                            Rectangle dstRect = new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height);
                            graphics.DrawImage(bitmap, dstRect, srcRect, GraphicsUnit.Pixel);
                        }
                    }
                    catch (Exception e)
                    {
                        device.StreamGrabber.FreeImageBuffer(frameOut);
                        MessageBox.Show(e.Message);
                        return;
                    }
#endif


                device.StreamGrabber.FreeImageBuffer(frameOut);
                //_hubContext.Clients.All.SendAsync("ReceiveImage", frameOut.Image.ToBitmap());
                using (MemoryStream ms = new MemoryStream())
                {
                    //frameOut.Image.ToBitmap().Save(ms, ImageFormat.Jpeg);
                    //string base64 = "data:image/jpeg;base64," + Convert.ToBase64String(ms.ToArray());

                    //_hubContext.Clients.All.SendAsync("ReceiveImage", base64);
                     WebSocketManager.SendToAllClientsAsync(frameOut.Image.ToBitmap());
                }
            }
        }
    }
    public void bnClose_Click()
    {
        logging("Closing cam ...");
        if (isGrabbing == true)
        {
            bnStopGrab_Click();
        }

        if (device != null)
        {
            StopBurt();
            device.Close();
            device.Dispose();
        }
        logging("Closed cam ...");
    }
    private void bnStopGrab_Click()
    {
        // ch:标志位设为false | en:Set flag bit false
        isGrabbing = false;
        receiveThread.Join();

        // ch:停止采集 | en:Stop Grabbing
        int result = device.StreamGrabber.StopGrabbing();
        if (result != MvError.MV_OK)
        {
            logging("Stop Grabbing fail!");
            throw new Exception("Stop Grabbing Fail!");
        }
    }
    private int SaveImage()
    {
        logging("Saving image ...");
        ImageFormatInfo imageFormatInfo;
        imageFormatInfo.FormatType = ImageFormatType.Jpeg;
        imageFormatInfo.JpegQuality = 99;
        if (frameForSave == null)
        {
            logging("No image to save!");
            throw new Exception("No vaild image");
        }

        // Tạo tên file ảnh
        string fileName = Guid.NewGuid() + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + "." + imageFormatInfo.FormatType.ToString();

        // Đường dẫn đến thư mục IMG trong thư mục gốc của ứng dụng
        string imgFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "IMG");

        // Tạo thư mục nếu chưa tồn tại
        if (!Directory.Exists(imgFolderPath))
        {
            Directory.CreateDirectory(imgFolderPath);
        }

        // Ghép đường dẫn đầy đủ đến file ảnh
        string imagePath = Path.Combine(imgFolderPath, fileName);

        lock (saveImageLock)
        {
            logging("Saving image OK ");
            return device.ImageSaver.SaveImageToFile(imagePath, frameForSave.Image, imageFormatInfo, CFAMethod.Equilibrated);
        }
    }
    public bool BurtShooting(int imageNum,int secNum)
    {
        logging("Burst shooting ...");
        if (isBursting) return true;
        try
        {
            if (secNum <= 0)
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            logging("Burst shooting fail!");
            return false;
        }
        int intervalMs = (int)((double)secNum * 1000 / imageNum);

        captureTokenSource = new CancellationTokenSource();
        CancellationToken token = captureTokenSource.Token;

        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                isBursting = true;
                try
                {
                    SaveImage(); // Hàm bạn đã có
                }
                catch (Exception ex)
                {
                    StopBurt();
                }

                await Task.Delay(intervalMs, token);
            }

            Console.WriteLine("Capturing stopped.");
        }, token);
        return true;
    }
    public bool StopBurt()
    {
        logging("Stop burst shooting ...");
        if (captureTokenSource != null)
        {
            captureTokenSource.Cancel();
            captureTokenSource.Dispose();
            captureTokenSource = null;
            isBursting = false;
        }
        logging("Stop burst shooting OK");
        return true;
    }
    public bool bnSaveJpg_Click()
    {
        logging("Capturing image ...");
        int result;
        try
        {
            result = SaveImage();
            if (result != MvError.MV_OK)
            {
                logging("Capture image fail!");
                return false;
            }
            else
            {
                logging("Capture image OK");
                return true;
            }

        }
        catch (Exception ex)
        {
            return false;
        }
    }
    private void logging(string log)
    {
        _hubContext.Clients.All.SendAsync("ReceiveLog", log);
    }
}
