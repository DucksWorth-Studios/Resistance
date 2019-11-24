/*
Function: 		A stateful variable to store the a succession of boolean states where the number stored is defined by capacity.
Author: 		NMCG
Version:		1.0
Date Updated:	10/10/17
Bugs:			None
Fixes:			None
*/

using System.Collections.Generic;
using System.Text;

namespace GDLibrary
{
    public class StatefulBool
    {
        private readonly int capacity;
        private readonly List<bool> stateList;

        public StatefulBool(int capacity)
        {
            this.capacity = capacity;
            stateList = new List<bool>(capacity);
        }

        public void Update(bool state)
        {
            stateList.Insert(0, state);

            //ensure that there are always just "capacity" states stored
            if (stateList.Count > capacity)
                stateList.RemoveAt(stateList.Count - 1);
        }

        //returns true if state goes from false to true
        public bool IsActivating()
        {
            if (stateList.Count >= 2)
                return stateList[0] && !stateList[1];
            return false;
        }

        //returns true if state goes from true to false
        public bool IsDeactivating()
        {
            if (stateList.Count >= 2)
                return !stateList[0] && stateList[1];
            return false;
        }

        //returns the last stored state
        public bool IsActive()
        {
            if (stateList.Count >= 1)
                return stateList[0];
            return false;
        }

        //returns true if active over first two successive states in the list
        public bool IsStillActive()
        {
            if (stateList.Count > 1)
                return stateList[0] && stateList[1];
            return false;
        }

        public override string ToString()
        {
            var str = new StringBuilder();

            foreach (var state in stateList)
            {
                str.Append(state);
                str.Append(",");
            }

            return str.ToString();
        }
    }
}