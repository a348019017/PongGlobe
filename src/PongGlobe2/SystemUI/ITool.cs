using System;
using System.Collections.Generic;
using System.Text;

namespace PongGlobe.SystemUI
{
    /// <summary>
    /// 交互式动作的公共接口,这里的事件处理是全局的，因此没有额外的事件，也不便区分UI上或者场景上的事件
    /// </summary>
    public interface ITool
    {
        // 摘要: 
        //     鼠标的样式，这里考虑跨平台支持，不能单独使用windowsCursor
        int Cursor { get; }

        // 摘要: 
        //     Causes the tool to no longer be the active tool.
        bool Deactivate();

        //在此函数中处理相关事件，直接使用全局的事件处理类
        bool OnEvent();
               
        //
        // 摘要: 
        //     预留待定
        void Refresh();
    }
}
