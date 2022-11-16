using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalSort
{
    class Menu
    {
        public static int GetNumberOfAnswer(string question,string[] answers)
        {
            Console.WriteLine(question);    
            for (int i = 0; i < answers.Length; i++)
            {
                Console.WriteLine($"{i + 1}) {answers[i]}");
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
                isParamSet &= numberParamForSort > 0 && numberParamForSort <= answers.Length;
                if (!isParamSet && !isMessageWrited) Console.WriteLine("Такого числа нет");
            }
            return numberParamForSort - 1;
        }

        public static bool LeftBeforeRight(string left, string right)
        {
            int length = left.Length > right.Length
                    ? right.Length
                    : left.Length;
            for (int i = 0; i < length; i++)
            {
                if (left.ToLower().ToCharArray()[i] <
                    right.ToLower().ToCharArray()[i]) return true;
                if (left.ToLower().ToCharArray()[i] >
                    right.ToLower().ToCharArray()[i]) return false;
            }
            return left.Length < right.Length;
        }

       
    }
}
