using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    /// <summary>
    /// Component data
    /// </summary>
    public abstract class ComponentInfo
    {
        protected ComponentInfo(Guid componentId, int port)
        {
            ComponentId = componentId;
            Port = port;
        }

        public Guid ComponentId { get; private set; }
        public int Port { get; private set; }
    }
}
