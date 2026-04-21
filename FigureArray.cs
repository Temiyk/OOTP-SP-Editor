using System;
using System.Collections;
using System.Collections.Generic;

namespace Lab1.Shapes
{
    // Реализуем IEnumerable, чтобы по массиву можно было проходиться циклом foreach
    public class FigureArray : IEnumerable<Figure>
    {
        private Figure[] _items;
        private int _count;

        // Конструктор задает начальный размер массива
        public FigureArray(int initialCapacity = 4)
        {
            _items = new Figure[initialCapacity];
            _count = 0;
        }

        // Свойство для получения текущего количества фигур
        public int Count => _count;

        // Индексатор, чтобы обращаться к массиву как figures[i]
        public Figure this[int index]
        {
            get
            {
                if (index < 0 || index >= _count) throw new IndexOutOfRangeException("Индекс вне границ массива");
                return _items[index];
            }
            set
            {
                if (index < 0 || index >= _count) throw new IndexOutOfRangeException("Индекс вне границ массива");
                _items[index] = value;
            }
        }

        // Добавление в конец массива
        public void Add(Figure figure)
        {
            // Если массив заполнен, увеличиваем его размер в 2 раза
            if (_count == _items.Length)
            {
                Array.Resize(ref _items, _items.Length * 2);
            }

            _items[_count] = figure;
            _count++;
        }

        // Добавление нескольких элементов сразу (пригодится для разгруппировки)
        public void AddRange(IEnumerable<Figure> collection)
        {
            foreach (var item in collection) Add(item);
        }

        // Удаление конкретной фигуры
        public bool Remove(Figure figure)
        {
            int index = Array.IndexOf(_items, figure, 0, _count);
            if (index < 0) return false; // Фигура не найдена

            RemoveAt(index);
            return true;
        }

        // ПРОЦЕДУРА СМЕЩЕНИЯ: Удаление по индексу
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _count) throw new IndexOutOfRangeException("Индекс вне границ массива");

            // Сдвигаем все элементы, идущие после удаляемого, на одну позицию влево
            for (int i = index; i < _count - 1; i++)
            {
                _items[i] = _items[i + 1];
            }

            // Очищаем последнюю ячейку и уменьшаем счетчик
            _items[_count - 1] = null;
            _count--;
        }

        public void Clear()
        {
            Array.Clear(_items, 0, _count);
            _count = 0;
        }

        // Методы для поддержки цикла foreach
        public IEnumerator<Figure> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
            {
                yield return _items[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}