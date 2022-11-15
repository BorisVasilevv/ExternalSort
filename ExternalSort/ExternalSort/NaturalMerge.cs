using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace ExternalSort
{
    public class NaturalMerge
    {
        public string FileInput { get; set; }
        private long  segments;

        public NaturalMerge(string input)
        {
            FileInput = input;
            
        }

        public void Sort()
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
        }

        private void SplitToFiles() // разделение на 2 вспом. файла
        {
            segments = 1;
            using (BinaryReader br = new BinaryReader(File.OpenRead(FileInput), Encoding.UTF8))
            using (BinaryWriter writerA = new BinaryWriter(File.Create("..\\..\\..\\..\\NaturalMergeA.txt", 65536), Encoding.UTF8))
            using (BinaryWriter writerB = new BinaryWriter(File.Create("..\\..\\..\\..\\NaturalMergeB.txt", 65536), Encoding.UTF8))
            {
                long counter = 0;
                bool flag = true; // запись либо в 1-ый, либо во 2-ой файл
                bool endSequence;
                long length = br.BaseStream.Length;
                long position = 0;
                double element = br.ReadDouble();
                position += 8;
                double nextElement=0;
                if (length>8)
                {
                    nextElement = br.ReadDouble();
                }
                while (position != length)
                {
                    // если достигли количества элементов в последовательности -
                    // меняем флаг для след. файла и обнуляем счетчик количества
                    if (counter == 0 || nextElement >element)
                    {
                        flag = flag;
                    }
                    else
                    {
                        flag=!flag;
                        segments++;
                    }

                    

                    if (counter==0||flag)
                    {
                        writerA.Write(element);
                    }
                    else
                    {
                        writerB.Write(element);
                    }
                    counter++;
                    element=nextElement;
                    nextElement=br.ReadDouble();
                    position += 8;
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
                double nextElemA=0, nextElemB=0;
                bool pickedA = false, pickedB = false, endA = false, endB = false, endSequenceA=false, endSequenceB=false;
                long lengthA = readerA.BaseStream.Length;
                long lengthB = readerB.BaseStream.Length;
                long positionA = 0;
                long positionB = 0;
                while (!endA || !endB)
                {
                    
                    if (positionA != lengthA)
                    {
                        nextElemA = readerA.ReadDouble();
                        positionA += 8;
                        if(positionA != lengthA)
                        {
                            elementA = nextElemA;
                            nextElemA=readerA.ReadDouble();
                            positionA += 8;
                            if ( !pickedA)
                            {                               
                                pickedA = true;
                            }
                        }              
                    }
                    else
                    {
                        endA = true;
                    }

                    if (positionB != lengthB)
                    {
                        if (/*counterB > 0 &&*/ !pickedB)
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
                                //counterA--;
                                pickedA = false;
                            }
                            else
                            {
                                bw.Write(elementB);
                                //counterB--;
                                pickedB = false;
                            }
                        }
                        else
                        {
                            bw.Write(elementA);
                            //counterA--;
                            pickedA = false;
                        }
                    }
                    else if (pickedB)
                    {
                        bw.Write(elementB);
                        //counterB--;
                        pickedB = false;
                    }
                }

                
            }
        }
    }
}
