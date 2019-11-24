/*
Function: 		Renders the collision skins of any ICollisionPrimitives used in the I-CA project.

Author: 		NMCG
Version:		1.0
Date Updated:	27/11/17
Bugs:			
Fixes:			None
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
    //Draws the bounding volume for your primitive objects
    public class PrimitiveDebugDrawer : PausableDrawableGameComponent
    {
        private readonly bool bShowCDCRSurfaces;
        private readonly bool bShowZones;
        private SphereCollisionPrimitive coll;
        private readonly ManagerParameters managerParameters;

        //temp vars
        private IVertexData vertexData;
        private BasicEffect wireframeEffect;
        private Matrix world;


        public PrimitiveDebugDrawer(Game game, EventDispatcher eventDispatcher, StatusType statusType,
            ManagerParameters managerParameters, bool bShowCDCRSurfaces, bool bShowZones)
            : base(game, eventDispatcher, statusType)
        {
            this.managerParameters = managerParameters;
            this.bShowCDCRSurfaces = bShowCDCRSurfaces;
            this.bShowZones = bShowZones;
        }

        #region Event Handling

        //See MenuManager::EventDispatcher_MenuChanged to see how it does the reverse i.e. they are mutually exclusive
        protected override void EventDispatcher_MenuChanged(EventData eventData)
        {
            //did the event come from the main menu and is it a start game event
            if (eventData.EventType == EventActionType.OnStart)
                //turn on update and draw i.e. hide the menu
                StatusType = StatusType.Update | StatusType.Drawn;
            //did the event come from the main menu and is it a start game event
            else if (eventData.EventType == EventActionType.OnPause)
                //turn off update and draw i.e. show the menu since the game is paused
                StatusType = StatusType.Off;
            else if (eventData.EventType == EventActionType.OnLose)
                //turn off update and draw i.e. show the menu since the game is paused
                StatusType = StatusType.Off;
        }

        #endregion


        public override void Initialize()
        {
            //used to draw bounding volumes
            wireframeEffect = new BasicEffect(Game.GraphicsDevice);
            wireframeEffect.VertexColorEnabled = true;
            base.Initialize();
        }

        protected override void ApplyDraw(GameTime gameTime)
        {
            //set so we dont see the bounding volume through the object is encloses - disable to see result
            Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            foreach (IActor actor in managerParameters.ObjectManager.OpaqueDrawList)
                DrawSurfaceOrZonePrimitive(gameTime, actor);
            foreach (IActor actor in managerParameters.ObjectManager.TransparentDrawList)
                DrawSurfaceOrZonePrimitive(gameTime, actor);
        }

        private void DrawSurfaceOrZonePrimitive(GameTime gameTime, IActor actor)
        {
            if (actor is CollidablePrimitiveObject && bShowCDCRSurfaces)
                DrawBoundingPrimitive(gameTime, (actor as CollidablePrimitiveObject).CollisionPrimitive,
                    Color.White); //collidable object volumes are White
            else if (actor is SimpleZoneObject && bShowZones)
                DrawBoundingPrimitive(gameTime, (actor as SimpleZoneObject).CollisionPrimitive,
                    Color.Red); //collidable zone volumes are red
        }

        private void DrawBoundingPrimitive(GameTime gameTime, ICollisionPrimitive collisionPrimitive, Color color)
        {
            if (collisionPrimitive is SphereCollisionPrimitive)
            {
                var primitiveCount = 0;
                vertexData = new VertexData<VertexPositionColor>(
                    VertexFactory.GetSphereVertices(1, 10, out primitiveCount),
                    PrimitiveType.LineStrip, primitiveCount);

                coll = collisionPrimitive as SphereCollisionPrimitive;
                world = Matrix.Identity * Matrix.CreateScale(coll.BoundingSphere.Radius) *
                        Matrix.CreateTranslation(coll.BoundingSphere.Center);
                wireframeEffect.World = world;
                wireframeEffect.View = managerParameters.CameraManager.ActiveCamera.View;
                wireframeEffect.Projection =
                    managerParameters.CameraManager.ActiveCamera.ProjectionParameters.Projection;
                wireframeEffect.DiffuseColor = Color.White.ToVector3();
                wireframeEffect.CurrentTechnique.Passes[0].Apply();
                vertexData.Draw(gameTime, wireframeEffect);
            }
            else
            {
                var coll = collisionPrimitive as BoxCollisionPrimitive;
                var buffers = BoundingBoxDrawer.CreateBoundingBoxBuffers(coll.BoundingBox, GraphicsDevice);
                BoundingBoxDrawer.DrawBoundingBox(buffers, wireframeEffect, GraphicsDevice,
                    managerParameters.CameraManager.ActiveCamera);
            }
        }
    }
}