using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace PongGlobe.Core.Extension
{
    /// <summary>
    /// 对system.numberic.matrix4x4的扩展
    /// </summary>
    public static class MatrixExtension
    {
        public static Matrix4x4 InvertOrthonormal(this Matrix4x4 m)
        {
            // This is assumed to contain matrix 3D transformation matrix. The upper 3x3 is transposed, the translation
            // components are multiplied by the transposed-upper-3x3 and negated.

            //m[1] = m[4];
            //m[4] = tmp;

            //tmp = m[2];
            //m[2] = m[8];
            //m[8] = tmp;

            //tmp = m[6];
            //m[6] = m[9];
            //m[9] = tmp;

            float x = m.M41;
            float y = m.M42;
            float z = m.M43;
            x = m.M11 * x + m.M21 * y + m.M31 * z;
            y = m.M12 * x + m.M22 * y + m.M32 * z;
            z = m.M13 * x + m.M23 * y + m.M33 * z;
            //m[3] = -(m[0] * x) - (m[1] * y) - (m[2] * z);
            //m[7] = -(m[4] * x) - (m[5] * y) - (m[6] * z);
            //m[11] = -(m[8] * x) - (m[9] * y) - (m[10] * z);

            //m[12] = 0;
            //m[13] = 0;
            //m[14] = 0;
            //m[15] = 1;

            return new Matrix4x4(m.M11,m.M21,m.M31,m.M14,m.M12,m.M22,m.M32,m.M24,m.M13,m.M23,m.M33,m.M34,-x,-y,-z,m.M44);
            //return default(Matrix4x4);
        }


        /// <summary>
        /// viewmatrix中提取相机坐标
        /// </summary>
        /// <returns></returns>
        public static Vector3 ExtractEyePosition(this Matrix4x4 v)
        {
            var result = new Vector3();
            result.X = -(v.M41 * v.M11) - (v.M42 * v.M21) - (v.M43 * v.M31);
            result.Y = -(v.M41 * v.M12) - (v.M42 * v.M22) - (v.M43 * v.M32);
            result.Z = -(v.M41 * v.M13) - (v.M42 * v.M23) - (v.M43 * v.M33);
            return result;
        }
    }
}
