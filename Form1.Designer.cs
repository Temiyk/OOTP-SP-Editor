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

            // ИЗМЕНЕНО: Основной шрифт увеличен до 12
            this.Font = new Font("Segoe UI", 12F, FontStyle.Regular);

            // --- ВЕРХНЯЯ ПАНЕЛЬ ---
            toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70, // ИЗМЕНЕНО: Панель стала чуть выше
                BackColor = Color.FromArgb(45, 45, 48),
                Padding = new Padding(10)
            };

            // 1. Выбор типа фигуры (ИЗМЕНЕНО: адаптирован размер)
            cbShapeType = new ComboBox
            {
                Location = new Point(15, 18),
                Size = new Size(180, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 12F)
            };
            cbShapeType.Items.AddRange(new object[] { "Прямоугольник", "Треугольник", "Круг", "Трапеция", "Пятиугольник" });
            cbShapeType.SelectedIndex = 0;

            // 2. Добавить фигуру (ИЗМЕНЕНО: адаптирован размер и позиция)
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

            // 3. Удалить всё (ИЗМЕНЕНО: адаптирован размер и позиция)
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

            // 4. Полный экран (F11) (ИЗМЕНЕНО: адаптирован размер и позиция)
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