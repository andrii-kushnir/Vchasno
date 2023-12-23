using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Vchasno
{
    public static class SerializeToJson
    {
        public static T ConvertJson<T>(this string json, ref string error)
        {
            T result;
            try
            {
                result = JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return default(T);
            }
            return result;
        }
    }
}

