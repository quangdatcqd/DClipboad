using D_Clipboards;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using RestSharp;
using System; 
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices; 
using System.Threading; 
using System.Windows.Forms; 
using SocketIOClient;
using Newtonsoft.Json;

namespace D_Clipboards
{
    public partial class DClipboard : Form
    {
        private SocketIO socket;
        private string RoomID = "all";
        private OverlayForm overlayForm;
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private NotifyIcon notifyIcon;
        private LowLevelKeyboardProc keyboardProc;
        private IntPtr keyboardHookId = IntPtr.Zero;

        public   DClipboard()
        {
            this.ShowInTaskbar = false;
           
            InitializeComponent();

            checkAppRunning();
            loadSettings();
            InitializeNotifyIcon();
             InitializeSocket();
            keyboardProc = HookCallback;

            keyboardHookId = SetKeyboardHook(keyboardProc);
            ClipboardMonitor clipboardMonitor = new ClipboardMonitor();
            clipboardMonitor.ClipboardChanged += ClipboardChangedHandler;
            clipboardMonitor.StartMonitoring();
           

            // Sau khi hoàn thành giám sát clipboard, bạn có thể gọi:
            // clipboardMonitor.StopMonitoring();
        }

        string getTextTemp;
        private async void ClipboardChangedHandler(object sender, EventArgs e)
        { 
            try
            { 
                string clipboardText = Clipboard.GetText();
                if(clipboardText != getTextTemp)
                {
                    var payload = new dataSocket { ID = RoomID, Message = clipboardText };
                    //await socket.EmitAsync("copy", payload);
                    getTextTemp = clipboardText;
                }
                
            }
            catch { }
        }

        string setTextTemp;
        public async void InitializeSocket()
        {
            try
            {
                string host = "http://103.200.20.251:3000";
                //string host = "http://cqdplus.com";
                socket = new SocketIO(host);

                socket.OnConnected += async (sender, e) =>
                {
                    var payload = new dataSocket { ID = RoomID, Message = "join" };
                    await socket.EmitAsync("joinRoom", payload);

                };


                socket.On("copy", response =>
                {
                    if(setTextTemp != response.ToString())
                    { 
                        Thread staThread = new Thread(() =>
                        {
                            // Đặt văn bản vào Clipboard
                            Clipboard.SetText(response.ToString());

                        });

                        staThread.SetApartmentState(ApartmentState.STA);
                        staThread.Start();
                        staThread.Join();
                    }
                });
                await socket.ConnectAsync();
            }
            catch(Exception ex)
            {
                statusClip(ex.Message);
            }
        }
 

        private void InitializeNotifyIcon()
        {
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new Icon("D:\\C# TOOL\\DClipboad\\D_Clipboards\\download.ico"); // Đặt biểu tượng cho NotifyIcon
            notifyIcon.Visible = true; // Hiển thị NotifyIcon trong System Tray
            notifyIcon.Text = "D Clipboard";
            notifyIcon.MouseMove += NotifyIcon_MouseMove;
            notifyIcon.MouseDoubleClick += NotifyIcon_DoubleClick;
        }
        private void NotifyIcon_DoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();

        }
        private void NotifyIcon_MouseMove(object sender, MouseEventArgs e)
        {
            notifyIcon.Text = "D Clipboard"; // Đặt lại tên ứng dụng khi di chuột lên icon
        }

        // Override phương thức OnFormClosing để ẩn NotifyIcon khi form đóng
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            notifyIcon.Visible = false; // Ẩn NotifyIcon khi form đóng
        }

        private IntPtr SetKeyboardHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        private bool ZKey = false;
        private bool winKey = false;
        private bool isKeyPressed = false;
        private DateTime startTime;

        private bool keyShortOn(int code)
        {
            const int ZKeyCode = 90;
            const int WinKeyCode = 91;
            const int TimeThreshold = 500; // 200ms
            if (code == WinKeyCode)
            {
                startTime = DateTime.Now;
                winKey = true;
            }
            if (code == ZKeyCode && winKey)
            { 
                ZKey = true;
            } 

            if (ZKey && (DateTime.Now - startTime).TotalMilliseconds > TimeThreshold)
            {
                ZKey = false;
                winKey = false;
                isKeyPressed = false;
            }


            if (ZKey && winKey && !isKeyPressed)
            {
                isKeyPressed = true;

            }

            if (isKeyPressed && (DateTime.Now - startTime).TotalMilliseconds <= TimeThreshold)
            {
                ZKey = false;
                winKey = false;
                isKeyPressed = false;
                return true;
            }
            else
            {
                return false;
            }
        }
        
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {

            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_KEYUP))
            {
                int vkCode = Marshal.ReadInt32(lParam);

                //if (vkCode == 27) overlayForm.Dispose();
                // Xử lý logic khi có sự kiện phím được nhấn hoặc thả ở bất kỳ đâu
                // 162 190. 61
                if (wParam == (IntPtr)WM_KEYDOWN)
                {
                    bool onShortKey = keyShortOn(vkCode);
                    if (onShortKey)
                    { 
                        // Chụp màn hình và tạo bitmap
                        if (overlayForm != null && overlayForm.Visible) {
                            if (overlayForm.InvokeRequired)
                            {
                                overlayForm.Invoke(new Action(() => overlayForm.Close()));
                            }
                            else
                            {
                                overlayForm.Close();
                            }
                        }
                        Screen[] screens = Screen.AllScreens;
                        int totalWidth =0;
                        int totalHeight= 0;
                        foreach (Screen screen in screens)
                        {
                            totalWidth += screen.Bounds.Width;

                            if (screen.Bounds.Height > totalHeight) totalHeight = screen.Bounds.Height;

                        }
                        RunApp(totalWidth, totalHeight);

                    }
                }
                else if (wParam == (IntPtr)WM_KEYUP)
                {
                    // Xử lý logic khi có sự kiện phím được thả
                }
            }

            return CallNextHookEx(keyboardHookId, nCode, wParam, lParam);
        }
      
        void RunApp(int Width, int Height)
        {
            new Thread(() =>
            { 
                Bitmap screenshot = new Bitmap(Width, Height);
                using(Graphics g =   Graphics.FromImage(screenshot))
                {
                    g.CopyFromScreen(0, 0, 0, 0, new Size(Width, Height));
                }
                // Tạo một instance của form overlay và hiển thị hình ảnh chụp màn hình trên nó
                overlayForm = new OverlayForm(screenshot  ); 
                overlayForm.Location = new Point(0, 0);
                overlayForm.Size = new Size(Width,  Height);
                overlayForm.FormClosed += (sender, e) =>
                {
                    // Mã xử lý sau khi form overlay được đóng 
                    new Thread(() =>
                    {
                        try
                        { 
                            Rectangle select = overlayForm.selection;
                            if (select.X == 0 && select.Y == 0 && select.Width == 0 && select.Height == 0) return;
                            Rectangle cropRectangle = new Rectangle(select.X, select.Y, select.Width, select.Height); // Thay thế x, y, width, height bằng kết quả rectangle của bạn

                            // Cắt ảnh từ hình ảnh gốc
                            Bitmap croppedImage = ImageCropper.CropImage(overlayForm.screenshot, cropRectangle);


                            string imageData = ConvertBitmapToBase64(croppedImage);
                            string data = ConvertText(imageData);
                            if (data == "0") return;

                            JObject text = JObject.Parse(data);
                            if (text["responses"][0].Count() <= 0) return;
                            //Invoke(new Action(() =>
                            //{
                            //    pbPreview.Image = croppedImage;
                            //    txtResult.Text = text["responses"][0]["fullTextAnnotation"]["text"].ToString();
                            //}));
                            // Gọi phương thức SetText trong luồng STA
                            Thread staThread = new Thread(() =>
                            {
                                // Đặt văn bản vào Clipboard
                                Clipboard.SetText(text["responses"][0]["fullTextAnnotation"]["text"].ToString());
                                // Tạo một NotifyIcon
                                NotifyIcon notifyIcon = new NotifyIcon();
                                notifyIcon.Icon = SystemIcons.Information;
                                notifyIcon.Visible = true;

                                // Hiển thị thông báo
                                notifyIcon.ShowBalloonTip(1000, "D Clipboard", "Thành công!",ToolTipIcon.Info );

                                // Đợi một khoảng thời gian sau đó đóng ứng dụng
                                System.Threading.Thread.Sleep(1000);

                                // Đóng NotifyIcon và thoát ứng dụng
                                notifyIcon.Dispose();
                            });
                            //Invoke(new Action(() =>
                            //{
                            //    //pbPreview.Image = croppedImage;
                            //    txtResult.Text = text["responses"][0]["fullTextAnnotation"]["text"].ToString();
                            //    pbPreview.Image = croppedImage;

                            //}));


                            // Đảm bảo rằng luồng STA mà chúng ta tạo sẽ không ngừng chạy cho đến khi hoàn thành công việc của nó
                            staThread.SetApartmentState(ApartmentState.STA);
                            staThread.Start();
                            staThread.Join();
                            overlayForm.screenshot.Dispose();
                            croppedImage.Dispose();
                        }
                        catch { }
                    }).Start();


                    // Chạy các lệnh tiếp theo tại đây
                    // ...
                };
                
                overlayForm.Show(); 
                Application.Run();

                // Giải phóng tài nguyên khi kết thúc chương trình
                screenshot.Dispose();

            }).Start();
        }


        private void AutoStartupCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (autoStartupCheckBox.Checked)
            {
                EnableAutoStartup();
            }
            else
            {
                DisableAutoStartup();
            }
        }

        private void EnableAutoStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.SetValue(Application.ProductName, Application.ExecutablePath);
            }
        }

        private void DisableAutoStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.DeleteValue(Application.ProductName, false);
            }
        }

        private bool IsAutoStartupEnabled()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                return key.GetValue(Application.ProductName) != null;
            }
        }
        private string ConvertText(string content)
        {
            try
            {
                var options = new RestClientOptions("https://cqdgo.com/zalo/converttext.php")
                {
                    MaxTimeout = -1,
                };
                var client = new RestClient(options);
                var request = new RestRequest("", Method.Post);
                request.AlwaysMultipartFormData = true;
                request.AddParameter("content", content);
                RestResponse response = client.Execute(request);
                return response.Content;
            }
            catch { return "0"; }
        }

        private static string ConvertBitmapToBase64(Bitmap bitmap)
        {
            // Tạo một đối tượng MemoryStream để lưu trữ dữ liệu hình ảnh
            using (MemoryStream ms = new MemoryStream())
            {
                // Chuyển đổi hình ảnh sang định dạng PNG và lưu vào MemoryStream
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                // Đọc dữ liệu từ MemoryStream và chuyển đổi sang mảng byte
                byte[] imageBytes = ms.ToArray();

                // Chuyển đổi mảng byte sang chuỗi Base64
                string base64String = Convert.ToBase64String(imageBytes);

                return base64String;
            }
        }
        private void btnStopApp_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
            Application.Exit();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
            Application.Exit();

        }

        #region Win32 API

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        #endregion

        private void DClipboard_FormClosing(object sender, EventArgs e)
        { 
        }
        void checkAppRunning() 
        {
            var appName = Process.GetProcessesByName("D_Clipboards");
            if(appName.Length>1)
            {
                appName[0].Kill();
            } 
        }

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        public void pnMoveForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, new IntPtr(HT_CAPTION), IntPtr.Zero);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void DClipboard_Load(object sender, EventArgs e)
        {
         







            checkAppRunning();
            loadSettings();
            Invoke(new Action(() =>
            {

                if (IsAutoStartupEnabled())
                {
                    autoStartupCheckBox.Checked = true;
                    EnableAutoStartup();
                }
                else
                {
                    autoStartupCheckBox.Checked = false;
                }
            }));

        }


        void loadSettings()
        {
            try
            {
                string data = File.ReadAllText("settings.txt");
                JObject rs = JObject.Parse(data);
                RoomID = rs["SocketRoom"].ToString() ;
                if (RoomID == "")
                    RoomID = "all";
                Invoke(new Action(() =>
                {
                    txtUserName.Text = RoomID;
                })); 
            }
            catch
            {

            }
        }
        private void txtUserName_TextChanged(object sender, EventArgs e)
        {
            RoomID = txtUserName.Text;
            Settings settings =  new Settings { SocketRoom = RoomID };
            string setting = JsonConvert.SerializeObject(settings);
            File.WriteAllText("settings.txt", setting );

        }

        void statusClip(string text)
        {
            try
            {
                Invoke(new Action(() =>
                {
                    lbStatus.Text = text;
                }));
            }
            catch { }
           
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {
            Invoke(new Action(() =>
            {
                if (autoStartupCheckBox.Checked)
                {
                    autoStartupCheckBox.Checked = false;
                }
                else
                {
                    autoStartupCheckBox.Checked = true;
                }
            }));
        }
    }
}





public class ImageCropper
{
    public static Bitmap CropImage(Bitmap sourceImage, Rectangle cropRectangle)
    {
        // Tạo một bitmap mới với kích thước của crop rectangle
        Bitmap croppedImage = new Bitmap(cropRectangle.Width, cropRectangle.Height);

        // Tạo đối tượng Graphics từ croppedImage
        using (Graphics graphics = Graphics.FromImage(croppedImage))
        {
            // Cắt và vẽ phần ảnh cần cắt từ hình ảnh gốc lên croppedImage
            graphics.DrawImage(sourceImage, new Rectangle(0, 0, croppedImage.Width, croppedImage.Height), cropRectangle, GraphicsUnit.Pixel);
        }

        return croppedImage;
    }
}

public class dataSocket
{
    public string ID { get; set; }
    public string Message { get; set; }
}

public class Settings
{
    public string SocketRoom { get; set; } 
}