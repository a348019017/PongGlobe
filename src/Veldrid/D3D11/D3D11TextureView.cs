﻿using SharpDX.Direct3D11;
using System;

namespace Veldrid.D3D11
{
    internal class D3D11TextureView : TextureView
    {
        private string _name;
        private bool _srvOwned;

        public ShaderResourceView ShaderResourceView { get; }
        public UnorderedAccessView UnorderedAccessView { get; }

        public D3D11TextureView(Device device, ref TextureViewDescription description)
            : base(ref description)
        {
            D3D11Texture d3dTex = Util.AssertSubtype<Texture, D3D11Texture>(description.Target);

            if (BaseMipLevel == 0 && MipLevels == Target.MipLevels
                || BaseArrayLayer == 0 && ArrayLayers == Target.ArrayLayers)
            {
                ShaderResourceView = d3dTex.GetFullShaderResourceView();
            }
            else
            {
                _srvOwned = true;
                ShaderResourceViewDescription srvDesc = D3D11Util.GetSrvDesc(
                    d3dTex,
                    description.BaseMipLevel,
                    description.MipLevels,
                    description.BaseArrayLayer,
                    description.ArrayLayers);
                ShaderResourceView = new ShaderResourceView(device, d3dTex.DeviceTexture, srvDesc);
            }

            if ((d3dTex.Usage & TextureUsage.Storage) == TextureUsage.Storage)
            {
                UnorderedAccessViewDescription uavDesc = new UnorderedAccessViewDescription();
                uavDesc.Format = D3D11Formats.GetViewFormat(d3dTex.DxgiFormat);

                if ((d3dTex.Usage & TextureUsage.Cubemap) == TextureUsage.Cubemap)
                {
                    throw new NotSupportedException();
                }
                else if (d3dTex.Depth == 1)
                {
                    if (d3dTex.ArrayLayers == 1)
                    {
                        uavDesc.Dimension = UnorderedAccessViewDimension.Texture2D;
                        uavDesc.Texture2D.MipSlice = (int)description.BaseMipLevel;
                    }
                    else
                    {
                        uavDesc.Dimension = UnorderedAccessViewDimension.Texture2DArray;
                        uavDesc.Texture2DArray.MipSlice = (int)description.BaseMipLevel;
                        uavDesc.Texture2DArray.FirstArraySlice = (int)description.BaseArrayLayer;
                        uavDesc.Texture2DArray.ArraySize = (int)description.ArrayLayers;
                    }
                }
                else
                {
                    uavDesc.Dimension = UnorderedAccessViewDimension.Texture3D;
                    uavDesc.Texture3D.MipSlice = (int)description.BaseMipLevel;
                    uavDesc.Texture3D.FirstWSlice = (int)description.BaseArrayLayer;
                    uavDesc.Texture3D.WSize = (int)description.ArrayLayers;
                }

                UnorderedAccessView = new UnorderedAccessView(device, d3dTex.DeviceTexture, uavDesc);
            }
        }

        public override string Name
        {
            get => _name;
            set
            {
                _name = value;
                if (ShaderResourceView != null)
                {
                    ShaderResourceView.DebugName = value + "_SRV";
                }
                if (UnorderedAccessView != null)
                {
                    UnorderedAccessView.DebugName = value + "_UAV";
                }
            }
        }

        public override void Dispose()
        {
            if (_srvOwned)
            {
                ShaderResourceView.Dispose();
            }
            UnorderedAccessView?.Dispose();
        }
    }
}