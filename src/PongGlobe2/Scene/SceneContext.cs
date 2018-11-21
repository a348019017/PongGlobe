// Copyright (c) Xenko contributors (https://xenko.com) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Collections.Generic;
using PongGlobe.Core;
using Veldrid;
using System.Collections.Concurrent;

namespace PongGlobe.Scene
{
    /// <summary>
    /// 场景上下文对象，管理当前场景对象的上下文,类似于GraphicContext的作用
    /// </summary>
    public class SceneContext
    {
        public readonly ConcurrentQueue<IRender> Updaters=new ConcurrentQueue<IRender>();
        private Scene _scene=null;
        /// <summary>
        /// 当前上下文的CommandList
        /// </summary>
        public CommandList CommandList { get; set; }        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="scene"></param>
        /// <param name="commandList"></param>
        public SceneContext(GraphicsDevice graphicsDevice,Scene scene,CommandList commandList = null)
        {

            CommandList = commandList ?? graphicsDevice.ResourceFactory.CreateCommandList();        
        }
    }
}
