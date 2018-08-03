using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphix.Physic;
using SharpDX.Windows;
using SharpDX;
using Graphix.Prototypes;

namespace Graphix.Rendering
{
    public class Renderer : IDisposable
    {
        public AnimationRuntime Animation { get; private set; }

        public Status CurrentStatus { get => Animation.CurrentStatus; set => Animation.CurrentStatus = value; }

        public bool ShowFps { get => FpsRender.Show; set => FpsRender.Show = value; }

        public bool VSync { get; set; }

        public List<FlatPrototype> Objects { get; private set; }

        public Dictionary<string, Status> Status { get; private set; }

        public event Action LoadAssets;

        public Renderer()
        {
            VSync = true;
            Objects = new List<FlatPrototype>();
            Status = new Dictionary<string, Status>();
            Animation = new AnimationRuntime();
            RessourceManager = new RessourceManager();
            Animation.SoundPlayer = new SoundPlayer();
        }

        public Status GetStatus(string fullName)
        {
            if (fullName == null) return null;
            var parts = fullName.Split('|');
            if (!Status.ContainsKey(parts[0])) return null;
            var st = Status[parts[0]];
            return st.Find(parts);
        }

        public void Import(PrototypeLoader prototypes)
        {
            var exp = new PrototypeExporter();
            exp.ImportFlatten(prototypes);
            foreach (var obj in exp.Objects)
            {
                Objects.Add(obj);
                LoadAnimations(obj);
            }
            foreach (var st in exp.Status)
                Status[st.Key] = st.Value;
        }

        void LoadAnimations(FlatPrototype prot)
        {
            foreach (var obj in prot.Container)
                LoadAnimations(obj);
            foreach (var anim in prot.Animations)
                Animation.Register(anim);
        }

        internal RenderForm Form { get; private set; }
        internal SharpRender SharpRender { get; private set; }
        internal FpsRenderer FpsRender { get; private set; }
        internal WaitingRenderer WaitingRender { get; private set; }
        internal ObjectRender ObjectRender { get; private set; }
        internal RessourceManager RessourceManager { get; private set; }

        Size2 screenSize;
        bool loadAssets = true;

        List<Task> preloads = new List<Task>();

        public void SetupForm(string title, int screenIndex)
        {
            Form = new RenderForm(title);
            Logger.Log("RenderForm created");
            if (screenIndex < 0) screenIndex += System.Windows.Forms.Screen.AllScreens.Length;
            
            Form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Logger.Log("Border style setted");
            Form.Bounds = System.Windows.Forms.Screen.AllScreens[screenIndex].Bounds;
            Logger.Log("Bounds setted");
            //Form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            //Form.IsFullscreen = true;
            //Form.AllowUserResizing = false;
            screenSize = new Size2(Form.Width, Form.Height);
            Form.MouseDown += Form_MouseDown;
            Form.KeyDown += Form_KeyDown;

            SharpRender = new SharpRender(Form);
            Logger.Log("Sharp Renderer created");
            FpsRender = new FpsRenderer();
            Logger.Log("Fps Renderer created");
            FpsRender.Initialize(SharpRender);
            Logger.Log("Fps Renderer initialized");

            WaitingRender = new WaitingRenderer(SharpRender);
            Logger.Log("Waiting Renderer created");

            ObjectRender = new ObjectRender(SharpRender);
            Logger.Log("Object Renderer created");
            RessourceManager.SetRenderTarget(SharpRender.d2dContext);
            Logger.Log("Ressource Manager created");
            Form.Bounds = System.Windows.Forms.Screen.AllScreens[screenIndex].Bounds;
            Logger.Log("Bounds setted");
        }

        public bool EnableSnapShot { get; set; }
        private void Form_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (EnableSnapShot && e.KeyCode == System.Windows.Forms.Keys.F7)
            {
                var pe = new PrototypeExporter();
                foreach (var st in Status)
                    pe.Status.Add(st.Key, st.Value);
                foreach (var obj in Objects)
                    pe.Objects.Add(obj);
                pe.SaveFlatDom("snapshot.xml");
            }
        }

        private void Form_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            new Task(() =>
            {
                var button = ClickButton.Unknown;
                switch (e.Button)
                {
                    case System.Windows.Forms.MouseButtons.Left: button = ClickButton.Left; break;
                    case System.Windows.Forms.MouseButtons.Middle: button = ClickButton.Middle; break;
                    case System.Windows.Forms.MouseButtons.Right: button = ClickButton.Right; break;
                }
                ObjectRender.Click(this, new Vector2(e.X, e.Y), screenSize, button);
            }).Start();
        }

        public void SetupBaseRenderer()
        {
            AddSpecialRenderer<Rect, RectRenderer>();
            AddSpecialRenderer<Ellipse, EllipseRenderer>();
            AddSpecialRenderer<Line, LineRenderer>();
            AddSpecialRenderer<Text, TextRenderer>();
            AddSpecialRenderer<Image, ImgRenderer>();
            AddSpecialRenderer<AnimImage, AnimImgRenderer>();
        }

        public void AddSpecialRenderer<Prototype, Renderer>() where Prototype : PrototypeBase where Renderer : SpecialRenderer, new()
        {
            var rend = new Renderer();
            rend.Ressources = RessourceManager;
            ObjectRender.Renderer[typeof(Prototype).FullName] = rend;
        }
        
        public void Run()
        {
            new Task(async () =>
            {
                LoadAssets?.Invoke();
                Task.WaitAll(preloads.ToArray());
                loadAssets = false;
                await Task.Delay(500);
                WaitingRender.Dispose();
                if (Status.ContainsKey("Main"))
                    CurrentStatus = Status["Main"];
            }).Start();
            Animation.StartTimer();
            RenderLoop.Run(Form, Render);
            Animation.StopTimer();
        }

        void Render()
        {
            var r = SharpRender;
            r.d2dContext.BeginDraw();
            r.d2dContext.Clear(SharpRender.c(System.Drawing.Color.Black));

            //r.d2dContext.FillEllipse(new SharpDX.Direct2D1.Ellipse(new SharpDX.Mathematics.Interop.RawVector2(300, 300), 50, 50), r.brush);

            if (loadAssets) WaitingRender.Render(r.d2dContext, screenSize);
            else ObjectRender.Render(this, screenSize);
            
            FpsRender.Render(r.d2dContext);

            r.d2dContext.EndDraw();
            r.swapChain.TryPresent(0, VSync ? SharpDX.DXGI.PresentFlags.None : SharpDX.DXGI.PresentFlags.DoNotWait);
        }

        public void Dispose()
        {
            Animation?.StopTimer();
            Animation?.SoundPlayer?.Dispose();
            RessourceManager?.Dispose();
            ObjectRender?.Dispose();
            WaitingRender?.Dispose();
            SharpRender?.Dispose();
            Form?.Dispose();
        }

        public void PreloadImage(string file)
        {
            var task = new Task(() =>
            {
                RessourceManager.GetBitmap(file);
            });
            preloads.Add(task);
            task.Start();
        }

        public void PreloadAnimImageDir(string dir)
        {
            foreach (var file in RessourceManager.GetAnimBitmapFiles(dir))
                PreloadImage(file);
        }
    }
}
