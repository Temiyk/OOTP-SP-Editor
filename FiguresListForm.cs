using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Lab1.Shapes;

namespace Lab1
{
    public class FiguresListForm : Form
    {
        // ИСПРАВЛЕНИЕ 1: Меняем ListBox на TreeView
        private TreeView tvFigures;
        private Button btnGroup, btnUngroup, btnRename, btnDelete, btnEditGroup;
        private Form1 mainForm;

        public FiguresListForm(Form1 main)
        {
            mainForm = main;
            this.Text = "Список фигур";
            this.Size = new Size(350, 500); // Немного расширим окно для удобства
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.TopMost = true;

            // Настраиваем TreeView
            tvFigures = new TreeView
            {
                Dock = DockStyle.Fill,
                CheckBoxes = true, // Включаем галочки для множественного выбора
                HideSelection = false // Оставляем подсветку синим даже при клике на другие кнопки
            };

            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 150 };
            btnGroup = new Button { Text = "Сгруппировать (по галочкам)", Dock = DockStyle.Top, Height = 30 };
            btnUngroup = new Button { Text = "Разгруппировать (по галочкам)", Dock = DockStyle.Top, Height = 30 };
            btnEditGroup = new Button { Text = "Редактировать выделенную группу", Dock = DockStyle.Top, Height = 30, BackColor = Color.LightYellow };
            btnRename = new Button { Text = "Переименовать выделенное", Dock = DockStyle.Top, Height = 30 };
            btnDelete = new Button { Text = "Удалить (по галочкам)", Dock = DockStyle.Top, Height = 30, BackColor = Color.MistyRose };

            topPanel.Controls.AddRange(new Control[] { btnDelete, btnRename, btnEditGroup, btnUngroup, btnGroup });
            this.Controls.Add(tvFigures);
            this.Controls.Add(topPanel);

            btnGroup.Click += (s, e) => GroupSelected();
            btnUngroup.Click += (s, e) => UngroupSelected();
            btnEditGroup.Click += (s, e) => EditGroupSelected();
            btnDelete.Click += (s, e) => DeleteSelected();
            btnRename.Click += (s, e) => RenameSelected();

            RefreshList();
        }

        public void RefreshList()
        {
            tvFigures.Nodes.Clear();
            foreach (var fig in mainForm.figures)
            {
                // ИСПРАВЛЕНИЕ 2: Используем специальный метод для построения дерева
                tvFigures.Nodes.Add(CreateNode(fig));
            }
            // Разворачиваем все списки по умолчанию, чтобы сразу видеть состав групп
            tvFigures.ExpandAll();
        }

        // Вспомогательный метод для создания узлов дерева. 
        // Он вызывает сам себя (рекурсия), если находит группу внутри группы!
        private TreeNode CreateNode(Figure fig)
        {
            TreeNode node = new TreeNode($"[{fig.Id}] {fig.Name}");
            node.Tag = fig; // Прячем саму фигуру в узел (Tag), чтобы легко к ней обращаться

            if (fig is CompositeFigure group)
            {
                foreach (var child in group.Children)
                {
                    node.Nodes.Add(CreateNode(child)); // Добавляем дочерние фигуры
                }
            }
            return node;
        }

        // Собираем все фигуры, рядом с которыми поставлена галочка
        private List<Figure> GetCheckedFigures()
        {
            List<Figure> checkedFigs = new List<Figure>();
            CollectCheckedNodes(tvFigures.Nodes, checkedFigs);
            return checkedFigs;
        }

        private void CollectCheckedNodes(TreeNodeCollection nodes, List<Figure> checkedFigs)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Checked && node.Tag is Figure fig)
                {
                    checkedFigs.Add(fig);
                }
                // Проверяем и вложенные элементы тоже
                CollectCheckedNodes(node.Nodes, checkedFigs);
            }
        }

        private void GroupSelected()
        {
            var toGroup = GetCheckedFigures();

            var validToGroup = toGroup.Where(f => mainForm.figures.Contains(f)).ToList();

            if (validToGroup.Count < 2)
            {
                MessageBox.Show("Пожалуйста, отметьте галочками хотя бы две фигуры верхнего уровня для группировки.");
                return;
            }

            foreach (var f in validToGroup)
            {
                mainForm.figures.Remove(f);
                if (mainForm.selectedFigures.Contains(f))
                {
                    mainForm.selectedFigures.Remove(f);
                }
            }

            var group = new CompositeFigure(validToGroup, "Новая группа");
            mainForm.figures.Add(group);

            mainForm.selectedFigures.Clear();
            mainForm.selectedFigures.Add(group);

            // Моментально обновляем интерфейс и холст
            RefreshList();
            mainForm.RefreshCanvas();
        }

        private void UngroupSelected()
        {
            var checkedFigs = GetCheckedFigures();
            bool changed = false;

            foreach (var fig in checkedFigs)
            {
                // Разгруппировываем, только если это группа и она лежит на главном холсте
                if (fig is CompositeFigure group && mainForm.figures.Contains(group))
                {
                    mainForm.figures.Remove(group);
                    mainForm.figures.AddRange(group.Children);

                    // ИСПРАВЛЕНИЕ: Если эта группа была выделена на холсте, сбрасываем выделение!
                    if (mainForm.selectedFigures.Contains(group))
                    {
                        mainForm.selectedFigures.Remove(group);
                    }

                    changed = true;
                }
            }

            // Моментально обновляем интерфейс и холст, если были изменения
            if (changed)
            {
                RefreshList();
                mainForm.RefreshCanvas();
            }
        }

        private void DeleteSelected()
        {
            var checkedFigs = GetCheckedFigures();
            bool changed = false;

            foreach (var fig in checkedFigs)
            {
                if (mainForm.figures.Contains(fig))
                {
                    mainForm.figures.Remove(fig);
                    changed = true;
                }
            }

            if (changed) { RefreshList(); mainForm.RefreshCanvas(); }
            else if (checkedFigs.Count > 0)
            {
                MessageBox.Show("Вы можете удалять из списка только фигуры верхнего уровня. Чтобы удалить фигуру из группы, воспользуйтесь редактором группы.");
            }
        }

        private void RenameSelected()
        {
            // Здесь мы смотрим не на галочки, а на то, куда кликнул пользователь (SelectedNode)
            if (tvFigures.SelectedNode == null || !(tvFigures.SelectedNode.Tag is Figure fig))
            {
                MessageBox.Show("Сначала кликните по названию фигуры (чтобы оно выделилось синим цветом).");
                return;
            }

            string newName = Microsoft.VisualBasic.Interaction.InputBox("Введите имя:", "Переименование", fig.Name);
            if (!string.IsNullOrEmpty(newName))
            {
                fig.Name = newName;
                RefreshList();
            }
        }

        private void EditGroupSelected()
        {
            if (tvFigures.SelectedNode == null || !(tvFigures.SelectedNode.Tag is Figure selectedFig))
            {
                MessageBox.Show("Сначала кликните по названию группы (чтобы оно выделилось синим цветом).");
                return;
            }

            if (selectedFig is CompositeFigure group)
            {
                var editor = new GroupEditorForm(group, mainForm, this);
                editor.Show();
            }
            else
            {
                MessageBox.Show("Выбранная фигура не является группой.");
            }
        }
    }
}