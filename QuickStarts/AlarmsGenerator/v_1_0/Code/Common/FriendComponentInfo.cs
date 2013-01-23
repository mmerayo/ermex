using System;

namespace Common
{
    /// <summary>
    /// Friend Component Data
    /// </summary>
    public sealed class FriendComponentInfo : ComponentInfo
    {

        public FriendComponentInfo(Guid componentId,int port):base(componentId,port)
        {
        }
    }
}