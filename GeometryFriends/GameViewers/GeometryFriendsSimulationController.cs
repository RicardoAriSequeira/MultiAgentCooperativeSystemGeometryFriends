using GeometryFriends.XNAStub;
using System;
using System.Windows.Forms;

namespace GeometryFriends.GameViewers
{
    internal partial class GeometryFriendsSimulationController : Form
    {
        public GeometryFriendsSimulationController(Game engine)
        {
            InitializeComponent();
            engine.GameExit += button1_Click;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //exit the viewer
            if (this != null)
            {
                if (this.InvokeRequired)
                {
                    if (!this.IsDisposed)
                    {
                        try
                        {
                            this.BeginInvoke((MethodInvoker)delegate()
                            {
                                this.Close();
                            });
                        }
                        catch (ObjectDisposedException)
                        {
                            //already disposed, nothing to be done
                        }
                    }
                }
                else
                {
                    if (!this.IsDisposed)
                    {
                        try
                        {
                            this.Close();
                        }
                        catch (ObjectDisposedException)
                        {
                            //already disposed, nothing to be done
                        }
                    }
                }
            }            
        }
    }
}
