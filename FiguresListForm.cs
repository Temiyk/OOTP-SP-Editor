using System;
using System.Drawing;
using System.Windows.Forms;
using Lab1.Shapes;
using System.Linq;

namespace Lab1
{
    public class FiguresListForm : Form
    {
        private ListBox lbFigures;
        private Button btnGroup, btnUngroup, btnRename, btnDelete, btnEditGroup;
        private Form1 mainForm;

        public FiguresListForm(Form1 main)
        {
            mainForm = main;
            this.Text = "Список фигур";
            this.Size = new Size(300, 500);
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.TopMost = true;

            lbFigures = new ListBox { Dock = DockStyle.Fill, SelectionMode = SelectionMode.MultiExtended };

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 150 };
            btnGroup = new Button { Text = "Сгруппировать", Dock = DockStyle.Top, Height = 30 };
            btnUngroup = new Button { Text = "Разгруппировать", Dock = DockStyle.Top, Height = 30 };
            btnEditGroup = new Button { Text = "Редактировать группу", Dock = DockStyle.Top, Height = 30, BackColor = Color.LightYellow };
            btnRename = new Button { Text = "Переименовать", Dock = DockStyle.Top, Height = 30 };
            btnDelete = new Button { Text = "Удалить", Dock = DockStyle.Top, Height = 30, BackColor = Color.MistyRose };

            topPanel.Controls.AddRange(new Control[] { btnDelete, btnRename, btnEditGroup, btnUngroup, btnGroup });
            this.Controls.Add(lbFigures);
            this.Controls.Add(topPanel);

            btnGroup.Click += (s, e) => GroupSelected();
            btnUngroup.Click += (s, e) => UngroupSelected();
            btnEditGroup.Click += (s, e) => EditGroupSelected(); // Новая функция
            btnDelete.Click += (s, e) => DeleteSelected();
            btnRename.Click += (s, e) => RenameSelected();

            RefreshList();
        }

        public void RefreshList()
        {
            lbFigures.Items.Clear();
            foreach (var f in mainForm.figures) lbFigures.Items.Add(f.Name);
        }

        private void EditGroupSelected()
        {
            if (lbFigures.SelectedIndex < 0) return;
            var selectedFig = mainForm.figures[lbFigures.SelectedIndex];

            if (selectedFig is CompositeFigure group)
            {
                var editor = new GroupEditorForm(group, mainForm, this);
                editor.Show();
            }
            else
            {
                MessageBox.Show("Выбранная фигура не является группой. Пожалуйста, выберите сборку.");
            }
        }

        private void GroupSelected()
        {
            var selectedIndices = lbFigures.SelectedIndices.Cast<int>().OrderByDescending(i => i).ToList();
            if (selectedIndices.Count < 2) return;

            var toGroup = selectedIndices.Select(i => mainForm.figures[i]).ToList();
            foreach (var f in toGroup) mainForm.figures.Remove(f);

            var group = new CompositeFigure(toGroup, "Новая группа");
            mainForm.figures.Add(group);
            RefreshList();
            mainForm.RefreshCanvas();
        }

        private void UngroupSelected()
        {
            var selectedIndices = lbFigures.SelectedIndices.Cast<int>().OrderByDescending(i => i).ToList();
            bool changed = false;
            foreach (var i in selectedIndices)
            {
                if (mainForm.figures[i] is CompositeFigure group)
                {
                    mainForm.figures.RemoveAt(i);
                    mainForm.figures.AddRange(group.Children);
                    changed = true;
                }
            }
            if (changed) { RefreshList(); mainForm.RefreshCanvas(); }
        }

        private void DeleteSelected()
        {
            var selectedIndices = lbFigures.SelectedIndices.Cast<int>().OrderByDescending(i => i).ToList();
            foreach (var i in selectedIndices) mainForm.figures.RemoveAt(i);
            RefreshList();
            mainForm.RefreshCanvas();
        }

        private void RenameSelected()
        {
            if (lbFigures.SelectedIndex < 0) return;
            string newName = Microsoft.VisualBasic.Interaction.InputBox("Введите имя:", "Переименование", mainForm.figures[lbFigures.SelectedIndex].Name);
            if (!string.IsNullOrEmpty(newName))
            {
                mainForm.figures[lbFigures.SelectedIndex].Name = newName;
                RefreshList();
            }
        }
    }
}