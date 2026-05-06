using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Newtonsoft.Json;

namespace Lab1.Shapes
{
    public class Ellipse : Figure
    {
        [JsonProperty]
        public PointF Focus1 { get; set; }
        [JsonProperty]
        public PointF Focus2 { get; set; }
        [JsonProperty]
        public float DistanceSum { get; set; } // сумма расстояний до фокусов (2a)

        [JsonIgnore]
        public float RadiusX => DistanceSum / 2f; // большая полуось
        [JsonIgnore]
        public float RadiusY
        {
            get
            {
                float dx = Focus2.X - Focus1.X;
                float dy = Focus2.Y - Focus1.Y;
                float c = (float)Math.Sqrt(dx * dx + dy * dy) / 2f; // половина фокального расстояния
                float a = DistanceSum / 2f;
                return (float)Math.Sqrt(Math.Max(0, a * a - c * c)); // малая полуось
            }
        }

        [JsonIgnore]
        public float Angle
        {
            get
            {
                float dx = Focus2.X - Focus1.X;
                float dy = Focus2.Y - Focus1.Y;
                return (float)(Math.Atan2(dy, dx) * 180.0 / Math.PI);
            }
        }

        [JsonIgnore]
        public SideStyle StrokeStyle
        {
            get => Sides.Count > 0 ? Sides[0] : null;
            set
            {
                if (Sides.Count == 0)
                    Sides.Add(value);
                else
                    Sides[0] = value;
            }
        }

        [JsonConstructor]
        protected Ellipse() { }

        public Ellipse(Point center, PointF focus1, PointF focus2, float distanceSum, string name = "Эллипс")
            : base(center)
        {
            Name = name;
            Focus1 = focus1;
            Focus2 = focus2;
            DistanceSum = distanceSum;
            Sides.Add(new SideStyle(0, 0) { Color = Color.Black, Thickness = 2.0f });
        }

        // Упрощённый конструктор для совместимости (эллипс по умолчанию с фокусами)
        public Ellipse(Point center, float radiusX = 50, float radiusY = 30, string name = "Эллипс")
            : base(center)
        {
            Name = name;
            float c = (float)Math.Sqrt(Math.Max(0, radiusX * radiusX - radiusY * radiusY));
            Focus1 = new PointF(-c, 0);
            Focus2 = new PointF(c, 0);
            DistanceSum = radiusX * 2;
            Sides.Add(new SideStyle(0, 0) { Color = Color.Black, Thickness = 2.0f });
        }

        public static Ellipse CreateCircle(Point center, float radius = 50, string name = "Круг")
        {
            return new Ellipse(center, radius, radius, name);
        }

        public override void Draw(Graphics g)
        {
            if (StrokeStyle == null) return;

            float scale = Size / 100f;
            float rx = RadiusX * scale;
            float ry = RadiusY * scale;
            float angle = Angle;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            var oldTransform = g.Transform;
            g.TranslateTransform(Center.X, Center.Y);
            g.RotateTransform(angle);

            if (FillColor != Color.Transparent)
            {
                using (Brush b = new SolidBrush(FillColor))
                    g.FillEllipse(b, -rx, -ry, rx * 2, ry * 2);
            }

            using (Pen p = new Pen(StrokeStyle.Color, StrokeStyle.Thickness))
                g.DrawEllipse(p, -rx, -ry, rx * 2, ry * 2);

            if (StrokeStyle == HighlightedSide)
            {
                using (Pen highlight = new Pen(Color.Cyan, 3) { DashStyle = DashStyle.Dash })
                    g.DrawEllipse(highlight, -rx, -ry, rx * 2, ry * 2);
            }

            g.Transform = oldTransform;
        }

        public override RectangleF GetBounds()
        {
            float scale = Size / 100f;
            float a = RadiusX * scale;
            float b = RadiusY * scale;

            // Переводим угол из градусов в радианы
            double angleRad = Angle * Math.PI / 180.0;

            // Точный математический расчет границ повернутого эллипса
            float halfWidth = (float)Math.Sqrt(Math.Pow(a * Math.Cos(angleRad), 2) + Math.Pow(b * Math.Sin(angleRad), 2));
            float halfHeight = (float)Math.Sqrt(Math.Pow(a * Math.Sin(angleRad), 2) + Math.Pow(b * Math.Cos(angleRad), 2));

            float padding = MaxThickness / 2f;
            float totalWidth = halfWidth * 2 + padding * 2;
            float totalHeight = halfHeight * 2 + padding * 2;

            return new RectangleF(
                Center.X - halfWidth - padding,
                Center.Y - halfHeight - padding,
                totalWidth,
                totalHeight
            );
        }

        public override bool Contains(Point p)
        {
            float scale = Size / 100f;
            float dx = p.X - Center.X;
            float dy = p.Y - Center.Y;
            double angleRad = Angle * Math.PI / 180.0;
            double cos = Math.Cos(-angleRad);
            double sin = Math.Sin(-angleRad);
            double xRot = dx * cos - dy * sin;
            double yRot = dx * sin + dy * cos;

            float rx = RadiusX * scale;
            float ry = RadiusY * scale;
            return (xRot * xRot) / (rx * rx) + (yRot * yRot) / (ry * ry) <= 1.0;
        }

        public override Figure Clone()
        {
            Ellipse clone = new Ellipse(
                new Point(this.BaseLocation.X, this.BaseLocation.Y),
                this.Focus1,
                this.Focus2,
                this.DistanceSum,
                this.Name
            );
            this.CopyBaseProperties(clone);
            return clone;
        }
    }
}