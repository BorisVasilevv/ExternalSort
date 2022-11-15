using System;
using System.Text;

namespace ExternalSort
{
    class Program
    {
        public static double[] arr = new double[] {119,354,293,56,164,45,309,241,124,45};
        public static void Main(string[] args)
        {
            string file = "..\\..\\..\\..\\Merge.txt";
            LargeFileGeneration(file);
            DirectMerge direct = new DirectMerge(file);
            NaturalMerge natural = new NaturalMerge(file);
            MultipathMerge multipath = new MultipathMerge(file);
            //double[] outd = direct.Sort();
            double[] outn=natural.Sort();
            //double[] outm = multipath.Sort();

        }


        public static void LargeFileGeneration(string file)
        {
            using (BinaryWriter bw = new BinaryWriter(File.Create(file, 256), Encoding.UTF8))
            {
                Random rnd = new Random();
                for (int i = 0; i < 10; i++)
                {
                    double a = rnd.Next(500);
                    bw.Write(arr[i]);
                    //arr[i] = a;
                }
            }
        }

        public static void ChangePositionInArray<T>(string[,] arrayToChange, T[] newPosition, int positionOfColumn)
        {
            string[,] CopyOfArray = new string[arrayToChange.GetLength(0), arrayToChange.GetLength(1)];

            for (int i = 0; i < arrayToChange.GetLength(0); i++)
                for (int j = 0; j < arrayToChange.GetLength(1); j++)
                    CopyOfArray[i, j] = new(arrayToChange[i, j]);

            for (int j = 0; j < newPosition.Length; j++)
            {
                for (int i = 0; i < CopyOfArray.GetLength(1); i++)
                {

                    if (CopyOfArray[positionOfColumn, i].Equals(newPosition[j].ToString()))
                    {
                        for (int k = 0; k < CopyOfArray.GetLength(0); k++)
                        {
                            arrayToChange[k, j] = CopyOfArray[k, i];
                        }
                    }
                }
            }
        }

        public static int GetNumberOfSortParam(string[] headlines)
        {

            for (int i = 0; i < headlines.Length; i++)
            {
                Console.WriteLine($"{i + 1}) {headlines[i]}");
            }

            int numberParamForSort = -1;
            bool isParamSet = false;

            while (!isParamSet)
            {
                bool isMessageWrited = false;
                try
                {
                    numberParamForSort = Int32.Parse(Console.ReadLine());
                    isParamSet = true;
                }
                catch
                {
                    Console.WriteLine("Вы ввели не число");
                    isMessageWrited = true;
                }
                isParamSet &= numberParamForSort > 0 && numberParamForSort <= headlines.Length;
                if (!isParamSet && !isMessageWrited) Console.WriteLine("Такого числа нет");
            }
            return numberParamForSort - 1;
        }
    }
}
