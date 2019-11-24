/*
Function: 		Represents the parent class for all updateable 3D AND 2D game objects (e.g. camera,pickup, player, menu text). Notice that this class doesn't 
                have any positional information (i.e. a Transform3D or Transform2D). This will allow us to use Actor as the parent for both 3D and 2D game objects (e.g. a player or a string of menu text).
Author: 		NMCG
Version:		1.0
Date Updated:	17/8/17
Bugs:			None
Fixes:			None
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class Actor : IActor, ICloneable
    {
        public Actor(string id, ActorType actorType, StatusType statusType)
        {
            ID = id;
            ActorType = actorType;
            StatusType = statusType;
        }

        public virtual ActorType GetActorType()
        {
            return ActorType;
        }

        public virtual string GetID()
        {
            return ID;
        }

        public virtual float GetAlpha()
        {
            return 1;
        }

        public virtual StatusType GetStatusType()
        {
            return StatusType;
        }

        public virtual bool Remove()
        {
            //tag for garbage collection
            if (ControllerList != null)
            {
                ControllerList.Clear();
                ControllerList = null;
            }

            return true;
        }

        public virtual void Update(GameTime gameTime)
        {
            if (ControllerList != null && (StatusType & StatusType.Update) == StatusType.Update)
                foreach (var controller in ControllerList)
                    controller.Update(gameTime, this); //you control me, update!
        }

        public object Clone()
        {
            //update for new parameter
            var clone = new Actor(ID, ActorType, StatusType);
            //remember using "as" is more flexible than a traditional typecast. Why?
            clone.GroupParameters = GroupParameters.Clone() as GroupParameters;
            return clone;
        }

        public virtual Matrix GetWorldMatrix()
        {
            return Matrix.Identity; //does nothing - see derived classes especially CollidableObject
        }

        public override bool Equals(object obj)
        {
            var other = obj as Actor;

            if (other == null)
                return false;
            if (this == other)
                return true;

            var bEquals = ID.Equals(other.ID)
                          && ActorType == other.ActorType
                          && StatusType.Equals(other.StatusType);

            //update for new parameter
            if (GroupParameters != null)
                bEquals = bEquals && GroupParameters.Equals(other.GroupParameters);

            return bEquals;
        }

        public override int GetHashCode()
        {
            var hash = 1;
            hash = hash * 7 + ID.GetHashCode();
            hash = hash * 11 + ActorType.GetHashCode();
            hash = hash * 17 + StatusType.GetHashCode();

            //update for new parameter
            if (GroupParameters != null)
                hash = hash * 17 + GroupParameters.GetHashCode();

            return hash;
        }

        #region Fields

        #endregion

        #region Properties

        public ActorType ActorType { get; set; }

        public string ID { get; set; }

        public StatusType StatusType { get; set; }

        public List<IController> ControllerList { get; private set; }

        /* We wont add this new parameter to the constructor as it will affect too many child classes.
         * Instead, if a user wishes to set the parameter, s/he can use code similar to the code below:
         * 
         *  Camera3D camera = new Camera(...);
         *  camera.GroupParameters = new GroupParameters(...);
         */

        public GroupParameters GroupParameters { get; set; }

        #endregion

        #region Controller Specific

        public virtual void AttachController(IController controller)
        {
            if (ControllerList == null)
                ControllerList = new List<IController>();
            ControllerList.Add(controller); //duplicates?
        }

        public virtual bool DetachController(IController controller)
        {
            if (ControllerList != null)
                return ControllerList.Remove(controller);

            return false;
        }

        public virtual int DetachControllers(Predicate<IController> predicate)
        {
            var findList = FindControllers(predicate);

            if (findList != null)
                foreach (var controller in findList)
                    ControllerList.Remove(controller);

            return findList.Count;
        }

        public List<IController> FindControllers(Predicate<IController> predicate)
        {
            return ControllerList.FindAll(predicate);
        }

        //allows us to set the PlayStatus for all controllers simultaneously (e.g. play all, reset all, stop all)
        public virtual void SetAllControllers(PlayStatusType playStatusType)
        {
            if (ControllerList != null)
                foreach (var controller in ControllerList)
                    controller.SetControllerPlayStatus(playStatusType);
        }

        //allows us to set the PlayStatus for all controllers with the same GROUP parameters simultaneously (e.g. "play" all controllers with a group ID of 1)
        public virtual void SetAllControllers(PlayStatusType playStatusType, Predicate<IController> predicate)
        {
            var findList = FindControllers(predicate);
            if (findList != null)
                foreach (var controller in findList)
                    controller.SetControllerPlayStatus(playStatusType);
        }

        #endregion
    }
}