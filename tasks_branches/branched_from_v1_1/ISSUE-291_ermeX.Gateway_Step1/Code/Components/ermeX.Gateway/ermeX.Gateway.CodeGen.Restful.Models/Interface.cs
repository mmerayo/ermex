using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ermeX.Gateway.CodeGen.Restful.Models
{
    public class Interface
    {
        public string Name { get; set; }
        public string NamespaceUri { get; set; }
        public List<InterfaceOperation> Operations { get; set; }
    }
}
