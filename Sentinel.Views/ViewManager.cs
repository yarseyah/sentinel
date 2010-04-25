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
using System.Diagnostics;
using System.Linq;
using Sentinel.Logs.Interfaces;
using Sentinel.Views.Interfaces;

#endregion

namespace Sentinel.Views
{
    [Export(typeof(IViewManager))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ViewManager : IViewManager
    {
        private Dictionary<IViewInformation, Type> registeredTypes = new Dictionary<IViewInformation, Type>();

        private List<IWindowFrame> instances = new List<IWindowFrame>();

        [ImportMany(typeof(ILogViewer))]
        private IEnumerable<Lazy<ILogViewer, IViewInformation>> viewers { get; set; }

        [Import(typeof(ILogManager))]
        private ILogManager logManager;

        public ViewManager()
        {
            Viewers = new ObservableCollection<IWindowFrame>();
        }

        #region IViewManager Members

        /// <summary>
        /// Observerable collection of the instances of a viewer main frame.
        /// </summary>
        public ObservableCollection<IWindowFrame> Viewers { get; private set; }

        public void Register(IViewInformation info, Type viewerType)
        {
            if ( registeredTypes.Any(t => t.Key.Identifier == info.Identifier) )
            {
                throw new NotSupportedException("Already have a registered viewer with the Id of " + info.Identifier);
            }

            // Validate that the type supports the necessary interface: ILogViewer
            Type intefaceType = typeof(ILogViewer);
            if (!viewerType.GetInterfaces().Any(i => i == intefaceType))
            {
                throw new NotSupportedException("Types registered in ViewManager must support the inteface " +
                                                intefaceType);
            }

            // Populate the registration information.
            registeredTypes.Add(info, viewerType);
        }

        public IEnumerable<IViewInformation> GetRegistered()
        {
            // Combine the explicit and automatically registered views.
            List<IViewInformation> manuallyRegistered = new List<IViewInformation>(registeredTypes.Keys);
            List<IViewInformation> automaticallyRegistered = new List<IViewInformation>(viewers.Select(v => v.Metadata));

            return manuallyRegistered.Concat(automaticallyRegistered);
        }

        public IViewInformation Get(string identifier)
        {
            List<IViewInformation> manuallyRegistered = new List<IViewInformation>(registeredTypes.Keys);
            List<IViewInformation> automaticallyRegistered = new List<IViewInformation>(viewers.Select(v => v.Metadata));

            return manuallyRegistered.Concat(automaticallyRegistered).FirstOrDefault(v => v.Identifier == identifier);
        }

        public ILogViewer GetInstance(string identifier)
        {
            List<IViewInformation> manuallyRegistered = new List<IViewInformation>(registeredTypes.Keys);
            List<IViewInformation> automaticallyRegistered = new List<IViewInformation>(viewers.Select(v => v.Metadata));
            
            // See if in automatically registered list
            if ( automaticallyRegistered.Any(i => i.Identifier == identifier))
            {
                // Need to handle this type of registration
                foreach (Lazy<ILogViewer, IViewInformation> lazyViewer in viewers)
                {
                    if ( lazyViewer.Metadata.Identifier == identifier )
                    {
                        // Ideally, we should be able to ask MEF for a new dynamic creation of the 
                        // item, but we will always get the same one.  So try and establish the
                        // concrete type and create one of them.

                        Type t = lazyViewer.Value.GetType();
                        return (ILogViewer) Activator.CreateInstance(t);

                        // return lazyViewer.Value;
                    }
                }

                throw new NotSupportedException(
                    "Precheck stated that there was a registered viewer, but subsequent walking "
                    + "of the collection didn't find one - has the colleciton been modified?");
            }

            if (manuallyRegistered.Any(i => i.Identifier == identifier))
            {
                Type t = registeredTypes.First(v => v.Key.Identifier == identifier).Value;

                // Create an instance of the type (must have a default constructor).
                return (ILogViewer) Activator.CreateInstance(t);
            }

            Debug.Assert(
                manuallyRegistered.Concat(automaticallyRegistered).Any(i => i.Identifier == identifier),
                "Identifier must be registered in the collection of views, either explicity or by auto discovery");

            return null;
        }

        #endregion
    }
}