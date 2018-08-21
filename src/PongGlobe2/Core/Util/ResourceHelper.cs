using System;
using System.Collections.Generic;
using System.Text;
using Veldrid;
using System.IO;
using System.Reflection;

namespace PongGlobe.Core.Util
{
    /// <summary>
    /// 管理资源等加载的帮助类
    /// </summary>
    public class ResourceHelper
    {
        /// <summary>
        /// 从指定程序集获取嵌入式资源
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="embedName"></param>
        /// <param name="GraphicsDevice"></param>
        /// <param name="assm"></param>
        /// <returns></returns>
        public static Shader LoadEmbbedShader(ShaderStages stage, string embedName,GraphicsDevice GraphicsDevice,Assembly assm)
        {
            byte[] shaderBytes = ReadEmbeddedAssetBytes(embedName,assm);
            string entryPoint = stage == ShaderStages.Vertex ? "VS" : "FS";
            return GraphicsDevice.ResourceFactory.CreateShader(new ShaderDescription(stage, shaderBytes, entryPoint));
        }
        public static byte[] ReadEmbeddedAssetBytes(string name,Assembly ass)
        {
            using (Stream stream = ass.GetManifestResourceStream(name))
            {
                byte[] bytes = new byte[stream.Length];
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    stream.CopyTo(ms);
                    return bytes;
                }
            }
        }

       


    }
}
