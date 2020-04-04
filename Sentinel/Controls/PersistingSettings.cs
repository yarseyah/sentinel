namespace Sentinel.Controls
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using Sentinel.Interfaces;
    using Sentinel.Support;

    [DataContract]
    public class PersistingSettings
    {
        [DataMember]
        public WindowPlacementInfo WindowPlacementInfo { get; set; }

        [DataMember]
        public IUserPreferences UserPreferences { get; set; }

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

                switch (fileHeader)
                {
                    case "{\"$type\":\"Sentinel.Controls.WindowPlacementInfo":
                        return DeserializeFromV1(fileContents);
                    case "{\"$type\":\"Sentinel.Controls.PersistingSettings,":
                        return DeserializeFromV2(fileContents);
                    default:
                        return null;
                }
            }

            return null;
        }

        internal static void Save(string fileName, WindowPlacementInfo placementInfo, IUserPreferences userPreferences)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(fileName));
            }

            var wrapper = new PersistingSettings
            {
                WindowPlacementInfo = placementInfo,
                UserPreferences = userPreferences,
            };

            try
            {
                JsonHelper.SerializeToFile(wrapper, fileName);
            }
            catch (Exception e)
            {
                Trace.TraceError($"Unable to write persistence file: {e.Message}");
            }
        }

        private static PersistingSettings DeserializeFromV1(string fileContents)
        {
            Trace.WriteLine("DeserializeFromV1");

            PersistingSettings wrapper = null;

            try
            {
                wrapper = new PersistingSettings
                {
                    WindowPlacementInfo = JsonHelper.DeserializeFromString<WindowPlacementInfo>(fileContents),
                };
            }
            catch (Exception e)
            {
                Trace.TraceError($"Deserialize error: {e.Message}");
            }

            return wrapper;
        }

        private static PersistingSettings DeserializeFromV2(string fileContents)
        {
            Trace.WriteLine("DeserializeFromV1");

            try
            {
                return JsonHelper.DeserializeFromString<PersistingSettings>(fileContents);
            }
            catch (Exception e)
            {
                Trace.TraceError($"Deserialize error: {e.Message}");
                return null;
            }
        }
    }
}