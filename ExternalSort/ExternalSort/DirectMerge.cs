using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;

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
                    break;
                }
                MergeStringsPairs();
            }
            Console.WriteLine();
            List<string> points = new List<string>();
            using (BinaryReader reader = new BinaryReader(File.OpenRead(FileInput), Encoding.UTF8))
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
                    break;
                }
                MergeNumbersPairs();
            }
            Console.WriteLine();
            List<double> points = new List<double>();
            using (BinaryReader reader = new BinaryReader(File.OpenRead(FileInput), Encoding.UTF8))
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
                        writerA.Write(element);
                    }
                    else
                    {
                        writerB.Write(element);
                    }
                    counter++;
                }
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
                            if (elementA < elementB)
                            {
                                bw.Write(elementA);
                                counterA--;
                                pickedA = false;
                            }
                            else
                            {
                                bw.Write(elementB);
                                counterB--;
                                pickedB = false;
                            }
                        }
                        else
                        {
                            bw.Write(elementA);
                            counterA--;
                            pickedA = false;
                        }
                    }
                    else if (pickedB)
                    {
                        bw.Write(elementB);
                        counterB--;
                        pickedB = false;
                    }
                }

                iterations *= 2; // увеличиваем размер серии в 2 раза
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
                        writerA.Write(element);
                    }
                    else
                    {
                        writerB.Write(element);
                    }
                    counter++;
                }
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
                        if (counterA > 0 && !pickedA)
                        {
                            try
                            {
                                elementA = readerA.ReadString();
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
                        if (counterB > 0 && !pickedB)
                        {
                            try
                            {
                                elementB = readerB.ReadString();
                                pickedB = true;
                            }
                            catch (Exception ignore)
                            {
                                endB = true;
                                pickedB = false;
                            }
                        }
                    }
                    

                    if (pickedA)
                    {
                        if (pickedB)
                        {
                            if (Menu.LeftBeforeRight(elementA , elementB))
                            {
                                bw.Write(elementA);
                                counterA--;
                                pickedA = false;
                            }
                            else
                            {
                                bw.Write(elementB);
                                counterB--;
                                pickedB = false;
                            }
                        }
                        else
                        {
                            bw.Write(elementA);
                            counterA--;
                            pickedA = false;
                        }
                    }
                    else if (pickedB)
                    {
                        bw.Write(elementB);
                        counterB--;
                        pickedB = false;
                    }
                }

                iterations *= 2; // увеличиваем размер серии в 2 раза
            }
        }

        
    }
}
