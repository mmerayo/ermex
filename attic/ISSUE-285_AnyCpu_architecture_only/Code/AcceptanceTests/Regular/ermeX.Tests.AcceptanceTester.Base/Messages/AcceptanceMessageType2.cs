using System;

namespace ermeX.Tests.AcceptanceTester.Base.Messages
{
    [Serializable]
    public class AcceptanceMessageType2 : AcceptanceMessageType
    {


        public AcceptanceMessageType2(bool generateRandomValues = false)
            : base(generateRandomValues)
        {

        }

        public static bool operator ==(AcceptanceMessageType2 a, AcceptanceMessageType2 b)
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

        public static bool operator !=(AcceptanceMessageType2 a, AcceptanceMessageType2 b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(AcceptanceMessageType2)) return false;
            return this == (AcceptanceMessageType2)obj;
        }
        public bool Equals(AcceptanceMessageType2 other)
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
