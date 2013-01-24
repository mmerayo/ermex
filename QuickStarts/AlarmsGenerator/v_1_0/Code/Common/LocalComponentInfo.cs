// /*---------------------------------------------------------------------------------------*/
//        Licensed to the Apache Software Foundation (ASF) under one
//        or more contributor license agreements.  See the NOTICE file
//        distributed with this work for additional information
//        regarding copyright ownership.  The ASF licenses this file
//        to you under the Apache License, Version 2.0 (the
//        "License"); you may not use this file except in compliance
//        with the License.  You may obtain a copy of the License at
// 
//          http://www.apache.org/licenses/LICENSE-2.0
// 
//        Unless required by applicable law or agreed to in writing,
//        software distributed under the License is distributed on an
//        "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//        KIND, either express or implied.  See the License for the
//        specific language governing permissions and limitations
//        under the License.
// /*---------------------------------------------------------------------------------------*/
using System;

namespace Common
{
    /// <summary>
    /// Local component data
    /// </summary>
    public sealed class LocalComponentInfo : ComponentInfo
    {

        public LocalComponentInfo(string friendlyName, Guid componentId,int port):this(friendlyName, componentId,port,null)
        {
            
        }
        public LocalComponentInfo(string friendlyName, Guid componentId, int port, FriendComponentInfo friendComponent)
            : base(componentId, port)
        {
            if(string.IsNullOrEmpty(friendlyName))
                throw new ArgumentException("friendlyName must have a value");
            FriendlyName = friendlyName;
            FriendComponent = friendComponent;
        }

        /// <summary>
        /// The friend component is only provided once there is already a component created previously
        /// </summary>
        public FriendComponentInfo FriendComponent { get; private set; }

        /// <summary>
        /// The friendly name of the component.<remarks>it will help to identify it in the network in a nicer way than using the id</remarks>
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Creates a LocalComponentInfo from the call paramters
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static LocalComponentInfo FromCallParameters(string [] args)
        {
            FriendComponentInfo friendComponent = null;

            //if it was configured to join another(it was not the first process created)
            if(args.Length>3) 
                friendComponent = new FriendComponentInfo(Guid.Parse(args[3]), int.Parse(args[4]));
            
            return new LocalComponentInfo(args[0], Guid.Parse(args[1]),int.Parse(args[2]),friendComponent);
        }
    }
}