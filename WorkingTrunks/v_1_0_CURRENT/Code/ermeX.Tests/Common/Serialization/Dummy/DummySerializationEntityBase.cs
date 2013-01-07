// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using ermeX.Tests.Common.RandomValues;

namespace ermeX.Tests.Common.Serialization.Dummy
{
    internal abstract class DummySerializationEntityBase
    {
        public DummySerializationEntityBase TheBaseValue;
        private int _thePrivateField;

        protected DummySerializationEntityBase()
        {
            _thePrivateField = RandomHelper.GetRandomInt(12, 1000);
            ThePrivateProperty = RandomHelper.GetRandomInt(123, 11000);
            ThePrivateSetter = RandomHelper.GetRandomInt(33, 33333);
        }

        private int ThePrivateProperty { get; set; }
        public int ThePrivateSetter { get; private set; }

        public int TheInt { get; set; }
        public string TheString { get; set; }
        public object TheObjectValue { get; set; }
        public Dictionary<string, DummySerializationEntityBase> TheBaseDictionary { get; set; }
        public Dictionary<string, object> TheObjectDictionary { get; set; }
        public List<DummySerializationEntityBase> TheBaseList { get; set; }
        public List<object> TheObjectList { get; set; }

        public TestEnum TheEnumProperty { get; set; }


       

        #region Nested type: TestEnum

        internal enum TestEnum
        {
            Value1 = 1,
            Value2
        }

        #endregion
    }
}