using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
    public class DualTextureEffectParameters : EffectParameters
    {
        //used to create the simplest instance of the class - fields will be set by each instanciating object - see Main::InitializeEffects()
        public DualTextureEffectParameters(Effect effect)
            : base(effect)
        {
        }

        public DualTextureEffectParameters(Effect effect, Texture2D texture, Color diffuseColor, float alpha,
            Texture2D texture2)
            : base(effect, texture, diffuseColor, alpha)
        {
            Initialize(texture2);

            //store original values in case of reset
            OriginalEffectParameters = new DualTextureEffectParameters(effect);
            OriginalEffectParameters.Initialize(texture2);
        }

        protected void Initialize(Texture2D texture2)
        {
            Texture2 = texture2;
        }

        protected override void Reset()
        {
            base.Reset();
            Initialize(OriginalEffectParameters.Texture2);
        }

        public override void SetParameters(Camera3D camera)
        {
            var bEffect = Effect as DualTextureEffect;
            bEffect.View = camera.View;
            bEffect.Projection = camera.Projection;
            bEffect.DiffuseColor = DiffuseColor.ToVector3();
            bEffect.Alpha = Alpha;

            if (Texture != null && Texture2 != null)
            {
                bEffect.Texture = Texture;
                bEffect.Texture2 = Texture2;
            }

            base.SetParameters(camera);
        }

        public override void SetWorld(Matrix world)
        {
            (Effect as DualTextureEffect).World = world;
        }

        //add equals, gethashcode...

        public override EffectParameters GetDeepCopy()
        {
            return new DualTextureEffectParameters(Effect, //shallow - a reference
                Texture, //shallow - a reference
                DiffuseColor, //deep
                Alpha, //deep
                Texture2); //shallow - a reference
        }

        public override object Clone()
        {
            return GetDeepCopy();
        }

        #region Fields

        //reset

        #endregion

        #region Properties

        public Texture2D Texture2 { get; set; }

        public new DualTextureEffectParameters OriginalEffectParameters { get; }

        #endregion
    }
}