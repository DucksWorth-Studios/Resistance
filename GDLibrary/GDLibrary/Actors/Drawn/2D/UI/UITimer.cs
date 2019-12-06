using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
    public class UITimer : UITextObject
    {
        private TimerUtility timer;

        public UITimer(string id, Transform2D transform, Color color, SpriteEffects spriteEffects, 
            float layerDepth, SpriteFont spriteFont, TimerUtility timer) : 
            base(id, ActorType.UIDynamicText, StatusType.Drawn, transform, color, spriteEffects, layerDepth, timer.ToString(), spriteFont)
        {
            this.timer = timer;
        }

        public UITimer(Transform2D transform, float layerDepth, SpriteFont spriteFont, TimerUtility timer) :
            base("Default Timer", ActorType.UIDynamicText, StatusType.Drawn | StatusType.Update, 
                transform, Color.WhiteSmoke, SpriteEffects.None, layerDepth, 
                timer.ToString(), spriteFont)
        {
            this.timer = timer;
        }

        public override void Update(GameTime gameTime)
        {
            Text = timer.ToString();
            base.Update(gameTime);
        }
    }
}
