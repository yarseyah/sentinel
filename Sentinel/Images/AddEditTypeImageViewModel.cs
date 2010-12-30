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
using System.ComponentModel.Composition;
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
    [Export]
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
                        "Image must be less than 128x128. [Note: images are most often used at 20x20]"
                        },
                    {ImageError.Unknown, "Problem with reading image."},
                    {ImageError.NoError, null}
                };

        [Import(typeof(ITypeImageService))]
        private ITypeImageService ImageService { get; set; }

        private readonly Dictionary<TypeError, string> typeErrorMessages =
            new Dictionary<TypeError, string>
                {
                    {TypeError.NotSpecified, "Type name must be specified."},
                    {TypeError.Duplicate, "There is already an entry for types with this name."},
                    {TypeError.NoError, null}
                };

        private readonly Window window;

        private string errorMessage = "No image selected.";

        private string fileName;

        private BitmapImage image;

        private ImageError imageError = ImageError.NotSpecified;

        private string type = string.Empty;

        private TypeError typeError = TypeError.NotSpecified;

        public AddEditTypeImageViewModel(Window window, ITypeImageService images)
        {
            this.window = window;
            this.window.DataContext = this;

            Accept = new DelegateCommand(e => CloseDialog(true), c => IsValid);
            Reject = new DelegateCommand(e => CloseDialog(false));
            Browse = new DelegateCommand(BrowseForImageFiles);

            ImageService = images;

            UpdateErrorMessage(false);
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

                if (fieldName == "Type")
                {
                    TypeError oldTypeError = typeError;

                    if (!string.IsNullOrEmpty(Type)
                        && ImageService != null
                        && ImageService.ImageMappings.Any(kvp => kvp.Key == Type))
                    {
                        typeError = TypeError.Duplicate;
                    }
                    else if (fieldName == "Type" && string.IsNullOrEmpty(Type))
                    {
                        typeError = TypeError.NotSpecified;
                    }
                    else if (fieldName == "Type")
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
            OpenFileDialog openFileDialog = new OpenFileDialog
                                                {
                                                    Title = "Select image",
                                                    ValidateNames = true,
                                                    CheckFileExists = true,
                                                    Multiselect = false,
                                                    Filter = "Images files|*.png;*.bmp|All Files|*.*",
                                                    InitialDirectory =
                                                        Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
                                                };

            bool? dialogResult = openFileDialog.ShowDialog(window);

            ImageError oldImageError = imageError;

            if (dialogResult == true)
            {
                try
                {
                    // Check file exists, it should!
                    FileInfo fi = new FileInfo(openFileDialog.FileName);
                    if (fi.Exists)
                    {
                        BitmapImage i = new BitmapImage();
                        i.BeginInit();
                        i.UriSource = new Uri(openFileDialog.FileName, UriKind.RelativeOrAbsolute);
                        i.EndInit();

                        Image = i;

                        // Some santity checking.
                        if (i.Width <= 128 && i.Height <= 128)
                        {
                            imageError = ImageError.NoError;
                        }
                        else
                        {
                            imageError = ImageError.TooLarge;
                        }
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

                // In all cases, keep the filename entered or it is confusing for the user.
                FileName = openFileDialog.FileName;

                if (imageError != oldImageError)
                {
                    UpdateErrorMessage(true);
                }
            }
        }

        private void CloseDialog(bool dialogResult)
        {
            window.DialogResult = dialogResult;
            window.Close();
        }

        private void UpdateErrorMessage(bool imageChangedMostRecently)
        {
            string oldErrorMessage = Error;
            string newErrorMessage;

            string imageErrorMessage = imageErrorMessages[imageError];
            string typeErrorMessage = typeErrorMessages[typeError];

            if (imageChangedMostRecently)
            {
                newErrorMessage = imageErrorMessage ?? typeErrorMessage;
            }
            else
            {
                newErrorMessage = typeErrorMessage ?? imageErrorMessage;
            }

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