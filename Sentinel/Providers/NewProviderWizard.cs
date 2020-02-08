namespace Sentinel.Providers
{
    using System;
    using System.Windows;

    using Sentinel.Interfaces.Providers;
    using Sentinel.Providers.Interfaces;

    using WpfExtras;

    public class NewProviderWizard : INewProviderWizard
    {
        public IProviderInfo Provider { get; private set; }

        public IProviderSettings Settings { get; private set; }

        public bool Display(Window parent)
        {
            IProviderSettings settings = new ProviderSettings();

            // Construct the wizard
            var wizard = new Wizard
                             {
                                 Owner = parent,
                                 ShowNavigationTree = false,
                                 SavedData = settings,
                                 Title = "Add New Log Provider",
                             };

            wizard.AddPage(new SelectProviderPage());

            var dialogResult = wizard.ShowDialog();
            if (dialogResult == true)
            {
                if (wizard.SavedData == null && !(wizard.SavedData is IProviderSettings))
                {
                    throw new NotImplementedException(
                        "The UserData was either null or the supplied object was not of the expected type: IProviderSettings");
                }

                Settings = (IProviderSettings)wizard.SavedData;
                Provider = Settings.Info;
            }

            return dialogResult ?? false;
        }
    }
}
