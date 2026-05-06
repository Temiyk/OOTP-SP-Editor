using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lab1.Shapes;
using System.Windows.Forms; // Необходимо для вывода окон с ошибками MessageBox

namespace Lab1
{
    public static class FileManager
    {
        // Этап 1: Обновление настроек библиотеки
        private static JsonSerializerSettings GetSettings()
        {
            return new JsonSerializerSettings
            {
                // Auto записывает $type только для наследников (Polygon, Ellipse и т.д.), 
                // оставляя сам список фигур обычным массивом. Это устраняет ошибку несовпадения типов.
                TypeNameHandling = TypeNameHandling.Auto,

                Formatting = Formatting.Indented, // Делаем текст красивым и читаемым

                // ВАЖНО: Replace гарантирует, что списки (например, Sides или Children) 
                // будут полностью заменяться данными из файла, а не склеиваться с пустыми списками из конструктора.
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };
        }

        public static void SaveToFile(FigureArray figures, string filePath)
        {
            try
            {
                // Для удобства сериализации перекладываем фигуры во временный стандартный список
                List<Figure> tempToSave = new List<Figure>();
                foreach (var fig in figures)
                {
                    tempToSave.Add(fig);
                }

                // Превращаем список в текст и записываем в файл
                string jsonText = JsonConvert.SerializeObject(tempToSave, GetSettings());
                File.WriteAllText(filePath, jsonText);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void SaveFigureToFile(Figure figure, string filePath)
        {
            try
            {
                List<Figure> temp = new List<Figure> { figure };
                string jsonText = JsonConvert.SerializeObject(temp, GetSettings());
                File.WriteAllText(filePath, jsonText);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении фигуры: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static List<Figure> LoadFiguresFromFile(string filePath)
        {
            if (!File.Exists(filePath)) return new List<Figure>();

            try
            {
                string jsonText = File.ReadAllText(filePath);
                var loaded = JsonConvert.DeserializeObject<List<Figure>>(jsonText, GetSettings());
                return loaded ?? new List<Figure>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при чтении файла: {ex.Message}\nВозможно, файл поврежден.", "Ошибка чтения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new List<Figure>();
            }
        }

        public static void UpdateNextIdFromFigures(IEnumerable<Figure> figures)
        {
            if (figures == null || !figures.Any()) return;
            int maxId = figures.Max(f => f.Id);
            int currentMax = Figure.GetCurrentMaxId();
            if (maxId >= currentMax)
                Figure.SetNextId(maxId + 1);
        }

        // Этап 2: Защита метода загрузки от вылетов
        public static FigureArray LoadFromFile(string filePath)
        {
            FigureArray loadedFigures = new FigureArray();

            if (!File.Exists(filePath)) return loadedFigures;

            try
            {
                // Читаем текст из файла
                string jsonText = File.ReadAllText(filePath);

                // Восстанавливаем список фигур из текста. 
                // Теперь настройки безопасны, а возможная ошибка будет поймана в блок catch.
                var tempLoaded = JsonConvert.DeserializeObject<List<Figure>>(jsonText, GetSettings());

                if (tempLoaded != null && tempLoaded.Count > 0)
                {
                    // Находим максимальный числовой ID среди загруженных и выставляем счетчик на +1, 
                    // чтобы новые фигуры получали уникальные идентификаторы.
                    int maxId = tempLoaded.Max(f => f.Id);
                    Figure.SetNextId(maxId + 1);

                    foreach (var fig in tempLoaded)
                    {
                        loadedFigures.Add(fig);
                    }
                }
            }
            catch (Exception ex)
            {
                // Если файл содержит старый или сломанный JSON, показываем ошибку вместо "падения" приложения
                MessageBox.Show($"Не удалось загрузить фигуры.\nДетали ошибки: {ex.Message}", "Ошибка загрузки", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return loadedFigures;
        }
    }
}