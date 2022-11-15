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

        public double[] Sort()
        {
            while (true)
            {
                SplitToFiles();
                // суть сортировки заключается в распределении на
                // отсортированные последовательности.
                // если после распределения на 2 вспомогательных файла
                // выясняется, что последовательность одна, значит файл
                // отсортирован, завершаем работу.
                if (segments == 1)
                {
                    break;
                }
                MergePairs();
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

        private void SplitToFiles() // разделение на 2 вспом. файла
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
                    writerA.Write(br.ReadDouble());
                    return;
                }

                while (position != length)
                {
                    // если достигли количества элементов в последовательности -
                    // меняем флаг для след. файла и обнуляем счетчик количества

                    if (!start)
                    {
                        element = br.ReadDouble();
                        position += 8;
                        writerA.Write(element);
                        start = true;
                    }

                    nextElement = br.ReadDouble();
                    position += 8;

                    if (element > nextElement)
                    {
                        flag = !flag;
                        segments++;
                    }

                    if (flag)
                    {
                        writerA.Write(nextElement);
                    }
                    else
                    {
                        writerB.Write(nextElement);
                    }
                    element = nextElement;

                }
            }
        }

        private void MergePairs() // слияние отсорт. последовательностей обратно в файл
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
                while (!endA || !endB || pickedA || pickedB)
                {
                    endA = positionA == lengthA;
                    endB= positionB == lengthB;
                    if(!endA&!pickedA)
                    {
                        elementA = readerA.ReadDouble();
                        positionA += 8;
                        pickedA = true;
                    }

                    if (!endB & !pickedB)
                    {
                        elementB = readerB.ReadDouble();
                        positionB += 8;
                        pickedB = true;
                    }

                    if (pickedA)
                    {
                        if (pickedB)
                        {
                            if (elementA < elementB)
                            {
                                bw.Write(elementA);
                                pickedA= false;
                            }
                            else
                            {
                                bw.Write(elementB);
                                pickedB= false;
                            }
                        }
                        else
                        {
                            bw.Write(elementA);
                            pickedA = false;
                        }
                    }
                    else if (pickedB)
                    {
                        bw.Write(elementB);
                        pickedB=false;
                    }
                }
            }
        }
    }
}
