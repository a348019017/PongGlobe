using System;
using System.Collections.Generic;
using System.Text;
using PongGlobe.Core;
using Veldrid;
using Veldrid.ImageSharp;
namespace PongGlobe.Scene.Terrain
{
    /// <summary>
    /// 用于描述Geometry ClipMaps的每一个层级，目前的设计需要支持多个地形图层，当然绘制需要统一。
    /// 当有多个地形图层时，如有交叉，确定某个规则生成相应的值，空值则默认为0；设计成可渲染对象
    /// </summary>
    public class ClipMapLevel:IRender,IDisposable
    {
        //由ImageSharp创建的纹理对象，此纹理对象默认为512个宽度和高度,试用于1024*768分辨率的屏幕情况。
        public ImageSharpTexture HeightTexture;
        public ImageSharpTexture NormalTexture;
        public ImageSharpTexture ImageryTexture;

        //
        private Texture _heightTexture;
        private Texture _normalTexture;
        private Texture _imageryTexture;

        private TextureView _heightTextureView;
        private TextureView __normalTextureView;
        private TextureView _imageryTextureView;

        //此ClipMapLevel的二维范围
        private object Extent { get; set; }

        /// <summary>
        /// 滞后计算其AABB包围盒,用于视椎体裁切等操作
        /// </summary>
        private object AABB { get; set; }

        public void Draw()
        {
            //throw new NotImplementedException();
        }

        public void Update()
        {
            //throw new NotImplementedException();
        }

        public void CreateDeviceResources(GraphicsDevice gd, ResourceFactory factory)
        {
            //根据纹理对象创建相关设备资源
            if (HeightTexture != null)
            {
                _heightTexture = HeightTexture.CreateDeviceTexture(gd, factory);
                _heightTextureView = factory.CreateTextureView(_heightTexture);
            }
            if(NormalTexture!=null)
            {
                _normalTexture = NormalTexture.CreateDeviceTexture(gd, factory);
                __normalTextureView = factory.CreateTextureView(_normalTexture);
            }
            if (ImageryTexture != null)
            {
                _imageryTexture = ImageryTexture.CreateDeviceTexture(gd, factory);
                _imageryTextureView = factory.CreateTextureView(_imageryTexture);
            }

                

        }

        /// <summary>
        /// 释放相关资源
        /// </summary>
        public void Dispose()
        {
            _heightTexture.Dispose();
            _heightTexture = null;
            _heightTextureView.Dispose();
            _heightTextureView = null;
        }
    }


    /// <summary>
    /// 由此图层统一管理ClipMapLevels的渲染，clip便不再单独完成渲染，在一个Commandlist内完成渲染
    /// </summary>
    public class TerrainLayer : IRender
    {
        private CommandList _cmd = null;
        public void CreateDeviceResources(GraphicsDevice gd, ResourceFactory factory)
        {
            //创建一个默认的CommandList
            _cmd = factory.CreateCommandList();
            //创建多个渲染管道，同时开启多个绘制类似DrawPrimitive

        }

        public void Draw()
        {
            //throw new NotImplementedException();
        }

        //在绘制前对一些数据的更新操作，也可以放置到Draw中进行部分不太耗时的操作
        public void Update()
        {
            //添加对Camera的监听，在Camera发送变化时，后台更新每个Clips的内容，同时确定需要渲染的Clamps层数。例如当相机水平看去时，可能需要渲染很远处的山（地形），究竟多远就需要进行视椎体的裁切
            //同时使用View Frustum Culling提前裁切需要clamp和其中的Block，减少Gpu的Load

        }
    }
}
