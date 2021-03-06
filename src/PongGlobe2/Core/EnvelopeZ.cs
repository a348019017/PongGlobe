﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace PongGlobe.Core
{
    /// <summary>
    /// 从xna上拷贝的代码，用于表示一个外包矩形，同时包含深度Z
    /// </summary>
    public struct Envelope2DZ : IEquatable<Envelope2DZ>
    {

        #region Public Fields

     
        public Vector2 Min;

        public float Z;
       
        public Vector2 Max;

        public const int CornerCount = 8;

        #endregion Public Fields


        #region Public Constructors

        public Envelope2DZ(Vector2 min, Vector2 max,float z)
        {
            this.Min = min;
            this.Max = max;
            this.Z = z;
        }

        public Envelope2DZ(Vector3 min, Vector3 max)
        {
            this.Min = new Vector2(min.X,min.Y);
            this.Max = new Vector2(max.X,max.Y);
            this.Z = min.Z;
        }

        #endregion Public Constructors


        #region Public Methods



        public bool Equals(Envelope2DZ other)
        {
            return (this.Min == other.Min) && (this.Max == other.Max);
        }

        public override bool Equals(object obj)
        {
            return (obj is Envelope2DZ) ? this.Equals((Envelope2DZ)obj) : false;
        }

       

        

        public override int GetHashCode()
        {
            return this.Min.GetHashCode() + this.Max.GetHashCode();
        }

        /// <summary>
        /// 仅实现了一个Intersect接口
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public bool Intersects(Envelope2DZ box)
        {
            bool result;
            Intersects(ref box, out result);
            return result;
        }

        public void Intersects(ref Envelope2DZ box, out bool result)
        {
            result= Math.Min(box.Max.X, this.Max.X) >= Math.Max(this.Min.X, box.Min.X) && Math.Min(this.Max.Y, box.Max.Y) >= Math.Max(this.Min.Y, box.Min.Y);
            return;
        }

       

        

        #endregion Public Methods
    }

   
}
