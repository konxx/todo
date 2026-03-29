using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace TodoDeskApp
{
    internal static class Program
    {
        private const string AppDisplayName = "桃神自用";

        [STAThread]
        private static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.ThreadException += Application_ThreadException;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TodoDeskForm());
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            ShowFatalError(e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception ?? new Exception("Unknown fatal error.");
            ShowFatalError(ex);
        }

        private static void ShowFatalError(Exception ex)
        {
            try
            {
                string storageDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "TodoDesk"
                );
                Directory.CreateDirectory(storageDir);
                string logPath = Path.Combine(storageDir, "fatal.log");
                File.AppendAllText(
                    logPath,
                    BuildErrorBlock(ex),
                    Encoding.UTF8
                );

                MessageBox.Show(
                    AppDisplayName + " 启动失败。\r\n\r\n" + ex.Message + "\r\n\r\n日志: " + logPath,
                    AppDisplayName,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            catch
            {
            }
        }

        private static string BuildErrorBlock(Exception ex)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]");
            builder.AppendLine(ex.ToString());
            builder.AppendLine();
            return builder.ToString();
        }
    }

    internal sealed class AppSurfacePanel : Panel
    {
        public int CornerRadius { get; set; }
        public Color BorderColor { get; set; }

        public AppSurfacePanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            UiDrawing.ApplyRoundedRegion(this, CornerRadius);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle bounds = ClientRectangle;
            bounds.Width -= 1;
            bounds.Height -= 1;

            using (GraphicsPath path = UiDrawing.CreateRoundedPath(bounds, CornerRadius))
            using (LinearGradientBrush backgroundBrush = new LinearGradientBrush(bounds, UiPalette.SurfaceBackground, UiPalette.SurfaceGlow, LinearGradientMode.Vertical))
            using (Pen borderPen = new Pen(BorderColor))
            {
                e.Graphics.FillPath(backgroundBrush, path);
                e.Graphics.DrawPath(borderPen, path);
            }
        }
    }

    internal sealed class CountBadge : Control
    {
        private string badgeText;

        public string BadgeText
        {
            get { return badgeText; }
            set
            {
                badgeText = value;
                using (Font badgeFont = new Font("Segoe UI", 7.8F, FontStyle.Bold, GraphicsUnit.Point))
                {
                    Size badgeSize = TextRenderer.MeasureText(
                        badgeText ?? string.Empty,
                        badgeFont,
                        new Size(int.MaxValue, int.MaxValue),
                        TextFormatFlags.NoPadding
                    );
                    Width = Math.Max(24, badgeSize.Width + 14);
                }
                Height = 22;
                Invalidate();
            }
        }

        public CountBadge()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            ForeColor = Color.White;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle bounds = ClientRectangle;
            bounds.Width -= 1;
            bounds.Height -= 1;

            using (GraphicsPath path = UiDrawing.CreateRoundedPath(bounds, Height / 2))
            using (LinearGradientBrush brush = new LinearGradientBrush(bounds, UiPalette.PrimaryBlue, UiPalette.PrimaryBlueBright, LinearGradientMode.Horizontal))
            using (Pen borderPen = new Pen(Color.FromArgb(28, 255, 255, 255)))
            using (Font badgeFont = new Font("Segoe UI", 7.8F, FontStyle.Bold, GraphicsUnit.Point))
            {
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(borderPen, path);
                TextRenderer.DrawText(
                    e.Graphics,
                    badgeText ?? string.Empty,
                    badgeFont,
                    ClientRectangle,
                    Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding
                );
            }
        }
    }

    internal sealed class BufferedFlowLayoutPanel : FlowLayoutPanel
    {
        public BufferedFlowLayoutPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }

        protected override Point ScrollToControl(Control activeControl)
        {
            return DisplayRectangle.Location;
        }
    }

    internal sealed class HeaderActionButton : Button
    {
        public HeaderActionButton()
        {
            SetStyle(ControlStyles.Selectable, false);
            TabStop = false;
        }

        protected override bool ShowFocusCues
        {
            get { return false; }
        }
    }
}
