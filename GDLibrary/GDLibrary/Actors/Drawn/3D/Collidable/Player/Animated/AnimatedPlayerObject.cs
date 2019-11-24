using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SkinnedModel;

namespace GDLibrary
{
    //used internally as a unique key (take + model name) to access a specific animation (useful when lots of FBX files have same default take name i.e. Take001)
    internal class AnimationDictionaryKey
    {
        public string fileNameNoSuffix;
        public string takeName;

        public AnimationDictionaryKey(string takeName, string fileNameNoSuffix)
        {
            this.takeName = takeName;
            this.fileNameNoSuffix = fileNameNoSuffix;
        }

        //Why do we override equals and gethashcode? Clue: this.modelDictionary.ContainsKey()
        public override bool Equals(object obj)
        {
            var other = obj as AnimationDictionaryKey;
            return takeName.Equals(other.takeName) && fileNameNoSuffix.Equals(other.fileNameNoSuffix);
        }

        public override int GetHashCode()
        {
            var hash = 1;
            hash = hash * 31 + takeName.GetHashCode();
            hash = hash * 17 + fileNameNoSuffix.GetHashCode();
            return hash;
        }
    }

    public class AnimatedPlayerObject : PlayerObject
    {
        public AnimatedPlayerObject(string id, ActorType actorType, Transform3D transform,
            EffectParameters effectParameters,
            Keys[] moveKeys, float radius, float height,
            float accelerationRate, float decelerationRate, float jumpHeight,
            Vector3 translationOffset, KeyboardManager keyboardManager)
            : base(id, actorType, transform, effectParameters, null, moveKeys, radius, height, accelerationRate,
                decelerationRate, jumpHeight, translationOffset, keyboardManager)
        {
            //initialize dictionaries
            modelDictionary = new Dictionary<AnimationDictionaryKey, Model>();
            animationPlayerDictionary = new Dictionary<AnimationDictionaryKey, AnimationPlayer>();
            skinningDataDictionary = new Dictionary<AnimationDictionaryKey, SkinningData>();
        }

        public void AddAnimation(string takeName, string fileNameNoSuffix, Model model)
        {
            var key = new AnimationDictionaryKey(takeName, fileNameNoSuffix);

            //if not already added
            if (!modelDictionary.ContainsKey(key))
            {
                modelDictionary.Add(key, model);
                //read the skinning data (i.e. the set of transforms applied to each model bone for each frame of the animation)
                skinningData = model.Tag as SkinningData;

                if (skinningData == null)
                    throw new InvalidOperationException("The model [" + fileNameNoSuffix +
                                                        "] does not contain a SkinningData tag.");

                //make an animation player for the model
                animationPlayerDictionary.Add(key, new AnimationPlayer(skinningData));

                //store the skinning data for the model 
                skinningDataDictionary.Add(key, skinningData);
            }
        }

        public override void Update(GameTime gameTime)
        {
            //update player to return bone transforms for the appropriate frame in the animation
            AnimationPlayer.Update(gameTime.ElapsedGameTime, true, Matrix.Identity);
            base.Update(gameTime);
        }

        //sets the first frame for the take and file (e.g. "Take 001", "dude")
        public void SetAnimation(string takeName, string fileNameNoSuffix)
        {
            var key = new AnimationDictionaryKey(takeName, fileNameNoSuffix);

            //have we requested a different animation and is it in the dictionary?
            //first time or different animation request
            if (oldKey == null || !oldKey.Equals(key) && modelDictionary.ContainsKey(key))
            {
                //set the model based on the animation being played
                Model = modelDictionary[key];

                //retrieve the animation player
                AnimationPlayer = animationPlayerDictionary[key];

                //retrieve the skinning data
                skinningData = skinningDataDictionary[key];

                //set the skinning data in the animation player and set the player to start at the first frame for the take
                AnimationPlayer.StartClip(skinningData.AnimationClips[key.takeName]);
            }


            //store current key for comparison in next update to prevent re-setting the same animation in successive calls to SetAnimation()
            oldKey = key;
        }

        //sets the take based on what the user presses/clicks
        protected virtual void SetAnimationByInput()
        {
        }

        #region Variables

        private SkinningData skinningData;

        //stores all the data related to a character with multiple individual FBX animation files (e.g. walk.fbx, idle,fbx, run.fbx)
        private readonly Dictionary<AnimationDictionaryKey, Model> modelDictionary;
        private readonly Dictionary<AnimationDictionaryKey, AnimationPlayer> animationPlayerDictionary;
        private readonly Dictionary<AnimationDictionaryKey, SkinningData> skinningDataDictionary;
        private AnimationDictionaryKey oldKey;

        #endregion

        #region Properties

        public AnimationStateType AnimationState { get; set; }

        public AnimationPlayer AnimationPlayer { get; private set; }

        #endregion
    }
}