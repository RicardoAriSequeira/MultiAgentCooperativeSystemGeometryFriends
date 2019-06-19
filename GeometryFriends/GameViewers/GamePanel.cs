using System.Windows.Forms;

namespace GeometryFriends.GameViewers
{
    internal class GamePanel : Panel
    {

        public GamePanel()
        {
            //set proper style to draw on the panel
            this.SetStyle(
                System.Windows.Forms.ControlStyles.UserPaint | 
                System.Windows.Forms.ControlStyles.AllPaintingInWmPaint | 
                //System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer | // already done manually!
                System.Windows.Forms.ControlStyles.Opaque,
                true);

            //this.Dock = DockStyle.Fill;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // GamePanel
            // 
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.Transparent;
            this.ResumeLayout(false);

        }
    }
}
