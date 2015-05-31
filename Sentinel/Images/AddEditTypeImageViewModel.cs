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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Sentinel.Images.Interfaces;
using Sentinel.Support.Mvvm;

#endregion

namespace Sentinel.Images
{
    public class AddEditTypeImageViewModel
        : ViewModelBase, IDataErrorInfo
    {
        private readonly Dictionary<ImageError, string> imageErrorMessages =
            new Dictionary<ImageError, string>
                {
                    {ImageError.NotSpecified, "No image selected."},
                    {ImageError.NotFound, "Image not found."},
                    {
                        ImageError.TooLarge,
                        "Image must be less than 128x128. [Note: images are most often used at 64x64]"
                        },
                    {ImageError.Unknown, "Problem with reading image."},
                    {ImageError.NoError, null}
                };

        private ITypeImageService ImageService { get; set; }

        private readonly Dictionary<TypeError, string> typeErrorMessages =
            new Dictionary<TypeError, string>
                {
                    {TypeError.NotSpecified, "Type name must be specified."},
                    {TypeError.Duplicate, "There is already an entry for this type name and size combination."},
                    {TypeError.NoError, null}
                };

        private readonly Window window;

        private string errorMessage = "No image selected.";

        private string fileName;

        private BitmapImage image;

        private ImageError imageError;

        private string type = string.Empty;

        private string size = "Small";

        private bool _isAddMode;

        private TypeError typeError;

        public AddEditTypeImageViewModel(Window window, ITypeImageService images, bool isAddMode)
        {            
            this.window = window;
            if (window != null)
            {
                window.Title = String.Format("{0} Image", (isAddMode ? "Edit" : "Add"));
            }
            this.window.DataContext = this;

            _isAddMode = isAddMode;
            imageError = isAddMode ? ImageError.NotSpecified : ImageError.NoError;
            typeError = isAddMode ? TypeError.NotSpecified : TypeError.NoError;

            Accept = new DelegateCommand(e => CloseDialog(true), c => IsValid);
            Reject = new DelegateCommand(e => CloseDialog(false));
            Browse = new DelegateCommand(BrowseForImageFiles);

            ImageService = images;

            UpdateErrorMessage(false);

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "FileName")
                {
                    LoadImageFromFileName(FileName);
                }
            };
        }

        /// <summary>
        /// Gets the <c>ICommand</c> corresponding to the Accept action.
        /// </summary>
        public ICommand Accept { get; private set; }

        /// <summary>
        /// Gets the <c>ICommand</c> corresponding to the Browse action.
        /// </summary>
        public ICommand Browse { get; private set; }

        /// <summary>
        /// Gets or sets the filename for the icon to use.
        /// </summary>
        public string FileName
        {
            get
            {
                return fileName;
            }

            set
            {
                if (fileName != value)
                {
                    fileName = value;
                    OnPropertyChanged("FileName");
                }
            }
        }

        /// <summary>
        /// Gets or sets the image to use as the type icon.
        /// </summary>
        public BitmapImage Image
        {
            get
            {
                return image;
            }

            set
            {
                if (image != value)
                {
                    image = value;
                    OnPropertyChanged("Image");
                }
            }
        }

        public bool IsValid
        {
            get
            {
                return typeError == TypeError.NoError && imageError == ImageError.NoError;
            }
        }

        public ICommand Reject { get; private set; }

        /// <summary>
        /// Gets or sets the type of a TypeImage.
        /// </summary>
        public string Type
        {
            get
            {
                return type;
            }

            set
            {
                if (type != value)
                {
                    type = value;
                    OnPropertyChanged("Type");
                }
            }
        }

        public string Size
        {
            get
            {
                return size;
            }

            set
            {
                if (size != value)
                {
                    size = value;
                    OnPropertyChanged("Size");
                }
            }
        }

        public bool IsAddMode
        {
            get
            {
                return _isAddMode;
            }
            set
            {
                if (_isAddMode != value)
                {
                    _isAddMode = value;
                    OnPropertyChanged("IsAddMode");
                }
            }
        }

        #region IDataErrorInfo Members

        /// <summary>
        /// Gets or sets an error message indicating what is wrong with this object.
        /// </summary>
        /// <returns>
        /// An error message indicating what is wrong with this object. The default is an empty string ("").
        /// </returns>
        public string Error
        {
            get
            {
                return errorMessage;
            }

            set
            {
                if (errorMessage != value)
                {
                    errorMessage = value;
                    OnPropertyChanged("Error");
                    OnPropertyChanged("IsValid");
                }
            }
        }

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <returns>
        /// The error message for the property. The default is an empty string ("").
        /// </returns>
        /// <param name="fieldName">The name of the property whose error message to get. 
        /// </param>
        public string this[string fieldName]
        {
            get
            {
                string error = null;

                if (_isAddMode && (fieldName == "Type" || fieldName == "Size"))
                {
                    var oldTypeError = typeError;

                    // TODO: need to be aware of the different qualities of images, using best available
                    ImageQuality selectedSize = (ImageQuality)Enum.Parse(typeof(ImageQuality), Size);
                    if (!string.IsNullOrEmpty(Type)
                        && ImageService != null
                        && ImageService.Get(Type, selectedSize, false, false) != null)
                    {
                        typeError = TypeError.Duplicate;
                    }
                    else if (fieldName == "Type" && string.IsNullOrEmpty(Type))
                    {
                        typeError = TypeError.NotSpecified;
                    }
                    else if (fieldName == "Type" || fieldName == "Size")
                    {
                        typeError = TypeError.NoError;
                    }

                    error = typeErrorMessages[typeError];

                    if (oldTypeError != typeError)
                    {
                        UpdateErrorMessage(false);
                    }
                }

                return error;
            }
        }

        #endregion

        private void BrowseForImageFiles(object obj)
        {
            var openFileDialog = new OpenFileDialog
                {
                    Title = "Select image",
                    ValidateNames = true,
                    CheckFileExists = true,
                    Multiselect = false,
                    Filter = "Images files|*.png;*.bmp|All Files|*.*",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
                };

            var dialogResult = openFileDialog.ShowDialog(window);
            

            if (dialogResult == true)
            {

                //LoadImageFromFileName(openFileDialog.FileName);

                // In all cases, keep the filename entered or it is confusing for the user.
                // Plus, we need to raise the PropertyChanged event for the FileName property.
                FileName = openFileDialog.FileName;
                
            }
        }

        private void LoadImageFromFileName(string fileName)
        {
            var oldImageError = imageError;

            try
            {
                // Check file exists, it should!
                var fi = new FileInfo(fileName);
                if (fi.Exists)
                {
                    var i = new BitmapImage();
                    i.BeginInit();
                    i.UriSource = new Uri(fileName, UriKind.RelativeOrAbsolute);
                    i.EndInit();

                    Image = i;

                    // Some santity checking.
                    this.imageError = i.Width <= 128 && i.Height <= 128 ? ImageError.NoError : ImageError.TooLarge;
                }
                else
                {
                    Image = null;
                    imageError = ImageError.NotFound;
                }
            }
            catch (Exception)
            {
                Image = null;
                imageError = ImageError.Unknown;
            }

            if (imageError != oldImageError)
            {
                UpdateErrorMessage(true);
            }
        }

        private void CloseDialog(bool dialogResult)
        {
            window.DialogResult = dialogResult;
            window.Close();
        }

        private void UpdateErrorMessage(bool imageChangedMostRecently)
        {
            var oldErrorMessage = Error;
            var imageErrorMessage = imageErrorMessages[imageError];
            var typeErrorMessage = typeErrorMessages[typeError];

            var newErrorMessage = imageChangedMostRecently
                                      ? (imageErrorMessage ?? typeErrorMessage)
                                      : (typeErrorMessage ?? imageErrorMessage);

            if (oldErrorMessage != newErrorMessage)
            {
                Error = newErrorMessage;
            }
        }

        #region Nested type: ImageError

        /// <summary>
        /// Error messages corresponding to the image field.
        /// </summary>
        private enum ImageError
        {
            /// <summary>
            /// No image yet specified.
            /// </summary>
            NotSpecified,

            /// <summary>
            /// Image can not be found.
            /// </summary>
            NotFound,

            /// <summary>
            /// Image is too large for the purposes.
            /// </summary>
            TooLarge,

            /// <summary>
            /// Some unknown error.
            /// </summary>
            Unknown,

            /// <summary>
            /// No error condition encountered.
            /// </summary>
            NoError
        }

        #endregion

        #region Nested type: TypeError

        /// <summary>
        /// Errors corresponding to the data validation of the Type field.
        /// </summary>
        private enum TypeError
        {
            /// <summary>
            /// Type name has not been specified.
            /// </summary>
            NotSpecified,

            /// <summary>
            /// Type name duplicates another.
            /// </summary>
            Duplicate,

            /// <summary>
            /// No error encountered.
            /// </summary>
            NoError
        }

        #endregion
    }
}