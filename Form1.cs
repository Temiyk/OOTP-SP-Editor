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
        public FigureArray figures = new FigureArray(); 

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

            this.Load += Form1_Load;

            btnToggleDraw.Click += (s, e) => {
                isDrawingMode = !isDrawingMode;
                btnToggleDraw.Text = isDrawingMode ? "Рисование (Вкл)" : "Рисование (Выкл)";
                btnToggleDraw.BackColor = isDrawingMode ? Color.MediumSeaGreen : Color.FromArgb(63, 63, 70);
                tempPoints.Clear();
                UpdateDrawingUI();   // <-- этот вызов должен быть
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

            btnSave.Click += (s, e) =>
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "JSON файлы (*.json)|*.json|Текстовые файлы (*.txt)|*.txt";
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        FileManager.SaveToFile(figures, sfd.FileName);
                        MessageBox.Show("Фигуры успешно сохранены!", "Сохранение");
                    }
                }
            };

            btnLoad.Click += (s, e) =>
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "JSON файлы (*.json)|*.json|Текстовые файлы (*.txt)|*.txt";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        figures = FileManager.LoadFromFile(ofd.FileName);
                        selectedFigures.Clear();
                        if (currentEditor != null && !currentEditor.IsDisposed) currentEditor.Close();
                        if (listForm != null && !listForm.IsDisposed) listForm.RefreshList();
                        RefreshCanvas();
                        MessageBox.Show("Фигуры загружены на холст!", "Загрузка");
                    }
                }
            };

            btnAddPointByParams.Click += (s, e) => {
                if (!isDrawingMode || tempPoints == null || tempPoints.Count == 0) return;
                if (nudAngle == null || nudLength == null) return;

                int inputAngle = (int)nudAngle.Value;
                double length = (double)nudLength.Value;

                double absoluteAngleDeg = inputAngle;

                if (tempPoints.Count >= 2)
                {
                    Point last = tempPoints[tempPoints.Count - 1]; // Точка B
                    Point prev = tempPoints[tempPoints.Count - 2]; // Точка A

                    double angBA = Math.Atan2(prev.Y - last.Y, prev.X - last.X) * 180.0 / Math.PI;

                    absoluteAngleDeg = angBA + inputAngle;
                }

                double angleRad = absoluteAngleDeg * Math.PI / 180.0;
                Point currentLastPoint = tempPoints.Last();
                int newX = currentLastPoint.X + (int)Math.Round(length * Math.Cos(angleRad));
                int newY = currentLastPoint.Y + (int)Math.Round(length * Math.Sin(angleRad));

                tempPoints.Add(new Point(newX, newY));
                RefreshCanvas();
            };

            canvasPanel.Paint += CanvasPanel_Paint;
            canvasPanel.MouseDown += CanvasPanel_MouseDown;
            canvasPanel.MouseMove += CanvasPanel_MouseMove;

            canvasPanel.MouseUp += (s, e) => {
                isDragging = false;
                RefreshCanvas();
            };

            btnSaveSelected.Click += (s, e) =>
            {
                // 1. Проверяем, есть ли вообще выделенные фигуры
                if (selectedFigures.Count == 0)
                {
                    MessageBox.Show("Нет выделенных фигур для сохранения.");
                    return;
                }

                // 2. Открываем диалоговое окно для выбора места сохранения
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "JSON файлы (*.json)|*.json|Текстовые файлы (*.txt)|*.txt";
                    sfd.Title = "Сохранить выделенные фигуры как...";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        // 3. Создаем временный объект FigureArray, так как FileManager работает с ним
                        FigureArray tempArray = new FigureArray();

                        // 4. Копируем все выделенные фигуры во временный массив
                        foreach (var fig in selectedFigures)
                        {
                            tempArray.Add(fig);
                        }
                        // 5. Вызываем сохранение целого массива фигур
                        FileManager.SaveToFile(tempArray, sfd.FileName);

                        // 6. Сообщаем пользователю об успехе
                        MessageBox.Show($"Успешно сохранено фигур: {selectedFigures.Count}", "Сохранение");
                    }
                }
            };

            btnLoadAdd.Click += (s, e) =>
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Файл фигуры (*.json)|*.json|Текстовый файл (*.txt)|*.txt";
                    ofd.Title = "Выберите файл с фигурой(ами)";
                    ofd.Multiselect = false;
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            List<Figure> loadedFigures = FileManager.LoadFiguresFromFile(ofd.FileName);
                            if (loadedFigures.Count == 0)
                            {
                                MessageBox.Show("Файл не содержит фигур.");
                                return;
                            }

                            // Рекурсивный помощник для переназначения ID, чтобы не забыть вложенные группы
                            Action<Figure> assignNewId = null;
                            assignNewId = (f) => {
                                f.Id = Figure.GetCurrentMaxId();
                                Figure.SetNextId(f.Id + 1);
                                if (f is CompositeFigure comp)
                                {
                                    foreach (var c in comp.Children) assignNewId(c);
                                }
                            };

                            // Добавляем все загруженные фигуры на холст с НОВЫМИ ID
                            foreach (var fig in loadedFigures)
                            {
                                assignNewId(fig);
                                figures.Add(fig);
                            }

                            // Обновляем интерфейс
                            selectedFigures.Clear();
                            UpdateListForm();
                            RefreshCanvas();
                            MessageBox.Show($"Загружено фигур: {loadedFigures.Count}.", "Добавление");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Ошибка при загрузке: " + ex.Message);
                        }
                    }
                }
            };


            this.KeyPreview = true; 
            this.KeyDown += (s, e) => {
                if (e.Control && e.KeyCode == Keys.S)
                {
                    
                    using (SaveFileDialog sfd = new SaveFileDialog())
                    {
                        sfd.Filter = "Текстовые файлы (*.txt)|*.txt|JSON файлы (*.json)|*.json";
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            FileManager.SaveToFile(figures, sfd.FileName);
                            MessageBox.Show("Фигуры успешно сохранены!", "Сохранение");
                        }
                    }
                }
                if (e.Control && e.KeyCode == Keys.O)
                {
                    
                    using (OpenFileDialog ofd = new OpenFileDialog())
                    {
                        ofd.Filter = "Текстовые файлы (*.txt)|*.txt|JSON файлы (*.json)|*.json";
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            figures = FileManager.LoadFromFile(ofd.FileName);
                            selectedFigures.Clear();
                            UpdateListForm();
                            RefreshCanvas();
                            MessageBox.Show("Фигуры загружены на холст!", "Загрузка");
                        }
                    }
                }
            };
        }

        public void UpdateListForm()
        {
            if (listForm != null && !listForm.IsDisposed) listForm.RefreshList();
        }

        private void UpdateDrawingUI()
        {
            if (nudAngle == null || nudLength == null || btnAddPointByParams == null)
                return;

            bool drawingActive = isDrawingMode;
            nudAngle.Visible = drawingActive;
            nudLength.Visible = drawingActive;
            btnAddPointByParams.Visible = drawingActive;

            btnAddPointByParams.Enabled = drawingActive && tempPoints != null && tempPoints.Count > 0;

            if (btnSaveSelected != null)
            {
                btnSaveSelected.Visible = !drawingActive;
                btnSaveSelected.Enabled = !drawingActive;
            }
            if (btnLoadAdd != null)
            {
                btnLoadAdd.Visible = !drawingActive;
                btnLoadAdd.Enabled = !drawingActive;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            foreach (var fig in figures)
            {
                fig.Draw(e.Graphics);
            }
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
                case "Прямоугольник": f = Polygon.CreateRectangle(center); break;
                case "Треугольник": f = Polygon.CreateTriangle(center); break;
                case "Круг": f = Ellipse.CreateCircle(center); break;
                case "Эллипс": f = new Ellipse(center, 60, 30, "Эллипс"); break;
                case "Трапеция": f = Polygon.CreateTrapezium(center); break;
                case "Пятиугольник": f = Polygon.CreatePentagon(center); break;
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

                        double absAngle = Math.Atan2(p2.Y - p1.Y, p2.X - p1.X) * 180.0 / Math.PI;
                        double displayAngle = absAngle;

                        // ИСПРАВЛЕНИЕ: Высчитываем относительный угол для подписей
                        if (i >= 2)
                        {
                            Point p0 = tempPoints[i - 2];
                            double prevAbsAngle = Math.Atan2(p1.Y - p0.Y, p1.X - p0.X) * 180.0 / Math.PI;
                            displayAngle = absAngle - prevAbsAngle;
                        }

                        // Нормализация до 0-359
                        displayAngle = Math.Round(displayAngle);
                        while (displayAngle < 0) displayAngle += 360;
                        while (displayAngle >= 360) displayAngle -= 360;

                        double length = Math.Round(Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2)));

                        string text = $"{displayAngle}° | L: {length}px";
                        PointF mid = new PointF((p1.X + p2.X) / 2f + 5, (p1.Y + p2.Y) / 2f - 15);
                        e.Graphics.DrawString(text, new Font("Segoe UI", 9, FontStyle.Bold), Brushes.Blue, mid);
                    }
                }

                if (tempPoints.Count > 0)
                {
                    Point lastPoint = tempPoints[tempPoints.Count - 1];
                    using (Pen guidePen = new Pen(Color.Gray, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                    {
                        e.Graphics.DrawLine(guidePen, lastPoint, currentMousePos);
                    }

                    double dx = currentMousePos.X - lastPoint.X;
                    double dy = currentMousePos.Y - lastPoint.Y;

                    double absAngle = Math.Atan2(dy, dx) * 180.0 / Math.PI;
                    double displayAngle = absAngle;

                    if (tempPoints.Count >= 2)
                    {
                        Point prevPoint = tempPoints[tempPoints.Count - 2];
                        double prevAbsAngle = Math.Atan2(lastPoint.Y - prevPoint.Y, lastPoint.X - prevPoint.X) * 180.0 / Math.PI;
                        displayAngle = absAngle - prevAbsAngle;
                    }

                    displayAngle = Math.Round(displayAngle);
                    while (displayAngle < 0) displayAngle += 360;
                    while (displayAngle >= 360) displayAngle -= 360;

                    double dist = Math.Round(Math.Sqrt(dx * dx + dy * dy));

                    string infoText = $"{displayAngle}°\nL: {dist}px";
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
                        e.Graphics.DrawRectangle(selectPen, bounds.X, bounds.Y, bounds.Width, bounds.Height);

                        using (Pen p = new Pen(Color.Red, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                            e.Graphics.DrawEllipse(p, fig.BaseLocation.X - 5, fig.BaseLocation.Y - 5, 10, 10);

                        if (fig is Ellipse el)
                        {
                            // Вычисляем масштаб (по умолчанию 100% = 1.0)[cite: 1, 2]
                            float scale = el.Size / 100f;

                            // Пересчитываем относительные координаты фокусов в абсолютные координаты на экране[cite: 1, 2]
                            // Мы берем Центр фигуры и добавляем к нему смещение фокуса, умноженное на масштаб[cite: 1, 2]
                            PointF f1Abs = new PointF(el.Center.X + el.Focus1.X * scale, el.Center.Y + el.Focus1.Y * scale);
                            PointF f2Abs = new PointF(el.Center.X + el.Focus2.X * scale, el.Center.Y + el.Focus2.Y * scale);

                            // Рисуем синие точки размером 8x8 пикселей[cite: 6]
                            using (Brush focusBrush = new SolidBrush(Color.Blue))
                            {
                                e.Graphics.FillEllipse(focusBrush, f1Abs.X - 4, f1Abs.Y - 4, 8, 8);
                                e.Graphics.FillEllipse(focusBrush, f2Abs.X - 4, f2Abs.Y - 4, 8, 8);
                            }
                        }
                    }
                }
            }
        }
        private void CanvasPanel_MouseDown(object sender, MouseEventArgs e)
        {
            foreach (var fig in figures)
            {
                fig.HighlightedSide = null;
            }

            if (isDrawingMode)
            {
                if (e.Button == MouseButtons.Left)
                {
                    tempPoints.Add(e.Location);
                    UpdateDrawingUI();   
                    RefreshCanvas();
                }
                else if (e.Button == MouseButtons.Right && tempPoints.Count > 2)
                {
                    Point center = tempPoints[0];
                    var newPoly = new Polygon(center, "Нарисованная фигура");

                    foreach (var p in tempPoints)
                    {
                        newPoly.Vertices.Add(new PointF(p.X - center.X, p.Y - center.Y));
                    }

                    newPoly.SortVerticesClockwise();

                    figures.Add(newPoly);

                    tempPoints.Clear();
                    UpdateDrawingUI();
                    RefreshCanvas();
                    UpdateListForm();
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

                    if (e.Button == MouseButtons.Left)
                    {
                        isDragging = true;
                    }

                    if (e.Button == MouseButtons.Right)
                    {
                      
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

            if (!hit && !isMultiSelect)
            {
                selectedFigures.Clear();
            }

            RefreshCanvas();
        }

        private void CanvasPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawingMode && tempPoints != null && tempPoints.Count > 0)
            {
                Point last = tempPoints.Last();
                double dx = currentMousePos.X - last.X;
                double dy = currentMousePos.Y - last.Y;
                double angBC = Math.Atan2(dy, dx) * 180.0 / Math.PI;
                double displayAngle = angBC;

                if (tempPoints.Count >= 2)
                {
                    Point prev = tempPoints[tempPoints.Count - 2];
                    double angBA = Math.Atan2(prev.Y - last.Y, prev.X - last.X) * 180.0 / Math.PI;
                    displayAngle = angBC - angBA;
                }

                int finalAngle = (int)Math.Round(displayAngle);
                while (finalAngle < 0) finalAngle += 360;
                while (finalAngle >= 360) finalAngle -= 360;

                double length = Math.Round(Math.Sqrt(dx * dx + dy * dy));

                if (nudAngle != null)
                {
                    decimal newAngle = finalAngle;
                    if (newAngle < nudAngle.Minimum) newAngle = nudAngle.Minimum;
                    if (newAngle > nudAngle.Maximum) newAngle = nudAngle.Maximum;
                    nudAngle.Value = newAngle;
                }
                if (nudLength != null)
                {
                    decimal newLength = (decimal)length;
                    if (newLength < nudLength.Minimum) newLength = nudLength.Minimum;
                    if (newLength > nudLength.Maximum) newLength = nudLength.Maximum;
                    nudLength.Value = newLength;
                }

                if (nudAngle != null && !nudAngle.Visible) UpdateDrawingUI();
            }
            else
            {
                if (nudAngle != null && nudAngle.Visible)
                    UpdateDrawingUI();
            }

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
        private void btnToggleDraw_Click(object sender, EventArgs e)
        {
            isDrawingMode = !isDrawingMode;
            btnToggleDraw.Text = isDrawingMode ? "Рисование (Вкл)" : "Рисование (Выкл)";
            btnToggleDraw.BackColor = isDrawingMode ? Color.MediumSeaGreen : Color.FromArgb(63, 63, 70);
            tempPoints.Clear();
            UpdateDrawingUI();   // Показываем/скрываем элементы
            RefreshCanvas();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (nudAngle != null && nudLength != null && btnAddPointByParams != null)
            {
                nudAngle.Location = new Point(700, 18);
                nudLength.Location = new Point(770, 18);
                btnAddPointByParams.Location = new Point(850, 16);

                nudAngle.DecimalPlaces = 0;
                nudAngle.Increment = 1;
            }
        }
    }
}