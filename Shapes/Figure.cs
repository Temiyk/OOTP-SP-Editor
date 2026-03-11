using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Lab1.Shapes
{
    public abstract class Figure
    {
        public string Name { get; set; } = "Новая фигура";

        public int Size { get; set; } = 100;
        public Point BaseLocation { get; set; }
        public PointF RelativePivot { get; set; } = new PointF(0, 0);
        public SideStyle HighlightedSide { get; set; } = null;
        public Point Center
        {
            get => new Point(
                BaseLocation.X - (int)RelativePivot.X,
                BaseLocation.Y - (int)RelativePivot.Y
            );
            set
            {
                BaseLocation = new Point(
                    value.X + (int)RelativePivot.X,
                    value.Y + (int)RelativePivot.Y
                );
            }
        }

        public List<SideStyle> Sides { get; set; } = new List<SideStyle>();
        public Color FillColor { get; set; } = Color.Transparent;
        public float MaxThickness => Sides.Count > 0 ? Sides.Max(s => s.Thickness) : 0;

        public Figure(Point center)
        {
            BaseLocation = center;
        }

        public abstract void Draw(Graphics g);
        public abstract bool Contains(Point p);
        public abstract RectangleF GetBounds();
        public abstract Figure Clone();
        public virtual void Move(int dx, int dy)
        {
            BaseLocation = new Point(BaseLocation.X + dx, BaseLocation.Y + dy);
        }
        protected void CopyBaseProperties(Figure clone)
        {
            clone.Name = this.Name;
            clone.Size = this.Size;
            clone.BaseLocation = new Point(this.BaseLocation.X, this.BaseLocation.Y);
            clone.RelativePivot = new PointF(this.RelativePivot.X, this.RelativePivot.Y);
            clone.FillColor = this.FillColor;

            // Глубокое копирование списка сторон
            clone.Sides = new List<SideStyle>();
            foreach (var side in this.Sides)
            {
                clone.Sides.Add(side.Clone());
            }
        }
    }
}