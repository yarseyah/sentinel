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
using System.Runtime.Serialization;
using System.Windows.Input;

#endregion

namespace Sentinel.Images.Interfaces
{
    public interface ITypeImageService
    {
        ICommand Add { get; }

        ICommand Edit { get; }

        [DataMember]
        ObservableCollection<ImageTypeRecord> ImageMappings { get; }

        ICommand Remove { get; }

        int SelectedIndex { get; set; }

        void Register(string type, ImageQuality quality, string image);

        ImageTypeRecord Get(string type, ImageQuality quality = ImageQuality.BestAvailable, bool acceptLower = true, bool mustHaveImage = false);
    }
}