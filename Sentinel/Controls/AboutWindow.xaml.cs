using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Sentinel.Controls
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow(Window parent)
        {
            InitializeComponent();

            this.Owner = parent;            

            var assembly = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);

            AssemblyNameLabel.Text = assembly.ProductName;
            VersionNumberLabel.Text = assembly.ProductVersion;
            DescriptionLabel.Text = assembly.Comments;
            DeveloperInfoLabel.Text = assembly.CompanyName;
            CopyrightInfoLabel.Text = assembly.LegalCopyright;
        }
    }
}
