using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Lab1.Shapes
{
    public class Circle : Figure
    {
        public Circle(Point center, string name = "Круг") : base(center)
        {
            Name = name;
            Sides.Add(new SideStyle(50, 0));
        }

        public override void Draw(Graphics g)
        {
            float scale = Size / 100f;
            float r = Math.Abs(Sides[0].RelativeOffset.X) * scale;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (FillColor != Color.Transparent)
            {
                using (Brush b = new SolidBrush(FillColor))
                    g.FillEllipse(b, Center.X - r, Center.Y - r, r * 2, r * 2);
            }
            using (Pen p = new Pen(Sides[0].Color, Sides[0].Thickness))
                g.DrawEllipse(p, Center.X - r, Center.Y - r, r * 2, r * 2);

            // ИСПРАВЛЕНИЕ 3: Подсветка для круга
            if (Sides.Count > 0 && Sides[0] == HighlightedSide)
            {
                using (Pen highlight = new Pen(Color.Cyan, 3) { DashStyle = DashStyle.Dash })
                {
                    g.DrawEllipse(highlight, Center.X - r, Center.Y - r, r * 2, r * 2);
                }
            }
        }

        public override RectangleF GetBounds()
        {
            float scale = Size / 100f;
            float r = Math.Abs(Sides[0].RelativeOffset.X) * scale;
            float padding = MaxThickness / 2f;
            return new RectangleF(Center.X - r - padding, Center.Y - r - padding, (r * 2) + padding * 2, (r * 2) + padding * 2);
        }

        public override bool Contains(Point p)
        {
            float scale = Size / 100f;
            float r = Math.Abs(Sides[0].RelativeOffset.X) * scale;
            return (Math.Pow(p.X - Center.X, 2) + Math.Pow(p.Y - Center.Y, 2)) <= r * r;
        }

        public override Figure Clone()
        {
            Circle clone = new Circle(new Point(this.BaseLocation.X, this.BaseLocation.Y), this.Name);
            clone.Sides.Clear();
            this.CopyBaseProperties(clone);
            return clone;
        }
    }
}