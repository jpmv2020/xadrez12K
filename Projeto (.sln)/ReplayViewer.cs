using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Reflection;

namespace Xadrez_Louco_Novo
{
    public partial class ReplayViewer : Form
    {
        public ReplayViewer()
        {
            InitializeComponent();
            CleanBackColors();
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

        //Variaveis
        #region Variables
        int[,,] boardprints = new int[1000000, 8, 8]; //Array que guarda as posiçoes de cada peça em cada movimento [z,x,y]
        int movecount = 0; //Conta o numero de movimentos                                                     z=movimento / x=x / y=y
        int currshowing = 0; //Movimento apresentado de momento
        bool reverse = false; //Autoplay ao contrario
        string[] mergeholder = new string[1000000];
        string myExeDir = new FileInfo(Assembly.GetEntryAssembly().Location).Directory.ToString();
        #endregion

        //Transfere os movimentos do ficheiro para um array
        #region FillBoardPrints(ref int[,,] tofill, int[] filler, int z)
        private void FillBoardPrints(ref int[,,] tofill, int[] filler, int z)
        {
            int x1 = 0;
            int y1 = 0;
            int fillerC = 0;
            while (y1 <= 7) //Transfere as peças dum movimento em especifico dum array monodimensional para um tridimensional
            {
                tofill[z, x1, y1] = filler[fillerC];
                if (x1 == 7)
                {
                    y1++;
                    x1 = 0;
                }
                else
                    x1++;
                fillerC++;
            }
        }
        #endregion

        //Escolhe um ficheiro de replay para apresentar
        #region LoadReplay
        private void btn_loadreplay_Click(object sender, EventArgs e) //Isto foi coisas que eu pensei muito na hora e para explicar acho que complicava muito
                                                                      //Mas o q isto faz é tirar as coisas dum ficheiro trocar de array para array até chegar ao que se quer
        {
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                int arraycount = 0;
                string line = "";
                StreamReader sr = new StreamReader(openFile.FileName.ToString());
                while ((line = sr.ReadLine()) != null)
                {
                    arraycount++;
                    string[] start = line.Split('_');
                    int[] nums = Array.ConvertAll(start, Convert.ToInt32);
                    FillBoardPrints(ref boardprints, nums, arraycount - 1);
                }
                movecount = arraycount;


                for (int showcount = 0; showcount < arraycount; showcount++)
                {
                    listBox_sideXtra.Items.Insert(0, "");
                    for (int y = 0; y < 8; y++)
                    {
                        string stringS = "";
                        for (int x = 0; x < 8; x++)
                            stringS = stringS + boardprints[showcount, x, y] + " ";

                        listBox_sideXtra.Items.Insert(0, stringS);
                    }
                    listBox_sideXtra.Items.Insert(0, "Move: " + showcount);                    
                }

                UpdateBoard(0);
                currshowing = 0;
                CurrMoveShow();

                this.BackColor = Color.Green;
                btn_loadreplay.Enabled = false;
                timer_infoblink.Enabled = true;
            }
        }
        #endregion

        //Atualiza o tabuleiro = ao do jogo (mas com uma dimensão extra)
        #region UpdateBoard(int z)
        private void UpdateBoard(int z)
        {
            for (int x = 0; x <= 7; x++)
                for (int y = 0; y <= 7; y++)
                    this.Controls["board_" + x + "_" + y].BackgroundImage = pnglist_Tiles.Images[boardprints[z, x, y]];
        }
        #endregion

        //Pinta os quadrados de fundo = ao do jogo
        #region CleanBackColors()
        private void CleanBackColors()
        {
            for (int x = 0; x <= 7; x++)
                for (int y = 0; y <= 7; y++)
                    this.Controls["board_" + x + "_" + y].BackColor = Color.Silver;
            for (int x = 0; x <= 7; x++)
                for (int y = 0; y <= 7; y++)
                {
                    if (x == 0 && y == 0 || x % 2 == 0 && y % 2 == 0)
                    {
                        this.Controls["board_" + x + "_" + y].BackColor = Color.Gray;
                    }
                    if (x % 2 != 0 && y % 2 != 0)
                    {
                        this.Controls["board_" + x + "_" + y].BackColor = Color.Gray;
                    }
                }
        }
        #endregion

        //Mostra a proxima jogada
        #region NextMove
        private void btn_nexmove_Click(object sender, EventArgs e)
        {
            if (currshowing + 1 < movecount)
            {
                currshowing++;
                UpdateBoard(currshowing);
                CurrMoveShow();
            }
        }
        #endregion

        //Mostra a jogada anterior
        #region PrevMove
        private void btn_PrevMove_Click(object sender, EventArgs e)
        {
            if (currshowing - 1 >= 0)
            {
                currshowing--;
                UpdateBoard(currshowing);
                CurrMoveShow();
            }
        }
        #endregion

        //Fecha o viewer e volta para o jogo
        #region Close
        private void btn_close_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
        #endregion

        //Minimiza o viewer = ao do jogo
        #region Minimize
        private void btn_minimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        #endregion

        //Atualiza o nº do movimento que esta a ser apresentado
        #region CurrMoveShow()
        private void CurrMoveShow()
        {
            showmove.Text = currshowing.ToString();
        }
        #endregion

        //Passa por todos os movimentos automaticamente
        #region Autoplay
        private void autoplay_Click(object sender, EventArgs e) //Liga/desliga o timer
        {
            if(btn_autoplay.BackColor == Color.WhiteSmoke)
            {
                timer_autoplay.Enabled = true;
                btn_autoplay.BackColor = Color.DimGray;
            }
            else
            {
                timer_autoplay.Enabled = false;
                btn_autoplay.BackColor = Color.WhiteSmoke;
            }
        }
        private void timer_autoplay_Tick(object sender, EventArgs e) //Timer
        {
            if (!reverse)
                btn_nexmove.PerformClick();
            else
                btn_PrevMove.PerformClick();
        }
        private void autoplayspeed_SelectedIndexChanged(object sender, EventArgs e) //Define a velocidade do timer
        {
            timer_autoplay.Interval = int.Parse(autoplayspeed.Text);
        }
        #endregion

        //Autoplay ao contrario
        #region ReverseAutoPlay
        private void btn_autoplayreverse_Click(object sender, EventArgs e) //Liga/Desliga o reverse, que faz com que a reprodoção ande ao contrario
        {
            if (btn_autoplayreverse.BackColor == Color.WhiteSmoke)
            {
                reverse = true;
                btn_autoplayreverse.BackColor = Color.DimGray;
            }
            else
            {
                reverse = false;
                btn_autoplayreverse.BackColor = Color.WhiteSmoke;
            }               
        }
        #endregion

        //Menu de baixo
        #region BottomMenu
        private void btn_bottommenu_Click(object sender, EventArgs e) //Expande o form um pouco para baixo para mostrar alguns botões escondidos
        {
            if (this.Height == 420)
            {
                this.Height = 450;
                btn_bottommenu.BackColor = Color.DimGray;
            }
            else
            {
                this.Height = 420;
                btn_bottommenu.BackColor = Color.WhiteSmoke;
            }                
        }
        #endregion

        //Conbina dois replays em 1 só
        #region btn_mergereplays
        private void btn_mergereplays_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("To merge 2 files put them on the mergefolder folder and rename\nthem to '1.chessreplay' and '2.chessreplay'\nIs it done?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                int linecount = 0;
                string line = "";
                StreamReader sr = new StreamReader(myExeDir + "\\mergefolder\\"+ "1.chessreplay");
                StreamWriter sw = new StreamWriter(myExeDir + "\\mergefolder\\" + "mergedreplay.chessreplay");
                while ((line = sr.ReadLine()) != null)
                {
                    if (line != "\n")
                        mergeholder[linecount] = line;
                    linecount++;
                }
                sr.Close();
                sr = new StreamReader(myExeDir + "\\mergefolder\\" + "2.chessreplay");
                linecount = linecount - 1;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line != "\n")
                        mergeholder[linecount] = line;
                    linecount++;
                }

                for (int n = 0; n<=linecount; n++)
                {
                    if (n < linecount)
                        sw.WriteLine(mergeholder[n]);
                    else if (n == linecount)
                        sw.Write(mergeholder[n]);
                }
                sw.Close();

                this.BackColor = Color.Green;
                btn_mergereplays.Enabled = false;
                timer_infoblink.Enabled = true;
            }
        }
        #endregion

        //Timer que faz piscar o form
        #region timer_infoblink
        private void timer_infoblink_Tick(object sender, EventArgs e)
        {
            this.BackColor = Color.LightGray;
            btn_mergereplays.Enabled = true;
            btn_loadreplay.Enabled = true;
            timer_infoblink.Enabled = false;
        }
        #endregion
    }
}

//Tentei fazer esta parte de maneira a que se eu fizer outro jogo de tabuleiro 8x8, consiga copiar/colar e mudar só umas coisinhas pequenas para dar
//e acho que consegui +/-