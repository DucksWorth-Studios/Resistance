using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
    public class BillboardEffectParameters : EffectParameters
    {
        public BillboardEffectParameters(Effect effect)
            : base(effect, null, Color.White, DefaultAlpha)
        {
        }

        //for objects with colour but no texture e.g. a glass cube
        public BillboardEffectParameters(Effect effect, Color diffusecolor, float alpha)
            : base(effect, null, diffusecolor, alpha)
        {
        }

        //for objects with texture and alpha but no specular or emmissive
        public BillboardEffectParameters(Effect effect, Texture2D texture, Color diffusecolor, float alpha)
            : base(effect, texture, diffusecolor, alpha)
        {
        }

        public override void SetParameters(Camera3D camera, BillboardOrientationParameters billboardParameters)
        {
            Effect.CurrentTechnique = Effect.Techniques[billboardParameters.Technique];
            Effect.Parameters["View"].SetValue(camera.View);
            Effect.Parameters["Projection"].SetValue(camera.ProjectionParameters.Projection);
            Effect.Parameters["Up"].SetValue(billboardParameters.Up);
            Effect.Parameters["Right"].SetValue(billboardParameters.Right);
            Effect.Parameters["DiffuseColor"].SetValue(DiffuseColor.ToVector4());
            Effect.Parameters["DiffuseTexture"].SetValue(Texture);
            Effect.Parameters["Alpha"].SetValue(Alpha);

            //animation specific parameters
            Effect.Parameters["IsScrolling"].SetValue(billboardParameters.IsScrolling);
            Effect.Parameters["scrollRate"].SetValue(billboardParameters.scrollValue);
            Effect.Parameters["IsAnimated"].SetValue(billboardParameters.IsAnimated);
            Effect.Parameters["InverseFrameCount"].SetValue(billboardParameters.inverseFrameCount);
            Effect.Parameters["CurrentFrame"].SetValue(billboardParameters.currentFrame);


            base.SetParameters(camera);
        }

        public override void SetWorld(Matrix world)
        {
            Effect.Parameters["World"].SetValue(world);
        }

        public override EffectParameters GetDeepCopy()
        {
            return new BillboardEffectParameters(Effect, //shallow - a reference
                Texture, //shallow - a reference
                DiffuseColor, //deep
                Alpha); //deep
        }

        public override object Clone()
        {
            return GetDeepCopy();
        }
    }
}