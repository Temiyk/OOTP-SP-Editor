using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using Lab1.Shapes;

namespace Lab1
{
    public static class FileManager
    {
        // Настройки для сохранения: TypeNameHandling.All говорит библиотеке 
        // запоминать точные названия классов (Circle, CompositeFigure и т.д.)
        private static JsonSerializerSettings GetSettings()
        {
            return new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                Formatting = Formatting.Indented // Делаем текст красивым и читаемым
            };
        }

        public static void SaveToFile(FigureArray figures, string filePath)
        {
            // Для удобства сериализации перекладываем фигуры во временный стандартный список
            List<Figure> tempToSave = new List<Figure>();
            foreach (var fig in figures)
            {
                tempToSave.Add(fig);
            }

            // Превращаем список в текст
            string jsonText = JsonConvert.SerializeObject(tempToSave, GetSettings());

            // Записываем текст в файл
            File.WriteAllText(filePath, jsonText);
        }

        public static FigureArray LoadFromFile(string filePath)
        {
            FigureArray loadedFigures = new FigureArray();

            if (!File.Exists(filePath)) return loadedFigures;

            // Читаем текст из файла
            string jsonText = File.ReadAllText(filePath);

            // Восстанавливаем список фигур из текста
            var tempLoaded = JsonConvert.DeserializeObject<List<Figure>>(jsonText, GetSettings());
            if (tempLoaded != null && tempLoaded.Count > 0)
            {
                // Находим максимальный ID среди загруженных и выставляем счетчик на +1
                int maxId = tempLoaded.Max(f => f.Id);
                Figure.SetNextId(maxId + 1);

                foreach (var fig in tempLoaded) loadedFigures.Add(fig);
            }
            return loadedFigures;
        }
    }
}