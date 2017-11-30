using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
    public class SkinnedModelEffectParameters : BasicEffectParameters
    {
        public SkinnedModelEffectParameters(Effect effect)
           : base(effect)
        {


        }
        public SkinnedModelEffectParameters(Effect effect, Texture2D texture, Color ambientColor, Color diffuseColor, Color specularColor,
                    Color emissiveColor, int specularPower, float alpha)
            : base(effect, texture, ambientColor, diffuseColor, specularColor, emissiveColor, specularPower, alpha)
        {

        }

        public override void SetParameters(Camera3D camera, Matrix[] bones)
        {
            SkinnedEffect bEffect = this.Effect as SkinnedEffect;
            bEffect.View = camera.View;
            bEffect.Projection = camera.Projection;

            bEffect.SetBoneTransforms(bones);
            bEffect.AmbientLightColor = this.AmbientColor.ToVector3();
            bEffect.DiffuseColor = this.DiffuseColor.ToVector3();
            bEffect.SpecularColor = this.SpecularColor.ToVector3();
            bEffect.SpecularPower = this.SpecularPower;
            bEffect.EmissiveColor = this.EmissiveColor.ToVector3();
            bEffect.Alpha = this.Alpha;

            //apply baked texture from 3DS Max when texture == null, otherwise use the texture passed by the developer in effectParameters.
            if (this.Texture != null)
                bEffect.Texture = this.Texture;

            base.SetParameters(camera);
        }

        public override void SetWorld(Matrix world)
        {
            (this.Effect as SkinnedEffect).World = world;
        }

        //add equals, gethashcode...

        public override object Clone()
        {
            return new SkinnedModelEffectParameters(this.Effect, //shallow - a reference
                            this.Texture, //shallow - a reference
                            this.AmbientColor, //deep
                            this.DiffuseColor,//deep
                            this.SpecularColor,//deep
                            this.EmissiveColor,//deep
                            this.SpecularPower,//deep
                            this.Alpha);//deep
        }

    }
}
