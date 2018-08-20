using System;
using System.Collections.Generic;
using System.Text;
using PongGlobe.Core;
using Veldrid;
using System.Numerics;
using PongGlobe.Core.Extension;
using NetTopologySuite.IO.ShapeFile.Extended.Entities;
using System.Linq;

namespace PongGlobe.Layers
{
    /// <summary>
    /// 矢量图层类,根据点线面的不同这里还是区分出来
    /// </summary>
    public class VectorLayer
    {

    }

    /// <summary>
    /// 对单个要素进行渲染,独立渲染每个要素
    /// </summary>
    public class FeatureRender : IRender
    {
        /// <summary>
        /// feature转换成的mesh对象
        /// </summary>
        private Mesh<Vector3> _mesh = null;
        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _indexBuffer;
        //可能用到的纹理
        private Texture _surfaceTexture;
        private TextureView _surfaceTextureView;


        private Ellipsoid _shape = Ellipsoid.ScaledWgs842;
        /// <summary>
        /// 
        /// </summary>
        private IShapefileFeature _feature;
        /// <summary>
        /// 使用_fea构造FeatureRFender对象
        /// </summary>
        /// <param name="_fea"></param>
        public FeatureRender(IShapefileFeature _fea)
        {
            _feature = _fea;

        }

        /// <summary>
        /// 显示轮廓
        /// </summary>
        /// <returns></returns>
        public bool ShowWireFrame { get; set; }
        

        /// <summary>
        /// 创建相关资源
        /// </summary>
        /// <param name="gd"></param>
        /// <param name="factory"></param>
        public void CreateDeviceResources(GraphicsDevice gd, ResourceFactory factory)
        {
            List<Vector3> positions = new List<Vector3>();
            //记录其indices
            // List<ushort> indics = new List<ushort>();
            //填充顶点跟索引
            //详细流程，如果是投影坐标，将其转换成wgs84的经纬度坐标，再使用参考系计算出其真实的地理坐标         
            foreach (var coord in _feature.Geometry.Coordinates)
            {
                //将其转换成弧度制,自动贴地
                positions.Add(_shape.ToVector3(new Geodetic3D(MathExtension.ToRadius(coord.X), MathExtension.ToRadius(coord.Y))));
            }
            //三角网化
            var indices= EarClippingOnEllipsoid.Triangulate(positions);
            //三角细分,细分精度为1度
            _mesh = TriangleMeshSubdivision.Compute(positions, indices.ToArray(), Math.PI / 180);



        }

        /// <summary>
        /// 释放相关资源
        /// </summary>
        public void Dispose()
        {
            
        }

        public void Draw()
        {
            //throw new NotImplementedException();
        }

        public void Update()
        {
            //throw new NotImplementedException();
        }
    }


    /// <summary>
    /// 面图层矢量类
    /// </summary>
    public class PolygonVectorLayer : IRender
    {
        Ellipsoid _shape = Ellipsoid.ScaledWgs842;
        /// <summary>
        /// 将要素转换成Mesh对象
        /// </summary>
        private void FeaturesToMesh()
        {
            //读取shapefile的内容
            string path = "";
            var rader = new NetTopologySuite.IO.ShapeFile.Extended.ShapeDataReader( path);
            var result= rader.ReadByMBRFilter(rader.ShapefileBounds);
            //读取其中的点信息，转换成mesh对象
            var mesh = new PongGlobe.Core.Mesh<Vector3>();
            mesh.PrimitiveType = PrimitiveType.TriangleStrip;
            //在转换成
            List<Vector3> positions = new List<Vector3>();
            //记录其indices
            List<ushort> indics = new List<ushort>();
            //填充顶点跟索引
            //详细流程，如果是投影坐标，将其转换成wgs84的经纬度坐标，再使用参考系计算出其真实的地理坐标
            foreach (var item in result)
            {
                var geo = item.Geometry.Coordinates;
                //暂未考虑multipolygon的情况，也即是Hole polygon.
                foreach (var coord in geo)
                {
                    //将其转换成弧度制
                    positions.Add(_shape.ToVector3(new Geodetic3D(MathExtension.ToRadius(coord.X),MathExtension.ToRadius(coord.Y))));
                }               
            }
            //简化几何，如去除重复点，重置indices等操作

        }

        public void CreateDeviceResources(GraphicsDevice gd, ResourceFactory factory)
        {
            //throw new NotImplementedException();
        }

        public void Draw()
        {
            //throw new NotImplementedException();
        }

        public void Update()
        {
            //throw new NotImplementedException();
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
