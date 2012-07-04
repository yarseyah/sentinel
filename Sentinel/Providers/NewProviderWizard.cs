using System;
using System.Windows;
using Sentinel.Providers.Interfaces;
using WpfExtras;

namespace Sentinel.Providers
{
    using Sentinel.Interfaces.Providers;

    public class NewProviderWizard : INewProviderWizard
    {
        public bool Display(Window parent)
        {
            IProviderSettings settings = new ProviderSettings();

            // Construct the wizard
            Wizard wizard = new Wizard
                                {
                                    Owner = parent, 
                                    ShowNavigationTree = false,
                                    SavedData = settings,
                                    Title = "Add New Log Provider"
                                };

            wizard.AddPage(new SelectProviderPage());

            bool? dialogResult = wizard.ShowDialog();
            if ( dialogResult == true )
            {
                if (wizard.SavedData == null && !(wizard.SavedData is IProviderSettings))
                {
                    throw new NotImplementedException(
                        "The UserData was either null or the supplied object was not of the expected type: IProviderSettings");
                }

                Settings = (IProviderSettings) wizard.SavedData;
                Provider = Settings.Info;
            }

            return dialogResult ?? false;
        }

        public IProviderInfo Provider { get; private set; }

        public IProviderSettings Settings { get; private set; }
    }
}
