using System;
using System.Windows.Forms;
using System.Threading;

namespace Graphix.DemoApp
{
    class Program
    {
#if DEBUG
        static bool Debug = true;
#else
        static bool Debug = false;
#endif

        /// <summary>
        /// Main program
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            try
            {
                //hook unhandled exceptions
                Application.ThreadException += Application_ThreadException;

                //create renderer for presenting
                using (var renderer = new Graphix.Rendering.Renderer())
                {
                    //setup renderer on screen with index 0
                    renderer.SetupForm("Graphix Demo", 0);

                    //setup rendering classes for basic objects
                    renderer.SetupBaseRenderer();

                    //set loader function of all assets
                    renderer.LoadAssets += () =>
                    {
                        //load a visual prototype
                        PrototypeLoader pl = new PrototypeLoader();
                        pl.Load(@"ui\demo.xml");
                        renderer.Import(pl);
                    };

                    //setup debug
                    if (Debug)
                    {
                        renderer.ShowFps = true;
                        renderer.VSync = false;
                    }
                    else
                    {
                        renderer.ShowFps = false;
                        renderer.VSync = true;
                    }

                    //start the renderer and display main window
                    renderer.Run();
                    //this method is running during the whole application lifetime

                    //cleanup
                    renderer.CurrentStatus = null;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show(string.Format("Exception: {0}\nin: {1}\n\nTrace:\n{2}",
                e.Exception.Message, e.Exception.Source, e.Exception.StackTrace),
                "Unhandled exception called", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
