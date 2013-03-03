using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ermeX.Gateway.CodeGen.Restful.Models
{
    public class SchemaElement
    {
        public string Name { get; set; }

        /// <summary>
        /// Supports basic types
        /// </summary>
        public string Type { get; set; }
    }
}
