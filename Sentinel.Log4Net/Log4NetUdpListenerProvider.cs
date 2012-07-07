namespace Sentinel.Log4Net
{
    using System;

    using Sentinel.Interfaces.Providers;

    public class Log4NetUdpListenerProvider : IProviderInfo
    {
        public Guid Identifier
        {
            get
            {
                return new Guid("D19E8097-FC08-47AF-8418-F737168A9645");
            }
        }

        public string Name
        {
            get
            {
                return "Log4Net UdpAppender Provider";
            }
        }

        public string Description
        {
            get
            {
                return "Handler for the remote side of log4net's UdpAppender";
            }
        }
    }
}