using System;
using System.Drawing;
using System.Windows.Forms;

namespace TodoDeskApp
{
    internal sealed class TaskDialogForm : Form
    {
        private const string AppDisplayName = "桃神自用";
        private readonly UiText text;
        private readonly TextBox titleTextBox;
        private readonly TextBox noteTextBox;

        public string TaskTitle
        {
            get { return titleTextBox.Text; }
        }

        public string TaskNote
        {
            get { return noteTextBox.Text; }
        }

        public TaskDialogForm(UiText text)
        {
            this.text = text;

            Text = AppDisplayName;
            ClientSize = new Size(196, 228);
            MinimumSize = new Size(196, 228);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            ShowIcon = false;
            ShowInTaskbar = false;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = UiPalette.FormBackground;
            Padding = new Padding(10);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            KeyPreview = true;

            AppSurfacePanel surfacePanel = new AppSurfacePanel
            {
                Dock = DockStyle.Fill,
                BackColor = UiPalette.SurfaceBackground,
                BorderColor = UiPalette.SurfaceBorder,
                CornerRadius = 18
            };
            Controls.Add(surfacePanel);

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                Padding = new Padding(10, 10, 10, 10),
                BackColor = Color.Transparent
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            surfacePanel.Controls.Add(layout);

            Label dialogTitle = new Label
            {
                AutoSize = true,
                Text = text.AddDialogTitle,
                Font = new Font("Segoe UI", 9.2F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = UiPalette.TitleText,
                Margin = new Padding(0, 0, 0, 8)
            };
            layout.Controls.Add(dialogTitle, 0, 0);

            Label titleHint = new Label
            {
                AutoSize = true,
                Text = text.TitleHint,
                ForeColor = UiPalette.GhostText,
                Font = new Font("Segoe UI", 7.7F, FontStyle.Bold, GraphicsUnit.Point),
                Margin = new Padding(0, 0, 0, 3)
            };
            layout.Controls.Add(titleHint, 0, 1);

            titleTextBox = CreateInputBox(false);
            titleTextBox.Margin = new Padding(0, 0, 0, 8);
            titleTextBox.MinimumSize = new Size(0, 28);
            layout.Controls.Add(titleTextBox, 0, 2);

            Label noteHint = new Label
            {
                AutoSize = true,
                Text = text.NoteHint,
                ForeColor = UiPalette.GhostText,
                Font = new Font("Segoe UI", 7.7F, FontStyle.Bold, GraphicsUnit.Point),
                Margin = new Padding(0, 0, 0, 3)
            };
            layout.Controls.Add(noteHint, 0, 3);

            noteTextBox = CreateInputBox(true);
            noteTextBox.Margin = new Padding(0);
            noteTextBox.MinimumSize = new Size(0, 72);
            layout.Controls.Add(noteTextBox, 0, 4);

            FlowLayoutPanel buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill,
                WrapContents = false,
                Margin = new Padding(0, 8, 0, 0),
                AutoSize = true
            };
            layout.Controls.Add(buttonPanel, 0, 5);

            Button cancelButton = CreateDialogButton(text.CancelButton, false);
            cancelButton.Click += CancelButton_Click;
            buttonPanel.Controls.Add(cancelButton);

            Button addButton = CreateDialogButton(text.AddButton, true);
            addButton.Click += AddButton_Click;
            buttonPanel.Controls.Add(addButton);

            AcceptButton = addButton;
            CancelButton = cancelButton;

            Shown += TaskDialogForm_Shown;
            AttachDrag(surfacePanel);
            AttachDrag(layout);
            AttachDrag(dialogTitle);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int CsDropShadow = 0x00020000;
                CreateParams parameters = base.CreateParams;
                parameters.ClassStyle |= CsDropShadow;
                return parameters;
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            UiDrawing.ApplyRoundedRegion(this, 22);
        }

        private TextBox CreateInputBox(bool multiline)
        {
            TextBox textBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = multiline,
                ScrollBars = multiline ? ScrollBars.Vertical : ScrollBars.None,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = UiPalette.InputBackground,
                ForeColor = UiPalette.TitleText,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point)
            };
            return textBox;
        }

        private Button CreateDialogButton(string label, bool primary)
        {
            Button button = new Button
            {
                Text = label,
                Width = 68,
                Height = 28,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(4, 0, 0, 0),
                Font = new Font("Segoe UI", 7.7F, FontStyle.Bold, GraphicsUnit.Point),
                TabStop = false,
                UseVisualStyleBackColor = false
            };
            button.FlatAppearance.BorderSize = 0;
            button.SizeChanged += DialogButton_SizeChanged;

            if (primary)
            {
                button.BackColor = UiPalette.PrimaryBlue;
                button.ForeColor = Color.White;
                button.FlatAppearance.MouseOverBackColor = UiPalette.PrimaryBlueDark;
                button.FlatAppearance.MouseDownBackColor = UiPalette.PrimaryBlueDark;
            }
            else
            {
                button.BackColor = UiPalette.GhostHover;
                button.ForeColor = UiPalette.GhostText;
                button.FlatAppearance.MouseOverBackColor = UiPalette.GhostActiveBackground;
                button.FlatAppearance.MouseDownBackColor = UiPalette.GhostActiveBackground;
            }

            return button;
        }

        private void DialogButton_SizeChanged(object sender, EventArgs e)
        {
            Control control = sender as Control;
            if (control != null)
            {
                UiDrawing.ApplyRoundedRegion(control, 12);
            }
        }

        private void TaskDialogForm_Shown(object sender, EventArgs e)
        {
            titleTextBox.Focus();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            if ((titleTextBox.Text ?? string.Empty).Trim().Length == 0)
            {
                MessageBox.Show(this, text.TitleRequired, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void AttachDrag(Control control)
        {
            control.MouseDown += Drag_MouseDown;
        }

        private void Drag_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            NativeMethods.ReleaseCapture();
            NativeMethods.SendMessage(Handle, NativeMethods.WM_NCLBUTTONDOWN, (IntPtr)NativeMethods.HTCAPTION, IntPtr.Zero);
        }
    }
}
