using System.Diagnostics;
using System.Windows;
using WpfExtras;

namespace Sentinel.Logs.Gui
{
    public class NewLoggerWizard
    {
        public NewLoggerWizard()
        {
            Settings = new NewLoggerSettings();
        }

        public bool Display(Window parent)
        {
            Wizard wizard = new Wizard
                                {
                                    Owner = parent,
                                    //ShowNavigationTree = false,
                                    Title = "Sentinel - Add new logger",
                                    SavedData = Settings
                                };

            wizard.AddPage(new AddNewLoggerWelcomePage());
            wizard.AddPage(new SetLoggerNamePage());
            wizard.AddPage(new ProvidersPage());
            wizard.AddPage(new ViewSelectionPage());
            // wizard.AddPage(new NewLoggerSummaryPage());

            bool? dialogResult = wizard.ShowDialog();
            if (dialogResult == true)
            {
                Settings = wizard.SavedData as NewLoggerSettings;
                Debug.Assert(Settings != null, "Settings should be non-null and of NewLoggerSettings type");
            }

            return dialogResult ?? false;
        }

        public NewLoggerSettings Settings { get; private set; }
    }
}
