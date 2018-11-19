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
using Veldrid.ImageSharp;
using PongGlobe.Core.Util;

namespace PongGlobe.Graphics.GeometricPrimitive
{

    /// <summary>
    /// A geometric primitive. Use <see cref="Cube"/>, <see cref="Cylinder"/>, <see cref="GeoSphere"/>, <see cref="Plane"/>, <see cref="Sphere"/>, <see cref="Teapot"/>, <see cref="Torus"/>. See <see cref="Draw+vertices"/> to learn how to use it.
    /// </summary>
    public partial class GeometricPrimitive : GeometricPrimitive<VertexPositionNormalTexture>
    {
        public GeometricPrimitive(GraphicsDevice graphicsDevice, GeometricMeshData<VertexPositionNormalTexture> geometryMesh) : base(graphicsDevice, geometryMesh)
        {

        }
    }


    /// <summary>
    /// A geometric primitive used to draw a simple model built from a set of vertices and indices.
    /// </summary>
    public class GeometricPrimitive<T> : ComponentBase where T : struct ,IVertex
    {
        private GeometricMeshData<T> _meshData = null;
        private Texture _texture = null;
        private TextureView _textureView = null;
        private DeviceBuffer _styleBuffer = null;
        private ResourceSet _styleResourceSet = null;
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
        /// get or set Primitive Style
        /// </summary>
        public GeometryPrimitiveStyle Style { get; set; } = new GeometryPrimitiveStyle();
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometricPrimitive{T}"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="geometryMesh">The geometry mesh.</param>
        /// <exception cref="System.InvalidOperationException">Cannot generate more than 65535 indices on feature level HW <= 9.3</exception>
        public GeometricPrimitive(GraphicsDevice graphicsDevice, GeometricMeshData<T> geometryMesh)
        {
            //创建一个MutePipeLine对象
            PipelineState = new MutablePipeline(graphicsDevice);
            _meshData = geometryMesh;
            GraphicsDevice = graphicsDevice;           
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
                //提前更新Buffer
                graphicsDevice.UpdateBuffer(IndexBuffer, 0,  indicesShort);
            }
            else
            {
                //判断特性是否支持超过65535个Indices，16位表示
                //if (graphicsDevice.Features.CurrentProfile <= GraphicsProfile.Level_9_3)
                //{
                //    throw new InvalidOperationException("Cannot generate more than 65535 indices on feature level HW <= 9.3");
                //}
                IndexBuffer = graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription((uint)(sizeof(uint) * indices.Length), BufferUsage.VertexBuffer));
                IsIndex32Bits = true;
                graphicsDevice.UpdateBuffer(IndexBuffer, 0, indices);
            }
            // 创建顶点缓存
            VertexBuffer = graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription((uint)(32 * vertices.Length), BufferUsage.VertexBuffer));
            //提前更新顶点
            graphicsDevice.UpdateBuffer(VertexBuffer, 0, vertices);
            //创建一个临时的渲染管线，由于渲染管线的状态会发现变化，因此需要一个缓存的CachePipeLine类来处理
            //创建一个ShaderSet,
            var shaderSet = new ShaderSetDescription(new[] { new T().GetLayout() }, new Shader[] {
                   ResourceHelper.LoadEmbbedShader(ShaderStages.Fragment,"GeoPrimFrag.spv",GraphicsDevice,this.GetType().Assembly),
                   ResourceHelper.LoadEmbbedShader(ShaderStages.Vertex,"GeoPrimVet.spv",GraphicsDevice,this.GetType().Assembly)
            });
            //创建渲染管线
            PipelineState.State.ShaderSet = shaderSet;
            //创建另一个UniformBuffer,供FrameShader使用
            _styleBuffer = graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(8, BufferUsage.UniformBuffer));
            //创建Style的ResourceSet布局
            ResourceLayout styleLayout = graphicsDevice.ResourceFactory.CreateResourceLayout(
              new ResourceLayoutDescription(
                  new ResourceLayoutElementDescription("GeometryPrimitiveStyle", ResourceKind.UniformBuffer, ShaderStages.Fragment ),               
                  new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                  new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment)
                  ));                    
            //根据Style看是否创建Texture
            if (Style.Image != null)
            {
                var textureImage = new ImageSharpTexture(Style.Image);
                _texture = textureImage.CreateDeviceTexture(graphicsDevice, graphicsDevice.ResourceFactory);
                _textureView = graphicsDevice.ResourceFactory.CreateTextureView(_texture);
                _styleResourceSet = graphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
              styleLayout,
              _styleBuffer, _textureView, graphicsDevice.Aniso4xSampler
              ));
            }
            else
            {
                //创建的Resourceset
                _styleResourceSet = graphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
              styleLayout,
              _styleBuffer, null, null));               
            }
            //设置resourceSetLayout给Pipeline
            PipelineState.State.ResourceLayouts = new[] {ShareResource.ProjectionResourceLayout, styleLayout };
            //设置其PrimitiveTopoType
            PipelineState.State.PrimitiveTopology = new T().PrimitiveTopology;           
            //更新管线渲染管线状态
            PipelineState.Update();
            //提前更新一次StyleBuffer的参数,在需要的时候再更新,不必在Draw循环中频繁更新，手动控制Buffer的更新
            var style = Style.ToStyleStruct();
            graphicsDevice.UpdateBuffer(_styleBuffer,0, ref style);
        }


       
        
        /// <summary>
        /// 更新相关操作，如更新纹理，可以另建CommandList，更新StaticBuffer的操作,说白了，频次较低的操作在Update，频次较高如Draw操作在Draw中，更新渲染管线的操作
        /// 当高层次API属性发生变化时,或者是与Draw操作无关的操作。
        /// </summary>
        /// <param name="graphicsDevice"></param>
        public void Update(GraphicsDevice graphicsDevice)
        {
            //if(Style.)
        }
        /// <summary>
        /// Draws this <see cref="GeometricPrimitive" />.
        /// 将EffctInstance替换成活动参数，如TexturePath，ModelViewMatrix,完成一个初步的抽象
        /// </summary>
        /// <param name="commandList">The command list.</param>
        public void Draw(GraphicsContext graphicsContext)
        {
            var commandList = graphicsContext.CommandList;
            commandList.SetPipeline(PipelineState.CurrentPipeLine);
            commandList.SetGraphicsResourceSet(0, ShareResource.ProjectionResourceSet);
            commandList.SetGraphicsResourceSet(1, _styleResourceSet);
            commandList.SetIndexBuffer(IndexBuffer,IsIndex32Bits?IndexFormat.UInt32:IndexFormat.UInt16);
            commandList.SetVertexBuffer(0, VertexBuffer);           
            commandList.DrawIndexed((uint)_meshData.Indices.Length);
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

   
}
