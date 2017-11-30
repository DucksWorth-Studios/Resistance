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

        public override void SetParameters(Camera3D camera, BillboardParameters billboardParameters)
        {
            this.Effect.CurrentTechnique = this.Effect.Techniques[billboardParameters.Technique];
            this.Effect.Parameters["View"].SetValue(camera.View);
            this.Effect.Parameters["Projection"].SetValue(camera.ProjectionParameters.Projection);
            this.Effect.Parameters["Up"].SetValue(billboardParameters.Up);
            this.Effect.Parameters["DiffuseTexture"].SetValue(this.Texture);
            this.Effect.Parameters["Alpha"].SetValue(this.Alpha);

            if (billboardParameters.BillboardType == BillboardType.Normal)
            {
                this.Effect.Parameters["Right"].SetValue(billboardParameters.Right);
            }

            if (billboardParameters.IsScrolling)
            {
                this.Effect.Parameters["IsScrolling"].SetValue(billboardParameters.IsScrolling);
                this.Effect.Parameters["scrollRate"].SetValue(billboardParameters.scrollValue);
            }
            else
            {
                this.Effect.Parameters["IsScrolling"].SetValue(false);
            }

            if (billboardParameters.IsAnimated)
            {
                this.Effect.Parameters["IsAnimated"].SetValue(billboardParameters.IsAnimated);
                this.Effect.Parameters["InverseFrameCount"].SetValue(billboardParameters.inverseFrameCount);
                this.Effect.Parameters["CurrentFrame"].SetValue(billboardParameters.currentFrame);
            }
            else
            {
                this.Effect.Parameters["IsAnimated"].SetValue(false);
            }

            base.SetParameters(camera);
        }

        public override void SetWorld(Matrix world)
        {
            this.Effect.Parameters["World"].SetValue(world);
        }
    }
}
