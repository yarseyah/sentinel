namespace Sentinel.Support
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Threading;

    public class ScrollingHelper
    {
        public delegate void VoidFunctionHandler(ListBox lb);

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