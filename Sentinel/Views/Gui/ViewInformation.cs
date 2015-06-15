namespace Sentinel.Views.Gui
{
    using Sentinel.Views.Interfaces;

    public class ViewInformation : IViewInformation
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