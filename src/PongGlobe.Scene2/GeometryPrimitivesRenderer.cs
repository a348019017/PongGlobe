using System;
using System.Collections.Generic;
using System.Text;
using PongGlobe.Core;
using PongGlobe.Graphics.GeometricPrimitive;
using Veldrid;
using System.Collections.Concurrent;

namespace PongGlobe.Scene
{
    /// <summary>
    /// GeometryPrimitive渲染器
    /// </summary>
    public class GeometryPrimitivesRenderer : IRender<GeometricPrimitive>
    {


        private GraphicsDevice GraphicsDevice = null;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="style"> 样式</param>
        /// <param name="primitives"> 几何图元实体集合</param>
        public GeometryPrimitivesRenderer(GraphicsDevice graphicsDevice, GeometryPrimitiveStyle style = null,IEnumerable<GeometricPrimitive> primitives=null)
        {
            if (style == null)
                style = new GeometryPrimitiveStyle();
            Style = style;
            //将实体添加到线程安全的集合中，集合肯定是安全的但是对象不一定，使用Clone的方式相对安全但是
            if(primitives!=null)
                _geo = new ConcurrentBag<GeometricPrimitive>(primitives);
            //对Style添加INotify接口及时捕获参数的变化
            //Style.PropertyChanged += Style_PropertyChanged;
            
           
        }

        private void Style_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            
        }

        public GeometryPrimitiveStyle Style { get; set; } = new GeometryPrimitiveStyle();
        //线程安全的集合，可能阻塞渲染进程
        private ConcurrentBag<GeometricPrimitive> _geo=new ConcurrentBag<GeometricPrimitive>();

        //返回集合对象，方便添加数据
        public ConcurrentBag<GeometricPrimitive> GeometricPrimitives { get { return _geo; } }

        public void CreateDeviceResources(GraphicsDevice gd, ResourceFactory factory)
        {
            //throw new NotImplementedException();
            //_geo.
        }
        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public void Draw(CommandList _cl)
        {
            foreach (var item in GeometricPrimitives)
            {
                item.Draw(_cl);
            }
        }

        public void Update()
        {
            foreach (var item in GeometricPrimitives)
            {
                item.Update(GraphicsDevice);
            }
        }

        public void UpdateQueueOp()
        {
            
        }
    }
}
