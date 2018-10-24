
using PongGlobe.Scene;
using System.Numerics;
using PongGlobe.Windows;
using System;
using System.Text;
using Xilium.CefGlue;
using PongGlobe.Scene.cef;
namespace GettingStarted2
{
    class Program
    {
        public static void Main(string[] args)
        {


            InitalCefRuntime();


            //string str = "您好";
            //var byte1= Encoding.UTF8.GetBytes(str);
            //Encoding.Default
            //Console.WriteLine("您好");
            VeldridStartupWindow window = new VeldridStartupWindow("Textured Cube");
            EarthApplication texturedCube = new EarthApplication(window);
            //RayCastedGlobe globe = new RayCastedGlobe(window);







            window.Run();
        }


        /// <summary>
        /// 初始化cef的一些参数
        /// </summary>
        public static void InitalCefRuntime()
        {
            CefRuntime.Load();
            var cefApp = new CefOSRApp();
            var cefMainArgs = new CefMainArgs(new string[] { });

            if (CefRuntime.ExecuteProcess(cefMainArgs, cefApp, IntPtr.Zero) != -1)
                Console.WriteLine("Could not start the secondary process.");

            var cefSettings = new CefSettings
            {
                //ExternalMessagePump = true,
                MultiThreadedMessageLoop = true,
                //SingleProcess = true,
                LogSeverity = CefLogSeverity.Verbose,
                LogFile = "cef.log",
                WindowlessRenderingEnabled = true,
                NoSandbox = true,
            };
            CefRuntime.Initialize(cefMainArgs, cefSettings, cefApp, IntPtr.Zero);
        }
    }
}
