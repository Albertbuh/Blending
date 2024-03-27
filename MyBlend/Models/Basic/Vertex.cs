using System.Numerics;

namespace MyBlend.Models.Basic
{
    public struct Vertex
    {
        public Vector3 Normal;
        public Vector3 WorldPosition;
        public Vector2 ScreenPosition;
        public Vector2? TexturePosition;
        public float W;
    }
}
