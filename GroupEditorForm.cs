using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Lab1.Shapes;

namespace Lab1
{
    public partial class GroupEditorForm : Form
    {
        private CompositeFigure targetGroup;
        private Form1 mainForm;
        private FiguresListForm listForm;

        private ListBox lbInGroup, lbOnCanvas;
        private Button btnAdd, btnRemove;

        private List<Figure> inGroupRefs = new List<Figure>();
        private List<Figure> onCanvasRefs = new List<Figure>();

        public GroupEditorForm(CompositeFigure group, Form1 main, FiguresListForm listForm)
        {
            this.targetGroup = group;
            this.mainForm = main;
            this.listForm = listForm;

            this.Text = "Редактор группы: " + group.Name;
            this.Size = new Size(540, 450);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.TopMost = true;

            Label lblGroup = new Label { Text = "Внутри группы:", Location = new Point(10, 10), AutoSize = true };
            lbInGroup = new ListBox { Location = new Point(10, 30), Size = new Size(200, 350), SelectionMode = SelectionMode.MultiExtended };

            Label lblCanvas = new Label { Text = "Снаружи (на холсте):", Location = new Point(310, 10), AutoSize = true };
            lbOnCanvas = new ListBox { Location = new Point(310, 30), Size = new Size(200, 350), SelectionMode = SelectionMode.MultiExtended };

            btnAdd = new Button { Text = "< Добавить", Location = new Point(220, 150), Size = new Size(80, 40), BackColor = Color.LightGreen };
            btnRemove = new Button { Text = "Удалить >", Location = new Point(220, 200), Size = new Size(80, 40), BackColor = Color.MistyRose };

            btnAdd.Click += BtnAdd_Click;
            btnRemove.Click += BtnRemove_Click;

            this.Controls.AddRange(new Control[] { lblGroup, lbInGroup, lblCanvas, lbOnCanvas, btnAdd, btnRemove });

            RefreshData();
        }

        private void RefreshData()
        {
            lbInGroup.Items.Clear();
            inGroupRefs.Clear();
            foreach (var f in targetGroup.Children)
            {
                lbInGroup.Items.Add(f.Name);
                inGroupRefs.Add(f);
            }

            lbOnCanvas.Items.Clear();
            onCanvasRefs.Clear();
            foreach (var f in mainForm.figures)
            {
                // Не показываем саму редактируеную группу на холсте
                if (f != targetGroup)
                {
                    lbOnCanvas.Items.Add(f.Name);
                    onCanvasRefs.Add(f);
                }
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var indices = lbOnCanvas.SelectedIndices.Cast<int>().OrderByDescending(i => i).ToList();
            if (indices.Count == 0) return;

            foreach (var i in indices)
            {
                Figure figToAdd = onCanvasRefs[i];
                mainForm.figures.Remove(figToAdd); // Убираем с общего холста
                targetGroup.Children.Add(figToAdd); // Добавляем в группу
            }

            RecalculateGroupCenter();
            UpdateAllViews();
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            var indices = lbInGroup.SelectedIndices.Cast<int>().OrderByDescending(i => i).ToList();
            if (indices.Count == 0) return;

            foreach (var i in indices)
            {
                Figure figToRemove = inGroupRefs[i];
                targetGroup.Children.Remove(figToRemove); // Убираем из группы
                mainForm.figures.Add(figToRemove); // Возвращаем на общий холст
            }

            RecalculateGroupCenter();
            UpdateAllViews();
        }

        private void RecalculateGroupCenter()
        {
            if (targetGroup.Children.Count > 0)
            {
                int avgX = (int)targetGroup.Children.Average(c => c.BaseLocation.X);
                int avgY = (int)targetGroup.Children.Average(c => c.BaseLocation.Y);
                targetGroup.BaseLocation = new Point(avgX, avgY);
            }
        }

        private void UpdateAllViews()
        {
            RefreshData();
            mainForm.RefreshCanvas();
            listForm.RefreshList();
        }
    }
}