using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace ExternalSort
{
    enum FileToWrite
    {
        A, B, C
    }
    public class MultipathMerge
    {
        public string FileInput { get; set; }
        private long segments;

        public MultipathMerge(string input)
        {
            FileInput = input;

        }

        public double[] Sort(double[] array)
        {
            return SortNumbers();
        }

        public string[] Sort(string[] array)
        {
            return SortStrings();
        }

        public string[] SortStrings()
        {
            while (true)
            {
                SplitStringsToFiles();
                // суть сортировки заключается в распределении на
                // отсортированные последовательности.
                // если после распределения на 2 вспомогательных файла
                // выясняется, что последовательность одна, значит файл
                // отсортирован, завершаем работу.
                if (segments == 1)
                {
                    MergeStringsPairs();
                    Console.WriteLine("Данные отсортированы");
                    break;
                }
                MergeStringsPairs();
            }
            return FileWorker.FromFileToStringArray(FileInput);
        }

        public double[] SortNumbers()
        {
            while (true)
            {
                SplitNumbersToFiles();
                // суть сортировки заключается в распределении на
                // отсортированные последовательности.
                // если после распределения на 2 вспомогательных файла
                // выясняется, что последовательность одна, значит файл
                // отсортирован, завершаем работу.
                if (segments == 1)
                {
                    MergeNumbersPairs();
                    Console.WriteLine("Данные отсортированы");
                    break;
                }
                MergeNumbersPairs();
            }

            return FileWorker.FromFileToNumberArray(FileInput);
        }

        private void SplitNumbersToFiles() // разделение на 3 вспом. файла
        {
            segments = 1;
            using (BinaryReader br = new BinaryReader(File.OpenRead(FileInput), Encoding.UTF8))
            using (BinaryWriter writerA = new BinaryWriter(File.Create("..\\..\\..\\..\\MultipathMergeA.txt", 65536), Encoding.UTF8))
            using (BinaryWriter writerB = new BinaryWriter(File.Create("..\\..\\..\\..\\MultipathMergeB.txt", 65536), Encoding.UTF8))
            using (BinaryWriter writerC = new BinaryWriter(File.Create("..\\..\\..\\..\\MultipathMergeC.txt", 65536), Encoding.UTF8))
            {
                bool start = false;
                FileToWrite fileNumber = FileToWrite.A;
                long length = br.BaseStream.Length;
                long position = 0;
                double element = 0;
                double nextElement = 0;

                if (length == 8)
                {
                    double element1 = br.ReadDouble();
                    Console.WriteLine($"Записываем {element1} в первый файл");
                    writerA.Write(element1);
                    return;
                }

                Console.WriteLine($"Записываем поочерёдно уже отсортированные массивы чисел в файл");
                while (position != length)
                {
                    if (!start)
                    {

                        element = br.ReadDouble();
                        Console.WriteLine($"Читаем из основного файла: {element}");
                        position += 8;
                        Console.WriteLine($"Записываем {element} в первый файл");
                        writerA.Write(element);
                        start = true;
                    }


                    nextElement = br.ReadDouble();
                    Console.WriteLine($"Читаем из основного файла: {nextElement}");
                    position += 8;


                    Console.WriteLine($"Сравниваем {element} и {nextElement}");
                    if (element > nextElement)
                    {
                        Console.WriteLine($"Так как {element} > {nextElement} отсортированный массив прервался");
                        if (fileNumber == FileToWrite.A)
                        {
                            Console.WriteLine($"Тепель будем писать во второй файл");
                            writerA.Write(double.MaxValue);
                            fileNumber = FileToWrite.B;
                        }
                        else if (fileNumber == FileToWrite.B)
                        {
                            Console.WriteLine($"Тепель будем писать в третий файл");
                            writerB.Write(double.MaxValue);
                            fileNumber = FileToWrite.C;
                        }
                        else if (fileNumber == FileToWrite.C)
                        {
                            Console.WriteLine($"Тепель будем писать в первый файл");
                            writerC.Write(double.MaxValue);
                            fileNumber = FileToWrite.A;
                        }


                        segments++;
                    }
                    else
                    {
                        Console.WriteLine($"Так как {element} < {nextElement} продолжаем записывать в тот же файл");
                    }

                    if (fileNumber == FileToWrite.A)
                    {
                        Console.WriteLine($"Записываем {nextElement} в первый файл");
                        writerA.Write(nextElement);
                    }
                    else if (fileNumber == FileToWrite.B)
                    {
                        Console.WriteLine($"Записываем {nextElement} во второй файл");
                        writerB.Write(nextElement);
                    }
                    else if (fileNumber == FileToWrite.C)
                    {
                        Console.WriteLine($"Записываем {nextElement} в третий файл");
                        writerC.Write(nextElement);
                    }
                    element = nextElement;
                }
            }
        }

        private void MergeNumbersPairs() // слияние отсорт. последовательностей обратно в файл
        {
            using (BinaryReader readerA = new BinaryReader(File.OpenRead("..\\..\\..\\..\\MultipathMergeA.txt"), Encoding.UTF8))
            using (BinaryReader readerB = new BinaryReader(File.OpenRead("..\\..\\..\\..\\MultipathMergeB.txt"), Encoding.UTF8))
            using (BinaryReader readerC = new BinaryReader(File.OpenRead("..\\..\\..\\..\\MultipathMergeC.txt"), Encoding.UTF8))
            using (BinaryWriter bw = new BinaryWriter(File.Create(FileInput, 65536)))
            {
                double elementA = 0, elementB = 0, elementC = 0;
                bool pickedA = false, pickedB = false, pickedC = false, endA = false, endB = false, endC = false;
                long lengthA = readerA.BaseStream.Length;
                long lengthB = readerB.BaseStream.Length;
                long lengthC = readerC.BaseStream.Length;
                long positionA = 0;
                long positionB = 0;
                long positionC = 0;

                bool endPartA = false;
                bool endPartB = false;
                bool endPartC = false;

                while (!endA || !endB || !endC || pickedA || pickedC || pickedB)
                {
                    endA = positionA == lengthA;
                    endB = positionB == lengthB;
                    endC = positionC == lengthC;


                    if (endPartA && endPartB && endPartC)
                    {
                        endPartA = false;
                        endPartB = false;
                        endPartC = false;
                    }
                    else if (endPartB && endA && endPartC)
                    {
                        endPartA = true;
                        endPartB = false;
                        endPartC = false;
                    }
                    else if (endPartB && endA && endC)
                    {
                        endPartA = true;
                        endPartB = false;
                        endPartC = true;
                    }
                    else if (endPartB && endPartA && endC)
                    {
                        endPartA = false;
                        endPartB = false;
                        endPartC = true;
                    }
                    else if (endPartA && endB && endPartC)
                    {
                        endPartA = false;
                        endPartB = true;
                        endPartC = false;
                    }
                    else if (endPartA && endB && endC)
                    {
                        endPartA = false;
                        endPartB = true;
                        endPartC = true;
                    }
                    else if (endPartC && endB && endA)
                    {
                        endPartA = true;
                        endPartB = true;
                        endPartC = false;
                    }

                    if (!endA && !pickedA && !endPartA)
                    {
                        elementA = readerA.ReadDouble();
                        if (elementA == double.MaxValue)
                        {
                            Console.WriteLine("Последовательность в 1 файле закончилась");
                            endPartA = true;
                        }
                        else
                        {
                            Console.WriteLine($"Читаем из первого файла: {elementA}");
                            pickedA = true;
                        }
                        positionA += 8;
                    }

                    if (!endB & !pickedB & !endPartB)
                    {
                        elementB = readerB.ReadDouble();
                        if (elementB == double.MaxValue)
                        {
                            Console.WriteLine("Последовательность во 2 файле закончилась");
                            endPartB = true;
                        }
                        else
                        {
                            Console.WriteLine($"Читаем из второго файла: {elementB}");
                            pickedB = true;
                        }
                        positionB += 8;
                    }

                    if (!endC & !pickedC & !endPartC)
                    {
                        elementC = readerC.ReadDouble();
                        if (elementC == double.MaxValue)
                        {
                            Console.WriteLine("Последовательность в 3 файле закончилась");
                            endPartC = true;
                        }
                        else
                        {
                            Console.WriteLine($"Читаем из третьего файла: {elementC}");
                            pickedC = true;
                        }
                        positionC += 8;
                    }



                    if (pickedA)
                    {
                        if (pickedB)
                        {
                            if (pickedC)
                            {
                                Console.WriteLine($"Сравниваем {elementA} и {elementB}");
                                if (elementA < elementB)
                                {
                                    Console.WriteLine($"Сравниваем {elementA} и {elementC}");
                                    if (elementA < elementC)
                                    {
                                        Console.WriteLine($"Наименьший элемент {elementA} его и запишем в основной файл");
                                        bw.Write(elementA);
                                        pickedA = false;
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Наименьший элемент {elementC} его и запишем в основной файл");
                                        bw.Write(elementC);
                                        pickedC = false;
                                    }

                                }
                                else
                                {
                                    Console.WriteLine($"Сравниваем {elementB} и {elementC}");
                                    if (elementB < elementC)
                                    {
                                        Console.WriteLine($"Наименьший элемент {elementB} его и запишем в основной файл");
                                        bw.Write(elementB);
                                        pickedB = false;
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Наименьший элемент {elementC} его и запишем в основной файл");
                                        bw.Write(elementC);
                                        pickedC = false;
                                    }
                                }
                            }
                            else
                            {
                                if (elementA < elementB)
                                {
                                    Console.WriteLine($"Наименьший элемент {elementA} его и запишем в основной файл");
                                    bw.Write(elementA);
                                    pickedA = false;
                                }
                                else
                                {
                                    Console.WriteLine($"Наименьший элемент {elementB} его и запишем в основной файл");
                                    bw.Write(elementB);
                                    pickedB = false;
                                }
                            }

                        }
                        else if (pickedC)
                        {
                            if (elementA < elementC)
                            {
                                Console.WriteLine($"Наименьший элемент {elementA} его и запишем в основной файл");
                                bw.Write(elementA);
                                pickedA = false;
                            }
                            else
                            {
                                Console.WriteLine($"Наименьший элемент {elementC} его и запишем в основной файл");
                                bw.Write(elementC);
                                pickedC = false;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Наименьший элемент {elementA} его и запишем в основной файл");
                            bw.Write(elementA);
                            pickedA = false;
                        }
                    }
                    else if (pickedB)
                    {
                        if (pickedC)
                        {
                            Console.WriteLine($"Сравниваем {elementB} и {elementC}");
                            if (elementB < elementC)
                            {
                                Console.WriteLine($"Наименьший элемент {elementB} его и запишем в основной файл");
                                bw.Write(elementB);
                                pickedB = false;
                            }
                            else
                            {
                                Console.WriteLine($"Наименьший элемент {elementC} его и запишем в основной файл");
                                bw.Write(elementC);
                                pickedC = false;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Наименьший элемент {elementB} его и запишем в основной файл");
                            bw.Write(elementB);
                            pickedB = false;
                        }
                    }
                    else if (pickedC)
                    {
                        Console.WriteLine($"Наименьший элемент {elementC} его и запишем в основной файл");
                        bw.Write(elementC);
                        pickedC = false;
                    }
                }
            }
        }

        private void SplitStringsToFiles()
        {
            segments = 1;
            using (BinaryReader br = new BinaryReader(File.OpenRead(FileInput), Encoding.UTF8))
            using (BinaryWriter writerA = new BinaryWriter(File.Create("..\\..\\..\\..\\MultipathMergeA.txt", 65536), Encoding.UTF8))
            using (BinaryWriter writerB = new BinaryWriter(File.Create("..\\..\\..\\..\\MultipathMergeB.txt", 65536), Encoding.UTF8))
            using (BinaryWriter writerC = new BinaryWriter(File.Create("..\\..\\..\\..\\MultipathMergeC.txt", 65536), Encoding.UTF8))
            {
                bool start = false;
                FileToWrite fileNumber = FileToWrite.A;


                string element = null, nextElement = null;
                bool end = false;
                while (!end)
                {

                    if (!start)
                    {
                        try
                        {
                            element = br.ReadString();
                            Console.WriteLine($"Читаем из основного файла: {element}");
                            writerA.Write(element);
                            Console.WriteLine($"Записываем {element} в первый файл");
                            start = true;
                        }
                        catch
                        {
                            end = true;
                            break;
                        }
                    }

                    try
                    {
                        nextElement = br.ReadString();
                        Console.WriteLine($"Читаем из основного файла: {nextElement}");
                    }
                    catch
                    {
                        end = true;
                        break;
                    }


                    if (!Menu.LeftBeforeRight(element, nextElement))
                    {
                        Console.WriteLine($"Так как {element} позднее по алфавиту, чем {nextElement} отсортированный массив прервался будем писать в другой файл");
                        if (fileNumber == FileToWrite.A)
                        {
                            writerA.Write(double.MaxValue.ToString());
                            fileNumber = FileToWrite.B;
                        }
                        else if (fileNumber == FileToWrite.B)
                        {
                            writerB.Write(double.MaxValue.ToString());
                            fileNumber = FileToWrite.C;
                        }
                        else if (fileNumber == FileToWrite.C)
                        {
                            writerC.Write(double.MaxValue.ToString());
                            fileNumber = FileToWrite.A;
                        }

                        segments++;
                    }
                    else
                    {
                        Console.WriteLine($"Так как {element} раньше по алфовиту, чем {nextElement} отсортированный массив не прервался будем писать в этот же файл");
                    }

                    if (fileNumber == FileToWrite.A)
                    {
                        Console.WriteLine($"Записываем {nextElement} в первый файл");
                        writerA.Write(nextElement);
                    }
                    else if (fileNumber == FileToWrite.B)
                    {
                        Console.WriteLine($"Записываем {nextElement} во второй файл");
                        writerB.Write(nextElement);
                    }
                    else if (fileNumber == FileToWrite.C)
                    {
                        Console.WriteLine($"Записываем {nextElement} в третий файл");
                        writerC.Write(nextElement);
                    }
                    element = nextElement;
                }
            }
        }

        private void MergeStringsPairs()
        {
            using (BinaryReader readerA = new BinaryReader(File.OpenRead("..\\..\\..\\..\\MultipathMergeA.txt"), Encoding.UTF8))
            using (BinaryReader readerB = new BinaryReader(File.OpenRead("..\\..\\..\\..\\MultipathMergeB.txt"), Encoding.UTF8))
            using (BinaryReader readerC = new BinaryReader(File.OpenRead("..\\..\\..\\..\\MultipathMergeC.txt"), Encoding.UTF8))
            using (BinaryWriter bw = new BinaryWriter(File.Create(FileInput, 65536)))
            {
                string elementA = null, elementB = null, elementC = null;
                bool pickedA = false, pickedB = false, pickedC = false, endA = false, endB = false, endC = false;

                bool endPartA = false, endPartB = false, endPartC = false;
                Console.WriteLine($"Записываем обратно в один файл");
                while (!endA || !endB || !endC || pickedA || pickedC || pickedB)
                {
                    if(endA) endPartA=true;
                    if (endB) endPartB = true;
                    if (endC) endPartC = true;

                    if (endPartA && endPartB && endPartC)
                    {
                        endPartA = false;
                        endPartB = false;
                        endPartC = false;
                    }
                    else if (endPartB && endA && endPartC)
                    {
                        endPartA = true;
                        endPartB = false;
                        endPartC = false;
                    }
                    else if (endPartB && endA && endC)
                    {
                        endPartA = true;
                        endPartB = false;
                        endPartC = true;
                    }
                    else if (endPartB && endPartA && endC)
                    {
                        endPartA = false;
                        endPartB = false;
                        endPartC = true;
                    }
                    else if (endPartA && endB && endPartC)
                    {
                        endPartA = false;
                        endPartB = true;
                        endPartC = false;
                    }
                    else if (endPartA && endB && endC)
                    {
                        endPartA = false;
                        endPartB = true;
                        endPartC = true;
                    }
                    else if (endPartC && endB && endA)
                    {
                        endPartA = true;
                        endPartB = true;
                        endPartC = false;
                    }

                    if (!endA && !pickedA && !endPartA)
                    {
                        try
                        {
                            elementA = readerA.ReadString();
                            double a;
                            if (double.TryParse(elementA, out a))
                            {
                                if (a == double.MaxValue)
                                {
                                    Console.WriteLine("Последовательность в 1 файле закончилась");
                                    endPartA = true;
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Читаем из первого файла: {elementA}");
                                pickedA = true;
                            }
                        }
                        catch
                        {
                            Console.WriteLine("1 файл закончился");
                            endA = true;
                            pickedA = false;
                            endPartA = true;
                        }
                    }

                    if (!endB && !pickedB && !endPartB)
                    {
                        try
                        {
                            elementB = readerB.ReadString();
                            double a;
                            if (double.TryParse(elementB, out a))
                            {
                                if (a == double.MaxValue)
                                {
                                    Console.WriteLine("Последовательность во 2 файле закончилась");
                                    endPartB = true;
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Читаем из второго файла: {elementB}");
                                pickedB = true;
                            }

                        }
                        catch
                        {
                            Console.WriteLine("2 файл закончился");
                            endB = true;
                            pickedB = false;
                            endPartB = true;
                        }
                    }

                    if (!endC && !pickedC && !endPartC)
                    {
                        try
                        {
                            elementC = readerC.ReadString();
                            double a;
                            if (double.TryParse(elementC, out a))
                            {
                                if (a == double.MaxValue)
                                {
                                    Console.WriteLine("Последовательность в 3 файле закончилась");
                                    endPartC = true;
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Читаем из третьего файла: {elementC}");
                                pickedC = true;
                            }

                        }
                        catch
                        {
                            Console.WriteLine("3 файл закончился");
                            endC = true;
                            pickedC = false;
                            endPartC = true;
                        }
                    }

                    if (pickedA)
                    {
                        if (pickedB)
                        {
                            if (pickedC)
                            {
                                Console.WriteLine($"Сравниваем {elementA} и {elementB}");
                                if (Menu.LeftBeforeRight(elementA, elementB))
                                {
                                    Console.WriteLine($"Сравниваем {elementA} и {elementC}");
                                    if (Menu.LeftBeforeRight(elementA, elementC))
                                    {
                                        Console.WriteLine($"Так как строка {elementA} по алфавиту раньше, других записываем в основной файл {elementA}");
                                        bw.Write(elementA);
                                        pickedA = false;
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Так как строка {elementC} по алфавиту раньше, других записываем в основной файл {elementC}");
                                        bw.Write(elementC);
                                        pickedC = false;
                                    }

                                }
                                else
                                {
                                    Console.WriteLine($"Сравниваем {elementB} и {elementC}");
                                    if (Menu.LeftBeforeRight(elementB, elementC))
                                    {
                                        Console.WriteLine($"Так как строка {elementB} по алфавиту раньше, других записываем в основной файл {elementB}");
                                        bw.Write(elementB);
                                        pickedB = false;
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Так как строка {elementC} по алфавиту раньше, других записываем в основной файл {elementC}");
                                        bw.Write(elementC);
                                        pickedC = false;
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Сравниваем {elementA} и {elementB}");
                                if (Menu.LeftBeforeRight(elementA, elementB))
                                {
                                    Console.WriteLine($"Так как строка {elementA} по алфавиту раньше, других записываем в основной файл {elementA}");
                                    bw.Write(elementA);
                                    pickedA = false;
                                }
                                else
                                {
                                    Console.WriteLine($"Так как строка {elementB} по алфавиту раньше, других записываем в основной файл {elementB}");
                                    bw.Write(elementB);
                                    pickedB = false;
                                }
                            }

                        }
                        else if (pickedC)
                        {
                            Console.WriteLine($"Сравниваем {elementA} и {elementC}");
                            if (Menu.LeftBeforeRight(elementA, elementC))
                            {
                                Console.WriteLine($"Так как строка {elementA} по алфавиту раньше, других записываем в основной файл {elementA}");

                                bw.Write(elementA);
                                pickedA = false;
                            }
                            else
                            {
                                Console.WriteLine($"Так как строка {elementC} по алфавиту раньше, других записываем в основной файл {elementC}");
                                bw.Write(elementC);
                                pickedC = false;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Так как строка {elementA} по алфавиту раньше, других записываем в основной файл {elementA}");
                            bw.Write(elementA);
                            pickedA = false;
                        }
                    }
                    else if (pickedB)
                    {
                        if (pickedC)
                        {
                            Console.WriteLine($"Сравниваем {elementB} и {elementC}");
                            if (Menu.LeftBeforeRight(elementB, elementC))
                            {
                                Console.WriteLine($"Так как строка {elementB} по алфавиту раньше, других записываем в основной файл {elementB}");
                                bw.Write(elementB);
                                pickedB = false;
                            }
                            else
                            {
                                Console.WriteLine($"Так как строка {elementC} по алфавиту раньше, других записываем в основной файл {elementC}");
                                bw.Write(elementC);
                                pickedC = false;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Так как строка {elementB} по алфавиту раньше, других записываем в основной файл {elementB}");
                            bw.Write(elementB);
                            pickedB = false;
                        }
                    }
                    else if (pickedC)
                    {
                        Console.WriteLine($"Так как строка {elementC} по алфавиту раньше, других записываем в основной файл {elementC}");
                        bw.Write(elementC);
                        pickedC = false;
                    }
                }
            }
        }
    }
}
