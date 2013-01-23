using System;

namespace Common
{
    /// <summary>
    /// Local component data
    /// </summary>
    public sealed class LocalComponentInfo : ComponentInfo
    {

        public LocalComponentInfo(Guid componentId,int port):this(componentId,port,null)
        {
            
        }
        public LocalComponentInfo(Guid componentId, int port,FriendComponentInfo friendComponent):base(componentId,port)
        {
            FriendComponent = friendComponent;
        }

        /// <summary>
        /// The friend component is only provided once there is already a component created previously
        /// </summary>
        public FriendComponentInfo FriendComponent { get; private set; }


        /// <summary>
        /// Creates a LocalComponentInfo from the call paramters
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static LocalComponentInfo FromCallParameters(string [] args)
        {
            FriendComponentInfo friendComponent = null;
            if(args.Length>2) 
                friendComponent = new FriendComponentInfo(Guid.Parse(args[2]), int.Parse(args[3]));
            
            return new LocalComponentInfo(Guid.Parse(args[0]),int.Parse(args[1]),friendComponent);
        }
    }
}