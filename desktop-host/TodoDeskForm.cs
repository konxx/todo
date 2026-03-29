using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Windows.Forms;

namespace TodoDeskApp
{
    internal sealed class TodoDeskForm : Form
    {
        private const string AppDisplayName = "桃神自用";
        private const int WindowWidth = 210;
        private const int WindowHeight = 300;

        private readonly Random random = new Random();
        private readonly string storageDir;
        private readonly string tasksPath;
        private readonly string settingsPath;

        private readonly FlowLayoutPanel headerLeftPanel;
        private readonly FlowLayoutPanel headerRightPanel;
        private readonly CountBadge countBadge;
        private readonly Button languageButton;
        private readonly Button topmostButton;
        private readonly Button addButton;
        private readonly BufferedFlowLayoutPanel taskListPanel;
        private readonly NotifyIcon trayIcon;
        private readonly ToolStripMenuItem trayToggleItem;
        private readonly ToolStripMenuItem trayExitItem;

        private List<TodoTask> tasks;
        private AppSettings settings;
        private string expandedTaskId;

        public TodoDeskForm()
        {
            storageDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TodoDesk");
            tasksPath = Path.Combine(storageDir, "tasks.json");
            settingsPath = Path.Combine(storageDir, "settings.json");

            Text = AppDisplayName;
            ClientSize = new Size(WindowWidth, WindowHeight);
            MinimumSize = new Size(WindowWidth, WindowHeight);
            MaximumSize = new Size(WindowWidth, WindowHeight);
            StartPosition = FormStartPosition.Manual;
            Location = new Point(200, 80);
            BackColor = UiPalette.FormBackground;
            ForeColor = UiPalette.TitleText;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.None;
            ShowIcon = false;
            MaximizeBox = false;
            MinimizeBox = false;
            KeyPreview = true;
            Padding = new Padding(10);
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            AppSurfacePanel surfacePanel = new AppSurfacePanel
            {
                Dock = DockStyle.Fill,
                BackColor = UiPalette.SurfaceBackground,
                BorderColor = UiPalette.SurfaceBorder,
                CornerRadius = 18
            };
            Controls.Add(surfacePanel);

            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 46,
                BackColor = UiPalette.HeaderBackground,
                Padding = new Padding(10, 8, 10, 8)
            };
            headerPanel.Paint += HeaderPanel_Paint;
            surfacePanel.Controls.Add(headerPanel);

            taskListPanel = new BufferedFlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = UiPalette.TaskListBackground,
                Padding = new Padding(4, 10, 4, 8),
                Margin = Padding.Empty
            };
            taskListPanel.SizeChanged += TaskListPanel_SizeChanged;
            surfacePanel.Controls.Add(taskListPanel);
            taskListPanel.BringToFront();

            headerLeftPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Left,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            headerPanel.Controls.Add(headerLeftPanel);

            Label titleLabel = new Label
            {
                AutoSize = true,
                Text = AppDisplayName,
                Margin = new Padding(0, 6, 4, 0),
                Font = new Font("Segoe UI", 7.8F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = UiPalette.TitleText
            };
            headerLeftPanel.Controls.Add(titleLabel);

            countBadge = new CountBadge
            {
                Visible = false,
                Margin = new Padding(0, 4, 0, 0)
            };
            headerLeftPanel.Controls.Add(countBadge);

            headerRightPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Right,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            headerPanel.Controls.Add(headerRightPanel);

            languageButton = CreateGhostButton();
            languageButton.Paint += LanguageButton_Paint;
            languageButton.Click += LanguageButton_Click;
            headerRightPanel.Controls.Add(languageButton);

            topmostButton = CreateGhostButton();
            topmostButton.Paint += TopmostButton_Paint;
            topmostButton.Click += TopmostButton_Click;
            headerRightPanel.Controls.Add(topmostButton);

            addButton = CreateGhostButton();
            addButton.Paint += AddButton_Paint;
            addButton.Click += AddButton_Click;
            headerRightPanel.Controls.Add(addButton);

            ContextMenuStrip trayMenu = new ContextMenuStrip();
            trayToggleItem = new ToolStripMenuItem();
            trayExitItem = new ToolStripMenuItem();
            trayToggleItem.Click += TrayToggleItem_Click;
            trayExitItem.Click += TrayExitItem_Click;
            trayMenu.Items.Add(trayToggleItem);
            trayMenu.Items.Add(trayExitItem);

            trayIcon = new NotifyIcon
            {
                Text = AppDisplayName,
                Icon = Icon ?? SystemIcons.Application,
                ContextMenuStrip = trayMenu,
                Visible = true
            };
            trayIcon.DoubleClick += TrayIcon_DoubleClick;

            AttachDragSurface(headerPanel);
            AttachDragSurface(headerLeftPanel);
            AttachDragSurface(titleLabel);
            AttachDragSurface(countBadge);

            EnsureStorageDirectory();
            settings = LoadSettings();
            tasks = LoadTasks();
            ApplyUiState();

            FormClosing += TodoDeskForm_FormClosing;
            VisibleChanged += TodoDeskForm_VisibleChanged;
        }

        private UiText CurrentText
        {
            get { return UiTextCatalog.Resolve(settings.Language); }
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

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.N))
            {
                ShowAddDialog();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            UiDrawing.ApplyRoundedRegion(this, 24);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
            }

            base.Dispose(disposing);
        }

        private void HeaderPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (Pen borderPen = new Pen(UiPalette.HeaderBorder))
            {
                Control header = (Control)sender;
                e.Graphics.DrawLine(borderPen, 0, header.Height - 1, header.Width, header.Height - 1);
            }
        }

        private void TaskListPanel_SizeChanged(object sender, EventArgs e)
        {
            UpdateTaskControlWidths();
        }

        private void LanguageButton_Click(object sender, EventArgs e)
        {
            settings.Language = UiTextCatalog.NormalizeLanguage(settings.Language) == "zh_CN" ? "en_US" : "zh_CN";
            SaveSettings();
            ApplyUiState();
        }

        private void TopmostButton_Click(object sender, EventArgs e)
        {
            settings.Topmost = !settings.Topmost;
            SaveSettings();
            ApplyUiState();
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            ShowAddDialog();
        }

        private void TrayToggleItem_Click(object sender, EventArgs e)
        {
            ToggleVisibility();
        }

        private void TrayExitItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            ToggleVisibility();
        }

        private void TodoDeskForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            trayIcon.Visible = false;
        }

        private void TodoDeskForm_VisibleChanged(object sender, EventArgs e)
        {
            UpdateTrayMenu();
        }

        private void ToggleVisibility()
        {
            if (Visible)
            {
                Hide();
            }
            else
            {
                Show();
                WindowState = FormWindowState.Normal;
                Activate();
            }
        }

        private void EnsureStorageDirectory()
        {
            if (!Directory.Exists(storageDir))
            {
                Directory.CreateDirectory(storageDir);
            }
        }

        private void ShowAddDialog()
        {
            UiText text = CurrentText;
            using (TaskDialogForm dialog = new TaskDialogForm(text))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                string title = TrimToEmpty(dialog.TaskTitle);
                if (title.Length == 0)
                {
                    MessageBox.Show(this, text.TitleRequired, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string timestamp = NowString();
                TodoTask task = new TodoTask
                {
                    Id = MakeId(),
                    Title = title,
                    Note = TrimToEmpty(dialog.TaskNote),
                    Done = false,
                    CreatedAt = timestamp,
                    UpdatedAt = timestamp
                };

                tasks.Add(task);
                tasks = SortTasks(tasks);
                SaveTasks();
                RefreshTaskList();
            }
        }

        private void ApplyUiState()
        {
            TopMost = settings.Topmost;
            languageButton.Text = string.Empty;
            topmostButton.Text = string.Empty;
            addButton.Text = string.Empty;
            StyleGhostButton(languageButton, false);
            StyleGhostButton(topmostButton, settings.Topmost);
            StyleGhostButton(addButton, false);
            ApplyCompactHeaderMetrics();
            UpdateTrayMenu();
            UpdateCountBadge();
            RefreshTaskList();
        }

        private void RefreshTaskList()
        {
            taskListPanel.SuspendLayout();
            try
            {
                taskListPanel.Controls.Clear();

                if (tasks.Count == 0)
                {
                    Label emptyLabel = new Label
                    {
                        AutoSize = false,
                        Height = 116,
                        TextAlign = ContentAlignment.MiddleCenter,
                        ForeColor = UiPalette.SecondaryText,
                        Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point),
                        Text = CurrentText.EmptyState,
                        Margin = new Padding(0, 6, 0, 0),
                        BackColor = UiPalette.TaskListBackground
                    };
                    taskListPanel.Controls.Add(emptyLabel);
                }
                else
                {
                    foreach (TodoTask task in tasks)
                    {
                        TaskItemControl item = new TaskItemControl(task, expandedTaskId == task.Id);
                        item.Margin = new Padding(0, 0, 0, 8);
                        item.ToggleDoneRequested += TaskItem_ToggleDoneRequested;
                        item.ToggleExpandedRequested += TaskItem_ToggleExpandedRequested;
                        item.DeleteRequested += TaskItem_DeleteRequested;
                        taskListPanel.Controls.Add(item);
                    }
                }
            }
            finally
            {
                taskListPanel.ResumeLayout();
            }

            UpdateCountBadge();
            UpdateTaskControlWidths();
        }

        private void TaskItem_ToggleDoneRequested(object sender, EventArgs e)
        {
            TaskItemControl item = sender as TaskItemControl;
            if (item == null)
            {
                return;
            }

            TodoTask task = tasks.FirstOrDefault(entry => entry.Id == item.TaskId);
            if (task == null)
            {
                return;
            }

            task.Done = !task.Done;
            tasks = SortTasks(tasks);
            SaveTasks();
            RefreshTaskList();
        }

        private void TaskItem_ToggleExpandedRequested(object sender, EventArgs e)
        {
            TaskItemControl item = sender as TaskItemControl;
            if (item != null)
            {
                expandedTaskId = expandedTaskId == item.TaskId ? null : item.TaskId;
                RefreshTaskList();
            }
        }

        private void TaskItem_DeleteRequested(object sender, EventArgs e)
        {
            TaskItemControl item = sender as TaskItemControl;
            if (item == null)
            {
                return;
            }

            TodoTask task = tasks.FirstOrDefault(entry => entry.Id == item.TaskId);
            if (task == null)
            {
                return;
            }

            if (MessageBox.Show(this, CurrentText.DeleteConfirm, Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
            {
                return;
            }

            tasks.Remove(task);
            if (expandedTaskId == item.TaskId)
            {
                expandedTaskId = null;
            }

            SaveTasks();
            RefreshTaskList();
        }

        private void UpdateCountBadge()
        {
            countBadge.BadgeText = string.Empty;
            countBadge.Visible = false;
        }

        private void UpdateTrayMenu()
        {
            trayToggleItem.Text = Visible ? CurrentText.TrayHide : CurrentText.TrayShow;
            trayExitItem.Text = CurrentText.TrayExit;
        }

        private void UpdateTaskControlWidths()
        {
            int width = Math.Max(92, taskListPanel.ClientSize.Width - taskListPanel.Padding.Horizontal - 2);
            foreach (Control control in taskListPanel.Controls)
            {
                control.Width = width;
            }
        }

        private AppSettings LoadSettings()
        {
            AppSettings loaded = LoadJson(settingsPath, new AppSettings
            {
                Language = "zh_CN",
                Topmost = false
            });
            loaded.Language = UiTextCatalog.NormalizeLanguage(loaded.Language);
            return loaded;
        }

        private List<TodoTask> LoadTasks()
        {
            List<TodoTask> loaded = LoadJson(tasksPath, new List<TodoTask>());
            if (loaded == null)
            {
                loaded = new List<TodoTask>();
            }

            List<TodoTask> normalized = new List<TodoTask>();
            foreach (TodoTask task in loaded)
            {
                if (task == null)
                {
                    continue;
                }

                string title = TrimToEmpty(task.Title);
                if (title.Length == 0)
                {
                    continue;
                }

                string createdAt = string.IsNullOrWhiteSpace(task.CreatedAt)
                    ? (string.IsNullOrWhiteSpace(task.UpdatedAt) ? NowString() : task.UpdatedAt)
                    : task.CreatedAt;
                string updatedAt = string.IsNullOrWhiteSpace(task.UpdatedAt)
                    ? createdAt
                    : task.UpdatedAt;

                normalized.Add(new TodoTask
                {
                    Id = string.IsNullOrWhiteSpace(task.Id) ? Guid.NewGuid().ToString("N") : task.Id,
                    Title = title,
                    Note = TrimToEmpty(task.Note),
                    Done = task.Done,
                    CreatedAt = createdAt,
                    UpdatedAt = updatedAt
                });
            }

            return SortTasks(normalized);
        }

        private void SaveSettings()
        {
            SaveJson(settingsPath, settings);
        }

        private void SaveTasks()
        {
            tasks = SortTasks(tasks);
            SaveJson(tasksPath, tasks);
        }

        private static T LoadJson<T>(string path, T fallback)
        {
            if (!File.Exists(path))
            {
                return fallback;
            }

            try
            {
                using (FileStream stream = File.OpenRead(path))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                    return (T)serializer.ReadObject(stream);
                }
            }
            catch
            {
                return fallback;
            }
        }

        private static void SaveJson<T>(string path, T data)
        {
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (FileStream stream = File.Create(path))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                serializer.WriteObject(stream, data);
            }
        }

        private static List<TodoTask> SortTasks(IEnumerable<TodoTask> source)
        {
            return source
                .OrderBy(task => task.Done)
                .ThenByDescending(task => ParseTimestamp(task.CreatedAt))
                .ThenBy(task => task.Title ?? string.Empty, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
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

        private string MakeId()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}.{1:000}-{2:0000}",
                DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture),
                DateTime.Now.Millisecond,
                random.Next(1000, 9999)
            );
        }

        private static string NowString()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        private static string TrimToEmpty(string value)
        {
            return (value ?? string.Empty).Trim();
        }

        private Button CreateGhostButton()
        {
            Button button = new HeaderActionButton
            {
                AutoSize = false,
                Height = 28,
                Margin = new Padding(0, 0, 6, 0),
                Padding = new Padding(8, 0, 8, 0),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 7.8F, FontStyle.Bold, GraphicsUnit.Point),
                UseVisualStyleBackColor = false
            };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = UiPalette.GhostHover;
            button.FlatAppearance.MouseDownBackColor = UiPalette.GhostHover;
            button.SizeChanged += RoundedButton_SizeChanged;
            return button;
        }

        private void RoundedButton_SizeChanged(object sender, EventArgs e)
        {
            Control control = sender as Control;
            if (control != null)
            {
                UiDrawing.ApplyRoundedRegion(control, Math.Max(8, control.Height / 2));
            }
        }

        private void ApplyCompactHeaderMetrics()
        {
            languageButton.Size = new Size(28, 28);
            languageButton.Padding = Padding.Empty;
            languageButton.TextAlign = ContentAlignment.MiddleCenter;
            languageButton.Font = new Font("Segoe UI", 7.0F, FontStyle.Bold, GraphicsUnit.Point);

            topmostButton.Size = new Size(28, 28);
            topmostButton.Padding = Padding.Empty;
            topmostButton.TextAlign = ContentAlignment.MiddleCenter;
            topmostButton.Font = new Font("Segoe UI", 8.0F, FontStyle.Bold, GraphicsUnit.Point);

            addButton.Size = new Size(28, 28);
            addButton.Padding = Padding.Empty;
            addButton.TextAlign = ContentAlignment.MiddleCenter;
            addButton.Font = new Font("Segoe UI", 8.0F, FontStyle.Bold, GraphicsUnit.Point);
            addButton.Margin = Padding.Empty;
        }

        private void LanguageButton_Paint(object sender, PaintEventArgs e)
        {
            Button button = (Button)sender;
            bool hot = button.ClientRectangle.Contains(button.PointToClient(Cursor.Position));
            string label = UiTextCatalog.NormalizeLanguage(settings.Language) == "zh_CN" ? "EN" : "CH";
            Color textColor = hot ? UiPalette.PrimaryBlueDark : UiPalette.GhostText;

            using (Font font = new Font("Segoe UI", 7.0F, FontStyle.Bold, GraphicsUnit.Point))
            {
                TextRenderer.DrawText(
                    e.Graphics,
                    label,
                    font,
                    button.ClientRectangle,
                    textColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.NoPadding | TextFormatFlags.SingleLine
                );
            }
        }

        private void TopmostButton_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle bounds = topmostButton.ClientRectangle;
            float centerX = bounds.Left + (bounds.Width / 2F);
            float centerY = bounds.Top + (bounds.Height / 2F);
            using (Pen pen = new Pen(settings.Topmost ? UiPalette.PrimaryBlue : UiPalette.GhostText, 1.5F))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                e.Graphics.DrawEllipse(pen, centerX - 3.0F, centerY - 8.0F, 6.0F, 6.0F);
                e.Graphics.DrawLine(pen, centerX - 5.0F, centerY - 4.5F, centerX + 5.0F, centerY - 4.5F);
                e.Graphics.DrawLine(pen, centerX, centerY - 2.0F, centerX, centerY + 5.0F);
                e.Graphics.DrawLine(pen, centerX, centerY + 5.0F, centerX - 2.8F, centerY + 8.5F);
            }
        }

        private void AddButton_Paint(object sender, PaintEventArgs e)
        {
            Button button = (Button)sender;
            bool hot = button.ClientRectangle.Contains(button.PointToClient(Cursor.Position));
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            int size = Math.Max(8, Math.Min(button.ClientSize.Width, button.ClientSize.Height) - 14);
            int centerX = button.ClientSize.Width / 2;
            int centerY = button.ClientSize.Height / 2;
            using (Pen pen = new Pen(hot ? UiPalette.PrimaryBlue : UiPalette.GhostText, 1.6F))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                e.Graphics.DrawLine(pen, centerX - (size / 2), centerY, centerX + (size / 2), centerY);
                e.Graphics.DrawLine(pen, centerX, centerY - (size / 2), centerX, centerY + (size / 2));
            }
        }

        private static void StyleGhostButton(Button button, bool active)
        {
            if (active)
            {
                button.BackColor = UiPalette.GhostActiveBackground;
                button.ForeColor = UiPalette.PrimaryBlue;
                button.FlatAppearance.MouseOverBackColor = UiPalette.GhostActiveBackground;
                button.FlatAppearance.MouseDownBackColor = UiPalette.GhostActiveBackground;
            }
            else
            {
                button.BackColor = UiPalette.GhostHover;
                button.ForeColor = UiPalette.GhostText;
                button.FlatAppearance.MouseOverBackColor = UiPalette.GhostHover;
                button.FlatAppearance.MouseDownBackColor = UiPalette.GhostHover;
            }
        }

        private void AttachDragSurface(Control control)
        {
            control.MouseDown += DragSurface_MouseDown;
        }

        private void DragSurface_MouseDown(object sender, MouseEventArgs e)
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
