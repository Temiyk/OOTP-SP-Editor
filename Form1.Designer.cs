using System.Drawing;
using System.Windows.Forms;

namespace Lab1
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private Panel toolbar;
        private DoubleBufferedPanel canvasPanel;

        private ComboBox cbShapeType;
        private Button btnAddShape;
        private Button btnClearAll;
        private Button btnFullScreen;

        private Button btnToggleDraw;
        private Button btnOpenList;

        private Button btnSave;
        private Button btnLoad;

        //Button btnSaveSelected = new Button
        //{
        //    Text = "Сохранить выделенное",
        //    Location = new Point(btnSave.Left, btnSave.Bottom + 5),
        //    Size = btnSave.Size,
        //    BackColor = Color.LightBlue
        //};
        //Button btnLoadAdd = new Button
        //{
        //    Text = "Добавить из файла",
        //    Location = new Point(btnLoad.Left, btnLoad.Bottom + 5),
        //    Size = btnLoad.Size,
        //    BackColor = Color.LightCoral
        //};

        private Button btnSaveSelected;
        private Button btnLoadAdd;

        private NumericUpDown nudAngle;
        private NumericUpDown nudLength;
        private Button btnAddPointByParams;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.Text = "Shape Vector Editor";
            this.Size = new Size(1200, 800);
            this.KeyPreview = true;
            this.Font = new Font("Segoe UI", 12F, FontStyle.Regular);

            toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(45, 45, 48),
                Padding = new Padding(10)
            };

            nudAngle = new NumericUpDown
            {
                Location = new Point(980, 18),
                Size = new Size(60, 30),
                Minimum = 0,
                Maximum = 360,
                DecimalPlaces = 1,
                Increment = 1,
                Visible = false
            };
            nudLength = new NumericUpDown
            {
                Location = new Point(1050, 18),
                Size = new Size(70, 30),
                Minimum = 1,
                Maximum = 2000,
                DecimalPlaces = 0,
                Increment = 10,
                Visible = false
            };
            btnAddPointByParams = new Button
            {
                Text = "Добавить точку",
                Location = new Point(1130, 16),
                Size = new Size(120, 36),
                BackColor = Color.LightBlue,
                FlatStyle = FlatStyle.Flat,
                Visible = false
            };
            toolbar.Controls.AddRange(new Control[] { nudAngle, nudLength, btnAddPointByParams });

            nudAngle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nudLength.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAddPointByParams.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            cbShapeType = new ComboBox
            {
                Location = new Point(15, 18),
                Size = new Size(180, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 12F)
            };
            cbShapeType.Items.AddRange(new object[] { "Прямоугольник", "Треугольник", "Круг", "Эллипс", "Трапеция", "Пятиугольник" });
            cbShapeType.SelectedIndex = 0;

            btnAddShape = new Button
            {
                Text = "Добавить фигуру",
                Location = new Point(210, 16),
                Size = new Size(170, 36),
                BackColor = Color.FromArgb(63, 63, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAddShape.FlatAppearance.BorderSize = 0;

            btnClearAll = new Button
            {
                Text = "Удалить всё",
                Location = new Point(395, 16),
                Size = new Size(130, 36),
                BackColor = Color.FromArgb(63, 63, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnClearAll.FlatAppearance.BorderSize = 0;

            btnToggleDraw = new Button
            {
                Text = "Рисование (Выкл)",
                Location = new Point(540, 16),
                Size = new Size(180, 36),
                BackColor = Color.FromArgb(63, 63, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnToggleDraw.FlatAppearance.BorderSize = 0;

            btnOpenList = new Button
            {
                Text = "Список фигур",
                Location = new Point(735, 16),
                Size = new Size(150, 36),
                BackColor = Color.FromArgb(63, 63, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnOpenList.FlatAppearance.BorderSize = 0;

            btnFullScreen = new Button
            {
                Text = "Полный экран (F11)",
                Location = new Point(980, 16),
                Size = new Size(190, 36),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnFullScreen.FlatAppearance.BorderSize = 0;

            btnSave = new Button();
            btnSave.Text = "Сохранить";
            btnSave.Size = new Size(190, 36);
            btnSave.Location = new Point(930, 16);
            btnSave.BackColor = Color.FromArgb(0, 122, 204);
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.ForeColor = Color.White;
            btnSave.FlatAppearance.BorderSize = 0;

            btnLoad = new Button();
            btnLoad.Text = "Загрузить";
            btnLoad.Size = new Size(190, 36);
            btnLoad.Location = new Point(1125, 16); 
            btnLoad.BackColor = Color.FromArgb(0, 122, 204);
            btnLoad.FlatStyle = FlatStyle.Flat;
            btnLoad.ForeColor = Color.White;
            btnLoad.FlatAppearance.BorderSize = 0;

            btnSaveSelected = new Button
            {
                Text = "Сохранить выделенное",
                Location = new Point(1320, 16),
                Size = new Size(190, 36),
                BackColor = Color.FromArgb(0, 122, 204)
            };
            btnLoadAdd = new Button
            {
                Text = "Добавить из файла",
                Location = new Point(1515, 16),
                Size = new Size(190, 36),
                BackColor = Color.FromArgb(0, 122, 204)
            };

            toolbar.Controls.Add(cbShapeType);
            toolbar.Controls.Add(btnAddShape);
            toolbar.Controls.Add(btnClearAll);
            toolbar.Controls.Add(btnToggleDraw);
            toolbar.Controls.Add(btnOpenList);
            toolbar.Controls.Add(btnFullScreen);
            toolbar.Controls.Add(btnSave);
            toolbar.Controls.Add(btnLoad);
            toolbar.Controls.Add(btnSaveSelected);
            toolbar.Controls.Add(btnLoadAdd);

            canvasPanel = new DoubleBufferedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.WhiteSmoke
            };

            this.Controls.Add(canvasPanel);
            this.Controls.Add(toolbar);

            btnFullScreen.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        }
    }

    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }
    }
}