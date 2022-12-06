using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ExternalSort
{
    public class DirectMerge
    {
        public string FileInput { get; set; }
        private long iterations, segments;

        public DirectMerge(string input)
        {
            FileInput = input;
            iterations = 1; // степень двойки, количество элементов в каждой последовательности
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
                Console.WriteLine("Разбиваем из одного файла в два");
                SplitNumbersToFiles();
                if (segments == 1)
                {
                    MergeNumbersPairs();
                    Console.WriteLine("Данные отсортированы");
                    break;
                }
                Console.WriteLine("Производим слияние из двух файлов в один");
                MergeNumbersPairs();
            }

            return FileWorker.FromFileToNumberArray(FileInput);
        }

        private void SplitNumbersToFiles() // разделение на 2 вспом. файла
        {
            segments = 1;
            using (BinaryReader br = new BinaryReader(File.OpenRead(FileInput), Encoding.UTF8))
            using (BinaryWriter writerA = new BinaryWriter(File.Create("..\\..\\..\\..\\DirectMergeA.txt", 65536), Encoding.UTF8))
            using (BinaryWriter writerB = new BinaryWriter(File.Create("..\\..\\..\\..\\DirectMergeB.txt", 65536), Encoding.UTF8))
            {
                long counter = 0;
                bool flag = true; // запись либо в 1-ый, либо во 2-ой файл

                long length = br.BaseStream.Length;
                long position = 0;
                Console.WriteLine($"Записываем поочерёдно серии по {iterations} чисел в файл");
                while (position != length)
                {
                    // если достигли количества элементов в последовательности -
                    // меняем флаг для след. файла и обнуляем счетчик количества
                    if (counter == iterations)
                    {
                        flag = !flag;
                        counter = 0;
                        segments++;
                    }

                    double element = br.ReadDouble();

                    position += 8;
                    if (flag)
                    {
                        Console.WriteLine($"Записываем элемент {element} в первый файл");
                        writerA.Write(element);
                    }
                    else
                    {
                        Console.WriteLine($"Записываем элемент {element} во второй файл");
                        writerB.Write(element);
                    }
                    counter++;
                }

                Console.WriteLine("Последовательность закончилась");
            }
        }

        private void MergeNumbersPairs() // слияние отсорт. последовательностей обратно в файл
        {
            using (BinaryReader readerA = new BinaryReader(File.OpenRead("..\\..\\..\\..\\DirectMergeA.txt"), Encoding.UTF8))
            using (BinaryReader readerB = new BinaryReader(File.OpenRead("..\\..\\..\\..\\DirectMergeB.txt"), Encoding.UTF8))
            using (BinaryWriter bw = new BinaryWriter(File.Create(FileInput, 65536)))
            {
                long counterA = iterations, counterB = iterations;
                double elementA = 0, elementB = 0;
                bool pickedA = false, pickedB = false, endA = false, endB = false;
                long lengthA = readerA.BaseStream.Length;
                long lengthB = readerB.BaseStream.Length;
                long positionA = 0;
                long positionB = 0;
                while (!endA || !endB)
                {
                    if (counterA == 0 && counterB == 0)
                    {
                        counterA = iterations;
                        counterB = iterations;
                    }

                    if (positionA != lengthA)
                    {
                        if (counterA > 0 && !pickedA)
                        {
                            
                            elementA = readerA.ReadDouble();
                            Console.WriteLine($"Считываем элемент из первого файла: {elementA}");
                            positionA += 8;
                            pickedA = true;
                        }
                    }
                    else
                    {
                        endA = true;
                    }

                    if (positionB != lengthB)
                    {
                        if (counterB > 0 && !pickedB)
                        {
                            
                            elementB = readerB.ReadDouble();
                            Console.WriteLine($"Считываем элемент из второго файла: {elementB}");
                            positionB += 8;
                            pickedB = true;
                        }
                    }
                    else
                    {
                        endB = true;
                    }

                    if (pickedA)
                    {
                        if (pickedB)
                        {
                            Console.WriteLine($"Сравниваем {elementA} и {elementB}");
                            if (elementA < elementB)
                            {
                                bw.Write(elementA);
                                Console.WriteLine($"Так как {elementA} < {elementB} записываем в файл {elementA}");
                                counterA--;
                                pickedA = false;
                            }
                            else
                            {
                                Console.WriteLine($"Так как {elementB} < {elementA} записываем в файл {elementB}");
                                bw.Write(elementB);
                                counterB--;
                                pickedB = false;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Так как был последовательноть из второго файла кончилась записываем в файл {elementA}");
                            bw.Write(elementA);
                            counterA--;
                            pickedA = false;
                        }
                    }
                    else if (pickedB)
                    {
                        bw.Write(elementB);
                        Console.WriteLine($"Так как был последовательноть из первого файла кончилась записываем в файл {elementB}");
                        counterB--;
                        pickedB = false;
                    }
                }

                iterations *= 2; //
                Console.WriteLine("Увеличиваем размер серии в 2 раза");
            }
        }

        private void SplitStringsToFiles()
        {
            segments = 1;
            using (BinaryReader br = new BinaryReader(File.OpenRead(FileInput), Encoding.UTF8))
            using (BinaryWriter writerA = new BinaryWriter(File.Create("..\\..\\..\\..\\DirectMergeA.txt", 65536), Encoding.UTF8))
            using (BinaryWriter writerB = new BinaryWriter(File.Create("..\\..\\..\\..\\DirectMergeB.txt", 65536), Encoding.UTF8))
            {
                long counter = 0;
                bool flag = true; // запись либо в 1-ый, либо во 2-ой файл
                bool readerEmpty = false;
                Console.WriteLine($"Записываем поочерёдно серии по {iterations} строк в файл");
                while (!readerEmpty)
                {
                    // если достигли количества элементов в последовательности -
                    // меняем флаг для след. файла и обнуляем счетчик количества
                    if (counter == iterations)
                    {
                        flag = !flag;
                        counter = 0;
                        segments++;
                    }
                    string element=null;
                    try
                    {
                        element = br.ReadString();
                    }
                    catch (Exception ignoreException)
                    {

                        readerEmpty = true;
                        break;
                    }


                    if (flag)
                    {
                        Console.WriteLine($"Записываем элемент {element} в первый файл");
                        writerA.Write(element);
                    }
                    else
                    {
                        Console.WriteLine($"Записываем элемент {element} во второй файл");
                        writerB.Write(element);
                    }
                    counter++;
                }

                Console.WriteLine("Последовательность закончилась");
            }
        }

        private void MergeStringsPairs()
        {
            using (BinaryReader readerA = new BinaryReader(File.OpenRead("..\\..\\..\\..\\DirectMergeA.txt"), Encoding.UTF8))
            using (BinaryReader readerB = new BinaryReader(File.OpenRead("..\\..\\..\\..\\DirectMergeB.txt"), Encoding.UTF8))
            using (BinaryWriter bw = new BinaryWriter(File.Create(FileInput, 65536)))
            {
                long counterA = iterations, counterB = iterations;
                string elementA = null, elementB = null;
                bool pickedA = false, pickedB = false, endA = false, endB = false;
                while (!endA || !endB)
                {
                    if (counterA == 0 && counterB == 0)
                    {
                        counterA = iterations;
                        counterB = iterations;
                    }

                    if (!endA)
                    {
                        if (!pickedA)
                        {
                            try
                            {
                                elementA = readerA.ReadString();
                                Console.WriteLine($"Считываем элемент из первого файла: {elementA}");
                                pickedA = true;
                            }
                            catch (Exception ignore)
                            {
                                endA = true;
                                pickedA= false;
                            }
                        }

                    }
                    

                    if (!endB)
                    {
                        if (!pickedB)
                        {
                            try
                            {
                                elementB = readerB.ReadString();
                                Console.WriteLine($"Считываем элемент из второго файла: {elementB}");
                                pickedB = true;
                            }
                            catch (Exception ignore)
                            {
                                endB = true;
                                pickedB = false;
                            }
                        }
                    }
                    

                    if (counterA > 0 && pickedA)
                    {
                        if (counterB > 0 && pickedB)
                        {
                            if (Menu.LeftBeforeRight(elementA , elementB))
                            {
                                Console.WriteLine($"Так как строка {elementA} идёт раньше строки {elementB} записываем в файл {elementA}");
                                bw.Write(elementA);
                                counterA--;
                                pickedA = false;
                            }
                            else
                            {
                                Console.WriteLine($"Так как строка {elementB} идёт раньше строки {elementA} записываем в файл {elementB}");

                                bw.Write(elementB);
                                counterB--;
                                pickedB = false;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Так как был последовательноть из второго файла кончилась записываем в файл {elementA}");
                            bw.Write(elementA);
                            counterA--;
                            pickedA = false;
                        }
                    }
                    else if (pickedB)
                    {
                        Console.WriteLine($"Так как был последовательноть из первого файла кончилась записываем в файл {elementB}");
                        bw.Write(elementB);
                        counterB--;
                        pickedB = false;
                    }
                }

                iterations *= 2; // увеличиваем размер серии в 2 раза
                Console.WriteLine("Увеличиваем размер серии в 2 раза");
            }
        }

        
    }
}
