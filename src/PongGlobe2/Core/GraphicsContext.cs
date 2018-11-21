// Copyright (c) Xenko contributors (https://xenko.com) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Collections.Generic;
using PongGlobe.Core;
using Veldrid;
using System.Collections.Concurrent;

namespace PongGlobe.Graphics
{
    /// <summary>
    /// ͼ��������
    /// </summary>
    public class GraphicsContext
    {
        public readonly ConcurrentQueue<IRender> Updaters = new ConcurrentQueue<IRender>();
        /// <summary>
        /// ��ǰ�����ĵ�CommandList
        /// </summary>
        public CommandList CommandList { get; set; }
         
        public GraphicsContext(GraphicsDevice graphicsDevice,CommandList commandList = null)
        {
            CommandList = commandList ?? graphicsDevice.ResourceFactory.CreateCommandList();        
        }
    }
}
