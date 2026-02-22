using System.Drawing;
using System.Windows.Forms;

namespace Lab1
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private Panel toolbar;
        private DoubleBufferedPanel canvasPanel;

        // Элементы управления в верхней панели
        private ComboBox cbShapeType;
        private Button btnAddShape;
        private Button btnClearAll;
        private Button btnFullScreen;

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
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            // --- ВЕРХНЯЯ ПАНЕЛЬ ---
            toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(45, 45, 48),
                Padding = new Padding(10)
            };

            // 1. Выбор типа фигуры
            cbShapeType = new ComboBox
            {
                Location = new Point(15, 17),
                Size = new Size(160, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            cbShapeType.Items.AddRange(new object[] { "Прямоугольник", "Треугольник", "Круг", "Трапеция", "Пятиугольник" });
            cbShapeType.SelectedIndex = 0;

            // 2. Добавить фигуру
            btnAddShape = new Button
            {
                Text = "Добавить фигуру",
                Location = new Point(190, 15),
                Size = new Size(150, 32),
                BackColor = Color.FromArgb(63, 63, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAddShape.FlatAppearance.BorderSize = 0;

            // 3. Удалить всё
            btnClearAll = new Button
            {
                Text = "Удалить всё",
                Location = new Point(350, 15),
                Size = new Size(120, 32),
                BackColor = Color.FromArgb(63, 63, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnClearAll.FlatAppearance.BorderSize = 0;

            // 4. Полный экран (F11)
            // ВАЖНО: Пока не задаем свойство Anchor, сделаем это позже!
            btnFullScreen = new Button
            {
                Text = "Полный экран (F11)",
                Location = new Point(980, 15),
                Size = new Size(180, 32),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnFullScreen.FlatAppearance.BorderSize = 0;

            // Добавляем элементы на панель
            toolbar.Controls.Add(cbShapeType);
            toolbar.Controls.Add(btnAddShape);
            toolbar.Controls.Add(btnClearAll);
            toolbar.Controls.Add(btnFullScreen);

            // --- ХОЛСТ ---
            canvasPanel = new DoubleBufferedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.WhiteSmoke
            };

            // Сначала добавляем панели на форму, чтобы они приняли нужный размер
            this.Controls.Add(canvasPanel);
            this.Controls.Add(toolbar);

            // И только теперь, когда всё стоит на своих местах, "привязываем" кнопку к правому краю!
            btnFullScreen.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        }
    }

    // Класс для плавной отрисовки без мерцания
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