/*
Function: 		Encapsulates the effect, texture, color (ambient, diffuse, specular, emmissive), and alpha fields for any drawn 3D object.
Author: 		NMCG
Version:		1.0
Date Updated:	22/10/17
Bugs:			None
Fixes:			None
*/

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
    public class EffectParameters : IEffectParameters
    {
        //used to set originalEffectParameters only
        private EffectParameters()
        {
        }

        //used to create the simplest instance of the class - fields will be set by each instanciating object - see Main::InitializeEffects()
        public EffectParameters(Effect effect)
            : this(effect, null, Color.White, DefaultAlpha)
        {
        }

        //for objects with colour but no texture e.g. a glass cube
        public EffectParameters(Effect effect, Color diffusecolor, float alpha)
            : this(effect, null, diffusecolor, alpha)
        {
        }

        //for objects with texture and alpha but no specular or emmissive
        public EffectParameters(Effect effect, Texture2D texture, Color diffusecolor, float alpha)
        {
            Initialize(effect, texture, DiffuseColor, alpha);

            //store original values in case of reset
            OriginalEffectParameters = new EffectParameters();
            OriginalEffectParameters.Initialize(effect, texture, DiffuseColor, alpha);
        }

        public virtual void SetParameters(Camera3D camera)
        {
            //apply or serialise the variables above to the GFX card
            Effect.CurrentTechnique.Passes[0].Apply();
        }

        public virtual void SetWorld(Matrix world)
        {
        }

        public virtual object Clone()
        {
            return GetDeepCopy();
        }

        protected void Initialize(Effect effect, Texture2D texture, Color diffuseColor, float alpha)
        {
            Effect = effect;
            if (texture != null)
                Texture = texture;

            //use Property to ensure values are inside correct ranges
            Alpha = alpha;
        }

        protected virtual void Reset()
        {
            Initialize(OriginalEffectParameters.Effect,
                OriginalEffectParameters.Texture,
                OriginalEffectParameters.DiffuseColor,
                OriginalEffectParameters.Alpha);
        }

        //used by animated models
        public virtual void SetParameters(Camera3D camera, Matrix[] bones)
        {
            //apply or serialise the variables above to the GFX card
            Effect.CurrentTechnique.Passes[0].Apply();
        }

        //used by billboards
        public virtual void SetParameters(Camera3D camera, BillboardOrientationParameters billboardParameters)
        {
        }

        //add equals, gethashcode...

        public virtual EffectParameters GetDeepCopy()
        {
            return new EffectParameters(Effect, //shallow - a reference
                Texture, //shallow - a reference
                DiffuseColor, //deep
                alpha); //deep
        }

        #region Fields

        //statics
        protected static readonly int DefaultAlphaRoundPrecision = 3; //rounding on alpha setter
        protected static readonly float DefaultAlpha = 1;

        //shader reference

        //texture

        //color specific
        //defaults in case the developer forgets to set these values when adding a model object (or child object).
        //setting these values prevents us from seeing only a black surface (i.e. no texture, no color) or no object at all (alpha = 0).
        private float alpha = DefaultAlpha;

        //reset

        #endregion

        #region Properties

        public Effect Effect { get; set; }

        public Texture2D Texture { get; set; }

        public Color DiffuseColor { get; set; } = Color.White;

        public float Alpha
        {
            get => alpha;
            set
            {
                if (value < 0)
                    alpha = 0;
                else if (value > 1)
                    alpha = 1;
                else
                    alpha = (float) Math.Round(value, DefaultAlphaRoundPrecision);
            }
        }

        public EffectParameters OriginalEffectParameters { get; }

        #endregion
    }
}