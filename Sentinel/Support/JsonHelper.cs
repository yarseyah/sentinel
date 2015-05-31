namespace Sentinel.Support
{
    using System;
    using System.Diagnostics;
    using System.IO;

    using Newtonsoft.Json;

    public static class JsonHelper
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
                                                                      {
                                                                          TypeNameHandling = TypeNameHandling.All
                                                                      };

        public static void SerializeToFile<T>(T objectToSerialize, string filename)
        {
            try
            {
                var objectString = JsonConvert.SerializeObject(objectToSerialize, Formatting.Indented, Settings);

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

        public static string SerializeToString<T>(T objectToSerilaize)
        {
            try
            {
                return JsonConvert.SerializeObject(objectToSerilaize, Formatting.Indented, Settings);
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
                            return JsonConvert.DeserializeObject<T>(asString, Settings);
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

        public static T DeserializeFromString<T>(string value)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(value, Settings);
            }
            catch (Exception e)
            {
                Trace.WriteLine("Exception when trying to de-serialize from given string");
                Trace.WriteLine(e.Message);
            }

            return default(T);
        }
    }
}