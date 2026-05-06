using Newtonsoft.Json;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Lab1.Shapes
{
    public class CompositeFigure : Figure
    {
        [JsonProperty]
        public List<Figure> Children { get; set; } = new List<Figure>();

        // --- ДОБАВЛЕНО: Конструктор для JSON ---
        [JsonConstructor]
        protected CompositeFigure() { }

        public CompositeFigure(List<Figure> children, string name = "Группа фигур")
            : base(children.Count > 0 ? children[0].BaseLocation : new Point(0, 0))
        {
            Name = name;
            Children = children.ToList();

            if (Children.Count > 0)
            {
                int avgX = (int)Children.Average(c => c.BaseLocation.X);
                int avgY = (int)Children.Average(c => c.BaseLocation.Y);
                BaseLocation = new Point(avgX, avgY);
            }
        }

        public override void Draw(Graphics g)
        {
            foreach (var child in Children) child.Draw(g);
        }

        public override bool Contains(Point p)
        {
            return Children.Any(child => child.Contains(p));
        }

        public override RectangleF GetBounds()
        {
            if (Children.Count == 0) return new RectangleF(BaseLocation.X, BaseLocation.Y, 0, 0);

            float minX = Children.Min(c => c.GetBounds().Left);
            float minY = Children.Min(c => c.GetBounds().Top);
            float maxX = Children.Max(c => c.GetBounds().Right);
            float maxY = Children.Max(c => c.GetBounds().Bottom);

            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }

        public override void Move(int dx, int dy)
        {
            base.Move(dx, dy);
            foreach (var child in Children)
            {
                child.Move(dx, dy);
            }
        }
        public override Figure Clone()
        {
            // Глубокое копирование всех вложенных фигур
            List<Figure> clonedChildren = this.Children.Select(child => child.Clone()).ToList();

            CompositeFigure clone = new CompositeFigure(clonedChildren, this.Name);
            this.CopyBaseProperties(clone);
            return clone;
        }
    }
}