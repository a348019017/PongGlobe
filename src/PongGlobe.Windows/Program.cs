
using PongGlobe.Scene;
using System.Numerics;
using PongGlobe.Windows;

namespace GettingStarted2
{
    class Program
    {
        public static void Main(string[] args)
        {

            //计算NDC的Z值是否和view的1/z成反比,每次单独计算一个最大的Z值作裁剪
            var ree = Vector3.Normalize(new Vector3(0, 0, 0));

            //MyCameraController2 camera = new MyCameraController2(640, 360);
            //var source = new Vector3(320f, 180f, 2.5f);
            //var reailCoord = new Vector3(1, 0, -1f);
            //var resultXX = camera.Project(reailCoord, camera.ProjectionMatrix, camera.ViewMatrix, Matrix4x4.Identity);
            //var result1= camera.Unproject(source, camera.ProjectionMatrix, camera.ViewMatrix, Matrix4x4.Identity);
            //var source2 = new Vector3(320f, 180f, 6f);
            //var result2= camera.Unproject(source2, camera.ProjectionMatrix, camera.ViewMatrix, Matrix4x4.Identity);
            //var source3 = new Vector3(320f, 180f, 10);
            //var resul32 = camera.Unproject(source3, camera.ProjectionMatrix, camera.ViewMatrix, Matrix4x4.Identity);
            //camera.TestViewportMaxtrix();

            VeldridStartupWindow window = new VeldridStartupWindow("Textured Cube");
            EarthApplication texturedCube = new EarthApplication(window);
            //RayCastedGlobe globe = new RayCastedGlobe(window);
            window.Run();
        }
    }
}
