using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using ProtoBuf;

namespace Sentinel.Support
{
    public class ProtoHelper
    {
        public static void Serialize<T>(T objectToSerialize, string fullFilename)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                Serializer.Serialize(ms, objectToSerialize);

                ms.Position = 0;
                long size = ms.Length;

                var fi = new FileInfo(fullFilename);
                using (var fs = fi.Open(FileMode.Create, FileAccess.Write))
                {
                    ms.CopyTo(fs);
                    Trace.WriteLine(
                        string.Format(
                            "Wrote {0} consisting of {1} bytes of data to {2}",
                            objectToSerialize,
                            size,
                            fullFilename));
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine("Exception caught in proto-saving:");
                Trace.WriteLine(e.Message);
                throw;
            }
        }

        public static T Deserialize<T>(string fullFilename)
        {
            var fi = new FileInfo(fullFilename);
            if (fi.Exists)
            {
                using (var fs = fi.OpenRead())
                {
                    try
                    {
                        T obj = Serializer.Deserialize<T>(fs);
                        Debug.WriteLine("Loaded {0} with settings in {1}",
                                        obj.GetType().FullName,
                                        fullFilename);
                        return obj;
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(string.Format("Exception when trying to de-serialize from {0}", fullFilename));
                        Trace.WriteLine(e.Message);
                    }
                }                
            }

            return default(T);
        }

        public static bool Wrap(MemoryStream ms, object obj)
        {
            try
            {
                Serializer.Serialize(ms, obj.GetType().FullName);
                Serializer.Serialize(ms, obj);
                return true;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw;
            }
        }

        public static bool Unwrap(MemoryStream ms, out object deserializedObject)
        {
            if (ms == null) throw new ArgumentNullException("ms");

            // Get the type
            string typeName = Serializer.Deserialize<string>(ms);
            Type type = Type.GetType(typeName);

            MethodInfo mi = typeof(Serializer).GetMethod("Deserialize");
            MethodInfo miConstructed = mi.MakeGenericMethod(type);
            object[] args = { ms };
            deserializedObject = miConstructed.Invoke(null, args);

            return true;
        }
    }
}
