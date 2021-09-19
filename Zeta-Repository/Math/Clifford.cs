using System;
using System.Collections.Generic;
using System.Text;
using Zeta.Generics;

namespace Zeta.Math
{
    public class CMultivector2<T>
    {
        AlgebraicOperator<T> Op;
        public T Scalar;
        CVector2<T> Vector;
        CBivector2<T> Bivector;

        public T x { 
            get => Vector.x;
            set => Vector.x = value;
        }
        public T y
        {
            get => Vector.y;
            set => Vector.y = value;
        }
        public T xy
        {
            get => Bivector.xy;
            set => Bivector.xy = value;
        }

        public CMultivector2(AlgebraicOperator<T> op, T scalar, CVector2<T> vector, CBivector2<T> bivector)
        {
            Op = op;
            Scalar = scalar;
            Vector = vector;
            Bivector = bivector;
        }
        public CMultivector2(AlgebraicOperator<T> op, T scalar, T x, T y, T xy)
        {
            Op = op;
            Scalar = scalar;
            Vector = new CVector2<T>(op, x, y);
            Bivector = new CBivector2<T>(op, xy);
        }

        #region Basic Global Operations
        public static CMultivector2<T> Add(CMultivector2<T> v1, CMultivector2<T> v2, AlgebraicOperator<T> overrideOperator = null)
        {
            var op = overrideOperator ?? v1.Op;
            return new CMultivector2<T>(op, op.Add(v1.Scalar, v2.Scalar), op.Add(v1.x, v2.x), op.Add(v1.y, v2.y), op.Add(v1.xy, v2.xy));
        }
        public static CMultivector2<T> ScalarMult(T c, CMultivector2<T> v, AlgebraicOperator<T> overrideOperator = null)
        {
            var op = overrideOperator ?? v.Op;
            return new CMultivector2<T>(op, op.Mult(c, v.Scalar), op.Mult(c, v.x), op.Mult(c, v.y), op.Mult(c, v.xy));
        }
        public static CMultivector2<T> Negate(CMultivector2<T> v, AlgebraicOperator<T> overrideOperator = null)
        {
            var op = overrideOperator ?? v.Op;
            return ScalarMult(op.Negate(op.Unit), v);
        }
        #endregion

        public void Add(CMultivector2<T> v)
        {
            Scalar = Op.Add(Scalar, v.Scalar);
            x = Op.Add(x, v.x);
            y = Op.Add(y, v.y);
            xy = Op.Add(xy, v.xy);
        }
        public void ScalarMult(T c)
        {
            Scalar = Op.Mult(c, Scalar);
            x = Op.Mult(c, x);
            y = Op.Mult(c, y);
            xy = Op.Mult(c, xy);
        }
        public void Negate() => ScalarMult(Op.Negate(Op.Unit));

        #region Geometric Operations

        public static CMultivector2<T> GeometricProduct(CMultivector2<T> v1, CMultivector2<T> v2, AlgebraicOperator<T> overrideOperator = null)
        {
            var op = overrideOperator ?? v1.Op;

            var scalar = AddAll(op,
                op.Mult(v1.Scalar, v2.Scalar),
                op.Mult(v1.x, v2.x),
                op.Mult(v1.y, v2.y),
                op.Mult(v1.xy, v2.xy)
                );
            var x = AddAll(op,
                op.Mult(v1.Scalar, v2.x),
                op.Mult(v1.x, v2.Scalar),
                op.Negate(op.Mult(v1.y, v2.xy)),
                op.Mult(v1.xy, v2.y)
                );
            var y = AddAll(op,
                op.Mult(v1.Scalar, v2.y),
                op.Mult(v1.y, v2.Scalar),
                op.Mult(v1.x, v2.xy),
                op.Negate(op.Mult(v1.xy, v2.x))
                );
            var xy = AddAll(op,
                op.Mult(v1.Scalar, v2.xy),
                op.Mult(v1.xy, v2.Scalar),
                op.Mult(v1.x, v2.y),
                op.Negate(op.Mult(v1.y, v2.x))
                );

            return new CMultivector2<T>(op, scalar, x, y, xy);
        }

        #endregion

        public static T AddAll(AlgebraicOperator<T> op, params T[] arr)
        {
            T val = op.Zero;
            foreach(var v in arr)
            {
                val = op.Add(val, v);
            }
            return val;
        }
    }

    public class CVector2<T>
    {
        public AlgebraicOperator<T> Op { get; private set; }
        public T x;
        public T y;

        public CVector2(AlgebraicOperator<T> op)
        {
            Op = op;
            x = Op.Zero;
            y = Op.Zero;
        }
        public CVector2(AlgebraicOperator<T> op, T _x, T _y)
        {
            Op = op;
            x = _x;
            y = _y;
        }

        #region Basic Global Operations
        public static CVector2<T> Add(CVector2<T> v1, CVector2<T> v2, AlgebraicOperator<T> overrideOperator = null)
        {
            var op = overrideOperator ?? v1.Op;
            return new CVector2<T>(op, op.Add(v1.x, v2.x), op.Add(v1.y, v2.y));
        }
        public static CVector2<T> ScalarMult(T c, CVector2<T> v, AlgebraicOperator<T> overrideOperator = null)
        {
            var op = overrideOperator ?? v.Op;
            return new CVector2<T>(op, op.Mult(c, v.x), op.Mult(c, v.y));
        }
        public static CVector2<T> Negate(CVector2<T> v, AlgebraicOperator<T> overrideOperator = null)
        {
            var op = overrideOperator ?? v.Op;
            return ScalarMult(op.Negate(op.Unit), v);
        }
        #endregion

        #region Vector Operations

        public T SqrMagnitude => Op.Add(Op.Mult(x, x), Op.Mult(y, y));
        public T Magnitude => Op.Sqrt(SqrMagnitude);
        public CVector2<T> Inverse => ScalarMult(Op.Inverse(SqrMagnitude), this);


        public static T Dot(CVector2<T> v1, CVector2<T> v2, AlgebraicOperator<T> overrideOperator = null)
        {
            var op = overrideOperator ?? v1.Op;
            return op.Add(op.Mult(v1.x, v2.x), op.Mult(v1.y, v2.y));
        }
        public static CBivector2<T> Wedge(CVector2<T> v1, CVector2<T> v2, AlgebraicOperator<T> overrideOperator = null)
        {
            var op = overrideOperator ?? v1.Op;
            return new CBivector2<T>(op, op.Add(op.Mult(v1.x, v2.y), op.Negate(op.Mult(v1.y, v2.x))));
        }

        #endregion

        public void Add(CVector2<T> v)
        {
            x = Op.Add(x, v.x);
            y = Op.Add(y, v.y);
        }
        public void ScalarMult(T c)
        {
            x = Op.Mult(c, x);
            y = Op.Mult(c, y);
        }
        public void Negate() => ScalarMult(Op.Negate(Op.Unit));
    }

    public class CBivector2<T>
    {
        public AlgebraicOperator<T> Op { get; private set; }
        public T xy;

        public CBivector2(AlgebraicOperator<T> op)
        {
            Op = op;
            xy = Op.Zero;
        }
        public CBivector2(AlgebraicOperator<T> op, T _xy)
        {
            Op = op;
            xy = _xy;
        }

        #region Basic Global Operations
        public static CBivector2<T> Add(CBivector2<T> v1, CBivector2<T> v2, AlgebraicOperator<T> overrideOperator = null)
        {
            var op = overrideOperator ?? v1.Op;
            return new CBivector2<T>(op, op.Add(v1.xy, v2.xy));
        }
        public static CBivector2<T> ScalarMult(T c, CBivector2<T> v, AlgebraicOperator<T> overrideOperator = null)
        {
            var op = overrideOperator ?? v.Op;
            return new CBivector2<T>(op, op.Mult(c, v.xy));
        }
        public static CBivector2<T> Negate(CBivector2<T> v, AlgebraicOperator<T> overrideOperator = null)
        {
            var op = overrideOperator ?? v.Op;
            return ScalarMult(op.Negate(op.Unit), v);
        }
        #endregion

        public void Add(CVector2<T> v)
        {
            xy = Op.Add(xy, v.x);
        }
        public void ScalarMult(T c)
        {
            xy = Op.Mult(c, xy);
        }
        public void Negate() => ScalarMult(Op.Negate(Op.Unit));
    }
}
