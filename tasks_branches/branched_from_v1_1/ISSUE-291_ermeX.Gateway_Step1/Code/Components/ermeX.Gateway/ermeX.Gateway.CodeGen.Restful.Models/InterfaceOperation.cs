using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ermeX.Gateway.CodeGen.Restful.Models
{
    public class InterfaceOperation
    {
        public string Name { get; set; }
        public Schema InputType { get; set; }
        public Schema OutputType { get; set; }
    }
}
