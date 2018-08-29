﻿using System;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Veldrid.D3D11
{
    internal class D3D11CommandList : CommandList
    {
        private readonly D3D11GraphicsDevice _gd;
        private readonly DeviceContext _context;
        private readonly DeviceContext1 _context1;
        private bool _begun;
        private bool _disposed;

        private RawViewportF[] _viewports = new RawViewportF[0];
        private RawRectangle[] _scissors = new RawRectangle[0];
        private bool _viewportsChanged;
        private bool _scissorRectsChanged;

        private uint _numVertexBindings = 0;
        private SharpDX.Direct3D11.Buffer[] _vertexBindings = new SharpDX.Direct3D11.Buffer[1];
        private int[] _vertexStrides;
        private int[] _vertexOffsets = new int[1];

        // Cached pipeline State
        private DeviceBuffer _ib;
        private uint _ibOffset;
        private BlendState _blendState;
        private DepthStencilState _depthStencilState;
        private uint _stencilReference;
        private RasterizerState _rasterizerState;
        private SharpDX.Direct3D.PrimitiveTopology _primitiveTopology;
        private InputLayout _inputLayout;
        private VertexShader _vertexShader;
        private GeometryShader _geometryShader;
        private HullShader _hullShader;
        private DomainShader _domainShader;
        private PixelShader _pixelShader;

        private new D3D11Pipeline _graphicsPipeline;
        private D3D11ResourceSet[] _graphicsResourceSets = new D3D11ResourceSet[1];
        // Resource sets are invalidated when a new resource set is bound with an incompatible SRV or UAV.
        private bool[] _invalidatedGraphicsResourceSets = new bool[1];

        private new D3D11Pipeline _computePipeline;
        private D3D11ResourceSet[] _computeResourceSets = new D3D11ResourceSet[1];
        // Resource sets are invalidated when a new resource set is bound with an incompatible SRV or UAV.
        private bool[] _invalidatedComputeResourceSets = new bool[1];
        private string _name;

        // Cached resources
        private const int MaxCachedUniformBuffers = 15;
        private readonly D3D11Buffer[] _vertexBoundUniformBuffers = new D3D11Buffer[MaxCachedUniformBuffers];
        private readonly D3D11Buffer[] _fragmentBoundUniformBuffers = new D3D11Buffer[MaxCachedUniformBuffers];
        private const int MaxCachedTextureViews = 16;
        private readonly D3D11TextureView[] _vertexBoundTextureViews = new D3D11TextureView[MaxCachedTextureViews];
        private readonly D3D11TextureView[] _fragmentBoundTextureViews = new D3D11TextureView[MaxCachedTextureViews];
        private const int MaxCachedSamplers = 4;
        private readonly D3D11Sampler[] _vertexBoundSamplers = new D3D11Sampler[MaxCachedSamplers];
        private readonly D3D11Sampler[] _fragmentBoundSamplers = new D3D11Sampler[MaxCachedSamplers];

        private readonly Dictionary<Texture, List<BoundTextureInfo>> _boundSRVs = new Dictionary<Texture, List<BoundTextureInfo>>();
        private readonly Dictionary<Texture, List<BoundTextureInfo>> _boundUAVs = new Dictionary<Texture, List<BoundTextureInfo>>();
        private readonly List<List<BoundTextureInfo>> _boundTextureInfoPool = new List<List<BoundTextureInfo>>(MaxCachedTextureInfoLists);
        private const int MaxCachedTextureInfoLists = 20;

        private const int MaxUAVs = 8;
        private readonly List<(DeviceBuffer, int)> _boundComputeUAVBuffers = new List<(DeviceBuffer, int)>(MaxUAVs);
        private readonly List<(DeviceBuffer, int)> _boundOMUAVBuffers = new List<(DeviceBuffer, int)>(MaxUAVs);

        private readonly List<D3D11Buffer> _availableStagingBuffers = new List<D3D11Buffer>();
        private readonly List<D3D11Buffer> _submittedStagingBuffers = new List<D3D11Buffer>();

        private readonly List<D3D11Swapchain> _referencedSwapchains = new List<D3D11Swapchain>();

        public D3D11CommandList(D3D11GraphicsDevice gd, ref CommandListDescription description)
            : base(ref description, gd.Features)
        {
            _gd = gd;
            _context = new DeviceContext(gd.Device);
            _context1 = _context.QueryInterfaceOrNull<DeviceContext1>();
            if (_context1 == null)
            {
                throw new VeldridException("Direct3D 11.1 is required.");
            }
        }

        public SharpDX.Direct3D11.CommandList DeviceCommandList { get; private set; }

        internal DeviceContext DeviceContext => _context;

        private D3D11Framebuffer D3D11Framebuffer => Util.AssertSubtype<Framebuffer, D3D11Framebuffer>(_framebuffer);

        public override void Begin()
        {
            DeviceCommandList?.Dispose();
            DeviceCommandList = null;
            ClearState();
            _begun = true;
        }

        private void ClearState()
        {
            ClearCachedState();
            _context.ClearState();
            ResetManagedState();
        }

        private void ResetManagedState()
        {
            _numVertexBindings = 0;
            Util.ClearArray(_vertexBindings);
            _vertexStrides = null;
            Util.ClearArray(_vertexOffsets);

            _framebuffer = null;

            Util.ClearArray(_viewports);
            Util.ClearArray(_scissors);
            _viewportsChanged = false;
            _scissorRectsChanged = false;

            _ib = null;
            _graphicsPipeline = null;
            _blendState = null;
            _depthStencilState = null;
            _rasterizerState = null;
            _primitiveTopology = SharpDX.Direct3D.PrimitiveTopology.Undefined;
            _inputLayout = null;
            _vertexShader = null;
            _geometryShader = null;
            _hullShader = null;
            _domainShader = null;
            _pixelShader = null;
            Util.ClearArray(_graphicsResourceSets);

            Util.ClearArray(_vertexBoundUniformBuffers);
            Util.ClearArray(_vertexBoundTextureViews);
            Util.ClearArray(_vertexBoundSamplers);

            Util.ClearArray(_fragmentBoundUniformBuffers);
            Util.ClearArray(_fragmentBoundTextureViews);
            Util.ClearArray(_fragmentBoundSamplers);

            _computePipeline = null;
            Util.ClearArray(_computeResourceSets);

            foreach (KeyValuePair<Texture, List<BoundTextureInfo>> kvp in _boundSRVs)
            {
                List<BoundTextureInfo> list = kvp.Value;
                list.Clear();
                PoolBoundTextureList(list);
            }
            _boundSRVs.Clear();

            foreach (KeyValuePair<Texture, List<BoundTextureInfo>> kvp in _boundUAVs)
            {
                List<BoundTextureInfo> list = kvp.Value;
                list.Clear();
                PoolBoundTextureList(list);
            }
            _boundUAVs.Clear();
        }

        public override void End()
        {
            if (DeviceCommandList != null)
            {
                throw new VeldridException("Invalid use of End().");
            }

            DeviceCommandList = _context.FinishCommandList(false);
            ResetManagedState();
            _begun = false;
        }

        public void Reset()
        {
            if (DeviceCommandList != null)
            {
                DeviceCommandList.Dispose();
                DeviceCommandList = null;
            }
            else if (_begun)
            {
                _context.ClearState();
                SharpDX.Direct3D11.CommandList cl = _context.FinishCommandList(false);
                cl.Dispose();
            }

            ResetManagedState();
            _begun = false;
        }

        protected override void SetIndexBufferCore(DeviceBuffer buffer, IndexFormat format, uint offset)
        {
            if (_ib != buffer || _ibOffset != offset)
            {
                _ib = buffer;
                _ibOffset = offset;
                D3D11Buffer d3d11Buffer = Util.AssertSubtype<DeviceBuffer, D3D11Buffer>(buffer);
                UnbindUAVBuffer(buffer);
                _context.InputAssembler.SetIndexBuffer(d3d11Buffer.Buffer, D3D11Formats.ToDxgiFormat(format), (int)offset);
            }
        }

        protected override void SetPipelineCore(Pipeline pipeline)
        {
            if (!pipeline.IsComputePipeline && _graphicsPipeline != pipeline)
            {
                D3D11Pipeline d3dPipeline = Util.AssertSubtype<Pipeline, D3D11Pipeline>(pipeline);
                _graphicsPipeline = d3dPipeline;
                Util.ClearArray(_graphicsResourceSets); // Invalidate resource set bindings -- they may be invalid.
                Util.ClearArray(_invalidatedGraphicsResourceSets);

                BlendState blendState = d3dPipeline.BlendState;
                if (_blendState != blendState)
                {
                    _blendState = blendState;
                    _context.OutputMerger.SetBlendState(blendState);
                }

                DepthStencilState depthStencilState = d3dPipeline.DepthStencilState;
                uint stencilReference = d3dPipeline.StencilReference;
                if (_depthStencilState != depthStencilState || _stencilReference != stencilReference)
                {
                    _depthStencilState = depthStencilState;
                    _stencilReference = stencilReference;
                    _context.OutputMerger.SetDepthStencilState(depthStencilState, (int)stencilReference);
                }

                RasterizerState rasterizerState = d3dPipeline.RasterizerState;
                if (_rasterizerState != rasterizerState)
                {
                    _rasterizerState = rasterizerState;
                    _context.Rasterizer.State = rasterizerState;
                }

                SharpDX.Direct3D.PrimitiveTopology primitiveTopology = d3dPipeline.PrimitiveTopology;
                if (_primitiveTopology != primitiveTopology)
                {
                    _primitiveTopology = primitiveTopology;
                    _context.InputAssembler.PrimitiveTopology = primitiveTopology;
                }

                InputLayout inputLayout = d3dPipeline.InputLayout;
                if (_inputLayout != inputLayout)
                {
                    _inputLayout = inputLayout;
                    _context.InputAssembler.InputLayout = inputLayout;
                }

                VertexShader vertexShader = d3dPipeline.VertexShader;
                if (_vertexShader != vertexShader)
                {
                    _vertexShader = vertexShader;
                    _context.VertexShader.Set(vertexShader);
                }

                GeometryShader geometryShader = d3dPipeline.GeometryShader;
                if (_geometryShader != geometryShader)
                {
                    _geometryShader = geometryShader;
                    _context.GeometryShader.Set(geometryShader);
                }

                HullShader hullShader = d3dPipeline.HullShader;
                if (_hullShader != hullShader)
                {
                    _hullShader = hullShader;
                    _context.HullShader.Set(hullShader);
                }

                DomainShader domainShader = d3dPipeline.DomainShader;
                if (_domainShader != domainShader)
                {
                    _domainShader = domainShader;
                    _context.DomainShader.Set(domainShader);
                }

                PixelShader pixelShader = d3dPipeline.PixelShader;
                if (_pixelShader != pixelShader)
                {
                    _pixelShader = pixelShader;
                    _context.PixelShader.Set(pixelShader);
                }

                _vertexStrides = d3dPipeline.VertexStrides;
                if (_vertexStrides != null)
                {
                    int vertexStridesCount = _vertexStrides.Length;
                    Util.EnsureArrayMinimumSize(ref _vertexBindings, (uint)vertexStridesCount);
                    Util.EnsureArrayMinimumSize(ref _vertexOffsets, (uint)vertexStridesCount);
                }

                Util.EnsureArrayMinimumSize(ref _graphicsResourceSets, (uint)d3dPipeline.ResourceLayouts.Length);
                Util.EnsureArrayMinimumSize(ref _invalidatedGraphicsResourceSets, (uint)d3dPipeline.ResourceLayouts.Length);
            }
            else if (pipeline.IsComputePipeline && _computePipeline != pipeline)
            {
                D3D11Pipeline d3dPipeline = Util.AssertSubtype<Pipeline, D3D11Pipeline>(pipeline);
                _computePipeline = d3dPipeline;
                Util.ClearArray(_computeResourceSets); // Invalidate resource set bindings -- they may be invalid.
                Util.ClearArray(_invalidatedComputeResourceSets);

                ComputeShader computeShader = d3dPipeline.ComputeShader;
                _context.ComputeShader.Set(computeShader);
                Util.EnsureArrayMinimumSize(ref _computeResourceSets, (uint)d3dPipeline.ResourceLayouts.Length);
                Util.EnsureArrayMinimumSize(ref _invalidatedComputeResourceSets, (uint)d3dPipeline.ResourceLayouts.Length);
            }
        }

        protected override void SetGraphicsResourceSetCore(uint slot, ResourceSet rs)
        {
            if (_graphicsResourceSets[slot] == rs)
            {
                return;
            }

            D3D11ResourceSet d3d11RS = Util.AssertSubtype<ResourceSet, D3D11ResourceSet>(rs);
            _graphicsResourceSets[slot] = d3d11RS;
            ActivateResourceSet(slot, d3d11RS, true);
        }

        protected override void SetComputeResourceSetCore(uint slot, ResourceSet set)
        {
            if (_computeResourceSets[slot] == set)
            {
                return;
            }

            D3D11ResourceSet d3d11RS = Util.AssertSubtype<ResourceSet, D3D11ResourceSet>(set);
            _computeResourceSets[slot] = d3d11RS;
            ActivateResourceSet(slot, d3d11RS, false);
        }

        private void ActivateResourceSet(uint slot, D3D11ResourceSet d3d11RS, bool graphics)
        {
            int cbBase = GetConstantBufferBase(slot, graphics);
            int uaBase = GetUnorderedAccessBase(slot, graphics);
            int textureBase = GetTextureBase(slot, graphics);
            int samplerBase = GetSamplerBase(slot, graphics);

            D3D11ResourceLayout layout = d3d11RS.Layout;
            BindableResource[] resources = d3d11RS.Resources;
            for (int i = 0; i < resources.Length; i++)
            {
                BindableResource resource = resources[i];
                D3D11ResourceLayout.ResourceBindingInfo rbi = layout.GetDeviceSlotIndex(i);
                switch (rbi.Kind)
                {
                    case ResourceKind.UniformBuffer:
                        D3D11Buffer uniformBuffer = Util.AssertSubtype<BindableResource, D3D11Buffer>(resource);
                        BindUniformBuffer(uniformBuffer, cbBase + rbi.Slot, rbi.Stages);
                        break;
                    case ResourceKind.StructuredBufferReadOnly:
                        D3D11Buffer storageBufferRO = Util.AssertSubtype<BindableResource, D3D11Buffer>(resource);
                        BindStorageBufferView(storageBufferRO, textureBase + rbi.Slot, rbi.Stages);
                        break;
                    case ResourceKind.StructuredBufferReadWrite:
                        D3D11Buffer storageBuffer = Util.AssertSubtype<BindableResource, D3D11Buffer>(resource);
                        BindUnorderedAccessView(null, storageBuffer, storageBuffer.UnorderedAccessView, uaBase + rbi.Slot, rbi.Stages, slot);
                        break;
                    case ResourceKind.TextureReadOnly:
                        D3D11TextureView texView = Util.AssertSubtype<BindableResource, D3D11TextureView>(resource);
                        UnbindUAVTexture(texView.Target);
                        BindTextureView(texView, textureBase + rbi.Slot, rbi.Stages, slot);
                        break;
                    case ResourceKind.TextureReadWrite:
                        D3D11TextureView rwTexView = Util.AssertSubtype<BindableResource, D3D11TextureView>(resource);
                        UnbindSRVTexture(rwTexView.Target);
                        BindUnorderedAccessView(rwTexView.Target, null, rwTexView.UnorderedAccessView, uaBase + rbi.Slot, rbi.Stages, slot);
                        break;
                    case ResourceKind.Sampler:
                        D3D11Sampler sampler = Util.AssertSubtype<BindableResource, D3D11Sampler>(resource);
                        BindSampler(sampler, samplerBase + rbi.Slot, rbi.Stages);
                        break;
                    default: throw Illegal.Value<ResourceKind>();
                }
            }
        }

        private void UnbindSRVTexture(Texture target)
        {
            if (_boundSRVs.TryGetValue(target, out List<BoundTextureInfo> btis))
            {
                foreach (BoundTextureInfo bti in btis)
                {
                    BindTextureView(null, bti.Slot, bti.Stages, 0);

                    if ((bti.Stages & ShaderStages.Compute) == ShaderStages.Compute)
                    {
                        _invalidatedComputeResourceSets[bti.ResourceSet] = true;
                    }
                    else
                    {
                        _invalidatedGraphicsResourceSets[bti.ResourceSet] = true;
                    }
                }

                bool result = _boundSRVs.Remove(target);
                Debug.Assert(result);

                btis.Clear();
                PoolBoundTextureList(btis);
            }
        }

        private void PoolBoundTextureList(List<BoundTextureInfo> btis)
        {
            if (_boundTextureInfoPool.Count < MaxCachedTextureInfoLists)
            {
                _boundTextureInfoPool.Add(btis);
            }
        }

        private void UnbindUAVTexture(Texture target)
        {
            if (_boundUAVs.TryGetValue(target, out List<BoundTextureInfo> btis))
            {
                foreach (BoundTextureInfo bti in btis)
                {
                    BindUnorderedAccessView(null, null, null, bti.Slot, bti.Stages, bti.ResourceSet);
                    if ((bti.Stages & ShaderStages.Compute) == ShaderStages.Compute)
                    {
                        _invalidatedComputeResourceSets[bti.ResourceSet] = true;
                    }
                    else
                    {
                        _invalidatedGraphicsResourceSets[bti.ResourceSet] = true;
                    }
                }

                bool result = _boundUAVs.Remove(target);
                Debug.Assert(result);

                btis.Clear();
                PoolBoundTextureList(btis);
            }
        }

        private int GetConstantBufferBase(uint slot, bool graphics)
        {
            D3D11ResourceLayout[] layouts = graphics ? _graphicsPipeline.ResourceLayouts : _computePipeline.ResourceLayouts;
            int ret = 0;
            for (int i = 0; i < slot; i++)
            {
                Debug.Assert(layouts[i] != null);
                ret += layouts[i].UniformBufferCount;
            }

            return ret;
        }

        private int GetUnorderedAccessBase(uint slot, bool graphics)
        {
            D3D11ResourceLayout[] layouts = graphics ? _graphicsPipeline.ResourceLayouts : _computePipeline.ResourceLayouts;
            int ret = 0;
            for (int i = 0; i < slot; i++)
            {
                Debug.Assert(layouts[i] != null);
                ret += layouts[i].StorageBufferCount;
            }

            return ret;
        }

        private int GetTextureBase(uint slot, bool graphics)
        {
            D3D11ResourceLayout[] layouts = graphics ? _graphicsPipeline.ResourceLayouts : _computePipeline.ResourceLayouts;
            int ret = 0;
            for (int i = 0; i < slot; i++)
            {
                Debug.Assert(layouts[i] != null);
                ret += layouts[i].TextureCount;
            }

            return ret;
        }

        private int GetSamplerBase(uint slot, bool graphics)
        {
            D3D11ResourceLayout[] layouts = graphics ? _graphicsPipeline.ResourceLayouts : _computePipeline.ResourceLayouts;
            int ret = 0;
            for (int i = 0; i < slot; i++)
            {
                Debug.Assert(layouts[i] != null);
                ret += layouts[i].SamplerCount;
            }

            return ret;
        }

        protected override void SetVertexBufferCore(uint index, DeviceBuffer buffer, uint offset)
        {
            D3D11Buffer d3d11Buffer = Util.AssertSubtype<DeviceBuffer, D3D11Buffer>(buffer);
            UnbindUAVBuffer(buffer);
            _vertexBindings[index] = d3d11Buffer.Buffer;
            _vertexOffsets[index] = (int)offset;
            _numVertexBindings = Math.Max((index + 1), _numVertexBindings);
        }

        protected override void DrawCore(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
        {
            PreDrawCommand();

            if (instanceCount == 1 && instanceStart == 0)
            {
                _context.Draw((int)vertexCount, (int)vertexStart);
            }
            else
            {
                _context.DrawInstanced((int)vertexCount, (int)instanceCount, (int)vertexStart, (int)instanceStart);
            }
        }

        protected override void DrawIndexedCore(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            PreDrawCommand();

            Debug.Assert(_ib != null);
            if (instanceCount == 1 && instanceStart == 0)
            {
                _context.DrawIndexed((int)indexCount, (int)indexStart, vertexOffset);
            }
            else
            {
                _context.DrawIndexedInstanced((int)indexCount, (int)instanceCount, (int)indexStart, vertexOffset, (int)instanceStart);
            }
        }

        protected override void DrawIndirectCore(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            PreDrawCommand();

            D3D11Buffer d3d11Buffer = Util.AssertSubtype<DeviceBuffer, D3D11Buffer>(indirectBuffer);
            int currentOffset = (int)offset;
            for (uint i = 0; i < drawCount; i++)
            {
                _context.DrawInstancedIndirect(d3d11Buffer.Buffer, currentOffset);
                currentOffset += (int)stride;
            }
        }

        protected override void DrawIndexedIndirectCore(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            PreDrawCommand();

            D3D11Buffer d3d11Buffer = Util.AssertSubtype<DeviceBuffer, D3D11Buffer>(indirectBuffer);
            int currentOffset = (int)offset;
            for (uint i = 0; i < drawCount; i++)
            {
                _context.DrawIndexedInstancedIndirect(d3d11Buffer.Buffer, currentOffset);
                currentOffset += (int)stride;
            }
        }

        private void PreDrawCommand()
        {
            FlushViewports();
            FlushScissorRects();
            FlushVertexBindings();

            int graphicsResourceCount = _graphicsPipeline.ResourceLayouts.Length;
            for (uint i = 0; i < graphicsResourceCount; i++)
            {
                if (_invalidatedGraphicsResourceSets[i])
                {
                    _invalidatedGraphicsResourceSets[i] = false;
                    ActivateResourceSet(i, _graphicsResourceSets[i], true);
                }
            }
        }

        public override void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ)
        {
            PreDispatchCommand();

            _context.Dispatch((int)groupCountX, (int)groupCountY, (int)groupCountZ);
        }

        protected override void DispatchIndirectCore(DeviceBuffer indirectBuffer, uint offset)
        {
            PreDispatchCommand();
            D3D11Buffer d3d11Buffer = Util.AssertSubtype<DeviceBuffer, D3D11Buffer>(indirectBuffer);
            _context.DispatchIndirect(d3d11Buffer.Buffer, (int)offset);
        }

        private void PreDispatchCommand()
        {
            int computeResourceCount = _computePipeline.ResourceLayouts.Length;
            for (uint i = 0; i < computeResourceCount; i++)
            {
                if (_invalidatedComputeResourceSets[i])
                {
                    _invalidatedComputeResourceSets[i] = false;
                    ActivateResourceSet(i, _computeResourceSets[i], false);
                }
            }
        }

        protected override void ResolveTextureCore(Texture source, Texture destination)
        {
            D3D11Texture d3d11Source = Util.AssertSubtype<Texture, D3D11Texture>(source);
            D3D11Texture d3d11Destination = Util.AssertSubtype<Texture, D3D11Texture>(destination);
            _context.ResolveSubresource(
                d3d11Source.DeviceTexture,
                0,
                d3d11Destination.DeviceTexture,
                0,
                d3d11Destination.DxgiFormat);
        }

        private void FlushViewports()
        {
            if (_viewportsChanged)
            {
                _viewportsChanged = false;
                _context.Rasterizer.SetViewports(_viewports, _viewports.Length);
            }
        }

        private void FlushScissorRects()
        {
            if (_scissorRectsChanged)
            {
                _scissorRectsChanged = false;
                if (_scissors.Length > 0)
                {
                    // Because this array is resized using Util.EnsureMinimumArraySize, this might set more scissor rectangles
                    // than are actually needed, but this is okay -- extras are essentially ignored and should be harmless.
                    _context.Rasterizer.SetScissorRectangles(_scissors);
                }
            }
        }

        private unsafe void FlushVertexBindings()
        {
            IntPtr* buffersPtr = stackalloc IntPtr[(int)_numVertexBindings];
            for (int i = 0; i < _numVertexBindings; i++)
            {
                buffersPtr[i] = _vertexBindings[i].NativePointer;
            }
            fixed (int* stridesPtr = _vertexStrides)
            fixed (int* offsetsPtr = _vertexOffsets)
            {
                _context.InputAssembler.SetVertexBuffers(
                    0, (int)_numVertexBindings,
                    (IntPtr)buffersPtr,
                    (IntPtr)stridesPtr,
                    (IntPtr)offsetsPtr);
            }
        }

        public override void SetScissorRect(uint index, uint x, uint y, uint width, uint height)
        {
            _scissorRectsChanged = true;
            Util.EnsureArrayMinimumSize(ref _scissors, index + 1);
            _scissors[index] = new RawRectangle((int)x, (int)y, (int)(x + width), (int)(y + height));
        }

        public override void SetViewport(uint index, ref Viewport viewport)
        {
            _viewportsChanged = true;
            Util.EnsureArrayMinimumSize(ref _viewports, index + 1);
            _viewports[index] = new RawViewportF
            {
                X = viewport.X,
                Y = viewport.Y,
                Width = viewport.Width,
                Height = viewport.Height,
                MinDepth = viewport.MinDepth,
                MaxDepth = viewport.MaxDepth
            };
        }

        private void BindTextureView(D3D11TextureView texView, int slot, ShaderStages stages, uint resourceSet)
        {
            ShaderResourceView srv = texView?.ShaderResourceView ?? null;
            if (srv != null)
            {
                if (!_boundSRVs.TryGetValue(texView.Target, out List<BoundTextureInfo> list))
                {
                    list = GetNewOrCachedBoundTextureInfoList();
                    _boundSRVs.Add(texView.Target, list);
                }
                list.Add(new BoundTextureInfo { Slot = slot, Stages = stages, ResourceSet = resourceSet });
            }

            if ((stages & ShaderStages.Vertex) == ShaderStages.Vertex)
            {
                bool bind = false;
                if (slot < MaxCachedUniformBuffers)
                {
                    if (_vertexBoundTextureViews[slot] != texView)
                    {
                        _vertexBoundTextureViews[slot] = texView;
                        bind = true;
                    }
                }
                else
                {
                    bind = true;
                }
                if (bind)
                {
                    _context.VertexShader.SetShaderResource(slot, srv);
                }
            }
            if ((stages & ShaderStages.Geometry) == ShaderStages.Geometry)
            {
                _context.GeometryShader.SetShaderResource(slot, srv);
            }
            if ((stages & ShaderStages.TessellationControl) == ShaderStages.TessellationControl)
            {
                _context.HullShader.SetShaderResource(slot, srv);
            }
            if ((stages & ShaderStages.TessellationEvaluation) == ShaderStages.TessellationEvaluation)
            {
                _context.DomainShader.SetShaderResource(slot, srv);
            }
            if ((stages & ShaderStages.Fragment) == ShaderStages.Fragment)
            {
                bool bind = false;
                if (slot < MaxCachedUniformBuffers)
                {
                    if (_fragmentBoundTextureViews[slot] != texView)
                    {
                        _fragmentBoundTextureViews[slot] = texView;
                        bind = true;
                    }
                }
                else
                {
                    bind = true;
                }
                if (bind)
                {
                    _context.PixelShader.SetShaderResource(slot, srv);
                }
            }
            if ((stages & ShaderStages.Compute) == ShaderStages.Compute)
            {
                _context.ComputeShader.SetShaderResource(slot, srv);
            }
        }

        private List<BoundTextureInfo> GetNewOrCachedBoundTextureInfoList()
        {
            if (_boundTextureInfoPool.Count > 0)
            {
                int index = _boundTextureInfoPool.Count - 1;
                List<BoundTextureInfo> ret = _boundTextureInfoPool[index];
                _boundTextureInfoPool.RemoveAt(index);
                return ret;
            }

            return new List<BoundTextureInfo>();
        }

        private void BindStorageBufferView(D3D11Buffer storageBufferRO, int slot, ShaderStages stages)
        {
            bool compute = (stages & ShaderStages.Compute) != 0;
            UnbindUAVBuffer(storageBufferRO);

            ShaderResourceView srv = storageBufferRO.ShaderResourceView;
            if ((stages & ShaderStages.Vertex) == ShaderStages.Vertex)
            {
                _context.VertexShader.SetShaderResource(slot, srv);
            }
            if ((stages & ShaderStages.Geometry) == ShaderStages.Geometry)
            {
                _context.GeometryShader.SetShaderResource(slot, srv);
            }
            if ((stages & ShaderStages.TessellationControl) == ShaderStages.TessellationControl)
            {
                _context.HullShader.SetShaderResource(slot, srv);
            }
            if ((stages & ShaderStages.TessellationEvaluation) == ShaderStages.TessellationEvaluation)
            {
                _context.DomainShader.SetShaderResource(slot, srv);
            }
            if ((stages & ShaderStages.Fragment) == ShaderStages.Fragment)
            {
                _context.PixelShader.SetShaderResource(slot, srv);
            }
            if (compute)
            {
                _context.ComputeShader.SetShaderResource(slot, srv);
            }
        }

        private void BindUniformBuffer(D3D11Buffer ub, int slot, ShaderStages stages)
        {
            if ((stages & ShaderStages.Vertex) == ShaderStages.Vertex)
            {
                bool bind = false;
                if (slot < MaxCachedUniformBuffers)
                {
                    if (_vertexBoundUniformBuffers[slot] != ub)
                    {
                        _vertexBoundUniformBuffers[slot] = ub;
                        bind = true;
                    }
                }
                else
                {
                    bind = true;
                }
                if (bind)
                {
                    _context.VertexShader.SetConstantBuffer(slot, ub.Buffer);
                }
            }
            if ((stages & ShaderStages.Geometry) == ShaderStages.Geometry)
            {
                _context.GeometryShader.SetConstantBuffer(slot, ub.Buffer);
            }
            if ((stages & ShaderStages.TessellationControl) == ShaderStages.TessellationControl)
            {
                _context.HullShader.SetConstantBuffer(slot, ub.Buffer);
            }
            if ((stages & ShaderStages.TessellationEvaluation) == ShaderStages.TessellationEvaluation)
            {
                _context.DomainShader.SetConstantBuffer(slot, ub.Buffer);
            }
            if ((stages & ShaderStages.Fragment) == ShaderStages.Fragment)
            {
                bool bind = false;
                if (slot < MaxCachedUniformBuffers)
                {
                    if (_fragmentBoundUniformBuffers[slot] != ub)
                    {
                        _fragmentBoundUniformBuffers[slot] = ub;
                        bind = true;
                    }
                }
                else
                {
                    bind = true;
                }
                if (bind)
                {
                    _context.PixelShader.SetConstantBuffer(slot, ub.Buffer);
                }
            }
            if ((stages & ShaderStages.Compute) == ShaderStages.Compute)
            {
                _context.ComputeShader.SetConstantBuffer(slot, ub.Buffer);
            }
        }

        private void BindUnorderedAccessView(
            Texture texture,
            DeviceBuffer buffer,
            UnorderedAccessView uav,
            int slot,
            ShaderStages stages,
            uint resourceSet)
        {
            bool compute = stages == ShaderStages.Compute;
            Debug.Assert(compute || ((stages & ShaderStages.Compute) == 0));
            Debug.Assert(texture == null || buffer == null);

            if (texture != null && uav != null)
            {
                if (!_boundUAVs.TryGetValue(texture, out List<BoundTextureInfo> list))
                {
                    list = GetNewOrCachedBoundTextureInfoList();
                    _boundUAVs.Add(texture, list);
                }
                list.Add(new BoundTextureInfo { Slot = slot, Stages = stages, ResourceSet = resourceSet });
            }


            int baseSlot = 0;
            if (!compute && _fragmentBoundSamplers != null)
            {
                baseSlot = _framebuffer.ColorTargets.Count;
            }
            int actualSlot = baseSlot + slot;

            if (buffer != null)
            {
                TrackBoundUAVBuffer(buffer, actualSlot, compute);
            }

            if (compute)
            {
                _context.ComputeShader.SetUnorderedAccessView(actualSlot, uav);
            }
            else
            {
                _context.OutputMerger.SetUnorderedAccessView(actualSlot, uav);
            }
        }

        private void TrackBoundUAVBuffer(DeviceBuffer buffer, int slot, bool compute)
        {
            List<(DeviceBuffer, int)> list = compute ? _boundComputeUAVBuffers : _boundOMUAVBuffers;
            list.Add((buffer, slot));
        }

        private void UnbindUAVBuffer(DeviceBuffer buffer)
        {
            UnbindUAVBufferIndividual(buffer, false);
            UnbindUAVBufferIndividual(buffer, true);
        }

        private void UnbindUAVBufferIndividual(DeviceBuffer buffer, bool compute)
        {
            List<(DeviceBuffer, int)> list = compute ? _boundComputeUAVBuffers : _boundOMUAVBuffers;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Item1 == buffer)
                {
                    int slot = list[i].Item2;
                    if (compute)
                    {
                        _context.ComputeShader.SetUnorderedAccessView(slot, null);
                    }
                    else
                    {
                        _context.OutputMerger.SetUnorderedAccessView(slot, null);
                    }

                    list.RemoveAt(i);
                    i -= 1;
                }
            }
        }

        private void BindSampler(D3D11Sampler sampler, int slot, ShaderStages stages)
        {
            if ((stages & ShaderStages.Vertex) == ShaderStages.Vertex)
            {
                bool bind = false;
                if (slot < MaxCachedSamplers)
                {
                    if (_vertexBoundSamplers[slot] != sampler)
                    {
                        _vertexBoundSamplers[slot] = sampler;
                        bind = true;
                    }
                }
                else
                {
                    bind = true;
                }
                if (bind)
                {
                    _context.VertexShader.SetSampler(slot, sampler.DeviceSampler);
                }
            }
            if ((stages & ShaderStages.Geometry) == ShaderStages.Geometry)
            {
                _context.GeometryShader.SetSampler(slot, sampler.DeviceSampler);
            }
            if ((stages & ShaderStages.TessellationControl) == ShaderStages.TessellationControl)
            {
                _context.HullShader.SetSampler(slot, sampler.DeviceSampler);
            }
            if ((stages & ShaderStages.TessellationEvaluation) == ShaderStages.TessellationEvaluation)
            {
                _context.DomainShader.SetSampler(slot, sampler.DeviceSampler);
            }
            if ((stages & ShaderStages.Fragment) == ShaderStages.Fragment)
            {
                bool bind = false;
                if (slot < MaxCachedSamplers)
                {
                    if (_fragmentBoundSamplers[slot] != sampler)
                    {
                        _fragmentBoundSamplers[slot] = sampler;
                        bind = true;
                    }
                }
                else
                {
                    bind = true;
                }
                if (bind)
                {
                    _context.PixelShader.SetSampler(slot, sampler.DeviceSampler);
                }
            }
        }

        protected override void SetFramebufferCore(Framebuffer fb)
        {
            D3D11Framebuffer d3dFB = Util.AssertSubtype<Framebuffer, D3D11Framebuffer>(fb);
            if (d3dFB.Swapchain != null)
            {
                d3dFB.Swapchain.AddCommandListReference(this);
                _referencedSwapchains.Add(d3dFB.Swapchain);
            }

            _context.OutputMerger.SetRenderTargets(d3dFB.DepthStencilView, d3dFB.RenderTargetViews);
        }

        protected override void ClearColorTargetCore(uint index, RgbaFloat clearColor)
        {
            _context.ClearRenderTargetView(D3D11Framebuffer.RenderTargetViews[index], new RawColor4(clearColor.R, clearColor.G, clearColor.B, clearColor.A));
        }

        protected override void ClearDepthStencilCore(float depth, byte stencil)
        {
            _context.ClearDepthStencilView(D3D11Framebuffer.DepthStencilView, DepthStencilClearFlags.Depth, depth, stencil);
        }

        public unsafe override void UpdateBuffer(DeviceBuffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
        {
            D3D11Buffer d3dBuffer = Util.AssertSubtype<DeviceBuffer, D3D11Buffer>(buffer);
            if (sizeInBytes == 0)
            {
                return;
            }

            bool isDynamic = (buffer.Usage & BufferUsage.Dynamic) == BufferUsage.Dynamic;
            bool isStaging = (buffer.Usage & BufferUsage.Staging) == BufferUsage.Staging;
            bool isUniformBuffer = (buffer.Usage & BufferUsage.UniformBuffer) == BufferUsage.UniformBuffer;
            bool useMap = isDynamic;
            bool updateFullBuffer = bufferOffsetInBytes == 0 && sizeInBytes == buffer.SizeInBytes;
            bool useUpdateSubresource = !isDynamic && !isStaging && (!isUniformBuffer || updateFullBuffer);

            if (useUpdateSubresource)
            {
                ResourceRegion? subregion = new ResourceRegion()
                {
                    Left = (int)bufferOffsetInBytes,
                    Right = (int)(sizeInBytes + bufferOffsetInBytes),
                    Bottom = 1,
                    Back = 1
                };
                if (isUniformBuffer)
                {
                    subregion = null;
                }

                if (bufferOffsetInBytes == 0)
                {
                    _context.UpdateSubresource(d3dBuffer.Buffer, 0, subregion, source, 0, 0);
                }
                else
                {
                    _context1.UpdateSubresource1(d3dBuffer.Buffer, 0, subregion, source, 0, 0, 0);
                }
            }
            else if (useMap && updateFullBuffer) // Can only update full buffer with WriteDiscard.
            {
                SharpDX.DataBox db = _context.MapSubresource(
                    d3dBuffer.Buffer,
                    0,
                    D3D11Formats.VdToD3D11MapMode(isDynamic, MapMode.Write),
                    MapFlags.None);
                if (sizeInBytes < 1024)
                {
                    Unsafe.CopyBlock(db.DataPointer.ToPointer(), source.ToPointer(), sizeInBytes);
                }
                else
                {
                    System.Buffer.MemoryCopy(source.ToPointer(), db.DataPointer.ToPointer(), buffer.SizeInBytes, sizeInBytes);
                }
                _context.UnmapSubresource(d3dBuffer.Buffer, 0);
            }
            else
            {
                D3D11Buffer staging = GetFreeStagingBuffer(sizeInBytes);
                _gd.UpdateBuffer(staging, 0, source, sizeInBytes);
                CopyBuffer(staging, 0, buffer, bufferOffsetInBytes, sizeInBytes);
                _submittedStagingBuffers.Add(staging);
            }
        }

        private D3D11Buffer GetFreeStagingBuffer(uint sizeInBytes)
        {
            foreach (D3D11Buffer buffer in _availableStagingBuffers)
            {
                if (buffer.SizeInBytes >= sizeInBytes)
                {
                    _availableStagingBuffers.Remove(buffer);
                    return buffer;
                }
            }

            DeviceBuffer staging = _gd.ResourceFactory.CreateBuffer(
                new BufferDescription(sizeInBytes, BufferUsage.Staging));

            return Util.AssertSubtype<DeviceBuffer, D3D11Buffer>(staging);
        }

        protected override void CopyBufferCore(DeviceBuffer source, uint sourceOffset, DeviceBuffer destination, uint destinationOffset, uint sizeInBytes)
        {
            D3D11Buffer srcD3D11Buffer = Util.AssertSubtype<DeviceBuffer, D3D11Buffer>(source);
            D3D11Buffer dstD3D11Buffer = Util.AssertSubtype<DeviceBuffer, D3D11Buffer>(destination);

            ResourceRegion region = new ResourceRegion((int)sourceOffset, 0, 0, (int)(sourceOffset + sizeInBytes), 1, 1);

            _context.CopySubresourceRegion(srcD3D11Buffer.Buffer, 0, region, dstD3D11Buffer.Buffer, 0, (int)destinationOffset, 0, 0);
        }

        protected override void CopyTextureCore(
            Texture source,
            uint srcX, uint srcY, uint srcZ,
            uint srcMipLevel,
            uint srcBaseArrayLayer,
            Texture destination,
            uint dstX, uint dstY, uint dstZ,
            uint dstMipLevel,
            uint dstBaseArrayLayer,
            uint width, uint height, uint depth,
            uint layerCount)
        {
            D3D11Texture srcD3D11Texture = Util.AssertSubtype<Texture, D3D11Texture>(source);
            D3D11Texture dstD3D11Texture = Util.AssertSubtype<Texture, D3D11Texture>(destination);

            uint blockSize = FormatHelpers.IsCompressedFormat(source.Format) ? 4u : 1u;
            uint clampedWidth = Math.Max(blockSize, width);
            uint clampedHeight = Math.Max(blockSize, height);

            ResourceRegion region = new ResourceRegion(
                (int)srcX,
                (int)srcY,
                (int)srcZ,
                (int)(srcX + clampedWidth),
                (int)(srcY + clampedHeight),
                (int)(srcZ + depth));

            for (uint i = 0; i < layerCount; i++)
            {
                int srcSubresource = D3D11Util.ComputeSubresource(srcMipLevel, source.MipLevels, srcBaseArrayLayer + i);
                int dstSubresource = D3D11Util.ComputeSubresource(dstMipLevel, destination.MipLevels, dstBaseArrayLayer + i);

                _context.CopySubresourceRegion(
                    srcD3D11Texture.DeviceTexture,
                    srcSubresource,
                    region,
                    dstD3D11Texture.DeviceTexture,
                    dstSubresource,
                    (int)dstX,
                    (int)dstY,
                    (int)dstZ);
            }
        }

        protected override void GenerateMipmapsCore(Texture texture)
        {
            D3D11Texture d3d11Texture = Util.AssertSubtype<Texture, D3D11Texture>(texture);
            ShaderResourceView srv = d3d11Texture.GetFullShaderResourceView();
            _context.GenerateMips(srv);
        }

        public override string Name
        {
            get => _name;
            set
            {
                _name = value;
                _context.DebugName = value;
            }
        }

        internal void OnCompleted()
        {
            DeviceCommandList.Dispose();
            DeviceCommandList = null;

            foreach (D3D11Swapchain sc in _referencedSwapchains)
            {
                sc.RemoveCommandListReference(this);
            }
            _referencedSwapchains.Clear();

            foreach (D3D11Buffer buffer in _submittedStagingBuffers)
            {
                _availableStagingBuffers.Add(buffer);
            }

            _submittedStagingBuffers.Clear();
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                DeviceCommandList?.Dispose();
                _context.Dispose();
                _context1.Dispose();
                _disposed = true;
            }
        }

        private struct BoundTextureInfo
        {
            public int Slot;
            public ShaderStages Stages;
            public uint ResourceSet;
        }
    }
}
