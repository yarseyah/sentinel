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
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Sentinel.Images.Interfaces;
using Sentinel.Support.Mvvm;

#endregion

namespace Sentinel.Images
{
    using System.Diagnostics;

    public enum ImageQuality
    {
        Small,
        Medium,
        Large,
        BestAvailable
    }

    public class TypeToImageService : ViewModelBase, ITypeImageService
    {
        private int selectedIndex;

        public TypeToImageService()
        {
            ImageMappings = new ObservableCollection<ImageTypeRecord>();

            // TODO: Register defaults, this should be persisting somewhere
            Register("ERROR", ImageQuality.Small, "/Resources/Small/Error.png");
            Register("WARN", ImageQuality.Small, "/Resources/Small/Warning.png");
            Register("INFO", ImageQuality.Small, "/Resources/Small/Info.png");
            Register("DEBUG", ImageQuality.Small, "/Resources/Small/Debug.png");
            Register("TRACE", ImageQuality.Small, "/Resources/Small/Trace.png");

            Register("FATAL", ImageQuality.Small, "/Resources/Small/Fatal.png");
            Register("FATAL", ImageQuality.Medium, "/Resources/Medium/Fatal.png");
            Register("FATAL", ImageQuality.Large, "/Resources/Large/Fatal.png");

            Add = new DelegateCommand(AddMapping);
            Edit = new DelegateCommand(EditMapping, e => selectedIndex != -1);
            Remove = new DelegateCommand(RemoveMapping, e => selectedIndex != -1);

            AddImage = new AddTypeImageService();
            EditImage = new EditTypeImageMapping();
            RemoveImage = new RemoveTypeImageMapping();
        }

        public ICommand Add { get; private set; }

        public ICommand Edit { get; private set; }

        public ObservableCollection<ImageTypeRecord> ImageMappings { get; private set; }

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

        private IAddTypeImage AddImage { get; set; }

        private IEditTypeImage EditImage { get; set; }

        private IRemoveTypeImage RemoveImage { get; set; }

        public void Register(string type, ImageQuality quality, string image)
        {
            Debug.Assert(quality != ImageQuality.BestAvailable, "Must use a specific size when registering");

            var typeName = type.ToUpper();
            var imageRecord = Get(typeName, quality, false);

            var updated = false;
            if (imageRecord != null)
            {
                if (imageRecord.Image != image)
                {
                    imageRecord.Image = image;
                    updated = true;
                }
            }
            else
            {
                ImageMappings.Add(new ImageTypeRecord(typeName, quality, image));
                updated = true;
            }

            if (updated)
            {
                OnPropertyChanged("ImageMappings");
            }
        }

        public ImageTypeRecord Get(string type, ImageQuality quality = ImageQuality.BestAvailable, bool acceptLower = true)
        {
            var typeName = type.ToUpper();
            var sorted = ImageMappings.Where(r => r.Name == typeName).OrderByDescending(r => r.Quality);

            if (quality == ImageQuality.BestAvailable)
            {
                return sorted.FirstOrDefault();
            }

            var exactMatch = sorted.SingleOrDefault(r => r.Quality == quality);
            if (exactMatch != null)
            {
                return exactMatch;
            }

            // Don't have explicit size or have not asked for best available.
            if (acceptLower)
            {
                Debug.Assert(quality != ImageQuality.BestAvailable, "Must be an explicit quality");
                var newQuality = quality == ImageQuality.Large ? ImageQuality.Medium : ImageQuality.Small;
                if (newQuality != quality)
                {
                    return Get(type, newQuality);
                }
            }

            return null;
        }

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