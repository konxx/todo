using System;
using System.Drawing;
using System.Runtime.Serialization;

namespace TodoDeskApp
{
    [DataContract]
    internal sealed class TodoTask
    {
        [DataMember(Order = 1)]
        public string Id { get; set; }

        [DataMember(Order = 2)]
        public string Title { get; set; }

        [DataMember(Order = 3)]
        public string Note { get; set; }

        [DataMember(Order = 4)]
        public bool Done { get; set; }

        [DataMember(Order = 5)]
        public string CreatedAt { get; set; }

        [DataMember(Order = 6)]
        public string UpdatedAt { get; set; }
    }

    [DataContract]
    internal sealed class AppSettings
    {
        [DataMember(Order = 1)]
        public string Language { get; set; }

        [DataMember(Order = 2)]
        public bool Topmost { get; set; }
    }

    internal sealed class UiText
    {
        public string AddDialogTitle;
        public string TitleHint;
        public string NoteHint;
        public string AddButton;
        public string CancelButton;
        public string TitleRequired;
        public string DeleteConfirm;
        public string EmptyState;
        public string TopmostOn;
        public string TopmostOff;
        public string TrayShow;
        public string TrayHide;
        public string TrayExit;
    }

    internal static class UiPalette
    {
        public static readonly Color FormBackground = Color.FromArgb(236, 246, 241);
        public static readonly Color SurfaceBackground = Color.FromArgb(253, 255, 254);
        public static readonly Color SurfaceBorder = Color.FromArgb(208, 226, 218);
        public static readonly Color SurfaceGlow = Color.FromArgb(243, 251, 247);
        public static readonly Color HeaderBackground = Color.FromArgb(245, 251, 248);
        public static readonly Color HeaderBorder = Color.FromArgb(225, 236, 232);
        public static readonly Color Divider = Color.FromArgb(231, 240, 236);
        public static readonly Color TitleText = Color.FromArgb(31, 51, 49);
        public static readonly Color PrimaryBlue = Color.FromArgb(34, 156, 130);
        public static readonly Color PrimaryBlueDark = Color.FromArgb(22, 128, 106);
        public static readonly Color PrimaryBlueBright = Color.FromArgb(92, 214, 186);
        public static readonly Color PrimaryBlueLight = Color.FromArgb(219, 244, 236);
        public static readonly Color FocusOutline = Color.FromArgb(163, 217, 200);
        public static readonly Color GhostText = Color.FromArgb(70, 96, 90);
        public static readonly Color GhostHover = Color.FromArgb(233, 244, 239);
        public static readonly Color GhostActiveBackground = Color.FromArgb(216, 241, 232);
        public static readonly Color GhostActiveBorder = Color.FromArgb(173, 214, 199);
        public static readonly Color TaskListBackground = Color.FromArgb(246, 251, 249);
        public static readonly Color TaskCardBackground = Color.FromArgb(255, 255, 255);
        public static readonly Color TaskHoverBackground = Color.FromArgb(241, 250, 246);
        public static readonly Color TaskDoneBackground = Color.FromArgb(236, 247, 242);
        public static readonly Color TaskBorder = Color.FromArgb(224, 236, 230);
        public static readonly Color SecondaryText = Color.FromArgb(92, 109, 104);
        public static readonly Color MutedText = Color.FromArgb(132, 146, 142);
        public static readonly Color TimestampText = Color.FromArgb(123, 150, 143);
        public static readonly Color NoteBackground = Color.FromArgb(244, 250, 247);
        public static readonly Color DeleteHoverBackground = Color.FromArgb(255, 238, 239);
        public static readonly Color DeleteHoverText = Color.FromArgb(218, 74, 87);
        public static readonly Color CheckboxBorder = Color.FromArgb(183, 204, 196);
        public static readonly Color InputBackground = Color.FromArgb(248, 252, 250);
        public static readonly Color InputBorder = Color.FromArgb(206, 222, 215);
    }

    internal static class UiTextCatalog
    {
        public static readonly UiText Chinese = new UiText
        {
            AddDialogTitle = "添加待办",
            TitleHint = "任务标题",
            NoteHint = "备注（可选）",
            AddButton = "添加",
            CancelButton = "取消",
            TitleRequired = "请输入任务标题。",
            DeleteConfirm = "确认删除此任务？",
            EmptyState = "当前没有待办\r\n点击右上角按钮添加",
            TopmostOn = "置顶中",
            TopmostOff = "置顶",
            TrayShow = "显示",
            TrayHide = "隐藏",
            TrayExit = "退出"
        };

        public static readonly UiText English = new UiText
        {
            AddDialogTitle = "Add Task",
            TitleHint = "Task title",
            NoteHint = "Notes (optional)",
            AddButton = "Add",
            CancelButton = "Cancel",
            TitleRequired = "Please enter a task title.",
            DeleteConfirm = "Delete this task?",
            EmptyState = "No tasks yet.\r\nUse the top-right button to add one.",
            TopmostOn = "Pinned",
            TopmostOff = "Pin",
            TrayShow = "Show",
            TrayHide = "Hide",
            TrayExit = "Exit"
        };

        public static string NormalizeLanguage(string value)
        {
            return value == "en_US" ? "en_US" : "zh_CN";
        }

        public static UiText Resolve(string language)
        {
            return NormalizeLanguage(language) == "en_US" ? English : Chinese;
        }
    }
}
