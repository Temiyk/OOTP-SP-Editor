using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Lab1.Shapes;
using System.Linq;

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

        public List<Figure> selectedFigures = new List<Figure>();

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

            // Сбрасываем флаг перетаскивания при отпускании любой кнопки мыши
            canvasPanel.MouseUp += (s, e) => {
                isDragging = false;
                RefreshCanvas();
            };
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
            if (!cbShapeType.Items.Contains(templateName))
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

            foreach (var fig in figures) fig.Draw(e.Graphics);

            if (isDrawingMode)
            {
                if (tempPoints.Count > 1)
                {
                    e.Graphics.DrawLines(Pens.Gray, tempPoints.ToArray());

                    for (int i = 1; i < tempPoints.Count; i++)
                    {
                        Point p1 = tempPoints[i - 1];
                        Point p2 = tempPoints[i];

                        // Расчет угла
                        double angleDeg = Math.Atan2(p2.Y - p1.Y, p2.X - p1.X) * 180.0 / Math.PI;
                        if (angleDeg < 0) angleDeg += 360;

                        // --- НОВОЕ: Расчет длины сегмента ---
                        double length = Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));

                        string text = $"{Math.Round(angleDeg)}° | L: {Math.Round(length)}px";
                        PointF mid = new PointF((p1.X + p2.X) / 2f + 5, (p1.Y + p2.Y) / 2f - 15);
                        e.Graphics.DrawString(text, new Font("Segoe UI", 9, FontStyle.Bold), Brushes.Blue, mid);
                    }
                }

                // Отрисовка "резиновой нити" от последней точки до курсора
                if (tempPoints.Count > 0)
                {
                    Point lastPoint = tempPoints[tempPoints.Count - 1];
                    using (Pen guidePen = new Pen(Color.Gray, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                    {
                        e.Graphics.DrawLine(guidePen, lastPoint, currentMousePos);
                    }

                    double dx = currentMousePos.X - lastPoint.X;
                    double dy = currentMousePos.Y - lastPoint.Y;

                    // --- НОВОЕ: Длина и угол для динамической линии ---
                    double angle = Math.Atan2(dy, dx) * (180.0 / Math.PI);
                    if (angle < 0) angle += 360;
                    double dist = Math.Sqrt(dx * dx + dy * dy);

                    string infoText = $"{Math.Round(angle, 1)}°\nL: {Math.Round(dist)}px";
                    e.Graphics.DrawString(infoText, new Font("Segoe UI", 9, FontStyle.Bold), Brushes.DarkBlue,
                                         currentMousePos.X + 15, currentMousePos.Y + 15);
                }
            }
            else
            {
                // Отрисовка строгих границ (без отступов, единая логика)
                using (Pen selectPen = new Pen(Color.Orange, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                {
                    foreach (var fig in selectedFigures)
                    {
                        var bounds = fig.GetBounds();
                        // Строго по виртуальным границам фигуры
                        e.Graphics.DrawRectangle(selectPen, bounds.X, bounds.Y, bounds.Width, bounds.Height);

                        // Точка центра фигуры (опционально для удобства)
                        using (Pen p = new Pen(Color.Red, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                            e.Graphics.DrawEllipse(p, fig.BaseLocation.X - 5, fig.BaseLocation.Y - 5, 10, 10);
                    }
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

                    // Перетаскиваем только если зажата левая кнопка мыши
                    if (e.Button == MouseButtons.Left)
                    {
                        isDragging = true;
                    }

                    // Если кликнули ПКМ, открываем окно свойств
                    if (e.Button == MouseButtons.Right)
                    {
                        // Если выделено несколько фигур, но мы кликнули ПКМ по одной из них,
                        // логично открыть редактор именно для той, по которой кликнули.
                        // Поэтому мы сбрасываем выделение и оставляем только её (если не зажат Ctrl).
                        if (!isMultiSelect)
                        {
                            selectedFigures.Clear();
                            selectedFigures.Add(figures[i]);
                        }

                        if (currentEditor != null && !currentEditor.IsDisposed) currentEditor.Close();
                        currentEditor = new EditorForm(figures[i], this, canvasPanel);
                        currentEditor.Show();
                    }
                    break;
                }
            }

            // Если кликнули в пустоту — просто сбрасываем выделение
            if (!hit && !isMultiSelect)
            {
                selectedFigures.Clear();
            }

            RefreshCanvas();
        }

        private void CanvasPanel_MouseMove(object sender, MouseEventArgs e)
        {
            currentMousePos = e.Location;

            if (isDragging && e.Button == MouseButtons.Left)
            {
                int dx = e.X - lastMousePos.X;
                int dy = e.Y - lastMousePos.Y;
                foreach (var fig in selectedFigures) fig.Move(dx, dy);
                lastMousePos = e.Location;

                // Обновляем координаты в редакторе, если он открыт
                if (currentEditor != null && !currentEditor.IsDisposed) currentEditor.UpdateCoordinates();
            }

            RefreshCanvas();
        }
    }
}