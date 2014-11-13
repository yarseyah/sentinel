using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sentinel.Controls
{
    [DataContract]
    public class RecentFileInfo
    {        
        [DataMember]
        public IEnumerable<string> RecentFilePaths { get; set; }
    }
}
