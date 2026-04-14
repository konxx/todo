using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TodoDeskApp
{
    internal static class UiDrawing
    {
        public static GraphicsPath CreateRoundedPath(Rectangle bounds, int radius)
        {
            return CreateRoundedPath(
                RectangleF.FromLTRB(
                    bounds.Left + 0.5F,
                    bounds.Top + 0.5F,
                    bounds.Right - 0.5F,
                    bounds.Bottom - 0.5F
                ),
                radius
            );
        }

        public static GraphicsPath CreateRoundedPath(RectangleF bounds, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (bounds.Width <= 0F || bounds.Height <= 0F)
            {
                return path;
            }

            float maxRadius = Math.Max(1F, (Math.Min(bounds.Width, bounds.Height) - 1F) / 2F);
            float safeRadius = Math.Max(1F, Math.Min(radius, maxRadius));
            float diameter = safeRadius * 2F;
            float right = bounds.Right - diameter;
            float bottom = bounds.Bottom - diameter;

            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(right, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(right, bottom, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bottom, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        public static void ConfigureHighQuality(Graphics graphics)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
        }

        public static void ApplyRoundedRegion(Control control, int radius)
        {
            if (control.Width <= 1 || control.Height <= 1)
            {
                return;
            }

            Region previousRegion = control.Region;
            using (GraphicsPath path = CreateRoundedPath(new Rectangle(0, 0, control.Width, control.Height), radius))
            {
                control.Region = new Region(path);
            }

            if (previousRegion != null)
            {
                previousRegion.Dispose();
            }
        }
    }

    internal static class NativeMethods
    {
        internal const int WM_NCLBUTTONDOWN = 0x00A1;
        internal const int HTCAPTION = 0x0002;

        [DllImport("user32.dll")]
        internal static extern bool ReleaseCapture();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    }
}
