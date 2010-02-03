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
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using Sentinel.Logger;
using Sentinel.Services;

#endregion

namespace Sentinel.Networking
{

    #region Using directives

    #endregion

    public class UdpListener : INetworkListener
    {
        private const int MaximumAge = 500;

        private readonly BackgroundWorker backgroundWorker;

        private readonly ILogger details;

        private readonly ILogManager logManager = ServiceLocator.Instance.Get<ILogManager>();

        private readonly Queue<string> pendingQueue = new Queue<string>();

        private readonly BackgroundWorker purgeWorker;

        public UdpListener(string name, int port)
        {
            details = logManager.Get(name);

            Port = port;
            Name = name;

            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += BackgroundWorkerDoWork;
            backgroundWorker.RunWorkerCompleted += BackgroundWorkerRunWorkerCompleted;
            backgroundWorker.RunWorkerAsync();

            purgeWorker = new BackgroundWorker {WorkerReportsProgress = true};
            purgeWorker.DoWork += PurgeWorkerDoWork;
            purgeWorker.RunWorkerAsync();
        }

        #region INetworkListener Members

        public string Name { get; private set; }

        public int Port { get; private set; }

        #endregion

        private void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            UdpClient listener = null;

            try
            {
                listener = new UdpClient(Port);
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, Port);

                while (!e.Cancel)
                {
                    byte[] bytes = listener.Receive(ref endPoint);
                    string message = Encoding.ASCII.GetString(bytes, 0, bytes.Length);

                    lock (pendingQueue)
                    {
                        pendingQueue.Enqueue(message);
                    }
                }
            }
            catch (Exception exception)
            {
                e.Result = exception;
            }
            finally
            {
                if (listener != null)
                {
                    listener.Close();
                }
            }
        }

        private void BackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Trace.WriteLine(string.Format("RunWorkerCompleted - UdpListener {0} - {1}", Name, Port));
            Trace.WriteLine(string.Format("   - Cancelled = {0} ", e.Cancelled));
            Trace.WriteLine(string.Format("   - Error     = {0} ", e.Error));

            if (e.Result is Exception)
            {
                string errorMessage = string.Format(
                    "There was a problem with the UDP listener which was "
                    + " operating on port '{0}'\r\n"
                    + "\r\nThe following error message was returned from the " +
                    "exception:\r\n\r\n     \"{1}\"",
                    Port,
                    (e.Result as Exception).Message);

                ServiceLocator.Instance.Get<IViewManager>().SetState(Name, LogViewerState.Stopped);

                MessageBox.Show(
                    errorMessage,
                    "Exception Caught",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void PurgeWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            while (!e.Cancel)
            {
                // Go to sleep.
                Thread.Sleep(MaximumAge);

                if (pendingQueue.Count > 0)
                {
                    Queue<string> postQueue;
                    lock (pendingQueue)
                    {
                        // Copy the pendingQueue so that we block it for the smallest possible time.
                        postQueue = new Queue<string>(pendingQueue);
                        pendingQueue.Clear();
                    }

                    details.AddBatch(postQueue);
                }
            }
        }
    }
}