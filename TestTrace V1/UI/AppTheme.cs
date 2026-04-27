using System.Drawing.Drawing2D;
using System.Windows.Forms.VisualStyles;

namespace TestTrace_V1.UI;

public enum AppThemeMode
{
    Dark,
    Light
}

public enum ThemeButtonKind
{
    Secondary,
    Primary,
    Positive,
    Negative
}

public sealed class ThemePalette
{
    public required Color AppBackground { get; init; }
    public required Color Surface { get; init; }
    public required Color SurfaceAlt { get; init; }
    public required Color SurfaceElevated { get; init; }
    public required Color Border { get; init; }
    public required Color HeaderBackground { get; init; }
    public required Color HeaderForeground { get; init; }
    public required Color TextPrimary { get; init; }
    public required Color TextSecondary { get; init; }
    public required Color TextMuted { get; init; }
    public required Color Accent { get; init; }
    public required Color InputBackground { get; init; }
    public required Color InputForeground { get; init; }
    public required Color InputReadOnlyBackground { get; init; }
    public required Color SelectionBackground { get; init; }
    public required Color SelectionForeground { get; init; }
    public required Color StatusDraftBackground { get; init; }
    public required Color StatusDraftForeground { get; init; }
    public required Color StatusExecutableBackground { get; init; }
    public required Color StatusExecutableForeground { get; init; }
    public required Color StatusReleasedBackground { get; init; }
    public required Color StatusReleasedForeground { get; init; }
    public required Color PassBackground { get; init; }
    public required Color PassForeground { get; init; }
    public required Color PassSoftBackground { get; init; }
    public required Color FailBackground { get; init; }
    public required Color FailForeground { get; init; }
    public required Color FailSoftBackground { get; init; }
    public required Color NotApplicableBackground { get; init; }
    public required Color NotApplicableForeground { get; init; }
    public required Color ButtonPrimaryBackground { get; init; }
    public required Color ButtonPrimaryForeground { get; init; }
    public required Color ButtonSecondaryBackground { get; init; }
    public required Color ButtonSecondaryForeground { get; init; }
    public required Color ButtonBorder { get; init; }
}

public static class AppTheme
{
    private static readonly ThemePalette DarkPalette = new()
    {
        AppBackground = Color.FromArgb(30, 35, 40),
        Surface = Color.FromArgb(37, 43, 49),
        SurfaceAlt = Color.FromArgb(43, 50, 57),
        SurfaceElevated = Color.FromArgb(49, 57, 65),
        Border = Color.FromArgb(60, 70, 80),
        HeaderBackground = Color.FromArgb(32, 44, 60),
        HeaderForeground = Color.FromArgb(231, 237, 242),
        TextPrimary = Color.FromArgb(231, 237, 242),
        TextSecondary = Color.FromArgb(174, 184, 194),
        TextMuted = Color.FromArgb(136, 146, 156),
        Accent = Color.FromArgb(110, 147, 176),
        InputBackground = Color.FromArgb(28, 33, 38),
        InputForeground = Color.FromArgb(231, 237, 242),
        InputReadOnlyBackground = Color.FromArgb(43, 50, 57),
        SelectionBackground = Color.FromArgb(54, 78, 102),
        SelectionForeground = Color.FromArgb(242, 247, 251),
        StatusDraftBackground = Color.FromArgb(74, 67, 34),
        StatusDraftForeground = Color.FromArgb(246, 232, 173),
        StatusExecutableBackground = Color.FromArgb(32, 54, 40),
        StatusExecutableForeground = Color.FromArgb(188, 228, 195),
        StatusReleasedBackground = Color.FromArgb(58, 62, 67),
        StatusReleasedForeground = Color.FromArgb(220, 226, 232),
        PassBackground = Color.FromArgb(36, 56, 43),
        PassForeground = Color.FromArgb(159, 211, 171),
        PassSoftBackground = Color.FromArgb(41, 56, 47),
        FailBackground = Color.FromArgb(68, 41, 43),
        FailForeground = Color.FromArgb(241, 162, 167),
        FailSoftBackground = Color.FromArgb(60, 42, 44),
        NotApplicableBackground = Color.FromArgb(52, 56, 61),
        NotApplicableForeground = Color.FromArgb(156, 165, 174),
        ButtonPrimaryBackground = Color.FromArgb(64, 98, 126),
        ButtonPrimaryForeground = Color.FromArgb(245, 249, 252),
        ButtonSecondaryBackground = Color.FromArgb(43, 50, 57),
        ButtonSecondaryForeground = Color.FromArgb(231, 237, 242),
        ButtonBorder = Color.FromArgb(73, 85, 96)
    };

    private static readonly ThemePalette LightPalette = new()
    {
        AppBackground = Color.FromArgb(243, 245, 247),
        Surface = Color.White,
        SurfaceAlt = Color.FromArgb(238, 242, 245),
        SurfaceElevated = Color.FromArgb(249, 251, 252),
        Border = Color.FromArgb(215, 222, 229),
        HeaderBackground = Color.FromArgb(46, 91, 123),
        HeaderForeground = Color.White,
        TextPrimary = Color.FromArgb(30, 42, 50),
        TextSecondary = Color.FromArgb(95, 107, 118),
        TextMuted = Color.FromArgb(119, 129, 139),
        Accent = Color.FromArgb(46, 91, 123),
        InputBackground = Color.White,
        InputForeground = Color.FromArgb(30, 42, 50),
        InputReadOnlyBackground = Color.FromArgb(246, 248, 250),
        SelectionBackground = Color.FromArgb(222, 234, 244),
        SelectionForeground = Color.FromArgb(30, 42, 50),
        StatusDraftBackground = Color.FromArgb(246, 231, 168),
        StatusDraftForeground = Color.FromArgb(88, 72, 22),
        StatusExecutableBackground = Color.FromArgb(223, 242, 227),
        StatusExecutableForeground = Color.FromArgb(42, 90, 54),
        StatusReleasedBackground = Color.FromArgb(230, 234, 238),
        StatusReleasedForeground = Color.FromArgb(76, 84, 92),
        PassBackground = Color.FromArgb(173, 222, 183),
        PassForeground = Color.FromArgb(30, 105, 55),
        PassSoftBackground = Color.FromArgb(223, 242, 227),
        FailBackground = Color.FromArgb(240, 167, 167),
        FailForeground = Color.FromArgb(150, 30, 30),
        FailSoftBackground = Color.FromArgb(247, 217, 217),
        NotApplicableBackground = Color.FromArgb(230, 232, 235),
        NotApplicableForeground = Color.FromArgb(113, 121, 128),
        ButtonPrimaryBackground = Color.FromArgb(46, 91, 123),
        ButtonPrimaryForeground = Color.White,
        ButtonSecondaryBackground = Color.White,
        ButtonSecondaryForeground = Color.FromArgb(30, 42, 50),
        ButtonBorder = Color.FromArgb(190, 200, 209)
    };

    public static AppThemeMode Mode { get; private set; } = AppThemeMode.Dark;

    public static ThemePalette Current => Mode == AppThemeMode.Dark ? DarkPalette : LightPalette;

    public static void SetMode(AppThemeMode mode)
    {
        Mode = mode;
    }

    public static void Apply(Form form)
    {
        form.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
        form.BackColor = Current.AppBackground;
        form.ForeColor = Current.TextPrimary;
        ApplyToControlHierarchy(form);
    }

    public static void ApplyToControlHierarchy(Control root)
    {
        StyleControl(root);
        foreach (Control child in root.Controls)
        {
            ApplyToControlHierarchy(child);
        }
    }

    public static void StyleButton(Button button, ThemeButtonKind kind = ThemeButtonKind.Secondary)
    {
        var palette = Current;
        button.FlatStyle = FlatStyle.Flat;
        button.UseVisualStyleBackColor = false;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = kind switch
        {
            ThemeButtonKind.Primary => Lighten(palette.ButtonPrimaryBackground, 12),
            ThemeButtonKind.Positive => Lighten(palette.PassBackground, 12),
            ThemeButtonKind.Negative => Lighten(palette.FailBackground, 12),
            _ => Lighten(palette.ButtonSecondaryBackground, 10)
        };
        button.FlatAppearance.MouseDownBackColor = kind switch
        {
            ThemeButtonKind.Primary => Darken(palette.ButtonPrimaryBackground, 10),
            ThemeButtonKind.Positive => Darken(palette.PassBackground, 10),
            ThemeButtonKind.Negative => Darken(palette.FailBackground, 10),
            _ => Darken(palette.ButtonSecondaryBackground, 8)
        };

        switch (kind)
        {
            case ThemeButtonKind.Primary:
                button.BackColor = palette.ButtonPrimaryBackground;
                button.ForeColor = palette.ButtonPrimaryForeground;
                button.FlatAppearance.BorderColor = Lighten(palette.ButtonPrimaryBackground, 10);
                break;
            case ThemeButtonKind.Positive:
                button.BackColor = palette.PassBackground;
                button.ForeColor = palette.PassForeground;
                button.FlatAppearance.BorderColor = Lighten(palette.PassBackground, 10);
                break;
            case ThemeButtonKind.Negative:
                button.BackColor = palette.FailBackground;
                button.ForeColor = palette.FailForeground;
                button.FlatAppearance.BorderColor = Lighten(palette.FailBackground, 10);
                break;
            default:
                button.BackColor = palette.ButtonSecondaryBackground;
                button.ForeColor = palette.ButtonSecondaryForeground;
                button.FlatAppearance.BorderColor = palette.ButtonBorder;
                break;
        }
    }

    public static void StyleSurface(Control control, bool elevated = false)
    {
        control.BackColor = elevated ? Current.SurfaceElevated : Current.Surface;
        control.ForeColor = Current.TextPrimary;
    }

    private static void StyleControl(Control control)
    {
        switch (control)
        {
            case TabPage tabPage:
                tabPage.BackColor = Current.Surface;
                tabPage.ForeColor = Current.TextPrimary;
                break;
            case Panel panel:
                if (ShouldUseThemeContainerBackColor(panel.BackColor))
                {
                    panel.BackColor = ContainerBackColor(panel);
                }

                panel.ForeColor = Current.TextPrimary;
                break;
            case GroupBox groupBox:
                groupBox.BackColor = Current.SurfaceElevated;
                groupBox.ForeColor = Current.TextPrimary;
                groupBox.FlatStyle = FlatStyle.Flat;
                break;
            case SplitContainer splitContainer:
                splitContainer.BackColor = Current.Border;
                splitContainer.Panel1.BackColor = Current.AppBackground;
                splitContainer.Panel2.BackColor = Current.AppBackground;
                break;
            case TreeView treeView:
                ConfigureTreeView(treeView);
                break;
            case ListView listView:
                ConfigureListView(listView);
                break;
            case DataGridView dataGridView:
                ConfigureDataGridView(dataGridView);
                break;
            case TabControl tabControl:
                ConfigureTabControl(tabControl);
                break;
            case TextBox textBox:
                textBox.BorderStyle = BorderStyle.FixedSingle;
                textBox.BackColor = textBox.ReadOnly ? Current.InputReadOnlyBackground : Current.InputBackground;
                textBox.ForeColor = Current.InputForeground;
                break;
            case ComboBox comboBox:
                comboBox.FlatStyle = FlatStyle.Flat;
                comboBox.BackColor = Current.InputBackground;
                comboBox.ForeColor = Current.InputForeground;
                ConfigureComboBox(comboBox);
                break;
            case CheckBox checkBox:
                checkBox.ForeColor = Current.TextPrimary;
                checkBox.BackColor = Color.Transparent;
                break;
            case RadioButton radioButton:
                radioButton.ForeColor = Current.TextPrimary;
                radioButton.BackColor = Color.Transparent;
                break;
            case Button button:
                StyleButton(button, GuessButtonKind(button));
                break;
            case Label label:
                label.ForeColor = Current.TextPrimary;
                break;
        }
    }

    private static bool ShouldUseThemeContainerBackColor(Color color)
    {
        return color.IsEmpty ||
               color.ToArgb() == Control.DefaultBackColor.ToArgb() ||
               color.ToArgb() == SystemColors.Control.ToArgb();
    }

    private static Color ContainerBackColor(Control control)
    {
        return control.Parent switch
        {
            GroupBox => Current.SurfaceElevated,
            TabPage => Current.Surface,
            Control parent when !parent.BackColor.IsEmpty => parent.BackColor,
            _ => Current.AppBackground
        };
    }

    private static ThemeButtonKind GuessButtonKind(Button button)
    {
        var text = button.Text.Trim().ToUpperInvariant();
        if (text == "PASS")
        {
            return ThemeButtonKind.Positive;
        }

        if (text == "FAIL")
        {
            return ThemeButtonKind.Negative;
        }

        if (text.Contains("BUILD") ||
            text.Contains("OPEN") ||
            text.Contains("NEW PROJECT") ||
            text.Contains("SAVE") ||
            text.Contains("APPROVE") ||
            text.Contains("RELEASE") ||
            text.Contains("EXPORT"))
        {
            return ThemeButtonKind.Primary;
        }

        return ThemeButtonKind.Secondary;
    }

    private static void ConfigureComboBox(ComboBox comboBox)
    {
        if (comboBox.DrawMode != DrawMode.OwnerDrawFixed)
        {
            comboBox.DrawMode = DrawMode.OwnerDrawFixed;
            comboBox.DrawItem += DrawComboBoxItem;
        }
    }

    private static void DrawComboBoxItem(object? sender, DrawItemEventArgs e)
    {
        var comboBox = (ComboBox)sender!;
        e.DrawBackground();

        var selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        var back = selected ? Current.SelectionBackground : comboBox.BackColor;
        var fore = selected ? Current.SelectionForeground : comboBox.ForeColor;

        using var backgroundBrush = new SolidBrush(back);
        e.Graphics.FillRectangle(backgroundBrush, e.Bounds);

        if (e.Index >= 0)
        {
            var text = comboBox.GetItemText(comboBox.Items[e.Index]);
            TextRenderer.DrawText(
                e.Graphics,
                text,
                comboBox.Font,
                e.Bounds,
                fore,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        e.DrawFocusRectangle();
    }

    private static void ConfigureTabControl(TabControl tabControl)
    {
        tabControl.BackColor = Current.Surface;
        tabControl.ForeColor = Current.TextPrimary;
        tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
        tabControl.SizeMode = TabSizeMode.Fixed;
        tabControl.ItemSize = new Size(120, 30);
        tabControl.Padding = new Point(18, 6);
        if (tabControl.Appearance != TabAppearance.Normal)
        {
            tabControl.Appearance = TabAppearance.Normal;
        }

        tabControl.DrawItem -= DrawTab;
        tabControl.DrawItem += DrawTab;
    }

    private static void DrawTab(object? sender, DrawItemEventArgs e)
    {
        if (sender is not TabControl tabControl || e.Index < 0 || e.Index >= tabControl.TabPages.Count)
        {
            return;
        }

        var selected = e.Index == tabControl.SelectedIndex;
        var bounds = e.Bounds;
        var back = selected ? Current.SurfaceElevated : Current.SurfaceAlt;
        var fore = selected ? Current.TextPrimary : Current.TextSecondary;

        if (e.Index == 0 && bounds.X > 0)
        {
            using var leftFillBrush = new SolidBrush(Current.Surface);
            e.Graphics.FillRectangle(leftFillBrush, 0, bounds.Y, bounds.X, bounds.Height);
        }

        if (e.Index == tabControl.TabCount - 1 && bounds.Right < tabControl.ClientSize.Width)
        {
            using var headerFillBrush = new SolidBrush(Current.Surface);
            e.Graphics.FillRectangle(headerFillBrush, bounds.Right, bounds.Y, tabControl.ClientSize.Width - bounds.Right, bounds.Height);
        }

        using var backgroundBrush = new SolidBrush(back);
        e.Graphics.FillRectangle(backgroundBrush, bounds);

        using var borderPen = new Pen(Current.Border);
        e.Graphics.DrawRectangle(borderPen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);

        if (selected)
        {
            using var accentBrush = new SolidBrush(Current.Accent);
            e.Graphics.FillRectangle(accentBrush, bounds.X + 1, bounds.Bottom - 3, bounds.Width - 2, 3);
        }

        TextRenderer.DrawText(
            e.Graphics,
            tabControl.TabPages[e.Index].Text,
            tabControl.Font,
            bounds,
            fore,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private static void ConfigureTreeView(TreeView treeView)
    {
        treeView.BackColor = Current.Surface;
        treeView.ForeColor = Current.TextPrimary;
        treeView.BorderStyle = BorderStyle.FixedSingle;
        treeView.LineColor = Current.Border;
        treeView.ItemHeight = 24;
        treeView.Indent = 22;
        if (treeView.DrawMode != TreeViewDrawMode.OwnerDrawText)
        {
            treeView.DrawMode = TreeViewDrawMode.OwnerDrawText;
            treeView.DrawNode += DrawTreeNode;
        }
    }

    private static void DrawTreeNode(object? sender, DrawTreeNodeEventArgs e)
    {
        if (sender is not TreeView treeView || e.Node is null)
        {
            return;
        }

        var selected = (e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected;
        var bounds = new Rectangle(0, e.Bounds.Top, treeView.ClientSize.Width, e.Bounds.Height);
        var back = selected ? Current.SelectionBackground : treeView.BackColor;
        var fore = selected ? Current.SelectionForeground : e.Node.ForeColor.IsEmpty ? treeView.ForeColor : e.Node.ForeColor;

        using var backgroundBrush = new SolidBrush(back);
        e.Graphics.FillRectangle(backgroundBrush, bounds);

        TextRenderer.DrawText(
            e.Graphics,
            e.Node.Text,
            e.Node.NodeFont ?? treeView.Font,
            e.Bounds,
            fore,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private const int LVM_FIRST = 0x1000;
    private const int LVM_SETEXTENDEDLISTVIEWSTYLE = LVM_FIRST + 54;
    private const int LVS_EX_DOUBLEBUFFER = 0x00010000;
    private const int LVS_EX_FULLROWSELECT = 0x00000020;

    [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    private static void ConfigureListView(ListView listView)
    {
        listView.BackColor = Current.Surface;
        listView.ForeColor = Current.TextPrimary;
        listView.BorderStyle = BorderStyle.FixedSingle;
        listView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        listView.OwnerDraw = true;
        listView.DrawColumnHeader -= DrawListViewHeader;
        listView.DrawItem -= DrawListViewItem;
        listView.DrawSubItem -= DrawListViewSubItem;
        listView.DrawColumnHeader += DrawListViewHeader;
        listView.DrawItem += DrawListViewItem;
        listView.DrawSubItem += DrawListViewSubItem;
        EnableListViewDoubleBuffering(listView);
        listView.HandleCreated -= ListViewHandleCreated;
        listView.HandleCreated += ListViewHandleCreated;
    }

    private static void ListViewHandleCreated(object? sender, EventArgs e)
    {
        if (sender is ListView listView)
        {
            EnableListViewDoubleBuffering(listView);
        }
    }

    private static void EnableListViewDoubleBuffering(ListView listView)
    {
        if (!listView.IsHandleCreated)
        {
            return;
        }

        const int extendedMask = LVS_EX_DOUBLEBUFFER | LVS_EX_FULLROWSELECT;
        SendMessage(
            listView.Handle,
            LVM_SETEXTENDEDLISTVIEWSTYLE,
            new IntPtr(extendedMask),
            new IntPtr(extendedMask));
    }

    private static void DrawListViewHeader(object? sender, DrawListViewColumnHeaderEventArgs e)
    {
        if (e.Header is null)
        {
            return;
        }

        using var backgroundBrush = new SolidBrush(Current.SurfaceAlt);
        using var borderPen = new Pen(Current.Border);
        e.Graphics.FillRectangle(backgroundBrush, e.Bounds);
        e.Graphics.DrawRectangle(borderPen, e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
        TextRenderer.DrawText(
            e.Graphics,
            e.Header.Text,
            e.Font,
            e.Bounds,
            Current.TextSecondary,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private static void DrawListViewItem(object? sender, DrawListViewItemEventArgs e)
    {
        if (sender is not ListView listView || e.Item is null)
        {
            return;
        }

        if (listView.View != View.Details)
        {
            e.DrawDefault = true;
            return;
        }

        // Each cell is fully painted by DrawListViewSubItem (bg + border + text).
        // Painting the row bg here as well caused partial-repaint blanking when
        // sub-item draw events did not re-fire for every cell during hover.
    }

    private static void DrawListViewSubItem(object? sender, DrawListViewSubItemEventArgs e)
    {
        if (sender is not ListView listView || e.Item is null || e.SubItem is null)
        {
            return;
        }

        var selected = e.Item.Selected;
        var bounds = e.Bounds;
        using var backgroundBrush = new SolidBrush(selected ? Current.SelectionBackground : Current.Surface);
        e.Graphics.FillRectangle(backgroundBrush, bounds);
        using var borderPen = new Pen(Current.Border);
        e.Graphics.DrawRectangle(borderPen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
        TextRenderer.DrawText(
            e.Graphics,
            e.SubItem.Text,
            listView.Font,
            bounds,
            selected ? Current.SelectionForeground : Current.TextPrimary,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private static void ConfigureDataGridView(DataGridView grid)
    {
        grid.BackgroundColor = Current.Surface;
        grid.BorderStyle = BorderStyle.FixedSingle;
        grid.GridColor = Current.Border;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        grid.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Current.SurfaceAlt;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Current.TextSecondary;
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Current.SurfaceAlt;
        grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Current.TextSecondary;
        grid.DefaultCellStyle.BackColor = Current.Surface;
        grid.DefaultCellStyle.ForeColor = Current.TextPrimary;
        grid.DefaultCellStyle.SelectionBackColor = Current.SelectionBackground;
        grid.DefaultCellStyle.SelectionForeColor = Current.SelectionForeground;
        grid.RowsDefaultCellStyle.BackColor = Current.Surface;
        grid.RowsDefaultCellStyle.ForeColor = Current.TextPrimary;
        grid.RowsDefaultCellStyle.SelectionBackColor = Current.SelectionBackground;
        grid.RowsDefaultCellStyle.SelectionForeColor = Current.SelectionForeground;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Current.SurfaceElevated;
        grid.AlternatingRowsDefaultCellStyle.ForeColor = Current.TextPrimary;
    }

    public static Color Lighten(Color color, int amount)
    {
        return Color.FromArgb(
            color.A,
            Math.Min(255, color.R + amount),
            Math.Min(255, color.G + amount),
            Math.Min(255, color.B + amount));
    }

    public static Color Darken(Color color, int amount)
    {
        return Color.FromArgb(
            color.A,
            Math.Max(0, color.R - amount),
            Math.Max(0, color.G - amount),
            Math.Max(0, color.B - amount));
    }
}
