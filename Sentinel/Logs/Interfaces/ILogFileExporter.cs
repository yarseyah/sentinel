using Sentinel.Views.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentinel.Logs.Interfaces
{
    public interface ILogFileExporter
    {
        void SaveLogViewerToFile(IWindowFrame windowFrame, string filePath);
    }
}
