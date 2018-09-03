using System;
using System.Collections.Generic;
using System.Text;
using PongGlobe.Core;
using Veldrid;
using System.Numerics;
using PongGlobe.Scene;

namespace PongGlobe.Renders
{
    /// <summary>
    /// 公共的渲染类,主要用于更新一些公共的UBO等参数
    /// </summary>
    public class ShareRender : IRender
    {
        /// <summary>
        /// 公共视图缓存
        /// </summary>
        private DeviceBuffer _projectionBuffer;
        private BaseUBO _ubo = new BaseUBO();
        private ICameraController _camera;
        private ResourceSet _projViewSet;
        private Ellipsoid Shape { get; set; }

        public ShareRender(Scene.Scene scene)
        {
            this._camera = scene.Camera;
            this.Shape = scene.Ellipsoid;
        }

        public void CreateDeviceResources(GraphicsDevice gd, ResourceFactory factory)
        {
            _projectionBuffer = factory.CreateBuffer(new BufferDescription(224, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
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


            ResourceLayout projViewLayout = factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Projection", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment|ShaderStages.Geometry)
                    ));

            _projViewSet = factory.CreateResourceSet(new ResourceSetDescription(
                projViewLayout,
                _projectionBuffer
                ));

            //保存一些公共资源
            ShareResource.ProjectionBuffer = _projectionBuffer;
            ShareResource.ProjectionResourceLoyout = projViewLayout;
            ShareResource.ProjectuibResourceSet = _projViewSet;
        }

        public void Dispose()
        {
           // throw new NotImplementedException();
        }

        public void Draw(CommandList _cl)
        {
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
            _ubo.ViewPort = ((MyCameraController2)_camera).ViewPort
;            _cl.UpdateBuffer(_projectionBuffer, 0, _ubo);
        }

        public void Update()
        {
            //throw new NotImplementedException();
        }
    }
}
