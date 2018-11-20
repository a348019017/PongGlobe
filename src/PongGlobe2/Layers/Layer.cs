using System;
using System.Collections.Generic;
using System.Text;

namespace PongGlobe.Core
{
   

    /// <summary>
    /// 虚基类图层
    /// </summary>
    public abstract class Layer : ILayer
    {
        public string Name { get; set ; }
        public string AliasName { get; set ; }

        //修改图层显影性
        public virtual bool Visible { get ; set; }
        public double MinVisible { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public double MaxVisible { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool Enabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string LayerName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string LayerTitle { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public object Envelope => throw new NotImplementedException();
        public int SRID { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
