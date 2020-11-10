namespace Xadrez_Louco_Novo
{
    partial class PawnPromoBlack
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PawnPromoBlack));
            this.select_bishop = new System.Windows.Forms.PictureBox();
            this.select_tower = new System.Windows.Forms.PictureBox();
            this.select_knight = new System.Windows.Forms.PictureBox();
            this.select_queen = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.select_bishop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.select_tower)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.select_knight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.select_queen)).BeginInit();
            this.SuspendLayout();
            // 
            // select_bishop
            // 
            this.select_bishop.BackColor = System.Drawing.Color.Gray;
            this.select_bishop.BackgroundImage = global::Xadrez_Louco_Novo.Properties.Resources.blackBispo;
            this.select_bishop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.select_bishop.Location = new System.Drawing.Point(300, 20);
            this.select_bishop.Name = "select_bishop";
            this.select_bishop.Size = new System.Drawing.Size(100, 100);
            this.select_bishop.TabIndex = 1;
            this.select_bishop.TabStop = false;
            this.select_bishop.Click += new System.EventHandler(this.select_bishop_Click);
            // 
            // select_tower
            // 
            this.select_tower.BackColor = System.Drawing.Color.Silver;
            this.select_tower.BackgroundImage = global::Xadrez_Louco_Novo.Properties.Resources.blackTorre;
            this.select_tower.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.select_tower.Location = new System.Drawing.Point(200, 20);
            this.select_tower.Name = "select_tower";
            this.select_tower.Size = new System.Drawing.Size(100, 100);
            this.select_tower.TabIndex = 2;
            this.select_tower.TabStop = false;
            this.select_tower.Click += new System.EventHandler(this.select_tower_Click);
            // 
            // select_knight
            // 
            this.select_knight.BackColor = System.Drawing.Color.Gray;
            this.select_knight.BackgroundImage = global::Xadrez_Louco_Novo.Properties.Resources.blackCavalo;
            this.select_knight.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.select_knight.Location = new System.Drawing.Point(100, 20);
            this.select_knight.Name = "select_knight";
            this.select_knight.Size = new System.Drawing.Size(100, 100);
            this.select_knight.TabIndex = 3;
            this.select_knight.TabStop = false;
            this.select_knight.Click += new System.EventHandler(this.select_knight_Click);
            // 
            // select_queen
            // 
            this.select_queen.BackColor = System.Drawing.Color.Silver;
            this.select_queen.BackgroundImage = global::Xadrez_Louco_Novo.Properties.Resources.blackQueen;
            this.select_queen.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.select_queen.Location = new System.Drawing.Point(0, 20);
            this.select_queen.Name = "select_queen";
            this.select_queen.Size = new System.Drawing.Size(100, 100);
            this.select_queen.TabIndex = 4;
            this.select_queen.TabStop = false;
            this.select_queen.Click += new System.EventHandler(this.select_queen_Click);
            // 
            // Form3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.ClientSize = new System.Drawing.Size(400, 120);
            this.Controls.Add(this.select_bishop);
            this.Controls.Add(this.select_tower);
            this.Controls.Add(this.select_knight);
            this.Controls.Add(this.select_queen);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form3";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select Piece";
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.movewindow_trigger);
            ((System.ComponentModel.ISupportInitialize)(this.select_bishop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.select_tower)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.select_knight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.select_queen)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox select_bishop;
        private System.Windows.Forms.PictureBox select_tower;
        private System.Windows.Forms.PictureBox select_knight;
        private System.Windows.Forms.PictureBox select_queen;
    }
}