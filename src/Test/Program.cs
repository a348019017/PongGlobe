using System;
using PongGlobe.Core;
using System.Numerics;
namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
           var isRight=  EllipsoidTest.TestGeoToCartesianTransform();

            var testCoord = new Geodetic3D(MathF.PI/4, MathF.PI/4, 1);
            var Shape = Ellipsoid.ScaledWgs842;

            var testCoord2 = new Vector3(0, 0, 0);
            

            var transformZZZ = Shape.GeographicToCartesianTransform2(testCoord);

            var resultCoord = Vector3.Transform(testCoord2, transformZZZ);


            //var inverseTransformZZZ = Matrix4x4.Invert();


            //先绕X旋转XXX，再按Z旋转XXX，
            var rotateX1 = Matrix4x4.CreateFromAxisAngle(Vector3.UnitX, MathF.PI / 4);
            var rotateZ1 = Matrix4x4.CreateFromAxisAngle(Vector3.UnitZ, MathF.PI / 2);
            var transform3 = transformZZZ * rotateX1 * rotateZ1;
            //
            Matrix4x4 transform4 = Matrix4x4.CreateFromYawPitchRoll(0, MathF.PI / 4, MathF.PI / 2);
            var transform5 = transformZZZ * transform4;

            ////平移到Z坐标，在开始旋转
            //var translation = Matrix4x4.CreateTranslation(vectorCoord);
            //var rotate = Matrix4x4.CreateFromYawPitchRoll((float)testCoord.Longitude, -(float)testCoord.Latitude, 0);
            //var transformZZZ = rotate*translation ;


            //Matrix4x4 transform4 = Matrix4x4.CreateFromYawPitchRoll(0, MathF.PI / 4, MathF.PI / 2);
            //var transform5 = transoformAll * transform4;
            //var testCamera = new CameraInfo(0, 0, 3, MathF.PI/6, 0);          
            //var transform = Shape.geographicToCartesianTransform(testCoord); 
            ////获取当前camera的世界坐标            
            //var cameraR = Vector3.Transform(Vector3.UnitX,transform);
            //var rotateX = Matrix4x4.CreateFromAxisAngle(Vector3.UnitX, (float)testCamera.Tilt);
            ////transform.
            //Matrix4x4 transform2= Matrix4x4.CreateFromYawPitchRoll(0, (float)testCamera.Tilt, -(float)testCamera.Heading);

            //var vector11= Vector3.Transform(Vector3.Zero, transform);
            //var vector22 = Vector3.Transform(vector11, rotateX);
            //var transofrmAll = transform* rotateX;
            ////
            //var translate = Matrix4x4.Decompose(transofrmAll, out Vector3 scale,out Quaternion quad,out Vector3 trans);

            ////计算两个向量的点乘法
            //var vectorA = new Vector3(0, 0, 4);
            //var result = Vector3.Dot(vectorA, vector22);
            //var angle = MathF.Acos(result/16);
        }
    }
}
