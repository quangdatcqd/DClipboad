using System; 
using System.Drawing; 
using System.Windows.Forms;

namespace D_Clipboards
{
    

    public class OverlayForm : Form
    {
        private OverlayLayer overlayLayer;
        public Bitmap screenshot;
        public Rectangle selection;
        public OverlayForm(Bitmap screenshot)
        {
            this.Cursor = Cursors.Cross;
            // Tạo form overlay và thiết lập thuộc tính để đảm bảo rằng nó sẽ vẽ lên tất cả các cửa sổ ứng dụng khác
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.WindowState = FormWindowState.Maximized;
            this.BackgroundImage = screenshot;
            this.screenshot = screenshot; 

            // Tạo một instance của lớp phủ
            this.overlayLayer = new OverlayLayer();

            // Thêm lớp phủ vào lớp overlay
            this.Controls.Add(this.overlayLayer);

            // Thiết lập sự kiện MouseDown để bắt đầu vẽ hình chữ nhật
            this.overlayLayer.MouseDown += OverlayLayer_MouseDown;

            // Thiết lập sự kiện MouseMove để vẽ hình chữ nhật khi di chuyển chuột
            this.overlayLayer.MouseMove += OverlayLayer_MouseMove;

            // Thiết lập sự kiện MouseUp để kết thúc vẽ hình chữ nhật
            this.overlayLayer.MouseUp += OverlayLayer_MouseUp;

        }

        // ...

        private void OverlayLayer_MouseDown(object sender, MouseEventArgs e)
        {
            // Bắt đầu vẽ hình chữ nhật khi người dùng nhấn chuột trái
            // trên lớp phủ

            this.overlayLayer.StartSelection(e.Location);
        }



        private void OverlayLayer_MouseMove(object sender, MouseEventArgs e)
        {
            // Vẽ hình chữ nhật khi người dùng di chuyển chuột trên lớp phủ
            this.overlayLayer.UpdateSelection(e.Location);
        }

        private void OverlayLayer_MouseUp(object sender, MouseEventArgs e)
        {
            // Kết thúc vẽ hình chữ nhật khi người dùng nhả chuột trái
            // trên lớp phủ

            this.selection = this.overlayLayer.EndSelection();
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            // Giải phóng tài nguyên khi đóng form overlay
            base.OnClosed(e);
        }
        protected override void OnMouseClick(MouseEventArgs e)
        {
            // Đóng form overlay khi người dùng nhấn chuột
            this.Dispose();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // OverlayForm
            // 
            this.ClientSize = new System.Drawing.Size(328, 221);
            this.Name = "OverlayForm";
            this.ResumeLayout(false);

        }
    }

}
 