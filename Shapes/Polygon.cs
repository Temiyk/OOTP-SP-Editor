using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;

namespace Lab1.Shapes
{
    public class Polygon : Figure
    {
        [JsonProperty]
        public List<PointF> Vertices { get; set; } = new List<PointF>();
        
        [JsonConstructor]
        protected Polygon() { }
        public Polygon(Point center, string name = "Многоугольник") : base(center)
        {
            Name = name;
        }

        // Статические методы создания предопределённых фигур
        public static Polygon CreateRectangle(Point center)
        {
            var f = new Polygon(center, "Прямоугольник");
            f.Vertices = new List<PointF>
            {
                new PointF(-50, -50),
                new PointF(50, -50),
                new PointF(50, 50),
                new PointF(-50, 50)
            };
            f.SyncSidesFromVertices();
            return f;
        }

        public static Polygon CreateTriangle(Point center)
        {
            var f = new Polygon(center, "Треугольник");
            f.Vertices = new List<PointF>
            {
                new PointF(0, -50),
                new PointF(45, 40),
                new PointF(-45, 40)
            };
            f.SyncSidesFromVertices();
            return f;
        }

        public static Polygon CreateTrapezium(Point center)
        {
            var f = new Polygon(center, "Трапеция");
            f.Vertices = new List<PointF>
            {
                new PointF(-30, -30),
                new PointF(30, -30),
                new PointF(60, 30),
                new PointF(-60, 30)
            };
            f.SyncSidesFromVertices();
            return f;
        }

        public static Polygon CreatePentagon(Point center)
        {
            var f = new Polygon(center, "Пятиугольник");
            f.Vertices = new List<PointF>();
            for (int i = 0; i < 5; i++)
            {
                double angle = -Math.PI / 2 + i * 2 * Math.PI / 5;
                f.Vertices.Add(new PointF((int)(60 * Math.Cos(angle)), (int)(60 * Math.Sin(angle))));
            }
            f.SyncSidesFromVertices();
            return f;
        }

        // Синхронизация стилей с вершинами
        private void SyncSidesFromVertices()
        {
            var oldStyles = Sides.ToList();
            Sides.Clear();
            for (int i = 0; i < Vertices.Count; i++)
            {
                SideStyle style;
                if (i < oldStyles.Count)
                    style = oldStyles[i].Clone();
                else
                    style = new SideStyle(Vertices[i].X, Vertices[i].Y) { Color = Color.Black, Thickness = 2.0f };
                style.RelativeOffset = Vertices[i];
                Sides.Add(style);
            }
        }

        // Метод для сортировки вершин по часовой стрелке
        public void SortVerticesClockwise()
        {
            if (Vertices.Count < 3) return;
            float cx = Vertices.Average(v => v.X);
            float cy = Vertices.Average(v => v.Y);
            Vertices = Vertices.OrderBy(v => Math.Atan2(v.Y - cy, v.X - cx)).ToList();
            SyncSidesFromVertices();
        }

        public PointF[] GetVertices()
        {
            float scale = Size / 100f;
            return Vertices.Select(v => new PointF(
                Center.X + v.X * scale,
                Center.Y + v.Y * scale
            )).ToArray();
        }

        public override RectangleF GetBounds()
        {
            var vertices = GetVertices();
            if (vertices.Length == 0) return new RectangleF(Center.X, Center.Y, 0, 0);
            float minX = vertices.Min(v => v.X);
            float minY = vertices.Min(v => v.Y);
            float maxX = vertices.Max(v => v.X);
            float maxY = vertices.Max(v => v.Y);
            float padding = MaxThickness / 2f;
            return new RectangleF(minX - padding, minY - padding,
                                 (maxX - minX) + padding * 2, (maxY - minY) + padding * 2);
        }

        public override void Draw(Graphics g)
        {
            var vertices = GetVertices();
            int n = vertices.Length;
            if (n < 2) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (FillColor != Color.Transparent)
            {
                using (var brush = new SolidBrush(FillColor))
                    g.FillPolygon(brush, vertices);
            }

            PointF[] pts = vertices;
            Vector2[] dirs = new Vector2[n];
            Vector2[] normals = new Vector2[n];
            float[] halfThick = Sides.Select(s => s.Thickness / 2f).ToArray();

            for (int i = 0; i < n; i++)
            {
                int next = (i + 1) % n;
                Vector2 side = new Vector2(pts[next].X - pts[i].X, pts[next].Y - pts[i].Y);
                float len = side.Length();
                dirs[i] = len > 0.1f ? side / len : new Vector2(1, 0);
                Vector2 n1 = new Vector2(-dirs[i].Y, dirs[i].X);
                PointF mid = new PointF((pts[i].X + pts[next].X) / 2, (pts[i].Y + pts[next].Y) / 2);
                Vector2 toCenter = new Vector2(Center.X - mid.X, Center.Y - mid.Y);
                normals[i] = Vector2.Dot(n1, toCenter) > 0 ? -n1 : n1;
            }

            PointF[] outer = new PointF[n];
            PointF[] inner = new PointF[n];

            for (int i = 0; i < n; i++)
            {
                int prev = (i - 1 + n) % n;
                int curr = i;
                float det = dirs[prev].X * dirs[curr].Y - dirs[prev].Y * dirs[curr].X;

                if (Math.Abs(det) < 1e-4)
                {
                    Vector2 shift = normals[curr] * halfThick[curr];
                    outer[i] = new PointF(pts[i].X + shift.X, pts[i].Y + shift.Y);
                    inner[i] = new PointF(pts[i].X - shift.X, pts[i].Y - shift.Y);
                }
                else
                {
                    outer[i] = GetIntersect(pts[i], dirs[prev], normals[prev], halfThick[prev],
                                            dirs[curr], normals[curr], halfThick[curr], true);
                    inner[i] = GetIntersect(pts[i], dirs[prev], normals[prev], halfThick[prev],
                                            dirs[curr], normals[curr], halfThick[curr], false);
                }
            }

            for (int i = 0; i < n; i++)
            {
                int next = (i + 1) % n;
                PointF[] quad = { outer[i], outer[next], inner[next], inner[i] };
                using (var brush = new SolidBrush(Sides[i].Color))
                    g.FillPolygon(brush, quad);

                if (Sides[i] == HighlightedSide)
                {
                    using (Pen highlight = new Pen(Color.Cyan, 3) { DashStyle = DashStyle.Dash })
                        g.DrawPolygon(highlight, quad);
                }
            }
        }

        private PointF GetIntersect(PointF P, Vector2 d1, Vector2 n1, float h1,
                                    Vector2 d2, Vector2 n2, float h2, bool isOuter)
        {
            float s = isOuter ? 1 : -1;
            Vector2 P1 = new Vector2(P.X, P.Y) + n1 * h1 * s;
            Vector2 P2 = new Vector2(P.X, P.Y) + n2 * h2 * s;
            float det = (-d1.X) * (-d2.Y) - (-d1.Y) * (-d2.X);
            Vector2 rhs = P2 - P1;
            float u = ((-d1.X) * rhs.Y - (-d1.Y) * rhs.X) / det;
            Vector2 res = P2 + d2 * u;
            return new PointF(res.X, res.Y);
        }

        public override bool Contains(Point p)
        {
            var poly = GetVertices();
            if (poly.Length < 3) return false;
            bool res = false;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
            {
                if (((poly[i].Y > p.Y) != (poly[j].Y > p.Y)) &&
                    (p.X < (poly[j].X - poly[i].X) * (p.Y - poly[i].Y) / (poly[j].Y - poly[i].Y) + poly[i].X))
                    res = !res;
            }
            return res;
        }

        public override Figure Clone()
        {
            Polygon clone = new Polygon(new Point(this.BaseLocation.X, this.BaseLocation.Y), this.Name);
            clone.Vertices = this.Vertices.Select(v => new PointF(v.X, v.Y)).ToList();
            this.CopyBaseProperties(clone);
            clone.SyncSidesFromVertices();
            return clone;
        }
    }
}