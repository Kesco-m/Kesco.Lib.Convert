using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Kesco.Lib.ConvertExtention
{
    /// <summary>
    ///     Класс реализующий методы конвертации и приведения
    /// </summary>
    public class Convert
    {

        /// <summary>
        /// Преобразование версии библиотеки к int
        /// </summary>
        /// <param name="version">Номер версии</param>
        /// <returns></returns>
        public static int VersionToInt(Version version)
        {
            int versionInteger =  int.Parse(version.Major.ToString().PadRight(6,'0'))
                                + int.Parse(version.Minor.ToString().PadRight(6, '0'))
                                + int.Parse(version.Build.ToString().PadRight(6, '0')) 
                                + version.Revision;

            return versionInteger;
        }


        /// <summary>
        ///     Шаблон регулярного выражения "[A-ZА-Я0-9_]+" - используется для проверки ключа в коллекции
        /// </summary>
        public static Regex regex4KeysCollection = new Regex("[A-ZА-Я0-9_]+", RegexOptions.IgnoreCase);

        /// <summary>
        ///     Шаблон регулярного выражения "-?\\d+" - только числа
        /// </summary>
        public static Regex regex4IntCollection = new Regex("-?\\d+", RegexOptions.IgnoreCase);

        /// <summary>
        ///     Шаблон регулярного выражения "-?\\d+" - используется для проверки значений в коллекции при приведении к строке
        /// </summary>
        public static Regex regex4DefaultCollection =
            new Regex("-?\\d+|[_a-z0-9]+|[_а-я0-9]+", RegexOptions.IgnoreCase);

        /// <summary>
        ///     Формат преобразования чисел к строковому представлению, приемлемому в SQL операторах
        /// </summary>
        private static readonly NumberFormatInfo sqlLiteralDecimalFormat;

        static Convert()
        {
            sqlLiteralDecimalFormat = (NumberFormatInfo) NumberFormatInfo.InvariantInfo.Clone();
            sqlLiteralDecimalFormat.NumberDecimalSeparator = ".";
            sqlLiteralDecimalFormat.NumberGroupSeparator = "";
        }

        /// <summary>
        ///     Свойство возвращающее формат преобразования чисел к строковому представлению, приемлемому в SQL операторах
        /// </summary>
        public static NumberFormatInfo SqlLiteralDecimalFormat => (NumberFormatInfo) sqlLiteralDecimalFormat.Clone();

        /// <summary>
        ///     Преобразование коллекции к строке с разделителем ','
        /// </summary>
        /// <param name="col">Коллекция строк</param>
        /// <returns>Представление коллекции ввиде строки</returns>
        public static string Collection2Str(StringCollection col)
        {
            var b = new StringBuilder();
            for (var i = 0; i < col.Count; i++)
            {
                if (i > 0) b.Append(",");
                b.Append(col[i]);
            }

            return b.ToString();
        }


        /// <summary>
        ///     Преобразование строки к int nullable
        /// </summary>
        public static int? Str2IntNullable(string val)
        {
            int i;
            if (int.TryParse(val, out i)) return i;
            return null;
        }

        /// <summary>
        ///     Преобразование коллекции к строке с разделителем ','
        /// </summary>
        public static string Collection2Str(IEnumerable<string> col)
        {
            if (col != null)
            {
                var b = new StringBuilder();
                var counter = 0;
                foreach (var c in col)
                {
                    if (counter > 0) b.Append(",");
                    b.Append(c);

                    counter++;
                }

                return b.ToString();
            }

            return "";
        }

        /// <summary>
        ///     Преобразование строки в коллекцию строк, в качестве разделителя используется ','
        /// </summary>
        /// <param name="val">Строка, которую необходимо преобразовать</param>
        /// <returns>Коллекция строк</returns>
        public static StringCollection Str2Collection(string val)
        {
            return Str2Collection(val, regex4DefaultCollection);
        }

        /// <summary>
        ///     Преобразование строки в коллекцию строк, в качестве разделителя используется переданный шаблон
        /// </summary>
        /// <param name="val">Строка, которую необходимо преобразовать</param>
        /// <param name="regex">Регулярное выражение</param>
        /// <returns>Коллекция строк</returns>
        public static StringCollection Str2Collection(string val, Regex regex)
        {
            var col = new StringCollection();
            var m = regex.Matches(val);

            for (var i = 0; i < m.Count; i++)
            {
                if (col.Contains(m[i].Value)) continue;
                col.Add(m[i].Value);
            }

            return col;
        }


        /// <summary>
        ///     Преобразует секунды в строку формата HH:MM:SS
        /// </summary>
        /// <param name="s">Количество секунд</param>
        /// <returns>Представление целого в указанном формате времени</returns>
        public static string Second2TimeFormat(int s)
        {
            var hours = s / 3600;
            var mins = s % 3600 / 60;
            var secs = s % 60;
            return hours + ":" + mins.ToString("d2") + ":" + secs.ToString("d2");
        }

        /// <summary>
        ///     Преобразование числа типа Decimal к строке, с точностью 0
        /// </summary>
        /// <param name="val">Число</param>
        /// <returns>Полученное строковое значение</returns>
        private static string Decimal2Str(decimal val)
        {
            return Decimal2Str(val, 0);
        }

        /// <summary>
        ///     Преобразование числа типа Decimal к строке, с указанной точностью
        /// </summary>
        /// <param name="val">Число</param>
        /// <param name="scale">Точность</param>
        /// <param name="sqlFormat">Формат</param>
        /// <returns>Полученное строковое значение</returns>
        public static string Decimal2Str(decimal val, int scale, bool sqlFormat = true)
        {
            if (scale < 0) scale = 0;
            var format = sqlFormat ? sqlLiteralDecimalFormat : NumberFormatInfo.CurrentInfo;
            var maxScaleDecimal = val.ToString("", format);
            var userScaleDecimal = val.ToString(scale > 0 ? "N" + scale : "", format);

            return DecimalStr2Str(maxScaleDecimal.Length > userScaleDecimal.Length
                ? maxScaleDecimal
                : userScaleDecimal);
        }

        /// <summary>
        ///     Преобразование числа типа Decimal к строке, с указанной точностью
        /// </summary>
        /// <param name="val">Число</param>
        /// <param name="scale">Точность</param>
        /// <returns>Полученное строковое значение</returns>
        public static string Decimal2StrInit(decimal? val, int scale)
        {
            var summ = val ?? 0;
            if (Math.Round(summ, scale) == val) return summ.ToString(scale > 0 ? "N" + scale : "");
            var s = summ.ToString("G29");
            return s;
        }

        /// <summary>
        ///     Вспомогательная функция преобразования числа в строку, проверяет строку на соответствие шаблону
        /// </summary>
        /// <param name="s">Число преобразованное в строку</param>
        /// <returns>Полученное строковое значение</returns>
        private static string DecimalStr2Str(string s)
        {
            var m = Regex.Match(s, "[.]\\d*(0+)$", RegexOptions.RightToLeft);
            if (m.Success) s = s.Remove(m.Groups[1].Index, m.Groups[1].Length);
            m = Regex.Match(s, "[.]$");
            if (m.Success) s = s.Remove(m.Index, 1);
            m = Regex.Match(s, "^-?(0+)\\d+");
            if (m.Success) s = s.Remove(m.Groups[1].Index, m.Groups[1].Length);
            return s;
        }

        /// <summary>
        ///     Преобразование строки к Decimal на основе настроек корпоративной культуры
        /// </summary>
        /// <param name="val">Строка, которую необходимо преобразовать</param>
        /// <param name="defaultValue">Значение, которое вернет функция, если преобразование закончится неудачно</param>
        /// <returns>Полученное число</returns>
        public static decimal Str2Decimal(string val, decimal defaultValue)
        {
            if (string.IsNullOrEmpty(val)) return defaultValue;
            return Str2Decimal(val);
        }

        /// <summary>
        ///     Преобразование строки к Decimal на основе настроек корпоративной культуры
        /// </summary>
        /// <param name="val">Строка, которую необходимо преобразовать</param>
        /// <returns>Полученное число</returns>
        public static decimal Str2Decimal(string val)
        {
            var cultureInfo = string.Format(
                @"Thread.CurrentThread.CurrentCulture = {0},
Thread.CurrentThread.CurrentUICulture = {1},
NumberFormatInfo.CurrentInfo.NumberDecimalSeparator = {2},
NumberFormatInfo.CurrentInfo.NumberGroupSeparator = {3},
NumberFormatInfo.InvariantInfo.NumberDecimalSeparator = {4},
NumberFormatInfo.InvariantInfo.NumberGroupSeparator = {5}",
                Thread.CurrentThread.CurrentCulture.Name,
                Thread.CurrentThread.CurrentUICulture.Name,
                NumberFormatInfo.CurrentInfo.NumberDecimalSeparator,
                NumberFormatInfo.CurrentInfo.NumberGroupSeparator,
                NumberFormatInfo.InvariantInfo.NumberDecimalSeparator,
                NumberFormatInfo.InvariantInfo.NumberGroupSeparator
            );

            try
            {
                val = val.Replace(NumberFormatInfo.CurrentInfo.NumberGroupSeparator, " ");
                val = val.Replace(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, ".");

                var nfi = (NumberFormatInfo) NumberFormatInfo.InvariantInfo.Clone();
                nfi.NumberDecimalSeparator = ".";
                nfi.NumberGroupSeparator = " ";

                double dbl;
                if (double.TryParse(val, NumberStyles.Any, nfi, out dbl)) return (decimal) dbl;

                nfi = (NumberFormatInfo) NumberFormatInfo.CurrentInfo.Clone();
                nfi.NumberDecimalSeparator = ".";
                return System.Convert.ToDecimal(val.Replace(" ", ""), nfi);
            }
            catch (Exception)
            {
                throw new Exception("Невозможно преобразовать строку '" + val + "' к типу Decimal. cultureInfo:" +
                                    cultureInfo);
            }
        }

        /// <summary>
        ///     Преобразование даты и времени к строке в формате ISO
        /// </summary>
        /// <param name="val">Объект Дата/время</param>
        /// <returns>Полученное строковое значение</returns>
        public static string DateTime2Str(DateTime val)
        {
            return Regex.Replace(val.ToString("yyyyMMddHHmmss"), "000000$", "");
        }

        /// <summary>
        ///     Преобразование строки(в формате ISO) к объекту типа Datetime
        /// </summary>
        /// <param name="val">Строка, которую необходимо преобразовать</param>
        /// <param name="defaultValue">Значение, которое вернет функция, если преобразование закончится неудачно</param>
        /// <returns>Полученный объет типа Datetime</returns>
        public static DateTime Str2DateTime(string val, DateTime defaultValue)
        {
            if (string.IsNullOrEmpty(val)) return defaultValue;
            return Str2DateTime(val);
        }

        /// <summary>
        ///     Преобразование строки(в формате ISO) к объекту типа Datetime
        /// </summary>
        /// <param name="val">Строка, которую необходимо преобразовать</param>
        /// <returns>Полученный объект типа Datetime</returns>
        public static DateTime Str2DateTime(string val)
        {
            if (val.IndexOf(":") > 0 && val.Length == 8)
                return DateTime.Parse(DateTime.MinValue.ToString("yyyy.MM.dd") + " " + val);

            return DateTime.ParseExact(val.PadRight(14, '0'), "yyyyMMddHHmmss",
                CultureInfo.InvariantCulture);
        }

        private static string Double2Str(double val)
        {
            return Double2Str(val, 0);
        }

        /// <summary>
        /// </summary>
        /// <param name="val"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static string Double2Str(double val, int scale)
        {
            if (scale < 0) scale = 0;

            var maxScaleDouble = val.ToString("", sqlLiteralDecimalFormat);
            var userScaleDouble = val.ToString(scale > 0 ? "N" + scale : "", sqlLiteralDecimalFormat);

            return DecimalStr2Str(maxScaleDouble.Length > userScaleDouble.Length ? maxScaleDouble : userScaleDouble);
        }

        /// <summary>
        /// </summary>
        /// <param name="val"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static double Str2Double(string val, double defaultValue)
        {
            if (val == null || val.Length == 0) return defaultValue;
            return Str2Double(val);
        }

        /// <summary>
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static double Str2Double(string val)
        {
            return double.Parse(val.Replace(".", NumberFormatInfo.CurrentInfo.NumberDecimalSeparator));
        }

        /// <summary>
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string Bool2Str(bool val)
        {
            return val ? "1" : "0";
        }

        /// <summary>
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static byte Bool2Byte(bool val)
        {
            return (byte) (val ? 1 : 0);
        }

        /// <summary>
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string Object2Str(object val)
        {
            if (val is bool) return Bool2Str((bool) val);
            if (val is decimal) return Decimal2Str((decimal) val);
            if (val is double || val is float) return Double2Str((double) val);
            if (val is DateTime) return DateTime2Str((DateTime) val);
            if (val is StringCollection) return Collection2Str((StringCollection) val);
            return val.ToString();
        }

        #region Округление

        /// <summary>
        ///     Функция математического округления
        /// </summary>
        /// <param name="numToRound">Число, которое необходимо округлить</param>
        /// <param name="numOfDec">Точность, с которой необходимо выполнить округление</param>
        /// <returns>Полученный число типа float</returns>
        public static float Round(float numToRound, int numOfDec)
        {
            return (float) Round((decimal) numToRound, numOfDec);
        }

        /// <summary>
        ///     Функция математического округления
        /// </summary>
        /// <param name="numToRound">Число, которое необходимо округлить</param>
        /// <param name="numOfDec">Точность, с которой необходимо выполнить округление</param>
        /// <returns>Полученный число типа double</returns>
        public static double Round(double numToRound, int numOfDec)
        {
            return (double) Round((decimal) numToRound, numOfDec);
        }

        /// <summary>
        ///     Функция математического округления
        /// </summary>
        /// <param name="numToRound">Число, которое необходимо округлить</param>
        /// <param name="numOfDec">Точность, с которой необходимо выполнить округление</param>
        /// <returns>Полученный число типа decimal</returns>
        public static decimal Round(decimal numToRound, int numOfDec)
        {
            if (numOfDec < 0)
                throw new ArgumentException("Число десятичных знаков должно быть больше или равно 0");

            return decimal.Parse(numToRound.ToString("N" + numOfDec));
        }

        #endregion
    }
}