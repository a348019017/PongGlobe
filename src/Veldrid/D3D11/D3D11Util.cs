﻿using SharpDX.Direct3D11;

namespace Veldrid.D3D11
{
    internal static class D3D11Util
    {
        public static int ComputeSubresource(uint mipLevel, uint mipLevelCount, uint arrayLayer)
        {
            return (int)((arrayLayer * mipLevelCount) + mipLevel);
        }

        internal static ShaderResourceViewDescription GetSrvDesc(
            D3D11Texture tex,
            uint baseMipLevel,
            uint levelCount,
            uint baseArrayLayer,
            uint layerCount)
        {
            ShaderResourceViewDescription srvDesc = new ShaderResourceViewDescription();
            srvDesc.Format = D3D11Formats.GetViewFormat(tex.DxgiFormat);

            if ((tex.Usage & TextureUsage.Cubemap) == TextureUsage.Cubemap)
            {
                if (tex.ArrayLayers == 1)
                {
                    srvDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.TextureCube;
                    srvDesc.TextureCube.MostDetailedMip = (int)baseMipLevel;
                    srvDesc.TextureCube.MipLevels = (int)levelCount;
                }
                else
                {
                    srvDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.TextureCubeArray;
                    srvDesc.TextureCubeArray.MostDetailedMip = (int)baseMipLevel;
                    srvDesc.TextureCubeArray.MipLevels = (int)levelCount;
                    srvDesc.TextureCubeArray.First2DArrayFace = (int)baseArrayLayer;
                    srvDesc.TextureCubeArray.CubeCount = (int)tex.ArrayLayers;
                }
            }
            else if (tex.Depth == 1)
            {
                if (tex.ArrayLayers == 1)
                {
                    srvDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D;
                    srvDesc.Texture2D.MostDetailedMip = (int)baseMipLevel;
                    srvDesc.Texture2D.MipLevels = (int)levelCount;
                }
                else
                {
                    srvDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2DArray;
                    srvDesc.Texture2DArray.MostDetailedMip = (int)baseMipLevel;
                    srvDesc.Texture2DArray.MipLevels = (int)levelCount;
                    srvDesc.Texture2DArray.FirstArraySlice = (int)baseArrayLayer;
                    srvDesc.Texture2DArray.ArraySize = (int)layerCount;
                }
            }
            else
            {
                srvDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture3D;
                srvDesc.Texture3D.MostDetailedMip = (int)baseMipLevel;
                srvDesc.Texture3D.MipLevels = (int)levelCount;
            }

            return srvDesc;
        }
    }
}
