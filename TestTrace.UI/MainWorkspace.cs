using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TestTrace.UI
{
    public partial class MainWorkspace : Form
    {
        // ===== Window drag support =====
        [DllImport("user32.dll")]
        private static extern void ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        private void BeginWindowDrag()
        {
            ReleaseCapture();
            SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
        }

        // Reduce flicker on a borderless, maximized host
        private void EnableFlickerReduction()
        {
            // Double buffering + avoid background erase
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }

        public MainWorkspace()
        {
            InitializeComponent();

            EnableFlickerReduction();

            // Apply global theme once
            ThemeManager.ApplyTheme(this);

            // Window setup
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;

            // MDL2 glyphs (buttons must use "Segoe MDL2 Assets" in Designer)
            btnMinimiseApp.Text = "\uE921"; // Minimise
            btnMaxRestoreApp.Text = "\uE923"; // Restore (window starts maximised)
            btnExitApp.Text = "\uE8BB"; // Close
        }

        private void btnTestTraceLaunch_Click(object sender, EventArgs e)
        {
            // Launch TestTrace Main Menu (modal, centred, owned by workspace)
            using (var mainMenu = new MainMenu())
            {
                mainMenu.StartPosition = FormStartPosition.CenterParent;
                mainMenu.ShowInTaskbar = false;              // keep taskbar clean
                mainMenu.ShowIcon = false;
                mainMenu.ShowDialog(this);
            }
        }

        private void btnExitApp_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnMinimiseApp_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnMaxRestoreApp_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                btnMaxRestoreApp.Text = "\uE922"; // Maximise icon
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
                btnMaxRestoreApp.Text = "\uE923"; // Restore icon
            }
        }

        private void PanelHeaderMW_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                BeginWindowDrag();
        }
    }
}
