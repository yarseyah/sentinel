namespace Sentinel.Controls
{
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Windows;

    /// <summary>
    /// Interaction logic for AboutWindow.xaml.
    /// </summary>
    public partial class AboutWindow
    {
        public AboutWindow(Window parent)
        {
            InitializeComponent();

            Owner = parent;

            try
            {
                var assembly = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
                AssemblyNameLabel.Text = assembly.ProductName;
                VersionNumberLabel.Text = assembly.ProductVersion;
                DescriptionLabel.Text = assembly.Comments;
                DeveloperInfoLabel.Text = assembly.CompanyName;
                CopyrightInfoLabel.Text = assembly.LegalCopyright;
            }
            catch (FileNotFoundException)
            {
                // Can be thrown by the GetVersionInfo call
            }
        }
    }
}
