﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
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
    /// 描述一个边，如多边形的某条边的类
    /// </summary>
    public struct Edge : IEquatable<Edge>
    {
        public Edge(int index0, int index1)
        {
            _index0 = index0;
            _index1 = index1;
        }

        public int Index0 { get { return _index0; } }
        public int Index1 { get { return _index1; } }
        
        public static bool operator ==(Edge left, Edge right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Edge left, Edge right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0}, {1}", _index0, _index1);
        }

        public override int GetHashCode()
        {
            return _index0.GetHashCode() ^ _index1.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Edge))
                return false;

            return this.Equals((Edge)obj);
        }

        #region IEquatable<Edge> Members

        public bool Equals(Edge other)
        {
            return (_index0 == other._index0) && (_index1 == other._index1);
        }

        #endregion

        private readonly int _index0;
        private readonly int _index1;
    }
}
