
using PongGlobe.Scene;
using System.Numerics;
using PongGlobe.Windows;

namespace GettingStarted2
{
    class Program
    {
        public static void Main(string[] args)
        {
            VeldridStartupWindow window = new VeldridStartupWindow("Textured Cube");
            EarthApplication texturedCube = new EarthApplication(window);
            //RayCastedGlobe globe = new RayCastedGlobe(window);
            window.Run();
        }
    }
}
