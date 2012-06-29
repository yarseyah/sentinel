namespace Sentinel.Support
{
    using System;
    using System.Diagnostics;
    using System.IO;

    using Newtonsoft.Json;

    public static class JsonHelper
    {
        public static void SerializeToFile<T>(T objectToSerialize, string filename)
        {
            try
            {
                var objectString = JsonConvert.SerializeObject(objectToSerialize, Formatting.Indented);

                var fi = new FileInfo(filename);
                using (var fs = fi.Open(FileMode.Create, FileAccess.Write))
                {
                    using (var sw = new StreamWriter(fs))
                    {
                        sw.Write(objectString);
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine("Exception caught in serialization:");
                Trace.WriteLine(e.Message);
                throw;
            }            
        }

        public static T DeserializeFromFile<T>(string filename)
        {
            try
            {
                var fi = new FileInfo(filename);

                if (fi.Exists)
                {
                    using (var fs = fi.OpenRead())
                    {
                        using (var sr = new StreamReader(fs))
                        {
                            var asString = sr.ReadToEnd();
                            return JsonConvert.DeserializeObject<T>(asString);
                        }
                    }
                }
                
            }
            catch (Exception e)
            {
                Trace.WriteLine(string.Format("Exception when trying to de-serialize from {0}", filename));
                Trace.WriteLine(e.Message);
            }

            return default(T);
        }
    }
}