namespace Sentinel.Controls
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class RecentFileInfo
    {
        [DataMember]
        public IEnumerable<string> RecentFilePaths { get; set; }
    }
}
