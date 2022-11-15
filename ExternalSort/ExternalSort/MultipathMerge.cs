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
                    writerA.Write(br.ReadDouble());
                    return;
                }

                while (position != length)
                {
                    
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
                        if(fileNumber == FileToWrite.A) fileNumber = FileToWrite.B;
                        else if (fileNumber == FileToWrite.B) fileNumber = FileToWrite.C;
                        else if (fileNumber == FileToWrite.C) fileNumber = FileToWrite.A;

                        segments++;
                    }

                    if (fileNumber==FileToWrite.A)
                    {
                        writerA.Write(nextElement);
                    }
                    else if (fileNumber == FileToWrite.B)
                    {
                        writerB.Write(nextElement);
                    }
                    else if (fileNumber == FileToWrite.C)
                    {
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
                while (!endA || !endB|| !endC||pickedA||pickedC||pickedB)
                {
                    endA = positionA == lengthA;
                    endB = positionB == lengthB;
                    endC = positionC == lengthC;
                    if (!endA & !pickedA)
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

                    if (!endC & !pickedC)
                    {
                        elementC = readerC.ReadDouble();
                        positionC += 8;
                        pickedC = true;
                    }

                    if (pickedA)
                    {
                        if (pickedB)
                        {
                            if (pickedC)
                            {
                                if (elementA < elementB)
                                {
                                    if (elementA < elementC)
                                    {
                                        bw.Write(elementA);
                                        pickedA = false;
                                    }
                                    else
                                    {
                                        bw.Write(elementC);
                                        pickedC = false;
                                    }

                                }
                                else
                                {
                                    if (elementB < elementC)
                                    {
                                        bw.Write(elementB);
                                        pickedB = false;
                                    }
                                    else
                                    {
                                        bw.Write(elementC);
                                        pickedC = false;
                                    }
                                }
                            }
                            else
                            {
                                if (elementA < elementB)
                                {
                                    bw.Write(elementA);
                                    pickedA = false;
                                }
                                else
                                {
                                    bw.Write(elementB);
                                    pickedB = false;
                                }
                            }
                            
                        }
                        else if (pickedC)
                        {
                            if (elementA < elementC)
                            {
                                bw.Write(elementA);
                                pickedA = false;
                            }
                            else
                            {
                                bw.Write(elementC);
                                pickedC = false;
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
                        if (pickedC)
                        {
                            if (elementB < elementC)
                            {
                                bw.Write(elementB);
                                pickedB = false;
                            }
                            else
                            {
                                bw.Write(elementC);
                                pickedC = false;
                            }
                        }
                        else
                        {
                            bw.Write(elementB);
                            pickedB = false;
                        }
                    }
                    else if (pickedC)
                    {
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
                            writerA.Write(element);
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
                    }
                    catch
                    {
                        end = true;
                        break;
                    }


                    if (!Menu.LeftBeforeRight(element , nextElement))
                    {
                        if (fileNumber == FileToWrite.A) fileNumber = FileToWrite.B;
                        else if (fileNumber == FileToWrite.B) fileNumber = FileToWrite.C;
                        else if (fileNumber == FileToWrite.C) fileNumber = FileToWrite.A;

                        segments++;
                    }

                    if (fileNumber == FileToWrite.A)
                    {
                        writerA.Write(nextElement);
                    }
                    else if (fileNumber == FileToWrite.B)
                    {
                        writerB.Write(nextElement);
                    }
                    else if (fileNumber == FileToWrite.C)
                    {
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

                while (!endA || !endB || !endC || pickedA || pickedC || pickedB)
                {

                    if (!endA & !pickedA)
                    {
                        try
                        {
                            elementA = readerA.ReadString();

                            pickedA = true;
                        }
                        catch (Exception e)
                        {
                            pickedA = false;
                            endA = true;
                        }
                        
                    }

                    if (!endB & !pickedB)
                    {
                        try
                        {
                            elementB = readerB.ReadString();
                            pickedB = true;
                        }
                        catch (Exception e)
                        {
                            pickedB = false;
                            endB = true;
                        }
                    }

                    if (!endC & !pickedC)
                    {
                        try
                        {
                            elementC = readerC.ReadString();
                            pickedC = true;
                        }
                        catch (Exception e)
                        {
                            pickedC = false;
                            endC = true;
                        }
                    }

                    if (pickedA)
                    {
                        if (pickedB)
                        {
                            if (pickedC)
                            {
                                if (Menu.LeftBeforeRight(elementA , elementB))
                                {
                                    if (Menu.LeftBeforeRight(elementA , elementC))
                                    {
                                        bw.Write(elementA);
                                        pickedA = false;
                                    }
                                    else
                                    {
                                        bw.Write(elementC);
                                        pickedC = false;
                                    }

                                }
                                else
                                {
                                    if (Menu.LeftBeforeRight(elementB , elementC))
                                    {
                                        bw.Write(elementB);
                                        pickedB = false;
                                    }
                                    else
                                    {
                                        bw.Write(elementC);
                                        pickedC = false;
                                    }
                                }
                            }
                            else
                            {
                                if (Menu.LeftBeforeRight(elementA , elementB))
                                {
                                    bw.Write(elementA);
                                    pickedA = false;
                                }
                                else
                                {
                                    bw.Write(elementB);
                                    pickedB = false;
                                }
                            }

                        }
                        else if (pickedC)
                        {
                            if (Menu.LeftBeforeRight(elementA , elementC))
                            {
                                bw.Write(elementA);
                                pickedA = false;
                            }
                            else
                            {
                                bw.Write(elementC);
                                pickedC = false;
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
                        if (pickedC)
                        {
                            if (Menu.LeftBeforeRight(elementB , elementC))
                            {
                                bw.Write(elementB);
                                pickedB = false;
                            }
                            else
                            {
                                bw.Write(elementC);
                                pickedC = false;
                            }
                        }
                        else
                        {
                            bw.Write(elementB);
                            pickedB = false;
                        }
                    }
                    else if (pickedC)
                    {
                        bw.Write(elementC);
                        pickedC = false;
                    }
                }
            }
        }
    }
}
