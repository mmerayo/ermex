using System;

namespace ermeX.Tests.AcceptanceTester.Base.Messages
{
    [Serializable]
    public class AcceptanceMessageType3 : AcceptanceMessageType
    {
        public AcceptanceMessageType3(bool generateRandomValues = false)
            : base(generateRandomValues)
        {
        }

        public static bool operator ==(AcceptanceMessageType3 a, AcceptanceMessageType3 b)
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
            return a.SenderId == b.SenderId && a.TheInt == b.TheInt && a.TheString == b.TheString && a.CompareLists(b) && a.TheDateTime.Ticks == b.TheDateTime.Ticks;
        }

        public static bool operator !=(AcceptanceMessageType3 a, AcceptanceMessageType3 b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(AcceptanceMessageType3)) return false;
            return this == (AcceptanceMessageType3)obj;
        }
        public bool Equals(AcceptanceMessageType3 other)
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
