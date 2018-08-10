using System;
using System.Collections.Generic;
using System.Text;
using PongGlobe.Core;
using Veldrid;
using Veldrid.ImageSharp;
using System.Drawing;
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

        //此ClipMapLevel的二维范围,对应于地理坐标
        public RectangleF  Extent { get; set; }

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
            //创建一个渲染管道


                

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
            _normalTexture.Dispose();
            _normalTexture = null;
            __normalTextureView.Dispose();
            __normalTextureView = null;
            _imageryTexture.Dispose();
            _imageryTextureView = null;
        }
    }


    /// <summary>
    /// 由此图层统一管理ClipMapLevels的渲染，clip便不再单独完成渲染，在一个Commandlist内完成渲染
    /// </summary>
    public class TerrainLayer : IRender
    {
        private List<ClipMapLevel> _clipLevels = new List<ClipMapLevel>();

        TerrainLayer()
        {

            //创建10个ClimpLevel由于测试，范围从[-180,-90,0,90开始]
            var MaxExtent = new RectangleF(-180, -90, 180, 180);
            for (int i = 0; i < 10; i++)
            {
                var level = new ClipMapLevel();
                var width = 180 / (2 ^ i);
                level.Extent = new RectangleF(-90f-width/2,0-width/2,width,width);
            }
        }

        private CommandList _cmd = null;
        public void CreateDeviceResources(GraphicsDevice gd, ResourceFactory factory)
        {
            //对于512的瓦片来说，分解成为12个block的ring,分别绘制每个Block
            var blockM = RectangleTessellator.Compute(new RectangleF(0, 0, 128, 128), 128, 128);
            //中间的分解成为


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
