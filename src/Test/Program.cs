using System;
using PongGlobe.Core;
using System.Numerics;
namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var testCoord = new Geodetic3D(MathF.PI/2, MathF.PI/4, 3);
            var Shape = Ellipsoid.ScaledWgs842;
            //平移到Z坐标，在开始旋转
            var translation = Matrix4x4.CreateTranslation(0, 0, Shape.Radii.Z+3);
            var rotate = Matrix4x4.CreateFromYawPitchRoll((float)testCoord.Longitude, -(float)testCoord.Latitude, 0);
            var transoformAll = rotate*translation ;
            

            var testCamera = new CameraInfo(0, 0, 3, MathF.PI/6, 0);
           
            var transform = Shape.geographicToCartesianTransform(testCoord);
            
            //获取当前camera的世界坐标            
            var cameraR = Vector3.Transform(Vector3.UnitX,transform);
            var rotateX = Matrix4x4.CreateFromAxisAngle(Vector3.UnitX, (float)testCamera.Tilt);
            //transform.
            Matrix4x4 transform2= Matrix4x4.CreateFromYawPitchRoll(0, (float)testCamera.Tilt, -(float)testCamera.Heading);
            
            var vector11= Vector3.Transform(Vector3.Zero, transform);
            var vector22 = Vector3.Transform(vector11, rotateX);
            var transofrmAll = transform* rotateX;
            //
            var translate = Matrix4x4.Decompose(transofrmAll, out Vector3 scale,out Quaternion quad,out Vector3 trans);

            //计算两个向量的点乘法
            var vectorA = new Vector3(0, 0, 4);
            var result = Vector3.Dot(vectorA, vector22);
            var angle = MathF.Acos(result/16);
        }
    }
}
