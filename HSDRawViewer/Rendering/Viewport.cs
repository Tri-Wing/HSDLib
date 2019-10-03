﻿using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Windows.Forms;
using HSDRaw;
using HSDRaw.Common.Animation;

namespace HSDRawViewer.Rendering
{
    public class Viewport : Panel
    {
        private GroupBox _animationGroup;
        private Button _animationPlayButton;
        private TrackBar _animationTrackBar;

        private GLControl _glViewport;

        public int ViewportWidth => _glViewport.Width;
        public int ViewportHeight => _glViewport.Height;

        public Matrix4 Camera;

        public bool ReadyToRender { get; internal set; } = false;

        public Matrix4 Translation
        {
            get
            {
                return Matrix4.CreateTranslation(_translation);
            }
            set
            {
                _translation = value.ExtractTranslation();
                UpdateCamera();
            }
        }

        private Matrix4 Transform
        {
            get
            {
                return Matrix4.CreateRotationZ(_rotation.Z) * Matrix4.CreateRotationY(_rotation.Y) * Matrix4.CreateRotationX(_rotation.X);
            }
        }

        float PrevX = 0, PrevY = 0;

        public float X
        {
            get
            {
                return _translation.X;
            }
            set
            {
                _translation.X = value;
                UpdateCamera();
            }
        }
        public float Y
        {
            get
            {
                return _translation.Y;
            }
            set
            {
                _translation.Y = value;
                UpdateCamera();
            }
        }
        public float Z
        {
            get
            {
                return _translation.Z;
            }
            set
            {
                _translation.Z = value;
                UpdateCamera();
            }
        }
        private Vector3 _translation = Vector3.Zero;

        public float XRotation
        {
            get
            {
                return _rotation.X;
            }
            set
            {
                _rotation.X = value;
                UpdateCamera();
            }
        }
        public float YRotation
        {
            get
            {
                return _rotation.Y;
            }
            set
            {
                _rotation.Y = value;
                UpdateCamera();
            }
        }
        public float ZRotation
        {
            get
            {
                return _rotation.Z;
            }
            set
            {
                _rotation.Z = value;
                UpdateCamera();
            }
        }
        private Vector3 _rotation = Vector3.Zero;

        private Matrix4 Perspective
        {
            get
            {
                return Matrix4.CreatePerspectiveFieldOfView(1f, Width / (float)Height, 0.1f, 10000);
            }
        }

        private Vector3 _defaultTranslation = new Vector3(0, -50, -100);
        private Vector3 _defaultRotation = new Vector3(0, 0, 0);

        private Renderer _renderer { get; set; }
        public HSDAccessor SelectedAccessor
        {
            internal get { return _selectedAccessor; }
            set {
                if (value is HSD_FigaTree tree)
                    _renderer.SetFigaTree(tree);
                else
                if (value is HSD_AnimJoint joint)
                    _renderer.SetAnimJoint(joint);
                else
                {
                    _renderer.SetFigaTree(null);
                    _selectedAccessor = value;
                }
            }
        }

        private HSDAccessor _selectedAccessor { get; set; }


        // Animation Track
        public static int AnimSpeed = 1;
        public static int MaxFrame { get; set; } = 0;
        public static int Frame { get; set; } = 0;
        public static bool EnableAnimationTrack
        {
            get => _enableAnimationTrack;
                set
            {
                _enableAnimationTrack = value;
            }
        }
        private static bool _enableAnimationTrack = false;

        private int FrameTimer = 0;
        private bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                _isPlaying = value;

                if (IsPlaying)
                    _animationPlayButton.Text = "Pause";
                else
                    _animationPlayButton.Text = "Play";
            }
        }
        private bool _isPlaying = false;
        
        public Viewport()
        {
            _glViewport = new GLControl(new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8, 16));
            _glViewport.Load += Viewport_Loaded;
            _glViewport.Resize += Viewport_Resize;
            _glViewport.MouseMove += Viewport_MouseMove;
            _glViewport.KeyPress += Viewport_KeyDown;
            _glViewport.Paint += Render;
            _glViewport.AutoSize = true;
            _glViewport.Dock = DockStyle.Fill;

            _renderer = new Renderer();

            _animationGroup = new GroupBox();
            _animationGroup.Visible = false;
            _animationGroup.Text = "Animation Track";
            _animationGroup.Dock = DockStyle.Bottom;

            _animationPlayButton = new Button();
            _animationPlayButton.Text = "Play";
            _animationPlayButton.Click += (sender, args) =>
            {
                IsPlaying = !IsPlaying;
            };
            _animationPlayButton.Dock = DockStyle.Fill;

            _animationTrackBar = new TrackBar();
            _animationTrackBar.Dock = DockStyle.Top;
            //_animationTrackBar.DataBindings.Add(new Binding("Value", this, "Frame"));
            //DataBindings.Add("Frame", _animationTrackBar, "Value");

            _animationGroup.Controls.Add(_animationPlayButton);
            _animationGroup.Controls.Add(_animationTrackBar);

            ClearControls();

            Application.Idle += RenderLoop;
        }

        public void RenderLoop(object sender, EventArgs args)
        {
            if (ReadyToRender && EnableAnimationTrack)
            {
                _glViewport.Invalidate();
            }
        }

        public void ClearControls()
        {
            Controls.Clear();
            Frame = 0;
            IsPlaying = false;
            EnableAnimationTrack = false;
            Controls.Add(_glViewport);
            Controls.Add(_animationGroup);
        }

        private void UpdateCamera()
        {
            Camera = Transform * Translation * Perspective;
        }
    
        public void Render(object sender, EventArgs args)
        {
            if (!_animationGroup.Visible && EnableAnimationTrack)
            {
                _animationGroup.Visible = true;
                _animationTrackBar.Maximum = MaxFrame;
                _animationTrackBar.Minimum = 0;
            }

            if (_animationGroup.Visible && !EnableAnimationTrack)
                _animationGroup.Visible = false;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref Camera);

            if (IsPlaying)
            {
                FrameTimer++;
                if (FrameTimer > AnimSpeed)
                {
                    IncrementFrame();
                    FrameTimer = 0;
                }
            }

            // rendering here
            if (SelectedAccessor != null)
                _renderer.Render(Camera, SelectedAccessor, this);
            
            _glViewport.SwapBuffers();
        }

        public void AddToolStrip(ToolStrip t)
        {
            Controls.Add(t);
            _glViewport.BringToFront();
        }

        public static void IncrementFrame()
        {
            Frame++;
            if (Frame >= MaxFrame)
                Frame = 0;
            if (Frame < 0)
                Frame = MaxFrame - 1;
        }

        public static void DecrementFrame()
        {
            Frame--;
            if (Frame >= MaxFrame)
                Frame = 0;
            if (Frame < 0)
                Frame = MaxFrame - 1;
        }
        
        private void Viewport_Resize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, _glViewport.Width, _glViewport.Height);
            UpdateCamera();
            _glViewport.Invalidate();
        }

        private void Viewport_Loaded(object sender, EventArgs args)
        {
            GL.ClearColor(Color.DarkSlateGray);
            Translation = Matrix4.CreateTranslation(_defaultTranslation);
            ReadyToRender = true;
        }
        
        private void Viewport_MouseMove(object sender, MouseEventArgs e)
        {
            float speed = 1 / Vector3.TransformPosition(Vector3.Zero, Camera).LengthFast;
            speed = (1 - speed) * 1;
            if (e.Button == MouseButtons.Left)
            {
                YRotation -= (PrevX - e.X) / 50f;
                XRotation -= (PrevY - e.Y) / 50f;
            }
            if (e.Button == MouseButtons.Right)
            {
                X -= (PrevX - e.X) * speed;
                Y += (PrevY - e.Y) * speed;
            }
            PrevX = e.X;
            PrevY = e.Y;

            _glViewport.Invalidate();
        }

        public void Viewport_KeyDown(object sender, System.Windows.Forms.KeyPressEventArgs args)
        {
            if (args.KeyChar == 'w')
                Z += 5f;

            if (args.KeyChar == 's')
                Z -= 5f;

            _glViewport.Invalidate();
        }

    }
}
