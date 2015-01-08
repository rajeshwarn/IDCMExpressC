﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Concurrent;
using IDCM.Data.Base;

namespace IDCM.Data.Base.Utils
{
    /// <summary>
    /// 用于DataGirdView显示时的字段名称映射
    /// </summary>
    [Serializable]
    public class ColumnMapping : ConcurrentDictionary<String, ObjectPair<int, int>>, ICloneable
    {
        public object Clone()
        {
            BinaryFormatter Formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.Clone));
            MemoryStream stream = new MemoryStream();
            Formatter.Serialize(stream, this);
            stream.Position = 0;
            object clonedObj = Formatter.Deserialize(stream);
            stream.Close();
            return clonedObj;
        }
    }
}