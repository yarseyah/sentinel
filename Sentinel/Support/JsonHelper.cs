namespace Sentinel.Support
{
    using System;
    using System.IO;

    using Common.Logging;

    using Newtonsoft.Json;

    public static class JsonHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(JsonHelper));

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
                                                                      {
                                                                          TypeNameHandling = TypeNameHandling.All,
                                                                      };

        public static void SerializeToFile<T>(T objectToSerialize, string filename)
        {
            try
            {
                var objectString = JsonConvert.SerializeObject(objectToSerialize, Formatting.Indented, Settings);

                var fi = new FileInfo(filename);
                Stream fs = null;
                try
                {
                    fs = fi.Open(FileMode.Create, FileAccess.Write);
                    using (var sw = new StreamWriter(fs))
                    {
                        fs = null;
                        sw.Write(objectString);
                    }
                }
                finally
                {
                    fs?.Dispose();
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception caught in serialization:", e);
                throw;
            }
        }

        public static string SerializeToString<T>(T objectToSerialize)
        {
            try
            {
                return JsonConvert.SerializeObject(objectToSerialize, Formatting.Indented, Settings);
            }
            catch (Exception e)
            {
                Log.Error("Exception caught in serialization:", e);
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
                    Stream fs = null;
                    try
                    {
                        fs = fi.OpenRead();
                        using (var sr = new StreamReader(fs))
                        {
                            fs = null;
                            var asString = sr.ReadToEnd();
                            return JsonConvert.DeserializeObject<T>(asString, Settings);
                        }
                    }
                    finally
                    {
                        fs?.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"Exception when trying to de-serialize from {filename}", e);
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
                Log.Error("Exception when trying to de-serialize from given string", e);
            }

            return default(T);
        }
    }
}