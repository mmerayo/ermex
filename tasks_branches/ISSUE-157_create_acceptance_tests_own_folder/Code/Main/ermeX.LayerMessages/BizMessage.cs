// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using ProtoBuf.Meta;
using ermeX.Common;

namespace ermeX.LayerMessages
{
    /// <summary>
    /// Represents a message at the BizLayer level
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    internal sealed class BizMessage: SystemMessage
    {
        protected string _jsonMessage;
        protected object _data = null;

        public BizMessage(object data) : base()
        {
            if (data == null) throw new ArgumentNullException("data");
            _data = data;
        }

        private BizMessage(){}

        public static BizMessage FromJson(string jsonData)
        {
            if(string.IsNullOrEmpty(jsonData)) 
                throw new ArgumentException();
            return new BizMessage(){JsonMessage = jsonData};
        }

        [ProtoMember(75)]
        internal string JsonMessage
        {
            get
            {
                if(string.IsNullOrEmpty(_jsonMessage))
                {
                    if (_data == null)
                        throw new ApplicationException(
                            "One of both, _data or the serialized json message must have a value");
                    _jsonMessage = JsonSerializer.SerializeObjectToJson(_data);
                }
                return _jsonMessage;
            }
            private set { _jsonMessage = value; }
        }

        public Type MessageType
        {
            get
            {
                UpdateData();
                if (_data == null)
                    return typeof(void);
                return _data.GetType();
            }
        }

        public object RawData
        {
            get
            {
                UpdateData();
                return _data;
            }
        }

        private void UpdateData()
        {
            if (_data == null)
            {
                if (string.IsNullOrEmpty(_jsonMessage))
                    throw new ApplicationException(
                        "One of both, _data or the serialized json message must have a value");
                _data = JsonSerializer.DeserializeObjectFromJson<object>(_jsonMessage);
            }
        }
    }

}
