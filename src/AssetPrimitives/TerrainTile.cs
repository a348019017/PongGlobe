using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Drawing;
using Veldrid.ImageSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.ImageSharp.Processing.Transforms.Resamplers;
using Veldrid;

namespace AssetPrimitives
{
    /// <summary>
    /// 表示一个地形的瓦片，由png或者tif文件生成
    /// </summary>
    public class TerrainTile
    {
        /// <summary>
        /// 根据文件生成地形瓦片
        /// </summary>
        /// <param name="bitmapPath"></param>
        /// <returns></returns>
        public static TerrainTile FromBitmap(string bitmapPath)
        {
            var bitmap = Image.Load(bitmapPath);
            //仅读取其中一位作为高程
            if (bitmap == null)
            {
                throw new ArgumentNullException("bitmap");
            }

            float[] heights = new float[bitmap.Width * bitmap.Height];
            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;

            int k = 0;
            for (int j = bitmap.Height - 1; j >= 0; --j)
            {
                for (int i = 0; i < bitmap.Width; ++i)
                {
                    float height = (float)(bitmap[i, j].R / 255.0);
                    heights[k++] = height;
                    minHeight = Math.Min(height, minHeight);
                    maxHeight = Math.Max(height, maxHeight);
                }
            }

            return new TerrainTile(
                new RectangleF(0.5f, 0.5f, bitmap.Width - 1, bitmap.Height -1),
                new Vector2(bitmap.Width, bitmap.Height),
                heights, minHeight, maxHeight);
        }

        public TerrainTile(
            RectangleF extent,
            Vector2 resolution,
            float[] heights,
            float minimumHeight,
            float maximumHeight)
        {
            if (extent.Right <= extent.Left ||
                extent.Top <= extent.Bottom)
            {
                throw new ArgumentOutOfRangeException("extent");
            }

            if (resolution.X < 0 || resolution.Y < 0)
            {
                throw new ArgumentOutOfRangeException("resolution");
            }

            if (heights == null)
            {
                throw new ArgumentNullException("heights");
            }

            if (heights.Length != resolution.X * resolution.Y)
            {
                throw new ArgumentException("heights.Length != resolution.Width * resolution.Height");
            }

            if (minimumHeight > maximumHeight)
            {
                throw new ArgumentOutOfRangeException("minimumHeight", "minimumHeight > maximumHeight");
            }

            _extent = extent;
            _resolution = resolution;
            _heights = heights;
            _minimumHeight = minimumHeight;
            _maximumHeight = maximumHeight;
        }

        public RectangleF Extent
        {
            get { return _extent; }
        }

        public Vector2 Resolution
        {
            get { return _resolution; }
        }

        public float[] Heights
        {
            get { return _heights; }
        }

        public float MinimumHeight
        {
            get { return _minimumHeight; }
        }

        public float MaximumHeight
        {
            get { return _maximumHeight; }
        }

        private readonly RectangleF _extent;
        private readonly Vector2 _resolution;
        private readonly float[] _heights;
        private readonly float _minimumHeight;
        private readonly float _maximumHeight;
    }


    /// <summary>
    /// 地形瓦片的渲染类，处理对Device的资源创建CreateResource，Draw，Update等操作
    /// </summary>
    public class TerrainTileRender : IRender,IDisposable
    {
        private TerrainTile _tile = null;
        public TerrainTileRender(TerrainTile tile)
        {
            if (tile == null)
                throw new Exception("Not TerrainTile!");
            _tile = tile;
        }


        public void CreateDeviceResources(GraphicsDevice gd, ResourceFactory factory)
        {
            //创建mesh
            if (_tile == null)
                throw new Exception("Not TerrainTile!");
            

        }

        public void Draw()
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            throw new NotImplementedException();
        }

        //资源的释放
        public void Dispose()
        {
           
        }


    }

    
}
