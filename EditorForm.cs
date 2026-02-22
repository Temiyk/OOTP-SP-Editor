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

        // Поля UI (согласно референсу)
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
            this.Text = "Editor";
            this.Size = new Size(320, 720);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(Cursor.Position.X, Cursor.Position.Y);
            this.TopMost = true;

            int y = 10;

            // Границы
            lblBounds = new Label { Location = new Point(10, y), Size = new Size(280, 60), Text = "Границы: ..." };
            this.Controls.Add(lblBounds); y += 65;

            // Координаты центра
            AddLabel("Координаты центра (X, Y):", ref y);
            txtCX = AddTextBox(ref y);
            txtCY = AddTextBox(ref y);

            // Сдвиг центра
            AddLabel("Сдвиг центра относительно (RelX, RelY):", ref y);
            txtRelX = AddTextBox(ref y);
            txtRelY = AddTextBox(ref y);

            // Масштаб
            AddLabel("Масштаб (1.0 = 100%):", ref y);
            txtScale = AddTextBox(ref y);

            // Цвет заливки
            AddLabel("Цвет фигуры:", ref y);
            pnlFillColor = new Panel { Location = new Point(10, y), Size = new Size(30, 25), BorderStyle = BorderStyle.FixedSingle };
            Button btnFill = new Button { Text = "Выбрать цвет заливки", Location = new Point(45, y), Size = new Size(240, 25) };
            btnFill.Click += (s, e) => PickColor(pnlFillColor);
            this.Controls.Add(pnlFillColor); this.Controls.Add(btnFill); y += 35;

            // Выбор стороны
            AddLabel("Выбор стороны для редактирования:", ref y);
            cbSides = new ComboBox { Location = new Point(10, y), Size = new Size(280, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cbSides.SelectedIndexChanged += (s, e) => LoadSideData();
            this.Controls.Add(cbSides); y += 30;

            // Свойства стороны
            AddLabel("Цвет стороны:", ref y);
            pnlSideColor = new Panel { Location = new Point(10, y), Size = new Size(30, 25), BorderStyle = BorderStyle.FixedSingle };
            Button btnSide = new Button { Text = "Выбрать цвет линии", Location = new Point(45, y), Size = new Size(240, 25) };
            btnSide.Click += (s, e) => PickColor(pnlSideColor);
            this.Controls.Add(pnlSideColor); this.Controls.Add(btnSide); y += 35;

            AddLabel("Толщина стороны:", ref y);
            txtThick = AddTextBox(ref y);

            AddLabel("Смещение стороны (RelX, RelY):", ref y);
            txtSideRelX = AddTextBox(ref y);
            txtSideRelY = AddTextBox(ref y);

            // Кнопки управления
            Button btnApply = new Button { Text = "Применить", Location = new Point(10, y), Size = new Size(280, 35), BackColor = Color.LightGreen };
            btnApply.Click += ApplyChanges;
            this.Controls.Add(btnApply); y += 40;

            Button btnDel = new Button { Text = "Удалить фигуру", Location = new Point(10, y), Size = new Size(280, 30), BackColor = Color.MistyRose };
            btnDel.Click += (s, e) =>
            {
                // 1. Удаляем фигуру из общего списка
                mainForm.figures.Remove(targetFigure);

                // 2. ВАЖНО: Снимаем выделение в главной форме, 
                // чтобы пропали границы и красная точка
                mainForm.selectedFigure = null;

                // 3. Перерисовываем холст
                canvas.Invalidate();

                // 4. Закрываем редактор
                this.Close();
            };
            this.Controls.Add(btnDel);
        }

        private void AddLabel(string text, ref int y)
        {
            this.Controls.Add(new Label { Text = text, Location = new Point(10, y), AutoSize = true }); y += 20;
        }

        private TextBox AddTextBox(ref int y)
        {
            TextBox tb = new TextBox { Location = new Point(10, y), Size = new Size(280, 25) };
            this.Controls.Add(tb); y += 28; return tb;
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
                // 1. Считываем новые значения из UI
                Point newBaseLocation = new Point(int.Parse(txtCX.Text), int.Parse(txtCY.Text));
                PointF newRelativePivot = new PointF(float.Parse(txtRelX.Text), float.Parse(txtRelY.Text));

                // 2. Сохраняем текущий визуальный центр до правок
                Point oldVisualCenter = targetFigure.Center;

                // 3. Проверяем: изменил ли пользователь абсолютные координаты (X, Y) вручную?
                bool isBaseLocationChangedManually = (newBaseLocation != targetFigure.BaseLocation);

                // 4. Обновляем относительный сдвиг
                targetFigure.RelativePivot = newRelativePivot;

                if (isBaseLocationChangedManually)
                {
                    // Если пользователь сам ввел новые X и Y, перемещаем точку привязки туда
                    targetFigure.BaseLocation = newBaseLocation;
                }
                else
                {
                    // Если X и Y не менялись в полях ввода, значит пользователь менял только сдвиг.
                    // Фигура должна остаться на месте: восстанавливаем визуальный центр.
                    // Сеттер Center в Figure.cs сам пересчитает BaseLocation под новый Pivot.
                    targetFigure.Center = oldVisualCenter;
                }

                // 5. Обновляем остальные параметры (размер, цвет и т.д.)
                targetFigure.Size = (int)(float.Parse(txtScale.Text) * 100);
                targetFigure.FillColor = pnlFillColor.BackColor;

                if (cbSides.SelectedIndex >= 0)
                {
                    var side = targetFigure.Sides[cbSides.SelectedIndex];
                    side.Color = pnlSideColor.BackColor;
                    side.Thickness = float.Parse(txtThick.Text);
                    side.RelativeOffset = new PointF(float.Parse(txtSideRelX.Text), float.Parse(txtSideRelY.Text));
                }

                // 6. Обновляем данные в полях (так как BaseLocation мог пересчитаться)
                LoadData();

                // 7. Перерисовываем холст
                canvas.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка ввода данных: " + ex.Message);
            }
        }
    }
}