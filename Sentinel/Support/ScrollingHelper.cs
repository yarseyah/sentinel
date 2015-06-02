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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

#endregion

namespace Sentinel.Support
{
    public class ScrollingHelper
    {
        #region Delegates

        public delegate void VoidFunctionHandler(ListBox lb);

        #endregion

        public static Visual GetDescendantByType(Visual element, Type type)
        {
            if (element != null)
            {
                if (element.GetType() != type)
                {
                    Visual foundElement = null;
                    if (element is FrameworkElement)
                    {
                        (element as FrameworkElement).ApplyTemplate();
                    }

                    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
                    {
                        Visual visual = VisualTreeHelper.GetChild(element, i) as Visual;
                        foundElement = GetDescendantByType(visual, type);
                        if (foundElement != null)
                        {
                            break;
                        }
                    }

                    return foundElement;
                }

                return element;
            }

            return null;
        }

        public static void ScrollToEnd(Dispatcher dispatcher, ListBox listBox)
        {
            if (dispatcher.CheckAccess())
            {
                SelectLastEntry(listBox);
            }
            else
            {
                dispatcher.BeginInvoke(
                    DispatcherPriority.Send,
                    new VoidFunctionHandler(SelectLastEntry),
                    listBox);
            }
        }

        private static void SelectLastEntry(ListBox listBox)
        {
            ScrollViewer scrollViewer = GetDescendantByType(listBox, typeof(ScrollViewer)) as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToEnd();
            }
        }
    }
}