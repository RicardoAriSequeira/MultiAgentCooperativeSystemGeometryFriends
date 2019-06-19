namespace GeometryFriends.GameViewers
{
    partial class GeometryFriendsViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GeometryFriendsViewer));
            this.drawingPanel = new GeometryFriends.GameViewers.GamePanel();
            this.SuspendLayout();
            // 
            // drawingPanel
            // 
            this.drawingPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.drawingPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.drawingPanel.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.drawingPanel.Location = new System.Drawing.Point(0, 0);
            this.drawingPanel.Margin = new System.Windows.Forms.Padding(0);
            this.drawingPanel.Name = "drawingPanel";
            this.drawingPanel.Size = new System.Drawing.Size(1296, 837);
            this.drawingPanel.TabIndex = 0;
            this.drawingPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.drawingPanel_Paint);
            this.drawingPanel.MouseEnter += new System.EventHandler(this.drawingPanel_MouseEnter);
            this.drawingPanel.MouseLeave += new System.EventHandler(this.drawingPanel_MouseLeave);
            // 
            // GeometryFriendsViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(1280, 798);
            this.Controls.Add(this.drawingPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(200, 200);
            this.Name = "GeometryFriendsViewer";
            this.Text = "Geometry Friends";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GeometryFriendsViewer_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GeometryFriendsViewer_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.GeometryFriendsViewer_KeyUp);
            this.Resize += new System.EventHandler(this.GeometryFriendsViewer_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private GamePanel drawingPanel;

    }
}

