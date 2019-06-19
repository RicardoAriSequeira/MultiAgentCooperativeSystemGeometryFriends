using GeometryFriends.XNAStub.DrawingInstructions;
using System;

namespace GeometryFriends.XNAStub
{
    internal class DrawableGameComponent
    {
        private bool _initialized;
        private int _drawOrder;
        private int _updateOrder;
        private bool _visible = true;

        public IGraphicsDevice GraphicsDevice { get; set; }

        public int DrawOrder
        {
            get { return _drawOrder; }
            set
            {
                if (_drawOrder != value)
                {
                    _drawOrder = value;
                    if (DrawOrderChanged != null)
                        DrawOrderChanged(this, null);
                    OnDrawOrderChanged(this, null);
                }
            }
        }

        public int UpdateOrder
        {
            get { return _updateOrder; }
            set
            {
                if (_updateOrder != value)
                {
                    _updateOrder = value;
                    if (this.UpdateOrderChanged != null)
                        this.UpdateOrderChanged(this, EventArgs.Empty);
                    OnUpdateOrderChanged(this, null);
                }
            }
        }
        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> UpdateOrderChanged;

        public bool Visible
        {
            get { return _visible; }
            set
            {
                if (_visible != value)
                {
                    _visible = value;
                    if (VisibleChanged != null)
                        VisibleChanged(this, EventArgs.Empty);
                    OnVisibleChanged(this, EventArgs.Empty);
                }
            }
        }
        public event EventHandler<EventArgs> DrawOrderChanged;
        public event EventHandler<EventArgs> VisibleChanged;

        public DrawableGameComponent(IGraphicsDevice drawingDevice)
        {
            GraphicsDevice = drawingDevice;
        }

        public virtual void Initialize()
        {
            if (!_initialized)
            {
                _initialized = true;
                LoadContent();
            }
        }

        protected virtual void LoadContent() { }

        protected virtual void UnloadContent() { }

        public virtual void Update(GameTime gameTime) { }

        public virtual void Draw(GameTime gameTime, DrawingInstructionsBatch instructionsBatch) { }

        protected virtual void OnVisibleChanged(object sender, EventArgs args) { }

        protected virtual void OnDrawOrderChanged(object sender, EventArgs args) { }

        protected virtual void OnUpdateOrderChanged(object sender, EventArgs args) { }

        protected virtual void OnEnabledChanged(object sender, EventArgs args) { }
    }
}
