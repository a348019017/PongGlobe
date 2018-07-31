#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using Veldrid;
using AssetPrimitives;
using PongGlobe.Core;
using Veldrid.ImageSharp;
using System.Numerics;
using SampleBase;
namespace PongGlobe.Scene
{
    public  class RayCastedGlobe : SampleApplication, IDisposable
    {

        private Texture _surfaceTexture;
        private TextureView _surfaceTextureView;
        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _indexBuffer;      
       
        //变换矩阵的Buffer
        private DeviceBuffer _uboBuffer;
        private BaseUBO _uboBase;
        //uniform集合       
        private ResourceSet _projViewSet;
        //textureAndSample
        private ResourceSet _textureSet;
        //渲染管线
        private Pipeline _pipeline;
        //命令队列
        private CommandList _cl;
       
        private Mesh _mesh;
        private bool _useAverageDepth;

        /// <summary>
        /// 构造可渲染对象
        /// </summary>
        /// <param name="context"></param>
        /// <param name="factory"></param>
        public RayCastedGlobe(ApplicationWindow window):base(window)
        {        
            Shape = Ellipsoid.ScaledWgs84;   
            
        }
    
        public bool UseAverageDepth
        {
            get { return _useAverageDepth; }
            set 
            { 
                _useAverageDepth = value;               
            }
        }

        public Ellipsoid Shape
        {
            get { return _shape; }
            set
            {
                //_dirty = true;
                _shape = value;
            }
        }

     
        

        #region IDisposable Members

        public void Dispose()
        {
           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="embedName"></param>
        /// <returns></returns>
        private Shader LoadEmbbedShader(ShaderStages stage, string embedName)
        {
            byte[] shaderBytes = ReadEmbeddedAssetBytes(embedName);
            string entryPoint = stage == ShaderStages.Vertex ? "VS" : "FS";
            return GraphicsDevice.ResourceFactory.CreateShader(new ShaderDescription(stage, shaderBytes, entryPoint));
        }

        protected override void HandleWindowResize()
        {
            if (_controller == null) return;
            _controller.WindowResized((int)this.Window.Width, (int)this.Window.Height);
        }


        protected override void OnDeviceDestroyed()
        {
            base.OnDeviceDestroyed();
        }

        protected unsafe override void CreateResources(ResourceFactory factory)
        {
            //创建一个最大半径的Cube
            var boxMesh = BoxTessellator.Compute(Shape.Radii * 2f);
            //创建此mesh的相关资源
            _mesh = boxMesh;

            //创建texture和textureview
            var inPath = @"E:\swyy\Lib\veldrid-samples\assets\Earth.png";
            ImageSharpTexture inputImage = new ImageSharpTexture(inPath, false);
            _surfaceTexture = inputImage.CreateDeviceTexture(GraphicsDevice, factory);
            _surfaceTextureView = factory.CreateTextureView(_surfaceTexture);

            //创建一个顶点缓冲数据
            _vertexBuffer = factory.CreateBuffer(new BufferDescription((uint)boxMesh.Positions.Length * VertexPosition.SizeInBytes, BufferUsage.VertexBuffer));
            _indexBuffer = factory.CreateBuffer(new BufferDescription((uint)(sizeof(ushort) * boxMesh.Indices.Length), BufferUsage.IndexBuffer));
            _uboBuffer = factory.CreateBuffer(new BufferDescription(144, BufferUsage.UniformBuffer|BufferUsage.Dynamic));

            //提前更新raddisquard参数
            _uboBase = new BaseUBO();
            //提前更新关照模型等固定参数
            //更新光照模型
            var DiffuseIntensity = 0.65f;
            var SpecularIntensity = 0.25f;
            var AmbientIntensity = 0.10f;
            var Shininess = 12;
            var lightModel = new Vector4(
                DiffuseIntensity,
                SpecularIntensity,
                AmbientIntensity,
                Shininess);
            _uboBase.DiffuseSpecularAmbientShininess = lightModel;
            _uboBase.GlobeOneOverRadiiSquared = Shape.OneOverRadiiSquared;
            _uboBase.UseAverageDepth = false;
            //提前更新参数
            GraphicsDevice.UpdateBuffer(_uboBuffer, 0, _uboBase);
            //创建prj的资源布局作为地球的默认资源布局
            ResourceLayout projViewLayout = factory.CreateResourceLayout(
              new ResourceLayoutDescription(
                  new ResourceLayoutElementDescription("UBO", ResourceKind.UniformBuffer, ShaderStages.Vertex|ShaderStages.Fragment)                 
                  ));
            //创建prj的资源
            _projViewSet = factory.CreateResourceSet(new ResourceSetDescription(
             projViewLayout,
            _uboBuffer                     
            ));

            GraphicsDevice.UpdateBuffer(_vertexBuffer, 0, _mesh.Positions);          
            GraphicsDevice.UpdateBuffer(_indexBuffer, 0, _mesh.Indices);

            //创建texture和sampler资源布局
            ResourceLayout worldTextureLayout = factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            //创建textureset
            _textureSet = factory.CreateResourceSet(new ResourceSetDescription(
               worldTextureLayout,
              _surfaceTextureView,
              GraphicsDevice.Aniso4xSampler
              ));

            //创建shaderprograme,仅传入inWorldPosition
            ShaderSetDescription shaderSet = new ShaderSetDescription(
                new[]
                {
                    new VertexLayoutDescription(
                        new VertexElementDescription("inPosition", VertexElementSemantic.Position, VertexElementFormat.Float3)     )
                },
                new Shader[]
                {
                    LoadEmbbedShader(ShaderStages.Vertex,"GlobeVS.spv"),
                    LoadEmbbedShader(ShaderStages.Fragment,"GlobeFS.spv")
                });


            //创建渲染管线
            _pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerStateDescription.Default,
                PrimitiveTopology.TriangleList,
                shaderSet,
                new[] { projViewLayout, worldTextureLayout },
                MainSwapchain.Framebuffer.OutputDescription));

            //创建一个命令队列
            _cl = factory.CreateCommandList();

            //设置相机参数
            _camera.Position = new Vector3(5f, 0f, 0f);
            _camera.NearDistance = 0.001f;
            _camera.FarDistance = 100f;
            _camera.Pitch = 0f;
            _camera.Yaw = (float)Math.PI / 2;

        }

        protected override void Draw(float deltaSeconds)
        {

            var prj = _camera.ProjectionMatrix;          
            var view =
                _camera.ViewMatrix;          
            //生成一个不变的矩阵
            //Matrix4x4 rotation =
             //   Matrix4x4.Identity;

            //生成一个变换矩阵
            var prjTemp = prj * view;

            Matrix4x4 rotation =
                Matrix4x4.CreateFromAxisAngle(-Vector3.UnitY, 0);
            //开始更新命令
            _cl.Begin();
            //更新变换矩阵           
            Vector3 cameraEyeSquared = _camera.Position * _camera.Position;
            _uboBase.CameraEye = _camera.Position;
            _uboBase.CameraEyeSquared = cameraEyeSquared;
            _uboBase.CameraLightPosition = _camera.Position;
            _uboBase.ProjectionViewModel = prjTemp;
            _uboBase.UseAverageDepth = _useAverageDepth;
            //更新数据
            _cl.UpdateBuffer(_uboBuffer, 0, ref _uboBase);           
          
            _cl.SetFramebuffer(MainSwapchain.Framebuffer);
            _cl.ClearColorTarget(0, RgbaFloat.Black);
            _cl.ClearDepthStencil(1f);
            _cl.SetPipeline(_pipeline);
            _cl.SetVertexBuffer(0, _vertexBuffer);
            _cl.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            _cl.SetGraphicsResourceSet(0, _projViewSet);
            _cl.SetGraphicsResourceSet(1, _textureSet);
            _cl.DrawIndexed((uint)_mesh.Indices.Length, 1, 0, 0, 0);
            _controller.Render(GraphicsDevice, _cl);
            _cl.End();
            GraphicsDevice.SubmitCommands(_cl);
            GraphicsDevice.SwapBuffers(MainSwapchain);
            GraphicsDevice.WaitForIdle();            
        }

        #endregion

      

        private Ellipsoid _shape;
        
    }


    /// <summary>
    /// 可能变化的相关参数
    /// </summary>
    public struct BaseUBO
    {
        //变换矩阵
        public Matrix4x4 ProjectionViewModel;
        //相机位置
        public Vector3 CameraEye;
        //相机位置的平方
        public Vector3 CameraEyeSquared;
        //光线起始位置，暂等于相机位置
        public Vector3 CameraLightPosition;
        //关照模型的参数
        public Vector4 DiffuseSpecularAmbientShininess;
        //是否计算平均深度
        public bool UseAverageDepth;
        //固定常量
        public Vector3 GlobeOneOverRadiiSquared;
    }

   

    
}