// Copyright (c) Xenko contributors (https://xenko.com) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Collections.Generic;
using PongGlobe.Core;
using Veldrid;

namespace PongGlobe.Graphics
{
    /// <summary>
    /// 不可变渲染管线，管理各种已经缓存（根据参数缓存）的渲染管线
    /// </summary>
    public class MutablePipeline
    {
        private readonly GraphicsDevice graphicsDevice;
        //使用Hash值作缓存
        private static readonly Dictionary<int, Pipeline> cache=new Dictionary<int, Pipeline>(10000);
        public GraphicsPipelineDescription State;

        /// <summary>
        /// Current compiled state.
        /// </summary>
        public Pipeline CurrentPipeLine;

        public MutablePipeline(GraphicsDevice graphicsDevice)
        {            
            this.graphicsDevice = graphicsDevice;         
            State = new GraphicsPipelineDescription();
            //设置初始值 
            State.Outputs = graphicsDevice.MainSwapchain.Framebuffer.OutputDescription;
            State.BlendState = BlendStateDescription.SingleOverrideBlend;
            State.DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual;
            State.RasterizerState = RasterizerStateDescription.Default;
            //State.ShaderSet
            //State.ResourceLayouts
            //State.ResourceBindingModel
        }

        /// <summary>
        /// Determine and updates <see cref="CurrentState"/> from <see cref="State"/>.
        /// </summary>
        public void Update()
        {
            // Hash current state
            var hashedState = State.GetHashCode();

            // Find existing PipelineState object
            Pipeline pipelineState;

            // TODO GRAPHICS REFACTOR We could avoid lock by adding them to a ThreadLocal (or RenderContext) and merge at end of frame
            lock (cache)
            {   
                //仅缓存10万个可编程渲染管线
                if (cache.Count > 100000) throw new Exception("over flow pipeline");
                if (!cache.TryGetValue(hashedState, out pipelineState))
                {
                    // Otherwise, instantiate it
                    // First, make an copy
                    //hashedState = new PipelineStateDescriptionWithHash(State.Clone());
                    cache.Add(hashedState, pipelineState = graphicsDevice.ResourceFactory.CreateGraphicsPipeline(State));
                }
            }
            CurrentPipeLine = pipelineState;
        }


    }
}
