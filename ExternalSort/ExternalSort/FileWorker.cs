using ExternalSort;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExternalSort
{
    internal class FileWorker
    {
        public static string Filepath;
        public static string SortedFilepath;

        public static string Filepath0 = @"C:\Users\PC\Desktop\C#\ExternalSort\CountryTable.xlsx";
        public static string SortedFilepath0 = @"C:\Users\PC\Desktop\C#\ExternalSort\SortedCountryTable.xlsx";
        public static string Filepath1 = @"C:\Users\PC\Desktop\C#\ExternalSort\ChemicalSubstances.xlsx";
        public static string SortedFilepath1 = @"C:\Users\PC\Desktop\C#\ExternalSort\SortedChemicalSubstances.xlsx";
        public static string Filepath2 = @"C:\Users\PC\Desktop\C#\ExternalSort\RussianWords.xlsx";
        public static string SortedFilepath2 = @"C:\Users\PC\Desktop\C#\ExternalSort\SortedRussianWords.xlsx";
        public static string Extension = ".xlsx";

        public static void FileToSortGeneration(string file, double[] array)
        {
            using (BinaryWriter bw = new BinaryWriter(File.Create(file, 256), Encoding.UTF8))
            {
                foreach (double elem in array)
                    bw.Write(elem);
            }
        }

        public static void FileToSortGeneration(string file, string[] array)
        {
            using (BinaryWriter bw = new BinaryWriter(File.Create(file, 256), Encoding.UTF8))
            {
                foreach (string elem in array)
                    bw.Write(elem);
            }
        }



        public static void ChoseFile()
        {
            int a = Menu.GetNumberOfAnswer("Выберите таблицу для работы",
                new string[] { "Страны", "Вещества", "Слова" });

            if (a == 0)
            {
                Filepath = Filepath0;
                SortedFilepath = SortedFilepath0;

            }
            else if (a == 1)
            {
                Filepath = Filepath1;
                SortedFilepath = SortedFilepath1;
            }
            else if (a == 2)
            {   
                Filepath = Filepath2;
                SortedFilepath = SortedFilepath2;
            }
        }

        public static (string[], string[,]) ReadFile()
        {
            Excel.Application objWorkExcel = new Excel.Application(); //открыть эксель
            Excel.Workbook objWorkBook = objWorkExcel.Workbooks.Open(Filepath, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing); //открыть файл
            Excel.Worksheet objWorkSheet = (Excel.Worksheet)objWorkBook.Sheets[1]; //получить 1 лист
            var lastCell = objWorkSheet.Cells.SpecialCells(Excel.XlCellType.xlCellTypeLastCell);//1 ячейку

            string[] headlines = new string[lastCell.Column];
            //колонка, строка
            string[,] list = new string[lastCell.Column, lastCell.Row - 1]; // массив значений с листа равен по размеру листу
            for (int i = 0; i < lastCell.Column; i++) //по всем
            {
                for (int j = 0; j < lastCell.Row; j++) // по всем строкам
                {
                    if (j != 0) list[i, j - 1] = objWorkSheet.Cells[j + 1, i + 1].Text.ToString();//считываем текст в строку
                    else headlines[i] = objWorkSheet.Cells[1, i + 1].Text.ToString();
                }
            }
            objWorkBook.Close(false, Type.Missing, Type.Missing); //закрыть не сохраняя
            objWorkExcel.Quit(); // выйти из экселя
            GC.Collect(); // убрать за собой

            return (headlines, list);
        }

        public static double[] FromFileToNumberArray(string file)
        {
            List<double> points = new List<double>();
            using (BinaryReader reader = new BinaryReader(File.OpenRead(file), Encoding.UTF8))
            {
                long length = reader.BaseStream.Length;
                long position = 0;
                while (position != length)
                {
                    points.Add(reader.ReadDouble());
                    position += 8;
                }
            }
            return points.ToArray();
        }

        public static string[] FromFileToStringArray(string file)
        {
            List<string> points = new List<string>();
            using (BinaryReader reader = new BinaryReader(File.OpenRead(file), Encoding.UTF8))
            {
                while (true)
                {
                    try
                    {
                        points.Add(reader.ReadString());
                    }
                    catch (Exception ignoreException)
                    {
                        break;
                    }
                }
            }
            return points.ToArray();
        }

public static void WriteToFile(string[] headlines, string[,] data)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < headlines.Length; i++) 
            {
                sb.Append(headlines[i]);
                sb.Append('\t');

            }
            sb.Append('\n');
            for (int i = 0; i < data.GetLength(1); i++)
            {
                for (int j = 0; j < data.GetLength(0); j++)
                {
                    sb.Append(data[j, i]);
                    sb.Append('\t');
                }
                sb.Append('\n');
            }
            File.WriteAllText(SortedFilepath, sb.ToString());
        }

        public static void WriteToFile(string[] data)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sb.Append(data[i]);
                sb.Append('\n');
            }
            File.WriteAllText(SortedFilepath, sb.ToString());
        }
    }
}
