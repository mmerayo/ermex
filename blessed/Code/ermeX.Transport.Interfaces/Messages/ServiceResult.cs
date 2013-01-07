// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using ProtoBuf;
using ermeX.Common;

namespace ermeX.Transport.Interfaces.Messages
{
    [ProtoContract(SkipConstructor = true)]
    internal class ServiceResult
    {
        private List<string> _serverMessages = new List<string>();
        private object _resultData;
        private string _jsonResultData;

        public ServiceResult(bool ok)
        {
            Ok = ok;
        }

        public ServiceResult()
        {
        }

        /// <summary>
        ///   if not default(Guid) then has the id for the response
        /// </summary>
        [ProtoMember(1)]
        public Guid AsyncResponseId { get; set; }

        [ProtoMember(2)]
        public bool Ok { get; set; }

        [ProtoMember(3)]
        public string JsonResultData
        {
            get
            {
                if (string.IsNullOrEmpty(_jsonResultData))
                {
                    _jsonResultData = _resultData == null
                                          ? string.Empty
                                          : JsonSerializer.SerializeObjectToJson(_resultData);
                }
                return _jsonResultData;
            }
            private set { _jsonResultData = value; }
        }

        /// <summary>
        ///   the result of the sync services is carried here
        /// </summary>
        public object ResultData
        {
            get
            {
                if (_resultData == null)
                    _resultData = string.IsNullOrEmpty(_jsonResultData)
                                      ? null
                                      : JsonSerializer.DeserializeObjectFromJson<object>(_jsonResultData);


                return _resultData;
            }
            set { _resultData = value; }
        }


        [ProtoMember(4)]
        public List<string> ServerMessages
        {
            get { return _serverMessages; }
            private set { _serverMessages = value; }
        }
    }
}