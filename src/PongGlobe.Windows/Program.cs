
using PongGlobe.Scene;
using System.Numerics;
using PongGlobe.Windows;
using System;
using System.Text;
namespace GettingStarted2
{
    class Program
    {
        public static void Main(string[] args)
        {
            //string str = "您好";
            //var byte1= Encoding.UTF8.GetBytes(str);
            //Encoding.Default
            //Console.WriteLine("您好");
            VeldridStartupWindow window = new VeldridStartupWindow("Textured Cube");
            EarthApplication texturedCube = new EarthApplication(window);
            //RayCastedGlobe globe = new RayCastedGlobe(window);
            window.Run();
        }
    }
}
