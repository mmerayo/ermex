// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;

namespace ermeX.Tests.WorldGateTests.Mock
{
    public struct MyCustomStruct
    {
        public DateTime DateTime;
        public TimeSpan TimeSpan;
        public Guid Guid;
        public string TheString;
        public decimal TheDecimal;
        public long TheLong;

        public bool AreEqual(MyCustomStruct o)
        {
            return DateTime == o.DateTime && TimeSpan == o.TimeSpan && Guid == o.Guid && TheString == o.TheString &&
                   TheDecimal == o.TheDecimal && TheLong == o.TheLong;
        }
    }
}