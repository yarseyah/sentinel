using System.Windows;
using ProtoBuf;

namespace Sentinel.Controls
{
    [ProtoContract]
    public class WindowPlacementInfo
    {
        [ProtoMember(1)]
        public int Top { get; set; }

        [ProtoMember(2)]
        public int Left { get; set; }

        [ProtoMember(3)]
        public int Width { get; set; }

        [ProtoMember(4)]
        public int Height { get; set; }

        [ProtoMember(5)]
        public WindowState WindowState { get; set; }
    }
}