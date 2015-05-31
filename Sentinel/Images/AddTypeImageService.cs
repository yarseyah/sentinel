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
using Sentinel.Images.Controls;
using Sentinel.Images.Interfaces;
using Sentinel.Support.Mvvm;
using System;

#endregion

namespace Sentinel.Images
{
    public class AddTypeImageService : ViewModelBase, IAddTypeImage
    {
        private AddImageWindow addImageWindow;

        #region IAddTypeImage Members

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
                addImageWindow = new AddImageWindow { DataContext = this, Owner = Application.Current.MainWindow };

                var imageService = Services.ServiceLocator.Instance.Get<ITypeImageService>();
                var data = new AddEditTypeImageViewModel(addImageWindow, imageService, true);
                var dialogResult = addImageWindow.ShowDialog();

                if (dialogResult == true)
                {
                    if (imageService != null)
                    {
                        var imageSize = (ImageQuality)Enum.Parse(typeof(ImageQuality), data.Size);
                        imageService.Register(data.Type, imageSize, data.FileName);
                    }
                }

                addImageWindow = null;
            }
        }

        #endregion
    }
}