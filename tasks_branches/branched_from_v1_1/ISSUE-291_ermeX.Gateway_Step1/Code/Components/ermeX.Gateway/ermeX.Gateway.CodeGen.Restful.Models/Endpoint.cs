using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ermeX.Gateway.CodeGen.Restful.Models
{
    public class Endpoint
    {
        public string Name { get; set; }
        public Binding Binding { get; set; }
        public string Address { get; set; }
    }
}
