using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Lab1.Figures;
using Lab1.Shapes;

namespace Lab1
{
    public partial class Form1 : Form
    {
        public List<Figure> figures = new List<Figure>();
        public Figure selectedFigure = null;
        private bool isDragging = false;
        private Point lastMousePos;
        private bool isFullScreen = false;
        private FormBorderStyle lastStyle;
        private FormWindowState lastState;

        // НОВОЕ: Ссылка на текущее открытое окно редактора
        private EditorForm currentEditor = null;

        public Form1()
        {
            InitializeComponent();

            // Привязка событий тулбара
            btnAddShape.Click += (s, e) => CreateFigureFromUI();
            btnClearAll.Click += (s, e) => {
                figures.Clear();
                selectedFigure = null;
                canvasPanel.Invalidate();
            };
            btnFullScreen.Click += (s, e) => ToggleFullScreen();
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.F11) ToggleFullScreen(); };

            // События холста
            canvasPanel.Paint += CanvasPanel_Paint;
            canvasPanel.MouseDown += CanvasPanel_MouseDown;
            canvasPanel.MouseMove += CanvasPanel_MouseMove;
            canvasPanel.MouseUp += (s, e) => isDragging = false;
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

        private void CreateFigureFromUI()
        {
            Point center = new Point(canvasPanel.Width / 2, canvasPanel.Height / 2);
            Figure f = null;

            switch (cbShapeType.SelectedItem.ToString())
            {
                case "Прямоугольник": f = new Lab1.Figures.Rectangle(center); break;
                case "Треугольник": f = new Triangle(center); break;
                case "Круг": f = new Circle(center); break;
                case "Трапеция": f = new Trapezium(center); break;
                case "Пятиугольник": f = new Pentagon(center); break;
            }

            if (f != null)
            {
                figures.Add(f);
                selectedFigure = f;
                canvasPanel.Invalidate();
            }
        }

        private void CanvasPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            foreach (var fig in figures)
            {
                fig.Draw(e.Graphics);
            }

            // ОТОБРАЖЕНИЕ ВИРТУАЛЬНЫХ ГРАНИЦ
            if (selectedFigure != null)
            {
                // 1. Точка привязки
                using (Pen p = new Pen(Color.Red, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                {
                    e.Graphics.DrawEllipse(p, selectedFigure.BaseLocation.X - 5, selectedFigure.BaseLocation.Y - 5, 10, 10);
                }

                // 2. Рамка выделения
                RectangleF bounds = selectedFigure.GetBounds();
                using (Pen borderPen = new Pen(Color.Red, 1))
                {
                    e.Graphics.DrawRectangle(borderPen, bounds.X - 5, bounds.Y - 5, bounds.Width + 10, bounds.Height + 10);
                }
            }
        }

        private void CanvasPanel_MouseDown(object sender, MouseEventArgs e)
        {
            bool hit = false;
            for (int i = figures.Count - 1; i >= 0; i--)
            {
                if (figures[i].Contains(e.Location))
                {
                    selectedFigure = figures[i];
                    hit = true;

                    if (e.Button == MouseButtons.Left)
                    {
                        isDragging = true;
                        lastMousePos = e.Location;
                    }
                    else if (e.Button == MouseButtons.Right)
                    {
                        // ИЗМЕНЕНО: Закрываем старое окно, если оно есть, и сохраняем ссылку на новое
                        if (currentEditor != null && !currentEditor.IsDisposed) currentEditor.Close();

                        currentEditor = new EditorForm(selectedFigure, this, canvasPanel);
                        currentEditor.Show();
                    }
                    break;
                }
            }

            if (!hit) selectedFigure = null;
            canvasPanel.Invalidate();
        }

        private void CanvasPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && selectedFigure != null)
            {
                selectedFigure.Move(e.X - lastMousePos.X, e.Y - lastMousePos.Y);
                lastMousePos = e.Location;
                canvasPanel.Invalidate();

                // ИЗМЕНЕНО: Обновляем координаты в открытом окне редактора при перетаскивании
                if (currentEditor != null && !currentEditor.IsDisposed)
                {
                    currentEditor.UpdateCoordinates();
                }
            }
        }
    }
}