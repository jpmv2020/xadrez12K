using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Xadrez_Louco_Novo
{
    public partial class PawnPromoBlack : Form
    {
        public PawnPromoBlack()
        {
            InitializeComponent();
        }

        //Codigo para mover a janela sem esta ter uma toolbar
        #region Mover Janela
        //Utilizo este grupinho muitas vezes, sempre que quero uma aplicação borderless (vi na net, obvio)
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        private void movewindow_trigger(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        #endregion

        private void select_queen_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void select_knight_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
        }

        private void select_tower_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
        }

        private void select_bishop_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
