using AssetPrimitives;
using SampleBase;
using System.Numerics;
using Veldrid;
using System.Collections.Generic;
using System;
using System.IO;
using Veldrid.ImageSharp;
using System.Runtime.InteropServices;
using ImGuiNET;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace GettingStarted2.GISEngine
{
    //测试图层,尝试Earth加载TMS图层
    /// <summary>
    /// 主要思想是动态修改纹理，每次鼠标运动之时，从TileCache中读取所需范围的瓦片数据，拼接成一张大纹理存放在纹理内存中。
    /// cesium或者osgearth都给予一个视野内的小范围来进行更新而不是完整的View区域，因此一张显示分辨率左右的
    /// 
    /// </summary>
    public class TileLayer : ILayer
    {
        public void Draw(GraphicsDevice g)
        {
            //throw new NotImplementedException();
            //stackalloc 
        }

        public void Update(GraphicsDevice g, IEarthView view)
        {
            //throw new NotImplementedException();
        }
    }

    public interface ILayer
    {
        //更新资源
        void Update(GraphicsDevice g,IEarthView view);

        //实时绘制,也可名为Render
        void Draw(GraphicsDevice g);
        

    }

    /// <summary>
    /// 描述抽象地球视图对象，如读取视图的经纬度范围，3d范围，2d范围（低维度向下兼容）等等，起始2d范围便能直接兼容现有的2dLayer对象的处理方式
    /// </summary>
    public interface IEarthView
    {
        //当前EarthView下的二维视野
        BruTile.Extent Extent { get; set; }
    }
}
