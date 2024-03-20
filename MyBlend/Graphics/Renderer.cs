using MyBlend.Models.Basic;
using MyBlend.Models.Display;
using MyBlend.Models.Light;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyBlend.Graphics
{
    public class Renderer
    {
        #region External
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void CopyMemory(IntPtr destination, IntPtr source, uint length);
        #endregion

        //byte array needed for bitmap clearing
        private readonly byte[] blankImage;
        private ZBuffer zBuffer;

        private readonly Screen screen;

        public readonly RgbColor RenderColor = new RgbColor(255, 255, 255);
        public int dpiX { get; set; } = 96;
        public int dpiY { get; set; } = 96;

        private int frameCount = 0;
        public int GetFrameCount()
        {
            var result = frameCount;
            frameCount = 0;
            return result;
        }

        private readonly WriteableBitmap wBitmap;
        private readonly WriteableBitmapInfo wBitmapInfo;

        private Shading? shader;
        public IEnumerable<Light>? Lights;

        private bool withTexture = false;
        private Entity currentEntity;
        public Renderer(Screen screen, Shading shader)
            : this(screen)
        {
            this.shader = shader;
        }
        public Renderer(Screen screen, IEnumerable<Light> lights)
            : this(screen)
        {
            this.Lights = lights;
        }


        public Renderer(Screen screen)
        {

            this.screen = screen;
            wBitmap = new WriteableBitmap((int)screen.Width, (int)screen.Height, dpiX, dpiY, PixelFormats.Bgr32, null);
            wBitmapInfo = new WriteableBitmapInfo(wBitmap.BackBuffer, wBitmap.BackBufferStride, wBitmap.Format.BitsPerPixel);
            RenderOptions.SetBitmapScalingMode(screen.Display, BitmapScalingMode.HighQuality);
            screen.Display.Source = wBitmap;
            blankImage = new byte[(int)(screen.Width * screen.Height * wBitmap.Format.BitsPerPixel / 8)];
            //Array.Fill<byte>(blankImage, 55);
            zBuffer = new ZBuffer((int)screen.Width, (int)screen.Height);
        }

        public void RasterizeEntityWithTexture(Matrix4x4 worldModel, Entity entity)
        {
            if (entity is ObjEntity objEntity)
            {
                currentEntity = objEntity;
                withTexture = true;
                float width = screen.Width;
                float height = screen.Height;

                var poligons = entity.Faces;
                var positions = entity.GetPositionsInWorldModel(worldModel);
                var normals = entity.GetNormalsInWorldModel(worldModel);
                var textures = entity.TexturePositions;

                zBuffer.Clear();
                wBitmap.Lock();
                ClearBitmapBuffer();
                void DrawTriangleInner(Face[] poligon)
                {
                    if (IsPoligonBehindCamera(poligon, positions))
                        return;

                    var vertices = new Vertex[poligon.Length];
                    for (int i = 0; i < poligon.Length; i++)
                    {
                        var pos = positions[poligon[i].vIndex];
                        vertices[i] = new Vertex()
                        {
                            ScreenPosition = new Vector2(Clamp(pos.X, 0, width - 1), Clamp(pos.Y, 0, height - 1)),
                            WorldPosition = new Vector3(pos.X, pos.Y, pos.Z),
                            Normal = normals[poligon[i].nIndex],
                            TexturePosition = textures[poligon[i].tIndex]
                        };
                    }

                    for (int i = 1; i < vertices.Length - 1; i++)
                    {
                        DrawTriangle(vertices[0], vertices[i], vertices[i + 1]);
                    }
                }
                Parallel.ForEach(poligons, (Face[] poligon) => DrawTriangleInner(poligon));

                wBitmap.AddDirtyRect(new Int32Rect(0, 0, (int)width, (int)height));
                wBitmap.Unlock();
                withTexture = false;

                frameCount++;
            }

        }

        public void RasterizeEntity(Matrix4x4 worldModel, Entity entity)
        {
            float width = screen.Width;
            float height = screen.Height;

            var poligons = entity.Faces;
            var positions = entity.GetPositionsInWorldModel(worldModel);
            var normals = entity.GetNormalsInWorldModel(worldModel);

            zBuffer.Clear();
            wBitmap.Lock();
            ClearBitmapBuffer();
            void DrawTriangleInner(Face[] poligon)
            {
                if (IsPoligonBehindCamera(poligon, positions))
                    return;

                var vertices = new Vertex[poligon.Length];
                for (int i = 0; i < poligon.Length; i++)
                {
                    var pos = positions[poligon[i].vIndex];
                    vertices[i] = new Vertex()
                    {
                        ScreenPosition = new Vector2(Clamp(pos.X, 0, width - 1), Clamp(pos.Y, 0, height - 1)),
                        WorldPosition = new Vector3(pos.X, pos.Y, pos.Z),
                        Normal = normals[poligon[i].nIndex]
                    };
                }

                for (int i = 1; i < vertices.Length - 1; i++)
                {
                    DrawTriangle(vertices[0], vertices[i], vertices[i + 1]);
                }
            }
            Parallel.ForEach(poligons, (Face[] poligon) => DrawTriangleInner(poligon));

            wBitmap.AddDirtyRect(new Int32Rect(0, 0, (int)width, (int)height));
            wBitmap.Unlock();

            frameCount++;
        }

        public void DrawEntityMesh(Matrix4x4 worldModel, Entity entity)
        {
            float width = screen.Width;
            float height = screen.Height;

            var poligons = entity.Faces;
            var positions = entity.GetPositionsInWorldModel(worldModel);

            ClearBitmapBuffer();
            var bmpInfo = new WriteableBitmapInfo(wBitmap.BackBuffer, wBitmap.BackBufferStride, wBitmap.Format.BitsPerPixel);
            wBitmap.Lock();

            void DrawPoligon(Face[] poligon)
            {
                if (IsPoligonBehindCamera(poligon, positions))
                    return;

                int i = 0;
                int x1, x2, y1, y2;
                for (i = 0; i < poligon.Length - 1; i++)
                {
                    x1 = GetCoordinateInRange(positions[poligon[i].vIndex].X, 0, width - 1);
                    y1 = GetCoordinateInRange(positions[poligon[i].vIndex].Y, 0, height - 1);

                    x2 = GetCoordinateInRange(positions[poligon[i + 1].vIndex].X, 0, width - 1);
                    y2 = GetCoordinateInRange(positions[poligon[i + 1].vIndex].Y, 0, height - 1);

                    DrawLine(bmpInfo, x1, y1, x2, y2);
                }

                x1 = GetCoordinateInRange(positions[poligon[i].vIndex].X, 0, width - 1);
                y1 = GetCoordinateInRange(positions[poligon[i].vIndex].Y, 0, height - 1);

                x2 = GetCoordinateInRange(positions[poligon[0].vIndex].X, 0, width - 1);
                y2 = GetCoordinateInRange(positions[poligon[0].vIndex].Y, 0, height - 1);

                DrawLine(bmpInfo, x1, y1, x2, y2);
            }
            Parallel.ForEach(poligons, (Face[] poligon) => DrawPoligon(poligon));

            wBitmap.AddDirtyRect(new Int32Rect(0, 0, (int)width, (int)height));
            wBitmap.Unlock();
        }

        private bool IsPoligonBehindCamera(Face[] poligon, IList<Vector4> positions) => poligon.Any(point => positions[point.vIndex] == Vector4.Zero);

        private int GetCoordinateInRange(float value, float min, float max) => (int)Clamp(value, min, max);

        private void DrawLine(WriteableBitmapInfo bmp, int x1, int y1, int x2, int y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            var steps = Math.Max(Math.Abs(dx), Math.Abs(dy));
            if (steps == 0)
                return;

            dx = dx / steps;
            dy = dy / steps;
            try
            {
                unsafe
                {
                    var backBuffer = bmp.BackBuffer;
                    int bmpStride = bmp.BackBufferStride;
                    int pixelSize = bmp.FormatBitsPerPixel / 8;
                    var startOfBuffer = backBuffer;
                    double row = y1, column = x1;

                    for (int i = 0; i < steps; i++)
                    {
                        backBuffer += (int)row * bmpStride;
                        backBuffer += (int)column * pixelSize;

                        (*(int*)backBuffer) = RenderColor.Color;

                        column += dx;
                        row += dy;
                        backBuffer = startOfBuffer;
                    }
                }
            }
            finally { }
        }

        private unsafe void ClearBitmapBuffer()
        {
            fixed (byte* b = blankImage)
            {
                CopyMemory(wBitmap.BackBuffer, (IntPtr)b, (uint)blankImage.Length);
            }
        }

        private void DrawPixel(int column, int row, int color = Int32.MinValue)
        {
            if (color == Int32.MinValue)
                color = RenderColor.Color;
            int pixelSize = wBitmapInfo.FormatBitsPerPixel / 8;
            try
            {
                unsafe
                {
                    var backBuffer = wBitmapInfo.BackBuffer;
                    backBuffer += row * wBitmapInfo.BackBufferStride;
                    backBuffer += column * pixelSize;

                    (*(int*)backBuffer) = color;
                }
            }
            finally
            {
            }
        }

        private record struct WriteableBitmapInfo(nint BackBuffer, int BackBufferStride, int FormatBitsPerPixel);

        public void UpdateShader(Shading? shader) => this.shader = shader;
        float Clamp(float value, float min = 0, float max = 1) => Math.Max(min, Math.Min(value, max));
        float Interpolate(float min, float max, float gradient) => min + (max - min) * Clamp(gradient);
        float Cross2D(float x0, float y0, float x1, float y1) => x0 * y1 - x1 * y0;
        float LineSide2D(Vector2 p, Vector2 lineFrom, Vector2 lineTo) => Cross2D(p.X - lineFrom.X, p.Y - lineFrom.Y, lineTo.X - lineFrom.X, lineTo.Y - lineFrom.Y);

        void ProcessScanLine(ScanLineData data, Vertex va, Vertex vb, Vertex vc, Vertex vd)
        {
            float GetColorIntensity(Vector3 light, Vector3 p)
            {
                if (shader == null)
                    return 1;

                if (vc.WorldPosition != va.WorldPosition && vc.WorldPosition != vb.WorldPosition)
                    return shader.GetColorIntensity(light, va, vb, vc, p);
                else
                    return shader.GetColorIntensity(light, va, vb, vd, p);
            }

            int GetColor(Vector3 p, float u = 0, float v = 0)
            {
                var clr = RgbColor.Black;
                if (Lights != null && Lights.Count() > 0)
                {
                    foreach (var light in Lights)
                    {
                        var lclr = light.Color;
                        if (withTexture && currentEntity is ObjEntity entity)
                        {
                            if (entity.Textures.ContainsKey("map_Kd"))
                            {
                                lclr = entity.Textures["map_Kd"].Map(u, 1 - v);
                            }
                            if (entity.Textures.ContainsKey("norm"))
                            {
                                var nclr = entity.Textures["norm"].Map(u, 1 - v);
                                var normal = new Vector3(nclr.R / (float)255, nclr.G / (float)255, nclr.B / (float)255);
                                normal = 2 * normal - Vector3.One;
                                va.Normal = vb.Normal = vc.Normal = vd.Normal = Vector3.Normalize(normal);
                            }
                        }

                        var intensity = GetColorIntensity(light.Position, p);
                        clr.UpdateColor(lclr.ByIntensity(intensity));
                    }
                }
                return clr.Color;
            }

            var pa = va.ScreenPosition;
            var pb = vb.ScreenPosition;
            var pc = vc.ScreenPosition;
            var pd = vd.ScreenPosition;

            var y = data.Y;

            var gradient1 = pa.Y != pb.Y ? (y - pa.Y) / (pb.Y - pa.Y) : 1;
            var gradient2 = pc.Y != pd.Y ? (y - pc.Y) / (pd.Y - pc.Y) : 1;

            int sx = (int)Interpolate(pa.X, pb.X, gradient1);
            int ex = (int)Interpolate(pc.X, pd.X, gradient2);

            float sz = Interpolate(va.WorldPosition.Z, vb.WorldPosition.Z, gradient1);
            float ez = Interpolate(vc.WorldPosition.Z, vd.WorldPosition.Z, gradient2);

            if (withTexture)
            {
                var su = Interpolate(va.TexturePosition!.Value.X, vb.TexturePosition!.Value.X, gradient1);
                var eu = Interpolate(vc.TexturePosition!.Value.X, vd.TexturePosition!.Value.X, gradient2);
                var sv = Interpolate(va.TexturePosition!.Value.Y, vb.TexturePosition!.Value.Y, gradient1);
                var ev = Interpolate(vc.TexturePosition!.Value.Y, vd.TexturePosition!.Value.Y, gradient2);

                for (var x = sx; x < ex; x++)
                {
                    float gradientZ = (x - sx) / (float)(ex - sx);
                    var z = Interpolate(sz, ez, gradientZ);
                    var u = Interpolate(su, eu, gradientZ);
                    var v = Interpolate(sv, ev, gradientZ);


                    var clr = GetColor(new Vector3(x, y, z), u, v);
                    zBuffer.TryToUpdateZ(x, y, z, () => DrawPixel(x, y, clr));
                }
            }
            else
            {
                for (var x = sx; x < ex; x++)
                {
                    float gradientZ = (x - sx) / (float)(ex - sx);
                    var z = Interpolate(sz, ez, gradientZ);

                    var clr = GetColor(new Vector3(x, y, z));
                    zBuffer.TryToUpdateZ(x, y, z, () => DrawPixel(x, y, clr));
                }
            }
        }

        void DrawTriangle(Vertex v1, Vertex v2, Vertex v3)
        {
            if (v1.ScreenPosition.Y > v2.ScreenPosition.Y) (v1, v2) = (v2, v1);
            if (v2.ScreenPosition.Y > v3.ScreenPosition.Y) (v2, v3) = (v3, v2);
            if (v1.ScreenPosition.Y > v2.ScreenPosition.Y) (v1, v2) = (v2, v1);

            ScanLineData data = new ScanLineData(Lights)
            {
                Color = RenderColor
            };
            var p1 = v1.ScreenPosition;
            var p2 = v2.ScreenPosition;
            var p3 = v3.ScreenPosition;
            // P1
            // -
            // -   - P2
            // -
            // P3
            if (LineSide2D(p2, p1, p3) > 0)
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    data.Y = y;
                    if (y < p2.Y)
                    {
                        ProcessScanLine(data, v1, v3, v1, v2);
                    }
                    else
                    {
                        ProcessScanLine(data, v1, v3, v2, v3);
                    }
                }
            }
            //       P1
            //        -
            // P2 -   - 
            //        -
            //       P3
            else
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    data.Y = y;
                    if (y < p2.Y)
                    {
                        ProcessScanLine(data, v1, v2, v1, v3);
                    }
                    else
                    {
                        ProcessScanLine(data, v2, v3, v1, v3);
                    }
                }
            }
        }
    }
}
