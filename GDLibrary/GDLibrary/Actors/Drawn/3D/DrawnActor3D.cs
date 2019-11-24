/*
Function: 		Represents the parent class for all updateable AND drawn 3D game objects. Notice that Effect has been added.
Author: 		NMCG
Version:		1.0
Date Updated:	17/8/17
Bugs:			None
Fixes:			None
*/

using System;

namespace GDLibrary
{
    public class DrawnActor3D : Actor3D, ICloneable
    {
        #region Fields

        #endregion

        //used when we don't want to specify status type
        public DrawnActor3D(string id, ActorType actorType, Transform3D transform, EffectParameters effectParameters)
            : this(id, actorType, transform, effectParameters, StatusType.Drawn | StatusType.Update)
        {
        }

        public DrawnActor3D(string id, ActorType actorType, Transform3D transform, EffectParameters effectParameters,
            StatusType statusType)
            : base(id, actorType, transform, statusType)
        {
            EffectParameters = effectParameters;
        }

        public new object Clone()
        {
            IActor actor = new DrawnActor3D("clone - " + ID, //deep
                ActorType, //deep
                (Transform3D) Transform.Clone(), //deep - calls the clone for Transform3D explicitly
                EffectParameters.GetDeepCopy(), //hybrid - shallow (texture and effect) and deep (all other fields) 
                StatusType); //deep - a simple numeric type

            if (ControllerList != null)
                //clone each of the (behavioural) controllers
                foreach (var controller in ControllerList)
                    actor.AttachController((IController) controller.Clone());

            return actor;
        }

        public override float GetAlpha()
        {
            return EffectParameters.Alpha;
        }

        public override bool Equals(object obj)
        {
            var other = obj as DrawnActor3D;

            if (other == null)
                return false;
            if (this == other)
                return true;

            return EffectParameters.Equals(other.EffectParameters) && Alpha.Equals(other.Alpha) && base.Equals(obj);
        }

        public override int GetHashCode()
        {
            var hash = 1;
            hash = hash * 31 + EffectParameters.GetHashCode();
            hash = hash * 43 + base.GetHashCode();
            return hash;
        }

        public override bool Remove()
        {
            EffectParameters = null;
            return base.Remove();
        }

        #region Properties

        public EffectParameters EffectParameters { get; set; }

        public float Alpha
        {
            get => EffectParameters.Alpha;
            set
            {
                //opaque to transparent AND valid (i.e. 0 <= x < 1)
                if (EffectParameters.Alpha == 1 && value < 1)
                    EventDispatcher.Publish(new EventData("OpTr", this, EventActionType.OnOpaqueToTransparent,
                        EventCategoryType.Opacity));
                //transparent to opaque
                else if (EffectParameters.Alpha < 1 && value == 1)
                    EventDispatcher.Publish(new EventData("TrOp", this, EventActionType.OnTransparentToOpaque,
                        EventCategoryType.Opacity));
                EffectParameters.Alpha = value;
            }
        }

        #endregion
    }
}