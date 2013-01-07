// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Linq;
using System.Text;

namespace ermeX.Tests.Acceptance.Dummy
{
    [Serializable]
    public class AcceptanceMessageType1 : AcceptanceMessageType
    {
        public AcceptanceMessageType1(bool generateRandomValues = false)
            : base(generateRandomValues)
        {
        }

        public static bool operator ==(AcceptanceMessageType1 a, AcceptanceMessageType1 b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            if (a.GetType() != b.GetType())
                throw new InvalidOperationException("Types are different");

            // Return true if the fields match:
            return a.Id == b.Id && a.TheInt == b.TheInt && a.TheString == b.TheString && a.CompareLists(b) && a.TheDateTime.Ticks == b.TheDateTime.Ticks;
        }

        public static bool operator !=(AcceptanceMessageType1 a, AcceptanceMessageType1 b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(AcceptanceMessageType1)) return false;
            return this == (AcceptanceMessageType1)obj;
        }
        public bool Equals(AcceptanceMessageType1 other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return this == other;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int result = TheInt;
                result = (result * 397) ^ (TheString != null ? TheString.GetHashCode() : 0);
                result = (result * 397) ^ (TheArray != null ? TheArray.GetHashCode() : 0);
                result = (result * 397) ^ (TheList != null ? TheList.GetHashCode() : 0);
                return result;
            }
        }


    }
}
