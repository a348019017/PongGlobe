﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Globalization;

namespace PongGlobe.Core
{
    /// <summary>
    /// 三角网的顶点索引结构体,ushort类型，最大支持65000个顶点
    /// </summary>
    public struct TriangleIndicesUnsignedShort : IEquatable<TriangleIndicesUnsignedShort>
    {
        public TriangleIndicesUnsignedShort(ushort ui0, ushort ui1, ushort ui2)
        {
            _ui0 = ui0;
            _ui1 = ui1;
            _ui2 = ui2;
        }

        public TriangleIndicesUnsignedShort(short i0, short i1, short i2)
        {
            if (i0 < 0)
            {
                throw new ArgumentOutOfRangeException("i0");
            }

            if (i1 < 0)
            {
                throw new ArgumentOutOfRangeException("i1");
            }

            if (i2 < 0)
            {
                throw new ArgumentOutOfRangeException("i2");
            }

            _ui0 = (ushort)i0;
            _ui1 = (ushort)i1;
            _ui2 = (ushort)i2;
        }

        public TriangleIndicesUnsignedShort(TriangleIndicesUnsignedShort other)
        {
            _ui0 = other.UI0;
            _ui1 = other.UI1;
            _ui2 = other.UI2;
        }

        public static bool operator ==(TriangleIndicesUnsignedShort left, TriangleIndicesUnsignedShort right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TriangleIndicesUnsignedShort left, TriangleIndicesUnsignedShort right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "i0: {0} i1: {1} i2: {2}", _ui0, _ui1, _ui2);
        }

        public override int GetHashCode()
        {
            return _ui0.GetHashCode() ^ _ui1.GetHashCode() ^ _ui2.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TriangleIndicesUnsignedShort))
                return false;

            return this.Equals((TriangleIndicesUnsignedShort)obj);
        }

        #region IEquatable<TriangleIndices> Members

        public bool Equals(TriangleIndicesUnsignedShort other)
        {
            return
                (_ui0.Equals(other.UI0)) &&
                (_ui1.Equals(other.UI1)) &&
                (_ui2.Equals(other.UI2));
        }

        #endregion

        public short I0
        {
            get { return (short)_ui0; }
        }

        public short I1
        {
            get { return (short)_ui1; }
        }

        public short I2
        {
            get { return (short)_ui2; }
        }

        public ushort UI0
        {
            get { return _ui0; }
        }

        public ushort UI1
        {
            get { return _ui1; }
        }

        public ushort UI2
        {
            get { return _ui2; }
        }

        private ushort _ui0;
        private ushort _ui1;
        private ushort _ui2;
    }
}
