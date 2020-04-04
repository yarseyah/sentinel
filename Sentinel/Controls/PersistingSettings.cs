namespace Sentinel.Controls
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using Sentinel.Support;

    [DataContract]
    public class PersistingSettings
    {
        internal static PersistingSettings Load(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new System.ArgumentException("Value cannot be null or whitespace.", nameof(fileName));
            }

            // Note that PersistingSettings is a new file format, so need to detect whether using
            // new serialisation or upgrading.
            if (System.IO.File.Exists(fileName))
            {
                var fileContents = System.IO.File.ReadAllText(fileName);

                // Version detect from file-header signature:
                var fileHeader = fileContents
                    .Substring(0, 52)
                    .Replace(" ", string.Empty)
                    .Replace("\r", string.Empty)
                    .Replace("\n", string.Empty);

                var version = -1;
                switch (fileHeader)
                {
                    case "{\"$type\":\"Sentinel.Controls.WindowPlacementInfo":
                        return DeserializeFromV1(fileContents);
                    case "version2":
                        version = 2;
                        break;
                    default:
                        version = -1;
                        break;
                }
            }

            return null;
        }

        private static PersistingSettings DeserializeFromV1(string fileContents)
        {
            Trace.WriteLine("DeserializeFromV1");

            PersistingSettings wrapper = null;

            try
            {
                wrapper = new PersistingSettings
                {
                    WindowPlacementInfo = JsonHelper.DeserializeFromString<WindowPlacementInfo>(fileContents)
                };
            }
            catch (Exception e)
            {
                Trace.TraceError($"Deserialize error: {e.Message}");
            }

            return wrapper;
        }

        [DataMember]
        public WindowPlacementInfo WindowPlacementInfo { get; set; }
    }
}