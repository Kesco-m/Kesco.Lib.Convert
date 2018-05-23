using System;
using System.Text.RegularExpressions;

namespace Kesco.Lib.ConvertExtention
{
    /// <summary>
    /// Класс парсер строки с датой
    /// </summary>
    public class DateParser
    {
        /// <summary>
        /// Парсер
        /// </summary>
        /// <param name="date">Дата в строковом виде</param>
        /// <returns>Дата</returns>
        public static DateTime Parse(string date)
        {
            int k1 = 0; //кол-во попавших на своё место разделителей

            DateTime ret = DateTime.Now;
            date = Regex.Replace(date, "\\D", ".");
            date = Regex.Replace(date, "[.]{1,}", ".");
            date = Regex.Replace(date, "^[.]|[.]$", "");

            DateTime foundDate = DateTime.Today;
            int foundPriority = int.MinValue;

            int s0 = date.IndexOf('.');
            int s1 = s0 > 0 ? date.IndexOf('.', s0 + 1) - 1 : -1;

            string d = date.Replace(".", "");
            if (d.Length == 0 || int.Parse(d) == 0) throw new Exception("Дата не введена");

            string d0;
            string[] arr;
            int i0, i1, i2;
            for (int i = d.Length; i >= 0; i--)
                for (int j = d.Length; j >= 0; j--)
                {
                    d0 = d.Insert(Math.Max(i, j), ".").Insert(Math.Min(i, j), ".");

                    arr = d0.Split('.');
                    i0 = arr[0].Length == 0 ? 0 : int.Parse(arr[0]);
                    i1 = arr[1].Length == 0 ? 0 : int.Parse(arr[1]);
                    i2 = arr[2].Length == 0 ? 0 : int.Parse(arr[2]);

                    k1 = (i == s0 && i != d.Length && i != 0 ? 1 : 0) + (j == s1 && j != d.Length && j != 0 ? 1 : 0);

                    Check(ref foundDate, ref foundPriority, i0, i1, i2, k1, arr[0].Length, arr[1].Length, arr[2].Length, 6);
                    Check(ref foundDate, ref foundPriority, i1, i0, i2, k1, arr[1].Length, arr[0].Length, arr[2].Length, 5);
                    Check(ref foundDate, ref foundPriority, i2, i0, i1, k1, arr[2].Length, arr[1].Length, arr[0].Length, 4);
                    Check(ref foundDate, ref foundPriority, i2, i1, i0, k1, arr[2].Length, arr[0].Length, arr[1].Length, 3);
                    Check(ref foundDate, ref foundPriority, i1, i2, i0, k1, arr[1].Length, arr[2].Length, arr[0].Length, 2);
                    Check(ref foundDate, ref foundPriority, i0, i2, i1, k1, arr[0].Length, arr[2].Length, arr[1].Length, 1);
                }

            if (foundPriority == int.MinValue) throw new Exception("Неправильный формат даты");
            return foundDate;
        }

        /// <summary>
        /// Метод проверки и возврата даты в соответствии с форматом
        /// </summary>
        /// <param name="foundDate">Возвращаемая сформированная дата</param>
        /// <param name="foundPriority">Возвращаемая сформированный приоритет</param>
        /// <param name="d">Номер деня</param>
        /// <param name="m">Номер месяца</param>
        /// <param name="y">Номер года</param>
        /// <param name="k1">Коэф. зависимости от кол-ва попавших на своё место разделителей</param>
        /// <param name="dl">Длина дня в дате</param>
        /// <param name="ml">Длина месяца в дате</param>
        /// <param name="yl">Длина года в дате</param>
        /// <param name="k5">Коэф. зависимости от взаимного расположения dd MM yyyy</param>
        static void Check(ref DateTime foundDate, ref int foundPriority, int d, int m, int y, int k1, int dl, int ml, int yl, int k5)
        {
            //k1 коэф. зависимости от кол-ва попавших на своё место разделителей
            int k2 = (dl == 1 || dl == 2) ? dl : 0;	//коэф. зависимости от кол-ва цифр в дне
            int k3 = (ml == 1 || ml == 2) ? ml : 0; //коэф. зависимости от кол-ва цифр в месяце
            int k4 = (yl == 1 || yl == 2 || yl == 4) ? yl : 0; //коэф. зависимости от кол-ва цифр в году
            //k5 коэф. зависимости от взаимного расположения dd MM yyyy

            DateTime dt, d0 = DateTime.Today;
            if (d == 0) d = d0.Day;
            if (m == 0) m = d0.Month;
            if (y == 0) y = d0.Year;
            if (y < 100) y = (y < 50 ? 2000 : 1900) + y;
            if (y > 2050 || y < 1950 || m > 12 || d > 31) return;

            try
            {
                dt = new DateTime(y, m, d);
            }
            catch
            {
                return;
            }

            if (!(dt.Day == d && dt.Month == m && dt.Year == y && dt.Year > 1950 && dt.Year < 2050)) return;

            int priority = k1 * 1000 + k5 * 50 + k2 * 100 + k3 * 30 + k4 * 10;

            if (foundPriority < priority) { foundPriority = priority; foundDate = dt; }
        }
    }
}
