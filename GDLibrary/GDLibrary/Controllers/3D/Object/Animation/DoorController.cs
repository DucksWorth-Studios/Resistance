﻿using System.Security.Policy;
using JigLibX.Collision;
using JigLibX.Geometry;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class DoorController : Controller
    {
        private bool opened = false;
        private bool opening = false;
        
        public DoorController(string id, ControllerType controllerType, EventDispatcher eventDispatcher) : base(id, controllerType)
        {
            RegisterForEventHandling(eventDispatcher);
        }
        
        #region Event Handeling

        protected override void RegisterForEventHandling(EventDispatcher eventDispatcher)
        {
            eventDispatcher.animationTriggered += OpenDoor;
            base.RegisterForEventHandling(eventDispatcher);
        }

        protected void OpenDoor(EventData eventData)
        {
            if (eventData.EventType == EventActionType.OpenDoor)
                opening = true;
        }

        #endregion

        public override void Update(GameTime gameTime, IActor actor)
        {
            CollidableObject parent = actor as CollidableObject;

            if (opening && !opened)
            {
                if (parent.Transform.Rotation.Y > 90)
                    parent.Transform.RotateAroundYBy(-1);
                else if (parent.Transform.Rotation.Y <= 90)
                {
                    opened = true;
                    opening = false;
                    
                    parent.Collision.RemoveAllPrimitives();

                    parent.Collision.AddPrimitive(new Box(new Vector3(0.2f, 0, -18), Matrix.Identity,
                            new Vector3(2f,15,15)),
                        new MaterialProperties(0.2f, 0.8f, 0.7f));
                }
            }

            base.Update(gameTime, actor);
        }
    }
}