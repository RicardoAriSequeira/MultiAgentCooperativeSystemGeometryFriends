using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using MSColor = System.Drawing.Color;
using MSMatrix = System.Drawing.Drawing2D.Matrix;
using XNAButtonState = GeometryFriends.XNAStub.ButtonState;
using XNAKeys = GeometryFriends.XNAStub.Keys;

namespace GeometryFriends.GameViewers
{
    internal partial class GeometryFriendsViewer : Form, IInputDevice, IGraphicsDevice
    {
        public bool _exiting;
        //Associated Interactive Platform
        //TODO: improve this integration
        public InteractivePlatform Platform { get; set; }

        //input
        private static readonly object _syncKeys = new Object();
        private static List<XNAKeys> _keysPressed;
        private static List<XNAKeys> _keysPressedTracker;
        private MouseState _mouseState;

        //drawing
        private readonly object _syncDrawing = new Object();
        private Bitmap _gamePanelBackbuffer;
        private Graphics _gamePanelBackbufferGraphics;
        private MSMatrix transformApplied, transformInitial;
        private int previousDPanelWidth, previousDPanelHeight;

        //bitmap where screenshot is stored
        private Bitmap screenshot;

        //resources
        Dictionary<string, Image> _imagesCache;
        Dictionary<int, Brush> _brushesCache;
        Dictionary<int, Pen> _penCache;

        public GeometryFriendsViewer()
        {
            InitializeComponent();
            
            _imagesCache = new Dictionary<string, Image>();
            _brushesCache = new Dictionary<int, Brush>();
            _penCache = new Dictionary<int, Pen>();

            _gamePanelBackbuffer = null;
            _exiting = false;
            _keysPressed = new List<XNAKeys>();
            _keysPressedTracker = new List<XNAKeys>();

            //force the required resolution for geometry friends
            //drawingPanel.Width = 1280;
            //drawingPanel.Height = 800;

            lock (_syncDrawing)
            {
                //we want to creaty only one Bitmap and dispose of it at the end
                _gamePanelBackbuffer = new Bitmap(this.drawingPanel.Width, this.drawingPanel.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                _gamePanelBackbufferGraphics = Graphics.FromImage(_gamePanelBackbuffer);
            }

            //prepare for screen resize
            transformInitial = _gamePanelBackbufferGraphics.Transform;
            transformApplied = _gamePanelBackbufferGraphics.Transform;
            previousDPanelWidth = drawingPanel.Width;
            previousDPanelHeight = drawingPanel.Height;

            // Capture extra mouse events.
            this.MouseWheel += OnMouseScroll;
        }

        private Image GetImageFromCache(string imagePath)
        {
            if (!_imagesCache.ContainsKey(imagePath))
            {
                _imagesCache[imagePath] = Image.FromFile(imagePath);   
            }
            return _imagesCache[imagePath];
        }

        private Brush GetBrushFromCache(MSColor brushColor)
        {
            int brushID = brushColor.ToArgb();

            if (!_brushesCache.ContainsKey(brushID))
            {
                _brushesCache[brushID] = new SolidBrush(brushColor);
            }
            return _brushesCache[brushID];
        }

        private Pen GetPenFromCache(MSColor penColor, float thickness)
        {
            int penID = penColor.ToArgb();

            if (!_penCache.ContainsKey(penID))
            {
                Pen newPen = new Pen(GetBrushFromCache(penColor), thickness);
                newPen.Alignment = PenAlignment.Inset;
                //newPen.DashCap = DashCap.Round;
                _penCache[penID] = newPen; ;
            }
            Pen tmp = _penCache[penID];
            tmp.Width = thickness;
            return tmp;
        }

        private void drawingPanel_Paint(object sender, PaintEventArgs e)
        {
            lock (_syncDrawing)
            {
                //e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawImageUnscaled(_gamePanelBackbuffer, 0, 0);
            }
        }

        private void OnMouseScroll(object sender, MouseEventArgs mouseEventArgs)
        {
            _mouseState.ScrollWheelValue += mouseEventArgs.Delta;
        }

        private void GeometryFriendsViewer_KeyDown(object sender, KeyEventArgs e)
        {
            XNAKeys xnaKey = (XNAKeys)e.KeyValue;

            lock (_syncKeys)
            {
                if (!_keysPressed.Contains(xnaKey))
                {
                    //not 100% accurate, but enough for GeometryFriends viewer, for a better implementation have to do a proper mapping of all the keys (check monogame implementation)
                    _keysPressed.Add(xnaKey);
                    _keysPressedTracker.Add(xnaKey);
                }
            }
        }

        private void GeometryFriendsViewer_KeyUp(object sender, KeyEventArgs e)
        {
            lock (_syncKeys)
            {
                _keysPressedTracker.Remove((XNAKeys)e.KeyValue);
            }
        }

        #region IInputDevice implementation
        public void ViewerExit()
        {
            //exit the viewer
            if (this != null)
            {
                if (this.InvokeRequired)
                {
                    if (!this.IsDisposed)
                    {
                        this.BeginInvoke((MethodInvoker)delegate()
                        {
                            this.Close();
                        });
                    }
                }
                else
                {
                    if (!this.IsDisposed)
                    {
                        this.Close();
                    }
                }
            }
        }

        public List<XNAKeys> GetPressedKeys()
        {
            List<XNAKeys> tmp;
            lock (_syncKeys)
            {
                tmp = _keysPressed;
                _keysPressed = new List<XNAKeys>();
                //directly add keys that are pressed
                foreach (XNAKeys item in _keysPressedTracker)
                {
                    _keysPressed.Add(item);
                }
            }
            return tmp;
        }

        public MouseState GetMouseState()
        {
            UpdateMouseState();
            return _mouseState;
        }

        private void UpdateMouseState()
        {
            if (this.InvokeRequired && !this.IsDisposed)
            {
                if (!this.IsDisposed)
                {
                    this.BeginInvoke((MethodInvoker)delegate()
                    {
                        MouseUpdate();
                    });
                }
            }
            else
            {
                if (!this.IsDisposed)
                {
                    MouseUpdate();
                }
            }
        }

        private void MouseUpdate()
        {
            // If we call the form client functions before the form has
            // been made visible it will cause the wrong window size to
            // be applied at startup.
            if (!this.Visible)
                return;

            var clientPos = this.PointToClient(Control.MousePosition);
            var withinClient = this.ClientRectangle.Contains(clientPos);
            var buttons = Control.MouseButtons;

            _mouseState.X = clientPos.X;
            _mouseState.Y = clientPos.Y;
            _mouseState.LeftButton = (buttons & MouseButtons.Left) == MouseButtons.Left ? XNAButtonState.Pressed : XNAButtonState.Released;
            _mouseState.MiddleButton = (buttons & MouseButtons.Middle) == MouseButtons.Middle ? XNAButtonState.Pressed : XNAButtonState.Released;
            _mouseState.RightButton = (buttons & MouseButtons.Right) == MouseButtons.Right ? XNAButtonState.Pressed : XNAButtonState.Released;
        }

        #endregion


        #region IGraphicsDevice implementation
        public void RepaintDevice(object sender, List<DrawingInsctruction> instructions, HashSet<float> priorities)
        {
            //prepare the new screen
            bool needsInvalidate = Redraw(instructions, priorities);
            //issue the panel repaint
            if (needsInvalidate){
                if (this.IsDisposed || this.drawingPanel.IsDisposed)
                    return;

                if (this.InvokeRequired)
                {
                    this.drawingPanel.BeginInvoke((MethodInvoker)delegate()
                    {
                        this.drawingPanel.Invalidate();
                    });
                }
                else
                {
                    this.drawingPanel.Invalidate();
                }
            }
        }

        public Vector2 MeasureString(string toMeasure, SpriteFont font)
        {
            SizeF size;
            lock (_syncDrawing)
            {
                size = _gamePanelBackbufferGraphics.MeasureString(toMeasure, font.BaseFont);
            }
            return new Vector2(size.Width, size.Height);
        }
        #endregion

        private bool Redraw(List<DrawingInsctruction> instructions, HashSet<float> priorities)
        {
            lock (_syncDrawing)
            {   
                if (_exiting)
                    return false;
                //we only create a bitmap in case we have some instruction to draw
                if (instructions.Count == 0)
                {
                    return false;
                }
                else
                {
                    //set anti aliasing properties
                    //_gamePanelBackbufferGraphics.SmoothingMode = SmoothingMode.AntiAlias;

                    RectangleInstruction rInstruction;
                    CircleInstruction cInstruction;
                    LineInstruction lInstruction;
                    TextureInstruction tInstruction;
                    StringInstruction sInstruction;
                    ClearInstruction clInstruction;
                    SetMatrixInstruction smInstruction;

                    MSMatrix initial, localTransform;

                    bool skipClearInstructions = false;

                    //sort priorities 
                    var orderedPriorities = priorities.OrderBy(x => x);

                    //apply screen resize
                    if (previousDPanelWidth != drawingPanel.Width || previousDPanelHeight != drawingPanel.Height)
                    {
                        float ratioW = (float) (drawingPanel.Width - 16) / 1280;
                        float ratioH = (float) (drawingPanel.Height - 37) / 800;

                        transformApplied = transformInitial.Clone();
                        transformApplied.Scale(ratioW, ratioH);
                        _gamePanelBackbufferGraphics.Transform = transformApplied;

                        previousDPanelWidth = drawingPanel.Width;
                        previousDPanelHeight = drawingPanel.Height;
                    }

                    //follow priorities order
                    foreach (int currPriority in orderedPriorities)
                    {
                        //paint all the instructions
                        foreach (DrawingInsctruction item in instructions)
                        {
                            //ensure draw order
                            if (item.HasPriority() && currPriority != item.Priority)
                                continue;

                            //apply the instruction
                            switch (item.Instruction)
                            {
                                case DrawingInsctruction.InstructionType.Rectangle:
                                    rInstruction = (RectangleInstruction)item;
                                    //TODO: apply color tint
                                    //TODO: border thickness without occupying drawing space
                                    //ensure draw order
                                    if (rInstruction.Rotation != 0)
                                    {
                                        initial = _gamePanelBackbufferGraphics.Transform;
                                        localTransform = _gamePanelBackbufferGraphics.Transform.Clone();
                                        localTransform.RotateAt(GeometryFriends.XNAStub.MathHelper.ToDegrees(rInstruction.Rotation), new PointF(rInstruction.Position.X + rInstruction.Width / 2, rInstruction.Position.Y + rInstruction.Height / 2));
                                        _gamePanelBackbufferGraphics.Transform = localTransform;
                                        DrawRectangle(rInstruction.FillColor.ToMSColor(), rInstruction.Position.X, rInstruction.Position.Y, rInstruction.Width, rInstruction.Height, rInstruction.BorderColor.ToMSColor(), rInstruction.BorderThickness);
                                        _gamePanelBackbufferGraphics.Transform = initial;
                                    }
                                    else
                                    {
                                        DrawRectangle(rInstruction.FillColor.ToMSColor(), rInstruction.Position.X, rInstruction.Position.Y, rInstruction.Width, rInstruction.Height, rInstruction.BorderColor.ToMSColor(), rInstruction.BorderThickness);
                                    }
                                    break;

                                case DrawingInsctruction.InstructionType.Circle:
                                    cInstruction = (CircleInstruction)item;
                                    //TODO: border thickness without occupying drawing space                                    
                                    if (cInstruction.Rotation != 0)
                                    {
                                        initial = _gamePanelBackbufferGraphics.Transform;
                                        localTransform = _gamePanelBackbufferGraphics.Transform.Clone();
                                        localTransform.RotateAt(GeometryFriends.XNAStub.MathHelper.ToDegrees(cInstruction.Rotation), new PointF(cInstruction.RotationPoint.X, cInstruction.RotationPoint.Y));
                                        _gamePanelBackbufferGraphics.Transform = localTransform;
                                        DrawCircle(cInstruction.FillColor.ToMSColor(), cInstruction.Position.X, cInstruction.Position.Y, cInstruction.Radius * 2, cInstruction.BorderColor.ToMSColor(), cInstruction.BorderThickness);
                                        _gamePanelBackbufferGraphics.Transform = initial;
                                    }
                                    else
                                    {
                                        DrawCircle(cInstruction.FillColor.ToMSColor(), cInstruction.Position.X, cInstruction.Position.Y, cInstruction.Radius * 2, cInstruction.BorderColor.ToMSColor(), cInstruction.BorderThickness);
                                    }
                                    break;

                                case DrawingInsctruction.InstructionType.Line:
                                    lInstruction = (LineInstruction)item;
                                    _gamePanelBackbufferGraphics.DrawLine(GetPenFromCache(lInstruction.LineColor.ToMSColor(), lInstruction.Thickness), lInstruction.GetStartPoint(), lInstruction.GetEndPoint());
                                    break;

                                case DrawingInsctruction.InstructionType.Texture:
                                    tInstruction = (TextureInstruction)item;
                                    Image toDraw = GetImageFromCache(tInstruction.Texture.Filepath);
                                    //TODO: tint the image
                                    //draw the image
                                    
                                    /*
                                    //optimize image drawing
                                    CompositingMode original = _gamePanelBackbufferGraphics.CompositingMode;                                    
                                    _gamePanelBackbufferGraphics.CompositingMode = CompositingMode.SourceCopy;
                                    _gamePanelBackbufferGraphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                                    _gamePanelBackbufferGraphics.DrawImage(toDraw, tInstruction.Position.X, tInstruction.Position.Y, tInstruction.Width, tInstruction.Height);                                    
                                    _gamePanelBackbufferGraphics.CompositingMode = original;*/

                                    //can be heavy in older computers
                                    _gamePanelBackbufferGraphics.DrawImage(toDraw, tInstruction.Position.X, tInstruction.Position.Y, tInstruction.Width, tInstruction.Height);
                                    break;

                                case DrawingInsctruction.InstructionType.String:
                                    sInstruction = (StringInstruction)item;
                                    _gamePanelBackbufferGraphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                                    //save the initial transform
                                    initial = _gamePanelBackbufferGraphics.Transform;
                                    localTransform = _gamePanelBackbufferGraphics.Transform.Clone();
                                    //translate must be applied on the matrix so that the scaling is properly applied
                                    localTransform.Translate(sInstruction.Position.X, sInstruction.Position.Y);
                                    //apply the scale                                
                                    localTransform.Scale(sInstruction.Scale, sInstruction.Scale);
                                    //apply the rotation
                                    localTransform.RotateAt(GeometryFriends.XNAStub.MathHelper.ToDegrees(sInstruction.Rotation), new PointF(sInstruction.Position.X, sInstruction.Position.Y));
                                    _gamePanelBackbufferGraphics.Transform = localTransform;
                                    //draw the text                                
                                    _gamePanelBackbufferGraphics.DrawString(sInstruction.Text, sInstruction.Font.BaseFont, GetBrushFromCache(sInstruction.TextColor.ToMSColor()), 0, 0);
                                    //TODO: add text styling
                                    _gamePanelBackbufferGraphics.Transform = initial;
                                    //RESET TRANSFORM
                                    break;

                                case DrawingInsctruction.InstructionType.Clear:
                                    if (skipClearInstructions)
                                        continue; //we already performed the clear instruction on the first priority pass

                                    clInstruction = (ClearInstruction)item;
                                    _gamePanelBackbufferGraphics.Clear(clInstruction.ClearColor.ToMSColor());
                                    break;

                                case DrawingInsctruction.InstructionType.SetMatrix:
                                    smInstruction = (SetMatrixInstruction)item;
                                    //apply translation
                                    localTransform = _gamePanelBackbufferGraphics.Transform.Clone();
                                    localTransform.Translate(smInstruction.TranslateX, smInstruction.TranslateY);
                                    //apply scale
                                    localTransform.Scale(smInstruction.Scale, smInstruction.Scale);
                                    //apply rotation
                                    localTransform.RotateAt(smInstruction.Rotation, new PointF(0, 0));
                                    //apply the matrix
                                    _gamePanelBackbufferGraphics.Transform = localTransform;
                                    break;

                                case DrawingInsctruction.InstructionType.ResetMatrix:
                                    //_gamePanelBackbufferGraphics.ResetTransform();
                                    _gamePanelBackbufferGraphics.Transform = transformApplied;
                                    break;

                                default:
                                    throw new NotImplementedException("Missing drawing instruction implementation.");
                            }
                        }
                        skipClearInstructions = true;
                   }
                    _gamePanelBackbufferGraphics.Transform = transformApplied;
                    return true;
                }
            }
        }

        private void DrawCircle(MSColor fillColor, float posx, float posy, float diameter, MSColor borderColor, int borderThickness)
        {   
            _gamePanelBackbufferGraphics.FillEllipse(GetBrushFromCache(fillColor), posx, posy, diameter, diameter);            
            _gamePanelBackbufferGraphics.DrawEllipse(GetPenFromCache(borderColor, borderThickness), posx, posy, diameter, diameter);
        }

        private void DrawRectangle(MSColor fillColor, float posx, float posy, int width, int height, MSColor borderColor, int borderThickness)
        {
            _gamePanelBackbufferGraphics.FillRectangle(GetBrushFromCache(fillColor), posx, posy, width, height);            
            _gamePanelBackbufferGraphics.DrawRectangle(GetPenFromCache(borderColor, borderThickness), posx, posy, width, height);
        }

        private void GeometryFriendsViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            lock (_syncDrawing)
            {
                _exiting = true;
            }
            //signal exit for the game also
            if (Platform != null)
            {
                Platform.DoInteractivePlatformExit(this);
            }
            //dispose all the resources
            foreach (Image item in _imagesCache.Values)
            {
                item.Dispose();
            }
            foreach (Pen item in _penCache.Values)
            {
                item.Dispose();
            }
            foreach (Brush item in _brushesCache.Values)
            {
                item.Dispose();
            }
            lock (_syncDrawing)
            {
                DisposeBufferData();
            }
        }

        private void DisposeBufferData()
        {
            if (_gamePanelBackbuffer != null)
            {
                _gamePanelBackbuffer.Dispose();
                _gamePanelBackbuffer = null;
            }
            if (_gamePanelBackbufferGraphics != null)
            {
                _gamePanelBackbufferGraphics.Dispose();
                _gamePanelBackbufferGraphics = null;
            }
        }

        private void GeometryFriendsViewer_Resize(object sender, EventArgs e)
        {
            lock (_syncDrawing)
            {
                DisposeBufferData();
                _gamePanelBackbuffer = new Bitmap(this.drawingPanel.Width, this.drawingPanel.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                _gamePanelBackbufferGraphics = Graphics.FromImage(_gamePanelBackbuffer);
            }
        }

        private void drawingPanel_MouseEnter(object sender, EventArgs e)
        {
            // Hide the cursor when the mouse enters the form.
            Cursor.Hide();
        }

        private void drawingPanel_MouseLeave(object sender, EventArgs e)
        {
            // Show the cursor when the mouse pointer leaves the form.
            Cursor.Show();
        }


        public Bitmap getScreenshot()
        {
            //Disposes Bitmap if there was one there already
            if (screenshot != null) screenshot.Dispose();
            lock (_syncDrawing)
            {
                //values in order to cut borders and extra space
                int border_width = 40 * _gamePanelBackbuffer.Width / 1296;
                int border_height = 40 * _gamePanelBackbuffer.Height / 837;
                int extra_spacex = 16;
                int extra_spacey = 37;
                //clones a selected area of the backbuffer into screenshot
                screenshot = _gamePanelBackbuffer.Clone(new Rectangle(border_width, border_height,
                    _gamePanelBackbuffer.Width - 2 * border_width - extra_spacex,
                    _gamePanelBackbuffer.Height - 2 * border_height - extra_spacey),
                    _gamePanelBackbuffer.PixelFormat);
            }
            return screenshot;
        }

    }
}
