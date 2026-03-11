using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Lab1.Shapes;

namespace Lab1
{
    public partial class Form1 : Form
    {
        public List<Figure> figures = new List<Figure>();
        private bool isDragging = false;
        private Point lastMousePos;
        private bool isFullScreen = false;
        private FormBorderStyle lastStyle;
        private FormWindowState lastState;
        public List<Figure> selectedFigures = new List<Figure>(); // Вместо одиночного selectedFigure
        private bool isSelecting = false;
        private Point selectionStart;
        private Rectangle selectionRect;

        private EditorForm currentEditor = null;
        private FiguresListForm listForm = null;

        // Для рисования
        private bool isDrawingMode = false;
        private List<Point> tempPoints = new List<Point>();
        private Point currentMousePos;
        private Dictionary<string, CompositeFigure> customTemplates = new Dictionary<string, CompositeFigure>();

        public Form1()
        {
            InitializeComponent();

            // Настраиваем события для новых кнопок на тулбаре
            btnToggleDraw.Click += (s, e) => {
                isDrawingMode = !isDrawingMode;
                btnToggleDraw.Text = isDrawingMode ? "Рисование (Вкл)" : "Рисование (Выкл)";
                btnToggleDraw.BackColor = isDrawingMode ? Color.MediumSeaGreen : Color.FromArgb(63, 63, 70);
                tempPoints.Clear();
                RefreshCanvas();
            };

            btnOpenList.Click += (s, e) => {
                if (listForm == null || listForm.IsDisposed) listForm = new FiguresListForm(this);
                listForm.Show();
            };

            btnAddShape.Click += (s, e) => CreateFigureFromUI();
            btnClearAll.Click += (s, e) => {
                figures.Clear();
                selectedFigures.Clear();
                UpdateListForm();
                RefreshCanvas();
            };
            btnFullScreen.Click += (s, e) => ToggleFullScreen();
            this.KeyDown += (s, e) => { 
                if (e.KeyCode == Keys.F11) ToggleFullScreen();
                if (e.Control && e.KeyCode == Keys.G) GroupAndSaveTemplate();
            };

            canvasPanel.Paint += CanvasPanel_Paint;
            canvasPanel.MouseDown += CanvasPanel_MouseDown;
            canvasPanel.MouseMove += CanvasPanel_MouseMove;
            canvasPanel.MouseUp += (s, e) => isDragging = false;
        }

        public void UpdateListForm()
        {
            if (listForm != null && !listForm.IsDisposed) listForm.RefreshList();
        }

        public void RefreshCanvas()
        {
            canvasPanel.Invalidate();
        }

        private void ToggleFullScreen()
        {
            if (!isFullScreen)
            {
                lastStyle = this.FormBorderStyle;
                lastState = this.WindowState;
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
            }
            else
            {
                this.FormBorderStyle = lastStyle;
                this.WindowState = lastState;
            }
            isFullScreen = !isFullScreen;
        }

        private void GroupAndSaveTemplate()
        {
            if (selectedFigures.Count < 2) return;

            string templateName = "Сборка " + (customTemplates.Count + 1);
            var newGroup = new CompositeFigure(selectedFigures.ToList(), templateName);

            foreach (var f in selectedFigures) figures.Remove(f);
            figures.Add(newGroup);

            selectedFigures.Clear();
            selectedFigures.Add(newGroup);

            customTemplates.Add(templateName, newGroup);
            if (!cbShapeType.Items.Contains(templateName)) // Проверка, чтобы не дублировать в списке
                cbShapeType.Items.Add(templateName);

            RefreshCanvas();
        }
        private void CreateFigureFromUI()
        {
            Point center = new Point(canvasPanel.Width / 2, canvasPanel.Height / 2);
            Figure f = null;
            string selectedType = cbShapeType.SelectedItem?.ToString();

            switch (selectedType)
            {
                case "Прямоугольник": f = CustomPolygon.CreateRectangle(center); break;
                case "Треугольник": f = CustomPolygon.CreateTriangle(center); break;
                case "Круг": f = new Circle(center); break;
                case "Трапеция": f = CustomPolygon.CreateTrapezium(center); break;
                case "Пятиугольник": f = CustomPolygon.CreatePentagon(center); break;
                default:
                   
                    if (selectedType != null && customTemplates.ContainsKey(selectedType))
                    {
                        
                        f = customTemplates[selectedType].Clone();
                        
                        f.BaseLocation = center;
                    }
                    break;
            }

            if (f != null)
            {
                figures.Add(f);
                selectedFigures.Clear();
                selectedFigures.Add(f);
                UpdateListForm();
                RefreshCanvas();
            }
        }

        private void CanvasPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            foreach (var fig in figures)
            {
                fig.Draw(e.Graphics);
            }

            // Отрисовка процесса рисования линии
            if (isDrawingMode)
            {
                if (tempPoints.Count > 1)
                {
                    e.Graphics.DrawLines(Pens.Gray, tempPoints.ToArray());

                    // ИСПРАВЛЕНИЕ 2: Отрисовка угла между точками
                    for (int i = 1; i < tempPoints.Count; i++)
                    {
                        Point p1 = tempPoints[i - 1];
                        Point p2 = tempPoints[i];

                        // Вычисляем угол в радианах, переводим в градусы
                        double angleRad = Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
                        double angleDeg = angleRad * 180.0 / Math.PI;
                        if (angleDeg < 0) angleDeg += 360; // Делаем угол положительным (0-360)

                        string text = $"{Math.Round(angleDeg)}°";

                        // Находим середину отрезка, чтобы разместить там текст
                        PointF mid = new PointF((p1.X + p2.X) / 2f + 5, (p1.Y + p2.Y) / 2f - 15);
                        e.Graphics.DrawString(text, new Font("Segoe UI", 10, FontStyle.Bold), Brushes.Blue, mid);
                    }
                }
                foreach (var p in tempPoints) e.Graphics.FillRectangle(Brushes.Red, p.X - 2, p.Y - 2, 4, 4);
            }

            
            if (!isDrawingMode)
            {
                foreach (var fig in selectedFigures)
                {
                    using (Pen p = new Pen(Color.Red, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                        e.Graphics.DrawEllipse(p, fig.BaseLocation.X - 5, fig.BaseLocation.Y - 5, 10, 10);

                    RectangleF bounds = fig.GetBounds();
                    using (Pen borderPen = new Pen(Color.Red, 1))
                        e.Graphics.DrawRectangle(borderPen, bounds.X - 5, bounds.Y - 5, bounds.Width + 10, bounds.Height + 10);
                }
            }

            if (isDrawingMode && tempPoints.Count > 0)
            {
                Point lastPoint = tempPoints[tempPoints.Count - 1];

                // Рисуем направляющую линию до курсора (пунктиром)
                using (Pen guidePen = new Pen(Color.Gray, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                {
                    e.Graphics.DrawLine(guidePen, lastPoint, currentMousePos);
                }

                // Вычисляем угол с помощью арктангенса
                double dx = currentMousePos.X - lastPoint.X;
                double dy = currentMousePos.Y - lastPoint.Y;
                double angleInDegrees = Math.Atan2(dy, dx) * (180.0 / Math.PI);

                // Приводим угол к понятному диапазону 0-360 градусов
                if (angleInDegrees < 0) angleInDegrees += 360;

                // Форматируем текст (округляем до 1 знака после запятой)
                string angleText = $"{Math.Round(angleInDegrees, 1)}°";

                // Отрисовываем текст рядом с курсором с красивой подложкой для читаемости
                PointF textPos = new PointF(currentMousePos.X + 15, currentMousePos.Y + 15);
                SizeF textSize = e.Graphics.MeasureString(angleText, this.Font);

                // Полупрозрачный белый фон под текст
                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(200, 255, 255, 255)))
                {
                    e.Graphics.FillRectangle(bgBrush, textPos.X, textPos.Y, textSize.Width, textSize.Height);
                }

                e.Graphics.DrawString(angleText, new Font("Segoe UI", 10, FontStyle.Bold), Brushes.DarkBlue, textPos);
            }

            if (isSelecting)
            {
                // Полупрозрачный синий фон
                using (SolidBrush selectionBrush = new SolidBrush(Color.FromArgb(50, Color.DodgerBlue)))
                {
                    e.Graphics.FillRectangle(selectionBrush, selectionRect);
                }

                // Синяя пунктирная граница
                using (Pen selectionPen = new Pen(Color.DodgerBlue, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                {
                    e.Graphics.DrawRectangle(selectionPen, selectionRect);
                }
            }
            using (Pen selectPen = new Pen(Color.Orange, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
            {
                foreach (var fig in selectedFigures)
                {
                    var bounds = fig.GetBounds();
                    // Рисуем чуть расширенную рамку вокруг фигуры
                    e.Graphics.DrawRectangle(selectPen, bounds.X - 2, bounds.Y - 2, bounds.Width + 4, bounds.Height + 4);
                }
            }

            // Рисуем общую рамку выделения (когда тянем мышью)
            if (isSelecting)
            {
                using (Pen p = new Pen(Color.DodgerBlue, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                using (SolidBrush b = new SolidBrush(Color.FromArgb(50, Color.LightSkyBlue)))
                {
                    e.Graphics.FillRectangle(b, selectionRect);
                    e.Graphics.DrawRectangle(p, selectionRect);
                }
            }
        }

        private void CanvasPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (isDrawingMode)
            {
                if (e.Button == MouseButtons.Left)
                {
                    tempPoints.Add(e.Location);
                    RefreshCanvas();
                }
                else if (e.Button == MouseButtons.Right && tempPoints.Count > 2)
                {
                    Point center = tempPoints[0];
                    var newPoly = new CustomPolygon(center, "Нарисованная фигура");
                    foreach (var p in tempPoints)
                    {
                        newPoly.Sides.Add(new SideStyle(p.X - center.X, p.Y - center.Y));
                    }
                    figures.Add(newPoly);
                    tempPoints.Clear();
                    UpdateListForm();
                    RefreshCanvas();
                }
                return;
            }

            lastMousePos = e.Location;
            bool hit = false;
            bool isMultiSelect = (Control.ModifierKeys & Keys.Control) == Keys.Control;

            // 1. Проверяем попадание в фигуры (с конца списка, чтобы брать верхние)
            for (int i = figures.Count - 1; i >= 0; i--)
            {
                if (figures[i].Contains(e.Location))
                {
                    hit = true;
                    if (!isMultiSelect && !selectedFigures.Contains(figures[i]))
                    {
                        selectedFigures.Clear();
                    }

                    if (!selectedFigures.Contains(figures[i]))
                        selectedFigures.Add(figures[i]);

                    isDragging = true;
                    break;
                }
            }

            // 2. Если кликнули в пустоту — начинаем выделение рамкой
            if (!hit)
            {
                if (!isMultiSelect) selectedFigures.Clear();
                isSelecting = true;
                selectionStart = e.Location;
                selectionRect = new Rectangle(e.Location, new Size(0, 0));
            }

            RefreshCanvas();
        }

        private void CanvasPanel_MouseMove(object sender, MouseEventArgs e)
        {
            currentMousePos = e.Location; // Важно для отрисовки угла

            if (isDragging)
            {
                int dx = e.X - lastMousePos.X;
                int dy = e.Y - lastMousePos.Y;
                foreach (var fig in selectedFigures) fig.Move(dx, dy);
                lastMousePos = e.Location;
            }
            else if (isSelecting)
            {
                // Формируем прямоугольник так, чтобы его можно было тянуть в любую сторону
                selectionRect = new Rectangle(
                    Math.Min(selectionStart.X, e.X),
                    Math.Min(selectionStart.Y, e.Y),
                    Math.Abs(selectionStart.X - e.X),
                    Math.Abs(selectionStart.Y - e.Y));
            }

            RefreshCanvas();
        }

        private void CanvasPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (isSelecting)
            {
                // Финальный расчет выделенных фигур
                foreach (var fig in figures)
                {
                    if (selectionRect.IntersectsWith(Rectangle.Round(fig.GetBounds())))
                    {
                        if (!selectedFigures.Contains(fig)) selectedFigures.Add(fig);
                    }
                }
            }

            isDragging = false;
            isSelecting = false; // СБРОС СОСТОЯНИЯ
            selectionRect = new Rectangle(0, 0, 0, 0);
            RefreshCanvas();
        }
    }
}