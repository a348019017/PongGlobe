// Copyright (c) Xenko contributors (https://xenko.com) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Collections.Generic;
using PongGlobe.Core;
using Veldrid;

namespace PongGlobe.Graphics
{
    /// <summary>
    /// ���ɱ���Ⱦ���ߣ���������Ѿ����棨���ݲ������棩����Ⱦ����
    /// </summary>
    public class MutablePipeline
    {
        private readonly GraphicsDevice graphicsDevice;
        //ʹ��Hashֵ������
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
            //���ó�ʼֵ 
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
                //������10����ɱ����Ⱦ����
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
