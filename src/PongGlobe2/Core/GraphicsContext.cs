// Copyright (c) Xenko contributors (https://xenko.com) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Collections.Generic;
using PongGlobe.Core;
using Veldrid;

namespace PongGlobe.Graphics
{
    /// <summary>
    /// 
    /// </summary>
    public class GraphicsContext
    {
        /// <summary>
        /// 当前上下文的CommandList
        /// </summary>
        public CommandList CommandList { get; set; }
         
        public GraphicsContext(GraphicsDevice graphicsDevice,  CommandList commandList = null)
        {
            CommandList = commandList ?? graphicsDevice.ResourceFactory.CreateCommandList();        
        }
    }
}
