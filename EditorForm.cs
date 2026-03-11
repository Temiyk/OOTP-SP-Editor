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
        private TextBox txtFigureName;
        private TextBox txtCX, txtCY, txtRelX, txtRelY, txtScale, txtThick, txtSideRelX, txtSideRelY;
        private Panel pnlFillColor, pnlSideColor;
        private ComboBox cbSides;

        // ИСПРАВЛЕНИЕ 3: Плоский список для хранения сторон даже из вложенных групп
        private List<SideStyle> flatSides = new List<SideStyle>();

        public EditorForm(Figure figure, Form1 main, Panel canvasPanel)
        {
            this.targetFigure = figure;
            this.mainForm = main;
            this.canvas = canvasPanel;

            InitializeUI();
            LoadData();
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

            Button btnApply = new Button { Text = "Применить", Location = new Point(10, y), Size = new Size(elementWidth, 45), BackColor = Color.LightGreen };
            btnApply.Click += ApplyChanges;
            this.Controls.Add(btnApply); y += 55;

            Button btnDel = new Button { Text = "Удалить фигуру", Location = new Point(10, y), Size = new Size(elementWidth, 45), BackColor = Color.MistyRose };
            btnDel.Click += (s, e) =>
            {
                mainForm.figures.Remove(targetFigure);
                mainForm.selectedFigures.Remove(targetFigure); // Исправлено: удаляем из списка выделенных
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

        // ИСПРАВЛЕНИЕ 3: Рекурсивный метод для сбора всех сторон
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
            txtFigureName.Text = targetFigure.Name;
            RectangleF b = targetFigure.GetBounds();
            lblBounds.Text = $"Границы:\nMinX: {b.Left:F0}  MinY: {b.Top:F0}\nMaxX: {b.Right:F0}  MaxY: {b.Bottom:F0}";

            txtCX.Text = targetFigure.BaseLocation.X.ToString();
            txtCY.Text = targetFigure.BaseLocation.Y.ToString();
            txtRelX.Text = targetFigure.RelativePivot.X.ToString();
            txtRelY.Text = targetFigure.RelativePivot.Y.ToString();
            txtScale.Text = (targetFigure.Size / 100f).ToString();
            pnlFillColor.BackColor = targetFigure.FillColor;

            // ИСПРАВЛЕНИЕ 3: Заполняем список с помощью рекурсивного метода
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

            // ИСПРАВЛЕНИЕ 3: Берем сторону из нашего плоского списка
            var side = flatSides[cbSides.SelectedIndex];
            pnlSideColor.BackColor = side.Color;
            txtThick.Text = side.Thickness.ToString();
            txtSideRelX.Text = side.RelativeOffset.X.ToString();
            txtSideRelY.Text = side.RelativeOffset.Y.ToString();
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
                    // ИСПРАВЛЕНИЕ 3: Применяем изменения к правильной стороне из плоского списка
                    var side = flatSides[cbSides.SelectedIndex];
                    side.Color = pnlSideColor.BackColor;
                    side.Thickness = float.Parse(txtThick.Text);
                    side.RelativeOffset = new PointF(float.Parse(txtSideRelX.Text), float.Parse(txtSideRelY.Text));
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