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
using System.ComponentModel;
using System.Diagnostics;

#endregion

namespace Sentinel.Support.Mvvm
{
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Gets or sets the display name for the view model, used for debug output.
        /// </summary>
        public virtual string DisplayName { get; set; }

        /// <summary>
        /// Gets a value indicating whether an exception is thrown, or if a Debug.Fail() 
        /// is used when an invalid property name is passed to the VerifyPropertyName method.
        /// The default value is false, but subclasses used by unit tests might 
        /// override this property's getter to return true.
        /// </summary>
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        #region IDisposable Members

        /// <summary>
        /// Invoked when this object is being removed from the application
        /// and will be subject to garbage collection.
        /// </summary>
        public void Dispose()
        {
            OnDispose();
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        /// <summary>
        /// Finalizes an instance of the ViewModelBase class.
        /// </summary>
        ~ViewModelBase()
        {
            Debug.WriteLine(
                string.Format(
                    "{0} ({1}) ({2}) Finalized",
                    GetType().Name,
                    DisplayName,
                    GetHashCode()));
        }

        /// <summary>
        /// Warns the developer if this object does not have
        /// a public property with the specified name. This 
        /// method does not exist in a Release build.
        /// </summary>
        /// <param name="propertyName">Name of property to investigate.</param>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                // Verify that the property name matches a real,  
                // public, instance property on this object.
                if (TypeDescriptor.GetProperties(this)[propertyName] == null)
                {
                    string msg = "Invalid property name: " + propertyName;

                    if (ThrowOnInvalidPropertyName)
                    {
                        throw new Exception(msg);
                    }

                    Debug.Fail(msg);
                }
            }
        }

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            VerifyPropertyName(propertyName);

            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        /// <summary>
        /// Child classes can override this method to perform 
        /// clean-up logic, such as removing event handlers.
        /// </summary>
        protected virtual void OnDispose()
        {
        }
    }
}