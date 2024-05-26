using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Accessibility;
using MyBlend.Graphics.RendererEnums;
using MyBlend.Models.Basic;
using MyBlend.Models.Display;
using MyBlend.Models.Light;
using MyBlend.Models.Textures;

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
        const int dpiX = 96;
        const int dpiY = 96;

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

        public TextureStyle TextureStyle = TextureStyle.None;
        public bool WithNormal = false;
        public LightStyle LightStyle = LightStyle.BlinnPhong;
        public RenderStyle RenderStyle = RenderStyle.Mesh;
        public bool WithOutline = false;
        private bool isColorByNormal = false;
        private Entity currentEntity;

        delegate float LightHandler(Vector3 camera, Vector3 light, Vector3 position, Vector3 normal);
        private Dictionary<LightStyle, LightHandler> lightMethods = new Dictionary<LightStyle, LightHandler>()
        {
            [LightStyle.Phong] = LightSystem.CalcPhongLightIntensity,
            [LightStyle.BlinnPhong] = LightSystem.CalcBlinnPhongLightIntensity,
            [LightStyle.CelShading] = LightSystem.CalcCelShading,
        };
        delegate void RenderMethodsHandler(Matrix4x4 worldModel, Entity entity);
        private Dictionary<RenderStyle, RenderMethodsHandler> renderMethods = new Dictionary<RenderStyle, RenderMethodsHandler>();

        public Renderer(Screen screen, IEnumerable<Light> lights)
            : this(screen)
        {
            this.Lights = lights;
        }

        public Renderer(Screen screen)
            : this()
        {
            this.screen = screen;
            wBitmap = new WriteableBitmap(
                (int)screen.Width,
                (int)screen.Height,
                dpiX,
                dpiY,
                PixelFormats.Bgr32,
                null
            );
            wBitmapInfo = new WriteableBitmapInfo(
                wBitmap.BackBuffer,
                wBitmap.BackBufferStride,
                wBitmap.Format.BitsPerPixel
            );
            RenderOptions.SetBitmapScalingMode(screen.Display, BitmapScalingMode.HighQuality);
            screen.Display.Source = wBitmap;
            blankImage = new byte[
                (int)(screen.Width * screen.Height * wBitmap.Format.BitsPerPixel / 8)
            ];
            //Array.Fill<byte>(blankImage, 55);
            zBuffer = new ZBuffer((int)screen.Width, (int)screen.Height);
        }

#pragma warning disable 8618
        public Renderer()
        {
            renderMethods.Add(RenderStyle.Mesh, DrawEntityMesh);
            renderMethods.Add(RenderStyle.Old, RasterizeEntityWithScanLine);
            renderMethods.Add(RenderStyle.Basic, RasterizeEntityWithBaricentricCoordinates);
        }

        public void RasterizeEntity(Matrix4x4 worldModel, Entity entity)
        {
            renderMethods[this.RenderStyle].Invoke(worldModel, entity);
        }

        void RasterizeEntityWithBaricentricCoordinates(Matrix4x4 worldModel, Entity entity)
        {
            zBuffer.Clear();
            wBitmap.Lock();
            ClearBitmapBuffer();

            var poligons = entity.Faces;
            var globalPositions = entity.Positions;
            var positions = entity.GetPositionsInWorldModel(worldModel);
            var normals = entity.GetNormalsInWorldModel(worldModel);
            var textures = entity.TexturePositions;

            void DrawTriangleInner(Vertex v0, Vertex v1, Vertex v2)
            {
                if (IsCutted(v0.Normal, v1.Normal, v2.Normal))
                    return;

                var poligonRectangle = GetPoligonBoundingRectangle(v0, v1, v2);

                float u, v, w;
                var square = CalcEdgeFunction(
                    v0.ScreenPosition,
                    v1.ScreenPosition,
                    v2.ScreenPosition.X,
                    v2.ScreenPosition.Y
                );

                for (int y = (int)poligonRectangle.Top; y <= poligonRectangle.Bottom; y++)
                {
                    if (y < 0)
                        continue;
                    if (y >= screen.Height)
                        break;

                    for (int x = (int)poligonRectangle.Left; x <= poligonRectangle.Right; x++)
                    {
                        if (x < 0)
                            continue;
                        if (x >= screen.Width)
                            break;

                        u = CalcEdgeFunction(
                            v1.ScreenPosition,
                            v2.ScreenPosition,
                            x,
                            y
                        );
                        v = CalcEdgeFunction(
                            v2.ScreenPosition,
                            v0.ScreenPosition,
                            x,
                            y
                        );
                        w = CalcEdgeFunction(
                            v0.ScreenPosition,
                            v1.ScreenPosition,
                            x,
                            y
                        );
                        if ((u >= 0 && v >= 0 && w >= 0) || (u <= 0 && v <= 0 && w <= 0))
                        {
                            u /= square;
                            v /= square;
                            w /= square;

                            var z = 1 /
                                (v0.ScreenPosition.Z * u
                                + v1.ScreenPosition.Z * v
                                + v2.ScreenPosition.Z * w);
                            zBuffer.TryToUpdateZ(x, y, z, DrawPoint);

                            void DrawPoint()
                            {
                                var globalPosition = Vector3.Zero;
                                var normal = Vector3.Zero;
                                var resultColor = RgbColor.White;

                                var textureColor = RgbColor.Black;
                                if (this.TextureStyle != TextureStyle.None 
                                    || WithNormal)
                                    textureColor = GetTextureColor((ObjEntity)entity, v0, v1, v2, u, v, w, ref normal);

                                var lightColor = RgbColor.Black;
                                Vector3 globalIntensity = Vector3.Zero;
                                if (this.LightStyle != LightStyle.None)
                                {
                                    globalIntensity = GetLightIntensity(v0, v1, v2, u, v, w, ref globalPosition, ref normal);
                                }
                                else
                                {
                                    globalIntensity = Vector3.One;
                                }


                                if (this.TextureStyle != TextureStyle.None)
                                {
                                    DrawPixel(x, y, textureColor.ByIntensity(globalIntensity.X, globalIntensity.Y, globalIntensity.Z));
                                }
                                else
                                    DrawPixel(x, y, RenderColor.ByIntensity(globalIntensity.X, globalIntensity.Y, globalIntensity.Z));
                            }

                        }
                    }
                }
            }

            if (WithOutline)
            {
                DrawCelOutline(worldModel, poligons, globalPositions, normals);
            }

            Parallel.ForEach(poligons, (Face[] poligon) =>
            {
                if (IsPoligonBehindCamera(poligon, positions))
                    return;

                for (int i = 1; i < poligon.Length - 1; i++)
                {
                    var v0 = new Vertex
                    {
                        GlobalPosition = globalPositions[poligon[0].vIndex],
                        ScreenPosition = positions[poligon[0].vIndex],
                        Normal = normals[poligon[0].nIndex],
                        TexturePosition = textures[poligon[0].tIndex]
                    };
                    var v1 = new Vertex
                    {
                        GlobalPosition = globalPositions[poligon[i].vIndex],
                        ScreenPosition = positions[poligon[i].vIndex],
                        Normal = normals[poligon[i].nIndex],
                        TexturePosition = textures[poligon[i].tIndex]
                    };
                    var v2 = new Vertex
                    {
                        GlobalPosition = globalPositions[poligon[i + 1].vIndex],
                        ScreenPosition = positions[poligon[i + 1].vIndex],
                        Normal = normals[poligon[i + 1].nIndex],
                        TexturePosition = textures[poligon[i + 1].tIndex]
                    };
                    DrawTriangleInner(v0, v1, v2);
                }
            });

            wBitmap.AddDirtyRect(new Int32Rect(0, 0, (int)screen.Width, (int)screen.Height));
            wBitmap.Unlock();

            frameCount++;
        }

        void RasterizeEntityWithScanLine(Matrix4x4 worldModel, Entity entity)
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
                        ScreenPosition = new Vector4(
                            Clamp(pos.X, 0, width - 1),
                            Clamp(pos.Y, 0, height - 1), 0, 0
                        ),
                        GlobalPosition = new Vector4(pos.X, pos.Y, -pos.Z, 1),
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
        void DrawEntityMesh(Matrix4x4 worldModel, Entity entity)
        {
            float width = screen.Width;
            float height = screen.Height;

            var poligons = entity.Faces;
            var positions = entity.GetPositionsInWorldModel(worldModel);

            ClearBitmapBuffer();
            var bmpInfo = new WriteableBitmapInfo(
                wBitmap.BackBuffer,
                wBitmap.BackBufferStride,
                wBitmap.Format.BitsPerPixel
            );
            wBitmap.Lock();

            void DrawPoligon(Face[] poligon)
            {
                if (IsPoligonBehindCamera(poligon, positions))
                    return;

                int i = 0;
                int x1,
                    x2,
                    y1,
                    y2;
                for (i = 0; i < poligon.Length - 1; i++)
                {
                    x1 = GetCoordinateInRange(positions[poligon[i].vIndex].X, 0, width - 1);
                    y1 = GetCoordinateInRange(positions[poligon[i].vIndex].Y, 0, height - 1);

                    x2 = GetCoordinateInRange(positions[poligon[i + 1].vIndex].X, 0, width - 1);
                    y2 = GetCoordinateInRange(positions[poligon[i + 1].vIndex].Y, 0, height - 1);

                    DrawLine(x1, y1, x2, y2);
                }

                x1 = GetCoordinateInRange(positions[poligon[i].vIndex].X, 0, width - 1);
                y1 = GetCoordinateInRange(positions[poligon[i].vIndex].Y, 0, height - 1);

                x2 = GetCoordinateInRange(positions[poligon[0].vIndex].X, 0, width - 1);
                y2 = GetCoordinateInRange(positions[poligon[0].vIndex].Y, 0, height - 1);

                DrawLine(x1, y1, x2, y2);
            }
            Parallel.ForEach(poligons, (Face[] poligon) => DrawPoligon(poligon));

            wBitmap.AddDirtyRect(new Int32Rect(0, 0, (int)width, (int)height));
            wBitmap.Unlock();
        }

        Rect GetPoligonBoundingRectangle(Vertex v0, Vertex v1, Vertex v2)
        {
            var left = Math.Min(v0.ScreenPosition.X, Math.Min(v1.ScreenPosition.X, v2.ScreenPosition.X));
            var right = Math.Max(v0.ScreenPosition.X, Math.Max(v1.ScreenPosition.X, v2.ScreenPosition.X));
            var top = Math.Min(v0.ScreenPosition.Y, Math.Min(v1.ScreenPosition.Y, v2.ScreenPosition.Y));
            var bottom = Math.Max(v0.ScreenPosition.Y, Math.Max(v1.ScreenPosition.Y, v2.ScreenPosition.Y));
            return new Rect(left, top, right - left, bottom - top);
        }

        Rect GetPoligonBoundingRectanble(Vector4 v0, Vector4 v1, Vector4 v2)
        {
            var left = Math.Min(v0.X, Math.Min(v1.X, v2.X));
            var right = Math.Max(v0.X, Math.Max(v1.X, v2.X));
            var top = Math.Min(v0.Y, Math.Min(v1.Y, v2.Y));
            var bottom = Math.Max(v0.Y, Math.Max(v1.Y, v2.Y));
            return new Rect(left, top, right - left, bottom - top);
        }

        float CalcEdgeFunction(Vector4 v1, Vector4 v2, float x, float y) =>
            (x - v1.X) * (v2.Y - v1.Y) - (y - v1.Y) * (v2.X - v1.X);

        Vector4 CountPositionInWorld(Vector4 v, Matrix4x4 m)
        {
            var result = Vector4.Transform(v, m);
            if (result.W <= 0)
                return Vector4.Zero;
            return new Vector4(result.X / result.W, result.Y / result.W, result.Z / result.W, result.W);
        }

        void DrawCelOutline(Matrix4x4 worldModel, List<Face[]> poligons, List<Vector4> globalPositions, List<Vector3> normals)
        {
            var width = 0.1f;
            void DrawOutline(Vector4 v0, Vector4 v1, Vector4 v2)
            {
                var poligonRectangle = GetPoligonBoundingRectanble(v0, v1, v2);

                float u, v, w;
                for (int y = (int)poligonRectangle.Top; y <= poligonRectangle.Bottom; y++)
                {
                    if (y < 0)
                        continue;
                    if (y >= screen.Height)
                        break;

                    for (int x = (int)poligonRectangle.Left; x <= poligonRectangle.Right; x++)
                    {
                        if (x < 0)
                            continue;
                        if (x >= screen.Width)
                            break;

                        u = CalcEdgeFunction(
                            v1,
                            v2,
                            x,
                            y
                        );
                        v = CalcEdgeFunction(
                            v2,
                            v0,
                            x,
                            y
                        );
                        w = CalcEdgeFunction(
                            v0,
                            v1,
                            x,
                            y
                        );
                        if ((u >= 0 && v >= 0 && w >= 0) || (u <= 0 && v <= 0 && w <= 0))
                        {
                            DrawPixel(x, y, RgbColor.White.Color);
                        }
                    }
                }
            }
            Vector4 CrossVectors(Vector4 v1, Vector3 v2)
            {
                v1.X += v2.X;
                v1.Y += v2.Y;
                v1.Z += v2.Z;
                return v1;
            }

            Parallel.ForEach(poligons, (Face[] poligon) =>
            {
                if (IsPoligonBehindCamera(poligon, globalPositions))
                    return;

                for (int i = 1; i < poligon.Length - 1; i++)
                {
                    var v0 = globalPositions[poligon[0].vIndex];
                    var n0 = normals[poligon[0].nIndex];
                    var v1 = globalPositions[poligon[i].vIndex];
                    var n1 = normals[poligon[i].nIndex];
                    var v2 = globalPositions[poligon[i + 1].vIndex];
                    var n2 = normals[poligon[i + 1].nIndex];
                    v0 = CountPositionInWorld(CrossVectors(v0, n0 * width), worldModel);
                    v1 = CountPositionInWorld(CrossVectors(v1, n1 * width), worldModel);
                    v2 = CountPositionInWorld(CrossVectors(v2, n2 * width), worldModel);
                    DrawOutline(v0, v1, v2);
                }
            });

        }

        Vector3 GetLightIntensity(Vertex v0, Vertex v1, Vertex v2, float u, float v, float w, ref Vector3 position, ref Vector3 normal)
        {
            var globalX =
                 v0.GlobalPosition.X * u
                 + v1.GlobalPosition.X * v
                 + v2.GlobalPosition.X * w;
            var globalY =
                v0.GlobalPosition.Y * u
                + v1.GlobalPosition.Y * v
                + v2.GlobalPosition.Y * w;
            var globalZ =
                v0.GlobalPosition.Z * u
                + v1.GlobalPosition.Z * v
                + v2.GlobalPosition.Z * w;
            position = new Vector3(globalX, globalY, globalZ);

            if (normal == Vector3.Zero)
            {
                var normalX =
                    v0.Normal.X * u
                    + v1.Normal.X * v
                    + v2.Normal.X * w;
                var normalY =
                    v0.Normal.Y * u
                    + v1.Normal.Y * v
                    + v2.Normal.Y * w;
                var normalZ =
                    v0.Normal.Z * u
                    + v1.Normal.Z * v
                    + v2.Normal.Z * w;
                normal = new Vector3(normalX, normalY, normalZ);
            }
            normal = Vector3.Normalize(normal);

            var lightColor = RgbColor.Black;
            var globalIntensity = Vector3.Zero;
            foreach (var light in Lights!)
            {
                var intensity = lightMethods[this.LightStyle].Invoke(
                    screen.Camera!.Eye,
                    light.Position,
                    position,
                    normal
                );
                lightColor = RgbColor.CrossColors(lightColor, light.Color.GetColorByIntensity(intensity));
                globalIntensity.X += intensity * (light.Color.R);
                globalIntensity.Y += intensity * (light.Color.G);
                globalIntensity.Z += intensity * (light.Color.B);
            }
            globalIntensity /= (float)255;

            return globalIntensity;
        }

        RgbColor GetTextureColor(ObjEntity objEntity, Vertex v0, Vertex v1, Vertex v2, float u, float v, float w, ref Vector3 normal)
        {

            var numeratorU =
                (v0.TexturePosition.X * v0.InverseW) * u
                + (v1.TexturePosition.X * v1.InverseW) * v
                + (v2.TexturePosition.X * v2.InverseW) * w;
            var numeratorV =
                (v0.TexturePosition.Y * v0.InverseW) * u
                + (v1.TexturePosition.Y * v1.InverseW) * v
                + (v2.TexturePosition.Y * v2.InverseW) * w;

            var inverseW =
                v0.InverseW * u
                + v1.InverseW * v
                + v2.InverseW * w;

            var textureU = numeratorU / inverseW;
            var textureV = numeratorV / inverseW;

            //Update texel normal
            if (WithNormal && objEntity.Textures.ContainsKey("norm"))
            {
                var nclr = objEntity.Textures["norm"].Map(textureU, 1 - textureV);
                var textureNormal = new Vector3(
                    (float)nclr.R / 255,
                    (float)nclr.G / 255,
                    (float)nclr.B / 255
                );
                normal = Vector3.Normalize(2 * textureNormal - Vector3.One);
            }

            var textureColor = RgbColor.Black;
            if (this.TextureStyle != TextureStyle.None && objEntity.Textures.ContainsKey("map_Kd"))
            {
                var texture = objEntity.Textures["map_Kd"];

                if (this.TextureStyle == TextureStyle.Basic)
                    textureColor = texture.Map(textureU, 1 - textureV);
                else if (this.TextureStyle == TextureStyle.Bilinear)
                    textureColor = GetColorWithBilinearFiltration(texture, textureU, 1 - textureV);
            }

            return textureColor;
        }

        RgbColor GetColorWithBilinearFiltration(Texture texture, float u, float v)
        {
            var textureX = (u * texture.Width);
            var textureY = (v * texture.Height);

            var texelTop = (int)Math.Floor(textureY) % texture.Height;
            var texelBottom = (int)Math.Ceiling(textureY) % texture.Height;
            var texelLeft = (int)Math.Floor(textureX) % texture.Width;
            var texelRight = (int)Math.Ceiling(textureX) % texture.Width;

            var s = (float)(textureX - Math.Truncate(textureX));
            var t = (float)(textureY - Math.Truncate(textureY));

            var clrTopRight = texture.GetPixel(texelRight, texelTop);
            var clrTopLeft = texture.GetPixel(texelLeft, texelTop);
            var clrBottomRight = texture.GetPixel(texelRight, texelBottom);
            var clrBottomLeft = texture.GetPixel(texelLeft, texelBottom);

            return RgbColor.Interpolate(
                RgbColor.Interpolate(clrTopLeft, clrTopRight, s),
                RgbColor.Interpolate(clrBottomLeft, clrBottomRight, s),
                t);
        }

        bool IsPoligonBehindCamera(Face[] poligon, IList<Vector4> positions) =>
            poligon.Any(point => positions[point.vIndex] == Vector4.Zero);

        bool IsCutted(params Vertex[] vertices) =>
            vertices.All(v => Vector3.Dot(screen.Camera!.Eye, v.Normal) < 0);

        bool IsCutted(params Vector3[] normals) =>
            normals.All(normal => Vector3.Dot(screen.Camera!.Eye, normal) < 0);

        int GetCoordinateInRange(float value, float min, float max) =>
            (int)Clamp(value, min, max);

        void DrawLine(int x1, int y1, int x2, int y2)
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
                    var backBuffer = wBitmapInfo.BackBuffer;
                    int bmpStride = wBitmapInfo.BackBufferStride;
                    int pixelSize = wBitmapInfo.FormatBitsPerPixel / 8;
                    var startOfBuffer = backBuffer;
                    double row = y1,
                        column = x1;

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

        unsafe void ClearBitmapBuffer()
        {
            fixed (byte* b = blankImage)
            {
                CopyMemory(wBitmap.BackBuffer, (IntPtr)b, (uint)blankImage.Length);
            }
        }

        void DrawPixel(int column, int row, int color = Int32.MinValue)
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
            finally { }
        }

        record struct WriteableBitmapInfo(
            nint BackBuffer,
            int BackBufferStride,
            int FormatBitsPerPixel
        );

        public void UpdateShader(Shading? shader, bool isColorByNormal = false)
        {
            this.shader = shader;
        }

        float Clamp(float value, float min = 0, float max = 1) =>
            Math.Max(min, Math.Min(value, max));

        float Interpolate(float min, float max, float gradient) =>
            min + (max - min) * Clamp(gradient);

        float Cross2D(float x0, float y0, float x1, float y1) => x0 * y1 - x1 * y0;

        float LineSide2D(Vector4 p, Vector4 lineFrom, Vector4 lineTo) =>
            Cross2D(
                p.X - lineFrom.X,
                p.Y - lineFrom.Y,
                lineTo.X - lineFrom.X,
                lineTo.Y - lineFrom.Y
            );

        void ProcessScanLine(ScanLineData data, Vertex va, Vertex vb, Vertex vc, Vertex vd)
        {
            float GetColorIntensity(Vector3 light, Vector3 p)
            {
                if (shader == null)
                    return 1;

                if (vc.GlobalPosition != va.GlobalPosition && vc.GlobalPosition != vb.GlobalPosition)
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

            float sz = Interpolate(va.GlobalPosition.Z, vb.GlobalPosition.Z, gradient1);
            float ez = Interpolate(vc.GlobalPosition.Z, vd.GlobalPosition.Z, gradient2);

            float sw = Interpolate(va.GlobalPosition.W, vb.GlobalPosition.W, gradient1);
            float ew = Interpolate(vc.GlobalPosition.W, vd.GlobalPosition.W, gradient2);


            for (var x = sx; x < ex; x++)
            {
                float gradientZ = (x - sx) / (float)(ex - sx);
                var z = Interpolate(sz, ez, gradientZ);

                var clr = GetColor(new Vector3(x, y, z));
                zBuffer.TryToUpdateZ(x, y, z, () => DrawPixel(x, y, clr));
            }
        }

        void DrawTriangle(Vertex v1, Vertex v2, Vertex v3)
        {
            if (IsCutted(v1, v2, v3))
                return;

            if (v1.ScreenPosition.Y > v2.ScreenPosition.Y)
                (v1, v2) = (v2, v1);
            if (v2.ScreenPosition.Y > v3.ScreenPosition.Y)
                (v2, v3) = (v3, v2);
            if (v1.ScreenPosition.Y > v2.ScreenPosition.Y)
                (v1, v2) = (v2, v1);

            ScanLineData data = new ScanLineData(Lights) { Color = RenderColor };
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
