#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System;
using System.Windows;
using Sentinel.Interfaces;
using Sentinel.Services;

#endregion

namespace Sentinel.Controls
{
    /// <summary>
    /// Interaction logic for PreferencesWindow.xaml
    /// </summary>
    public partial class PreferencesWindow : Window
    {
        public PreferencesWindow()
        {
            InitializeComponent();
            Preferences = ServiceLocator.Instance.Get<IUserPreferences>();
            DataContext = this;
        }

        public IUserPreferences Preferences { get; private set; }

        private void Window_Closed(object sender, EventArgs e)
        {
            Preferences.Show = false;
        }
    }
}