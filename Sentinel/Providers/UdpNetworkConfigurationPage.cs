namespace Sentinel.Providers
{
    public class UdpNetworkConfigurationPage : NetworkConfigurationPage
    {
        /// <summary>
        /// Overrides the base <c>SupportsTcp</c> method and return false.
        /// </summary>
        public override bool SupportsTcp
        {
            get
            {
                return false;
            }
        }
    }
}