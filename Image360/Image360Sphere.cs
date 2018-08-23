using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Urho.Resources;

namespace Image360
{
    public class Image360Sphere : Application
    {
        Scene scene;
        Node SphereNode;
        Node cameraNode;
        Camera camera;
        Octree octree;
        StaticModel sphere;
        float yaw;
        float pitch;
        float roll;
        const float Sensitivity = .5f;
        ImageDownloadManager imageDownloadManager = new ImageDownloadManager();
        public bool IsEnabled { get; set; } = true;
        [Preserve]
        public Image360Sphere(ApplicationOptions options = null) : base(null)
        {

        }

        static Image360Sphere()
        {
            UnhandledException += (s, e) =>
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
                e.Handled = true;
            };
        }

        protected override void Start()
        {
            base.Start();
            CreateScene();
            CreateSphere();
            SetupViewport();
        }

        private void CreateScene()
        {
            scene = new Scene();
            octree = scene.CreateComponent<Octree>();
           
            cameraNode = scene.CreateChild();
            cameraNode.LookAt(new Vector3(0, 1, 2), new Vector3(0, 1, 0));
            camera = cameraNode.CreateComponent<Camera>();

            cameraNode.Position = new Vector3(0, 0, 0);

            Node lightNode = cameraNode.CreateChild();
            lightNode.SetDirection(new Vector3(0, 1, -2));
            Light light = lightNode.CreateComponent<Light>();
            light.LightType = LightType.Directional;

            cameraNode.Rotation = new Quaternion(0.1f, 0, 0);

           
        }

        void SetupViewport()
        {
            var renderer = Renderer;
            var vp = new Viewport(Context, scene, camera, null);
            renderer.SetViewport(0, vp);
        }

        private void CreateSphere()
        {
            SphereNode = scene.CreateChild();
            SphereNode.Position = new Vector3(0, 0, 0);
            SphereNode.ScaleNode(2);
            sphere = SphereNode.CreateComponent<StaticModel>();
            sphere.Model = CoreAssets.Models.Sphere;
            var mat = Material.FromColor(Color.White);
            mat.CullMode = CullMode.None;
            sphere.SetMaterial(mat);
        }
        internal async Task SetImage(string imurl)
        {
            // clear image
            if (string.IsNullOrEmpty(imurl))
            {
                var mat = Material.FromColor(Color.White);
                mat.CullMode = CullMode.None;
                sphere.SetMaterial(mat);
                return;
            }

            if (imurl.ToLower().StartsWith("http"))
            {
                var localimagepath = await imageDownloadManager.GetLocalUrl(imurl);
                if (localimagepath != null)
                {
                    using (var fstream = File.OpenRead(localimagepath))
                    {
                        await LoadImage(fstream);
                    }
                }
            }
            else
            {

                var imageStream = Xamarin.Forms.Application.Current.GetType().GetTypeInfo().Assembly.GetManifestResourceStream(imurl);
                if (imageStream != null)
                    await LoadImage(imageStream);
            }


        }

        private async Task LoadImage(Stream imageStream)
        {
            byte[] imbytes = new byte[imageStream.Length];
            await imageStream.ReadAsync(imbytes, 0, (int)imageStream.Length).ConfigureAwait(true);

            Image im = new Image();
            im.Load(new MemoryBuffer(imbytes));
            Application.InvokeOnMain(() =>
            {
                var mat = Material.FromImage(im);
                mat.CullMode = CullMode.None;
                sphere.SetMaterial(mat);
            });
        }

        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);
            if (Input.NumTouches >= 1 && IsEnabled)
            {
                var touch = Input.GetTouch(0);
                yaw += Sensitivity * touch.Delta.X;
                pitch += Sensitivity * touch.Delta.Y;
                pitch = MathHelper.Clamp(pitch, -90, 90);
                roll = 0;
                cameraNode.Rotation = new Quaternion(-pitch, -yaw, roll);
            }
        }


    }
}
