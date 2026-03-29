using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace TodoDeskApp
{
    internal sealed class TaskItemControl : Control
    {
        private const int BaseHeight = 52;

        private readonly TodoTask task;
        private readonly string stampText;
        private readonly bool hasNote;

        private Rectangle cardBounds;
        private Rectangle checkboxBounds;
        private Rectangle titleBounds;
        private Rectangle stampBounds;
        private Rectangle deleteBounds;
        private Rectangle noteBounds;
        private Rectangle noteTextBounds;
        private bool showStamp;
        private bool hoverRow;
        private bool hoverDelete;
        private bool hoverTitle;

        public event EventHandler ToggleDoneRequested;
        public event EventHandler ToggleExpandedRequested;
        public event EventHandler DeleteRequested;

        public string TaskId
        {
            get { return task.Id; }
        }

        public bool Expanded { get; private set; }

        public TaskItemControl(TodoTask task, bool expanded)
        {
            this.task = task;
            Expanded = expanded;
            stampText = FormatStamp(task.CreatedAt);
            hasNote = !string.IsNullOrWhiteSpace(task.Note);

            BackColor = UiPalette.TaskListBackground;
            Margin = Padding.Empty;
            Padding = Padding.Empty;
            Cursor = Cursors.Default;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);

            UpdateMetrics();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateMetrics();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            bool nextHoverDelete = deleteBounds.Contains(e.Location);
            bool nextHoverTitle = hasNote && titleBounds.Contains(e.Location);
            if (!hoverRow || hoverDelete != nextHoverDelete || hoverTitle != nextHoverTitle)
            {
                hoverRow = true;
                hoverDelete = nextHoverDelete;
                hoverTitle = nextHoverTitle;
                Invalidate();
            }

            if (hoverDelete || checkboxBounds.Contains(e.Location) || hoverTitle)
            {
                Cursor = Cursors.Hand;
            }
            else
            {
                Cursor = Cursors.Default;
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            hoverRow = false;
            hoverDelete = false;
            hoverTitle = false;
            Cursor = Cursors.Default;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            if (checkboxBounds.Contains(e.Location))
            {
                if (ToggleDoneRequested != null)
                {
                    ToggleDoneRequested(this, EventArgs.Empty);
                }
                return;
            }

            if (deleteBounds.Contains(e.Location))
            {
                if (DeleteRequested != null)
                {
                    DeleteRequested(this, EventArgs.Empty);
                }
                return;
            }

            if (hasNote && titleBounds.Contains(e.Location) && ToggleExpandedRequested != null)
            {
                ToggleExpandedRequested(this, EventArgs.Empty);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(UiPalette.TaskListBackground);

            if (cardBounds.Width <= 0 || cardBounds.Height <= 0)
            {
                return;
            }

            DrawCard(e.Graphics);
            DrawCheckbox(e.Graphics);
            DrawTitle(e.Graphics);
            DrawStamp(e.Graphics);
            DrawDelete(e.Graphics);
            DrawNote(e.Graphics);
        }

        private void UpdateMetrics()
        {
            int width = Math.Max(108, Width);

            checkboxBounds = new Rectangle(14, 16, 18, 18);
            deleteBounds = new Rectangle(width - 34, 14, 20, 20);
            showStamp = width >= 166;

            if (showStamp)
            {
                using (Font stampFont = new Font("Segoe UI", 6.7F, FontStyle.Regular, GraphicsUnit.Point))
                {
                    Size stampSize = TextRenderer.MeasureText(
                        stampText,
                        stampFont,
                        new Size(120, 20),
                        TextFormatFlags.NoPadding | TextFormatFlags.SingleLine
                    );
                    stampBounds = new Rectangle(deleteBounds.Left - 6 - stampSize.Width, 19, stampSize.Width, 12);
                }
            }
            else
            {
                stampBounds = Rectangle.Empty;
            }

            int titleLeft = checkboxBounds.Right + 10;
            int titleRight = showStamp ? stampBounds.Left - 8 : deleteBounds.Left - 8;
            titleBounds = new Rectangle(titleLeft, 11, Math.Max(44, titleRight - titleLeft), 26);

            if (Expanded && hasNote)
            {
                int noteWidth = Math.Max(56, width - 32);
                int measuredHeight;

                using (Font noteFont = new Font("Segoe UI", 7.3F, FontStyle.Regular, GraphicsUnit.Point))
                {
                    Size measured = TextRenderer.MeasureText(
                        task.Note ?? string.Empty,
                        noteFont,
                        new Size(noteWidth - 20, int.MaxValue),
                        TextFormatFlags.WordBreak | TextFormatFlags.NoPadding | TextFormatFlags.TextBoxControl
                    );
                    measuredHeight = Math.Max(18, measured.Height);
                }

                noteBounds = new Rectangle(12, 42, noteWidth, measuredHeight + 18);
                noteTextBounds = new Rectangle(noteBounds.Left + 10, noteBounds.Top + 9, noteBounds.Width - 20, measuredHeight);
                Height = noteBounds.Bottom + 12;
            }
            else
            {
                noteBounds = Rectangle.Empty;
                noteTextBounds = Rectangle.Empty;
                Height = BaseHeight;
            }

            cardBounds = new Rectangle(2, 0, Math.Max(0, width - 5), Math.Max(0, Height - 2));
        }

        private void DrawCard(Graphics graphics)
        {
            Color cardColor = task.Done ? UiPalette.TaskDoneBackground : UiPalette.TaskCardBackground;
            if (hoverRow && !task.Done)
            {
                cardColor = UiPalette.TaskHoverBackground;
            }

            Color borderColor = hoverRow ? UiPalette.FocusOutline : UiPalette.TaskBorder;
            using (GraphicsPath path = UiDrawing.CreateRoundedPath(cardBounds, 14))
            using (SolidBrush fillBrush = new SolidBrush(cardColor))
            using (Pen borderPen = new Pen(borderColor))
            {
                graphics.FillPath(fillBrush, path);
                graphics.DrawPath(borderPen, path);
            }
        }

        private void DrawCheckbox(Graphics graphics)
        {
            Color fillColor = task.Done ? UiPalette.PrimaryBlue : Color.White;
            if (!task.Done && hoverRow)
            {
                fillColor = UiPalette.PrimaryBlueLight;
            }

            Color borderColor = task.Done ? UiPalette.PrimaryBlue : (hoverRow ? UiPalette.FocusOutline : UiPalette.CheckboxBorder);
            using (Pen borderPen = new Pen(borderColor, 1.5F))
            using (SolidBrush fillBrush = new SolidBrush(fillColor))
            {
                graphics.FillEllipse(fillBrush, checkboxBounds);
                graphics.DrawEllipse(borderPen, checkboxBounds);
            }

            if (!task.Done)
            {
                return;
            }

            PointF point1 = new PointF(checkboxBounds.Left + 4.0F, checkboxBounds.Top + 9.0F);
            PointF point2 = new PointF(checkboxBounds.Left + 7.2F, checkboxBounds.Top + 12.0F);
            PointF point3 = new PointF(checkboxBounds.Left + 13.0F, checkboxBounds.Top + 5.8F);
            using (Pen checkPen = new Pen(Color.White, 1.8F))
            {
                checkPen.StartCap = LineCap.Round;
                checkPen.EndCap = LineCap.Round;
                graphics.DrawLines(checkPen, new[] { point1, point2, point3 });
            }
        }

        private void DrawTitle(Graphics graphics)
        {
            Color titleColor = task.Done ? UiPalette.MutedText : UiPalette.TitleText;
            if (hasNote && hoverTitle && !task.Done)
            {
                titleColor = UiPalette.PrimaryBlueDark;
            }

            FontStyle style = task.Done ? FontStyle.Strikeout : FontStyle.Bold;
            using (Font titleFont = new Font("Segoe UI", 8.5F, style, GraphicsUnit.Point))
            {
                TextRenderer.DrawText(
                    graphics,
                    task.Title ?? string.Empty,
                    titleFont,
                    titleBounds,
                    titleColor,
                    TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPadding
                );
            }
        }

        private void DrawStamp(Graphics graphics)
        {
            if (!showStamp || stampText.Length == 0)
            {
                return;
            }

            using (Font stampFont = new Font("Segoe UI", 6.7F, FontStyle.Regular, GraphicsUnit.Point))
            {
                TextRenderer.DrawText(
                    graphics,
                    stampText,
                    stampFont,
                    stampBounds,
                    UiPalette.TimestampText,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPadding
                );
            }
        }

        private void DrawDelete(Graphics graphics)
        {
            if (!hoverRow && !hoverDelete)
            {
                return;
            }

            if (hoverDelete)
            {
                using (SolidBrush hoverBrush = new SolidBrush(UiPalette.DeleteHoverBackground))
                using (GraphicsPath path = UiDrawing.CreateRoundedPath(deleteBounds, 10))
                {
                    graphics.FillPath(hoverBrush, path);
                }
            }

            Color iconColor = hoverDelete ? UiPalette.DeleteHoverText : UiPalette.TimestampText;
            float left = deleteBounds.Left + 6.0F;
            float right = deleteBounds.Right - 6.0F;
            float top = deleteBounds.Top + 6.0F;
            float bottom = deleteBounds.Bottom - 6.0F;

            using (Pen pen = new Pen(iconColor, 1.5F))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                graphics.DrawLine(pen, left, top, right, bottom);
                graphics.DrawLine(pen, right, top, left, bottom);
            }
        }

        private void DrawNote(Graphics graphics)
        {
            if (!Expanded || !hasNote || noteBounds == Rectangle.Empty)
            {
                return;
            }

            using (GraphicsPath path = UiDrawing.CreateRoundedPath(noteBounds, 10))
            using (SolidBrush noteBrush = new SolidBrush(UiPalette.NoteBackground))
            using (Pen borderPen = new Pen(UiPalette.Divider))
            using (Font noteFont = new Font("Segoe UI", 7.3F, FontStyle.Regular, GraphicsUnit.Point))
            {
                graphics.FillPath(noteBrush, path);
                graphics.DrawPath(borderPen, path);
                TextRenderer.DrawText(
                    graphics,
                    task.Note ?? string.Empty,
                    noteFont,
                    noteTextBounds,
                    UiPalette.SecondaryText,
                    TextFormatFlags.WordBreak | TextFormatFlags.NoPadding | TextFormatFlags.TextBoxControl
                );
            }
        }

        private static DateTime ParseTimestamp(string value)
        {
            DateTime parsed;
            if (DateTime.TryParseExact(
                value,
                "yyyy-MM-dd HH:mm:ss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out parsed))
            {
                return parsed;
            }

            if (DateTime.TryParse(value, out parsed))
            {
                return parsed;
            }

            return DateTime.MinValue;
        }

        private static string FormatStamp(string value)
        {
            DateTime timestamp = ParseTimestamp(value);
            if (timestamp == DateTime.MinValue)
            {
                return string.Empty;
            }

            return timestamp.ToString("MM-dd HH:mm", CultureInfo.InvariantCulture);
        }
    }
}
