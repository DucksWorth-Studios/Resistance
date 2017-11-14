/*
Function: 		Encapsulates the parameters for a collectable collidable object (e.g. "ammo", 10)
Author: 		NMCG
Version:		1.0
Date Updated:	14/11/17
Bugs:			None
Fixes:			None
*/
namespace GDLibrary
{
    public class PickupParameters
    {
        #region Fields
        private string description;
        private float value;
        #endregion

        #region Properties
        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = (value.Length != 0) ? value : "no description specified";
            }
        }
        public float Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = (value >= 0) ? value : 0;
            }
        }
        #endregion

        public PickupParameters(string description, float value)
        {
            this.value = value;
            this.description = description;
        }

        public override bool Equals(object obj)
        {
            PickupParameters other = obj as PickupParameters;
            return this.description.Equals(other.Description)
                && this.value == other.Value;
        }

        public override int GetHashCode()
        {
            int hash = 1;
            hash = hash * 11 + this.description.GetHashCode();
            hash = hash * 17 + this.value.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            return "Desc.:" + this.description + ", Value: " + this.value;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

    }
}
