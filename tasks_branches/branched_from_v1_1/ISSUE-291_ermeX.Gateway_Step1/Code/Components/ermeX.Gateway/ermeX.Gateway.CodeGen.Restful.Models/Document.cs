using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ermeX.Gateway.CodeGen.Restful.Models
{
    public class Document
    {
        public Dictionary<string, string> Namespaces { get; set; }
        public List<Schema> Schema { get; set; }
        public List<Interface> Interfaces { get; set; }
        public List<Binding> Bindings { get; set; }
        public List<Service> Services { get; set; }
    }
}
