﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDCM.Data.Base.Utils
{
    public class StringUtil
    {
        public static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}
