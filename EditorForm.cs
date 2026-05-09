// EditorForm.cs
using Lab1.Shapes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Lab1
{
    public partial class EditorForm : Form
    {
        private Figure targetFigure;
        private Form1 mainForm;
        private Panel canvas;

        private bool isUpdatingUI = false;

        private Label lblBounds;
        private TextBox txtFigureName, txtId;
        private TextBox txtCX, txtCY, txtRelX, txtRelY, txtScale, txtThick, txtSideRelX, txtSideRelY;
        private TextBox txtSideLength;
        private Panel pnlFillColor, pnlSideColor;
        private ComboBox cbSides;

        private TextBox txtRadiusX, txtRadiusY, txtAngle;

        private Label lblRadiusX, lblRadiusY, lblAngle;
        private Label lblSides, lblSideColor, lblThick, lblSideRel, lblSideLength;
        private Button btnSideColor;
        private Button btnApply;

        private TextBox txtFocus1X, txtFocus1Y, txtFocus2X, txtFocus2Y, txtDistanceSum;
        private Label lblFocus1, lblFocus2, lblDistanceSum;

        private List<SideStyle> flatSides = new List<SideStyle>();

        public EditorForm(Figure figure, Form1 main, Panel canvasPanel)
        {
            this.targetFigure = figure;
            this.mainForm = main;
            this.canvas = canvasPanel;

            InitializeUI();
            LoadData();

            this.FormClosed += (s, e) =>
            {
                if (targetFigure != null) targetFigure.HighlightedSide = null;
                mainForm.RefreshCanvas();
            };
        }

        private void InitializeUI()
        {
            this.Font = new Font("Segoe UI", 12F, FontStyle.Regular);
            this.Text = "Редактор параметров";
            this.Size = new Size(400, 1100);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.Manual;
            this.TopMost = true;

            Rectangle screen = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(screen.Right - this.Width - 10, screen.Top + (screen.Height - this.Height) / 2);

            int y = 10;
            int elementWidth = 360;

            Label lblIdHeader = new Label
            {
                Text = "ID ОБЪЕКТА:",
                Location = new Point(10, y),
                Size = new Size(elementWidth, 15),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.Gray
            };
            this.Controls.Add(lblIdHeader);
            y += 18;

            txtId = new TextBox
            {
                Location = new Point(10, y),
                Size = new Size(elementWidth, 25),
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = SystemColors.Control,
                Font = new Font("Consolas", 10F, FontStyle.Regular),
                ForeColor = Color.DarkBlue,
                TabStop = false
            };
            this.Controls.Add(txtId);
            y += 25;

            Label line = new Label
            {
                BorderStyle = BorderStyle.Fixed3D,
                Height = 2,
                Width = elementWidth,
                Location = new Point(10, y)
            };
            this.Controls.Add(line);
            y += 15;

            AddLabel("Имя фигуры:", ref y);
            txtFigureName = AddTextBox(ref y, elementWidth);
            txtFigureName.TextChanged += (s, e) => { targetFigure.Name = txtFigureName.Text; mainForm.UpdateListForm(); };

            lblBounds = new Label { Location = new Point(10, y), Size = new Size(elementWidth, 90), Text = "Границы: ..." };
            this.Controls.Add(lblBounds); y += 95;

            AddLabel("Координаты центра (X, Y):", ref y);
            txtCX = AddTextBox(ref y, elementWidth);
            txtCY = AddTextBox(ref y, elementWidth);

            AddLabel("Сдвиг центра отн. (RelX, RelY):", ref y);
            txtRelX = AddTextBox(ref y, elementWidth);
            txtRelY = AddTextBox(ref y, elementWidth);

            AddLabel("Масштаб (1.0 = 100%):", ref y);
            txtScale = AddTextBox(ref y, elementWidth);

            AddLabel("Цвет фигуры:", ref y);
            pnlFillColor = new Panel { Location = new Point(10, y + 5), Size = new Size(35, 30), BorderStyle = BorderStyle.FixedSingle };
            Button btnFill = new Button { Text = "Выбрать цвет заливки", Location = new Point(55, y), Size = new Size(elementWidth - 45, 40) };
            btnFill.Click += (s, e) => { PickColor(pnlFillColor); AutoApply(null, null); };
            this.Controls.Add(pnlFillColor); this.Controls.Add(btnFill); y += 45;

            // Общие элементы: цвет и толщина контура
            lblSideColor = AddLabel("Цвет контура/стороны:", ref y);
            pnlSideColor = new Panel { Location = new Point(10, y + 5), Size = new Size(35, 30), BorderStyle = BorderStyle.FixedSingle };
            btnSideColor = new Button { Text = "Выбрать цвет линии", Location = new Point(55, y), Size = new Size(elementWidth - 45, 40) };
            btnSideColor.Click += (s, e) => { PickColor(pnlSideColor); AutoApply(null, null); };
            this.Controls.Add(pnlSideColor); this.Controls.Add(btnSideColor); y += 45;

            lblThick = AddLabel("Толщина контура/стороны:", ref y);
            txtThick = AddTextBox(ref y, elementWidth);

            int ySpecificStart = y;

            // --- Специфические элементы для многоугольника ---
            lblSides = AddLabel("Выбор стороны для редактирования:", ref y);
            cbSides = new ComboBox { Location = new Point(10, y), Size = new Size(elementWidth, 30), DropDownStyle = ComboBoxStyle.DropDownList };
            cbSides.SelectedIndexChanged += (s, e) => LoadSideData();
            this.Controls.Add(cbSides); y += 45;

            lblSideRel = AddLabel("Смещение стороны (RelX, RelY):", ref y);
            txtSideRelX = AddTextBox(ref y, elementWidth);
            txtSideRelY = AddTextBox(ref y, elementWidth);

            lblSideLength = AddLabel("Длина стороны (px):", ref y);
            txtSideLength = AddTextBox(ref y, elementWidth);

            int yAfterPoly = y;

            // --- Специфические элементы для эллипса ---
            y = ySpecificStart;
            lblFocus1 = AddLabel("Фокус 1 (X, Y):", ref y);
            txtFocus1X = AddTextBox(ref y, elementWidth / 2 - 5);
            txtFocus1Y = AddTextBox(ref y, elementWidth / 2 - 5);
            txtFocus1Y.Location = new Point(txtFocus1X.Right + 10, txtFocus1X.Top); y -= 40; // Ставим поля рядом

            lblFocus2 = AddLabel("Фокус 2 (X, Y):", ref y);
            txtFocus2X = AddTextBox(ref y, elementWidth / 2 - 5);
            txtFocus2Y = AddTextBox(ref y, elementWidth / 2 - 5);
            txtFocus2Y.Location = new Point(txtFocus2X.Right + 10, txtFocus2X.Top); y -= 40;

            lblDistanceSum = AddLabel("Сумма расстояний (2a):", ref y);
            txtDistanceSum = AddTextBox(ref y, elementWidth);

            int yAfterEllipse = y;
            y = Math.Max(yAfterPoly, yAfterEllipse);

            btnApply = new Button { Text = "Применить изменения", Location = new Point(10, y + 10), Size = new Size(elementWidth, 40), BackColor = Color.LightGreen };
            btnApply.Click += AutoApply; 
            this.Controls.Add(btnApply);
            y += 50; 

            Button btnDel = new Button { Text = "Удалить фигуру", Location = new Point(10, y + 10), Size = new Size(elementWidth, 45), BackColor = Color.MistyRose };
            btnDel.Click += (s, e) =>
            {
                mainForm.figures.Remove(targetFigure);
                mainForm.selectedFigures.Remove(targetFigure);
                mainForm.RefreshCanvas();
                mainForm.UpdateListForm();
                this.Close();
            };
            this.Controls.Add(btnDel);
        }

        private Label AddLabel(string text, ref int y)
        {
            Label lbl = new Label { Text = text, Location = new Point(10, y), AutoSize = true };
            this.Controls.Add(lbl);
            y += 30;
            return lbl;
        }

        private TextBox AddTextBox(ref int y, int width)
        {
            TextBox tb = new TextBox { Location = new Point(10, y), Size = new Size(width, 30) };
            this.Controls.Add(tb);
            y += 40;
            return tb;
        }

        private void PickColor(Panel p)
        {
            using (ColorDialog cd = new ColorDialog())
            {
                cd.Color = p.BackColor;
                if (cd.ShowDialog() == DialogResult.OK) p.BackColor = cd.Color;
            }
        }

        private void PopulateSides(Figure f, string prefixName)
        {
            if (f is CompositeFigure comp)
            {
                for (int i = 0; i < comp.Children.Count; i++)
                {
                    PopulateSides(comp.Children[i], prefixName + $"Фигура {i + 1} -> ");
                }
            }
            else
            {
                for (int i = 0; i < f.Sides.Count; i++)
                {
                    cbSides.Items.Add(prefixName + $"Сторона {i}");
                    flatSides.Add(f.Sides[i]);
                }
            }
        }

        private void LoadData()
        {
            isUpdatingUI = true;

            // Базовая информация об объекте[cite: 6]
            txtId.Text = targetFigure.Id.ToString().ToUpper();
            txtFigureName.Text = targetFigure.Name;

            UpdateBoundsLabel(); // Обновляем текст с границами[cite: 6]

            // Заполняем общие координаты[cite: 6]
            txtCX.Text = targetFigure.BaseLocation.X.ToString();
            txtCY.Text = targetFigure.BaseLocation.Y.ToString();
            txtRelX.Text = targetFigure.RelativePivot.X.ToString();
            txtRelY.Text = targetFigure.RelativePivot.Y.ToString();
            txtScale.Text = (targetFigure.Size / 100f).ToString();
            pnlFillColor.BackColor = targetFigure.FillColor;

            cbSides.Items.Clear();
            flatSides.Clear();

            bool isEllipse = targetFigure is Ellipse;

            // Управление видимостью полей для ЭЛЛИПСА[cite: 1, 6]
            lblFocus1.Visible = txtFocus1X.Visible = txtFocus1Y.Visible = isEllipse;
            lblFocus2.Visible = txtFocus2X.Visible = txtFocus2Y.Visible = isEllipse;
            lblDistanceSum.Visible = txtDistanceSum.Visible = isEllipse;

            // Управление видимостью полей для МНОГОУГОЛЬНИКА (Прямоугольника)[cite: 3, 6]
            lblSides.Visible = cbSides.Visible = !isEllipse;
            lblSideRel.Visible = txtSideRelX.Visible = txtSideRelY.Visible = !isEllipse;
            lblSideLength.Visible = txtSideLength.Visible = !isEllipse;

            if (isEllipse)
            {
                Ellipse ellipse = (Ellipse)targetFigure;
                // Загружаем данные фокусов[cite: 1]
                txtFocus1X.Text = ellipse.Focus1.X.ToString();
                txtFocus1Y.Text = ellipse.Focus1.Y.ToString();
                txtFocus2X.Text = ellipse.Focus2.X.ToString();
                txtFocus2Y.Text = ellipse.Focus2.Y.ToString();
                txtDistanceSum.Text = ellipse.DistanceSum.ToString();

                if (ellipse.Sides.Count > 0)
                {
                    pnlSideColor.BackColor = ellipse.Sides[0].Color;
                    txtThick.Text = ellipse.Sides[0].Thickness.ToString();
                }
            }
            else
            {
                // Если это прямоугольник или другой многоугольник, загружаем его стороны[cite: 3, 6]
                PopulateSides(targetFigure, "");
            }

            isUpdatingUI = false;

            // Если есть стороны, выбираем первую для редактирования[cite: 6]
            if (cbSides.Items.Count > 0 && !isEllipse) cbSides.SelectedIndex = 0;
        }

        private void UpdateBoundsLabel()
        {
            RectangleF b = targetFigure.GetBounds();
            lblBounds.Text = $"Границы:\nMinX: {b.Left:F0}  MinY: {b.Top:F0}\nMaxX: {b.Right:F0}  MaxY: {b.Bottom:F0}";
        }

        public void UpdateCoordinates()
        {
            UpdateBoundsLabel();

            isUpdatingUI = true;
            if (!txtCX.Focused) txtCX.Text = targetFigure.BaseLocation.X.ToString();
            if (!txtCY.Focused) txtCY.Text = targetFigure.BaseLocation.Y.ToString();
            isUpdatingUI = false;
        }

        private void LoadSideData()
        {
            if (cbSides.SelectedIndex < 0 || targetFigure is Ellipse) return;

            isUpdatingUI = true;

            var side = flatSides[cbSides.SelectedIndex];
            targetFigure.HighlightedSide = side;
            mainForm.RefreshCanvas();

            pnlSideColor.BackColor = side.Color;
            txtThick.Text = side.Thickness.ToString();
            txtSideRelX.Text = side.RelativeOffset.X.ToString();
            txtSideRelY.Text = side.RelativeOffset.Y.ToString();

            Figure parent = FindParentFigure(targetFigure, side);
            if (parent != null)
            {
                int idx = parent.Sides.IndexOf(side);
                int nextIdx = (idx + 1) % parent.Sides.Count;
                var nextSide = parent.Sides[nextIdx];
                double dx = nextSide.RelativeOffset.X - side.RelativeOffset.X;
                double dy = nextSide.RelativeOffset.Y - side.RelativeOffset.Y;
                double length = Math.Sqrt(dx * dx + dy * dy);
                txtSideLength.Text = Math.Round(length).ToString();
            }

            isUpdatingUI = false;
        }

        private Figure FindParentFigure(Figure root, SideStyle side)
        {
            if (root.Sides.Contains(side)) return root;
            if (root is CompositeFigure comp)
            {
                foreach (var child in comp.Children)
                {
                    var found = FindParentFigure(child, side);
                    if (found != null) return found;
                }
            }
            return null;
        }

        private void AutoApply(object sender, EventArgs e)
        {
            if (isUpdatingUI) return;

            ApplyChangesSafe();
        }

        private void ApplyChangesSafe()
        {
            try
            {
                Point newBaseLocation = new Point(int.Parse(txtCX.Text), int.Parse(txtCY.Text));
                PointF newRelativePivot = new PointF(float.Parse(txtRelX.Text), float.Parse(txtRelY.Text));
                Point oldVisualCenter = targetFigure.Center;
                bool isBaseLocationChangedManually = (newBaseLocation != targetFigure.BaseLocation);

                if (targetFigure.RelativePivot != newRelativePivot)
                {
                    Point oldCenter = targetFigure.Center; // Запоминаем текущий визуальный центр
                    targetFigure.RelativePivot = newRelativePivot; // Меняем привязку
                    targetFigure.Center = oldCenter; // Возвращаем центр на место (BaseLocation обновится автоматически)

                    // Обновляем текстовые поля новыми значениями BaseLocation
                    txtCX.Text = targetFigure.BaseLocation.X.ToString();
                    txtCY.Text = targetFigure.BaseLocation.Y.ToString();
                }
                // Иначе, если пользователь вручную ввел новые координаты базы:
                else if (targetFigure.BaseLocation != newBaseLocation)
                {
                    targetFigure.BaseLocation = newBaseLocation;
                }


                targetFigure.RelativePivot = newRelativePivot;
                if (isBaseLocationChangedManually) targetFigure.BaseLocation = newBaseLocation;
                else targetFigure.Center = oldVisualCenter;

                targetFigure.Size = (int)(float.Parse(txtScale.Text) * 100);
                targetFigure.FillColor = pnlFillColor.BackColor;

                if (targetFigure is Ellipse el)
                {
                    // 1. Сначала запоминаем ТЕКУЩИЙ вертикальный радиус (высоту), 
                    // чтобы он не «схлопнулся» при расчетах
                    float currentB = el.RadiusY;

                    // 2. Считываем новые координаты фокусов из текстовых полей
                    PointF newF1 = new PointF(float.Parse(txtFocus1X.Text), float.Parse(txtFocus1Y.Text));
                    PointF newF2 = new PointF(float.Parse(txtFocus2X.Text), float.Parse(txtFocus2Y.Text));

                    // 3. Вычисляем новое расстояние между фокусами (2c)[cite: 1]
                    float dx = newF2.X - newF1.X;
                    float dy = newF2.Y - newF1.Y;
                    float newC = (float)Math.Sqrt(dx * dx + dy * dy) / 2f;

                    // 4. Вычисляем новую большую полуось (a), которая сохранит высоту (currentB)[cite: 1]
                    // Используем формулу: a = sqrt(b^2 + c^2)
                    float newA = (float)Math.Sqrt(currentB * currentB + newC * newC);

                    // 5. Применяем обновленные параметры к эллипсу[cite: 1]
                    el.Focus1 = newF1;
                    el.Focus2 = newF2;
                    el.DistanceSum = newA * 2f; // Автоматически корректируем сумму расстояний

                    // 6. Обновляем текстовое поле суммы расстояний, чтобы пользователь видел изменения
                    txtDistanceSum.Text = el.DistanceSum.ToString();

                    // Применяем настройки контура (цвет и толщину)[cite: 6, 1]
                    if (el.Sides.Count > 0)
                    {
                        el.Sides[0].Color = pnlSideColor.BackColor;
                        el.Sides[0].Thickness = float.Parse(txtThick.Text);
                    }
                }
                else if (cbSides.SelectedIndex >= 0)
                {
                    var side = flatSides[cbSides.SelectedIndex];
                    side.Color = pnlSideColor.BackColor;
                    side.Thickness = float.Parse(txtThick.Text);

                    side.RelativeOffset = new PointF(float.Parse(txtSideRelX.Text), float.Parse(txtSideRelY.Text));

                    if (targetFigure is Polygon poly)
                    {
                        poly.Vertices[cbSides.SelectedIndex] = side.RelativeOffset;
                    }

                    Figure parent = FindParentFigure(targetFigure, side);
                    if (parent != null)
                    {
                        int idx = parent.Sides.IndexOf(side);
                        int nextIdx = (idx + 1) % parent.Sides.Count;
                        var nextSide = parent.Sides[nextIdx];
                        float newLen = float.Parse(txtSideLength.Text);
                        float dx = nextSide.RelativeOffset.X - side.RelativeOffset.X;
                        float dy = nextSide.RelativeOffset.Y - side.RelativeOffset.Y;
                        double oldLen = Math.Sqrt(dx * dx + dy * dy);

                        if (oldLen > 0.1)
                        {
                            nextSide.RelativeOffset = new PointF(
                                side.RelativeOffset.X + (float)(dx / oldLen * newLen),
                                side.RelativeOffset.Y + (float)(dy / oldLen * newLen)
                            );
                            if (parent is Polygon parentPoly)
                            {
                                parentPoly.Vertices[nextIdx] = nextSide.RelativeOffset;
                            }
                        }
                    }
                }

                UpdateBoundsLabel();
                mainForm.RefreshCanvas();
            }
            catch
            {
            }
        }
    }
}