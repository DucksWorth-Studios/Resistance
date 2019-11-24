/*
Function: 		Allows us to draw billboard primitives by explicitly defining the vertex data.
                Used in you I-CA project.
                 
Author: 		NMCG
Version:		1.0
Date Updated:	27/11/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class BillboardOrientationParameters
    {
        private int framesElapsed;
        public Vector2 scrollValue;
        public BillboardType BillboardType { get; set; }
        public Vector3 Up { get; set; }
        public Vector3 Right { get; set; }
        public string Technique { get; set; }

        //scrolling
        public bool IsScrolling { get; protected set; }
        public Vector2 ScrollRate { get; protected set; }

        //animation
        public bool IsAnimated { get; protected set; }
        public int currentFrame { get; protected set; }
        public int startFrame { get; protected set; }
        public int totalFrames { get; protected set; }
        public Vector2 inverseFrameCount { get; protected set; }
        public float frameDelay { get; protected set; }


        public void SetScrollRate(Vector2 scrollRate)
        {
            IsScrolling = true;
            ScrollRate = scrollRate;
            scrollValue = Vector2.Zero;
        }

        public void SetAnimationRate(int totalFrames, float frameDelay, int startFrame)
        {
            IsAnimated = true;
            this.totalFrames = totalFrames; //remember frames are arranged in a 1 x N strip, so totalFrames == N
            inverseFrameCount = new Vector2(1.0f / totalFrames, 1);
            this.frameDelay = frameDelay;
            this.startFrame = startFrame;
            currentFrame = startFrame;
        }

        public void UpdateScroll(GameTime gameTime)
        {
            var invDt = 1.0f / (1000 * gameTime.ElapsedGameTime.Milliseconds);

            scrollValue.X += ScrollRate.X * invDt;
            scrollValue.X %= 1;

            scrollValue.Y += ScrollRate.Y * invDt;
            scrollValue.Y %= 1;
        }

        public void UpdateAnimation(GameTime gameTime)
        {
            var frameRate = (int) (frameDelay * 1.0f / gameTime.ElapsedGameTime.TotalSeconds);

            if (framesElapsed >= frameRate)
            {
                framesElapsed = 0;
                currentFrame++;
                currentFrame %= totalFrames;
            }
            else
            {
                framesElapsed++;
            }
        }

        public void ResetAnimation()
        {
            framesElapsed = 0;
            currentFrame = startFrame;
        }

        public void ResetScroll()
        {
            scrollValue = Vector2.Zero;
        }
    }

    public class BillboardPrimitiveObject : PrimitiveObject
    {
        #region Variables

        #endregion

        public BillboardPrimitiveObject(string id, ActorType actorType, Transform3D transform,
            EffectParameters effectParameters, StatusType statusType, IVertexData vertexData,
            BillboardType billboardType)
            : base(id, actorType, transform, effectParameters, statusType, vertexData)
        {
            //create blank set of parameters and set type to be Normal - developer can change after instanciation - see Main::InitializeBillboards()
            BillboardOrientationParameters = new BillboardOrientationParameters();
            BillboardType = billboardType;
        }

        public override void Update(GameTime gameTime)
        {
            if (BillboardOrientationParameters.IsScrolling)
                BillboardOrientationParameters.UpdateScroll(gameTime);

            if (BillboardOrientationParameters.IsAnimated)
                BillboardOrientationParameters.UpdateAnimation(gameTime);

            base.Update(gameTime);
        }

        public new object Clone()
        {
            return new BillboardPrimitiveObject("clone - " + ID,
                ActorType, //deep
                (Transform3D) Transform.Clone(), //deep
                (BillboardEffectParameters) EffectParameters.Clone(), //deep
                StatusType, //deep
                VertexData, BillboardType); //shallow
        }

        #region Properties

        public BillboardType BillboardType
        {
            get => BillboardOrientationParameters.BillboardType;
            set
            {
                if (value == BillboardType.Normal)
                {
                    BillboardOrientationParameters.Technique = "Normal";
                    BillboardOrientationParameters.BillboardType = BillboardType.Normal;
                }
                else if (value == BillboardType.Cylindrical)
                {
                    BillboardOrientationParameters.Technique = "Cylindrical";
                    BillboardOrientationParameters.BillboardType = BillboardType.Cylindrical;
                }
                else
                {
                    BillboardOrientationParameters.Technique = "Spherical";
                    BillboardOrientationParameters.BillboardType = BillboardType.Spherical;
                }

                BillboardOrientationParameters.Up = Transform.Up;
                BillboardOrientationParameters.Right = Transform.Right;
            }
        }

        public BillboardOrientationParameters BillboardOrientationParameters { get; }

        #endregion
    }
}