﻿using System;
using System.Diagnostics;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace Veldrid.D3D11
{
    internal static class D3D11Formats
    {
        internal static Format ToDxgiFormat(PixelFormat format, bool depthFormat)
        {
            switch (format)
            {
                case PixelFormat.R8_UNorm:
                    return Format.R8_UNorm;
                case PixelFormat.R8_SNorm:
                    return Format.R8_SNorm;
                case PixelFormat.R8_UInt:
                    return Format.R8_UInt;
                case PixelFormat.R8_SInt:
                    return Format.R8_SInt;

                case PixelFormat.R16_UNorm:
                    return depthFormat ? Format.R16_Typeless : Format.R16_UNorm;
                case PixelFormat.R16_SNorm:
                    return Format.R16_SNorm;
                case PixelFormat.R16_UInt:
                    return Format.R16_UInt;
                case PixelFormat.R16_SInt:
                    return Format.R16_SInt;
                case PixelFormat.R16_Float:
                    return Format.R16_Float;

                case PixelFormat.R32_UInt:
                    return Format.R32_UInt;
                case PixelFormat.R32_SInt:
                    return Format.R32_SInt;
                case PixelFormat.R32_Float:
                    return depthFormat ? Format.R32_Typeless : Format.R32_Float;

                case PixelFormat.R8_G8_UNorm:
                    return Format.R8G8_UNorm;
                case PixelFormat.R8_G8_SNorm:
                    return Format.R8G8_SNorm;
                case PixelFormat.R8_G8_UInt:
                    return Format.R8G8_UInt;
                case PixelFormat.R8_G8_SInt:
                    return Format.R8G8_SInt;

                case PixelFormat.R16_G16_UNorm:
                    return Format.R16G16_UNorm;
                case PixelFormat.R16_G16_SNorm:
                    return Format.R16G16_SNorm;
                case PixelFormat.R16_G16_UInt:
                    return Format.R16G16_UInt;
                case PixelFormat.R16_G16_SInt:
                    return Format.R16G16_SInt;
                case PixelFormat.R16_G16_Float:
                    return Format.R16G16_Float;

                case PixelFormat.R32_G32_UInt:
                    return Format.R32G32_UInt;
                case PixelFormat.R32_G32_SInt:
                    return Format.R32G32_SInt;
                case PixelFormat.R32_G32_Float:
                    return Format.R32G32_Float;

                case PixelFormat.R8_G8_B8_A8_UNorm:
                    return Format.R8G8B8A8_UNorm;
                case PixelFormat.B8_G8_R8_A8_UNorm:
                    return Format.B8G8R8A8_UNorm;
                case PixelFormat.R8_G8_B8_A8_SNorm:
                    return Format.R8G8B8A8_SNorm;
                case PixelFormat.R8_G8_B8_A8_UInt:
                    return Format.R8G8B8A8_UInt;
                case PixelFormat.R8_G8_B8_A8_SInt:
                    return Format.R8G8B8A8_SInt;

                case PixelFormat.R16_G16_B16_A16_UNorm:
                    return Format.R16G16B16A16_UNorm;
                case PixelFormat.R16_G16_B16_A16_SNorm:
                    return Format.R16G16B16A16_SNorm;
                case PixelFormat.R16_G16_B16_A16_UInt:
                    return Format.R16G16B16A16_UInt;
                case PixelFormat.R16_G16_B16_A16_SInt:
                    return Format.R16G16B16A16_SInt;
                case PixelFormat.R16_G16_B16_A16_Float:
                    return Format.R16G16B16A16_Float;

                case PixelFormat.R32_G32_B32_A32_UInt:
                    return Format.R32G32B32A32_UInt;
                case PixelFormat.R32_G32_B32_A32_SInt:
                    return Format.R32G32B32A32_SInt;
                case PixelFormat.R32_G32_B32_A32_Float:
                    return Format.R32G32B32A32_Float;

                case PixelFormat.BC1_Rgb_UNorm:
                case PixelFormat.BC1_Rgba_UNorm:
                    return Format.BC1_UNorm;
                case PixelFormat.BC2_UNorm:
                    return Format.BC2_UNorm;
                case PixelFormat.BC3_UNorm:
                    return Format.BC3_UNorm;
                case PixelFormat.BC4_UNorm:
                    return Format.BC4_UNorm;
                case PixelFormat.BC4_SNorm:
                    return Format.BC4_SNorm;
                case PixelFormat.BC5_UNorm:
                    return Format.BC5_UNorm;
                case PixelFormat.BC5_SNorm:
                    return Format.BC5_SNorm;
                case PixelFormat.BC7_UNorm:
                    return Format.BC7_UNorm;

                case PixelFormat.D24_UNorm_S8_UInt:
                    Debug.Assert(depthFormat);
                    return Format.R24G8_Typeless;
                case PixelFormat.D32_Float_S8_UInt:
                    Debug.Assert(depthFormat);
                    return Format.R32G8X24_Typeless;

                case PixelFormat.R10_G10_B10_A2_UNorm:
                    return Format.R10G10B10A2_UNorm;
                case PixelFormat.R10_G10_B10_A2_UInt:
                    return Format.R10G10B10A2_UInt;
                case PixelFormat.R11_G11_B10_Float:
                    return Format.R11G11B10_Float;

                case PixelFormat.ETC2_R8_G8_B8_UNorm:
                case PixelFormat.ETC2_R8_G8_B8_A1_UNorm:
                case PixelFormat.ETC2_R8_G8_B8_A8_UNorm:
                    throw new VeldridException("ETC2 formats are not supported on Direct3D 11.");

                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        internal static BindFlags VdToD3D11BindFlags(BufferUsage usage)
        {
            BindFlags flags = BindFlags.None;
            if ((usage & BufferUsage.VertexBuffer) == BufferUsage.VertexBuffer)
            {
                flags |= BindFlags.VertexBuffer;
            }
            if ((usage & BufferUsage.IndexBuffer) == BufferUsage.IndexBuffer)
            {
                flags |= BindFlags.IndexBuffer;
            }
            if ((usage & BufferUsage.UniformBuffer) == BufferUsage.UniformBuffer)
            {
                flags |= BindFlags.ConstantBuffer;
            }
            if ((usage & BufferUsage.StructuredBufferReadOnly) == BufferUsage.StructuredBufferReadOnly
                || (usage & BufferUsage.StructuredBufferReadWrite) == BufferUsage.StructuredBufferReadWrite)
            {
                flags |= BindFlags.ShaderResource;
            }
            if ((usage & BufferUsage.StructuredBufferReadWrite) == BufferUsage.StructuredBufferReadWrite)
            {
                flags |= BindFlags.UnorderedAccess;
            }

            return flags;
        }

        internal static bool IsUnsupportedFormat(PixelFormat format)
        {
            return format == PixelFormat.ETC2_R8_G8_B8_UNorm
                || format == PixelFormat.ETC2_R8_G8_B8_A1_UNorm
                || format == PixelFormat.ETC2_R8_G8_B8_A8_UNorm;
        }

        internal static Format GetViewFormat(Format format)
        {
            switch (format)
            {
                case Format.R16_Typeless:
                    return Format.R16_UNorm;
                case Format.R32_Typeless:
                    return Format.R32_Float;
                case Format.R32G8X24_Typeless:
                    return Format.R32_Float_X8X24_Typeless;
                case Format.R24G8_Typeless:
                    return Format.R24_UNorm_X8_Typeless;
                default:
                    return format;
            }
        }

        internal static BlendOption VdToD3D11BlendOption(BlendFactor factor)
        {
            switch (factor)
            {
                case BlendFactor.Zero:
                    return BlendOption.Zero;
                case BlendFactor.One:
                    return BlendOption.One;
                case BlendFactor.SourceAlpha:
                    return BlendOption.SourceAlpha;
                case BlendFactor.InverseSourceAlpha:
                    return BlendOption.InverseSourceAlpha;
                case BlendFactor.DestinationAlpha:
                    return BlendOption.DestinationAlpha;
                case BlendFactor.InverseDestinationAlpha:
                    return BlendOption.InverseDestinationAlpha;
                case BlendFactor.SourceColor:
                    return BlendOption.SourceColor;
                case BlendFactor.InverseSourceColor:
                    return BlendOption.InverseSourceColor;
                case BlendFactor.DestinationColor:
                    return BlendOption.DestinationColor;
                case BlendFactor.InverseDestinationColor:
                    return BlendOption.InverseDestinationColor;
                case BlendFactor.BlendFactor:
                    return BlendOption.BlendFactor;
                case BlendFactor.InverseBlendFactor:
                    return BlendOption.BlendFactor;
                default:
                    throw Illegal.Value<BlendFactor>();
            }
        }

        internal static Format ToDxgiFormat(IndexFormat format)
        {
            switch (format)
            {
                case IndexFormat.UInt16:
                    return Format.R16_UInt;
                case IndexFormat.UInt32:
                    return Format.R32_UInt;
                default:
                    throw Illegal.Value<IndexFormat>();
            }
        }

        internal static SharpDX.Direct3D11.StencilOperation VdToD3D11StencilOperation(StencilOperation op)
        {
            switch (op)
            {
                case StencilOperation.Keep:
                    return SharpDX.Direct3D11.StencilOperation.Keep;
                case StencilOperation.Zero:
                    return SharpDX.Direct3D11.StencilOperation.Zero;
                case StencilOperation.Replace:
                    return SharpDX.Direct3D11.StencilOperation.Replace;
                case StencilOperation.IncrementAndClamp:
                    return SharpDX.Direct3D11.StencilOperation.IncrementAndClamp;
                case StencilOperation.DecrementAndClamp:
                    return SharpDX.Direct3D11.StencilOperation.DecrementAndClamp;
                case StencilOperation.Invert:
                    return SharpDX.Direct3D11.StencilOperation.Invert;
                case StencilOperation.IncrementAndWrap:
                    return SharpDX.Direct3D11.StencilOperation.Increment;
                case StencilOperation.DecrementAndWrap:
                    return SharpDX.Direct3D11.StencilOperation.Decrement;
                default:
                    throw Illegal.Value<StencilOperation>();
            }
        }

        internal static PixelFormat ToVdFormat(Format format)
        {
            switch (format)
            {
                case Format.R8_UNorm:
                    return PixelFormat.R8_UNorm;
                case Format.R8_SNorm:
                    return PixelFormat.R8_SNorm;
                case Format.R8_UInt:
                    return PixelFormat.R8_UInt;
                case Format.R8_SInt:
                    return PixelFormat.R8_SInt;

                case Format.R16_UNorm:
                case Format.D16_UNorm:
                    return PixelFormat.R16_UNorm;
                case Format.R16_SNorm:
                    return PixelFormat.R16_SNorm;
                case Format.R16_UInt:
                    return PixelFormat.R16_UInt;
                case Format.R16_SInt:
                    return PixelFormat.R16_SInt;
                case Format.R16_Float:
                    return PixelFormat.R16_Float;

                case Format.R32_UInt:
                    return PixelFormat.R32_UInt;
                case Format.R32_SInt:
                    return PixelFormat.R32_SInt;
                case Format.R32_Float:
                case Format.D32_Float:
                    return PixelFormat.R32_Float;

                case Format.R8G8_UNorm:
                    return PixelFormat.R8_G8_UNorm;
                case Format.R8G8_SNorm:
                    return PixelFormat.R8_G8_SNorm;
                case Format.R8G8_UInt:
                    return PixelFormat.R8_G8_UInt;
                case Format.R8G8_SInt:
                    return PixelFormat.R8_G8_SInt;

                case Format.R16G16_UNorm:
                    return PixelFormat.R16_G16_UNorm;
                case Format.R16G16_SNorm:
                    return PixelFormat.R16_G16_SNorm;
                case Format.R16G16_UInt:
                    return PixelFormat.R16_G16_UInt;
                case Format.R16G16_SInt:
                    return PixelFormat.R16_G16_SInt;
                case Format.R16G16_Float:
                    return PixelFormat.R16_G16_Float;

                case Format.R32G32_UInt:
                    return PixelFormat.R32_G32_UInt;
                case Format.R32G32_SInt:
                    return PixelFormat.R32_G32_SInt;
                case Format.R32G32_Float:
                    return PixelFormat.R32_G32_Float;

                case Format.R8G8B8A8_UNorm:
                    return PixelFormat.R8_G8_B8_A8_UNorm;
                case Format.B8G8R8A8_UNorm:
                    return PixelFormat.B8_G8_R8_A8_UNorm;
                case Format.R8G8B8A8_SNorm:
                    return PixelFormat.R8_G8_B8_A8_SNorm;
                case Format.R8G8B8A8_UInt:
                    return PixelFormat.R8_G8_B8_A8_UInt;
                case Format.R8G8B8A8_SInt:
                    return PixelFormat.R8_G8_B8_A8_SInt;

                case Format.R16G16B16A16_UNorm:
                    return PixelFormat.R16_G16_B16_A16_UNorm;
                case Format.R16G16B16A16_SNorm:
                    return PixelFormat.R16_G16_B16_A16_SNorm;
                case Format.R16G16B16A16_UInt:
                    return PixelFormat.R16_G16_B16_A16_UInt;
                case Format.R16G16B16A16_SInt:
                    return PixelFormat.R16_G16_B16_A16_SInt;
                case Format.R16G16B16A16_Float:
                    return PixelFormat.R16_G16_B16_A16_Float;

                case Format.R32G32B32A32_UInt:
                    return PixelFormat.R32_G32_B32_A32_UInt;
                case Format.R32G32B32A32_SInt:
                    return PixelFormat.R32_G32_B32_A32_SInt;
                case Format.R32G32B32A32_Float:
                    return PixelFormat.R32_G32_B32_A32_Float;

                case Format.BC1_UNorm:
                case Format.BC1_Typeless:
                    return PixelFormat.BC1_Rgba_UNorm;
                case Format.BC2_UNorm:
                    return PixelFormat.BC2_UNorm;
                case Format.BC3_UNorm:
                    return PixelFormat.BC3_UNorm;
                case Format.BC4_UNorm:
                    return PixelFormat.BC4_UNorm;
                case Format.BC4_SNorm:
                    return PixelFormat.BC4_SNorm;
                case Format.BC5_UNorm:
                    return PixelFormat.BC5_UNorm;
                case Format.BC5_SNorm:
                    return PixelFormat.BC5_SNorm;
                case Format.BC7_UNorm:
                    return PixelFormat.BC7_UNorm;

                case Format.D24_UNorm_S8_UInt:
                    return PixelFormat.D24_UNorm_S8_UInt;
                case Format.D32_Float_S8X24_UInt:
                    return PixelFormat.D32_Float_S8_UInt;

                case Format.R10G10B10A2_UInt:
                    return PixelFormat.R10_G10_B10_A2_UInt;
                case Format.R10G10B10A2_UNorm:
                    return PixelFormat.R10_G10_B10_A2_UNorm;
                case Format.R11G11B10_Float:
                    return PixelFormat.R11_G11_B10_Float;
                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        internal static TextureSampleCount ToVdSampleCount(SampleDescription sampleDescription)
        {
            switch (sampleDescription.Count)
            {
                case 1: return TextureSampleCount.Count1;
                case 2: return TextureSampleCount.Count2;
                case 4: return TextureSampleCount.Count4;
                case 8: return TextureSampleCount.Count8;
                case 16: return TextureSampleCount.Count16;
                case 32: return TextureSampleCount.Count32;
                default: throw new VeldridException("Unsupported multisample count: " + sampleDescription.Count);
            }
        }

        internal static BlendOperation VdToD3D11BlendOperation(BlendFunction function)
        {
            switch (function)
            {
                case BlendFunction.Add:
                    return BlendOperation.Add;
                case BlendFunction.Subtract:
                    return BlendOperation.Subtract;
                case BlendFunction.ReverseSubtract:
                    return BlendOperation.ReverseSubtract;
                case BlendFunction.Minimum:
                    return BlendOperation.Minimum;
                case BlendFunction.Maximum:
                    return BlendOperation.Maximum;
                default:
                    throw Illegal.Value<BlendFunction>();
            }
        }

        internal static Filter ToD3D11Filter(SamplerFilter filter, bool isComparison)
        {
            switch (filter)
            {
                case SamplerFilter.MinPoint_MagPoint_MipPoint:
                    return isComparison ? Filter.ComparisonMinMagMipPoint : Filter.MinMagMipPoint;
                case SamplerFilter.MinPoint_MagPoint_MipLinear:
                    return isComparison ? Filter.ComparisonMinMagPointMipLinear : Filter.MinMagPointMipLinear;
                case SamplerFilter.MinPoint_MagLinear_MipPoint:
                    return isComparison ? Filter.ComparisonMinPointMagLinearMipPoint : Filter.MinPointMagLinearMipPoint;
                case SamplerFilter.MinPoint_MagLinear_MipLinear:
                    return isComparison ? Filter.ComparisonMinPointMagMipLinear : Filter.MinPointMagMipLinear;
                case SamplerFilter.MinLinear_MagPoint_MipPoint:
                    return isComparison ? Filter.ComparisonMinLinearMagMipPoint : Filter.MinLinearMagMipPoint;
                case SamplerFilter.MinLinear_MagPoint_MipLinear:
                    return isComparison ? Filter.ComparisonMinLinearMagPointMipLinear : Filter.MinLinearMagPointMipLinear;
                case SamplerFilter.MinLinear_MagLinear_MipPoint:
                    return isComparison ? Filter.ComparisonMinMagLinearMipPoint : Filter.MinMagLinearMipPoint;
                case SamplerFilter.MinLinear_MagLinear_MipLinear:
                    return isComparison ? Filter.ComparisonMinMagMipLinear : Filter.MinMagMipLinear;
                case SamplerFilter.Anisotropic:
                    return isComparison ? Filter.ComparisonAnisotropic : Filter.Anisotropic;
                default:
                    throw Illegal.Value<SamplerFilter>();
            }
        }

        internal static SharpDX.Direct3D11.MapMode VdToD3D11MapMode(bool isDynamic, MapMode mode)
        {
            switch (mode)
            {
                case MapMode.Read:
                    return SharpDX.Direct3D11.MapMode.Read;
                case MapMode.Write:
                    return isDynamic ? SharpDX.Direct3D11.MapMode.WriteDiscard : SharpDX.Direct3D11.MapMode.Write;
                case MapMode.ReadWrite:
                    return SharpDX.Direct3D11.MapMode.ReadWrite;
                default:
                    throw Illegal.Value<MapMode>();
            }
        }

        internal static SharpDX.Direct3D.PrimitiveTopology VdToD3D11PrimitiveTopology(PrimitiveTopology primitiveTopology)
        {
            switch (primitiveTopology)
            {
                case PrimitiveTopology.TriangleList:
                    return SharpDX.Direct3D.PrimitiveTopology.TriangleList;
                case PrimitiveTopology.TriangleStrip:
                    return SharpDX.Direct3D.PrimitiveTopology.TriangleStrip;
                case PrimitiveTopology.LineList:
                    return SharpDX.Direct3D.PrimitiveTopology.LineList;
                case PrimitiveTopology.LineStrip:
                    return SharpDX.Direct3D.PrimitiveTopology.LineStrip;
                case PrimitiveTopology.PointList:
                    return SharpDX.Direct3D.PrimitiveTopology.PointList;
                default:
                    throw Illegal.Value<PrimitiveTopology>();
            }
        }

        internal static FillMode VdToD3D11FillMode(PolygonFillMode fillMode)
        {
            switch (fillMode)
            {
                case PolygonFillMode.Solid:
                    return FillMode.Solid;
                case PolygonFillMode.Wireframe:
                    return FillMode.Wireframe;
                default:
                    throw Illegal.Value<PolygonFillMode>();
            }
        }

        internal static CullMode VdToD3D11CullMode(FaceCullMode cullingMode)
        {
            switch (cullingMode)
            {
                case FaceCullMode.Back:
                    return CullMode.Back;
                case FaceCullMode.Front:
                    return CullMode.Front;
                case FaceCullMode.None:
                    return CullMode.None;
                default:
                    throw Illegal.Value<FaceCullMode>();
            }
        }

        internal static Format ToDxgiFormat(VertexElementFormat format)
        {
            switch (format)
            {
                case VertexElementFormat.Float1:
                    return Format.R32_Float;
                case VertexElementFormat.Float2:
                    return Format.R32G32_Float;
                case VertexElementFormat.Float3:
                    return Format.R32G32B32_Float;
                case VertexElementFormat.Float4:
                    return Format.R32G32B32A32_Float;
                case VertexElementFormat.Byte2_Norm:
                    return Format.R8G8_UNorm;
                case VertexElementFormat.Byte2:
                    return Format.R8G8_UInt;
                case VertexElementFormat.Byte4_Norm:
                    return Format.R8G8B8A8_UNorm;
                case VertexElementFormat.Byte4:
                    return Format.R8G8B8A8_UInt;
                case VertexElementFormat.SByte2_Norm:
                    return Format.R8G8_SNorm;
                case VertexElementFormat.SByte2:
                    return Format.R8G8_SInt;
                case VertexElementFormat.SByte4_Norm:
                    return Format.R8G8B8A8_SNorm;
                case VertexElementFormat.SByte4:
                    return Format.R8G8B8A8_SInt;
                case VertexElementFormat.UShort2_Norm:
                    return Format.R16G16_UNorm;
                case VertexElementFormat.UShort2:
                    return Format.R16G16_UInt;
                case VertexElementFormat.UShort4_Norm:
                    return Format.R16G16B16A16_UNorm;
                case VertexElementFormat.UShort4:
                    return Format.R16G16B16A16_UInt;
                case VertexElementFormat.Short2_Norm:
                    return Format.R16G16_SNorm;
                case VertexElementFormat.Short2:
                    return Format.R16G16_SInt;
                case VertexElementFormat.Short4_Norm:
                    return Format.R16G16B16A16_SNorm;
                case VertexElementFormat.Short4:
                    return Format.R16G16B16A16_SInt;
                case VertexElementFormat.UInt1:
                    return Format.R32_UInt;
                case VertexElementFormat.UInt2:
                    return Format.R32G32_UInt;
                case VertexElementFormat.UInt3:
                    return Format.R32G32B32_UInt;
                case VertexElementFormat.UInt4:
                    return Format.R32G32B32A32_UInt;
                case VertexElementFormat.Int1:
                    return Format.R32_SInt;
                case VertexElementFormat.Int2:
                    return Format.R32G32_SInt;
                case VertexElementFormat.Int3:
                    return Format.R32G32B32_SInt;
                case VertexElementFormat.Int4:
                    return Format.R32G32B32A32_SInt;

                default:
                    throw Illegal.Value<VertexElementFormat>();
            }
        }

        internal static Comparison VdToD3D11Comparison(ComparisonKind comparisonKind)
        {
            switch (comparisonKind)
            {
                case ComparisonKind.Never:
                    return Comparison.Never;
                case ComparisonKind.Less:
                    return Comparison.Less;
                case ComparisonKind.Equal:
                    return Comparison.Equal;
                case ComparisonKind.LessEqual:
                    return Comparison.LessEqual;
                case ComparisonKind.Greater:
                    return Comparison.Greater;
                case ComparisonKind.NotEqual:
                    return Comparison.NotEqual;
                case ComparisonKind.GreaterEqual:
                    return Comparison.GreaterEqual;
                case ComparisonKind.Always:
                    return Comparison.Always;
                default:
                    throw Illegal.Value<ComparisonKind>();
            }
        }

        internal static TextureAddressMode VdToD3D11AddressMode(SamplerAddressMode mode)
        {
            switch (mode)
            {
                case SamplerAddressMode.Wrap:
                    return TextureAddressMode.Wrap;
                case SamplerAddressMode.Mirror:
                    return TextureAddressMode.Mirror;
                case SamplerAddressMode.Clamp:
                    return TextureAddressMode.Clamp;
                case SamplerAddressMode.Border:
                    return TextureAddressMode.Border;
                default:
                    throw Illegal.Value<SamplerAddressMode>();
            }
        }

        internal static Format GetDepthFormat(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R32_Float:
                    return Format.D32_Float;
                case PixelFormat.R16_UNorm:
                    return Format.D16_UNorm;
                case PixelFormat.D24_UNorm_S8_UInt:
                    return Format.D24_UNorm_S8_UInt;
                case PixelFormat.D32_Float_S8_UInt:
                    return Format.D32_Float_S8X24_UInt;
                default:
                    throw new VeldridException("Invalid depth texture format: " + format);
            }
        }
    }
}
