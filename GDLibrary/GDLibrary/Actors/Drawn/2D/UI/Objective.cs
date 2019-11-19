using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
   public class Objective : DrawableGameComponent
    {
        Game game;
        Texture2D texture;
        ObjectiveManager ObjectiveManager;
        int x, y, tw, th;

        public Objective(Game game, int x, int y, int tw, int th, ObjectiveManager ObjectiveManager)
         :base(game)
        {
            this.game = game;
            this.x = x;
            this.y = y;
  
            this.tw = tw;
            this.th = th;
            this.ObjectiveManager = ObjectiveManager;
        }



        public void setTexture(Texture2D value)
        {
            this.texture = value;
        }



        public void Draw2()
        {
            Vector2 scale = new Vector2(
            (float)(x / y),
            (float)(x / y));


            Vector2 translation = new Vector2(
                (float)(x/ 2) - (((x / y) * tw)) / 2,
                (float)y / 18);

            Transform2D transform = new Transform2D(translation, 0, scale, new Vector2(0, 0), new Integer2(0, 0));


            // Transform2D transform = new Transform2D(new Vector2(x,y), 0, new Vector2(1f, 1f),new Vector2(1,1),new Integer2(w,z));
            Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(0, 0, tw, th);


            UITextureObject picture = new UITextureObject("PopUp", ActorType.PopUP, StatusType.Drawn, transform, Color.White,
                SpriteEffects.None, 0, texture, rect, new Vector2(0, 0));

            this.ObjectiveManager.Add(picture);
        }

    }
}
