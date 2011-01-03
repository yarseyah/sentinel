using ProtoBuf;

namespace Sentinel.Providers
{
    [ProtoContract]
    public class NetworkSettings : ProviderSettings
    {
        public NetworkSettings()
        {
            Port = 9999;
            IsUdp = true;
        }

        [ProtoMember(1)]
        public bool IsUdp
        {
            get;
            set;
        }

        [ProtoMember(2)]
        public int Port
        {
            get;
            set;
        }

        public override string Summary
        {
            get
            {
                return string.Format(
                    "Listens on {0} port {1}",
                    IsUdp ? "UDP" : "TCP",
                    Port);
            }
        }
    }
}