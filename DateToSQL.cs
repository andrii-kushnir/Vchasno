﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vchasno
{
    public static class ExtensionSQL
    {
        public static string DateToSQL(this DateTime date)
        {
            var dateBegin = new DateTime(1970, 1, 1);
            date = date < dateBegin ? dateBegin : date;
            var result = date.ToString("yyyy-MM-dd HH:mm:ss");
            return result;
        }

        public static int StringToInt(this String data)
        {
            if (data == null) return 0;
            if (Int32.TryParse(data, out int numeric))
                return numeric;
            else
                return 0;
        }

        public static string NullCheck(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return "0";
            return value;
        }

    }
}
