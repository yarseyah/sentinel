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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Sentinel.Images.Interfaces;
using Sentinel.Support.Mvvm;

#endregion

namespace Sentinel.Images
{
    [Export(typeof(ITypeImageService))]
    public class TypeToImageService
        : ViewModelBase
        , ITypeImageService
    {
        private int selectedIndex;

        [Import(typeof(IAddTypeImage))]
        private IAddTypeImage AddImage { get; set; }

        [Import(typeof(IEditTypeImage))]
        private IEditTypeImage EditImage { get; set; }

        [Import(typeof(IRemoveTypeImage))]
        private IRemoveTypeImage RemoveImage { get; set; }

        public TypeToImageService()
        {
            ImageMappings = new ObservableCollection<KeyValuePair<string, string>>
                                {
                                    new KeyValuePair<string, string>("ERROR", "/Resources/Error.png"),
                                    new KeyValuePair<string, string>("WARN", "/Resources/Warning.png"),
                                    new KeyValuePair<string, string>("INFO", "/Resources/Info.png"),
                                    new KeyValuePair<string, string>("FATAL", "/Resources/Fatal.png"),
                                    new KeyValuePair<string, string>("DEBUG", "/Resources/Debug.png"),
                                    new KeyValuePair<string, string>("TRACE", "/Resources/Trace.png")
                                };

            Add = new DelegateCommand(AddMapping);
            Edit = new DelegateCommand(EditMapping, e => selectedIndex != -1);
            Remove = new DelegateCommand(RemoveMapping, e => selectedIndex != -1);
        }

        #region ITypeImageService Members

        public ICommand Add { get; private set; }

        public ICommand Edit { get; private set; }

        public ObservableCollection<KeyValuePair<string, string>> ImageMappings { get; private set; }

        public ICommand Remove { get; private set; }

        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }

            set
            {
                if (value != selectedIndex)
                {
                    selectedIndex = value;
                    OnPropertyChanged("SelectedIndex");
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public void Register(string type, string image)
        {
            // See if already there....
            string typeUpper = type.ToUpper();
            bool found = ImageMappings.Any(i => i.Key.ToUpper() == typeUpper);

            KeyValuePair<string, string> record = new KeyValuePair<string, string>(typeUpper, image);

            if (found)
            {
                lock (ImageMappings)
                {
                    for (int i = 0; i < ImageMappings.Count; i++)
                    {
                        if (ImageMappings[i].Key == typeUpper)
                        {
                            ImageMappings[i] = record;
                        }
                    }
                }
            }
            else
            {
                ImageMappings.Add(record);
            }

            OnPropertyChanged("ImageMappings");
        }

        #endregion

        private void AddMapping(object obj)
        {
            AddImage.Add();
        }

        private void EditMapping(object obj)
        {
        }

        private void RemoveMapping(object obj)
        {
        }
    }
}