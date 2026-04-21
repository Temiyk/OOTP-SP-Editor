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

        private Label lblBounds;
        private TextBox txtFigureName, txtId;
        private TextBox txtCX, txtCY, txtRelX, txtRelY, txtScale, txtThick, txtSideRelX, txtSideRelY;
        private TextBox txtSideLength;
        private Panel pnlFillColor, pnlSideColor;
        private ComboBox cbSides;

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
            this.Size = new Size(400, 1150);
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
            btnFill.Click += (s, e) => PickColor(pnlFillColor);
            this.Controls.Add(pnlFillColor); this.Controls.Add(btnFill); y += 45;

            AddLabel("Выбор стороны для редактирования:", ref y);
            cbSides = new ComboBox { Location = new Point(10, y), Size = new Size(elementWidth, 30), DropDownStyle = ComboBoxStyle.DropDownList };
            cbSides.SelectedIndexChanged += (s, e) => LoadSideData();
            this.Controls.Add(cbSides); y += 45;

            AddLabel("Цвет стороны:", ref y);
            pnlSideColor = new Panel { Location = new Point(10, y + 5), Size = new Size(35, 30), BorderStyle = BorderStyle.FixedSingle };
            Button btnSide = new Button { Text = "Выбрать цвет линии", Location = new Point(55, y), Size = new Size(elementWidth - 45, 40) };
            btnSide.Click += (s, e) => PickColor(pnlSideColor);
            this.Controls.Add(pnlSideColor); this.Controls.Add(btnSide); y += 45;

            AddLabel("Толщина стороны:", ref y);
            txtThick = AddTextBox(ref y, elementWidth);

            AddLabel("Смещение стороны (RelX, RelY):", ref y);
            txtSideRelX = AddTextBox(ref y, elementWidth);
            txtSideRelY = AddTextBox(ref y, elementWidth);

            AddLabel("Длина стороны (px):", ref y);
            txtSideLength = AddTextBox(ref y, elementWidth);

            Button btnApply = new Button { Text = "Применить", Location = new Point(10, y), Size = new Size(elementWidth, 45), BackColor = Color.LightGreen };
            btnApply.Click += ApplyChanges;
            this.Controls.Add(btnApply); y += 55;

            Button btnDel = new Button { Text = "Удалить фигуру", Location = new Point(10, y), Size = new Size(elementWidth, 45), BackColor = Color.MistyRose };
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

        private void AddLabel(string text, ref int y)
        {
            this.Controls.Add(new Label { Text = text, Location = new Point(10, y), AutoSize = true });
            y += 30;
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
            txtId.Text = targetFigure.Id.ToString().ToUpper();

            txtFigureName.Text = targetFigure.Name;
            RectangleF b = targetFigure.GetBounds();
            lblBounds.Text = $"Границы:\nMinX: {b.Left:F0}  MinY: {b.Top:F0}\nMaxX: {b.Right:F0}  MaxY: {b.Bottom:F0}";

            txtCX.Text = targetFigure.BaseLocation.X.ToString();
            txtCY.Text = targetFigure.BaseLocation.Y.ToString();
            txtRelX.Text = targetFigure.RelativePivot.X.ToString();
            txtRelY.Text = targetFigure.RelativePivot.Y.ToString();
            txtScale.Text = (targetFigure.Size / 100f).ToString();
            pnlFillColor.BackColor = targetFigure.FillColor;

            cbSides.Items.Clear();
            flatSides.Clear();
            PopulateSides(targetFigure, "");

            if (cbSides.Items.Count > 0) cbSides.SelectedIndex = 0;
        }

        public void UpdateCoordinates()
        {
            RectangleF b = targetFigure.GetBounds();
            lblBounds.Text = $"Границы:\nMinX: {b.Left:F0}  MinY: {b.Top:F0}\nMaxX: {b.Right:F0}  MaxY: {b.Bottom:F0}";

            if (!txtCX.Focused) txtCX.Text = targetFigure.BaseLocation.X.ToString();
            if (!txtCY.Focused) txtCY.Text = targetFigure.BaseLocation.Y.ToString();
        }

        private void LoadSideData()
        {
            if (cbSides.SelectedIndex < 0) return;

            var side = flatSides[cbSides.SelectedIndex];

            // Устанавливаем сторону для мерцания/выделения и перерисовываем холст
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
        private void txtSideLength_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (cbSides.SelectedItem is SideStyle side)
                {
                    Figure parent = FindParentFigure(targetFigure, side);
                    if (parent != null)
                    {
                        int idx = parent.Sides.IndexOf(side);
                        // Находим следующую точку (с учетом замыкания: если последняя, то берем первую)
                        int nextIdx = (idx + 1) % parent.Sides.Count;
                        var nextSide = parent.Sides[nextIdx];

                        float newLen = float.Parse(txtSideLength.Text);

                        float dx = nextSide.RelativeOffset.X - side.RelativeOffset.X;
                        float dy = nextSide.RelativeOffset.Y - side.RelativeOffset.Y;
                        double oldLen = Math.Sqrt(dx * dx + dy * dy);

                        if (oldLen > 0.1)
                        {
                            // Двигаем следующую точку, сохраняя направление, тем самым "стыкуя" стороны
                            nextSide.RelativeOffset = new PointF(
                                side.RelativeOffset.X + (float)(dx / oldLen * newLen),
                                side.RelativeOffset.Y + (float)(dy / oldLen * newLen)
                            );
                        }
                    }
                }
                mainForm.RefreshCanvas();
            }
            catch { /* Игнорируем ошибки парсинга при вводе */ }
        }
        private void ApplyChanges(object sender, EventArgs e)
        {
            try
            {
                Point newBaseLocation = new Point(int.Parse(txtCX.Text), int.Parse(txtCY.Text));
                PointF newRelativePivot = new PointF(float.Parse(txtRelX.Text), float.Parse(txtRelY.Text));
                Point oldVisualCenter = targetFigure.Center;
                bool isBaseLocationChangedManually = (newBaseLocation != targetFigure.BaseLocation);

                targetFigure.RelativePivot = newRelativePivot;

                if (isBaseLocationChangedManually) targetFigure.BaseLocation = newBaseLocation;
                else targetFigure.Center = oldVisualCenter;

                targetFigure.Size = (int)(float.Parse(txtScale.Text) * 100);
                targetFigure.FillColor = pnlFillColor.BackColor;

                if (cbSides.SelectedIndex >= 0)
                {
                    var side = flatSides[cbSides.SelectedIndex];
                    side.Color = pnlSideColor.BackColor;
                    side.Thickness = float.Parse(txtThick.Text);
                    side.RelativeOffset = new PointF(float.Parse(txtSideRelX.Text), float.Parse(txtSideRelY.Text));
                }

                if (cbSides.SelectedIndex >= 0)
                {
                    var side = flatSides[cbSides.SelectedIndex];
                    side.Color = pnlSideColor.BackColor;
                    side.Thickness = float.Parse(txtThick.Text);
                    side.RelativeOffset = new PointF(float.Parse(txtSideRelX.Text), float.Parse(txtSideRelY.Text));

                    // --- НОВОЕ: Изменение длины ---
                    Figure parent = FindParentFigure(targetFigure, side);
                    if (parent != null)
                    {
                        int idx = parent.Sides.IndexOf(side);
                        int nextIdx = (idx + 1) % parent.Sides.Count;
                        var nextSide = parent.Sides[nextIdx];

                        float newLen = float.Parse(txtSideLength.Text);

                        // Вектор текущего направления
                        float dx = nextSide.RelativeOffset.X - side.RelativeOffset.X;
                        float dy = nextSide.RelativeOffset.Y - side.RelativeOffset.Y;
                        double oldLen = Math.Sqrt(dx * dx + dy * dy);

                        if (oldLen > 0.1) // Чтобы не делить на ноль
                        {
                            // Новые координаты следующей точки = старт + (направление * новая_длина)
                            nextSide.RelativeOffset = new PointF(
                                side.RelativeOffset.X + (float)(dx / oldLen * newLen),
                                side.RelativeOffset.Y + (float)(dy / oldLen * newLen)
                            );
                        }
                    }
                }

                LoadData();
                mainForm.RefreshCanvas();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка ввода данных: " + ex.Message);
            }
        }
    }
}