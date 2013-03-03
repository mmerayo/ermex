using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ermeX.Gateway.CodeGen.Restful.Models
{
    public class Binding
    {
        public string Name { get; set; }
        public Interface Interface { get; set; }
        public List<BindingOperation> Operations { get; set; }
    }
}
