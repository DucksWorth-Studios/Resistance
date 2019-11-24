/*
Function: 		Draws debug information to the screen for each camera (based on ScreenManager layout)
Author: 		NMCG
Version:		1.1
Date Updated:	11/9/17
Bugs:			None
Fixes:			None
Mods:           None
*/

using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
    public class DebugDrawer : PausableDrawableGameComponent
    {
        public DebugDrawer(Game game, ManagerParameters managerParameters,
            SpriteBatch spriteBatch, SpriteFont spriteFont, Color textColor, Vector2 textHoriVertOffset,
            EventDispatcher eventDispatcher,
            StatusType statusType)
            : base(game, eventDispatcher, statusType)
        {
            this.managerParameters = managerParameters;
            this.spriteBatch = spriteBatch;
            this.spriteFont = spriteFont;
            this.textColor = textColor;
            this.textHoriVertOffset = textHoriVertOffset;

            fpsText = new StringBuilder("FPS:N/A");
            //measure string height so we know how much vertical spacing is needed for multi-line debug info
            textHeight = this.spriteFont.MeasureString(fpsText).Y;
        }

        protected override void RegisterForEventHandling(EventDispatcher eventDispatcher)
        {
            eventDispatcher.DebugChanged += EventDispatcher_DebugChanged;
            base.RegisterForEventHandling(eventDispatcher);
        }

        protected override void ApplyUpdate(GameTime gameTime)
        {
            //total time since last update to FPS text
            totalElapsedTime += gameTime.ElapsedGameTime.Milliseconds;
            frameCount++;

            //if 1 second has elapsed
            if (totalElapsedTime >= 1000)
            {
                //set the FPS text
                fpsText = new StringBuilder("FPS:" + frameCount);
                //reset the count and the elapsed time
                totalElapsedTime = 0;
                frameCount = 0;
            }
        }

        protected override void ApplyDraw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.Default, null);
            if (managerParameters.ScreenManager.ScreenType == ScreenUtility.ScreenType.SingleScreen)
                DrawDebugInfo(managerParameters.CameraManager.ActiveCamera);
            else
                foreach (var camera in managerParameters.CameraManager)
                    DrawDebugInfo(camera);
            spriteBatch.End();
        }

        private void DrawDebugInfo(Camera3D camera)
        {
            textPosition = new Vector2(camera.Viewport.X, camera.Viewport.Y) + textHoriVertOffset;
            spriteBatch.DrawString(spriteFont, "ID:" + camera.ID, textPosition, textColor,
                0, Vector2.Zero, 1, SpriteEffects.None, DefaultLayerDepth);

            textPosition.Y += textHeight;
            spriteBatch.DrawString(spriteFont, fpsText, textPosition, textColor,
                0, Vector2.Zero, 1, SpriteEffects.None, DefaultLayerDepth);

            textPosition.Y += textHeight;
            spriteBatch.DrawString(spriteFont,
                "Pos:" + MathUtility.Round(camera.Transform.Translation, 1), textPosition, textColor,
                0, Vector2.Zero, 1, SpriteEffects.None, DefaultLayerDepth);

            textPosition.Y += textHeight;
            spriteBatch.DrawString(spriteFont,
                "Look:" + MathUtility.Round(camera.Transform.Look, 1), textPosition, textColor,
                0, Vector2.Zero, 1, SpriteEffects.None, DefaultLayerDepth);

            textPosition.Y += textHeight;
            spriteBatch.DrawString(spriteFont,
                "Nr. Drawn Obj.:" + managerParameters.ObjectManager.DebugDrawCount, textPosition, textColor,
                0, Vector2.Zero, 1, SpriteEffects.None, DefaultLayerDepth);
        }

        #region Fields 

        //statics
        private static readonly float DefaultLayerDepth = 0;
        private readonly ManagerParameters managerParameters;
        private readonly SpriteFont spriteFont;
        private readonly SpriteBatch spriteBatch;
        private readonly Color textColor;
        private readonly Vector2 textHoriVertOffset;
        private int totalElapsedTime;
        private Vector2 textPosition;
        private int frameCount;
        private StringBuilder fpsText;
        private readonly float textHeight;

        #endregion

        #region Properties 

        #endregion

        #region Event Handling

        //enable dynamic show/hide of debug info
        private void EventDispatcher_DebugChanged(EventData eventData)
        {
            if (eventData.EventType == EventActionType.OnToggleDebug)
            {
                if (StatusType == StatusType.Off)
                    StatusType = StatusType.Drawn | StatusType.Update;
                else
                    StatusType = StatusType.Off;
            }
        }

        //Same as ScreenManager::EventDispatcher_MenuChanged i.e. show if we're in-game and not in-menu
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
    }
}