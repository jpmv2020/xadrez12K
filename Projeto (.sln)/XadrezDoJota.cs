using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace Xadrez_Louco_Novo
{
    public partial class XadrezDoJota : Form
    {
        //Contrutor
        #region XadrezDoJota()
        public XadrezDoJota()
        {
            InitializeComponent();
            CleanBackColors();
            LogoStart();
        }
        #endregion

        //Codigo do Logo no inicio 
        #region Logo Code
        private int LogoTickCount = 0;
        private void LogoStart()
        {
            LogoPanel.Visible = true; //Fundo cinza visivel
            logo_panel2.Visible = true;
            LogoTickCount = 0; //Tick do timer para 0
            Picbox_logo.Image = Properties.Resources.JPLogoAnim; //Muda a imagem da picbox para o gif
            timer_logoFade.Enabled = true; //Liga o timer
        }
        private void timer_logoFade_Tick(object sender, EventArgs e) //Timer
        {
            float colorRGB = (191 - (LogoTickCount * 4.8f)) + 64; //Calcular a cor de branco para cinza
            label_Logo.ForeColor = Color.FromArgb((int)colorRGB, (int)colorRGB, (int)colorRGB); //Mudar a cor (texto)
            LogoTickCount++; //+1 tick
            if(colorRGB <= 64) //Se a cor for cinza
            {
                timer_logoFade.Enabled = false; //Desliga o timer            
                LogoEnd();
            }
        }
        private void LogoEnd() //Desligar Logo
        {
            Picbox_logo.Image = Properties.Resources.JPlogo1; //Remove a imagem da picbox (esta é uma q n se move)
            LogoPanel.Visible = false; //Poe o painel cinza invisivel
            logo_panel2.Visible = false;
            BoardFadeInStart();
        }
        private void BoardFadeInStart()
        {
            picbox_fadein.Image = Properties.Resources.BoardFadeIn;
            picbox_fadein.Visible = true;
            timer_boardFadeIn.Enabled = true;
        }
        private void timer_boardFadeIn_Tick(object sender, EventArgs e)
        {
            timer_boardFadeIn.Enabled = false;
            picbox_fadein.Visible = false;
            picbox_fadein.Image = null;
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            LogoStart();
        }
        #endregion

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
        //Variaveis usadas para escrever o historico de movimentos na listbox à direita
        string[] pieces = new string[17] { "empty", "peão branco", "bispo branco", "cavalo branco", "torre branca", "rainha branca", "rei branco", "empty", "empty", "empty", "empty", "peão preto", "bispo preto", "cavalo preto", "torre preta", "rainha preta", "rei preto" };
        string[] Xletter = new string[8] { "a", "b", "c", "d", "e", "f", "g", "h" };
        int movecount = 0;
        //Variaveis usadas para guardar posições relacionadas ao tabuleiro
        int[,] board_store_pieces = new int[8, 8];
        int[,] board_store_canmove = new int[8, 8];
        int[,] board_store_backcolors = new int[8, 8];
        int[,] check_store_whitepossiblemovements = new int[8, 8];
        int[,] check_store_blackpossiblemovements = new int[8, 8];
        //Guarda as posições clicadas
        int XClicked = 1;
        int YClicked = 1;        
        int XClickedPrev;
        int YClickedPrev;
        //Turno
        bool whiteturn = true;
        //Peça na casa em que se clicou
        int selectedpiece;
        //Rei e torres já se moveram ou não
        bool whitekingmoved = false;
        bool blackkingmoved = false;
        bool whiterighttowermoved = false;
        bool whitelefttowermoved = false;
        bool blackrighttowermoved = false;
        bool blacklefttowermoved = false;
        //Debug
        bool debugDelete = false;
        bool placequeen = false;
        //Som do rei em check
        System.Media.SoundPlayer checksoundplayer = new System.Media.SoundPlayer();
        bool Wkingoncheck = false;
        bool Bkingoncheck = false;
        //Variaveis relacionadas ao replay e saves
        bool willsavereplay = true;
        bool firstgame = true;
        string[] savelog = new string[0];
        int loglinecount = 0;
        string myExeDir = new FileInfo(Assembly.GetEntryAssembly().Location).Directory.ToString();
        string replayfilename;
        #endregion

        //Comandos Iniciais e de reset
        #region GameSetup()
        private void GameSetup()
        {
            //Cria novo replay
            CreateNewReplay();
            //Reset variaveis
            whiteturn = true; //Reinicia algumas variaveis
            movecount = 0;           
            Wkingoncheck = false;
            Bkingoncheck = false;            
            whitekingmoved = false;
            blackkingmoved = false;
            whiterighttowermoved = false;
            whitelefttowermoved = false;
            blackrighttowermoved = false;
            blacklefttowermoved = false;
            listbox_side.Items.Clear(); //Limpa listbox
            listbox_side.Items.Insert(0, "Move Log:");
            listbox_side.Items.Add("");
            listbox_side.Items.Add("");
            checksoundplayer.Stop(); //Para sons
            willsavereplay = true;
            

            for (int x = 0; x < 8; x++) //Limpa os arrays relacionados ao tabuleiro
                for (int y = 0; y < 8; y++)
                {
                    board_store_backcolors[x, y] = 0;
                    check_store_blackpossiblemovements[x, y] = 0;
                    check_store_whitepossiblemovements[x, y] = 0;
                    board_store_canmove[x, y] = 0;                  
                }
            ResetBoardPositions(); //Coloca as peças nas posiçoes iniciais
            CleanBackColors(); //Limpa as cores de fundo / pinta os quadrados cinza e cinza escuro
            DrawUpdateBoard(); //Atualiza o tabuleiro
            if (willsavereplay) //Começa a guardar, ou não, o replay
                SaveReplayFile();
            btn_savefile.Enabled = true;
        }
        #endregion

        //Renomeia replays para guardar muitos
        #region CreateNewReplay()
        private void CreateNewReplay()
        {
            string replayfolder = myExeDir + "\\replays\\";
            int count = 0;
            do
            {
                count++;
                replayfilename = "replay" + count + ".chessreplay";
            } while (File.Exists(replayfolder + replayfilename));
            using (StreamWriter sw = File.CreateText(replayfolder + replayfilename)) ;
        }
        #endregion

        //Coloca as peças nas posiçoes iniciais
        #region ResetBoardPositions()
        private void ResetBoardPositions()
        {
            //Clear Board
            for (int x = 0; x < 8; x++)
                for (int y = 0; y < 8; y++)
                    board_store_pieces[x, y] = 0;
            //White
            #region Place White
            //Peão
            for (int x = 0; x <= 7; x++)
                board_store_pieces[x, 1] = 1;
            //Torres
            board_store_pieces[0, 0] = board_store_pieces[7, 0] = 4;
            //Cavalos
            board_store_pieces[1, 0] = board_store_pieces[6, 0] = 3;
            //Bispo
            board_store_pieces[2, 0] = board_store_pieces[5, 0] = 2;
            //Rainha
            board_store_pieces[3, 0] = 5;
            //Rei
            board_store_pieces[4, 0] = 6;
            #endregion
            //Black
            #region Place Black
            //Peão
            for (int x = 0; x <= 7; x++)
                board_store_pieces[x, 6] = 11;
            //Torres
            board_store_pieces[0, 7] = board_store_pieces[7, 7] = 14;
            //Cavalos
            board_store_pieces[1, 7] = board_store_pieces[6, 7] = 13;
            //Bispo
            board_store_pieces[2, 7] = board_store_pieces[5, 7] = 12;
            //Rainha
            board_store_pieces[3, 7] = 15;
            //Rei
            board_store_pieces[4, 7] = 16;
            #endregion
        }
        #endregion

        //Atualiza as quadriculas (visualmente)
        #region DrawUpdateBoard()
        private void DrawUpdateBoard()
        {
            for (int x = 0; x <= 7; x++) //Coloca nas pictureboxes as peças guardadas no array
                for (int y = 0; y <= 7; y++)
                    this.Controls["board_" + x + "_" + y].BackgroundImage = pnglist_Tiles.Images[board_store_pieces[x, y]];                
        }
        #endregion

        //Atualiza as cores de fundo (como para selecionar)
        #region DrawUpdateBackColors()
        private void DrawUpdateBackColors()
        {
            for (int x = 0; x < 8; x++) //Atualiza as cores de fundo de acordo com o guardado no array
                for (int y = 0; y < 8; y++)
                {
                    if (board_store_backcolors[x, y] == 1)                          //1 = King on check
                    {
                        this.Controls["board_" + x + "_" + y].BackColor = Color.Red;
                    }
                    else if (board_store_backcolors[x, y] == 2)                     //2 = Selected his pieces
                    {
                        this.Controls["board_" + x + "_" + y].BackColor = Color.Aqua;
                    }
                    else if (board_store_backcolors[x, y] == 3)                     //3 = Selected others pieces
                    {
                        this.Controls["board_" + x + "_" + y].BackColor = Color.Salmon;
                    }
                    else if (board_store_backcolors[x, y] == 4)                     //4 = Can Move
                    {
                        this.Controls["board_" + x + "_" + y].BackColor = Color.LightGreen;
                    }
                    else if (board_store_backcolors[x, y] == 5)                     //5 = Can be killed
                    {
                        this.Controls["board_" + x + "_" + y].BackColor = Color.OrangeRed;
                    }
                    else if (board_store_backcolors[x, y] == 6)                     //6 = Captura En Passant
                    {
                        this.Controls["board_" + x + "_" + y].BackColor = Color.Chocolate;
                    }
                    else if (board_store_backcolors[x, y] == 7)                     //7 = Roque
                    {
                        this.Controls["board_" + x + "_" + y].BackColor = Color.MediumPurple;
                    }
                    else if (board_store_backcolors[x, y] == 8)                     //8 = Rei nao pode mover-se
                    {
                        this.Controls["board_" + x + "_" + y].BackColor = Color.Purple;
                    }
                }
        }
        #endregion

        //Limpa as cores de fundo
        #region CleanBackColors()
        private void CleanBackColors()
        {
            for (int x = 0; x <= 7; x++) //Limpa as cores de fundo e penta os quadrados cinza/cinzaescuro
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

        //Desseleciona
        #region DesselectPiece()
        private void DesselectPiece()
        {
            CleanBackColors(); //Limpa cores de fundo
            for (int x = 0; x < 8; x++) //Reset aos sitios que a peça pode mover e às cores correspondentes
                for (int y = 0; y < 8; y++)
                {
                    if (board_store_canmove[x,y] != 0)
                        board_store_backcolors[x, y] = 0;

                    board_store_canmove[x, y] = 0;
                }
            WhitePossibleMoviment(); //Calcula se algum rei está em check
            BlackPossibleMoviment();
            DrawUpdateBackColors(); //Atualiza as cores de fundo no tabuleiro
        }
        #endregion

        //Muda o turno
        #region ChangeTurn()
        private void ChangeTurn()
        {
            if (whiteturn)
                whiteturn = false;
            else
                whiteturn = true;
        }
        #endregion

        //Comandos a executar quando se clica numa quadricula
        #region ClickCommands()
        private void ClickCommands()
        {
            if (debugDelete == true) //Debug
            {
                board_store_pieces[XClicked, YClicked] = 0;
                DrawUpdateBoard();
            }
            else if (placequeen == true) //Debug
            {
                if (whiteturn)
                    board_store_pieces[XClicked, YClicked] = 5;
                else
                    board_store_pieces[XClicked, YClicked] = 15;
                DrawUpdateBoard();
            }
            else if (board_store_canmove[XClicked, YClicked] == 0) //Seleciona ou desceleciona uma peça
            {
                if (board_store_pieces[XClicked, YClicked] == 0)
                    DesselectPiece();
                else
                    SelectPiece();
            }
            else if (board_store_canmove[XClicked, YClicked] == 1) //Move uma peça
            {
                MovePiece();
            }
            else if (board_store_canmove[XClicked,YClicked] == 2) //Captura En Passant (regra especial dos peões)
            {
                MoveEnPassant();
            }
            else if (board_store_canmove[XClicked, YClicked] == 3) //Movimento Roque (trocar torre/rei duma forma meio louca)
            {
                MoveRoque();
            }

            XClickedPrev = XClicked;
            YClickedPrev = YClicked;
        }
        #endregion

        //Seleciona uma peça (mostra para onde pode mover-se ou não)
        #region SelectPiece()
        private void SelectPiece()
        {
            DesselectPiece(); //Desceleciona

            selectedpiece = board_store_pieces[XClicked, YClicked]; //Guarda a peça selecionada

            if (selectedpiece >= 1 && selectedpiece <= 6 && whiteturn) //Peça branca, turno branco
            {
                board_store_backcolors[XClicked, YClicked] = 2;
                board_store_canmove[XClicked, YClicked] = -1;
                PieceMovementWhite(); //Mostra o movimento possivel da peça selecionada
                DrawUpdateBackColors();
            }
            else if (selectedpiece >= 11 && selectedpiece <= 16 && whiteturn) //Peça preta, turno branco
            {
                board_store_backcolors[XClicked, YClicked] = 3;
                board_store_canmove[XClicked, YClicked] = -1;
                DrawUpdateBackColors();
            }
            else if (selectedpiece >= 11 && selectedpiece <= 16 && !whiteturn) //Peça preta, turno preto
            {
                board_store_backcolors[XClicked, YClicked] = 2;
                board_store_canmove[XClicked, YClicked] = -1;
                PieceMovementBlack(); //Mostra o movimento possivel da peça selecionada
                DrawUpdateBackColors();

            }
            else if (selectedpiece >= 1 && selectedpiece <= 6 && !whiteturn) //Peça branca, turno preto
            {
                board_store_backcolors[XClicked, YClicked] = 3;
                board_store_canmove[XClicked, YClicked] = -1;
                DrawUpdateBackColors();
            }
        }
        #endregion

        //Guarda o movimento das peças brancas
        #region PieceMovementWhite()
        private void PieceMovementWhite()
        {
            //Peão Branco
            #region PeãoBranco
            if (selectedpiece == 1)
            {
                //Movimento normal
                if (YClicked + 1 <= 7 && board_store_pieces[XClicked,YClicked+1] == 0) 
                {
                    board_store_canmove[XClicked, YClicked + 1] = 1;
                    board_store_backcolors[XClicked, YClicked + 1] = 4;                    
                }
                if (YClicked == 1 && YClicked + 2 <= 7 && board_store_pieces[XClicked, YClicked + 2] == 0 && board_store_pieces[XClicked, YClicked + 1] == 0)  
                {
                    board_store_canmove[XClicked, YClicked + 2] = 1;
                    board_store_backcolors[XClicked, YClicked + 2] = 4;
                }

                //Comidela
                if (XClicked+1 <= 7 && XClicked+1 >= 0 && YClicked + 1 <= 7 && board_store_pieces[XClicked + 1, YClicked + 1] <= 16 && board_store_pieces[XClicked + 1, YClicked + 1] >= 11) 
                {
                    board_store_canmove[XClicked+1, YClicked + 1] = 1;
                    board_store_backcolors[XClicked+1, YClicked + 1] = 5;
                }
                if (XClicked-1 <= 7 && XClicked-1 >= 0 && YClicked + 1 <= 7 && board_store_pieces[XClicked - 1, YClicked + 1] <= 16 && board_store_pieces[XClicked - 1, YClicked + 1] >= 11)
                {
                    board_store_canmove[XClicked - 1, YClicked + 1] = 1;
                    board_store_backcolors[XClicked - 1, YClicked + 1] = 5;
                }

                //Captura En Passant
                if (XClicked + 1 <= 7 && XClicked + 1 >= 0 && YClicked == 4 && board_store_pieces[XClicked + 1, YClicked + 1] == 0 && board_store_pieces[XClicked + 1, YClicked] == 11 && YClicked == 4)
                {
                    board_store_canmove[XClicked + 1, YClicked + 1] = 2;
                    board_store_canmove[XClicked + 1, YClicked] = 99;
                    board_store_backcolors[XClicked + 1, YClicked] = 5;
                    board_store_backcolors[XClicked + 1, YClicked + 1] = 6;
                }
                if (XClicked - 1 <= 7 && XClicked - 1 >= 0 && YClicked == 4 && board_store_pieces[XClicked - 1, YClicked + 1] == 0 && board_store_pieces[XClicked - 1, YClicked] == 11 && YClicked == 4)
                {
                    board_store_canmove[XClicked - 1, YClicked + 1] = 2;
                    board_store_canmove[XClicked - 1, YClicked] = 99;
                    board_store_backcolors[XClicked - 1, YClicked] = 5;
                    board_store_backcolors[XClicked - 1, YClicked + 1] = 6;
                }
            }
            #endregion

            //Bispo Branco
            #region BispoBranco
            if (selectedpiece == 2)
            {
                //Baixo/esquerda
                for (int n = -1; n >= -7; n--)
                {
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked + n, YClicked + n] == 0)
                    {
                        board_store_canmove[XClicked + n, YClicked + n] = 1;
                        board_store_backcolors[XClicked + n, YClicked + n] = 4;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked + n, YClicked + n] <= 16 && board_store_pieces[XClicked + n, YClicked + n] >= 11)
                    {
                        board_store_canmove[XClicked + n, YClicked + n] = 1;
                        board_store_backcolors[XClicked + n, YClicked + n] = 5;
                        n = -999;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked + n, YClicked + n] >= 1 && board_store_pieces[XClicked + n, YClicked + n] <= 6)
                    {
                        n = -999;
                    }
                }
                //Cima/Direita
                for (int n = 1; n <= 7; n++)
                {
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked + n, YClicked + n] == 0)
                    {
                        board_store_canmove[XClicked + n, YClicked + n] = 1;
                        board_store_backcolors[XClicked + n, YClicked + n] = 4;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked + n, YClicked + n] <= 16 && board_store_pieces[XClicked + n, YClicked + n] >= 11)
                    {
                        board_store_canmove[XClicked + n, YClicked + n] = 1;
                        board_store_backcolors[XClicked + n, YClicked + n] = 5;
                        n = 999;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked + n, YClicked + n] >= 1 && board_store_pieces[XClicked + n, YClicked + n] <= 6)
                    {
                        n = 999;
                    }
                }
                //baixo/direita
                for (int n = -1; n >= -7; n--)
                {
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked - n) >= 0 && (YClicked - n) <= 7 && board_store_pieces[XClicked + n, YClicked - n] == 0)
                    {
                        board_store_canmove[XClicked + n, YClicked - n] = 1;
                        board_store_backcolors[XClicked + n, YClicked - n] = 4;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked - n) >= 0 && (YClicked - n) <= 7 && board_store_pieces[XClicked + n, YClicked - n] <= 16 && board_store_pieces[XClicked + n, YClicked - n] >= 11)
                    {
                        board_store_canmove[XClicked + n, YClicked - n] = 1;
                        board_store_backcolors[XClicked + n, YClicked - n] = 5;
                        n = -999;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked - n) >= 0 && (YClicked - n) <= 7 && board_store_pieces[XClicked + n, YClicked - n] >= 1 && board_store_pieces[XClicked + n, YClicked - n] <= 6)
                    {
                        n = -999;
                    }
                }
                //cima/esquerda
                for (int n = 1; n <= 7; n++)
                {
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked - n) >= 0 && (YClicked - n) <= 7 && board_store_pieces[XClicked + n, YClicked - n] == 0)
                    {
                        board_store_canmove[XClicked + n, YClicked - n] = 1;
                        board_store_backcolors[XClicked + n, YClicked - n] = 4;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked - n) >= 0 && (YClicked - n) <= 7 && board_store_pieces[XClicked + n, YClicked - n] <= 16 && board_store_pieces[XClicked + n, YClicked - n] >= 11)
                    {
                        board_store_canmove[XClicked + n, YClicked - n] = 1;
                        board_store_backcolors[XClicked + n, YClicked - n] = 5;
                        n = 999;
                    }
                    else if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked - n) >= 0 && (YClicked - n) <= 7 && board_store_pieces[XClicked + n, YClicked - n] >= 1 && board_store_pieces[XClicked + n, YClicked - n] <= 6)
                    {
                        n = 999;
                    }
                }
            }
            #endregion

            //Cavalo Branco
            #region CavaloBranco
            if (selectedpiece == 3)
            {
                if ((XClicked + 1) >= 0 && (XClicked + 1) <= 7 && (YClicked + 2) >= 0 && (YClicked + 2) <= 7 && board_store_pieces[XClicked + 1, YClicked + 2] == 0)
                {
                    board_store_canmove[XClicked + 1, YClicked + 2] = 1;
                    board_store_backcolors[XClicked + 1, YClicked + 2] = 4;
                }
                if ((XClicked + 1) >= 0 && (XClicked + 1) <= 7 && (YClicked + 2) >= 0 && (YClicked + 2) <= 7 && board_store_pieces[XClicked + 1, YClicked + 2] >= 11 && board_store_pieces[XClicked + 1, YClicked + 2] <= 16)
                {
                    board_store_canmove[XClicked + 1, YClicked + 2] = 1;
                    board_store_backcolors[XClicked + 1, YClicked + 2] = 5;
                }

                if ((XClicked + 2) >= 0 && (XClicked + 2) <= 7 && (YClicked + 1) >= 0 && (YClicked + 1) <= 7 && board_store_pieces[XClicked + 2, YClicked + 1] == 0)
                {
                    board_store_canmove[XClicked + 2, YClicked + 1] = 1;
                    board_store_backcolors[XClicked + 2, YClicked + 1] = 4;
                }
                if ((XClicked + 2) >= 0 && (XClicked + 2) <= 7 && (YClicked + 1) >= 0 && (YClicked + 1) <= 7 && board_store_pieces[XClicked + 2, YClicked + 1] >= 11 && board_store_pieces[XClicked + 2, YClicked + 1] <= 16)
                {
                    board_store_canmove[XClicked + 2, YClicked + 1] = 1;
                    board_store_backcolors[XClicked + 2, YClicked + 1] = 5;
                }

                if ((XClicked - 1) >= 0 && (XClicked - 1) <= 7 && (YClicked + 2) >= 0 && (YClicked + 2) <= 7 && board_store_pieces[XClicked - 1, YClicked + 2] == 0)
                {
                    board_store_canmove[XClicked - 1, YClicked + 2] = 1;
                    board_store_backcolors[XClicked - 1, YClicked + 2] = 4;
                }
                if ((XClicked - 1) >= 0 && (XClicked - 1) <= 7 && (YClicked + 2) >= 0 && (YClicked + 2) <= 7 && board_store_pieces[XClicked - 1, YClicked + 2] >= 11 && board_store_pieces[XClicked - 1, YClicked + 2] <= 16)
                {
                    board_store_canmove[XClicked - 1, YClicked + 2] = 1;
                    board_store_backcolors[XClicked - 1, YClicked + 2] = 5;
                }

                if ((XClicked - 2) >= 0 && (XClicked - 2) <= 7 && (YClicked + 1) >= 0 && (YClicked + 1) <= 7 && board_store_pieces[XClicked - 2, YClicked + 1] == 0)
                {
                    board_store_canmove[XClicked - 2, YClicked + 1] = 1;
                    board_store_backcolors[XClicked - 2, YClicked + 1] = 4;
                }
                if ((XClicked - 2) >= 0 && (XClicked - 2) <= 7 && (YClicked + 1) >= 0 && (YClicked + 1) <= 7 && board_store_pieces[XClicked - 2, YClicked + 1] >= 11 && board_store_pieces[XClicked - 2, YClicked + 1] <= 16)
                {
                    board_store_canmove[XClicked - 2, YClicked + 1] = 1;
                    board_store_backcolors[XClicked - 2, YClicked + 1] = 5;
                }
                //
                if ((XClicked + 1) >= 0 && (XClicked + 1) <= 7 && (YClicked - 2) >= 0 && (YClicked - 2) <= 7 && board_store_pieces[XClicked + 1, YClicked - 2] == 0)
                {
                    board_store_canmove[XClicked + 1, YClicked - 2] = 1;
                    board_store_backcolors[XClicked + 1, YClicked - 2] = 4;
                }
                if ((XClicked + 1) >= 0 && (XClicked + 1) <= 7 && (YClicked - 2) >= 0 && (YClicked - 2) <= 7 && board_store_pieces[XClicked + 1, YClicked - 2] >= 11 && board_store_pieces[XClicked + 1, YClicked - 2] <= 16)
                {
                    board_store_canmove[XClicked + 1, YClicked - 2] = 1;
                    board_store_backcolors[XClicked + 1, YClicked - 2] = 5;
                }

                if ((XClicked + 2) >= 0 && (XClicked + 2) <= 7 && (YClicked - 1) >= 0 && (YClicked - 1) <= 7 && board_store_pieces[XClicked + 2, YClicked - 1] == 0)
                {
                    board_store_canmove[XClicked + 2, YClicked - 1] = 1;
                    board_store_backcolors[XClicked + 2, YClicked - 1] = 4;
                }
                if ((XClicked + 2) >= 0 && (XClicked + 2) <= 7 && (YClicked - 1) >= 0 && (YClicked - 1) <= 7 && board_store_pieces[XClicked + 2, YClicked - 1] >= 11 && board_store_pieces[XClicked + 2, YClicked - 1] <= 16)
                {
                    board_store_canmove[XClicked + 2, YClicked - 1] = 1;
                    board_store_backcolors[XClicked + 2, YClicked - 1] = 5;
                }

                if ((XClicked - 1) >= 0 && (XClicked - 1) <= 7 && (YClicked - 2) >= 0 && (YClicked - 2) <= 7 && board_store_pieces[XClicked - 1, YClicked - 2] == 0)
                {
                    board_store_canmove[XClicked - 1, YClicked - 2] = 1;
                    board_store_backcolors[XClicked - 1, YClicked - 2] = 4;
                }
                if ((XClicked - 1) >= 0 && (XClicked - 1) <= 7 && (YClicked - 2) >= 0 && (YClicked - 2) <= 7 && board_store_pieces[XClicked - 1, YClicked - 2] >= 11 && board_store_pieces[XClicked - 1, YClicked - 2] <= 16)
                {
                    board_store_canmove[XClicked - 1, YClicked - 2] = 1;
                    board_store_backcolors[XClicked - 1, YClicked - 2] = 5;
                }

                if ((XClicked - 2) >= 0 && (XClicked - 2) <= 7 && (YClicked - 1) >= 0 && (YClicked - 1) <= 7 && board_store_pieces[XClicked - 2, YClicked - 1] == 0)
                {
                    board_store_canmove[XClicked - 2, YClicked - 1] = 1;
                    board_store_backcolors[XClicked - 2, YClicked - 1] = 4;
                }
                if ((XClicked - 2) >= 0 && (XClicked - 2) <= 7 && (YClicked - 1) >= 0 && (YClicked - 1) <= 7 && board_store_pieces[XClicked - 2, YClicked - 1] >= 11 && board_store_pieces[XClicked - 2, YClicked - 1] <= 16)
                {
                    board_store_canmove[XClicked - 2, YClicked - 1] = 1;
                    board_store_backcolors[XClicked - 2, YClicked - 1] = 5;
                }
            }
            #endregion

            //Torre Branca
            #region TorreBranca
            if (selectedpiece == 4)
            {                
                //Cima
                for (int n = 1; n <= 7; n++)
                {
                    if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked, YClicked + n] == 0)
                    {
                        board_store_canmove[XClicked, YClicked + n] = 1;
                        board_store_backcolors[XClicked, YClicked + n] = 4;
                    }
                    if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked, YClicked + n] <= 16 && board_store_pieces[XClicked, YClicked + n] >= 11)
                    {
                        board_store_canmove[XClicked, YClicked + n] = 1;
                        board_store_backcolors[XClicked, YClicked + n] = 5;
                        n = 999;
                    }
                    if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked, YClicked + n] >= 1 && board_store_pieces[XClicked, YClicked + n] <= 6)
                    {
                        n = 999;
                    }
                }

                //Baixo
                for (int n = -1; n >= -7; n--)
                {
                    if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked, YClicked + n] == 0)
                    {
                        board_store_canmove[XClicked, YClicked + n] = 1;
                        board_store_backcolors[XClicked, YClicked + n] = 4;
                    }
                    if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked, YClicked + n] <= 16 && board_store_pieces[XClicked, YClicked + n] >= 11)
                    {
                        board_store_canmove[XClicked, YClicked + n] = 1;
                        board_store_backcolors[XClicked, YClicked + n] = 5;
                        n = -999;
                    }
                    if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked, YClicked + n] >= 1 && board_store_pieces[XClicked, YClicked + n] <= 6)
                    {
                        n = -999;
                    }
                }

                //Direita
                for (int n = 1; n <= 7; n++)
                {
                    if ((XClicked+n) >= 0 && (XClicked+n) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && board_store_pieces[XClicked+n, YClicked] == 0)
                    {
                        board_store_canmove[XClicked+n, YClicked] = 1;
                        board_store_backcolors[XClicked+n, YClicked] = 4;
                    }
                    if ((XClicked+n) >= 0 && (XClicked+n) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && board_store_pieces[XClicked+n, YClicked] <= 16 && board_store_pieces[XClicked+n, YClicked] >= 11)
                    {
                        board_store_canmove[XClicked+n, YClicked] = 1;
                        board_store_backcolors[XClicked+n, YClicked] = 5;
                        n = 999;
                    }
                    if ((XClicked+n) >= 0 && (XClicked+n) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && board_store_pieces[XClicked+n, YClicked] >= 1 && board_store_pieces[XClicked+n, YClicked] <= 6)
                    {
                        n = 999;
                    }
                }

                //Esquerda
                for (int n = -1; n >= -7; n--)
                {
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && board_store_pieces[XClicked + n, YClicked] == 0)
                    {
                        board_store_canmove[XClicked + n, YClicked] = 1;
                        board_store_backcolors[XClicked + n, YClicked] = 4;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && board_store_pieces[XClicked + n, YClicked] <= 16 && board_store_pieces[XClicked + n, YClicked] >= 11)
                    {
                        board_store_canmove[XClicked + n, YClicked] = 1;
                        board_store_backcolors[XClicked + n, YClicked] = 5;
                        n = -999;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && board_store_pieces[XClicked + n, YClicked] >= 1 && board_store_pieces[XClicked + n, YClicked] <= 6)
                    {
                        n = -999;
                    }
                }
            }

            #endregion

            //Rainha Branca
            #region RainhaBranca
            if (selectedpiece == 5)
            {
                //Cima
                for (int n = 1; n <= 7; n++)
                {
                    if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked, YClicked + n] == 0)
                    {
                        board_store_canmove[XClicked, YClicked + n] = 1;
                        board_store_backcolors[XClicked, YClicked + n] = 4;
                    }
                    if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked, YClicked + n] <= 16 && board_store_pieces[XClicked, YClicked + n] >= 11)
                    {
                        board_store_canmove[XClicked, YClicked + n] = 1;
                        board_store_backcolors[XClicked, YClicked + n] = 5;
                        n = 999;
                    }
                    if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked, YClicked + n] >= 1 && board_store_pieces[XClicked, YClicked + n] <= 6)
                    {
                        n = 999;
                    }
                }

                //Baixo
                for (int n = -1; n >= -7; n--)
                {
                    if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked, YClicked + n] == 0)
                    {
                        board_store_canmove[XClicked, YClicked + n] = 1;
                        board_store_backcolors[XClicked, YClicked + n] = 4;
                    }
                    if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked, YClicked + n] <= 16 && board_store_pieces[XClicked, YClicked + n] >= 11)
                    {
                        board_store_canmove[XClicked, YClicked + n] = 1;
                        board_store_backcolors[XClicked, YClicked + n] = 5;
                        n = -999;
                    }
                    if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked, YClicked + n] >= 1 && board_store_pieces[XClicked, YClicked + n] <= 6)
                    {
                        n = -999;
                    }
                }

                //Direita
                for (int n = 1; n <= 7; n++)
                {
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && board_store_pieces[XClicked + n, YClicked] == 0)
                    {
                        board_store_canmove[XClicked + n, YClicked] = 1;
                        board_store_backcolors[XClicked + n, YClicked] = 4;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && board_store_pieces[XClicked + n, YClicked] <= 16 && board_store_pieces[XClicked + n, YClicked] >= 11)
                    {
                        board_store_canmove[XClicked + n, YClicked] = 1;
                        board_store_backcolors[XClicked + n, YClicked] = 5;
                        n = 999;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && board_store_pieces[XClicked + n, YClicked] >= 1 && board_store_pieces[XClicked + n, YClicked] <= 6)
                    {
                        n = 999;
                    }
                }

                //Esquerda
                for (int n = -1; n >= -7; n--)
                {
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && board_store_pieces[XClicked + n, YClicked] == 0)
                    {
                        board_store_canmove[XClicked + n, YClicked] = 1;
                        board_store_backcolors[XClicked + n, YClicked] = 4;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && board_store_pieces[XClicked + n, YClicked] <= 16 && board_store_pieces[XClicked + n, YClicked] >= 11)
                    {
                        board_store_canmove[XClicked + n, YClicked] = 1;
                        board_store_backcolors[XClicked + n, YClicked] = 5;
                        n = -999;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && board_store_pieces[XClicked + n, YClicked] >= 1 && board_store_pieces[XClicked + n, YClicked] <= 6)
                    {
                        n = -999;
                    }
                }
                //Baixo/esquerda
                for (int n = -1; n >= -7; n--)
                {
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked + n, YClicked + n] == 0)
                    {
                        board_store_canmove[XClicked + n, YClicked + n] = 1;
                        board_store_backcolors[XClicked + n, YClicked + n] = 4;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked + n, YClicked + n] <= 16 && board_store_pieces[XClicked + n, YClicked + n] >= 11)
                    {
                        board_store_canmove[XClicked + n, YClicked + n] = 1;
                        board_store_backcolors[XClicked + n, YClicked + n] = 5;
                        n = -999;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked + n, YClicked + n] >= 1 && board_store_pieces[XClicked + n, YClicked + n] <= 6)
                    {
                        n = -999;
                    }
                }
                //Cima/Direita
                for (int n = 1; n <= 7; n++)
                {
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked + n, YClicked + n] == 0)
                    {
                        board_store_canmove[XClicked + n, YClicked + n] = 1;
                        board_store_backcolors[XClicked + n, YClicked + n] = 4;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked + n, YClicked + n] <= 16 && board_store_pieces[XClicked + n, YClicked + n] >= 11)
                    {
                        board_store_canmove[XClicked + n, YClicked + n] = 1;
                        board_store_backcolors[XClicked + n, YClicked + n] = 5;
                        n = 999;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked + n, YClicked + n] >= 1 && board_store_pieces[XClicked + n, YClicked + n] <= 6)
                    {
                        n = 999;
                    }
                }
                //baixo/direita
                for (int n = -1; n >= -7; n--)
                {
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked - n) >= 0 && (YClicked - n) <= 7 && board_store_pieces[XClicked + n, YClicked - n] == 0)
                    {
                        board_store_canmove[XClicked + n, YClicked - n] = 1;
                        board_store_backcolors[XClicked + n, YClicked - n] = 4;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked - n) >= 0 && (YClicked - n) <= 7 && board_store_pieces[XClicked + n, YClicked - n] <= 16 && board_store_pieces[XClicked + n, YClicked - n] >= 11)
                    {
                        board_store_canmove[XClicked + n, YClicked - n] = 1;
                        board_store_backcolors[XClicked + n, YClicked - n] = 5;
                        n = -999;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked - n) >= 0 && (YClicked - n) <= 7 && board_store_pieces[XClicked + n, YClicked - n] >= 1 && board_store_pieces[XClicked + n, YClicked - n] <= 6)
                    {
                        n = -999;
                    }
                }
                //cima/esquerda
                for (int n = 1; n <= 7; n++)
                {
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked - n) >= 0 && (YClicked - n) <= 7 && board_store_pieces[XClicked + n, YClicked - n] == 0)
                    {
                        board_store_canmove[XClicked + n, YClicked - n] = 1;
                        board_store_backcolors[XClicked + n, YClicked - n] = 4;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked - n) >= 0 && (YClicked - n) <= 7 && board_store_pieces[XClicked + n, YClicked - n] <= 16 && board_store_pieces[XClicked + n, YClicked - n] >= 11)
                    {
                        board_store_canmove[XClicked + n, YClicked - n] = 1;
                        board_store_backcolors[XClicked + n, YClicked - n] = 5;
                        n = 999;
                    }
                    else if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked - n) >= 0 && (YClicked - n) <= 7 && board_store_pieces[XClicked + n, YClicked - n] >= 1 && board_store_pieces[XClicked + n, YClicked - n] <= 6)
                    {
                        n = 999;
                    }
                }
            }

            #endregion

            //Rei Branco
            #region ReiBranco
            else if (selectedpiece == 6)
            {
                //Roque
                if (XClicked == 4 && YClicked == 0 && !whitekingmoved)
                {
                    if (board_store_pieces[0,0] == 4 && board_store_pieces[1,0] == 0 && board_store_pieces[2, 0] == 0 && board_store_pieces[3, 0] == 0 && !whitelefttowermoved)
                    {
                        board_store_canmove[0,0] = 3;
                        board_store_backcolors[0,0] = 7;
                    }
                    if (board_store_pieces[7, 0] == 4 && board_store_pieces[6, 0] == 0 && board_store_pieces[5, 0] == 0 && !whiterighttowermoved)
                    {
                        board_store_canmove[7, 0] = 3;
                        board_store_backcolors[7, 0] = 7;
                    }
                }

                //Movimentos normais
                if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked + 1) >= 0 && (YClicked + 1) <= 7 && check_store_blackpossiblemovements[XClicked,YClicked+1] == 0 &&board_store_pieces[XClicked, YClicked + 1] == 0)
                {
                    board_store_canmove[XClicked, YClicked + 1] = 1;
                    board_store_backcolors[XClicked, YClicked + 1] = 4;
                }
                if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked + 1) >= 0 && (YClicked + 1) <= 7 && check_store_blackpossiblemovements[XClicked, YClicked + 1] == 0 && board_store_pieces[XClicked, YClicked + 1] <= 16 && board_store_pieces[XClicked, YClicked + 1] >= 11)
                {
                    board_store_canmove[XClicked, YClicked + 1] = 1;
                    board_store_backcolors[XClicked, YClicked + 1] = 5;
                }

                if ((XClicked + 1) >= 0 && (XClicked + 1) <= 7 && (YClicked + 1) >= 0 && (YClicked + 1) <= 7 && check_store_blackpossiblemovements[XClicked + 1, YClicked + 1] == 0 && board_store_pieces[XClicked + 1, YClicked + 1] == 0)
                {
                    board_store_canmove[XClicked + 1, YClicked + 1] = 1;
                    board_store_backcolors[XClicked + 1, YClicked + 1] = 4;
                }

                if ((XClicked + 1) >= 0 && (XClicked + 1) <= 7 && (YClicked + 1) >= 0 && (YClicked + 1) <= 7 && check_store_blackpossiblemovements[XClicked + 1, YClicked + 1] == 0 && board_store_pieces[XClicked + 1, YClicked + 1] <= 16 && board_store_pieces[XClicked + 1, YClicked + 1] >= 11)
                {
                    board_store_canmove[XClicked + 1, YClicked + 1] = 1;
                    board_store_backcolors[XClicked + 1, YClicked + 1] = 5;
                }

                if ((XClicked - 1) >= 0 && (XClicked - 1) <= 7 && (YClicked + 1) >= 0 && (YClicked + 1) <= 7 && check_store_blackpossiblemovements[XClicked - 1, YClicked + 1] == 0 && board_store_pieces[XClicked - 1, YClicked + 1] == 0)
                {
                    board_store_canmove[XClicked - 1, YClicked + 1] = 1;
                    board_store_backcolors[XClicked - 1, YClicked + 1] = 4;
                }
                if ((XClicked - 1) >= 0 && (XClicked - 1) <= 7 && (YClicked + 1) >= 0 && (YClicked + 1) <= 7 && check_store_blackpossiblemovements[XClicked - 1, YClicked + 1] == 0 && board_store_pieces[XClicked - 1, YClicked + 1] <= 16 && board_store_pieces[XClicked - 1, YClicked + 1] >= 11)
                {
                    board_store_canmove[XClicked - 1, YClicked + 1] = 1;
                    board_store_backcolors[XClicked - 1, YClicked + 1] = 5;
                }

                //

                if ((XClicked + 1) >= 0 && (XClicked + 1) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && check_store_blackpossiblemovements[XClicked + 1, YClicked] == 0 && board_store_pieces[XClicked + 1, YClicked] == 0)
                {
                    board_store_canmove[XClicked + 1, YClicked] = 1;
                    board_store_backcolors[XClicked + 1, YClicked] = 4;
                }

                if ((XClicked + 1) >= 0 && (XClicked + 1) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && check_store_blackpossiblemovements[XClicked + 1, YClicked] == 0 && board_store_pieces[XClicked + 1, YClicked] <= 16 && board_store_pieces[XClicked + 1, YClicked] >= 11)
                {
                    board_store_canmove[XClicked + 1, YClicked] = 1;
                    board_store_backcolors[XClicked + 1, YClicked] = 5;
                }

                if ((XClicked - 1) >= 0 && (XClicked - 1) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && check_store_blackpossiblemovements[XClicked - 1, YClicked] == 0 && board_store_pieces[XClicked - 1, YClicked] == 0)
                {
                    board_store_canmove[XClicked - 1, YClicked] = 1;
                    board_store_backcolors[XClicked - 1, YClicked] = 4;
                }
                if ((XClicked - 1) >= 0 && (XClicked - 1) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && check_store_blackpossiblemovements[XClicked - 1, YClicked] == 0 && board_store_pieces[XClicked - 1, YClicked] <= 16 && board_store_pieces[XClicked - 1, YClicked] >= 11)
                {
                    board_store_canmove[XClicked - 1, YClicked] = 1;
                    board_store_backcolors[XClicked - 1, YClicked] = 5;
                }

                //

                if ((XClicked + 1) >= 0 && (XClicked + 1) <= 7 && (YClicked - 1) >= 0 && (YClicked - 1) <= 7 && check_store_blackpossiblemovements[XClicked + 1, YClicked - 1] == 0 && board_store_pieces[XClicked + 1, YClicked - 1] == 0)
                {
                    board_store_canmove[XClicked + 1, YClicked - 1] = 1;
                    board_store_backcolors[XClicked + 1, YClicked - 1] = 4;
                }
                if ((XClicked + 1) >= 0 && (XClicked + 1) <= 7 && (YClicked - 1) >= 0 && (YClicked - 1) <= 7 && check_store_blackpossiblemovements[XClicked + 1, YClicked - 1] == 0 && board_store_pieces[XClicked + 1, YClicked - 1] <= 16 && board_store_pieces[XClicked + 1, YClicked - 1] >= 11)
                {
                    board_store_canmove[XClicked + 1, YClicked - 1] = 1;
                    board_store_backcolors[XClicked + 1, YClicked - 1] = 5;
                }

                if ((XClicked - 1) >= 0 && (XClicked - 1) <= 7 && (YClicked - 1) >= 0 && (YClicked - 1) <= 7 && check_store_blackpossiblemovements[XClicked - 1, YClicked - 1] == 0 && board_store_pieces[XClicked - 1, YClicked - 1] == 0)
                {
                    board_store_canmove[XClicked - 1, YClicked - 1] = 1;
                    board_store_backcolors[XClicked - 1, YClicked - 1] = 4;
                }
                if ((XClicked - 1) >= 0 && (XClicked - 1) <= 7 && (YClicked - 1) >= 0 && (YClicked - 1) <= 7 && check_store_blackpossiblemovements[XClicked - 1, YClicked - 1] == 0 && board_store_pieces[XClicked - 1, YClicked - 1] <= 16 && board_store_pieces[XClicked - 1, YClicked - 1] >= 11)
                {
                    board_store_canmove[XClicked - 1, YClicked - 1] = 1;
                    board_store_backcolors[XClicked - 1, YClicked - 1] = 5;
                }

                if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked - 1) >= 0 && (YClicked - 1) <= 7 && check_store_blackpossiblemovements[XClicked, YClicked - 1] == 0 && board_store_pieces[XClicked, YClicked - 1] == 0)
                {
                    board_store_canmove[XClicked, YClicked - 1] = 1;
                    board_store_backcolors[XClicked, YClicked - 1] = 4;
                }
                if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked - 1) >= 0 && (YClicked - 1) <= 7 && check_store_blackpossiblemovements[XClicked, YClicked - 1] == 0 && board_store_pieces[XClicked, YClicked - 1] <= 16 && board_store_pieces[XClicked, YClicked - 1] >= 11)
                {
                    board_store_canmove[XClicked, YClicked - 1] = 1;
                    board_store_backcolors[XClicked, YClicked - 1] = 5;
                }
            }
            #endregion

            WhitePossibleMoviment();
        }

        #endregion

        //Guarda o movimento das peças pretas
        #region PieceMovementBlack()
        private void PieceMovementBlack()
        {
            //Peão Preto
            #region PeãoPreto
            if (selectedpiece == 11)
            {
                //Movimento normal
                if (YClicked - 1 >= 0 && board_store_pieces[XClicked, YClicked - 1] == 0)
                {
                    board_store_canmove[XClicked, YClicked - 1] = 1;
                    board_store_backcolors[XClicked, YClicked - 1] = 4;
                }
                if (YClicked == 6 && YClicked - 2 <= 7 && board_store_pieces[XClicked, YClicked - 2] == 0 && board_store_pieces[XClicked, YClicked - 1] == 0)
                {
                    board_store_canmove[XClicked, YClicked - 2] = 1;
                    board_store_backcolors[XClicked, YClicked - 2] = 4;
                }
                //Comidela
                if (XClicked + 1 <= 7 && XClicked + 1 >= 0 && YClicked - 1 >= 0 && board_store_pieces[XClicked + 1, YClicked - 1] <= 6 && board_store_pieces[XClicked + 1, YClicked - 1] >= 1)
                {
                    board_store_canmove[XClicked + 1, YClicked - 1] = 1;
                    board_store_backcolors[XClicked + 1, YClicked - 1] = 5;
                }
                if (XClicked - 1 <= 7 && XClicked - 1 >= 0 && YClicked - 1 >= 0 && board_store_pieces[XClicked - 1, YClicked - 1] <= 6 && board_store_pieces[XClicked - 1, YClicked - 1] >= 1)
                {
                    board_store_canmove[XClicked - 1, YClicked - 1] = 1;
                    board_store_backcolors[XClicked - 1, YClicked - 1] = 5;
                }
                //Captura En Passant
                if (XClicked + 1 <= 7 && XClicked + 1 >= 0 && YClicked ==3 && board_store_pieces[XClicked + 1, YClicked - 1] == 0 && board_store_pieces[XClicked + 1, YClicked] == 1 && YClicked == 3)
                {
                    board_store_canmove[XClicked + 1, YClicked - 1] = 2;
                    board_store_canmove[XClicked + 1, YClicked] = 99;
                    board_store_backcolors[XClicked + 1, YClicked] = 5;
                    board_store_backcolors[XClicked + 1, YClicked - 1] = 6;
                }
                if (XClicked - 1 <= 7 && XClicked - 1 >= 0 && YClicked == 3 && board_store_pieces[XClicked - 1, YClicked - 1] == 0 && board_store_pieces[XClicked - 1, YClicked] == 1 && YClicked == 3)
                {
                    board_store_canmove[XClicked - 1, YClicked - 1] = 2;
                    board_store_canmove[XClicked - 1, YClicked] = 99;
                    board_store_backcolors[XClicked - 1, YClicked] = 5;
                    board_store_backcolors[XClicked - 1, YClicked - 1] = 6;
                }
            }
            #endregion

            //Bispo Preto
            #region BispoPreto
            if (selectedpiece == 12)
            {
                //Baixo/esquerda
                for (int n = -1; n >= -7; n--)
                {
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked + n, YClicked + n] == 0)
                    {
                        board_store_canmove[XClicked + n, YClicked + n] = 1;
                        board_store_backcolors[XClicked + n, YClicked + n] = 4;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked + n, YClicked + n] <= 6 && board_store_pieces[XClicked + n, YClicked + n] >= 1)
                    {
                        board_store_canmove[XClicked + n, YClicked + n] = 1;
                        board_store_backcolors[XClicked + n, YClicked + n] = 5;
                        n = -999;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked + n, YClicked + n] >= 11 && board_store_pieces[XClicked + n, YClicked + n] <= 16)
                    {
                        n = -999;
                    }
                }
                //Cima/Direita
                for (int n = 1; n <= 7; n++)
                {
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked + n, YClicked + n] == 0)
                    {
                        board_store_canmove[XClicked + n, YClicked + n] = 1;
                        board_store_backcolors[XClicked + n, YClicked + n] = 4;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked + n, YClicked + n] <= 6 && board_store_pieces[XClicked + n, YClicked + n] >= 1)
                    {
                        board_store_canmove[XClicked + n, YClicked + n] = 1;
                        board_store_backcolors[XClicked + n, YClicked + n] = 5;
                        n = 999;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked + n, YClicked + n] >= 11 && board_store_pieces[XClicked + n, YClicked + n] <= 16)
                    {
                        n = 999;
                    }
                }
                //baixo/direita
                for (int n = -1; n >= -7; n--)
                {
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked - n) >= 0 && (YClicked - n) <= 7 && board_store_pieces[XClicked + n, YClicked - n] == 0)
                    {
                        board_store_canmove[XClicked + n, YClicked - n] = 1;
                        board_store_backcolors[XClicked + n, YClicked - n] = 4;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked - n) >= 0 && (YClicked - n) <= 7 && board_store_pieces[XClicked + n, YClicked - n] <= 6 && board_store_pieces[XClicked + n, YClicked - n] >= 1)
                    {
                        board_store_canmove[XClicked + n, YClicked - n] = 1;
                        board_store_backcolors[XClicked + n, YClicked - n] = 5;
                        n = -999;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked - n) >= 0 && (YClicked - n) <= 7 && board_store_pieces[XClicked + n, YClicked - n] >= 11 && board_store_pieces[XClicked + n, YClicked - n] <= 16)
                    {
                        n = -999;
                    }
                }
                //cima/esquerda
                for (int n = 1; n <= 7; n++)
                {
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked - n) >= 0 && (YClicked - n) <= 7 && board_store_pieces[XClicked + n, YClicked - n] == 0)
                    {
                        board_store_canmove[XClicked + n, YClicked - n] = 1;
                        board_store_backcolors[XClicked + n, YClicked - n] = 4;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked - n) >= 0 && (YClicked - n) <= 7 && board_store_pieces[XClicked + n, YClicked - n] <= 6 && board_store_pieces[XClicked + n, YClicked - n] >= 1)
                    {
                        board_store_canmove[XClicked + n, YClicked - n] = 1;
                        board_store_backcolors[XClicked + n, YClicked - n] = 5;
                        n = 999;
                    }
                    else if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked - n) >= 0 && (YClicked - n) <= 7 && board_store_pieces[XClicked + n, YClicked - n] >= 11 && board_store_pieces[XClicked + n, YClicked - n] <= 16)
                    {
                        n = 999;
                    }
                }
            }
            #endregion

            //Cavalo Preto
            #region CavaloPreto
            if (selectedpiece == 13)
            {
                if ((XClicked + 1) >= 0 && (XClicked + 1) <= 7 && (YClicked + 2) >= 0 && (YClicked + 2) <= 7 && board_store_pieces[XClicked + 1, YClicked + 2] == 0)
                {
                    board_store_canmove[XClicked + 1, YClicked + 2] = 1;
                    board_store_backcolors[XClicked + 1, YClicked + 2] = 4;
                }
                if ((XClicked + 1) >= 0 && (XClicked + 1) <= 7 && (YClicked + 2) >= 0 && (YClicked + 2) <= 7 && board_store_pieces[XClicked + 1, YClicked + 2] >= 1 && board_store_pieces[XClicked + 1, YClicked + 2] <= 6)
                {
                    board_store_canmove[XClicked + 1, YClicked + 2] = 1;
                    board_store_backcolors[XClicked + 1, YClicked + 2] = 5;
                }

                if ((XClicked + 2) >= 0 && (XClicked + 2) <= 7 && (YClicked + 1) >= 0 && (YClicked + 1) <= 7 && board_store_pieces[XClicked + 2, YClicked + 1] == 0)
                {
                    board_store_canmove[XClicked + 2, YClicked + 1] = 1;
                    board_store_backcolors[XClicked + 2, YClicked + 1] = 4;
                }
                if ((XClicked + 2) >= 0 && (XClicked + 2) <= 7 && (YClicked + 1) >= 0 && (YClicked + 1) <= 7 && board_store_pieces[XClicked + 2, YClicked + 1] >= 1 && board_store_pieces[XClicked + 2, YClicked + 1] <= 6)
                {
                    board_store_canmove[XClicked + 2, YClicked + 1] = 1;
                    board_store_backcolors[XClicked + 2, YClicked + 1] = 5;
                }

                if ((XClicked - 1) >= 0 && (XClicked - 1) <= 7 && (YClicked + 2) >= 0 && (YClicked + 2) <= 7 && board_store_pieces[XClicked - 1, YClicked + 2] == 0)
                {
                    board_store_canmove[XClicked - 1, YClicked + 2] = 1;
                    board_store_backcolors[XClicked - 1, YClicked + 2] = 4;
                }
                if ((XClicked - 1) >= 0 && (XClicked - 1) <= 7 && (YClicked + 2) >= 0 && (YClicked + 2) <= 7 && board_store_pieces[XClicked - 1, YClicked + 2] >= 1 && board_store_pieces[XClicked - 1, YClicked + 2] <= 6)
                {
                    board_store_canmove[XClicked - 1, YClicked + 2] = 1;
                    board_store_backcolors[XClicked - 1, YClicked + 2] = 5;
                }

                if ((XClicked - 2) >= 0 && (XClicked - 2) <= 7 && (YClicked + 1) >= 0 && (YClicked + 1) <= 7 && board_store_pieces[XClicked - 2, YClicked + 1] == 0)
                {
                    board_store_canmove[XClicked - 2, YClicked + 1] = 1;
                    board_store_backcolors[XClicked - 2, YClicked + 1] = 4;
                }
                if ((XClicked - 2) >= 0 && (XClicked - 2) <= 7 && (YClicked + 1) >= 0 && (YClicked + 1) <= 7 && board_store_pieces[XClicked - 2, YClicked + 1] >= 1 && board_store_pieces[XClicked - 2, YClicked + 1] <= 6)
                {
                    board_store_canmove[XClicked - 2, YClicked + 1] = 1;
                    board_store_backcolors[XClicked - 2, YClicked + 1] = 5;
                }
                //
                if ((XClicked + 1) >= 0 && (XClicked + 1) <= 7 && (YClicked - 2) >= 0 && (YClicked - 2) <= 7 && board_store_pieces[XClicked + 1, YClicked - 2] == 0)
                {
                    board_store_canmove[XClicked + 1, YClicked - 2] = 1;
                    board_store_backcolors[XClicked + 1, YClicked - 2] = 4;
                }
                if ((XClicked + 1) >= 0 && (XClicked + 1) <= 7 && (YClicked - 2) >= 0 && (YClicked - 2) <= 7 && board_store_pieces[XClicked + 1, YClicked - 2] >= 1 && board_store_pieces[XClicked + 1, YClicked - 2] <= 6)
                {
                    board_store_canmove[XClicked + 1, YClicked - 2] = 1;
                    board_store_backcolors[XClicked + 1, YClicked - 2] = 5;
                }

                if ((XClicked + 2) >= 0 && (XClicked + 2) <= 7 && (YClicked - 1) >= 0 && (YClicked - 1) <= 7 && board_store_pieces[XClicked + 2, YClicked - 1] == 0)
                {
                    board_store_canmove[XClicked + 2, YClicked - 1] = 1;
                    board_store_backcolors[XClicked + 2, YClicked - 1] = 4;
                }
                if ((XClicked + 2) >= 0 && (XClicked + 2) <= 7 && (YClicked - 1) >= 0 && (YClicked - 1) <= 7 && board_store_pieces[XClicked + 2, YClicked - 1] >= 1 && board_store_pieces[XClicked + 2, YClicked - 1] <= 6)
                {
                    board_store_canmove[XClicked + 2, YClicked - 1] = 1;
                    board_store_backcolors[XClicked + 2, YClicked - 1] = 5;
                }

                if ((XClicked - 1) >= 0 && (XClicked - 1) <= 7 && (YClicked - 2) >= 0 && (YClicked - 2) <= 7 && board_store_pieces[XClicked - 1, YClicked - 2] == 0)
                {
                    board_store_canmove[XClicked - 1, YClicked - 2] = 1;
                    board_store_backcolors[XClicked - 1, YClicked - 2] = 4;
                }
                if ((XClicked - 1) >= 0 && (XClicked - 1) <= 7 && (YClicked - 2) >= 0 && (YClicked - 2) <= 7 && board_store_pieces[XClicked - 1, YClicked - 2] >= 1 && board_store_pieces[XClicked - 1, YClicked - 2] <= 6)
                {
                    board_store_canmove[XClicked - 1, YClicked - 2] = 1;
                    board_store_backcolors[XClicked - 1, YClicked - 2] = 5;
                }

                if ((XClicked - 2) >= 0 && (XClicked - 2) <= 7 && (YClicked - 1) >= 0 && (YClicked - 1) <= 7 && board_store_pieces[XClicked - 2, YClicked - 1] == 0)
                {
                    board_store_canmove[XClicked - 2, YClicked - 1] = 1;
                    board_store_backcolors[XClicked - 2, YClicked - 1] = 4;
                }
                if ((XClicked - 2) >= 0 && (XClicked - 2) <= 7 && (YClicked - 1) >= 0 && (YClicked - 1) <= 7 && board_store_pieces[XClicked - 2, YClicked - 1] >= 1 && board_store_pieces[XClicked - 2, YClicked - 1] <= 6)
                {
                    board_store_canmove[XClicked - 2, YClicked - 1] = 1;
                    board_store_backcolors[XClicked - 2, YClicked - 1] = 5;
                }
            }
            #endregion

            //Torre Preta
            #region TorrePreta
            if (selectedpiece == 14)
            {
                //Cima
                for (int n = 1; n <= 7; n++)
                {
                    if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked, YClicked + n] == 0)
                    {
                        board_store_canmove[XClicked, YClicked + n] = 1;
                        board_store_backcolors[XClicked, YClicked + n] = 4;
                    }
                    if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked, YClicked + n] <= 6 && board_store_pieces[XClicked, YClicked + n] >= 1)
                    {
                        board_store_canmove[XClicked, YClicked + n] = 1;
                        board_store_backcolors[XClicked, YClicked + n] = 5;
                        n = 999;
                    }
                    if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked, YClicked + n] >= 11 && board_store_pieces[XClicked, YClicked + n] <= 16)
                    {
                        n = 999;
                    }
                }

                //Baixo
                for (int n = -1; n >= -7; n--)
                {
                    if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked, YClicked + n] == 0)
                    {
                        board_store_canmove[XClicked, YClicked + n] = 1;
                        board_store_backcolors[XClicked, YClicked + n] = 4;
                    }
                    if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked, YClicked + n] <= 6 && board_store_pieces[XClicked, YClicked + n] >= 1)
                    {
                        board_store_canmove[XClicked, YClicked + n] = 1;
                        board_store_backcolors[XClicked, YClicked + n] = 5;
                        n = -999;
                    }
                    if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked, YClicked + n] >= 11 && board_store_pieces[XClicked, YClicked + n] <= 16)
                    {
                        n = -999;
                    }
                }

                //Direita
                for (int n = 1; n <= 7; n++)
                {
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && board_store_pieces[XClicked + n, YClicked] == 0)
                    {
                        board_store_canmove[XClicked + n, YClicked] = 1;
                        board_store_backcolors[XClicked + n, YClicked] = 4;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && board_store_pieces[XClicked + n, YClicked] <= 6 && board_store_pieces[XClicked + n, YClicked] >= 1)
                    {
                        board_store_canmove[XClicked + n, YClicked] = 1;
                        board_store_backcolors[XClicked + n, YClicked] = 5;
                        n = 999;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && board_store_pieces[XClicked + n, YClicked] >= 11 && board_store_pieces[XClicked + n, YClicked] <= 16)
                    {
                        n = 999;
                    }
                }

                //Esquerda
                for (int n = -1; n >= -7; n--)
                {
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && board_store_pieces[XClicked + n, YClicked] == 0)
                    {
                        board_store_canmove[XClicked + n, YClicked] = 1;
                        board_store_backcolors[XClicked + n, YClicked] = 4;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && board_store_pieces[XClicked + n, YClicked] <= 6 && board_store_pieces[XClicked + n, YClicked] >= 1)
                    {
                        board_store_canmove[XClicked + n, YClicked] = 1;
                        board_store_backcolors[XClicked + n, YClicked] = 5;
                        n = -999;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && board_store_pieces[XClicked + n, YClicked] >= 11 && board_store_pieces[XClicked + n, YClicked] <= 16)
                    {
                        n = -999;
                    }
                }
            }

            #endregion

            //Rainha Preta
            #region RainhaPreta
            if (selectedpiece == 15)
            {
                //Cima
                for (int n = 1; n <= 7; n++)
                {
                    if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked, YClicked + n] == 0)
                    {
                        board_store_canmove[XClicked, YClicked + n] = 1;
                        board_store_backcolors[XClicked, YClicked + n] = 4;
                    }
                    if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked, YClicked + n] <= 6 && board_store_pieces[XClicked, YClicked + n] >= 1)
                    {
                        board_store_canmove[XClicked, YClicked + n] = 1;
                        board_store_backcolors[XClicked, YClicked + n] = 5;
                        n = 999;
                    }
                    if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked, YClicked + n] >= 11 && board_store_pieces[XClicked, YClicked + n] <= 16)
                    {
                        n = 999;
                    }
                }

                //Baixo
                for (int n = -1; n >= -7; n--)
                {
                    if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked, YClicked + n] == 0)
                    {
                        board_store_canmove[XClicked, YClicked + n] = 1;
                        board_store_backcolors[XClicked, YClicked + n] = 4;
                    }
                    if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked, YClicked + n] <= 6 && board_store_pieces[XClicked, YClicked + n] >= 1)
                    {
                        board_store_canmove[XClicked, YClicked + n] = 1;
                        board_store_backcolors[XClicked, YClicked + n] = 5;
                        n = -999;
                    }
                    if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked, YClicked + n] >= 11 && board_store_pieces[XClicked, YClicked + n] <= 16)
                    {
                        n = -999;
                    }
                }

                //Direita
                for (int n = 1; n <= 7; n++)
                {
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && board_store_pieces[XClicked + n, YClicked] == 0)
                    {
                        board_store_canmove[XClicked + n, YClicked] = 1;
                        board_store_backcolors[XClicked + n, YClicked] = 4;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && board_store_pieces[XClicked + n, YClicked] <= 6 && board_store_pieces[XClicked + n, YClicked] >= 1)
                    {
                        board_store_canmove[XClicked + n, YClicked] = 1;
                        board_store_backcolors[XClicked + n, YClicked] = 5;
                        n = 999;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && board_store_pieces[XClicked + n, YClicked] >= 11 && board_store_pieces[XClicked + n, YClicked] <= 16)
                    {
                        n = 999;
                    }
                }

                //Esquerda
                for (int n = -1; n >= -7; n--)
                {
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && board_store_pieces[XClicked + n, YClicked] == 0)
                    {
                        board_store_canmove[XClicked + n, YClicked] = 1;
                        board_store_backcolors[XClicked + n, YClicked] = 4;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && board_store_pieces[XClicked + n, YClicked] <= 6 && board_store_pieces[XClicked + n, YClicked] >= 1)
                    {
                        board_store_canmove[XClicked + n, YClicked] = 1;
                        board_store_backcolors[XClicked + n, YClicked] = 5;
                        n = -999;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && board_store_pieces[XClicked + n, YClicked] >= 11 && board_store_pieces[XClicked + n, YClicked] <= 16)
                    {
                        n = -999;
                    }
                }
                //Baixo/esquerda
                for (int n = -1; n >= -7; n--)
                {
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked + n, YClicked + n] == 0)
                    {
                        board_store_canmove[XClicked + n, YClicked + n] = 1;
                        board_store_backcolors[XClicked + n, YClicked + n] = 4;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked + n, YClicked + n] <= 6 && board_store_pieces[XClicked + n, YClicked + n] >= 1)
                    {
                        board_store_canmove[XClicked + n, YClicked + n] = 1;
                        board_store_backcolors[XClicked + n, YClicked + n] = 5;
                        n = -999;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked + n, YClicked + n] >= 11 && board_store_pieces[XClicked + n, YClicked + n] <= 16)
                    {
                        n = -999;
                    }
                }
                //Cima/Direita
                for (int n = 1; n <= 7; n++)
                {
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked + n, YClicked + n] == 0)
                    {
                        board_store_canmove[XClicked + n, YClicked + n] = 1;
                        board_store_backcolors[XClicked + n, YClicked + n] = 4;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked + n, YClicked + n] <= 6 && board_store_pieces[XClicked + n, YClicked + n] >= 1)
                    {
                        board_store_canmove[XClicked + n, YClicked + n] = 1;
                        board_store_backcolors[XClicked + n, YClicked + n] = 5;
                        n = 999;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked + n) >= 0 && (YClicked + n) <= 7 && board_store_pieces[XClicked + n, YClicked + n] >= 11 && board_store_pieces[XClicked + n, YClicked + n] <= 16)
                    {
                        n = 999;
                    }
                }
                //baixo/direita
                for (int n = -1; n >= -7; n--)
                {
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked - n) >= 0 && (YClicked - n) <= 7 && board_store_pieces[XClicked + n, YClicked - n] == 0)
                    {
                        board_store_canmove[XClicked + n, YClicked - n] = 1;
                        board_store_backcolors[XClicked + n, YClicked - n] = 4;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked - n) >= 0 && (YClicked - n) <= 7 && board_store_pieces[XClicked + n, YClicked - n] <= 6 && board_store_pieces[XClicked + n, YClicked - n] >= 1)
                    {
                        board_store_canmove[XClicked + n, YClicked - n] = 1;
                        board_store_backcolors[XClicked + n, YClicked - n] = 5;
                        n = -999;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked - n) >= 0 && (YClicked - n) <= 7 && board_store_pieces[XClicked + n, YClicked - n] >= 11 && board_store_pieces[XClicked + n, YClicked - n] <= 16)
                    {
                        n = -999;
                    }
                }
                //cima/esquerda
                for (int n = 1; n <= 7; n++)
                {
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked - n) >= 0 && (YClicked - n) <= 7 && board_store_pieces[XClicked + n, YClicked - n] == 0)
                    {
                        board_store_canmove[XClicked + n, YClicked - n] = 1;
                        board_store_backcolors[XClicked + n, YClicked - n] = 4;
                    }
                    if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked - n) >= 0 && (YClicked - n) <= 7 && board_store_pieces[XClicked + n, YClicked - n] <= 6 && board_store_pieces[XClicked + n, YClicked - n] >= 1)
                    {
                        board_store_canmove[XClicked + n, YClicked - n] = 1;
                        board_store_backcolors[XClicked + n, YClicked - n] = 5;
                        n = 999;
                    }
                    else if ((XClicked + n) >= 0 && (XClicked + n) <= 7 && (YClicked - n) >= 0 && (YClicked - n) <= 7 && board_store_pieces[XClicked + n, YClicked - n] >= 11 && board_store_pieces[XClicked + n, YClicked - n] <= 16)
                    {
                        n = 999;
                    }
                }
            }

            #endregion

            //Rei Preto
            #region ReiPreto
            else if (selectedpiece == 16)
            {
                //Roque
                if (XClicked == 4 && YClicked == 7 && !blackkingmoved)
                {
                    if (board_store_pieces[0, 7] == 14 && board_store_pieces[1, 7] == 0 && board_store_pieces[2, 7] == 0 && board_store_pieces[3, 7] == 0 && !blacklefttowermoved)
                    {
                        board_store_canmove[0, 7] = 3;
                        board_store_backcolors[0, 7] = 7;
                    }
                    if (board_store_pieces[7, 7] == 14 && board_store_pieces[6, 7] == 0 && board_store_pieces[5, 7] == 0 && !blackrighttowermoved)
                    {
                        board_store_canmove[7, 7] = 3;
                        board_store_backcolors[7, 7] = 7;
                    }
                }

                //Movimentos normais
                if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked + 1) >= 0 && (YClicked + 1) <= 7 && check_store_whitepossiblemovements[XClicked, YClicked + 1] == 0 && board_store_pieces[XClicked, YClicked + 1] == 0)
                {
                    board_store_canmove[XClicked, YClicked + 1] = 1;
                    board_store_backcolors[XClicked, YClicked + 1] = 4;
                }
                if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked + 1) >= 0 && (YClicked + 1) <= 7 && check_store_whitepossiblemovements[XClicked, YClicked + 1] == 0 && board_store_pieces[XClicked, YClicked + 1] <= 6 && board_store_pieces[XClicked, YClicked + 1] >= 1)
                {
                    board_store_canmove[XClicked, YClicked + 1] = 1;
                    board_store_backcolors[XClicked, YClicked + 1] = 5;
                }

                if ((XClicked + 1) >= 0 && (XClicked + 1) <= 7 && (YClicked + 1) >= 0 && (YClicked + 1) <= 7 && check_store_whitepossiblemovements[XClicked + 1, YClicked + 1] == 0 && board_store_pieces[XClicked + 1, YClicked + 1] == 0)
                {
                    board_store_canmove[XClicked + 1, YClicked + 1] = 1;
                    board_store_backcolors[XClicked + 1, YClicked + 1] = 4;
                }

                if ((XClicked + 1) >= 0 && (XClicked + 1) <= 7 && (YClicked + 1) >= 0 && (YClicked + 1) <= 7 && check_store_whitepossiblemovements[XClicked + 1, YClicked + 1] == 0 && board_store_pieces[XClicked + 1, YClicked + 1] <= 6 && board_store_pieces[XClicked + 1, YClicked + 1] >= 1)
                {
                    board_store_canmove[XClicked + 1, YClicked + 1] = 1;
                    board_store_backcolors[XClicked + 1, YClicked + 1] = 5;
                }

                if ((XClicked - 1) >= 0 && (XClicked - 1) <= 7 && (YClicked + 1) >= 0 && (YClicked + 1) <= 7 && check_store_whitepossiblemovements[XClicked - 1, YClicked + 1] == 0 && board_store_pieces[XClicked - 1, YClicked + 1] == 0)
                {
                    board_store_canmove[XClicked - 1, YClicked + 1] = 1;
                    board_store_backcolors[XClicked - 1, YClicked + 1] = 4;
                }
                if ((XClicked - 1) >= 0 && (XClicked - 1) <= 7 && (YClicked + 1) >= 0 && (YClicked + 1) <= 7 && check_store_whitepossiblemovements[XClicked - 1, YClicked + 1] == 0 && board_store_pieces[XClicked - 1, YClicked + 1] <= 6 && board_store_pieces[XClicked - 1, YClicked + 1] >= 1)
                {
                    board_store_canmove[XClicked - 1, YClicked + 1] = 1;
                    board_store_backcolors[XClicked - 1, YClicked + 1] = 5;
                }

                //

                if ((XClicked + 1) >= 0 && (XClicked + 1) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && check_store_whitepossiblemovements[XClicked + 1, YClicked] == 0 && board_store_pieces[XClicked + 1, YClicked] == 0)
                {
                    board_store_canmove[XClicked + 1, YClicked] = 1;
                    board_store_backcolors[XClicked + 1, YClicked] = 4;
                }

                if ((XClicked + 1) >= 0 && (XClicked + 1) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && check_store_whitepossiblemovements[XClicked + 1, YClicked] == 0 && board_store_pieces[XClicked + 1, YClicked] <= 6 && board_store_pieces[XClicked + 1, YClicked] >= 1)
                {
                    board_store_canmove[XClicked + 1, YClicked] = 1;
                    board_store_backcolors[XClicked + 1, YClicked] = 5;
                }

                if ((XClicked - 1) >= 0 && (XClicked - 1) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && check_store_whitepossiblemovements[XClicked - 1, YClicked] == 0 && board_store_pieces[XClicked - 1, YClicked] == 0)
                {
                    board_store_canmove[XClicked - 1, YClicked] = 1;
                    board_store_backcolors[XClicked - 1, YClicked] = 4;
                }
                if ((XClicked - 1) >= 0 && (XClicked - 1) <= 7 && (YClicked) >= 0 && (YClicked) <= 7 && check_store_whitepossiblemovements[XClicked - 1, YClicked] == 0 && board_store_pieces[XClicked - 1, YClicked] <= 6 && board_store_pieces[XClicked - 1, YClicked] >= 1)
                {
                    board_store_canmove[XClicked - 1, YClicked] = 1;
                    board_store_backcolors[XClicked - 1, YClicked] = 5;
                }

                //

                if ((XClicked + 1) >= 0 && (XClicked + 1) <= 7 && (YClicked - 1) >= 0 && (YClicked - 1) <= 7 && check_store_whitepossiblemovements[XClicked + 1, YClicked - 1] == 0 && board_store_pieces[XClicked + 1, YClicked - 1] == 0)
                {
                    board_store_canmove[XClicked + 1, YClicked - 1] = 1;
                    board_store_backcolors[XClicked + 1, YClicked - 1] = 4;
                }
                if ((XClicked + 1) >= 0 && (XClicked + 1) <= 7 && (YClicked - 1) >= 0 && (YClicked - 1) <= 7 && check_store_whitepossiblemovements[XClicked + 1, YClicked - 1] == 0 && board_store_pieces[XClicked + 1, YClicked - 1] <= 6 && board_store_pieces[XClicked + 1, YClicked - 1] >= 1)
                {
                    board_store_canmove[XClicked + 1, YClicked - 1] = 1;
                    board_store_backcolors[XClicked + 1, YClicked - 1] = 5;
                }

                if ((XClicked - 1) >= 0 && (XClicked - 1) <= 7 && (YClicked - 1) >= 0 && (YClicked - 1) <= 7 && check_store_whitepossiblemovements[XClicked - 1, YClicked - 1] == 0 && board_store_pieces[XClicked - 1, YClicked - 1] == 0)
                {
                    board_store_canmove[XClicked - 1, YClicked - 1] = 1;
                    board_store_backcolors[XClicked - 1, YClicked - 1] = 4;
                }
                if ((XClicked - 1) >= 0 && (XClicked - 1) <= 7 && (YClicked - 1) >= 0 && (YClicked - 1) <= 7 && check_store_whitepossiblemovements[XClicked - 1, YClicked - 1] == 0 && board_store_pieces[XClicked - 1, YClicked - 1] <= 6 && board_store_pieces[XClicked - 1, YClicked - 1] >= 1)
                {
                    board_store_canmove[XClicked - 1, YClicked - 1] = 1;
                    board_store_backcolors[XClicked - 1, YClicked - 1] = 5;
                }

                if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked - 1) >= 0 && (YClicked - 1) <= 7 && check_store_whitepossiblemovements[XClicked, YClicked - 1] == 0 && board_store_pieces[XClicked, YClicked - 1] == 0)
                {
                    board_store_canmove[XClicked, YClicked - 1] = 1;
                    board_store_backcolors[XClicked, YClicked - 1] = 4;
                }
                if ((XClicked) >= 0 && (XClicked) <= 7 && (YClicked - 1) >= 0 && (YClicked - 1) <= 7 && check_store_whitepossiblemovements[XClicked, YClicked - 1] == 0 && board_store_pieces[XClicked, YClicked - 1] <= 6 && board_store_pieces[XClicked, YClicked - 1] >= 1)
                {
                    board_store_canmove[XClicked, YClicked - 1] = 1;
                    board_store_backcolors[XClicked, YClicked - 1] = 5;
                }
            }
            #endregion

            BlackPossibleMoviment();
        }

        #endregion

        //Possiveis movimentos das peças brancas, vê se o rei preto está em check
        #region WhitePossibleMoviment()
        private void WhitePossibleMoviment()
        {
            for (int x = 0; x < 8; x++)
                for (int y = 0; y < 8; y++)
                    check_store_whitepossiblemovements[x, y] = 0;

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    //Peao branco
                    #region Peão Branco
                    if (board_store_pieces[x, y] == 1)
                    {
                        if (x + 1 <= 7 && x + 1 >= 0 && y + 1 <= 7 && board_store_pieces[x + 1, y + 1] <= 16 && board_store_pieces[x + 1, y + 1] >= 11)
                        {
                            check_store_whitepossiblemovements[x + 1, y + 1] = 1;
                        }
                        if (x + 1 <= 7 && x + 1 >= 0 && y + 1 <= 7 && board_store_pieces[x + 1, y + 1] == 0)
                        {
                            check_store_whitepossiblemovements[x + 1, y + 1] = 1;
                        }
                        //
                        if (x - 1 <= 7 && x - 1 >= 0 && y + 1 <= 7 && board_store_pieces[x - 1, y + 1] <= 16 && board_store_pieces[x - 1, y + 1] >= 11)
                        {
                            check_store_whitepossiblemovements[x - 1, y + 1] = 1;
                        }
                        if (x - 1 <= 7 && x - 1 >= 0 && y + 1 <= 7 && board_store_pieces[x - 1, y + 1] == 0)
                        {
                            check_store_whitepossiblemovements[x - 1, y + 1] = 1;
                        }
                    }
                    #endregion

                    //Bispo Branco
                    #region BispoBranco
                    else if (board_store_pieces[x, y] == 2)
                    {
                        //Baixo/esquerda
                        for (int n = -1; n >= -7; n--)
                        {
                            if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] == 0)
                            {
                                check_store_whitepossiblemovements[x + n, y + n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] == 16)
                            {
                                check_store_whitepossiblemovements[x + n, y + n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] <= 15 && board_store_pieces[x + n, y + n] >= 11)
                            {
                                check_store_whitepossiblemovements[x + n, y + n] = 1;
                                n = -999;
                            }
                            else if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] >= 1 && board_store_pieces[x + n, y + n] <= 6)
                            {
                                n = -999;
                            }
                        }
                        //Cima/Direita
                        for (int n = 1; n <= 7; n++)
                        {
                            if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] == 0)
                            {
                                check_store_whitepossiblemovements[x + n, y + n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] == 16)
                            {
                                check_store_whitepossiblemovements[x + n, y + n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] <= 15 && board_store_pieces[x + n, y + n] >= 11)
                            {
                                check_store_whitepossiblemovements[x + n, y + n] = 1;
                                n = 999;
                            }
                            else if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] >= 1 && board_store_pieces[x + n, y + n] <= 6)
                            {
                                n = 999;
                            }
                        }
                        //baixo/direita
                        for (int n = -1; n >= -7; n--)
                        {
                            if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] == 0)
                            {
                                check_store_whitepossiblemovements[x + n, y - n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] == 16)
                            {
                                check_store_whitepossiblemovements[x + n, y - n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] <= 15 && board_store_pieces[x + n, y - n] >= 11)
                            {
                                check_store_whitepossiblemovements[x + n, y - n] = 1;
                                n = -999;
                            }
                            else if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] >= 1 && board_store_pieces[x + n, y - n] <= 6)
                            {
                                n = -999;
                            }
                        }
                        //cima/esquerda
                        for (int n = 1; n <= 7; n++)
                        {
                            if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] == 0)
                            {
                                check_store_whitepossiblemovements[x + n, y - n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] == 16)
                            {
                                check_store_whitepossiblemovements[x + n, y - n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] <= 15 && board_store_pieces[x + n, y - n] >= 11)
                            {
                                check_store_whitepossiblemovements[x + n, y - n] = 1;
                                n = 999;
                            }
                            else if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] >= 1 && board_store_pieces[x + n, y - n] <= 6)
                            {
                                n = 999;
                            }
                        }
                    }
                    #endregion

                    //Cavalo Branco
                    #region CavaloBranco
                    else if (board_store_pieces[x, y] == 3)
                    {

                        if ((x + 1) >= 0 && (x + 1) <= 7 && (y + 2) >= 0 && (y + 2) <= 7 && board_store_pieces[x + 1, y + 2] == 0)
                        {
                            check_store_whitepossiblemovements[x + 1, y + 2] = 1;
                        }
                        if ((x + 1) >= 0 && (x + 1) <= 7 && (y + 2) >= 0 && (y + 2) <= 7 && board_store_pieces[x + 1, y + 2] <= 16 && board_store_pieces[x + 1, y + 2] >= 11)
                        {
                            check_store_whitepossiblemovements[x + 1, y + 2] = 1;
                        }

                        if ((x + 2) >= 0 && (x + 2) <= 7 && (y + 1) >= 0 && (y + 1) <= 7 && board_store_pieces[x + 2, y + 1] == 0)
                        {
                            check_store_whitepossiblemovements[x + 2, y + 1] = 1;
                        }
                        if ((x + 2) >= 0 && (x + 2) <= 7 && (y + 1) >= 0 && (y + 1) <= 7 && board_store_pieces[x + 2, y + 1] <= 16 && board_store_pieces[x + 2, y + 1] >= 11)
                        {
                            check_store_whitepossiblemovements[x + 2, y + 1] = 1;
                        }

                        if ((x - 1) >= 0 && (x - 1) <= 7 && (y + 2) >= 0 && (y + 2) <= 7 && board_store_pieces[x - 1, y + 2] == 0)
                        {
                            check_store_whitepossiblemovements[x - 1, y + 2] = 1;
                        }
                        if ((x - 1) >= 0 && (x - 1) <= 7 && (y + 2) >= 0 && (y + 2) <= 7 && board_store_pieces[x - 1, y + 2] <= 16 && board_store_pieces[x - 1, y + 2] >= 11)
                        {
                            check_store_whitepossiblemovements[x - 1, y + 2] = 1;
                        }

                        if ((x - 2) >= 0 && (x - 2) <= 7 && (y + 1) >= 0 && (y + 1) <= 7 && board_store_pieces[x - 2, y + 1] == 0)
                        {
                            check_store_whitepossiblemovements[x - 2, y + 1] = 1;
                        }
                        if ((x - 2) >= 0 && (x - 2) <= 7 && (y + 1) >= 0 && (y + 1) <= 7 && board_store_pieces[x - 2, y + 1] <= 16 && board_store_pieces[x - 2, y + 1] >= 11)
                        {
                            check_store_whitepossiblemovements[x - 2, y + 1] = 1;
                        }

                        //

                        if ((x + 1) >= 0 && (x + 1) <= 7 && (y - 2) >= 0 && (y - 2) <= 7 && board_store_pieces[x + 1, y - 2] == 0)
                        {
                            check_store_whitepossiblemovements[x + 1, y - 2] = 1;
                        }
                        if ((x + 1) >= 0 && (x + 1) <= 7 && (y - 2) >= 0 && (y - 2) <= 7 && board_store_pieces[x + 1, y - 2] <= 16 && board_store_pieces[x + 1, y - 2] >= 11)
                        {
                            check_store_whitepossiblemovements[x + 1, y - 2] = 1;
                        }

                        if ((x + 2) >= 0 && (x + 2) <= 7 && (y - 1) >= 0 && (y - 1) <= 7 && board_store_pieces[x + 2, y - 1] == 0)
                        {
                            check_store_whitepossiblemovements[x + 2, y - 1] = 1;
                        }
                        if ((x + 2) >= 0 && (x + 2) <= 7 && (y - 1) >= 0 && (y - 1) <= 7 && board_store_pieces[x + 2, y - 1] <= 16 && board_store_pieces[x + 2, y - 1] >= 11)
                        {
                            check_store_whitepossiblemovements[x + 2, y - 1] = 1;
                        }

                        if ((x - 1) >= 0 && (x - 1) <= 7 && (y - 2) >= 0 && (y - 2) <= 7 && board_store_pieces[x - 1, y - 2] == 0)
                        {
                            check_store_whitepossiblemovements[x - 1, y - 2] = 1;
                        }
                        if ((x - 1) >= 0 && (x - 1) <= 7 && (y - 2) >= 0 && (y - 2) <= 7 && board_store_pieces[x - 1, y - 2] <= 16 && board_store_pieces[x - 1, y - 2] >= 11)
                        {
                            check_store_whitepossiblemovements[x - 1, y - 2] = 1;
                        }

                        if ((x - 2) >= 0 && (x - 2) <= 7 && (y - 1) >= 0 && (y - 1) <= 7 && board_store_pieces[x - 2, y - 1] == 0)
                        {
                            check_store_whitepossiblemovements[x - 2, y - 1] = 1;
                        }
                        if ((x - 2) >= 0 && (x - 2) <= 7 && (y - 1) >= 0 && (y - 1) <= 7 && board_store_pieces[x - 2, y - 1] <= 16 && board_store_pieces[x - 2, y - 1] >= 11)
                        {
                            check_store_whitepossiblemovements[x - 2, y - 1] = 1;
                        }
                    }

                    #endregion

                    //Torre Branca
                    #region TorreBranca
                    else if (board_store_pieces[x, y] == 4)
                    {
                        //Cima
                        for (int n = 1; n <= 7; n++)
                        {
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] == 0)
                            {
                                check_store_whitepossiblemovements[x, y + n] = 1;
                            }
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] == 16)
                            {
                                check_store_whitepossiblemovements[x, y + n] = 1;
                            }
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] <= 15 && board_store_pieces[x, y + n] >= 11)
                            {
                                check_store_whitepossiblemovements[x, y + n] = 1;
                                n = 999;
                            }
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] >= 1 && board_store_pieces[x, y + n] <= 6)
                            {
                                n = 999;
                            }
                        }
                        //Baixo
                        for (int n = -1; n >= -7; n--)
                        {
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] == 0)
                            {
                                check_store_whitepossiblemovements[x, y + n] = 1;
                            }
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] == 16)
                            {
                                check_store_whitepossiblemovements[x, y + n] = 1;
                            }
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] <= 15 && board_store_pieces[x, y + n] >= 11)
                            {
                                check_store_whitepossiblemovements[x, y + n] = 1;
                                n = -999;
                            }
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] >= 1 && board_store_pieces[x, y + n] <= 6)
                            {
                                n = -999;
                            }
                        }
                        //Direita
                        for (int n = 1; n <= 7; n++)
                        {
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] == 0)
                            {
                                check_store_whitepossiblemovements[x + n, y] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] == 16)
                            {
                                check_store_whitepossiblemovements[x + n, y] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] <= 15 && board_store_pieces[x + n, y] >= 11)
                            {
                                check_store_whitepossiblemovements[x + n, y] = 1;
                                n = 999;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] >= 1 && board_store_pieces[x + n, y] <= 6)
                            {
                                n = 999;
                            }
                        }
                        //Esquerda
                        for (int n = -1; n >= -7; n--)
                        {
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] == 0)
                            {
                                check_store_whitepossiblemovements[x + n, y] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] == 16)
                            {
                                check_store_whitepossiblemovements[x + n, y] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] <= 15 && board_store_pieces[x + n, y] >= 11)
                            {
                                check_store_whitepossiblemovements[x + n, y] = 1;
                                n = -999;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] >= 1 && board_store_pieces[x + n, y] <= 6)
                            {
                                n = -999;
                            }
                        }
                    }

                    #endregion

                    //Rainha Branca
                    #region RainhaBranca
                    else if (board_store_pieces[x, y] == 5)
                    {
                        //Baixo/esquerda
                        for (int n = -1; n >= -7; n--)
                        {
                            if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] == 0)
                            {
                                check_store_whitepossiblemovements[x + n, y + n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] == 16)
                            {
                                check_store_whitepossiblemovements[x + n, y + n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] <= 15 && board_store_pieces[x + n, y + n] >= 11)
                            {
                                check_store_whitepossiblemovements[x + n, y + n] = 1;
                                n = -999;
                            }
                            else if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] >= 1 && board_store_pieces[x + n, y + n] <= 6)
                            {
                                n = -999;
                            }
                        }
                        //Cima/Direita
                        for (int n = 1; n <= 7; n++)
                        {
                            if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] == 0)
                            {
                                check_store_whitepossiblemovements[x + n, y + n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] == 16)
                            {
                                check_store_whitepossiblemovements[x + n, y + n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] <= 15 && board_store_pieces[x + n, y + n] >= 11)
                            {
                                check_store_whitepossiblemovements[x + n, y + n] = 1;
                                n = 999;
                            }
                            else if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] >= 1 && board_store_pieces[x + n, y + n] <= 6)
                            {
                                n = 999;
                            }
                        }
                        //baixo/direita
                        for (int n = -1; n >= -7; n--)
                        {
                            if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] == 0)
                            {
                                check_store_whitepossiblemovements[x + n, y - n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] == 16)
                            {
                                check_store_whitepossiblemovements[x + n, y - n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] <= 15 && board_store_pieces[x + n, y - n] >= 11)
                            {
                                check_store_whitepossiblemovements[x + n, y - n] = 1;
                                n = -999;
                            }
                            else if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] >= 1 && board_store_pieces[x + n, y - n] <= 6)
                            {
                                n = -999;
                            }
                        }
                        //cima/esquerda
                        for (int n = 1; n <= 7; n++)
                        {
                            if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] == 0)
                            {
                                check_store_whitepossiblemovements[x + n, y - n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] == 16)
                            {
                                check_store_whitepossiblemovements[x + n, y - n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] <= 15 && board_store_pieces[x + n, y - n] >= 11)
                            {
                                check_store_whitepossiblemovements[x + n, y - n] = 1;
                                n = 999;
                            }
                            else if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] >= 1 && board_store_pieces[x + n, y - n] <= 6)
                            {
                                n = 999;
                            }
                        }
                        //Cima
                        for (int n = 1; n <= 7; n++)
                        {
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] == 0)
                            {
                                check_store_whitepossiblemovements[x, y + n] = 1;
                            }
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] == 16)
                            {
                                check_store_whitepossiblemovements[x, y + n] = 1;
                            }
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] <= 15 && board_store_pieces[x, y + n] >= 11)
                            {
                                check_store_whitepossiblemovements[x, y + n] = 1;
                                n = 999;
                            }
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] >= 1 && board_store_pieces[x, y + n] <= 6)
                            {
                                n = 999;
                            }
                        }
                        //Baixo
                        for (int n = -1; n >= -7; n--)
                        {
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] == 0)
                            {
                                check_store_whitepossiblemovements[x, y + n] = 1;
                            }
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] == 16)
                            {
                                check_store_whitepossiblemovements[x, y + n] = 1;
                            }
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] <= 15 && board_store_pieces[x, y + n] >= 11)
                            {
                                check_store_whitepossiblemovements[x, y + n] = 1;
                                n = -999;
                            }
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] >= 1 && board_store_pieces[x, y + n] <= 6)
                            {
                                n = -999;
                            }
                        }
                        //Direita
                        for (int n = 1; n <= 7; n++)
                        {
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] == 0)
                            {
                                check_store_whitepossiblemovements[x + n, y] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] == 16)
                            {
                                check_store_whitepossiblemovements[x + n, y] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] <= 15 && board_store_pieces[x + n, y] >= 11)
                            {
                                check_store_whitepossiblemovements[x + n, y] = 1;
                                n = 999;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] >= 1 && board_store_pieces[x + n, y] <= 6)
                            {
                                n = 999;
                            }
                        }
                        //Esquerda
                        for (int n = -1; n >= -7; n--)
                        {
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] == 0)
                            {
                                check_store_whitepossiblemovements[x + n, y] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] == 16)
                            {
                                check_store_whitepossiblemovements[x + n, y] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] <= 15 && board_store_pieces[x + n, y] >= 11)
                            {
                                check_store_whitepossiblemovements[x + n, y] = 1;
                                n = -999;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] >= 1 && board_store_pieces[x + n, y] <= 6)
                            {
                                n = -999;
                            }
                        }
                    }
                    #endregion

                    //Rei Branco
                    #region ReiBranco
                    else if (board_store_pieces[x, y] == 6)
                    {
                        bool canmove = false;
                        if ((x) >= 0 && (x) <= 7 && (y + 1) >= 0 && (y + 1) <= 7 && board_store_pieces[x, y + 1] == 0)
                        {
                            check_store_whitepossiblemovements[x, y + 1] = 1;
                            canmove = true;
                        }
                        if ((x) >= 0 && (x) <= 7 && (y + 1) >= 0 && (y + 1) <= 7 && board_store_pieces[x, y + 1] <= 16 && board_store_pieces[x, y + 1] >= 11)
                        {
                            check_store_whitepossiblemovements[x, y + 1] = 1;
                            canmove = true;
                        }

                        if ((x + 1) >= 0 && (x + 1) <= 7 && (y + 1) >= 0 && (y + 1) <= 7 && board_store_pieces[x + 1, y + 1] == 0)
                        {
                            check_store_whitepossiblemovements[x + 1, y + 1] = 1;
                            canmove = true;
                        }

                        if ((x + 1) >= 0 && (x + 1) <= 7 && (y + 1) >= 0 && (y + 1) <= 7 && board_store_pieces[x + 1, y + 1] <= 16 && board_store_pieces[x + 1, y + 1] >= 11)
                        {
                            check_store_whitepossiblemovements[x + 1, y + 1] = 1;
                            canmove = true;
                        }

                        if ((x - 1) >= 0 && (x - 1) <= 7 && (y + 1) >= 0 && (y + 1) <= 7 && board_store_pieces[x - 1, y + 1] == 0)
                        {
                            check_store_whitepossiblemovements[x - 1, y + 1] = 1;
                            canmove = true;
                        }
                        if ((x - 1) >= 0 && (x - 1) <= 7 && (y + 1) >= 0 && (y + 1) <= 7 && board_store_pieces[x - 1, y + 1] <= 16 && board_store_pieces[x - 1, y + 1] >= 11)
                        {
                            check_store_whitepossiblemovements[x - 1, y + 1] = 1;
                            canmove = true;
                        }

                        //

                        if ((x + 1) >= 0 && (x + 1) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + 1, y] == 0)
                        {
                            check_store_whitepossiblemovements[x + 1, y] = 1;
                            canmove = true;
                        }

                        if ((x + 1) >= 0 && (x + 1) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + 1, y] <= 16 && board_store_pieces[x + 1, y] >= 11)
                        {
                            check_store_whitepossiblemovements[x + 1, y] = 1;
                            canmove = true;
                        }

                        if ((x - 1) >= 0 && (x - 1) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x - 1, y] == 0)
                        {
                            check_store_whitepossiblemovements[x - 1, y] = 1;
                            canmove = true;
                        }
                        if ((x - 1) >= 0 && (x - 1) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x - 1, y] <= 16 && board_store_pieces[x - 1, y] >= 11)
                        {
                            check_store_whitepossiblemovements[x - 1, y] = 1;
                            canmove = true;
                        }

                        //

                        if ((x + 1) >= 0 && (x + 1) <= 7 && (y - 1) >= 0 && (y - 1) <= 7 && board_store_pieces[x + 1, y - 1] == 0)
                        {
                            check_store_whitepossiblemovements[x + 1, y - 1] = 1;
                            canmove = true;
                        }
                        if ((x + 1) >= 0 && (x + 1) <= 7 && (y - 1) >= 0 && (y - 1) <= 7 && board_store_pieces[x + 1, y - 1] <= 16 && board_store_pieces[x + 1, y - 1] >= 11)
                        {
                            check_store_whitepossiblemovements[x + 1, y - 1] = 1;
                            canmove = true;
                        }

                        if ((x - 1) >= 0 && (x - 1) <= 7 && (y - 1) >= 0 && (y - 1) <= 7 && board_store_pieces[x - 1, y - 1] == 0)
                        {
                            check_store_whitepossiblemovements[x - 1, y - 1] = 1;
                            canmove = true;
                        }
                        if ((x - 1) >= 0 && (x - 1) <= 7 && (y - 1) >= 0 && (y - 1) <= 7 && board_store_pieces[x - 1, y - 1] <= 16 && board_store_pieces[x - 1, y - 1] >= 11)
                        {
                            check_store_whitepossiblemovements[x - 1, y - 1] = 1;
                            canmove = true;
                        }

                        if ((x) >= 0 && (x) <= 7 && (y - 1) >= 0 && (y - 1) <= 7 && board_store_pieces[x, y - 1] == 0)
                        {
                            check_store_whitepossiblemovements[x, y - 1] = 1;
                            canmove = true;
                        }
                        if ((x) >= 0 && (x) <= 7 && (y - 1) >= 0 && (y - 1) <= 7 && board_store_pieces[x, y - 1] <= 16 && board_store_pieces[x, y - 1] >= 11)
                        {
                            check_store_whitepossiblemovements[x, y - 1] = 1;
                            canmove = true;
                        }
                    }
                    #endregion

                }
            }

            for (int x = 0; x < 8; x++) //Vê se o rei oposto está em check e se estiver escreve no historico de movimentos
            {
                for (int y = 0; y < 8; y++)
                {
                    if (board_store_pieces[x, y] == 16 && check_store_whitepossiblemovements[x, y] == 1)
                    {
                        board_store_backcolors[x, y] = 1;
                        if (listbox_side.Items[2].ToString() != " Rei preto em check!")
                            listbox_side.Items.Insert(2, " Rei preto em check!");

                        Bkingoncheck = true;
                    }
                    else if (board_store_pieces[x, y] == 16 && check_store_whitepossiblemovements[x, y] == 0)
                    {
                        board_store_backcolors[x, y] = 0;
                        Bkingoncheck = false;
                    }                    
                }
            }
        }
        #endregion

        //Possiveis movimentos das peças pretas, vê se o rei branco está em check
        #region BlackPossibleMoviment()
        private void BlackPossibleMoviment()
        {
            for (int x = 0; x < 8; x++)
                for (int y = 0; y < 8; y++)
                    check_store_blackpossiblemovements[x, y] = 0;

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    //Peao preto
                    #region PeãoPreto
                    if (board_store_pieces[x, y] == 11)
                    {
                        if (x + 1 <= 7 && x + 1 >= 0 && y - 1 >= 0 && board_store_pieces[x + 1, y - 1] <= 6 && board_store_pieces[x + 1, y - 1] >= 1)
                        {
                            check_store_blackpossiblemovements[x + 1, y - 1] = 1;
                        }
                        if (x + 1 <= 7 && x + 1 >= 0 && y - 1 >= 0 && board_store_pieces[x + 1, y - 1] == 0)
                        {
                            check_store_blackpossiblemovements[x + 1, y - 1] = 1;
                        }
                        //
                        if (x - 1 <= 7 && x - 1 >= 0 && y - 1 >= 0 && board_store_pieces[x - 1, y - 1] <= 6 && board_store_pieces[x - 1, y - 1] >= 1)
                        {
                            check_store_blackpossiblemovements[x - 1, y - 1] = 1;
                        }
                        if (x - 1 <= 7 && x - 1 >= 0 && y - 1 >= 0 && board_store_pieces[x - 1, y - 1] == 0)
                        {
                            check_store_blackpossiblemovements[x - 1, y - 1] = 1;
                        }
                    }
                    #endregion
                    
                    //Bispo Preto
                    #region BispoPreto
                    else if (board_store_pieces[x, y] == 12)
                    {
                        //Baixo/esquerda
                        for (int n = -1; n >= -7; n--)
                        {
                            if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] == 0)
                            {
                                check_store_blackpossiblemovements[x + n, y + n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] == 6)
                            {
                                check_store_blackpossiblemovements[x + n, y + n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] <= 5 && board_store_pieces[x + n, y + n] >= 1)
                            {
                                check_store_blackpossiblemovements[x + n, y + n] = 1;
                                n = -999;
                            }
                            else if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] >= 11 && board_store_pieces[x + n, y + n] <= 16)
                            {
                                n = -999;
                            }
                        }
                        //Cima/Direita
                        for (int n = 1; n <= 7; n++)
                        {
                            if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] == 0)
                            {
                                check_store_blackpossiblemovements[x + n, y + n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] == 6)
                            {
                                check_store_blackpossiblemovements[x + n, y + n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] <= 5 && board_store_pieces[x + n, y + n] >= 1)
                            {
                                check_store_blackpossiblemovements[x + n, y + n] = 1;
                                n = 999;
                            }
                            else if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] >= 11 && board_store_pieces[x + n, y + n] <= 16)
                            {
                                n = 999;
                            }
                        }
                        //baixo/direita
                        for (int n = -1; n >= -7; n--)
                        {
                            if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] == 0)
                            {
                                check_store_blackpossiblemovements[x + n, y - n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] == 6)
                            {
                                check_store_blackpossiblemovements[x + n, y - n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] <= 5 && board_store_pieces[x + n, y - n] >= 1)
                            {
                                check_store_blackpossiblemovements[x + n, y - n] = 1;
                                n = -999;
                            }
                            else if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] >= 11 && board_store_pieces[x + n, y - n] <= 16)
                            {
                                n = -999;
                            }
                        }
                        //cima/esquerda
                        for (int n = 1; n <= 7; n++)
                        {
                            if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] == 0)
                            {
                                check_store_blackpossiblemovements[x + n, y - n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] == 6)
                            {
                                check_store_blackpossiblemovements[x + n, y - n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] <= 5 && board_store_pieces[x + n, y - n] >= 1)
                            {
                                check_store_blackpossiblemovements[x + n, y - n] = 1;
                                n = 999;
                            }
                            else if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] >= 11 && board_store_pieces[x + n, y - n] <= 16)
                            {
                                n = 999;
                            }
                        }
                    }
                    #endregion
                    
                    //Cavalo Preto
                    #region CavaloPreto
                    else if (board_store_pieces[x, y] == 13)
                    {

                        if ((x + 1) >= 0 && (x + 1) <= 7 && (y + 2) >= 0 && (y + 2) <= 7 && board_store_pieces[x + 1, y + 2] == 0)
                        {
                            check_store_blackpossiblemovements[x + 1, y + 2] = 1;
                        }
                        if ((x + 1) >= 0 && (x + 1) <= 7 && (y + 2) >= 0 && (y + 2) <= 7 && board_store_pieces[x + 1, y + 2] <= 6 && board_store_pieces[x + 1, y + 2] >= 1)
                        {
                            check_store_blackpossiblemovements[x + 1, y + 2] = 1;
                        }

                        if ((x + 2) >= 0 && (x + 2) <= 7 && (y + 1) >= 0 && (y + 1) <= 7 && board_store_pieces[x + 2, y + 1] == 0)
                        {
                            check_store_blackpossiblemovements[x + 2, y + 1] = 1;
                        }
                        if ((x + 2) >= 0 && (x + 2) <= 7 && (y + 1) >= 0 && (y + 1) <= 7 && board_store_pieces[x + 2, y + 1] <= 6 && board_store_pieces[x + 2, y + 1] >= 1)
                        {
                            check_store_blackpossiblemovements[x + 2, y + 1] = 1;
                        }

                        if ((x - 1) >= 0 && (x - 1) <= 7 && (y + 2) >= 0 && (y + 2) <= 7 && board_store_pieces[x - 1, y + 2] == 0)
                        {
                            check_store_blackpossiblemovements[x - 1, y + 2] = 1;
                        }
                        if ((x - 1) >= 0 && (x - 1) <= 7 && (y + 2) >= 0 && (y + 2) <= 7 && board_store_pieces[x - 1, y + 2] <= 6 && board_store_pieces[x - 1, y + 2] >= 1)
                        {
                            check_store_blackpossiblemovements[x - 1, y + 2] = 1;
                        }

                        if ((x - 2) >= 0 && (x - 2) <= 7 && (y + 1) >= 0 && (y + 1) <= 7 && board_store_pieces[x - 2, y + 1] == 0)
                        {
                            check_store_blackpossiblemovements[x - 2, y + 1] = 1;
                        }
                        if ((x - 2) >= 0 && (x - 2) <= 7 && (y + 1) >= 0 && (y + 1) <= 7 && board_store_pieces[x - 2, y + 1] <= 6 && board_store_pieces[x - 2, y + 1] >= 1)
                        {
                            check_store_blackpossiblemovements[x - 2, y + 1] = 1;
                        }

                        //

                        if ((x + 1) >= 0 && (x + 1) <= 7 && (y - 2) >= 0 && (y - 2) <= 7 && board_store_pieces[x + 1, y - 2] == 0)
                        {
                            check_store_blackpossiblemovements[x + 1, y - 2] = 1;
                        }
                        if ((x + 1) >= 0 && (x + 1) <= 7 && (y - 2) >= 0 && (y - 2) <= 7 && board_store_pieces[x + 1, y - 2] <= 6 && board_store_pieces[x + 1, y - 2] >= 1)
                        {
                            check_store_blackpossiblemovements[x + 1, y - 2] = 1;
                        }

                        if ((x + 2) >= 0 && (x + 2) <= 7 && (y - 1) >= 0 && (y - 1) <= 7 && board_store_pieces[x + 2, y - 1] == 0)
                        {
                            check_store_blackpossiblemovements[x + 2, y - 1] = 1;
                        }
                        if ((x + 2) >= 0 && (x + 2) <= 7 && (y - 1) >= 0 && (y - 1) <= 7 && board_store_pieces[x + 2, y - 1] <= 6 && board_store_pieces[x + 2, y - 1] >= 1)
                        {
                            check_store_blackpossiblemovements[x + 2, y - 1] = 1;
                        }

                        if ((x - 1) >= 0 && (x - 1) <= 7 && (y - 2) >= 0 && (y - 2) <= 7 && board_store_pieces[x - 1, y - 2] == 0)
                        {
                            check_store_blackpossiblemovements[x - 1, y - 2] = 1;
                        }
                        if ((x - 1) >= 0 && (x - 1) <= 7 && (y - 2) >= 0 && (y - 2) <= 7 && board_store_pieces[x - 1, y - 2] <= 6 && board_store_pieces[x - 1, y - 2] >= 1)
                        {
                            check_store_blackpossiblemovements[x - 1, y - 2] = 1;
                        }

                        if ((x - 2) >= 0 && (x - 2) <= 7 && (y - 1) >= 0 && (y - 1) <= 7 && board_store_pieces[x - 2, y - 1] == 0)
                        {
                            check_store_blackpossiblemovements[x - 2, y - 1] = 1;
                        }
                        if ((x - 2) >= 0 && (x - 2) <= 7 && (y - 1) >= 0 && (y - 1) <= 7 && board_store_pieces[x - 2, y - 1] <= 6 && board_store_pieces[x - 2, y - 1] >= 1)
                        {
                            check_store_blackpossiblemovements[x - 2, y - 1] = 1;
                        }
                    }

                    #endregion

                    //Torre Preta
                    #region TorrePreta
                    else if (board_store_pieces[x, y] == 14)
                    {
                        //Cima
                        for (int n = 1; n <= 7; n++)
                        {
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] == 0)
                            {
                                check_store_blackpossiblemovements[x, y + n] = 1;
                            }
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] == 6)
                            {
                                check_store_blackpossiblemovements[x, y + n] = 1;
                            }
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] <= 5 && board_store_pieces[x, y + n] >= 1)
                            {
                                check_store_blackpossiblemovements[x, y + n] = 1;
                                n = 999;
                            }
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] >= 11 && board_store_pieces[x, y + n] <= 16)
                            {
                                n = 999;
                            }
                        }
                        //Baixo
                        for (int n = -1; n >= -7; n--)
                        {
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] == 0)
                            {
                                check_store_blackpossiblemovements[x, y + n] = 1;
                            }
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] == 6)
                            {
                                check_store_blackpossiblemovements[x, y + n] = 1;
                            }
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] <= 5 && board_store_pieces[x, y + n] >= 1)
                            {
                                check_store_blackpossiblemovements[x, y + n] = 1;
                                n = -999;
                            }
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] >= 11 && board_store_pieces[x, y + n] <= 16)
                            {
                                n = -999;
                            }
                        }
                        //Direita
                        for (int n = 1; n <= 7; n++)
                        {
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] == 0)
                            {
                                check_store_blackpossiblemovements[x + n, y] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] == 6)
                            {
                                check_store_blackpossiblemovements[x + n, y] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] <= 5 && board_store_pieces[x + n, y] >= 1)
                            {
                                check_store_blackpossiblemovements[x + n, y] = 1;
                                n = 999;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] >= 11 && board_store_pieces[x + n, y] <= 16)
                            {
                                n = 999;
                            }
                        }
                        //Esquerda
                        for (int n = -1; n >= -7; n--)
                        {
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] == 0)
                            {
                                check_store_blackpossiblemovements[x + n, y] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] == 6)
                            {
                                check_store_blackpossiblemovements[x + n, y] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] <= 5 && board_store_pieces[x + n, y] >= 1)
                            {
                                check_store_blackpossiblemovements[x + n, y] = 1;
                                n = -999;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] >= 11 && board_store_pieces[x + n, y] <= 16)
                            {
                                n = -999;
                            }
                        }
                    }

                    #endregion

                    //Rainha Preta
                    #region RainhaPreta
                    else if (board_store_pieces[x, y] == 15)
                    {
                        //Cima
                        for (int n = 1; n <= 7; n++)
                        {
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] == 0)
                            {
                                check_store_blackpossiblemovements[x, y + n] = 1;
                            }
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] == 6)
                            {
                                check_store_blackpossiblemovements[x, y + n] = 1;
                            }
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] <= 5 && board_store_pieces[x, y + n] >= 1)
                            {
                                check_store_blackpossiblemovements[x, y + n] = 1;
                                n = 999;
                            }
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] >= 11 && board_store_pieces[x, y + n] <= 16)
                            {
                                n = 999;
                            }
                        }
                        //Baixo
                        for (int n = -1; n >= -7; n--)
                        {
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] == 0)
                            {
                                check_store_blackpossiblemovements[x, y + n] = 1;
                            }
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] == 6)
                            {
                                check_store_blackpossiblemovements[x, y + n] = 1;
                            }
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] <= 5 && board_store_pieces[x, y + n] >= 1)
                            {
                                check_store_blackpossiblemovements[x, y + n] = 1;
                                n = -999;
                            }
                            if ((x) >= 0 && (x) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x, y + n] >= 11 && board_store_pieces[x, y + n] <= 16)
                            {
                                n = -999;
                            }
                        }
                        //Direita
                        for (int n = 1; n <= 7; n++)
                        {
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] == 0)
                            {
                                check_store_blackpossiblemovements[x + n, y] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] == 6)
                            {
                                check_store_blackpossiblemovements[x + n, y] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] <= 5 && board_store_pieces[x + n, y] >= 1)
                            {
                                check_store_blackpossiblemovements[x + n, y] = 1;
                                n = 999;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] >= 11 && board_store_pieces[x + n, y] <= 16)
                            {
                                n = 999;
                            }
                        }
                        //Esquerda
                        for (int n = -1; n >= -7; n--)
                        {
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] == 0)
                            {
                                check_store_blackpossiblemovements[x + n, y] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] == 6)
                            {
                                check_store_blackpossiblemovements[x + n, y] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] <= 5 && board_store_pieces[x + n, y] >= 1)
                            {
                                check_store_blackpossiblemovements[x + n, y] = 1;
                                n = -999;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + n, y] >= 11 && board_store_pieces[x + n, y] <= 16)
                            {
                                n = -999;
                            }
                        }
                        //Baixo/esquerda
                        for (int n = -1; n >= -7; n--)
                        {
                            if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] == 0)
                            {
                                check_store_blackpossiblemovements[x + n, y + n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] == 6)
                            {
                                check_store_blackpossiblemovements[x + n, y + n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] <= 5 && board_store_pieces[x + n, y + n] >= 1)
                            {
                                check_store_blackpossiblemovements[x + n, y + n] = 1;
                                n = -999;
                            }
                            else if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] >= 11 && board_store_pieces[x + n, y + n] <= 16)
                            {
                                n = -999;
                            }
                        }
                        //Cima/Direita
                        for (int n = 1; n <= 7; n++)
                        {
                            if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] == 0)
                            {
                                check_store_blackpossiblemovements[x + n, y + n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] == 6)
                            {
                                check_store_blackpossiblemovements[x + n, y + n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] <= 5 && board_store_pieces[x + n, y + n] >= 1)
                            {
                                check_store_blackpossiblemovements[x + n, y + n] = 1;
                                n = 999;
                            }
                            else if ((x + n) >= 0 && (x + n) <= 7 && (y + n) >= 0 && (y + n) <= 7 && board_store_pieces[x + n, y + n] >= 11 && board_store_pieces[x + n, y + n] <= 16)
                            {
                                n = 999;
                            }
                        }
                        //baixo/direita
                        for (int n = -1; n >= -7; n--)
                        {
                            if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] == 0)
                            {
                                check_store_blackpossiblemovements[x + n, y - n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] == 6)
                            {
                                check_store_blackpossiblemovements[x + n, y - n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] <= 5 && board_store_pieces[x + n, y - n] >= 1)
                            {
                                check_store_blackpossiblemovements[x + n, y - n] = 1;
                                n = -999;
                            }
                            else if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] >= 11 && board_store_pieces[x + n, y - n] <= 16)
                            {
                                n = -999;
                            }
                        }
                        //cima/esquerda
                        for (int n = 1; n <= 7; n++)
                        {
                            if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] == 0)
                            {
                                check_store_blackpossiblemovements[x + n, y - n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] == 6)
                            {
                                check_store_blackpossiblemovements[x + n, y - n] = 1;
                            }
                            if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] <= 5 && board_store_pieces[x + n, y - n] >= 1)
                            {
                                check_store_blackpossiblemovements[x + n, y - n] = 1;
                                n = 999;
                            }
                            else if ((x + n) >= 0 && (x + n) <= 7 && (y - n) >= 0 && (y - n) <= 7 && board_store_pieces[x + n, y - n] >= 11 && board_store_pieces[x + n, y - n] <= 16)
                            {
                                n = 999;
                            }
                        }
                    }
                    #endregion

                    //Rei Preto
                    #region ReiPreto
                    else if (board_store_pieces[x, y] == 16)
                    {
                        if ((x) >= 0 && (x) <= 7 && (y + 1) >= 0 && (y + 1) <= 7 && board_store_pieces[x, y + 1] == 0)
                        {
                            check_store_blackpossiblemovements[x, y + 1] = 1;
                        }
                        if ((x) >= 0 && (x) <= 7 && (y + 1) >= 0 && (y + 1) <= 7 && board_store_pieces[x, y + 1] <= 6 && board_store_pieces[x, y + 1] >= 1)
                        {
                            check_store_blackpossiblemovements[x, y + 1] = 1;
                        }

                        if ((x + 1) >= 0 && (x + 1) <= 7 && (y + 1) >= 0 && (y + 1) <= 7 && board_store_pieces[x + 1, y + 1] == 0)
                        {
                            check_store_blackpossiblemovements[x + 1, y + 1] = 1;
                        }

                        if ((x + 1) >= 0 && (x + 1) <= 7 && (y + 1) >= 0 && (y + 1) <= 7 && board_store_pieces[x + 1, y + 1] <= 6 && board_store_pieces[x + 1, y + 1] >= 1)
                        {
                            check_store_blackpossiblemovements[x + 1, y + 1] = 1;
                        }

                        if ((x - 1) >= 0 && (x - 1) <= 7 && (y + 1) >= 0 && (y + 1) <= 7 && board_store_pieces[x - 1, y + 1] == 0)
                        {
                            check_store_blackpossiblemovements[x - 1, y + 1] = 1;
                        }
                        if ((x - 1) >= 0 && (x - 1) <= 7 && (y + 1) >= 0 && (y + 1) <= 7 && board_store_pieces[x - 1, y + 1] <= 6 && board_store_pieces[x - 1, y + 1] >= 1)
                        {
                            check_store_blackpossiblemovements[x - 1, y + 1] = 1;
                        }

                        //

                        if ((x + 1) >= 0 && (x + 1) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + 1, y] == 0)
                        {
                            check_store_blackpossiblemovements[x + 1, y] = 1;
                        }

                        if ((x + 1) >= 0 && (x + 1) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x + 1, y] <= 6 && board_store_pieces[x + 1, y] >= 1)
                        {
                            check_store_blackpossiblemovements[x + 1, y] = 1;
                        }

                        if ((x - 1) >= 0 && (x - 1) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x - 1, y] == 0)
                        {
                            check_store_blackpossiblemovements[x - 1, y] = 1;
                        }
                        if ((x - 1) >= 0 && (x - 1) <= 7 && (y) >= 0 && (y) <= 7 && board_store_pieces[x - 1, y] <= 6 && board_store_pieces[x - 1, y] >= 1)
                        {
                            check_store_blackpossiblemovements[x - 1, y] = 1;
                        }

                        //

                        if ((x + 1) >= 0 && (x + 1) <= 7 && (y - 1) >= 0 && (y - 1) <= 7 && board_store_pieces[x + 1, y - 1] == 0)
                        {
                            check_store_blackpossiblemovements[x + 1, y - 1] = 1;
                        }
                        if ((x + 1) >= 0 && (x + 1) <= 7 && (y - 1) >= 0 && (y - 1) <= 7 && board_store_pieces[x + 1, y - 1] <= 6 && board_store_pieces[x + 1, y - 1] >= 1)
                        {
                            check_store_blackpossiblemovements[x + 1, y - 1] = 1;
                        }

                        if ((x - 1) >= 0 && (x - 1) <= 7 && (y - 1) >= 0 && (y - 1) <= 7 && board_store_pieces[x - 1, y - 1] == 0)
                        {
                            check_store_blackpossiblemovements[x - 1, y - 1] = 1;
                        }
                        if ((x - 1) >= 0 && (x - 1) <= 7 && (y - 1) >= 0 && (y - 1) <= 7 && board_store_pieces[x - 1, y - 1] <= 6 && board_store_pieces[x - 1, y - 1] >= 1)
                        {
                            check_store_blackpossiblemovements[x - 1, y - 1] = 1;
                        }

                        if ((x) >= 0 && (x) <= 7 && (y - 1) >= 0 && (y - 1) <= 7 && board_store_pieces[x, y - 1] == 0)
                        {
                            check_store_blackpossiblemovements[x, y - 1] = 1;
                        }
                        if ((x) >= 0 && (x) <= 7 && (y - 1) >= 0 && (y - 1) <= 7 && board_store_pieces[x, y - 1] <= 6 && board_store_pieces[x, y - 1] >= 1)
                        {
                            check_store_blackpossiblemovements[x, y - 1] = 1;
                        }
                    }
                    #endregion
    
                }
            }

            for (int x = 0; x < 8; x++) //Vê se o rei oposto está em check e se estiver escreve no historico de movimentos
            {
                for (int y = 0; y < 8; y++)
                {
                    if (board_store_pieces[x, y] == 6 && check_store_blackpossiblemovements[x, y] == 1)
                    {
                        board_store_backcolors[x, y] = 1;
                        if (listbox_side.Items[2].ToString() != " Rei branco em check!")
                            listbox_side.Items.Insert(2, " Rei branco em check!");

                        Wkingoncheck = true;
                    }
                    else if (board_store_pieces[x, y] == 6 && check_store_blackpossiblemovements[x, y] == 0)
                    {
                        board_store_backcolors[x, y] = 0;
                        Wkingoncheck = false;
                    }
                }
            }
        }
        #endregion

        //Move as peças
        #region MovePiece()
        private void MovePiece()
        {
            DoLog(); //Guarda o movimento no historico
            board_store_pieces[XClicked, YClicked] = selectedpiece; //Move a peça
            board_store_pieces[XClickedPrev, YClickedPrev] = 0; //Apaga o que fica para traz
            DesselectPiece(); //Desceleciona
            KingsTowersMoved(); //Vê se os reis e torres se moveram (para o roque)
            DrawUpdateBoard(); //Atualiza o tabuleiro
            PawnPromotion(); //Promove um peão (se possivel)            
            ChangeTurn(); //Muda o turno
            DrawUpdateBoard(); //Atualiza o tabuleiro denovo, caso um peão seja promovido            
            DesselectPiece(); //Desceleciona denovo para calcular se algum rei está em check depois de promover o peão
            DoKingExist(); //Vê se ainda existem os dois reis (tem a ver com a musica do check)
            if (willsavereplay) //Guarda mais uma jogada no ficheiro se desejado
                SaveReplayFile();
        }
        #endregion

        //Captura En Passant (regra especial dos peões)
        #region MoveEnPassant()
        private void MoveEnPassant()
        {
            DoLogEnPassant();
            board_store_pieces[XClicked, YClicked] = selectedpiece; //Move a peça
            board_store_pieces[XClickedPrev, YClickedPrev] = 0; //Apaga o que fica para traz
            if (whiteturn)
                board_store_pieces[XClicked, YClicked - 1] = 0; //Apaga a peça inimiga preta
            else
                board_store_pieces[XClicked, YClicked + 1] = 0; //Apaga a peça inimiga branca
            DesselectPiece(); //Desceleciona
            KingsTowersMoved(); //Vê se os reis e torres se moveram (para o roque)
            DrawUpdateBoard(); //Atualiza o tabuleiro
            PawnPromotion(); //Promove um peão (se possivel)
            ChangeTurn(); //Muda o turno
            DrawUpdateBoard(); //Atualiza o tabuleiro denovo, caso um peão seja promovido            
            DoKingExist(); //Vê se ainda existem os dois reis (tem a ver com a musica do check)
            DesselectPiece(); //Desceleciona denovo para calcular se algum rei está em check depois de promover o peão
            if (willsavereplay) //Guarda mais uma jogada no ficheiro se desejado
                SaveReplayFile();
        }
        #endregion

        //Historico de movimentos especial para o EnPassant
        #region DoLogEnPassant()
        private void DoLogEnPassant()
        {
            movecount++; //conta o numero de movimentos no jogo
            if (whiteturn)
            {
                listbox_side.Items.Insert(1, " Rip: " + pieces[board_store_pieces[XClicked, YClicked - 1]]);
            }

            else
            {
                listbox_side.Items.Insert(1, " Rip: " + pieces[board_store_pieces[XClicked, YClicked + 1]]);
            }
            listbox_side.Items.Insert(1, movecount + ": " + pieces[selectedpiece] + ": de " + Xletter[XClickedPrev] + (YClickedPrev + 1) + " para " + Xletter[XClicked] + (YClicked + 1));
        }
        #endregion

        //Movimentos de Roque
        #region MoveRoque()
        private void MoveRoque()
        {
            DoLogRoque();
            if (XClicked == 0 && YClicked == 0 && selectedpiece == 6) //Torre esquerda branca
            {
                board_store_pieces[0, 0] = 0;
                board_store_pieces[4, 0] = 0;
                board_store_pieces[2, 0] = 6;
                board_store_pieces[3, 0] = 4;
            }
            else if (XClicked == 7 && YClicked == 0 && selectedpiece == 6) //Torre direita branca
            {
                board_store_pieces[7, 0] = 0;
                board_store_pieces[4, 0] = 0;
                board_store_pieces[6, 0] = 6;
                board_store_pieces[5, 0] = 4;
            }
            if (XClicked == 0 && YClicked == 7 && selectedpiece == 16) //Torre esquerda preta
            {
                board_store_pieces[0, 7] = 0;
                board_store_pieces[4, 7] = 0;
                board_store_pieces[2, 7] = 16;
                board_store_pieces[3, 7] = 14;
            }
            else if (XClicked == 7 && YClicked == 7 && selectedpiece == 16)//Torre direita preta
            {
                board_store_pieces[7, 7] = 0;
                board_store_pieces[4, 7] = 0;
                board_store_pieces[6, 7] = 16;
                board_store_pieces[5, 7] = 14;
            }
            DesselectPiece(); //Desceleciona
            KingsTowersMoved(); //Vê se os reis e torres se moveram (para o roque)
            DrawUpdateBoard(); //Atualiza o tabuleiro
            PawnPromotion(); //Promove um peão (se possivel)
            ChangeTurn(); //Muda o turno
            DrawUpdateBoard(); //Atualiza o tabuleiro denovo, caso um peão seja promovido            
            DoKingExist(); //Vê se ainda existem os dois reis (tem a ver com a musica do check)
            DesselectPiece(); //Desceleciona denovo para calcular se algum rei está em check depois de promover o peão
            if (willsavereplay) //Guarda mais uma jogada no ficheiro se desejado
                SaveReplayFile();
        }
        #endregion

        //Historico de movimentos especial para o Roque
        #region DoLogRoque()
        private void DoLogRoque()
        {
            movecount++; //conta o numero de movimentos no jogo
            if (whiteturn)
            {
                listbox_side.Items.Insert(1, movecount + ": " + "Roque do rei branco");
            }
            else
            {
                listbox_side.Items.Insert(1, movecount + ": " + "Roque do rei preto");
            }
                
        }
        #endregion

        //Cliques no tabuleiro (tudo copy-paste com uma alteração pequena)
        #region Cliques
        private void board_0_0_Click(object sender, EventArgs e)
        {
            XClicked = 0;
            YClicked = 0;
            ClickCommands();
        }
        private void board_1_0_Click(object sender, EventArgs e)
        {
            XClicked = 1;
            YClicked = 0;
            ClickCommands();
        }
        private void board_2_0_Click(object sender, EventArgs e)
        {
            XClicked = 2;
            YClicked = 0;
            ClickCommands();
        }
        private void board_3_0_Click(object sender, EventArgs e)
        {
            XClicked = 3;
            YClicked = 0;
            ClickCommands();
        }
        private void board_4_0_Click(object sender, EventArgs e)
        {
            XClicked = 4;
            YClicked = 0;
            ClickCommands();
        }
        private void board_5_0_Click(object sender, EventArgs e)
        {
            XClicked = 5;
            YClicked = 0;
            ClickCommands();
        }
        private void board_6_0_Click(object sender, EventArgs e)
        {
            XClicked = 6;
            YClicked = 0;
            ClickCommands();
        }
        private void board_7_0_Click(object sender, EventArgs e)
        {
            XClicked = 7;
            YClicked = 0;
            ClickCommands();
        }
        private void board_0_1_Click(object sender, EventArgs e)
        {
            XClicked = 0;
            YClicked = 1;
            ClickCommands();
        }
        private void board_1_1_Click(object sender, EventArgs e)
        {
            XClicked = 1;
            YClicked = 1;
            ClickCommands();
        }
        private void board_2_1_Click(object sender, EventArgs e)
        {
            XClicked = 2;
            YClicked = 1;
            ClickCommands();
        }
        private void board_3_1_Click(object sender, EventArgs e)
        {
            XClicked = 3;
            YClicked = 1;
            ClickCommands();
        }
        private void board_4_1_Click(object sender, EventArgs e)
        {
            XClicked = 4;
            YClicked = 1;
            ClickCommands();
        }
        private void board_5_1_Click(object sender, EventArgs e)
        {
            XClicked = 5;
            YClicked = 1;
            ClickCommands();
        }
        private void board_6_1_Click(object sender, EventArgs e)
        {
            XClicked = 6;
            YClicked = 1;
            ClickCommands();
        }
        private void board_7_1_Click(object sender, EventArgs e)
        {
            XClicked = 7;
            YClicked = 1;
            ClickCommands();
        }
        private void board_0_2_Click(object sender, EventArgs e)
        {
            XClicked = 0;
            YClicked = 2;
            ClickCommands();
        }
        private void board_1_2_Click(object sender, EventArgs e)
        {
            XClicked = 1;
            YClicked = 2;
            ClickCommands();
        }
        private void board_2_2_Click(object sender, EventArgs e)
        {
            XClicked = 2;
            YClicked = 2;
            ClickCommands();
        }
        private void board_3_2_Click(object sender, EventArgs e)
        {
            XClicked = 3;
            YClicked = 2;
            ClickCommands();
        }
        private void board_4_2_Click(object sender, EventArgs e)
        {
            XClicked = 4;
            YClicked = 2;
            ClickCommands();
        }
        private void board_5_2_Click(object sender, EventArgs e)
        {
            XClicked = 5;
            YClicked = 2;
            ClickCommands();
        }
        private void board_6_2_Click(object sender, EventArgs e)
        {
            XClicked = 6;
            YClicked = 2;
            ClickCommands();
        }
        private void board_7_2_Click(object sender, EventArgs e)
        {
            XClicked = 7;
            YClicked = 2;
            ClickCommands();
        }
        private void board_0_3_Click(object sender, EventArgs e)
        {
            XClicked = 0;
            YClicked = 3;
            ClickCommands();
        }
        private void board_1_3_Click(object sender, EventArgs e)
        {
            XClicked = 1;
            YClicked = 3;
            ClickCommands();
        }
        private void board_2_3_Click(object sender, EventArgs e)
        {
            XClicked = 2;
            YClicked = 3;
            ClickCommands();
        }
        private void board_3_3_Click(object sender, EventArgs e)
        {
            XClicked = 3;
            YClicked = 3;
            ClickCommands();
        }
        private void board_4_3_Click(object sender, EventArgs e)
        {
            XClicked = 4;
            YClicked = 3;
            ClickCommands();
        }
        private void board_5_3_Click(object sender, EventArgs e)
        {
            XClicked = 5;
            YClicked = 3;
            ClickCommands();
        }
        private void board_6_3_Click(object sender, EventArgs e)
        {
            XClicked = 6;
            YClicked = 3;
            ClickCommands();
        }
        private void board_7_3_Click(object sender, EventArgs e)
        {
            XClicked = 7;
            YClicked = 3;
            ClickCommands();
        }
        private void board_0_4_Click(object sender, EventArgs e)
        {
            XClicked = 0;
            YClicked = 4;
            ClickCommands();
        }
        private void board_1_4_Click(object sender, EventArgs e)
        {
            XClicked = 1;
            YClicked = 4;
            ClickCommands();
        }
        private void board_2_4_Click(object sender, EventArgs e)
        {
            XClicked = 2;
            YClicked = 4;
            ClickCommands();
        }
        private void board_3_4_Click(object sender, EventArgs e)
        {
            XClicked = 3;
            YClicked = 4;
            ClickCommands();
        }
        private void board_4_4_Click(object sender, EventArgs e)
        {
            XClicked = 4;
            YClicked = 4;
            ClickCommands();
        }
        private void board_5_4_Click(object sender, EventArgs e)
        {
            XClicked = 5;
            YClicked = 4;
            ClickCommands();
        }
        private void board_6_4_Click(object sender, EventArgs e)
        {
            XClicked = 6;
            YClicked = 4;
            ClickCommands();
        }
        private void board_7_4_Click(object sender, EventArgs e)
        {
            XClicked = 7;
            YClicked = 4;
            ClickCommands();
        }
        private void board_0_5_Click(object sender, EventArgs e)
        {
            XClicked = 0;
            YClicked = 5;
            ClickCommands();
        }
        private void board_1_5_Click(object sender, EventArgs e)
        {
            XClicked = 1;
            YClicked = 5;
            ClickCommands();
        }
        private void board_2_5_Click(object sender, EventArgs e)
        {
            XClicked = 2;
            YClicked = 5;
            ClickCommands();
        }
        private void board_3_5_Click(object sender, EventArgs e)
        {
            XClicked = 3;
            YClicked = 5;
            ClickCommands();
        }
        private void board_4_5_Click(object sender, EventArgs e)
        {
            XClicked = 4;
            YClicked = 5;
            ClickCommands();
        }
        private void board_5_5_Click(object sender, EventArgs e)
        {
            XClicked = 5;
            YClicked = 5;
            ClickCommands();
        }
        private void board_6_5_Click(object sender, EventArgs e)
        {
            XClicked = 6;
            YClicked = 5;
            ClickCommands();
        }
        private void board_7_5_Click(object sender, EventArgs e)
        {
            XClicked = 7;
            YClicked = 5;
            ClickCommands();
        }
        private void board_0_6_Click(object sender, EventArgs e)
        {
            XClicked = 0;
            YClicked = 6;
            ClickCommands();
        }
       private void board_1_6_Click(object sender, EventArgs e)
        {
            XClicked = 1;
            YClicked = 6;
            ClickCommands();
        }
        private void board_2_6_Click(object sender, EventArgs e)
        {
            XClicked = 2;
            YClicked = 6;
            ClickCommands();
        }
        private void board_3_6_Click(object sender, EventArgs e)
        {
            XClicked = 3;
            YClicked = 6;
            ClickCommands();
        }
        private void board_4_6_Click(object sender, EventArgs e)
        {
            XClicked = 4;
            YClicked = 6;
            ClickCommands();
        }
        private void board_5_6_Click(object sender, EventArgs e)
        {
            XClicked = 5;
            YClicked = 6;
            ClickCommands();
        }
        private void board_6_6_Click(object sender, EventArgs e)
        {
            XClicked = 6;
            YClicked = 6;
            ClickCommands();
        }
        private void board_7_6_Click(object sender, EventArgs e)
        {
            XClicked = 7;
            YClicked = 6;
            ClickCommands();
        }
        private void board_0_7_Click(object sender, EventArgs e)
        {
            XClicked = 0;
            YClicked = 7;
            ClickCommands();
        }
        private void board_1_7_Click(object sender, EventArgs e)
        {
            XClicked = 1;
            YClicked = 7;
            ClickCommands();
        }
        private void board_2_7_Click(object sender, EventArgs e)
        {
            XClicked = 2;
            YClicked = 7;
            ClickCommands();
        }
        private void board_3_7_Click(object sender, EventArgs e)
        {
            XClicked = 3;
            YClicked = 7;
            ClickCommands();
        }
        private void board_4_7_Click(object sender, EventArgs e)
        {
            XClicked = 4;
            YClicked = 7;
            ClickCommands();
        }
        private void board_5_7_Click(object sender, EventArgs e)
        {
            XClicked = 5;
            YClicked = 7;
            ClickCommands();
        }
        private void board_6_7_Click(object sender, EventArgs e)
        {
            XClicked = 6;
            YClicked = 7;
            ClickCommands();
        }
        private void board_7_7_Click(object sender, EventArgs e)
        {
            XClicked = 7;
            YClicked = 7;
            ClickCommands();
        }
        #endregion

        //Botões
        #region Btns_Click
        private void btn_newgame_Click(object sender, EventArgs e) //Novo jogo
        {
            if (!firstgame) //Se não for o primeiro jogo, pergunta se tem a certeza
            {
                if (MessageBox.Show("New Game?", "", MessageBoxButtons.YesNo,MessageBoxIcon.Question) == DialogResult.Yes)
                {
                     
                    GameSetup(); //Inicializa um novo jogo
                }
            }
            else //Se for o primeiro jogo
            {               
                GameSetup(); //Inicializa um novo jogo
                firstgame = false; //Diz que já não é o primeiro jogo
            }
        }
        private void btn_close_Click(object sender, EventArgs e) //Fecha o jogo
        {             
            Environment.Exit(0); //Fecha o jogo
        }
        private void btn_minimize_Click(object sender, EventArgs e) //Minimiza o jogo
        {
            this.WindowState = FormWindowState.Minimized;
        }


        private void button6_Click(object sender, EventArgs e) //Guarda o jogo
        {
            SaveCurrGame();
        }
        private void button7_Click(object sender, EventArgs e) //Carrega o jogo
        {
            LoadSavedGame();
        }

        #endregion

        //Promover peão
        #region PawnPromotion()
        private void PawnPromotion()
        {
            //Usei dialog result como variavel publica, porque é a forma que dá menos trabalho
            if (selectedpiece == 1 && YClicked == 7) //Promove peão branco se chegar ao topo do tabuleiro
            {
                PawnPromoWhite customMessageBox = new PawnPromoWhite(); //Abre um form que eu usei como message box
                DialogResult piece = customMessageBox.ShowDialog(); //Guarda o resultado do que for escolhido
                if (piece == DialogResult.OK)
                {
                    board_store_pieces[XClicked, YClicked] = 2; //Bispo
                    listbox_side.Items.Insert(2, " Peão promovido: " + pieces[board_store_pieces[XClicked, YClicked]]);
                }
                else if (piece == DialogResult.Yes)
                {
                    board_store_pieces[XClicked, YClicked] = 3; //Cavalo
                    listbox_side.Items.Insert(2, " Peão promovido: " + pieces[board_store_pieces[XClicked, YClicked]]);
                }
                else if (piece == DialogResult.No)
                {
                    board_store_pieces[XClicked, YClicked] = 4; //Torre 
                    listbox_side.Items.Insert(2, " Peão promovido: " + pieces[board_store_pieces[XClicked, YClicked]]);
                }
                else if (piece == DialogResult.Cancel)
                {
                    board_store_pieces[XClicked, YClicked] = 5; //Rainha
                    listbox_side.Items.Insert(2, " Peão promovido: " + pieces[board_store_pieces[XClicked, YClicked]]);
                }
            }
            else if (selectedpiece == 11 && YClicked == 0) //Promove peão preto se chegar ao fundo do tabuleiro
            {
                PawnPromoBlack customMessageBox = new PawnPromoBlack(); //Abre um form que eu usei como message box
                DialogResult piece = customMessageBox.ShowDialog(); //Guarda o resultado do que for escolhido
                if (piece == DialogResult.OK)
                {
                    board_store_pieces[XClicked, YClicked] = 12;
                    listbox_side.Items.Insert(2, " Peão promovido: " + pieces[board_store_pieces[XClicked, YClicked]]);
                }
                else if (piece == DialogResult.Yes)
                {
                    board_store_pieces[XClicked, YClicked] = 13;
                    listbox_side.Items.Insert(2, " Peão promovido: " + pieces[board_store_pieces[XClicked, YClicked]]);
                }
                else if (piece == DialogResult.No)
                {
                    board_store_pieces[XClicked, YClicked] = 14;
                    listbox_side.Items.Insert(2, " Peão promovido: " + pieces[board_store_pieces[XClicked, YClicked]]);
                }
                else if (piece == DialogResult.Cancel)
                {
                    board_store_pieces[XClicked, YClicked] = 15;
                    listbox_side.Items.Insert(2, " Peão promovido: " + pieces[board_store_pieces[XClicked, YClicked]]);
                }
            }            
        }
        #endregion

        //"Low Health Pokemon Black/White Sound" durante um check
        #region PlaySoundLowHP()
        private void PlaySoundLowHP()
        {
            Xadrez_Louco_Novo.Properties.Resources.lowhpPokeBW.Position = 0;
            checksoundplayer.Stream = null;
            checksoundplayer.Stream = Xadrez_Louco_Novo.Properties.Resources.lowhpPokeBW;
            checksoundplayer.Stream.Position = 0;
            checksoundplayer.PlayLooping();
        }
        #endregion

        //Historico de movimentos
        #region DoLog()
        private void DoLog()
        {
            movecount++; //conta o numero de movimentos no jogo
            if (board_store_pieces[XClicked,YClicked] != 0) //Caso alguma peça seja comida
            {
                listbox_side.Items.Insert(1, " Rip: " + pieces[board_store_pieces[XClicked, YClicked]]);
            }
            //Moveu tal peça de... para...
            listbox_side.Items.Insert(1, movecount + ": " + pieces[selectedpiece] + ": de " + Xletter[XClickedPrev] + (YClickedPrev + 1) + " para " + Xletter[XClicked] + (YClicked + 1));
        }
        #endregion

        //Vê se existe algum rei
        #region DoKingExist()
        private void DoKingExist()
        {
            //Se existirem 2 reis e algum estiver em check, toca o som, só existir 1, ou nenhum estiver em check, para o som
            int count = 0;
            for (int x = 0; x < 8; x++)
                for (int y = 0; y < 8; y++)
                {
                    if (board_store_pieces[x,y] == 16 || board_store_pieces[x, y] == 6)
                    {
                        count++;
                    }
                }
            if (count != 2)
            {
                checksoundplayer.Stop();
            }
            else
            {
                if ((Wkingoncheck || Bkingoncheck))
                    PlaySoundLowHP();
                else
                    checksoundplayer.Stop();
            }
        }
        #endregion

        //Vê se o rei e as torres já se moveram
        #region KingsTowersMoved()
        private void KingsTowersMoved()
        {
            if (board_store_pieces[4,0] != 6)            
                whitekingmoved = true;
            if (board_store_pieces[4, 7] != 16)
                blackkingmoved = true;

            if (board_store_pieces[0, 0] != 4)
                whitelefttowermoved = true;
            if (board_store_pieces[7, 0] != 4)
                whiterighttowermoved = true;
            if (board_store_pieces[0, 7] != 14)
                blacklefttowermoved = true;
            if (board_store_pieces[7, 7] != 14)
                blackrighttowermoved = true;
        }
        #endregion

        //Debug
        #region Debug
        //Ferramentas que me ajudaram a testar o jogo
        private void debug_timer_Tick(object sender, EventArgs e)
        {
            listBox_sideXtra.Items.Clear();

            for (int y = 0; y < 8; y++)
            {
                string stringS = "";
                for (int x = 0; x < 8; x++)
                    stringS = stringS + check_store_blackpossiblemovements[x, y] + "  ";

                listBox_sideXtra.Items.Insert(0, stringS);

            }
            listBox_sideXtra.Items.Insert(0, "BlackMovement");

            for (int y = 0; y < 8; y++)
            {
                string stringS = "";
                for (int x = 0; x < 8; x++)
                    stringS = stringS + check_store_whitepossiblemovements[x, y] + "  ";

                listBox_sideXtra.Items.Insert(0, stringS);

            }
            listBox_sideXtra.Items.Insert(0, "WhiteMovement");

            for (int y = 0; y < 8; y++)
            {
                string stringS = "";
                for (int x = 0; x < 8; x++)
                    stringS = stringS + board_store_canmove[x, y] + "  ";

                listBox_sideXtra.Items.Insert(0, stringS);

            }
            listBox_sideXtra.Items.Insert(0, "Canmove");

            for (int y = 0; y < 8; y++)
            {
                string stringS = "";
                for (int x = 0; x < 8; x++)
                    stringS = stringS + board_store_pieces[x, y] + "  ";

                listBox_sideXtra.Items.Insert(0, stringS);

            }
            listBox_sideXtra.Items.Insert(0, "Pieces");
        }
        private void btn_debugmode_Click(object sender, EventArgs e)
        {
            if (this.Width == 800)
                this.Width = 600;
            else
                this.Width = 800;
        }
        private void debug_whiteturn_Click(object sender, EventArgs e)
        {
            whiteturn = true;
        }
        private void debug_blackturn_Click(object sender, EventArgs e)
        {
            whiteturn = false;
        }
        private void debug_deletepiece_Click(object sender, EventArgs e)
        {
            if (debugDelete == true)
            {
                debug_deletepiece.FlatStyle = FlatStyle.Standard;
                debugDelete = false;
            }
            else
            {
                debug_deletepiece.FlatStyle = FlatStyle.Flat;
                debugDelete = true;
                if (placequeen == true)
                    debug_placequeen.PerformClick();
            }
        }
        private void debug_placequeen_Click(object sender, EventArgs e)
        {
            if (placequeen == true)
            {
                debug_placequeen.FlatStyle = FlatStyle.Standard;
                placequeen = false;
            }
            else
            {
                debug_placequeen.FlatStyle = FlatStyle.Flat;
                placequeen = true;
                if (debugDelete == true)
                    debug_deletepiece.PerformClick();
            }
        }
        private void debug_clearlog_Click(object sender, EventArgs e)
        {
            listbox_side.Items.Clear();
            listbox_side.Items.Insert(0, "Move Log:");
        }
        private void savereplay_Click(object sender, EventArgs e)
        {            
        }
        #endregion

        //Replay Viewer :3
        #region ReplayViewer
        //Botão que abre o replay viewer
        #region OpenRV
        private void btn_replayviewer_Click(object sender, EventArgs e)
        {
            ReplayViewer rpv = new ReplayViewer(); //Abre o form com o replay viewer
            rpv.Show();
        }
        #endregion

        //Guarda um ficheiro com o historico de movimentos
        #region SaveReplayFile()
        private void SaveReplayFile()
        {
            string replayfolder = myExeDir + "\\replays\\";
            string stringS = "";
            for (int y = 0; y < 8; y++) //Guarda todas as peças do jogo num formato "peça_peça_peça_peça_..." Por exemplo o inicio seria "4_3_2_6_5_2_3_4_1_1_1_1_1_1_1_1_0_0_0_0_0_..." que são as posições iniciais dos brancos e parte do espaço vazio
                for (int x = 0; x < 8; x++)
                {
                    if (stringS == "") //Primeiro carater (tem de ser assim com if porque a linha não pode começar com "_")
                        stringS = stringS + board_store_pieces[x, y].ToString();
                    else //Escreve os outros carateres
                        stringS = stringS + "_" + board_store_pieces[x, y];
                }
            using (StreamWriter sw = File.AppendText(replayfolder + replayfilename))
            {
                sw.WriteLine(stringS);                
            }
        }
        #endregion
        #endregion

        //Guarda o jogo atual
        #region SaveCurrGame()
        private void SaveCurrGame()
        {
            //Escritor binario
            BinaryWriter bw;

            //Criar Ficheiro
            try
            {
                bw = new BinaryWriter(new FileStream("save.chesssave", FileMode.Create));
            }
            catch (IOException e)
            {
                MessageBox.Show("Ficheiro não criado/substituido!");
                return;
            }

            //Escrever no ficheiro
            try
            {
                SaveLogToArray();

                bw.Write(whiteturn);
                bw.Write(movecount);
                bw.Write(whitekingmoved);
                bw.Write(blackkingmoved);
                bw.Write(whiterighttowermoved);
                bw.Write(whitelefttowermoved);
                bw.Write(blackrighttowermoved);
                bw.Write(blacklefttowermoved);
                bw.Write(loglinecount);
                bw.Write(replayfilename);

                for (int y = 0; y<= 7; y++)
                    for (int x = 0; x<= 7; x++)
                        bw.Write(board_store_pieces[x, y]);

                for (int n = 0; n < savelog.Length; n++)
                    bw.Write(savelog[n]);

                bw.Close(); //Fechar      

                this.BackColor = Color.Green;
                btn_savefile.Enabled = false;
                btn_loadfile.Enabled = false;                
                timer_infoblink.Enabled = true;
            }
            catch (IOException e)
            {
                MessageBox.Show("Erro a gravar ficheiro!");
                return;
            }
            bw.Close(); //Fechar            
        }
        #endregion

        //Carrega um jogo guardado
        #region LoadSavedGame()
        private void LoadSavedGame()
        {
            if (MessageBox.Show("Would you like to load\na previous saved game?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                BinaryReader br;

                //Abre ficheiro
                try
                {
                    br = new BinaryReader(new FileStream("save.chesssave", FileMode.Open));
                }
                catch (IOException e)
                {
                    MessageBox.Show("Erro a abrir ficheiro!");
                    return;
                }

                //Copia as variaveis guardadas no ficheiro
                try
                {
                    whiteturn = br.ReadBoolean();
                    movecount = br.ReadInt32();
                    whitekingmoved = br.ReadBoolean();
                    blackkingmoved = br.ReadBoolean();
                    whiterighttowermoved = br.ReadBoolean();
                    whitelefttowermoved = br.ReadBoolean();
                    blackrighttowermoved = br.ReadBoolean();
                    blacklefttowermoved = br.ReadBoolean();
                    loglinecount = br.ReadInt32();
                    replayfilename = br.ReadString();

                    for (int y = 0; y < 8; y++) //Limpa os arrays relacionados ao tabuleiro
                        for (int x = 0; x < 8; x++)
                        {
                            board_store_backcolors[x, y] = 0;
                            check_store_blackpossiblemovements[x, y] = 0;
                            check_store_whitepossiblemovements[x, y] = 0;
                            board_store_canmove[x, y] = 0;
                        }

                    for (int y = 0; y < 8; y++)
                        for (int x = 0; x < 8; x++)
                            board_store_pieces[x, y] = br.ReadInt32();

                    listbox_side.Items.Clear(); //Limpa listbox
                    listbox_side.Items.Insert(0, "Move Log:");
                    for (int n = 0; n < loglinecount; n++)
                        listbox_side.Items.Add(br.ReadString());
                    listbox_side.Items.Add("");
                    listbox_side.Items.Add("");
                    DesselectPiece();
                    CleanBackColors();
                    DrawUpdateBoard();

                    br.Close(); //Fechar

                    this.BackColor = Color.Green;
                    btn_savefile.Enabled = false;
                    btn_loadfile.Enabled = false;
                    timer_infoblink.Enabled = true;
                }
                catch (IOException e)
                {
                    MessageBox.Show("Erro a carregar ficheiro!");
                    return;
                }

                br.Close(); //Fechar
            }
            
        }
        #endregion

        //Guarda o historico num array para depois guardar num ficheiro
        #region SaveLogToArray()
        private void SaveLogToArray()
        {
            Array.Resize(ref savelog, listbox_side.Items.Count - 1);

            loglinecount = (listbox_side.Items.Count - 1);

            for (int n = 0; n < listbox_side.Items.Count - 1; n++)
            {
                int n1 = n + 1;
                savelog[n] = this.listbox_side.GetItemText(this.listbox_side.Items[n1]);
            }
        }
        #endregion

        //Timer que reseta a cor do form (uso para piscar verde quando gravo ou carrego o jogo)
        #region timer_infoblink_tick
        private void timer_infoblink_Tick(object sender, EventArgs e)
        {
            btn_savefile.Enabled = true;
            btn_loadfile.Enabled = true;
            this.BackColor = Color.FromArgb(64, 64, 64);
            timer_infoblink.Enabled = false;
        }
        #endregion


        //Extras (OFF)
        #region Extras
        /*
    #region SaveThings (OFF)
    #region SaveCurrGameXML()
    private void SaveCurrGameXML()
    {
        XmlSerializer xmlformatter = new XmlSerializer(typeof(int[,]));
        FileStream fs = new FileStream("test.xml", FileMode.Create);
        xmlformatter.Serialize(fs, board_store_pieces);
        fs.Close();
    }
    #endregion
    #region LoadGameXML()
    private void LoadGameXML() //
    {
        IFormatter xmlformatter = new BinaryFormatter();
        FileStream fs = new FileStream("test.xml", FileMode.Create);
        board_store_pieces = (int[,])xmlformatter.Deserialize(fs);
        fs.Close();
    }
    #endregion
    #region SaveCurrGameBinS()
    private void SaveCurrGameBinS()
    {
        IFormatter binformatter = new BinaryFormatter();
        FileStream fs = new FileStream("test.bin", FileMode.Create);
        binformatter.Serialize(fs, board_store_pieces);
        fs.Close();
    }
    #endregion
    #region LoadGameBinS()
    private void LoadGameBinS()
    {
        IFormatter binformatter = new BinaryFormatter();
        FileStream fs = new FileStream("test.bin", FileMode.Create);
        board_store_pieces = (int[,])binformatter.Deserialize(fs);
        fs.Close();
    }
    #endregion*/
        #endregion

    }
}
//Xadrez Louco, by João Paulo Vasconcelos, 12ºK
