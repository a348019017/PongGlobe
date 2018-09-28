using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using PongGlobe.Core;
using Veldrid;
using PongGlobe.Primitive;
using PongGlobe.Core.Util;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace PongGlobe.Renders
{
    /// <summary>
    /// 渲染一个cyliner
    /// </summary>
    public class CylinderRender : IRender
    {
        private Scene.Scene _scene;
        private Cylinder _cylinder;
        private Mesh<Vector3> _cylinderMesh;
        private GraphicsDevice _gd;
        private DeviceBuffer _VertexBuffer;
        private DeviceBuffer _IndicesBuffer;
        private ShaderSetDescription shaderSet;
        private DeviceBuffer _cylinderUBO;
        public CylinderRender(Scene.Scene scene, Cylinder cylinder)
        {
            _scene = scene;
            _cylinder = cylinder;
        }

        public void CreateDeviceResources(GraphicsDevice gd, ResourceFactory factory)
        {
            _gd = gd;
            _cylinderMesh = _cylinder.ToMesh();
            var result = _cylinderMesh.CreateGraphicResource(gd, factory);
            _VertexBuffer = result.Item1;
            _IndicesBuffer = result.Item2;
            ///Shader布局
            var curAss = this.GetType().Assembly;
            shaderSet = new ShaderSetDescription(
                new[]
                {
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float3)
                       )
                },
                new[]
                {
                   ResourceHelper.LoadEmbbedShader(ShaderStages.Vertex,"CylinderVS.spv",gd,curAss),
                   ResourceHelper.LoadEmbbedShader(ShaderStages.Fragment,"CylinderFS.spv",gd,curAss),
                   ResourceHelper.LoadEmbbedShader(ShaderStages.Geometry,"CylinderGS.spv",gd,curAss)
                });




        }

        public void Dispose()
        {

        }

        public void Draw(CommandList _cl)
        {

        }

        public void Update()
        {
            //throw new NotImplementedException();
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CylinderUBO
    {

    }


}
