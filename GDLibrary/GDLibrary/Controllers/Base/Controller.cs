/*
Function: 		Parent class for all controllers which adds id and controller type
Author: 		NMCG
Version:		1.0
Date Updated:	17/8/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class Controller : IController
    {
        public Controller(string id, ControllerType controllerType)
        {
            ID = id;
            ControllerType = controllerType;
        }


        public virtual string GetID()
        {
            return ID;
        }

        public virtual void SetActor(IActor actor)
        {
            //does nothing - no point in child classes calling this - see UIScaleLerpController::Reset()
        }

        public virtual bool SetControllerPlayStatus(PlayStatusType playStatusType)
        {
            //does nothing
            return false;
        }

        public virtual void Update(GameTime gameTime, IActor actor)
        {
            //does nothing - no point in child classes calling this.
        }

        public virtual object Clone()
        {
            return new Controller("clone - " + ID, ControllerType);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Controller;

            if (other == null)
                return false;
            if (this == other)
                return true;

            return ID.Equals(other.ID)
                   && ControllerType.Equals(other.ControllerType)
                   && base.Equals(obj);
        }

        public override int GetHashCode()
        {
            var hash = 1;
            hash = hash * 31 + ID.GetHashCode();
            hash = hash * 17 + ControllerType.GetHashCode();
            return hash;
        }

        //allows controllers to listen for events
        protected virtual void RegisterForEventHandling(EventDispatcher eventDispatcher)
        {
        }

        #region Fields

        #endregion

        #region Properties

        public string ID { get; set; }

        public ControllerType ControllerType { get; set; }

        #endregion
    }
}