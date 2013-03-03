using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ermeX.Gateway.CodeGen.Restful.Models
{
    public class Document
    {
        public Dictionary<string, string> Namespace { get; set; }
        public List<Schema> Schema { get; set; }
        public Interface Interface { get; set; }
        public Binding Binding { get; set; }
        public Service Service { get; set; }
    }
}
