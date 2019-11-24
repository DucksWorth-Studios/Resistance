using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace GDLibrary
{
    public class SoundManager : PausableGameComponent, IDisposable
    {
        //See http://rbwhitaker.wikidot.com/audio-tutorials
        //See http://msdn.microsoft.com/en-us/library/ff827590.aspx
        //See http://msdn.microsoft.com/en-us/library/dd940200.aspx

        public SoundManager(Game game, EventDispatcher eventDispatcher, StatusType statusType,
            string folderPath, string audioEngineStr, string waveBankStr, string soundBankStr)
            : base(game, eventDispatcher, statusType)
        {
            audioEngine = new AudioEngine(@"" + folderPath + "/" + audioEngineStr);
            waveBank = new WaveBank(audioEngine, @"" + folderPath + "/" + waveBankStr);
            soundBank = new SoundBank(audioEngine, @"" + folderPath + "/" + soundBankStr);
            cueList3D = new List<Cue3D>();
            playSet3D = new HashSet<string>();
            audioListener = new AudioListener();
        }

        #region Properties

        public float Volume
        {
            get => volume;
            set => volume = value >= 0 && value <= 1 ? value : DefaultVolume;
        }

        #endregion

        /*************** Play, Pause, Resume, and Stop 2D sound cues ***************/

        // Plays a 2D cue e.g menu, game music etc
        public void PlayCue(string cueName)
        {
            if (!playSet3D.Contains(cueName)
            ) //if we have not already been asked to play this in the current update loop then play it
            {
                var cue = soundBank.GetCue(cueName);
                cue.Play();
            }
        }

        //pauses a 2D cue
        public void PauseCue(string cueName)
        {
            var cue = soundBank.GetCue(cueName);
            if (cue != null && cue.IsPlaying)
                cue.Pause();
        }

        //resumes a paused 2D cue
        public void ResumeCue(string cueName)
        {
            var cue = soundBank.GetCue(cueName);
            if (cue != null && cue.IsPaused)
                cue.Resume();
        }

        //stop a 2D cue - AudioStopOptions: AsAuthored and Immediate
        public void StopCue(string cueName, AudioStopOptions audioStopOptions)
        {
            var cue = soundBank.GetCue(cueName);
            if (cue != null && cue.IsPlaying) cue.Stop(audioStopOptions);
        }

        /*************** Play, Pause, Resume, and Stop 3D sound cues ***************/

        // Plays a cue to be heard from the perspective of a player or camera in the game i.e. in 3D
        public void Play3DCue(string cueName, AudioEmitter audioEmitter)
        {
            var sound = new Cue3D();

            sound.Cue = soundBank.GetCue(cueName);
            if (!playSet3D.Contains(cueName)
            ) //if we have not already been asked to play this in the current update loop then play it
            {
                sound.Emitter = audioEmitter;
                sound.Cue.Apply3D(audioListener, audioEmitter);
                sound.Cue.Play();
                cueList3D.Add(sound);
                playSet3D.Add(cueName);
            }
        }

        //pause a 3D cue
        public void Pause3DCue(string cueName)
        {
            var cue3D = Get3DCue(cueName);
            if (cue3D != null && cue3D.Cue.IsPlaying)
                cue3D.Cue.Pause();
        }

        //resumes a paused 3D cue
        public void Resume3DCue(string cueName)
        {
            var cue3D = Get3DCue(cueName);
            if (cue3D != null && cue3D.Cue.IsPaused)
                cue3D.Cue.Resume();
        }

        //stop a 3D cue - AudioStopOptions: AsAuthored and Immediate
        public void Stop3DCue(string cueName, AudioStopOptions audioStopOptions)
        {
            var cue3D = Get3DCue(cueName);
            if (cue3D != null)
            {
                cue3D.Cue.Stop(audioStopOptions);
                playSet3D.Remove(cue3D.Cue.Name);
                cueList3D.Remove(cue3D);
            }
        }

        //stops all 3D cues - AudioStopOptions: AsAuthored and Immediate
        public void StopAll3DCues(AudioStopOptions audioStopOptions)
        {
            foreach (var cue3D in cueList3D)
            {
                cue3D.Cue.Stop(audioStopOptions);
                cueList3D.Remove(cue3D);
                playSet3D.Remove(cue3D.Cue.Name);
            }
        }

        //retrieves a 3D cue from the list of currently active cues
        public Cue3D Get3DCue(string name)
        {
            foreach (var cue3D in cueList3D)
                if (cue3D.Cue.Name.Equals(name))
                    return cue3D;
            return null;
        }

        //we can control the volume for each category in the sound bank (i.e. diegetic and non-diegetic)
        public void SetVolume(float newVolume, string soundCategoryStr)
        {
            try
            {
                var soundCategory = audioEngine.GetCategory(soundCategoryStr);
                if (soundCategory != null)
                {
                    //requested volume will be in appropriate range (0-1)
                    volume = MathHelper.Clamp(newVolume, 0, 1);
                    soundCategory.SetVolume(volume);
                }
            }
            catch (InvalidOperationException e)
            {
                Debug.WriteLine("Does category (soundCategoryStr) exist in your Xact file?");
            }
        }

        public void ChangeVolume(float deltaVolume, string soundCategoryStr)
        {
            try
            {
                var soundCategory = audioEngine.GetCategory(soundCategoryStr);
                if (soundCategory != null)
                {
                    //requested volume will be in appropriate range (0-1)
                    volume = MathHelper.Clamp(volume + deltaVolume, 0, 1);
                    soundCategory.SetVolume(volume);
                }
            }
            catch (InvalidOperationException e)
            {
                Debug.WriteLine("Does category (soundCategoryStr) exist in your Xact file?");
            }
        }


        //Called by the listener to update relative positions (i.e. everytime the 1st Person camera moves it should call this method so that the 3D sounds heard reflect the new camera position)
        public void UpdateListenerPosition(Vector3 position)
        {
            audioListener.Position = position;
        }

        // Pause all playing sounds
        public void PauseAll()
        {
            foreach (var cue in cueList3D) cue.Cue.Pause();
        }

        public void ResumeAll()
        {
            foreach (var cue in cueList3D) cue.Cue.Resume();
        }

        public override void Update(GameTime gameTime)
        {
            audioEngine.Update();
            for (var i = 0; i < cueList3D.Count; i++)
                if (cueList3D[i].Cue.IsPlaying)
                {
                    cueList3D[i].Cue.Apply3D(audioListener, cueList3D[i].Emitter);
                }
                else if (cueList3D[i].Cue.IsStopped)
                {
                    playSet3D.Remove(cueList3D[i].Cue.Name);
                    cueList3D.RemoveAt(i--);
                }

            base.Update(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            audioEngine.Dispose();
            soundBank.Dispose();
            waveBank.Dispose();
            base.Dispose(disposing);
        }

        #region Fields

        //statics 
        private static readonly float DefaultVolume = 0.5f;

        protected AudioEngine audioEngine;
        protected WaveBank waveBank;
        protected SoundBank soundBank;

        protected List<Cue3D> cueList3D;
        protected HashSet<string> playSet3D;

        protected AudioListener audioListener;
        protected List<string> categories;
        private float volume;

        #endregion


        #region Event Handling

        protected override void RegisterForEventHandling(EventDispatcher eventDispatcher)
        {
            eventDispatcher.GlobalSoundChanged += EventDispatcher_GlobalSoundChanged;
            eventDispatcher.Sound3DChanged += EventDispatcher_Sound3DChanged;
            eventDispatcher.Sound2DChanged += EventDispatcher_Sound2DChanged;
            base.RegisterForEventHandling(eventDispatcher);
        }

        protected virtual void EventDispatcher_GlobalSoundChanged(EventData eventData)
        {
            if (eventData.EventType == EventActionType.OnMute)
            {
                //any 2D sounds
                SoundEffect.MasterVolume = 0;
                //3d sounds
                //construct event to pass in volume (e.g. 0.5f) and category (game_sound_effects) that you group the sounds by in XACT
                //See https://www.youtube.com/watch?v=eG-FW6RAyHU
                var volume = (float) eventData.AdditionalParameters[0];
                var soundCategory = (string) eventData.AdditionalParameters[1];
                SetVolume(volume, soundCategory);
            }
            else if (eventData.EventType == EventActionType.OnUnMute)
            {
                //any 2D sounds
                SoundEffect.MasterVolume = DefaultVolume;
                //3d sounds
                var volume = (float) eventData.AdditionalParameters[0];
                var soundCategory = (string) eventData.AdditionalParameters[1];
                SetVolume(volume, soundCategory);
            }
            else if (eventData.EventType == EventActionType.OnVolumeChange)
            {
                //any 2D sounds
                var volumeDelta = (float) eventData.AdditionalParameters[0];
                SoundEffect.MasterVolume = MathHelper.Clamp(SoundEffect.MasterVolume + volumeDelta, 0, 1);
                //3d sounds
                var soundCategory = (string) eventData.AdditionalParameters[1];
                ChangeVolume(volumeDelta, soundCategory);
            }
        }

        protected virtual void EventDispatcher_Sound3DChanged(EventData eventData)
        {
            //control 3D sounds through events
            if (eventData.EventType != EventActionType.OnStopAll)
            {
                var cueName = eventData.AdditionalParameters[0] as string;

                if (eventData.EventType == EventActionType.OnPlay)
                {
                    //what object generated the sound?
                    var audioEmitter = eventData.AdditionalParameters[1] as AudioEmitter;
                    Play3DCue(cueName, audioEmitter);
                }
                else if (eventData.EventType == EventActionType.OnPause)
                {
                    Pause3DCue(cueName);
                }
                else if (eventData.EventType == EventActionType.OnResume)
                {
                    Resume3DCue(cueName);
                }
                else if (eventData.EventType == EventActionType.OnStop)
                {
                    Stop3DCue(cueName, AudioStopOptions.Immediate);
                }
            }
            else //OnStopAll - notice that the AdditionalParameters[0] parameter is now used to send the stop option (vs. above where it sent the cue name). be careful!
            {
                //since we can only pass refereneces in AdditionalParameters and AudioStopOption is an enum (i.e. a primitive type) then we need to hack the code a little
                if ((int) eventData.AdditionalParameters[0] == 0)
                    StopAll3DCues(AudioStopOptions.Immediate);
                else
                    StopAll3DCues(AudioStopOptions.AsAuthored);
            }
        }

        protected virtual void EventDispatcher_Sound2DChanged(EventData eventData)
        {
            var cueName = eventData.AdditionalParameters[0] as string;

            if (eventData.EventType == EventActionType.OnPlay)
            {
                PlayCue(cueName);
            }
            else if (eventData.EventType == EventActionType.OnPause)
            {
                PauseCue(cueName);
            }
            else if (eventData.EventType == EventActionType.OnResume)
            {
                ResumeCue(cueName);
            }
            else //OnStop
            {
                //since we can only pass refereneces in AdditionalParameters and AudioStopOption is an enum (i.e. a primitive type) then we need to hack the code a little
                if ((int) eventData.AdditionalParameters[1] == 0)
                    StopCue(cueName, AudioStopOptions.Immediate);
                else
                    StopCue(cueName, AudioStopOptions.AsAuthored);
            }


            //if (totalElapsedTime > 1)
            //{
            //    totalElapsedTime = 0;
            //    //object[] additionalParameters = { 1 };
            //    //EventDispatcher.Publish(new EventData(EventActionType.OnStop, EventCategoryType.Sound2D, additionalParameters));
            //}
            //else
            //    totalElapsedTime += gameTime.ElapsedTime.Milliseconds;
        }

        //Do we want sound to play in the menu? In this case, we should remove this code and set statusType to Update in the constructor.
        protected override void EventDispatcher_MenuChanged(EventData eventData)
        {
            //did the event come from the main menu and is it a start game event
            if (eventData.EventType == EventActionType.OnStart)
                //turn on update and draw i.e. hide the menu
                StatusType = StatusType.Update;
            //did the event come from the main menu and is it a pause game event
            else if (eventData.EventType == EventActionType.OnPause)
                //turn off update and draw i.e. show the menu since the game is paused
                StatusType = StatusType.Off;
            else if (eventData.EventType == EventActionType.OnLose)
                //turn off update and draw i.e. show the menu since the game is paused
                StatusType = StatusType.Off;
        }

        #endregion
    }
}