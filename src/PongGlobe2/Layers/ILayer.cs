using System;
using System.Collections.Generic;
using System.Text;

namespace PongGlobe.Core
{
    //图层的基接口
    public interface ILayer
    {
        string Name { get; set; }

        string AliasName { get; set; }

        bool Visible { get; set; }
    }

    
}
