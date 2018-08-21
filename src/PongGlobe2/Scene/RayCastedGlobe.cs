using System;
using System.Collections.Generic;
using System.Text;
using PongGlobe.Core;
using Veldrid;
using System.ComponentModel;
using System.Runtime.InteropServices;
using PongGlobe.Core.Util;
using System.Numerics;
using Veldrid.ImageSharp;

namespace PongGlobe.Scene
{
    /// <summary>
    /// 基于RayCast的globe渲染类，可独立渲染
    /// </summary>
    public class RayCastedGlobe : IRender
    {      
        private VertexPosition[] _vertices;
        private ushort[] _indices;
        private DeviceBuffer _projectionBuffer;
        //private DeviceBuffer _viewBuffer;
        //private DeviceBuffer _worldBuffer;
        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _indexBuffer;
        private CommandList _cl;
        private Texture _surfaceTexture;
        private TextureView _surfaceTextureView;
        private Pipeline _pipeline;
        private ResourceSet _projViewSet;
        private ResourceSet _worldTextureSet;
        private float _ticks;
        private BaseUBO _ubo = new BaseUBO();
        public Ellipsoid Shape { get; set; }
        private ICameraController _camera;
        private GraphicsDevice GraphicsDevice;

        /// <summary>
        /// 使用图形设备参数构造可渲染对象
        /// </summary>
        /// <param name="gd"></param>
        public RayCastedGlobe(GraphicsDevice gd)
        {
            GraphicsDevice = gd;
        }

        /// <summary>
        /// 通过当前的场景信息够着渲染对象
        /// </summary>
        /// <param name="scene"></param>
        public RayCastedGlobe(Scene scene)
        {
            this.Shape = scene.Ellipsoid;
            this._camera = scene.Camera;

        }

        public void CreateDeviceResources(GraphicsDevice gd, ResourceFactory factory)
        {
            ///创建一个椭球体的外包盒
            var mesh = PongGlobe.Core.BoxTessellator.Compute(2 * Shape.Radii);
            _vertices = mesh.Positions;
            _indices = mesh.Indices;

            _projectionBuffer = factory.CreateBuffer(new BufferDescription(208, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            // _viewBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            //_worldBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));

            _vertexBuffer = factory.CreateBuffer(new BufferDescription((uint)(VertexPositionColorTexture.SizeInBytes * 100000), BufferUsage.VertexBuffer));
            gd.UpdateBuffer(_vertexBuffer, 0, _vertices);

            _indexBuffer = factory.CreateBuffer(new BufferDescription(sizeof(ushort) * 400000, BufferUsage.IndexBuffer));

            gd.UpdateBuffer(_indexBuffer, 0, _indices);
            var DiffuseIntensity = 0.65f;
            var SpecularIntensity = 0.25f;
            var AmbientIntensity = 0.10f;
            var Shininess = 12;
            var lightModel = new Vector4(
                DiffuseIntensity,
                SpecularIntensity,
                AmbientIntensity,
                Shininess);
            _ubo.DiffuseSpecularAmbientShininess = lightModel;
            _ubo.GlobeOneOverRadiiSquared = Shape.OneOverRadiiSquared;
            //_ubo.UseAverageDepth = false;
            //提前更新参数
            gd.UpdateBuffer(_projectionBuffer, 0, _ubo);


            var inPath = @"E:\swyy\Lib\veldrid-samples\assets\NE2_50M_SR_W_4096.jpg";
            ImageSharpTexture inputImage = new ImageSharpTexture(inPath, false);
            _surfaceTexture = inputImage.CreateDeviceTexture(gd, factory);

            _surfaceTextureView = factory.CreateTextureView(_surfaceTexture);
            //_surfaceTexture = _stoneTexData.CreateDeviceTexture(GraphicsDevice, ResourceFactory,TextureUsage.Storage);
            //_surfaceTextureView = factory.CreateTextureView(_surfaceTexture);

            ShaderSetDescription shaderSet = new ShaderSetDescription(
                new[]
                {
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float3))
                },
                new[]
                {
                   ResourceHelper.LoadEmbbedShader(ShaderStages.Vertex,"GlobeVS.spv",gd),
                   ResourceHelper.LoadEmbbedShader(ShaderStages.Fragment,"GlobeFS.spv",gd)
                });

            ResourceLayout projViewLayout = factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Projection", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)

                    ));

            ResourceLayout worldTextureLayout = factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            var rd = RasterizerStateDescription.Default;
            rd.CullMode = FaceCullMode.None;
            rd.FillMode = PolygonFillMode.Solid;
            //rd.c
            //rd.FillMode = PolygonFillMode.Wireframe;
            _pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                rd,
                PrimitiveTopology.TriangleList,
                shaderSet,
                new[] { projViewLayout, worldTextureLayout },
                gd.MainSwapchain.Framebuffer.OutputDescription));

            _projViewSet = factory.CreateResourceSet(new ResourceSetDescription(
                projViewLayout,
                _projectionBuffer
                ));


            _worldTextureSet = factory.CreateResourceSet(new ResourceSetDescription(
                worldTextureLayout,
                _surfaceTextureView,
                gd.Aniso4xSampler));

            _cl = factory.CreateCommandList();
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public void Draw()
        {
            _cl.Begin();
            //投影矩阵
            var prj = _camera.ProjectionMatrix;
            var view =
                _camera.ViewMatrix;          
            //这里矩阵的定义和后者是有区别的，numberic中是行列，glsl中是列行，因此这里需要反向计算
            //            < pre >
            // *m[offset + 0] m[offset + 4] m[offset + 8] m[offset + 12]
            //* m[offset + 1] m[offset + 5] m[offset + 9] m[offset + 13]
            //* m[offset + 2] m[offset + 6] m[offset + 10] m[offset + 14]
            //* m[offset + 3] m[offset + 7] m[offset + 11] m[offset + 15] </ pre >

            //glsl是列主序，C#是行主序，虽然有所差异，但是并不需要装置，glsl中的第一行实际上就是传入矩阵的第一列，此列刚好能参与计算并返回正常值。
            //设置视点位置为2,2,2 ,target 为在0.2,0.2,0
            var eyePosition = _camera.Position;

            _ubo.prj = view * prj;
            _ubo.CameraEye = eyePosition;
            _ubo.CameraEyeSquared = eyePosition * eyePosition;
            _ubo.CameraLightPosition = eyePosition;

            _cl.UpdateBuffer(_projectionBuffer, 0, _ubo);

            _cl.SetFramebuffer(GraphicsDevice.MainSwapchain.Framebuffer);
            _cl.ClearColorTarget(0, RgbaFloat.Black);
            _cl.ClearDepthStencil(1f);

            _cl.SetPipeline(_pipeline);
            _cl.SetVertexBuffer(0, _vertexBuffer);
            _cl.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            _cl.SetGraphicsResourceSet(0, _projViewSet);
            _cl.SetGraphicsResourceSet(1, _worldTextureSet);
            _cl.DrawIndexed((uint)_indices.Length, 1, 0, 0, 0);
            //_controller.Render(GraphicsDevice, _cl);
            _cl.End();
            GraphicsDevice.SubmitCommands(_cl);

            GraphicsDevice.SwapBuffers(GraphicsDevice.MainSwapchain);
            GraphicsDevice.WaitForIdle();
        }

        public void Update()
        {
            //throw new NotImplementedException();
        }
    }


    /// <summary>
    /// 可能变化的相关参数
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BaseUBO
    {
        public Matrix4x4 prj;

        //相机位置的平方
        public Vector3 CameraEyeSquared;

        public float spa1;

        //相机位置
        public Vector3 CameraEye;

        public float spa2;

        //光线起始位置，暂等于相机位置
        public Vector3 CameraLightPosition;

        public float spa3;

        //关照模型的参数
        public Vector4 DiffuseSpecularAmbientShininess;

        //固定常量
        public Vector3 GlobeOneOverRadiiSquared;

        public float spa4;

    }
}
