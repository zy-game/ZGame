using BEPUutilities;
using FixMath.NET;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace BEPUConvertion
{
    /// <summary>
    /// Helps convert between XNA math types and the BEPUphysics replacement math types.
    /// A version of this converter could be created for other platforms to ease the integration of the engine.
    /// </summary>
    public static class MathConverter
    {
        //Vector2
        public static UnityEngine.Vector2 Convert(BEPUutilities.Vector2 bepuVector)
        {
            UnityEngine.Vector2 toReturn;
            toReturn.x = (float)bepuVector.X;
            toReturn.y = (float)bepuVector.Y;
            return toReturn;
        }

        public static void Convert(ref BEPUutilities.Vector2 bepuVector, out UnityEngine.Vector2 xnaVector)
        {
            xnaVector.x = (float)bepuVector.X;
            xnaVector.y = (float)bepuVector.Y;
        }

        public static BEPUutilities.Vector2 Convert(UnityEngine.Vector2 xnaVector)
        {
            BEPUutilities.Vector2 toReturn;
            toReturn.X = (Fix64)xnaVector.x;
            toReturn.Y = (Fix64)xnaVector.y;
            return toReturn;
        }

        public static void Convert(ref UnityEngine.Vector2 xnaVector, out BEPUutilities.Vector2 bepuVector)
        {
            bepuVector.X = (Fix64)xnaVector.x;
            bepuVector.Y = (Fix64)xnaVector.y;
        }

        //Vector3
        public static UnityEngine.Vector3 Convert(BEPUutilities.Vector3 bepuVector)
        {
            UnityEngine.Vector3 toReturn;
            toReturn.x = (float)bepuVector.X;
            toReturn.y = (float)bepuVector.Y;
            toReturn.z = (float)bepuVector.Z;
            return toReturn;
        }

        public static void Convert(ref BEPUutilities.Vector3 bepuVector, out UnityEngine.Vector3 xnaVector)
        {
            xnaVector.x = (float)bepuVector.X;
            xnaVector.y = (float)bepuVector.Y;
            xnaVector.z = (float)bepuVector.Z;
        }

        public static BEPUutilities.Vector3 Convert(UnityEngine.Vector3 xnaVector)
        {
            BEPUutilities.Vector3 toReturn;
            toReturn.X = (Fix64)xnaVector.x;
            toReturn.Y = (Fix64)xnaVector.y;
            toReturn.Z = (Fix64)xnaVector.z;
            return toReturn;
        }

        public static void Convert(ref UnityEngine.Vector3 xnaVector, out BEPUutilities.Vector3 bepuVector)
        {
            bepuVector.X = (Fix64)xnaVector.x;
            bepuVector.Y = (Fix64)xnaVector.y;
            bepuVector.Z = (Fix64)xnaVector.z;
        }

        public static UnityEngine.Vector3[] Convert(BEPUutilities.Vector3[] bepuVectors)
        {
            UnityEngine.Vector3[] xnaVectors = new UnityEngine.Vector3[bepuVectors.Length];
            for (int i = 0; i < bepuVectors.Length; i++)
            {
                Convert(ref bepuVectors[i], out xnaVectors[i]);
            }

            return xnaVectors;
        }

        public static BEPUutilities.Vector3[] Convert(UnityEngine.Vector3[] xnaVectors)
        {
            var bepuVectors = new BEPUutilities.Vector3[xnaVectors.Length];
            for (int i = 0; i < xnaVectors.Length; i++)
            {
                Convert(ref xnaVectors[i], out bepuVectors[i]);
            }

            return bepuVectors;
        }

        //Matrix
        public static UnityEngine.Matrix4x4 Convert(BEPUutilities.Matrix matrix)
        {
            UnityEngine.Matrix4x4 toReturn;
            Convert(ref matrix, out toReturn);
            return toReturn;
        }

        public static BEPUutilities.Matrix Convert(UnityEngine.Matrix4x4 matrix)
        {
            BEPUutilities.Matrix toReturn;
            Convert(ref matrix, out toReturn);
            return toReturn;
        }

        public static void Convert(ref BEPUutilities.Matrix matrix, out UnityEngine.Matrix4x4 xnaMatrix)
        {
            xnaMatrix.m00 = (float)matrix.M11;
            xnaMatrix.m01 = (float)matrix.M12;
            xnaMatrix.m02 = (float)matrix.M13;
            xnaMatrix.m03 = (float)matrix.M14;
            xnaMatrix.m10 = (float)matrix.M21;
            xnaMatrix.m11 = (float)matrix.M22;
            xnaMatrix.m12 = (float)matrix.M23;
            xnaMatrix.m13 = (float)matrix.M24;
            xnaMatrix.m20 = (float)matrix.M31;
            xnaMatrix.m21 = (float)matrix.M32;
            xnaMatrix.m22 = (float)matrix.M33;
            xnaMatrix.m23 = (float)matrix.M34;
            xnaMatrix.m30 = (float)matrix.M41;
            xnaMatrix.m31 = (float)matrix.M42;
            xnaMatrix.m32 = (float)matrix.M43;
            xnaMatrix.m33 = (float)matrix.M44;
        }

        public static void Convert(ref UnityEngine.Matrix4x4 matrix, out BEPUutilities.Matrix bepuMatrix)
        {
            bepuMatrix.M11 = (Fix64)matrix.m00;
            bepuMatrix.M12 = (Fix64)matrix.m01;
            bepuMatrix.M13 = (Fix64)matrix.m02;
            bepuMatrix.M14 = (Fix64)matrix.m03;
            bepuMatrix.M21 = (Fix64)matrix.m10;
            bepuMatrix.M22 = (Fix64)matrix.m11;
            bepuMatrix.M23 = (Fix64)matrix.m12;
            bepuMatrix.M24 = (Fix64)matrix.m13;
            bepuMatrix.M31 = (Fix64)matrix.m20;
            bepuMatrix.M32 = (Fix64)matrix.m21;
            bepuMatrix.M33 = (Fix64)matrix.m22;
            bepuMatrix.M34 = (Fix64)matrix.m23;
            bepuMatrix.M41 = (Fix64)matrix.m30;
            bepuMatrix.M42 = (Fix64)matrix.m31;
            bepuMatrix.M43 = (Fix64)matrix.m32;
            bepuMatrix.M44 = (Fix64)matrix.m33;
        }

        public static UnityEngine.Matrix4x4 Convert(BEPUutilities.Matrix3x3 matrix)
        {
            UnityEngine.Matrix4x4 toReturn;
            Convert(ref matrix, out toReturn);
            return toReturn;
        }

        public static void Convert(ref BEPUutilities.Matrix3x3 matrix, out UnityEngine.Matrix4x4 xnaMatrix)
        {
            xnaMatrix.m00 = (float)matrix.M11;
            xnaMatrix.m01 = (float)matrix.M12;
            xnaMatrix.m02 = (float)matrix.M13;
            xnaMatrix.m03 = 0;
            xnaMatrix.m10 = (float)matrix.M21;
            xnaMatrix.m11 = (float)matrix.M22;
            xnaMatrix.m12 = (float)matrix.M23;
            xnaMatrix.m13 = 0;
            xnaMatrix.m20 = (float)matrix.M31;
            xnaMatrix.m21 = (float)matrix.M32;
            xnaMatrix.m22 = (float)matrix.M33;
            xnaMatrix.m23 = 0;
            xnaMatrix.m30 = 0;
            xnaMatrix.m31 = 0;
            xnaMatrix.m32 = 0;
            xnaMatrix.m33 = 1;
        }

        public static void Convert(ref UnityEngine.Matrix4x4 matrix, out BEPUutilities.Matrix3x3 bepuMatrix)
        {
            bepuMatrix.M11 = (Fix64)matrix.m00;
            bepuMatrix.M12 = (Fix64)matrix.m01;
            bepuMatrix.M13 = (Fix64)matrix.m02;
            bepuMatrix.M21 = (Fix64)matrix.m10;
            bepuMatrix.M22 = (Fix64)matrix.m11;
            bepuMatrix.M23 = (Fix64)matrix.m12;
            bepuMatrix.M31 = (Fix64)matrix.m21;
            bepuMatrix.M32 = (Fix64)matrix.m22;
            bepuMatrix.M33 = (Fix64)matrix.m23;
        }

        //Quaternion
        public static UnityEngine.Quaternion Convert(BEPUutilities.Quaternion quaternion)
        {
            UnityEngine.Quaternion toReturn;
            toReturn.x = (float)quaternion.X;
            toReturn.y = (float)quaternion.Y;
            toReturn.z = (float)quaternion.Z;
            toReturn.w = (float)quaternion.W;
            return toReturn;
        }

        public static BEPUutilities.Quaternion Convert(UnityEngine.Quaternion quaternion)
        {
            BEPUutilities.Quaternion toReturn;
            toReturn.X = (Fix64)quaternion.x;
            toReturn.Y = (Fix64)quaternion.y;
            toReturn.Z = (Fix64)quaternion.z;
            toReturn.W = (Fix64)quaternion.w;
            return toReturn;
        }

        public static void Convert(ref BEPUutilities.Quaternion bepuQuaternion, out UnityEngine.Quaternion quaternion)
        {
            quaternion.x = (float)bepuQuaternion.X;
            quaternion.y = (float)bepuQuaternion.Y;
            quaternion.z = (float)bepuQuaternion.Z;
            quaternion.w = (float)bepuQuaternion.W;
        }

        public static void Convert(ref UnityEngine.Quaternion quaternion, out BEPUutilities.Quaternion bepuQuaternion)
        {
            bepuQuaternion.X = (Fix64)quaternion.x;
            bepuQuaternion.Y = (Fix64)quaternion.y;
            bepuQuaternion.Z = (Fix64)quaternion.z;
            bepuQuaternion.W = (Fix64)quaternion.w;
        }

        //Ray
        public static BEPUutilities.Ray Convert(UnityEngine.Ray ray)
        {
            BEPUutilities.Ray toReturn;
            UnityEngine.Vector3 origin = ray.origin;
            UnityEngine.Vector3 direction = ray.direction;
            Convert(ref origin, out toReturn.Position);
            Convert(ref direction, out toReturn.Direction);
            return toReturn;
        }

        public static void Convert(ref UnityEngine.Ray ray, out BEPUutilities.Ray bepuRay)
        {
            UnityEngine.Vector3 origin = ray.origin;
            UnityEngine.Vector3 direction = ray.direction;
            Convert(ref origin, out bepuRay.Position);
            Convert(ref direction, out bepuRay.Direction);
        }

        public static UnityEngine.Ray Convert(BEPUutilities.Ray ray)
        {
            UnityEngine.Ray toReturn = new UnityEngine.Ray();
            UnityEngine.Vector3 origin;
            UnityEngine.Vector3 direction;
            Convert(ref ray.Position, out origin);
            Convert(ref ray.Direction, out direction);
            toReturn.origin = origin;
            toReturn.direction = direction;
            return toReturn;
        }

        public static void Convert(ref BEPUutilities.Ray ray, out UnityEngine.Ray xnaRay)
        {
            xnaRay = new UnityEngine.Ray();
            UnityEngine.Vector3 origin;
            UnityEngine.Vector3 direction;
            Convert(ref ray.Position, out origin);
            Convert(ref ray.Direction, out direction);
            xnaRay.origin = origin;
            xnaRay.direction = direction;
        }

        //BoundingBox
        public static UnityEngine.Bounds Convert(BEPUutilities.BoundingBox boundingBox)
        {
            UnityEngine.Bounds toReturn = new Bounds();
            UnityEngine.Vector3 min;
            UnityEngine.Vector3 max;
            Convert(ref boundingBox.Min, out min);
            Convert(ref boundingBox.Max, out max);
            toReturn.min = min;
            toReturn.max = max;
            return toReturn;
        }

        public static BEPUutilities.BoundingBox Convert(UnityEngine.Bounds boundingBox)
        {
            BEPUutilities.BoundingBox toReturn;
            UnityEngine.Vector3 min = boundingBox.min;
            UnityEngine.Vector3 max = boundingBox.max;
            Convert(ref min, out toReturn.Min);
            Convert(ref max, out toReturn.Max);
            return toReturn;
        }

        public static void Convert(ref BEPUutilities.BoundingBox boundingBox, out UnityEngine.Bounds xnaBoundingBox)
        {
            xnaBoundingBox = new Bounds();
            UnityEngine.Vector3 min;
            UnityEngine.Vector3 max;
            Convert(ref boundingBox.Min, out min);
            Convert(ref boundingBox.Max, out max);
            xnaBoundingBox.min = min;
            xnaBoundingBox.max = max;
        }

        public static void Convert(ref UnityEngine.Bounds boundingBox, out BEPUutilities.BoundingBox bepuBoundingBox)
        {
            UnityEngine.Vector3 min = boundingBox.min;
            UnityEngine.Vector3 max = boundingBox.max;
            Convert(ref min, out bepuBoundingBox.Min);
            Convert(ref max, out bepuBoundingBox.Max);
        }

        //BoundingSphere
        public static UnityEngine.BoundingSphere Convert(BEPUutilities.BoundingSphere boundingSphere)
        {
            UnityEngine.BoundingSphere toReturn;
            Convert(ref boundingSphere.Center, out toReturn.position);
            toReturn.radius = (float)boundingSphere.Radius;
            return toReturn;
        }

        public static BEPUutilities.BoundingSphere Convert(UnityEngine.BoundingSphere boundingSphere)
        {
            BEPUutilities.BoundingSphere toReturn;
            Convert(ref boundingSphere.position, out toReturn.Center);
            toReturn.Radius = (Fix64)boundingSphere.radius;
            return toReturn;
        }

        public static void Convert(ref BEPUutilities.BoundingSphere boundingSphere, out UnityEngine.BoundingSphere xnaBoundingSphere)
        {
            Convert(ref boundingSphere.Center, out xnaBoundingSphere.position);
            xnaBoundingSphere.radius = (float)boundingSphere.Radius;
        }

        public static void Convert(ref UnityEngine.BoundingSphere boundingSphere, out BEPUutilities.BoundingSphere bepuBoundingSphere)
        {
            Convert(ref boundingSphere.position, out bepuBoundingSphere.Center);
            bepuBoundingSphere.Radius = (Fix64)boundingSphere.radius;
        }

        //Plane
        public static UnityEngine.Plane Convert(BEPUutilities.Plane plane)
        {
            UnityEngine.Plane toReturn = new UnityEngine.Plane();
            Convert(ref plane.Normal, out Vector3 normal);
            toReturn.distance = (float)plane.D;
            toReturn.normal = normal;
            return toReturn;
        }

        public static BEPUutilities.Plane Convert(UnityEngine.Plane plane)
        {
            BEPUutilities.Plane toReturn;
            UnityEngine.Vector3 normal = plane.normal;
            Convert(ref normal, out toReturn.Normal);
            toReturn.D = (Fix64)plane.distance;
            return toReturn;
        }

        public static void Convert(ref BEPUutilities.Plane plane, out UnityEngine.Plane xnaPlane)
        {
            xnaPlane = new UnityEngine.Plane();
            Convert(ref plane.Normal, out var normal);
            xnaPlane.distance = (float)plane.D;
            xnaPlane.normal = normal;
        }

        public static void Convert(ref UnityEngine.Plane plane, out BEPUutilities.Plane bepuPlane)
        {
            UnityEngine.Vector3 normal = plane.normal;
            Convert(ref normal, out bepuPlane.Normal);
            bepuPlane.D = (Fix64)plane.distance;
        }
    }
}