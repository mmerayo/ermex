// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using ermeX.Tests.Common;
using ermeX.Tests.Common.RandomValues;

namespace ermeX.Tests.Acceptance.Dummy
{
    [Serializable]
    public abstract class AcceptanceMessageType : MarshalByRefObject
    {

        public AcceptanceMessageType(bool generateRandomValues = false)
        {
            if (generateRandomValues)
                GenerateRandomValues();
        }
        public Guid Id { get; set; }
        public int TheInt { get; set; }
        public string TheString { get; set; }
        public int[] TheArray { get; set; }
        public List<string> TheList { get; set; }
        public DateTime TheDateTime { get; set; }

        public void GenerateRandomValues()
        {
            TheInt = RandomHelper.GetRandomInt(0, int.MaxValue);
            TheString = RandomHelper.GetRandomString(15);

            TheArray = new int[RandomHelper.GetRandomInt()];
            for (int i = 0; i < TheArray.Length; i++)
                TheArray[i] = RandomHelper.GetRandomInt(0, 255);

            int items = RandomHelper.GetRandomInt();
            TheList = new List<string>(items);
            for (int i = 0; i < items; i++)
                TheList.Add(RandomHelper.GetRandomString());

            TheDateTime = RandomHelper.GetRandomDateTime();

        }


        protected bool CompareLists(AcceptanceMessageType other)
        {
            if ((this.TheArray == null && other.TheArray != null) || (this.TheArray != null && other.TheArray == null))
                return false;
            if ((this.TheList == null && other.TheList != null) || (this.TheList != null && other.TheList == null))
                return false;
            if (TheArray != null && TheArray.Length != other.TheArray.Length)
                return false;
            if (TheList != null && this.TheList.Count != other.TheList.Count)
                return false;

            if (TheArray != null)
                for (int index = 0; index < TheArray.Length; index++)
                {
                    if (TheArray[index] != other.TheArray[index])
                        return false;
                }

            if (TheList != null)
                for (int index = 0; index < TheList.Count; index++)
                {
                    if (TheList[index] != other.TheList[index])
                        return false;
                }
            return true;
        }



    }
}