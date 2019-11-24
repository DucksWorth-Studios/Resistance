/*
Function: 		Used in CameraManager::SortByDepth to sort cameras by depth on screen for picture-in-picture effect. 
                We can also use a lambda expression - See https://www.dotnetperls.com/sort-list
Author: 		NMCG
Version:		1.0
Date Updated:	24/8/17
Bugs:			None
Fixes:			None
*/

using System.Collections.Generic;

namespace GDLibrary
{
    public class CameraDepthComparer : IComparer<Camera3D>
    {
        private readonly SortDirectionType sortDirectionType;

        public CameraDepthComparer(SortDirectionType sortDirectionType)
        {
            this.sortDirectionType = sortDirectionType;
        }

        public int Compare(Camera3D first, Camera3D second)
        {
            var diff = first.DrawDepth - second.DrawDepth;

            if (sortDirectionType == SortDirectionType.Descending)
                diff *= -1;

            if (diff < 0)
                return -1;
            if (diff > 0)
                return 1;
            return 0;
        }
    }
}