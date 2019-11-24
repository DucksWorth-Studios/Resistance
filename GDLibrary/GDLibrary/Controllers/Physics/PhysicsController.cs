/*
Function: 		Sums all forces and torques for all collidable bodies in the world
Author: 		NMCG
Version:		1.0
Date Updated:	27/10/17
Bugs:			
Fixes:			None
*/

using System.Collections.Generic;
using JigLibX.Physics;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class PhysicsController : JigLibX.Physics.Controller
    {
        #region Inherited from Controller

        public override void UpdateController(float elapsedTime)
        {
            // Apply pending forces
            while (forces.Count > 0)
            {
                var force = forces.Dequeue();
                switch (force.coordinateSystem)
                {
                    case CoordinateSystem.LocalCoordinates:
                    {
                        force.body.AddBodyForce(force.force, force.position);
                    }
                        break;
                    case CoordinateSystem.WorldCoordinates:
                    {
                        force.body.AddWorldForce(force.force, force.position);
                    }
                        break;
                }
            }


            // Apply pending torques
            while (torques.Count > 0)
            {
                var torque = torques.Dequeue();
                switch (torque.coordinateSystem)
                {
                    case CoordinateSystem.LocalCoordinates:
                    {
                        torque.body.AddBodyTorque(torque.torque);
                    }
                        break;
                    case CoordinateSystem.WorldCoordinates:
                    {
                        torque.body.AddWorldTorque(torque.torque);
                    }
                        break;
                }
            }
        }

        #endregion

        #region Properties

        public enum CoordinateSystem
        {
            WorldCoordinates = 0,
            LocalCoordinates = 1
        }

        public struct Force
        {
            public CoordinateSystem coordinateSystem;
            public Vector3 force;
            public Vector3 position;
            public Body body;
        }

        public struct Torque
        {
            public CoordinateSystem coordinateSystem;
            public Vector3 torque;
            public Body body;
        }

        public Queue<Force> forces = new Queue<Force>();
        internal Queue<Force> Forces => forces;

        public Queue<Torque> torques = new Queue<Torque>();
        internal Queue<Torque> Torques => torques;

        #endregion
    }
}