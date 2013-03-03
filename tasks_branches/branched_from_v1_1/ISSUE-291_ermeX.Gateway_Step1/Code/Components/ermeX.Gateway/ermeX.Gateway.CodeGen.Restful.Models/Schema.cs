using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ermeX.Gateway.CodeGen.Restful.Models
{
    public class Schema
    {
        public string Name { get; set; }

        public List<SchemaElement> Elements { get; set; }
    }
}
