// Copyright (c) Xenko contributors (https://xenko.com) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
//
// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

using PongGlobe.Core;
//using PongGlobe.Rendering;
using Veldrid;
using Veldrid.Vk;
using PongGlobe.Graphics;


namespace PongGlobe.Graphics.GeometricPrimitive
{
    /// <summary>
    /// A geometric primitive used to draw a simple model built from a set of vertices and indices.
    /// </summary>
    public class GeometricPrimitive<T> : ComponentBase where T : struct ,IVertex
    {
        /// <summary>
        /// The pipeline state.
        /// </summary>
        public readonly MutablePipeline PipelineState;

        /// <summary>
        /// The index buffer used by this geometric primitive.
        /// </summary>
        public readonly DeviceBuffer IndexBuffer;

        /// <summary>
        /// The vertex buffer used by this geometric primitive.
        /// </summary>
        public readonly DeviceBuffer VertexBuffer;

        /// <summary>
        /// The default graphics device.
        /// </summary>
        protected readonly GraphicsDevice GraphicsDevice;

        /// <summary>
        /// 顶点资源布局，根据T来进行布局,其它参数相对固定因此不列出
        /// </summary>
        protected readonly VertexLayoutDescription VertexBufferBinding;

        /// <summary>
        /// True if the index buffer is a 32 bit index buffer.
        /// </summary>
        public readonly bool IsIndex32Bits;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometricPrimitive{T}"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="geometryMesh">The geometry mesh.</param>
        /// <exception cref="System.InvalidOperationException">Cannot generate more than 65535 indices on feature level HW <= 9.3</exception>
        public GeometricPrimitive(GraphicsDevice graphicsDevice, GeometricMeshData<T> geometryMesh)
        {
            GraphicsDevice = graphicsDevice;
            //PipelineState = graphicsDevice.ResourceFactory.CreateGraphicsPipeline();

            var vertices = geometryMesh.Vertices;
            var indices = geometryMesh.Indices;

            if (geometryMesh.IsLeftHanded)
                ReverseWinding(vertices, indices);

            if (indices.Length < 0xFFFF)
            {
                var indicesShort = new ushort[indices.Length];
                for (int i = 0; i < indicesShort.Length; i++)
                {
                    indicesShort[i] = (ushort)indices[i];
                }            
                //创建IndexBuffer
                IndexBuffer = graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription((uint)(sizeof(ushort) * indicesShort.Length), BufferUsage.VertexBuffer));
            }
            else
            {
                //判断特性是否支持超过65535个Indices，16位表示
                //if (graphicsDevice.Features.CurrentProfile <= GraphicsProfile.Level_9_3)
                //{
                //    throw new InvalidOperationException("Cannot generate more than 65535 indices on feature level HW <= 9.3");
                //}
                IndexBuffer = graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription((uint)(sizeof(int) * indices.Length), BufferUsage.VertexBuffer));
                IsIndex32Bits = true;
            }

            // 创建顶点缓存
            VertexBuffer = graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription((uint)(32 * vertices.Length), BufferUsage.VertexBuffer));
            //VertexBufferBinding = new VertexBufferBinding(VertexBuffer, new T().GetLayout(), vertices.Length);

            //创建一个临时的渲染管线，由于渲染管线的状态会发现变化，因此需要一个缓存的CachePipeLine类来处理
            //PipelineState.State.SetDefaults();
            //PipelineState.State.InputElements = VertexBufferBinding.Declaration.CreateInputElements();
            //PipelineState.State.PrimitiveType = PrimitiveQuad.PrimitiveType;
        }


        /// <summary>
        /// 更新相关操作，如更新纹理，可以另建CommandList，更新StaticBuffer的操作,说白了，频次较低的操作在Update，频次较高如Draw操作在Draw中，更新渲染管线的操作
        /// 当高层次API属性发生变化时,或者是与Draw操作无关的操作。
        /// </summary>
        /// <param name="graphicsDevice"></param>
        public void Update(GraphicsDevice graphicsDevice)
        {
            //if (highObject.PropertyIsChange)
            //    ReConstructGrahphicStatusAndResourceStatus();
            //PutUpdate()

             
        }


        /// <summary>
        /// Draws this <see cref="GeometricPrimitive" />.
        /// 将EffctInstance替换成活动参数，如TexturePath，ModelViewMatrix,完成一个初步的抽象
        /// </summary>
        /// <param name="commandList">The command list.</param>
        public void Draw(GraphicsContext graphicsContext, EffectInstance effectInstance)
        {
            var commandList = graphicsContext.CommandList;
            //GraphicsDevice.comm
            // Update pipeline state
            //PipelineState.State.RootSignature = effectInstance.RootSignature;
            //PipelineState.State.EffectBytecode = effectInstance.Effect.Bytecode;
            //PipelineState.State.Output.CaptureState(commandList);
            //PipelineState.Update();
            commandList.SetPipeline(PipelineState.CurrentPipeLine);
            //effectInstance.Apply(graphicsContext);
            // Setup the Vertex Buffer
            commandList.SetIndexBuffer(IndexBuffer,IsIndex32Bits?IndexFormat.UInt32:IndexFormat.UInt16);
            commandList.SetVertexBuffer(0,VertexBuffer);
            // Finally Draw this mesh
            //UpdateDynamicBuffer
            commandList.DrawIndexed(100);
        }

        /// <summary>
        /// Helper for flipping winding of geometric primitives for LH vs. RH coordinates
        /// </summary>
        /// <typeparam name="TIndex">The type of the T index.</typeparam>
        /// <param name="vertices">The vertices.</param>
        /// <param name="indices">The indices.</param>
        private void ReverseWinding<TIndex>(T[] vertices, TIndex[] indices)
        {
            for (int i = 0; i < indices.Length; i += 3)
            {
                Utilities.Swap(ref indices[i], ref indices[i + 2]);
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].FlipWinding();
            }
        }
    }

    /// <summary>
    /// A geometric primitive. Use <see cref="Cube"/>, <see cref="Cylinder"/>, <see cref="GeoSphere"/>, <see cref="Plane"/>, <see cref="Sphere"/>, <see cref="Teapot"/>, <see cref="Torus"/>. See <see cref="Draw+vertices"/> to learn how to use it.
    /// </summary>
    public partial class GeometricPrimitive : GeometricPrimitive<VertexPositionNormalTexture>
    {
        public GeometricPrimitive(GraphicsDevice graphicsDevice, GeometricMeshData<VertexPositionNormalTexture> geometryMesh) : base(graphicsDevice, geometryMesh)
        {
        }
    }
}
