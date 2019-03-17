namespace WpfExtras
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;

    using Common.Logging;

    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        private readonly ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelBase"/> class.
        /// </summary>
        protected ViewModelBase()
        {
            log = LogManager.GetLogger(GetType().Name);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ViewModelBase"/> class.
        /// </summary>
        ~ViewModelBase()
        {
            log.DebugFormat("{0} ({1}) ({2}) Finalized", GetType().Name, DisplayName, GetHashCode());
        }

        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the display name for the view model, used for debug output.
        /// </summary>
        protected string DisplayName { get; set; }

        /// <summary>
        /// Gets a value indicating whether an exception is thrown, or if a Debug.Fail()
        /// is used when an invalid property name is passed to the VerifyPropertyName method.
        /// The default value is false, but subclasses used by unit tests might
        /// override this property's getter to return true.
        /// </summary>
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        /// <summary>
        /// Invoked when this object is being removed from the application
        /// and will be subject to garbage collection.
        /// </summary>
        public void Dispose()
        {
            OnDispose();
            GC.SuppressFinalize(this);
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
                    var msg = "Invalid property name: " + propertyName;

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

            var handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
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