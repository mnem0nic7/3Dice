using System.Numerics;

namespace _3Dice.Models
{
    public class Vector3D
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3D(float x = 0, float y = 0, float z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3D operator +(Vector3D a, Vector3D b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vector3D operator -(Vector3D a, Vector3D b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vector3D operator *(Vector3D v, float scalar) => new(v.X * scalar, v.Y * scalar, v.Z * scalar);

        public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z);

        public Vector3D Normalize()
        {
            var len = Length;
            return len > 0 ? new Vector3D(X / len, Y / len, Z / len) : new Vector3D();
        }

        public static float Dot(Vector3D a, Vector3D b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }
}