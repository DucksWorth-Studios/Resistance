using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
    public class BasicEffectParameters : EffectParameters
    {
        //used to create the simplest instance of the class - fields will be set by each instanciating object - see Main::InitializeEffects()
        public BasicEffectParameters(Effect effect)
            : base(effect)
        {
        }

        public BasicEffectParameters(Effect effect, Texture2D texture, Color ambientColor, Color diffuseColor,
            Color specularColor,
            Color emissiveColor, int specularPower, float alpha)
            : base(effect, texture, diffuseColor, alpha)
        {
            Initialize(ambientColor, diffuseColor, specularColor, emissiveColor, specularPower);

            //store original values in case of reset
            OriginalEffectParameters = new BasicEffectParameters(effect);
            OriginalEffectParameters.Initialize(ambientColor, diffuseColor, specularColor, emissiveColor,
                specularPower);
        }

        protected void Initialize(Color ambientColor, Color diffuseColor, Color specularColor, Color emmissiveColor,
            int specularPower)
        {
            AmbientColor = ambientColor;
            SpecularColor = specularColor;
            EmissiveColor = emmissiveColor;
            this.specularPower = specularPower;
        }

        protected override void Reset()
        {
            base.Reset();
            Initialize(OriginalEffectParameters.Effect, OriginalEffectParameters.Texture,
                OriginalEffectParameters.DiffuseColor, OriginalEffectParameters.Alpha);
        }

        public override void SetParameters(Camera3D camera)
        {
            var bEffect = Effect as BasicEffect;
            bEffect.View = camera.View;
            bEffect.Projection = camera.Projection;

            bEffect.AmbientLightColor = AmbientColor.ToVector3();
            bEffect.DiffuseColor = DiffuseColor.ToVector3();
            bEffect.SpecularColor = SpecularColor.ToVector3();
            bEffect.SpecularPower = SpecularPower;
            bEffect.EmissiveColor = EmissiveColor.ToVector3();
            bEffect.Alpha = Alpha;

            //Not all models NEED a texture. Does a semi-transparent window need a texture?
            if (Texture != null)
            {
                bEffect.TextureEnabled = true;
                bEffect.Texture = Texture;
            }
            else
            {
                bEffect.TextureEnabled = false;
            }

            base.SetParameters(camera);
        }

        public override void SetWorld(Matrix world)
        {
            (Effect as BasicEffect).World = world;
        }

        //add equals, gethashcode...

        public override EffectParameters GetDeepCopy()
        {
            return new BasicEffectParameters(Effect, //shallow - a reference
                Texture, //shallow - a reference
                AmbientColor, //deep
                DiffuseColor, //deep
                SpecularColor, //deep
                EmissiveColor, //deep
                SpecularPower, //deep
                Alpha); //deep
        }

        public override object Clone()
        {
            return GetDeepCopy();
        }

        #region Fields

        //statics
        protected static readonly Color DefaultWorldAmbientColor = Color.Black;
        protected static readonly Color DefaultSpecularColor = Color.White;

        protected static readonly int
            DefaultSpecularPower = 32; //1 - 256 - higher value (e.g. > 128) is more computationally expensive  

        protected static readonly Color DefaultEmissiveColor = Color.Black;

        //color specific
        private int specularPower = DefaultSpecularPower;

        //reset

        #endregion

        #region Properties

        public Color AmbientColor { get; set; } = DefaultWorldAmbientColor;

        public Color SpecularColor { get; set; } = DefaultSpecularColor;

        public int SpecularPower
        {
            get => specularPower;
            set => specularPower = value > 0 && value <= 256 ? value : DefaultSpecularPower;
        }

        public Color EmissiveColor { get; set; } = DefaultEmissiveColor;

        public new BasicEffectParameters OriginalEffectParameters { get; }

        #endregion
    }
}