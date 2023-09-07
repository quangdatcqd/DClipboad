 
using System.Drawing; 
using System.Windows.Forms;

namespace D_Clipboards
{
    public class OverlayLayer : Control
    {

        
            private Rectangle selectionRect;

            public OverlayLayer()
            {
                this.DoubleBuffered = true;
                this.ResizeRedraw = true;
                // Thiết lập thuộc tính để đảm bảo rằng lớp phủ sẽ không
                // che khuất các cửa sổ ứng dụng khác
                this.SetStyle(ControlStyles.Selectable, false);
                this.SetStyle(ControlStyles.UserMouse, true);
                this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
                this.BackColor = Color.Transparent;
                this.Dock = DockStyle.Fill;

            }

            protected override void OnPaint(PaintEventArgs e)
            {
                // Vẽ hình chữ nhật trên lớp phủ
                base.OnPaint(e);
                if (this.selectionRect != Rectangle.Empty)
                {
                    using (Pen pen = new Pen(Color.Red, 2))
                    {
                        e.Graphics.DrawRectangle(pen, this.selectionRect);
                    }
                }
            }
            private Point startPoint;
            public void StartSelection(Point location)
            {
                startPoint = location;
                // Bắt đầu vẽ hình chữ nhật với gốc tại vị trí chuột
                this.selectionRect = new Rectangle(location, Size.Empty);
                this.Invalidate();
            }

            public void UpdateSelection(Point location)
            {
                // Cập nhật kích thước của hình chữ nhật khi người dùng di chuyển chuột
                if (this.selectionRect != Rectangle.Empty)
                {

                    this.selectionRect = HandleMouseSelection(startPoint, location);
                    this.Invalidate();
                }
            }

            private Rectangle HandleMouseSelection(Point startPoint, Point endPoint)
            {
                int x, y, width, height;

                // Xử lý tọa độ và kích thước khi kéo từ phải qua trái
                if (endPoint.X < startPoint.X)
                {
                    x = endPoint.X;
                    width = startPoint.X - endPoint.X;
                }
                else
                {
                    x = startPoint.X;
                    width = endPoint.X - startPoint.X;
                }

                if (endPoint.Y < startPoint.Y)
                {
                    y = endPoint.Y;
                    height = startPoint.Y - endPoint.Y;
                }
                else
                {
                    y = startPoint.Y;
                    height = endPoint.Y - startPoint.Y;
                }

                // Sử dụng giá trị x, y, width, height để thực hiện các tác vụ khác
                // ...

                // Tạo Rectangle từ tọa độ và kích thước đã tính toán
                return new Rectangle(x, y, width, height);
            }

            public Rectangle EndSelection()
            {
                // Kết thúc vẽ hình chữ nhật
                Rectangle points = this.selectionRect;
                this.selectionRect = Rectangle.Empty;
                this.Invalidate();

                return points;
            }

         

            protected override void OnMouseClick(MouseEventArgs e)
            {
                // Đóng form overlay khi người dùng nhấn chuột
                this.Dispose();
            }
        }

    }
 
