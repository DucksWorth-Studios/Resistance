/*
Function: 		Represents a bounded rail in 3D along which an object can translate. Typically used by a rail controller attached to a camera which causes the camera to follow a moving object in a room.
Author: 		NMCG
Version:		1.0
Date Updated:	30/8/17
Bugs:			None
Fixes:			None
*/

using System;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class RailParameters
    {
        public RailParameters(string id, Vector3 start, Vector3 end)
        {
            this.start = start;
            this.end = end;
            ID = id;

            isDirty = true;
        }


        //Returns true if the position is between start and end, otherwise false
        public bool InsideRail(Vector3 position)
        {
            var distanceToStart = Vector3.Distance(position, start);
            var distanceToEnd = Vector3.Distance(position, end);
            return distanceToStart <= length && distanceToEnd <= length;
        }


        private void Update()
        {
            if (isDirty)
            {
                length = Math.Abs(Vector3.Distance(start, end));
                look = Vector3.Normalize(end - start);
                midPoint = (start + end) / 2;
                isDirty = false;
            }
        }

        #region Fields

        private Vector3 start;
        private readonly Vector3 end;
        private Vector3 midPoint, look;
        private bool isDirty;
        private float length;

        #endregion

        #region Properties

        public Vector3 Look
        {
            get
            {
                Update();
                return look;
            }
        }

        public float Length
        {
            get
            {
                Update();
                return length;
            }
        }

        public Vector3 MidPoint
        {
            get
            {
                Update();
                return midPoint;
            }
        }

        public Vector3 Start
        {
            get => start;
            set
            {
                start = value;
                isDirty = true;
            }
        }

        public Vector3 End
        {
            get => end;
            set
            {
                start = value;
                isDirty = true;
            }
        }

        public string ID { get; set; }

        #endregion

        //Add Equals, Clone, ToString, GetHashCode...
    }
}