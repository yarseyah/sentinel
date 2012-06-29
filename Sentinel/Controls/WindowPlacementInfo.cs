namespace Sentinel.Controls
{
    using System.Runtime.Serialization;
    using System.Windows;

    [DataContract]
    public class WindowPlacementInfo
    {
        [DataMember]
        public int Top { get; set; }

        [DataMember]
        public int Left { get; set; }

        [DataMember]
        public int Width { get; set; }

        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public WindowState WindowState { get; set; }
    }
}