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
        public PickupParameters(string description, float value)
            : this(description, value, null)
        {
        }

        public PickupParameters(string description, float value, object[] additionalParameters)
        {
            this.value = value;
            this.description = description;
            AdditionalParameters = additionalParameters;
        }

        public override bool Equals(object obj)
        {
            var other = obj as PickupParameters;
            var bEquals = description.Equals(other.Description) && value == other.Value;
            return bEquals && (AdditionalParameters != null && AdditionalParameters.Length != 0
                       ? AdditionalParameters.Equals(other.AdditionalParameters)
                       : true);
        }

        public override int GetHashCode()
        {
            var hash = 1;
            hash = hash * 11 + description.GetHashCode();
            hash = hash * 17 + value.GetHashCode();

            if (AdditionalParameters != null && AdditionalParameters.Length != 0)
                hash = hash * 31 + AdditionalParameters.GetHashCode();

            return hash;
        }

        public override string ToString()
        {
            return "Desc.:" + description + ", Value: " + value;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        #region Fields

        private string description;
        private float value;

        //an optional array to store multiple parameters (used for play with sound/video when we pickup this object)

        #endregion

        #region Properties

        public string Description
        {
            get => description;
            set => description = value.Length != 0 ? value : "no description specified";
        }

        public float Value
        {
            get => value;
            set => this.value = value >= 0 ? value : 0;
        }

        public object[] AdditionalParameters { get; set; }

        #endregion
    }
}