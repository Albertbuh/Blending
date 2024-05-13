using System.Numerics;

namespace MyBlend.Models.Basic
{
    public struct Vertex
    {
        public Vector3 Normal { get; init; }
        public Vector4 GlobalPosition { get; init; }
        public Vector4 ScreenPosition { get; set; }
        public Vector2 TexturePosition { get; init; }

        public Vertex(Vector4 global, Vector4 screen, Vector3 normal, Vector2 texture)
        {
            var moveVector = new Vector4(normal.X, normal.Y, normal.Z, 0);
            GlobalPosition = global + 2 * moveVector;
            ScreenPosition = screen;
            Normal = normal;
            TexturePosition = texture;
        }

        //Save inverse value of W in Viewer space for perspective correction
        public float InverseW => 1 / ScreenPosition.W;
    }
}
