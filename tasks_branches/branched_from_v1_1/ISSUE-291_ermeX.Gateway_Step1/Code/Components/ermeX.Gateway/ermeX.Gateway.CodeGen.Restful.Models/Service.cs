using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ermeX.Gateway.CodeGen.Restful.Models
{
    public class Service
    {
        public string Name { get; set; }
        public Interface Interface { get; set; }
        public List<Endpoint> Endpoints { get; set; }
    }
}
