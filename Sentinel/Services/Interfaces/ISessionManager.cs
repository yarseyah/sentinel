namespace Sentinel.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Windows;

    using Sentinel.Interfaces.Providers;
    using Sentinel.Providers.Interfaces;

    using WpfExtras;

    public interface ISessionManager
    {
        /// <summary>
        /// Gets the name of the session.
        /// </summary>
        [DataMember]
        string Name { get; }

        [DataMember]
        IEnumerable<IProviderSettings> ProviderSettings { get; }

        IEnumerable<ViewModelBase> ChangingViewModelBases { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current session has been saved
        /// in its current state (e.g. true if not dirty).
        /// </summary>
        bool IsSaved { get; set; }

        /// <summary>
        /// Called when loading a brand new session.
        /// </summary>
        /// <param name="parent">The <see cref="Window"/> to associate the new session.</param>
        void LoadNewSession(Window parent);

        /// <summary>
        /// Initialise the session manager from supplied providers.
        /// </summary>
        /// <param name="providers">Providers collection to load.</param>
        void LoadProviders(IEnumerable<PendingProviderRecord> providers);

        /// <summary>
        /// Called when loading a session that has been saved to disk.
        /// </summary>
        /// <param name="filePath">Directory where the session is saved.</param>
        void LoadSession(string filePath);

        /// <summary>
        /// Save the current session to the specified path.
        /// </summary>
        /// <param name="filePath">Directory to write sesion file.</param>
        void SaveSession(string filePath);
    }
}
