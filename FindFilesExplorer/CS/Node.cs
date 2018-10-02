using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindFilesExplorer.CS
{
    public class Node
    {
        public ObservableCollection<Node> Nodes { get; set; }
        public string Name { get; set; }
    }
}
