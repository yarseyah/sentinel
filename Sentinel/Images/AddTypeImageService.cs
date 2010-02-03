#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System.Collections.Generic;
using System.Windows;
using Sentinel.Controls;
using Sentinel.Services;
using Sentinel.Support;

#endregion

namespace Sentinel.Images
{

    #region Using directives

    #endregion

    public class AddTypeImageService
        : ViewModelBase, IAddTypeImageService
    {
        private readonly ITypeImageService imageService = ServiceLocator.Instance.Get<ITypeImageService>();

        private AddImageWindow addImageWindow;

        #region IAddTypeImageService Members

        public void Add()
        {
            if (addImageWindow != null)
            {
                MessageBox.Show(
                    "Only able to have one add image dialog open at a time!",
                    "Add image",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            else
            {
                addImageWindow = new AddImageWindow {DataContext = this, Owner = Application.Current.MainWindow};

                AddEditTypeImageViewModel data = new AddEditTypeImageViewModel(addImageWindow);

                bool? dialogResult = addImageWindow.ShowDialog();

                if (dialogResult == true)
                {
                    if (imageService != null)
                    {
                        imageService.ImageMappings.Add(
                            new KeyValuePair<string, string>(data.Type, data.FileName));
                    }
                }

                addImageWindow = null;
            }
        }

        #endregion
    }
}