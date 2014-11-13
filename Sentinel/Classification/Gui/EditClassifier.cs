using Sentinel.Classification.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Sentinel.Classification.Gui
{
    public class EditClassifier : IEditClassifyingService
    {
        public void Edit(IClassifier classifier)
        {
            Debug.Assert(classifier != null, "Extractor must be supplied to allow editing.");

            var window = new AddEditClassifierWindow();
            var data = new AddEditClassifier(window, true);
            window.DataContext = data;
            window.Owner = Application.Current.MainWindow;

            data.Name = classifier.Name;
            data.Field = classifier.Field;
            data.Pattern = classifier.Pattern;
            data.Mode = classifier.Mode;
            data.Type = classifier.Type;
            
            bool? dialogResult = window.ShowDialog();

            if (dialogResult != null && (bool)dialogResult)
            {
                classifier.Name = data.Name;
                classifier.Pattern = data.Pattern;
                classifier.Mode = data.Mode;
                classifier.Field = data.Field;
                classifier.Type = data.Type;
            }
        }
    }
}
