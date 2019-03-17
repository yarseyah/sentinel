namespace Sentinel.Views.Heartbeat
{
    public class ViewInformation : Sentinel.Views.Interfaces.IViewInformation
    {
        public ViewInformation(string identifier, string name)
        {
            Identifier = identifier;
            Name = name;
        }

        public string Identifier { get; private set; }

        public string Name { get; private set; }

        public string Description { get; set; }
    }
}