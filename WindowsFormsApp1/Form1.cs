using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Drawing.Printing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private int[,] matrix;
        private int rows;
        private int columns;
        private string filePath;

        public Form1()
        {
            InitializeComponent();
        }

        // Обработчик события нажатия кнопки "Сгенерировать"
        private void btnGenerate_Click(object sender, EventArgs e)
        {
            // Проверяем, удалось ли успешно преобразовать введенное количество строк и столбцов в числа
            if (int.TryParse(numberRows.Text, out rows) && int.TryParse(numberColumns.Text, out columns))
            {
                // Проверяем, выбран ли флажок "Пустая матрица"
                if (checkBoxEmptyMatrix.Checked)
                {
                    // Создаем пустую матрицу
                    CreateEmptyMatrix();
                }
                else
                {
                    // Создаем случайно заполненную матрицу
                    CreateRandomMatrix();
                }

                // Отображаем матрицу в DataGridView
                DisplayMatrix();

                // Устанавливаем ширину столбцов DataGridView и отключаем автоматическую настройку ширины столбцов
                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    column.Width = 31;
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                }
            }
            else
            {
                // Выводим сообщение об ошибке при недопустимом вводе количества строк или столбцов
                MessageBox.Show("Недопустимый ввод для строк или столбцов.");
            }
        }

        // Создание случайно заполненной матрицы
        private void CreateRandomMatrix()
        {
            matrix = new int[rows, columns];
            Random random = new Random();

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    // Заполнение каждого элемента матрицы случайным числом в диапазоне от -100 до 100
                    matrix[i, j] = random.Next(-100, 101);
                }
            }
        }

        // Создание пустой матрицы
        private void CreateEmptyMatrix()
        {
            matrix = new int[rows, columns];
        }

        // Сортировка столбцов матрицы
        private void SortColumns(bool ascending)
        {
            // Создание массива для хранения произведений элементов столбцов
            int[] columnProducts = new int[columns];

            // Вычисление произведений элементов каждого столбца
            for (int j = 0; j < columns; j++)
            {
                int product = 1;

                for (int i = 0; i < rows; i++)
                {
                    product *= matrix[i, j];
                }

                columnProducts[j] = product;
            }

            // Сортировка индексов столбцов на основе произведений элементов
            int[] sortedIndices = Enumerable.Range(0, columns)
                .OrderBy(i => ascending ? columnProducts[i] : -columnProducts[i])
                .ToArray();

            // Создание новой матрицы для хранения отсортированных столбцов
            int[,] sortedMatrix = new int[rows, columns];

            // Копирование элементов из исходной матрицы в новую матрицу в соответствии с отсортированными индексами столбцов
            for (int j = 0; j < columns; j++)
            {
                for (int i = 0; i < rows; i++)
                {
                    sortedMatrix[i, j] = matrix[i, sortedIndices[j]];
                }
            }

            // Обновление заголовков столбцов в DataGridView
            for (int j = 0; j < columns; j++)
            {
                dataGridView1.Columns[j].HeaderText = "Column " + sortedIndices[j];
            }

            // Очистка строк в DataGridView
            dataGridView1.Rows.Clear();

            // Добавление строк в DataGridView с элементами из отсортированной матрицы
            for (int i = 0; i < rows; i++)
            {
                object[] row = new object[columns];

                for (int j = 0; j < columns; j++)
                {
                    row[j] = sortedMatrix[i, j];
                }

                dataGridView1.Rows.Add(row);
            }
        }


        // Отображение матрицы в DataGridView
        private void DisplayMatrix()
        {
            // Очистка строк и столбцов в DataGridView
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            // Добавление столбцов в DataGridView
            for (int j = 0; j < columns; j++)
            {
                // Создание нового столбца с заголовком "Column j"
                dataGridView1.Columns.Add("col" + j, "Column " + j);
            }

            // Добавление строк с элементами матрицы в DataGridView
            for (int i = 0; i < rows; i++)
            {
                object[] row = new object[columns];

                for (int j = 0; j < columns; j++)
                {
                    // Заполнение элементов строки значениями из матрицы
                    row[j] = matrix[i, j];
                }

                // Добавление строки в DataGridView
                dataGridView1.Rows.Add(row);
            }
        }

        // Обработчик события изменения выбранного переключателя сортировки на "по возрастанию"
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            SortColumns(true);
        }

        // Обработчик события изменения выбранного переключателя сортировки на "по убыванию"
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            SortColumns(false);
        }

        // Обработчик нажатия кнопки "Загрузить"
        private void btnLoad_Click(object sender, EventArgs e)
        {
            // Открытие диалогового окна для выбора файла
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Текстовый файл (*.txt)|*.txt";
            openFileDialog.Title = "Выберите текстовый файл";

            // Если пользователь выбрал файл и нажал "ОК"
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Получение пути к выбранному файлу
                filePath = openFileDialog.FileName;

                // Загрузка матрицы из файла
                LoadMatrixFromFile(filePath);
            }
        }

        // Загрузка матрицы из файла
        private void LoadMatrixFromFile(string filePath)
        {
            try
            {
                // Чтение всех строк из файла
                string[] lines = File.ReadAllLines(filePath);

                // Определение количества строк и столбцов в матрице
                rows = lines.Length;
                columns = lines[0].Split(' ').Length;

                // Создание новой матрицы с заданным размером
                matrix = new int[rows, columns];

                // Очистка строк и столбцов в DataGridView
                dataGridView1.Rows.Clear();
                dataGridView1.Columns.Clear();

                // Добавление столбцов в DataGridView
                for (int j = 0; j < columns; j++)
                {
                    dataGridView1.Columns.Add("col" + j, "Column " + j);

                    // Установка ширины и режима автоматического изменения размера для каждого столбца
                    dataGridView1.Columns[j].Width = 31;
                    dataGridView1.Columns[j].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                }

                // Добавление строк и заполнение матрицы значениями из файла
                for (int i = 0; i < rows; i++)
                {
                    dataGridView1.Rows.Add();

                    string[] values = lines[i].Split(' ');

                    for (int j = 0; j < columns; j++)
                    {
                        // Преобразование строкового значения в целое число и сохранение в матрице
                        matrix[i, j] = int.Parse(values[j]);

                        // Установка значения ячейки в DataGridView
                        dataGridView1.Rows[i].Cells[j].Value = matrix[i, j];
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при загрузке матрицы: " + ex.Message);
            }
        }

        // Обработчик нажатия кнопки "Сохранить"
        private void btnSave_Click(object sender, EventArgs e)
        {
            // Открытие диалогового окна для выбора файла сохранения
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Текстовый файл (*.txt)|*.txt";
            saveFileDialog.Title = "Выберите текстовый файл";

            // Если пользователь выбрал файл и нажал "ОК"
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Получение пути к выбранному файлу
                string filePath = saveFileDialog.FileName;

                // Сохранение отсортированной матрицы в файл
                SaveSortedMatrixToFile(filePath);
            }
        }


        // Сохранение отсортированной матрицы в файл
        private void SaveSortedMatrixToFile(string filePath)
        {
            try
            {
                // Создание новой матрицы для сохранения отсортированных значений
                int[,] sortedMatrix = new int[rows, columns];

                // Заполнение отсортированной матрицы значениями из DataGridView
                for (int j = 0; j < columns; j++)
                {
                    for (int i = 0; i < rows; i++)
                    {
                        sortedMatrix[i, j] = int.Parse(dataGridView1.Rows[i].Cells[j].Value.ToString());
                    }
                }

                // Запись отсортированной матрицы в файл
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < columns; j++)
                        {
                            writer.Write(sortedMatrix[i, j]);

                            if (j < columns - 1)
                            {
                                writer.Write(" ");
                            }
                        }

                        writer.WriteLine();
                    }
                }

                MessageBox.Show("Отсортированная матрица успешно сохранена!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при сохранении отсортированной матрицы: " + ex.Message);
            }
        }

        // Обработчик события изменения выбранного переключателя на "Показать исходную матрицу"
        private void radioShowOriginal_CheckedChanged(object sender, EventArgs e)
        {
            ShowOriginalMatrix();
        }

        // Отображение исходной матрицы в DataGridView
        private void ShowOriginalMatrix()
        {
            // Очистка строк в DataGridView
            dataGridView1.Rows.Clear();

            // Добавление строк с элементами исходной матрицы в DataGridView
            for (int i = 0; i < rows; i++)
            {
                object[] row = new object[columns];

                for (int j = 0; j < columns; j++)
                {
                    row[j] = matrix[i, j];
                }

                dataGridView1.Rows.Add(row);
            }
        }


        // Обработчик события окончания редактирования ячейки DataGridView
        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            int rowIndex = e.RowIndex;
            int columnIndex = e.ColumnIndex;

            // Проверка, что индексы ячейки находятся в допустимых границах матрицы
            if (rowIndex >= 0 && rowIndex < rows && columnIndex >= 0 && columnIndex < columns)
            {
                object cellValue = dataGridView1.Rows[rowIndex].Cells[columnIndex].Value;

                // Проверка, что значение ячейки является числом
                if (cellValue != null && int.TryParse(cellValue.ToString(), out int newValue))
                {
                    // Присваивание нового значения ячейке матрицы
                    matrix[rowIndex, columnIndex] = newValue;
                }
                else
                {
                    MessageBox.Show("Недопустимое значение ячейки.");
                }
            }
        }

        // Обработчик события проверки ячейки DataGridView на валидность значения
        private void dataGridView1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            DataGridViewCell cell = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];

            if (!string.IsNullOrEmpty(e.FormattedValue.ToString()))
            {
                int value;
                // Проверка, что значение ячейки является числом в диапазоне от -100 до 100
                if (!int.TryParse(e.FormattedValue.ToString(), out value) || value < -100 || value > 100)
                {
                    // Отмена редактирования ячейки и вывод сообщения об ошибке
                    dataGridView1.CancelEdit();
                    MessageBox.Show("Пожалуйста, введите число от -100 до 100.");
                }
            }
        }

        // Обработчик события отображения редактора ячейки DataGridView
        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            System.Windows.Forms.TextBox textBox = e.Control as System.Windows.Forms.TextBox;
            if (textBox != null)
            {
                // Установка обработчика события нажатия клавиш для текстового поля редактора ячейки
                textBox.KeyPress -= dataGridView1_KeyPress;
                textBox.KeyPress += dataGridView1_KeyPress;
            }
        }

        // Обработчик события нажатия клавиши в редакторе ячейки DataGridView
        private void dataGridView1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Запрещение ввода символов, кроме цифр и знака минуса
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '-')
            {
                e.Handled = true;
            }

            // Разрешение ввода знака минуса только в начале значения
            System.Windows.Forms.TextBox textBox = (System.Windows.Forms.TextBox)sender;
            if (e.KeyChar == '-' && textBox.SelectionStart != 0)
            {
                e.Handled = true;
            }
        }

        // Обработчик события нажатия кнопки "Печать"
        private void btnPrint_Click(object sender, EventArgs e)
        {
            PrintDocument printDocument = new PrintDocument();
            printDocument.PrintPage += PrintDocument_PrintPage;

            PrintDialog printDialog = new PrintDialog();
            printDialog.Document = printDocument;

            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                // Печать документа
                printDocument.Print();
            }
        }

        // Обработчик события печати страницы для PrintDocument
        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            // Создание битмапа для отрисовки DataGridView
            Bitmap bitmap = new Bitmap(dataGridView1.Width, dataGridView1.Height);
            dataGridView1.DrawToBitmap(bitmap, dataGridView1.ClientRectangle);

            // Определение размеров печатной области
            RectangleF bounds = e.PageSettings.PrintableArea;
            float ratio = Math.Min(bounds.Width / bitmap.Width, bounds.Height / bitmap.Height);
            int newWidth = (int)(bitmap.Width * ratio);
            int newHeight = (int)(bitmap.Height * ratio);

            // Отрисовка битмапа на печатной странице
            e.Graphics.DrawImage(bitmap, bounds.Left, bounds.Top, newWidth, newHeight);
        }

        // Обработчик события нажатия кнопки "Открыть файл"
        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                try
                {
                    // Открытие файла с помощью программы "Блокнот"
                    Process.Start("notepad.exe", filePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Произошла ошибка при открытии файла: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Файл не загружен.");
            }
        }

        // Обработчик события нажатия кнопки "Обновить"
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                // Загрузка матрицы из файла
                LoadMatrixFromFile(filePath);
            }
            else
            {
                MessageBox.Show("Файл не загружен.");
            }
        }
    }
}