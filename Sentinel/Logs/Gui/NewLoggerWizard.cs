namespace Sentinel.Logs.Gui
{
    using System.Diagnostics;
    using System.Windows;

    using WpfExtras;

    public class NewLoggerWizard
    {
        public NewLoggerWizard()
        {
            Settings = new NewLoggerSettings();
        }

        public NewLoggerSettings Settings { get; private set; }

        public bool Display(Window parent)
        {
            var wizard = new Wizard
                             {
                                 Owner = parent,
                                 ShowNavigationTree = false,
                                 Title = "Sentinel - Add new logger",
                                 SavedData = Settings,
                             };

            wizard.AddPage(new AddNewLoggerWelcomePage());
            wizard.AddPage(new SetLoggerNamePage());
            wizard.AddPage(new ProvidersPage());
            wizard.AddPage(new ViewSelectionPage());

            var dialogResult = wizard.ShowDialog();
            if (dialogResult == true)
            {
                Settings = wizard.SavedData as NewLoggerSettings;
                Debug.Assert(Settings != null, "Settings should be non-null and of NewLoggerSettings type");
            }

            return dialogResult ?? false;
        }
    }
}
