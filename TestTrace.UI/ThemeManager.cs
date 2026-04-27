using System;
using System.Drawing;
using System.Windows.Forms;

namespace TestTrace.UI
{
    public static class ThemeManager
    {
        // ===== Core Colours =====

        public static readonly Color AppBackgroundColor =
            ColorTranslator.FromHtml("#1A2533"); // Dark navy

        public static readonly Color PanelBackgroundColor =
            ColorTranslator.FromHtml("#223044"); // Slightly lighter navy

        public static readonly Color HeaderBackgroundColor =
            ColorTranslator.FromHtml("#C7D5E0"); // Light steel blue

        public static readonly Color HeaderTextColor =
            Color.Black;

        public static readonly Color SectionHeaderColor =
            ColorTranslator.FromHtml("#8CC7E8"); // Light cyan/blue for section titles

        public static readonly Color ButtonColor =
            HeaderBackgroundColor;

        public static readonly Color ButtonHoverColor =
            ColorTranslator.FromHtml("#3D566E");

        public static readonly Color ButtonTextColor =
            Color.Black;

        public static readonly Color DangerHoverColor =
            ColorTranslator.FromHtml("#C0392B");

        public static readonly Color LabelTextColor =
            ColorTranslator.FromHtml("#D6E1EA");

        public static readonly Color TextBoxBackgroundColor =
            ColorTranslator.FromHtml("#E6EDF3");

        public static readonly Color TextBoxTextColor =
            Color.Black;

        public static readonly Color GroupBorderColor =
            ColorTranslator.FromHtml("#2E3B4C");

        // ===== Fonts =====

        public static readonly Font SmallButtonFont =
            new Font("Segoe UI", 9F, FontStyle.Regular);

        public static readonly Font DefaultButtonFont =
            new Font("Segoe UI", 10F, FontStyle.Regular);

        public static readonly Font LargeButtonFont =
            new Font("Segoe UI", 12F, FontStyle.Bold);

        public static readonly Font PrimaryButtonFont =
            new Font("Segoe UI", 20F, FontStyle.Bold);

        public static readonly Font HeaderFont =
            new Font("Segoe UI", 16F, FontStyle.Bold);

        public static readonly Font DefaultFont =
            new Font("Segoe UI", 10F, FontStyle.Regular);

        public static readonly Font SectionHeaderFont =
            new Font("Segoe UI Semibold", 11F, FontStyle.Bold);

        // ===== Public Entry Point =====

        public static void ApplyTheme(Form form)
        {
            form.BackColor = AppBackgroundColor;
            ApplyToControls(form);
        }

        // ===== Recursive Theme Application =====

        private static void ApplyToControls(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                switch (control)
                {
                    case Panel panel:
                        ApplyPanelTheme(panel);
                        break;

                    case GroupBox groupBox:
                        ApplyGroupBoxTheme(groupBox);
                        break;

                    case Button button:
                        ApplyButtonTheme(button);
                        break;

                    case Label label:
                        ApplyLabelTheme(label);
                        break;

                    case TextBox textBox:
                        ApplyTextBoxTheme(textBox);
                        break;

                    case DateTimePicker picker:
                        ApplyDatePickerTheme(picker);
                        break;

                    case CheckBox checkBox:
                        checkBox.ForeColor = LabelTextColor;
                        checkBox.Font = DefaultFont;
                        break;
                }

                if (control.HasChildren)
                    ApplyToControls(control);
            }
        }

        // ===== Control Styling =====

        private static void ApplyPanelTheme(Panel panel)
        {
            panel.BackColor = panel.Name.Contains("Header")
                ? HeaderBackgroundColor
                : PanelBackgroundColor;
        }

        private static void ApplyGroupBoxTheme(GroupBox groupBox)
        {
            groupBox.BackColor = PanelBackgroundColor;
            groupBox.ForeColor = SectionHeaderColor;
            groupBox.Font = SectionHeaderFont;
        }

        private static void ApplyButtonTheme(Button button)
        {
            if (button.Font != null && button.Font.Name == "Segoe MDL2 Assets")
            {
                button.BackColor = HeaderBackgroundColor;
                button.ForeColor = ButtonTextColor;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                return;
            }

            button.BackColor = ButtonColor;
            button.ForeColor = ButtonTextColor;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = DefaultButtonFont;

            button.MouseEnter -= Button_MouseEnter;
            button.MouseLeave -= Button_MouseLeave;

            button.MouseEnter += Button_MouseEnter;
            button.MouseLeave += Button_MouseLeave;

            if (button.Tag is string role)
            {
                switch (role)
                {
                    case "Small":
                        button.Font = SmallButtonFont;
                        break;
                    case "Large":
                        button.Font = LargeButtonFont;
                        break;
                    case "Primary":
                        button.Font = PrimaryButtonFont;
                        break;
                }
            }
        }

        private static void Button_MouseEnter(object? sender, EventArgs e)
        {
            if (sender is not Button button) return;

            if (button.Tag is string role && role == "Danger")
            {
                button.BackColor = DangerHoverColor;
                button.ForeColor = Color.White;
            }
            else
            {
                button.BackColor = ButtonHoverColor;
            }
        }

        private static void Button_MouseLeave(object? sender, EventArgs e)
        {
            if (sender is not Button button) return;

            button.BackColor = ButtonColor;
            button.ForeColor = ButtonTextColor;
        }

        private static void ApplyLabelTheme(Label label)
        {
            if (label.Name.Contains("Header"))
            {
                label.Font = HeaderFont;
                label.ForeColor = HeaderTextColor;
            }
            else
            {
                label.Font = DefaultFont;
                label.ForeColor = LabelTextColor;
            }

            label.BackColor = Color.Transparent;
        }

        private static void ApplyTextBoxTheme(TextBox textBox)
        {
            textBox.BackColor = TextBoxBackgroundColor;
            textBox.ForeColor = TextBoxTextColor;
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.Font = DefaultFont;
        }

        private static void ApplyDatePickerTheme(DateTimePicker picker)
        {
            picker.Font = DefaultFont;
            picker.CalendarMonthBackground = TextBoxBackgroundColor;
        }
    }
}
