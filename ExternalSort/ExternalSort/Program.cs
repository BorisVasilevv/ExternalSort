using Microsoft.Office.Interop.Excel;
using System;
using System.Text;

namespace ExternalSort
{
    
    class Program
    {
        static string filepath = "..\\..\\..\\..\\Merge.txt";


        public static double[] arr = new double[8000];
        public static void Main(string[] args)
        {
            FileWorker.ChoseFile();
            string[] headlines;
            string[,] list;
            (headlines, list) = FileWorker.ReadFile();

            int typeOfSort = Menu.GetNumberOfAnswer("Выберите тип внешней сортировки",
                new string[] { "Прямая", "Естественная", "Многопутевая" });

            
            if (headlines.Length == 1)
            {                
                string[] strs = new string[list.GetLength(1)];
                for (int i = 0; i < list.GetLength(1); i++)
                {
                    strs[i] = list[0, i];
                }
                FileWorker.FileToSortGeneration(filepath, strs);
                if (typeOfSort == 0)
                {
                    DirectMerge directMerge = new DirectMerge(filepath);
                    strs = directMerge.Sort(strs);
                }
                else if (typeOfSort == 1)
                {
                    NaturalMerge naturalMerge = new NaturalMerge(filepath); 
                    strs = naturalMerge.Sort(strs);
                }
                else
                {
                    MultipathMerge multipathMerge = new MultipathMerge(filepath);
                    strs = multipathMerge.Sort(strs);
                }
             
                string[] strings = new string[strs.Length + 1];
                strings[0] = headlines[0];
                for (int i = 1; i <= list.GetLength(1); i++)
                    strings[i] = strs[i - 1];

                FileWorker.WriteToFile(strings);
                return;
            }

            List<string> variantes =new List<string>();
            int numberOfColumn = Menu.GetNumberOfAnswer("Выберите колонку, в которой будет задан элемент сортировки  (напишите цифру)",
                headlines);
            for (int i = 0; i < list.GetLength(1); i++)
            {
                string temporaryVar = list[numberOfColumn, i];
                if (!variantes.Contains(temporaryVar))
                    variantes.Add(temporaryVar);
            }

            Console.WriteLine("Выберите элемет из этой колонки по которому отсортировать (напишите цифру)");

            int numberParam = Menu.GetNumberOfAnswer("Выберите элемет из этой колонки по которому отсортировать (напишите цифру)",
                variantes.ToArray());
            string sortParam = variantes[numberParam];

            List<int> goodRecordIds = new List<int>();
            for (int i = 0; i < list.GetLength(1); i++)
            {
                if (sortParam == list[numberOfColumn, i])
                    goodRecordIds.Add(i);
            }


            string[,] newList = new string[list.GetLength(0), goodRecordIds.Count];
            int index = 0;
            for (int i = 0; i < list.GetLength(1); i++)
            {
                if (goodRecordIds.Contains(i))
                {
                    for (int j = 0; j < list.GetLength(0); j++)
                    {
                        newList[j, index] = list[j, i];
                    }
                    index++;
                }
            }

            Console.WriteLine("Выберите колонку, в которой будет производиться сортировка (напишите цифру)");

            int numberParamForSort = Menu.GetNumberOfAnswer("Выберите колонку, по которой будет производиться сортировка (напишите цифру)",
                headlines);
            List<double> listNumbers= new List<double>();
            bool isSortByNumber = true;
            for (int i = 0; i < newList.GetLength(1); i++)
            {

                try
                {
                    listNumbers.Add(Double.Parse(newList[numberParamForSort, i]));
                }
                catch (Exception ignoreException)
                {
                    isSortByNumber = false;
                    break;
                }

            }


            if (!isSortByNumber)
            {
                string[] arrayS = new string[newList.GetLength(1)];

                for (int i = 0; i < newList.GetLength(1); i++)
                    arrayS[i] = (new(newList[numberParamForSort, i]));
                FileWorker.FileToSortGeneration(filepath,arrayS);
                if (typeOfSort == 0)
                {
                    DirectMerge directMerge = new DirectMerge(filepath);
                    arrayS = directMerge.Sort(arrayS);
                }
                else if (typeOfSort == 1)
                {
                    NaturalMerge naturalMerge = new NaturalMerge(filepath);
                    arrayS = naturalMerge.Sort(arrayS);
                }
                else
                {
                    MultipathMerge multipathMerge = new MultipathMerge(filepath);
                    arrayS = multipathMerge.Sort(arrayS);
                }
                ChangePositionInArray(newList, arrayS, numberParamForSort);
            }
            else
            {
                double[] numbers = new double[listNumbers.Count()];
                for (int i = 0; i < listNumbers.Count(); i++)
                    numbers[i] = listNumbers[i];
                FileWorker.FileToSortGeneration(filepath, numbers);
                if (typeOfSort == 0)
                {
                    DirectMerge directMerge = new DirectMerge(filepath);
                    numbers = directMerge.Sort(numbers);
                }
                else if (typeOfSort == 1)
                {
                    NaturalMerge naturalMerge = new NaturalMerge(filepath);
                    numbers = naturalMerge.Sort(numbers);
                }
                else
                {
                    MultipathMerge multipathMerge = new MultipathMerge(filepath);
                    numbers = multipathMerge.Sort(numbers);
                }
                ChangePositionInArray(newList, numbers, numberParamForSort);
            }

            FileWorker.WriteToFile(headlines, newList);
            //






            //string file = "..\\..\\..\\..\\Merge.txt";
            //DirectMerge direct = new DirectMerge(file);
            //NaturalMerge natural = new NaturalMerge(file);
            //MultipathMerge multipath = new MultipathMerge(file);
            //double[] outd = direct.SortNumbers();
            //double[] outn=natural.SortNumbers();
            //double[] outm = multipath.SortNumbers();

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
    }
}
