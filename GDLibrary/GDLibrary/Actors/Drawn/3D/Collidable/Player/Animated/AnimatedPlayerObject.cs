using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SkinnedModel;
using System;
using System.Collections.Generic;

namespace GDLibrary
{
    public class AnimatedPlayerObject : PlayerObject
    {
        #region Variables
        private AnimationPlayer animationPlayer;
        private SkinningData skinningData;
        private string startAnimationName;
        //stores all the data related to a character with multiple individual FBX animation files (e.g. walk.fbx, idle,fbx, run.fbx)
        private Dictionary<string, Model> modelDictionary;
        private Dictionary<string, AnimationPlayer> animationPlayerDictionary;
        private Dictionary<string, SkinningData> skinningDataDictionary;
        #endregion

        #region Properties
        public AnimationPlayer AnimationPlayer
        {
            get
            {
                return this.animationPlayer;
            }
        }
        #endregion

        public AnimatedPlayerObject(string id, ActorType actorType, Transform3D transform,
            EffectParameters effectParameters, Keys[] moveKeys, float radius, float height, 
            float accelerationRate, float decelerationRate, float jumpHeight,
            Vector3 translationOffset, KeyboardManager keyboardManager, 
            string startAnimationName, Dictionary<string, Model> modelDictionary)
            : base(id, actorType, transform, effectParameters, null, moveKeys, radius, height, accelerationRate, decelerationRate, jumpHeight, translationOffset, keyboardManager)
        {
            //set initial animation played when player instanciated
            this.startAnimationName = startAnimationName;

            this.modelDictionary = modelDictionary;

            //initialize both dictionaries
            this.animationPlayerDictionary = new Dictionary<string, AnimationPlayer>();
            this.skinningDataDictionary = new Dictionary<string, SkinningData>();

            //load animation player with initial take e.g. idle
            LoadAnimationPlayers(modelDictionary);

            //set initial clip
            SetClip(startAnimationName);
        }

        public void LoadAnimationPlayers(Dictionary<string, Model> modelDictionary)
        {
            foreach(string animationName in modelDictionary.Keys)
            {
                // Look up our custom skinning information.
                skinningData = modelDictionary[animationName].Tag as SkinningData;

                if (skinningData == null)
                    throw new InvalidOperationException ("The model [" + animationName + "] does not contain a SkinningData tag.");

                // Create an animation player, and start decoding an animation clip.
                animationPlayerDictionary.Add(animationName, new AnimationPlayer(skinningData));

                //Store the skinning data for the particular animation for the model (e.g. walk.fbs, idle.fbx)
                skinningDataDictionary.Add(animationName, skinningData);
            }
        }

        public override void Update(GameTime gameTime)
        {
            //update player to return bone transforms for the appropriate frame in the animation
            animationPlayer.Update(gameTime.ElapsedGameTime, true, Matrix.Identity);
            base.Update(gameTime);
        }

        //call to change animation clip during gameplay
        public void SetClip(string animationName)
        {
            //set the model based on the animation being played
            this.Model = modelDictionary[animationName];
            //retrieve the animation player
            animationPlayer = animationPlayerDictionary[animationName];
            //retrieve the skinning data
            skinningData = skinningDataDictionary[animationName];

            //set the skinning data in the animation player and set the player to start at the first frame
            animationPlayer.StartClip(skinningData.AnimationClips[animationName]);
        }
    }
}
