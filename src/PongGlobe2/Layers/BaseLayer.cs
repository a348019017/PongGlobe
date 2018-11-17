using System;
using System.Collections.Generic;
using System.Text;

namespace PongGlobe.Core
{
   

    /// <summary>
    /// 虚基类图层
    /// </summary>
    public abstract class BaseLayer : ILayer
    {
        public string Name { get; set ; }
        public string AliasName { get; set ; }

        //修改图层显影性
        public virtual bool Visible { get ; set; }
    }
}
