using Lab1.Figures;
using Lab1.Shapes;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Lab1
{
    public partial class EditorForm : Form
    {
        private Figure targetFigure;
        private Form1 mainForm;
        private Panel canvas;

        // Поля UI
        private Label lblBounds;
        private TextBox txtCX, txtCY, txtRelX, txtRelY, txtScale, txtThick, txtSideRelX, txtSideRelY;
        private Panel pnlFillColor, pnlSideColor;
        private ComboBox cbSides;

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
            this.Text = "Editor";

            // ИСПРАВЛЕНО: Увеличиваем размер окна, чтобы всё поместилось (400 в ширину, 920 в высоту)
            this.Size = new Size(400, 1000);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.Manual;
            this.TopMost = true;

            // ИСПРАВЛЕНО: Рассчитываем позицию окна - справа по центру экрана
            System.Drawing.Rectangle screen = Screen.PrimaryScreen.WorkingArea;
            int formX = screen.Right - this.Width - 10; // 10 пикселей отступа от правого края
            int formY = screen.Top + (screen.Height - this.Height) / 2; // По центру вертикали
            this.Location = new Point(formX, formY);

            int y = 10;
            int elementWidth = 360; // ИСПРАВЛЕНО: Универсальная ширина элементов для новой ширины формы

            // Границы
            lblBounds = new Label { Location = new Point(10, y), Size = new Size(elementWidth, 90), Text = "Границы: ..." };
            this.Controls.Add(lblBounds); y += 95; // ИСПРАВЛЕНО: увеличен отступ

            // Координаты центра
            AddLabel("Координаты центра (X, Y):", ref y);
            txtCX = AddTextBox(ref y, elementWidth);
            txtCY = AddTextBox(ref y, elementWidth);

            // Сдвиг центра
            AddLabel("Сдвиг центра отн. (RelX, RelY):", ref y);
            txtRelX = AddTextBox(ref y, elementWidth);
            txtRelY = AddTextBox(ref y, elementWidth);

            // Масштаб
            AddLabel("Масштаб (1.0 = 100%):", ref y);
            txtScale = AddTextBox(ref y, elementWidth);

            // Цвет заливки
            AddLabel("Цвет фигуры:", ref y);
            pnlFillColor = new Panel { Location = new Point(10, y+5), Size = new Size(35, 30), BorderStyle = BorderStyle.FixedSingle };
            Button btnFill = new Button { Text = "Выбрать цвет заливки", Location = new Point(55, y), Size = new Size(elementWidth - 45, 40) };
            btnFill.Click += (s, e) => PickColor(pnlFillColor);
            this.Controls.Add(pnlFillColor); this.Controls.Add(btnFill); y += 45; // ИСПРАВЛЕНО: увеличен отступ

            // Выбор стороны
            AddLabel("Выбор стороны для редактирования:", ref y);
            cbSides = new ComboBox { Location = new Point(10, y), Size = new Size(elementWidth, 30), DropDownStyle = ComboBoxStyle.DropDownList };
            cbSides.SelectedIndexChanged += (s, e) => LoadSideData();
            this.Controls.Add(cbSides); y += 45; // ИСПРАВЛЕНО: увеличен отступ

            // Свойства стороны
            AddLabel("Цвет стороны:", ref y);
            pnlSideColor = new Panel { Location = new Point(10, y+5), Size = new Size(35, 30), BorderStyle = BorderStyle.FixedSingle };
            Button btnSide = new Button { Text = "Выбрать цвет линии", Location = new Point(55, y), Size = new Size(elementWidth - 45, 40) };
            btnSide.Click += (s, e) => PickColor(pnlSideColor);
            this.Controls.Add(pnlSideColor); this.Controls.Add(btnSide); y += 45; // ИСПРАВЛЕНО: увеличен отступ

            AddLabel("Толщина стороны:", ref y);
            txtThick = AddTextBox(ref y, elementWidth);

            AddLabel("Смещение стороны (RelX, RelY):", ref y);
            txtSideRelX = AddTextBox(ref y, elementWidth);
            txtSideRelY = AddTextBox(ref y, elementWidth);

            // Кнопки управления
            Button btnApply = new Button { Text = "Применить", Location = new Point(10, y), Size = new Size(elementWidth, 45), BackColor = Color.LightGreen };
            btnApply.Click += ApplyChanges;
            this.Controls.Add(btnApply); y += 55; // ИСПРАВЛЕНО: увеличен отступ

            Button btnDel = new Button { Text = "Удалить фигуру", Location = new Point(10, y), Size = new Size(elementWidth, 45), BackColor = Color.MistyRose };
            btnDel.Click += (s, e) =>
            {
                mainForm.figures.Remove(targetFigure);
                mainForm.selectedFigure = null;
                canvas.Invalidate();
                this.Close();
            };
            this.Controls.Add(btnDel);
        }

        // ИСПРАВЛЕНО: Шаг Y увеличен, чтобы текст не обрезался
        private void AddLabel(string text, ref int y)
        {
            this.Controls.Add(new Label { Text = text, Location = new Point(10, y), AutoSize = true });
            y += 30;
        }

        // ИСПРАВЛЕНО: Добавлен параметр ширины и увеличен шаг по Y
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

        private void LoadData()
        {
            RectangleF b = targetFigure.GetBounds();
            lblBounds.Text = $"Границы:\nMinX: {b.Left:F0}  MinY: {b.Top:F0}\nMaxX: {b.Right:F0}  MaxY: {b.Bottom:F0}";

            txtCX.Text = targetFigure.BaseLocation.X.ToString();
            txtCY.Text = targetFigure.BaseLocation.Y.ToString();
            txtRelX.Text = targetFigure.RelativePivot.X.ToString();
            txtRelY.Text = targetFigure.RelativePivot.Y.ToString();
            txtScale.Text = (targetFigure.Size / 100f).ToString();
            pnlFillColor.BackColor = targetFigure.FillColor;

            cbSides.Items.Clear();
            for (int i = 0; i < targetFigure.Sides.Count; i++) cbSides.Items.Add($"Сторона {i}");
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
            var side = targetFigure.Sides[cbSides.SelectedIndex];
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

                if (isBaseLocationChangedManually)
                {
                    targetFigure.BaseLocation = newBaseLocation;
                }
                else
                {
                    targetFigure.Center = oldVisualCenter;
                }

                targetFigure.Size = (int)(float.Parse(txtScale.Text) * 100);
                targetFigure.FillColor = pnlFillColor.BackColor;

                if (cbSides.SelectedIndex >= 0)
                {
                    var side = targetFigure.Sides[cbSides.SelectedIndex];
                    side.Color = pnlSideColor.BackColor;
                    side.Thickness = float.Parse(txtThick.Text);
                    side.RelativeOffset = new PointF(float.Parse(txtSideRelX.Text), float.Parse(txtSideRelY.Text));
                }

                LoadData();
                canvas.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка ввода данных: " + ex.Message);
            }
        }
    }
}