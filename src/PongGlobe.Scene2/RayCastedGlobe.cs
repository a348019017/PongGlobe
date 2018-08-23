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
using PongGlobe.Scene;
namespace PongGlobe.Renders
{
    /// <summary>
    /// 基于RayCast的globe渲染类，可独立渲染
    /// </summary>
    public class RayCastedGlobe : IRender
    {      
        private VertexPosition[] _vertices;
        private ushort[] _indices;
              
        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _indexBuffer;
        private Texture _surfaceTexture;
        private TextureView _surfaceTextureView;
        private Pipeline _pipeline;
        private ResourceSet _worldTextureSet;
             
        public Ellipsoid Shape { get; set; }
       
        private GraphicsDevice GraphicsDevice;

        

        /// <summary>
        /// 通过当前的场景信息够着渲染对象
        /// </summary>
        /// <param name="scene"></param>
        public RayCastedGlobe(Scene.Scene scene)
        {
            this.Shape = scene.Ellipsoid;
            

        }

        public void CreateDeviceResources(GraphicsDevice gd, ResourceFactory factory)
        {
            GraphicsDevice = gd;
            ///创建一个椭球体的外包盒
            var mesh = PongGlobe.Core.BoxTessellator.Compute(2 * Shape.Radii);
            _vertices = mesh.Positions;
            _indices = mesh.Indices;

            
            // _viewBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            //_worldBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));

            _vertexBuffer = factory.CreateBuffer(new BufferDescription((uint)(VertexPositionColorTexture.SizeInBytes * _vertices.Length), BufferUsage.VertexBuffer));
            gd.UpdateBuffer(_vertexBuffer, 0, _vertices);

            _indexBuffer = factory.CreateBuffer(new BufferDescription((uint)(sizeof(ushort) * _indices.Length), BufferUsage.IndexBuffer));
            gd.UpdateBuffer(_indexBuffer, 0, _indices);
           

            var inPath = @"E:\swyy\Lib\veldrid-samples\assets\NE2_50M_SR_W_4096.jpg";
            ImageSharpTexture inputImage = new ImageSharpTexture(inPath, false);
            _surfaceTexture = inputImage.CreateDeviceTexture(gd, factory);

            _surfaceTextureView = factory.CreateTextureView(_surfaceTexture);
            //_surfaceTexture = _stoneTexData.CreateDeviceTexture(GraphicsDevice, ResourceFactory,TextureUsage.Storage);
            //_surfaceTextureView = factory.CreateTextureView(_surfaceTexture);
            var curAssembly = this.GetType().Assembly;
            ShaderSetDescription shaderSet = new ShaderSetDescription(
                new[]
                {
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float3))
                },
                new[]
                {
                   //嵌入shader至程序集中，为方便部署
                   ResourceHelper.LoadEmbbedShader(ShaderStages.Vertex,"GlobeVS.spv",gd,curAssembly),
                   ResourceHelper.LoadEmbbedShader(ShaderStages.Fragment,"GlobeFS.spv",gd,curAssembly)
                });

            

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
                new[] { ShareResource.ProjectionResourceLoyout, worldTextureLayout },
                gd.MainSwapchain.Framebuffer.OutputDescription));

            

            _worldTextureSet = factory.CreateResourceSet(new ResourceSetDescription(
                worldTextureLayout,
                _surfaceTextureView,
                gd.Aniso4xSampler));

            
            //_cl = factory.CreateCommandList();
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
        public void Draw(CommandList _cl)
        {                                
            _cl.SetPipeline(_pipeline);
            _cl.SetVertexBuffer(0, _vertexBuffer);
            _cl.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            _cl.SetGraphicsResourceSet(0, ShareResource.ProjectuibResourceSet);
            _cl.SetGraphicsResourceSet(1, _worldTextureSet);
            _cl.DrawIndexed((uint)_indices.Length, 1, 0, 0, 0);                      
        }

        public void Update()
        {
           
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
