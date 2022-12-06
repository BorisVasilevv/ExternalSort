using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace ExternalSort
{
    public class NaturalMerge
    {
        //List<double> Main = new List<double>();
        //List<double> MainA = new List<double>();
        //List<double> MainB = new List<double>();
        public string FileInput { get; set; }
        private long segments;

        public NaturalMerge(string input)
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

        private void SplitNumbersToFiles() // разделение на 2 вспом. файла
        {
            segments = 1;
            using (BinaryReader br = new BinaryReader(File.OpenRead(FileInput), Encoding.UTF8))
            using (BinaryWriter writerA = new BinaryWriter(File.Create("..\\..\\..\\..\\NaturalMergeA.txt", 65536), Encoding.UTF8))
            using (BinaryWriter writerB = new BinaryWriter(File.Create("..\\..\\..\\..\\NaturalMergeB.txt", 65536), Encoding.UTF8))
            {
                bool start = false;
                bool flag = true; // запись либо в 1-ый, либо во 2-ой файл
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
                    // если достигли количества элементов в последовательности -
                    // меняем флаг для след. файла и обнуляем счетчик количества

                    if (!start)
                    {
                        element = br.ReadDouble();
                        Console.WriteLine($"Читаем из основного файла: {element}");
                        position += 8;
                        writerA.Write(element);
                        Console.WriteLine($"Записываем {element} в первый файл");
                        start = true;
                    }

                    nextElement = br.ReadDouble();
                    position += 8;
                    Console.WriteLine($"Читаем из основного файла: {nextElement}");


                    Console.WriteLine($"Сравниваем {element} и {nextElement}");
                    if (element > nextElement)
                    {
                        Console.WriteLine($"Так как {element} > {nextElement} отсортированный массив прервался будем писать в другой файл");
                        if (flag) writerA.Write(double.MaxValue);
                        else writerB.Write(double.MaxValue);
                        flag = !flag;
                        segments++;
                    }
                    else
                    {
                        Console.WriteLine($"Так как {element} < {nextElement} отсортированный массив не прервался будем писать в этот же файл");
                    }

                    if (flag)
                    {
                        Console.WriteLine($"Записываем {nextElement} в первый файл");
                        writerA.Write(nextElement);
                    }
                    else
                    {
                        Console.WriteLine($"Записываем {nextElement} во второй файл");
                        writerB.Write(nextElement);
                    }
                    element = nextElement;

                }
            }
        }

        private void MergeNumbersPairs() // слияние отсорт. последовательностей обратно в файл
        {
            using (BinaryReader readerA = new BinaryReader(File.OpenRead("..\\..\\..\\..\\NaturalMergeA.txt"), Encoding.UTF8))
            using (BinaryReader readerB = new BinaryReader(File.OpenRead("..\\..\\..\\..\\NaturalMergeB.txt"), Encoding.UTF8))
            using (BinaryWriter bw = new BinaryWriter(File.Create(FileInput, 65536)))
            {
                double elementA = 0, elementB = 0;
                bool pickedA = false, pickedB = false, endA = false, endB = false;
                long lengthA = readerA.BaseStream.Length;
                long lengthB = readerB.BaseStream.Length;
                long positionA = 0;
                long positionB = 0;

                bool endPartA = false;
                bool endPartB = false;
                Console.WriteLine($"Записываем обратно в один файл");
                while (!endA || !endB || pickedA || pickedB)
                {
                    endA = positionA == lengthA;
                    endB = positionB == lengthB;
                    if (endPartA && endPartB)
                    {
                        endPartA = false;
                        endPartB = false;
                    }
                    else if (endPartB && endA)
                    {
                        endPartB = false;
                    }
                    else if (endPartA && endB)
                    {
                        endPartA = false;
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



                    if (pickedA)
                    {
                        if (pickedB)
                        {
                            Console.WriteLine($"Сравниваем {elementA} и {elementB}");
                            if (elementA < elementB)
                            {
                                Console.WriteLine($"Так как {elementA} < {elementB} записываем в основной файл {elementA}");
                                bw.Write(elementA);
                                pickedA = false;
                            }
                            else
                            {
                                Console.WriteLine($"Так как {elementB} < {elementA} записываем в основной файл {elementB}");
                                bw.Write(elementB);
                                pickedB = false;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Так как был последовательноть из второго файла кончилась записываем в основной файл {elementA}");
                            bw.Write(elementA);
                            pickedA = false;
                        }
                    }
                    else if (pickedB)
                    {
                        Console.WriteLine($"Так как был последовательноть из первого файла кончилась записываем в основной файл {elementB}");
                        bw.Write(elementB);
                        pickedB = false;
                    }
                }

            }
        }

        private void SplitStringsToFiles()
        {
            segments = 1;
            using (BinaryReader br = new BinaryReader(File.OpenRead(FileInput), Encoding.UTF8))
            using (BinaryWriter writerA = new BinaryWriter(File.Create("..\\..\\..\\..\\NaturalMergeA.txt", 65536), Encoding.UTF8))
            using (BinaryWriter writerB = new BinaryWriter(File.Create("..\\..\\..\\..\\NaturalMergeB.txt", 65536), Encoding.UTF8))
            {
                bool start = false;
                bool flag = true; // запись либо в 1-ый, либо во 2-ой файл

                string element = null;
                string nextElement = null;
                bool end = false;

                while (!end)
                {
                    // если достигли количества элементов в последовательности -
                    // меняем флаг для след. файла и обнуляем счетчик количества

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
                        catch (Exception e)
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
                    catch (Exception e)
                    {
                        end = true;
                        break;
                    }


                    if (!Menu.LeftBeforeRight(element, nextElement))
                    {
                        Console.WriteLine($"Так как {element} позднее по алфавиту, чем {nextElement} отсортированный массив прервался будем писать в другой файл");
                        if (flag) writerA.Write(double.MaxValue.ToString());
                        else writerB.Write(double.MaxValue.ToString());
                        flag = !flag;
                        segments++;
                    }
                    else
                    {
                        Console.WriteLine($"Так как {element} раньше по алфовиту, чем {nextElement} отсортированный массив не прервался будем писать в этот же файл");
                    }

                    if (flag)
                    {
                        Console.WriteLine($"Записываем {nextElement} в первый файл");
                        writerA.Write(nextElement);
                    }
                    else
                    {
                        Console.WriteLine($"Записываем {nextElement} во второй файл");
                        writerB.Write(nextElement);
                    }
                    element = nextElement;

                }
            }
        }

        private void MergeStringsPairs()
        {
            using (BinaryReader readerA = new BinaryReader(File.OpenRead("..\\..\\..\\..\\NaturalMergeA.txt"), Encoding.UTF8))
            using (BinaryReader readerB = new BinaryReader(File.OpenRead("..\\..\\..\\..\\NaturalMergeB.txt"), Encoding.UTF8))
            using (BinaryWriter bw = new BinaryWriter(File.Create(FileInput, 65536)))
            {
                string elementA = null, elementB = null;
                bool pickedA = false, pickedB = false, endA = false, endB = false;

                bool endPartA=false, endPartB=false;
                Console.WriteLine($"Записываем обратно в один файл");
                while (!endA || !endB || pickedA || pickedB||!endPartA||!endPartB)
                {
                    if (endPartA && endPartB &&!(endA || endB))
                    {
                        endPartA = false;
                        endPartB = false;
                    }
                    else if (endPartB && endA)
                    {
                        endPartB = false;
                    }
                    else if (endPartA && endB)
                    {
                        endPartA = false;
                    }


                    if (!endA && !pickedA && !endPartA)
                    {
                        try
                        {
                            elementA = readerA.ReadString();
                            double a;
                            if(double.TryParse(elementA, out a))
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


                    if (pickedA)
                    {
                        if (pickedB)
                        {
                            Console.WriteLine($"Сравниваем {elementA} и {elementB}");
                            if (Menu.LeftBeforeRight(elementA, elementB))
                            {
                                Console.WriteLine($"Так как {elementA} по алфавиту раньше, чем {elementB} записываем в основной файл {elementA}");
                                bw.Write(elementA);
                                pickedA = false;
                            }
                            else
                            {
                                Console.WriteLine($"Так как {elementB} по алфавиту раньше, чем {elementA} записываем в основной файл {elementB}");
                                bw.Write(elementB);
                                pickedB = false;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Так как был последовательноть из второго файла кончилась записываем в основной файл {elementA}");
                            bw.Write(elementA);
                            pickedA = false;
                        }
                    }
                    else if (pickedB)
                    {
                        Console.WriteLine($"Так как был последовательноть из первого файла кончилась записываем в основной файл {elementB}");
                        bw.Write(elementB);
                        pickedB = false;
                    }
                }
            }
        }
    }
}
