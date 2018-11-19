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
        /// ������Դ���֣�����T�����в���,����������Թ̶���˲��г�
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
            //����һ��MutePipeLine����
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
                //����IndexBuffer
                IndexBuffer = graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription((uint)(sizeof(ushort) * indicesShort.Length), BufferUsage.VertexBuffer));
                //��ǰ����Buffer
                graphicsDevice.UpdateBuffer(IndexBuffer, 0,  indicesShort);
            }
            else
            {
                //�ж������Ƿ�֧�ֳ���65535��Indices��16λ��ʾ
                //if (graphicsDevice.Features.CurrentProfile <= GraphicsProfile.Level_9_3)
                //{
                //    throw new InvalidOperationException("Cannot generate more than 65535 indices on feature level HW <= 9.3");
                //}
                IndexBuffer = graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription((uint)(sizeof(uint) * indices.Length), BufferUsage.VertexBuffer));
                IsIndex32Bits = true;
                graphicsDevice.UpdateBuffer(IndexBuffer, 0, indices);
            }
            // �������㻺��
            VertexBuffer = graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription((uint)(32 * vertices.Length), BufferUsage.VertexBuffer));
            //��ǰ���¶���
            graphicsDevice.UpdateBuffer(VertexBuffer, 0, vertices);
            //����һ����ʱ����Ⱦ���ߣ�������Ⱦ���ߵ�״̬�ᷢ�ֱ仯�������Ҫһ�������CachePipeLine��������
            //����һ��ShaderSet,
            var shaderSet = new ShaderSetDescription(new[] { new T().GetLayout() }, new Shader[] {
                   ResourceHelper.LoadEmbbedShader(ShaderStages.Fragment,"GeoPrimFrag.spv",GraphicsDevice,this.GetType().Assembly),
                   ResourceHelper.LoadEmbbedShader(ShaderStages.Vertex,"GeoPrimVet.spv",GraphicsDevice,this.GetType().Assembly)
            });
            //������Ⱦ����
            PipelineState.State.ShaderSet = shaderSet;
            //������һ��UniformBuffer,��FrameShaderʹ��
            _styleBuffer = graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(8, BufferUsage.UniformBuffer));
            //����Style��ResourceSet����
            ResourceLayout styleLayout = graphicsDevice.ResourceFactory.CreateResourceLayout(
              new ResourceLayoutDescription(
                  new ResourceLayoutElementDescription("GeometryPrimitiveStyle", ResourceKind.UniformBuffer, ShaderStages.Fragment ),               
                  new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                  new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment)
                  ));                    
            //����Style���Ƿ񴴽�Texture
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
                //������Resourceset
                _styleResourceSet = graphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
              styleLayout,
              _styleBuffer, null, null));               
            }
            //����resourceSetLayout��Pipeline
            PipelineState.State.ResourceLayouts = new[] {ShareResource.ProjectionResourceLayout, styleLayout };
            //������PrimitiveTopoType
            PipelineState.State.PrimitiveTopology = new T().PrimitiveTopology;           
            //���¹�����Ⱦ����״̬
            PipelineState.Update();
            //��ǰ����һ��StyleBuffer�Ĳ���,����Ҫ��ʱ���ٸ���,������Drawѭ����Ƶ�����£��ֶ�����Buffer�ĸ���
            var style = Style.ToStyleStruct();
            graphicsDevice.UpdateBuffer(_styleBuffer,0, ref style);
        }


       
        
        /// <summary>
        /// ������ز��������������������CommandList������StaticBuffer�Ĳ���,˵���ˣ�Ƶ�νϵ͵Ĳ�����Update��Ƶ�νϸ���Draw������Draw�У�������Ⱦ���ߵĲ���
        /// ���߲��API���Է����仯ʱ,��������Draw�����޹صĲ�����
        /// </summary>
        /// <param name="graphicsDevice"></param>
        public void Update(GraphicsDevice graphicsDevice)
        {
            //if(Style.)
        }
        /// <summary>
        /// Draws this <see cref="GeometricPrimitive" />.
        /// ��EffctInstance�滻�ɻ��������TexturePath��ModelViewMatrix,���һ�������ĳ���
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
