﻿/*
Function: 		Represent a message broker for events received and routed through the game engine. 
                Allows the receiver to receive event messages with no reference to the publisher - decouples the sender and receiver.
Author: 		NMCG
Version:		1.0
Date Updated:	11/10/17
Bugs:			None
Fixes:			None
Comments:       Should consider making this class a Singleton because of the static message Stack - See https://msdn.microsoft.com/en-us/library/ff650316.aspx
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class EventDispatcher : GameComponent
    {
        //See Queue doc - https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.queue-1?view=netframework-4.7.1
        private static Queue<EventData> queue; //stores events in arrival sequence
        private static HashSet<EventData> uniqueSet; //prevents the same event from existing in the stack for a single update cycle (e.g. when playing a sound based on keyboard press)
      

        //a delegate is basically a list - the list contains a pointer to a function - this function pointer comes from the object wishing to be notified when the event occurs.
        public delegate void CameraEventHandler(EventData eventData);
        public delegate void MenuEventHandler(EventData eventData);
        public delegate void ScreenEventHandler(EventData eventData);
        public delegate void OpacityEventHandler(EventData eventData);
        public delegate void AddActorEventHandler(EventData eventData);
        public delegate void RemoveActorEventHandler(EventData eventData);
        public delegate void PlayerEventHandler(EventData eventData);
        public delegate void PlayerWinEventHandler(EventData eventData);
        public delegate void GlobalSoundEventHandler(EventData eventData);
        public delegate void Sound3DEventHandler(EventData eventData);
        public delegate void Sound2DEventHandler(EventData eventData);
        public delegate void ObjectPickingEventHandler(EventData eventData);
        public delegate void MouseEventHandler(EventData eventData);
        public delegate void VideoEventHandler(EventData eventData);
        public delegate void DebugEventHandler(EventData eventData);
        public delegate void InteractHandler(EventData eventData);
        public delegate void mouseLockingHandler(EventData eventData);
        public delegate void PuzzleHandler(EventData eventData);
        public delegate void RiddleHandler(EventData eventData);
        public delegate void RiddleAnswerHandler(EventData eventData);
        public delegate void PopUpHandler(EventData eventData);
        public delegate void CutsceneHandler(EventData eventData);
        public delegate void AnimationHandler(EventData eventData);
        public delegate void ObjectiveHandler(EventData eventData);
        public delegate void ResetHandler(EventData eventData);
        public delegate void VolumeHandler(EventData eventData);
        public delegate void MessageHandler(EventData eventData);
        public delegate void StartSoundHandler(EventData eventData);


        //an event is either null (not yet happened) or non-null - when the event occurs the delegate reads through its list and calls all the listening functions
        public event CameraEventHandler CameraChanged;
        public event MenuEventHandler MenuChanged;
        public event ScreenEventHandler ScreenChanged;
        public event OpacityEventHandler OpacityChanged;
        public event AddActorEventHandler AddActorChanged;
        public event RemoveActorEventHandler RemoveActorChanged;
        public event PlayerEventHandler PlayerChanged;
        public event PlayerWinEventHandler PlayerWinChanged;
        public event GlobalSoundEventHandler GlobalSoundChanged;
        public event Sound3DEventHandler Sound3DChanged;
        public event Sound2DEventHandler Sound2DChanged;
        public event ObjectPickingEventHandler ObjectPickChanged;
        public event MouseEventHandler MouseChanged;
        public event VideoEventHandler VideoChanged;
        public event DebugEventHandler DebugChanged;
        public event InteractHandler InteractChanged;
        public event mouseLockingHandler lockChanged;
        public event PuzzleHandler PuzzleChanged;
        public event RiddleHandler RiddleChanged;
        public event RiddleAnswerHandler RiddleAnswerChanged;
        public event PopUpHandler PopUpChanged;
        public event CutsceneHandler cutsceneChanged;
        public event AnimationHandler animationTriggered;
        public event ObjectiveHandler ObjectiveChanged;
        public event ResetHandler Reset;
        public event VolumeHandler VolumeChanged;
        public event VolumeHandler MessageChanged;
        public event StartSoundHandler StartSoundChanged;

        public EventDispatcher(Game game, int initialSize)
            : base(game)
        {
            queue = new Queue<EventData>(initialSize);
            uniqueSet = new HashSet<EventData>(new EventDataEqualityComparer());
        }
        public static void Publish(EventData eventData)
        {
            //this prevents the same event being added multiple times within a single update e.g. 10x bell ring sounds
            if (!uniqueSet.Contains(eventData))
            {
                queue.Enqueue(eventData);
                uniqueSet.Add(eventData);
            }
        }

        EventData eventData;
        public override void Update(GameTime gameTime)
        { 
            for (int i = 0; i < queue.Count; i++)
            {
                eventData = queue.Dequeue();
                Process(eventData);
                uniqueSet.Remove(eventData);
            }

            //Update() method can be pre-empted and not complete processing all events so we need to store for next update
            //queue.Clear();
            //uniqueSet.Clear();

            base.Update(gameTime);
        }

        private void Process(EventData eventData)
        {
            //Switch - See https://msdn.microsoft.com/en-us/library/06tc147t.aspx
            //one case for each category type
            switch (eventData.EventCategoryType)
            {
                case EventCategoryType.Camera:
                    OnCamera(eventData);
                    break;

                case EventCategoryType.MainMenu:
                    OnMenu(eventData);
                    break;

                //add a case to handle the On...() method for each type
                case EventCategoryType.Screen:
                    OnScreen(eventData);
                    break;

                case EventCategoryType.Opacity:
                    OnOpacity(eventData);
                    break;

                case EventCategoryType.SystemAdd:
                    OnAddActor(eventData);
                    break;

                case EventCategoryType.SystemRemove:
                    OnRemoveActor(eventData);
                    break;

                case EventCategoryType.Player:
                    OnPlayer(eventData);
                    break;

                case EventCategoryType.Win:
                    OnPlayerWin(eventData);
                    break;

                case EventCategoryType.Debug:
                    OnDebug(eventData);
                    break;

                case EventCategoryType.Sound3D:
                    OnSound3D(eventData);
                    break;

                case EventCategoryType.SoundStart:
                    OnSound2D(eventData);
                    break;

                case EventCategoryType.GlobalSound:
                    OnGlobalSound(eventData);
                    break;

                case EventCategoryType.ObjectPicking:
                    OnObjectPicking(eventData);
                    break;

                case EventCategoryType.Mouse:
                    OnMouse(eventData);
                    break;

                case EventCategoryType.Video:
                    OnVideo(eventData);
                    break;
                case EventCategoryType.Interactive:
                    OnInteract(eventData);
                    break;
                case EventCategoryType.mouseLock:
                    onMouseLock(eventData);
                    break;
                case EventCategoryType.LogicPuzzle:
                    OnPuzzleStateChange(eventData);
                    break;
                case EventCategoryType.Riddle:
                    OnRiddleInteract(eventData);
                    break;
                case EventCategoryType.RiddleAnswer:
                    onRiddleAnswerInteract(eventData);
                    break;
                case EventCategoryType.PopUpDown:
                    OnPopUpDown(eventData);
                    break;
                case EventCategoryType.Cutscene:
                    OnCutscene(eventData);
                    break;
                case EventCategoryType.Animator:
                    OnAnimation(eventData);
                    break;
                case EventCategoryType.Objective:
                    OnObjective(eventData);
                    break;
                case EventCategoryType.volume:
                    OnVolume(eventData);
                    break;
                case EventCategoryType.Reset:
                    OnReset(eventData);
                    break;
                case EventCategoryType.InteractMessage:
                    OnMessage(eventData);
                    break;
                case EventCategoryType.StartSound:
                    OnstartSound(eventData);
                    break;
                default:
                    break;
            }
        }

        private void OnPlayerWin(EventData eventData)
        {
            PlayerWinChanged?.Invoke(eventData);
        }

        private void onRiddleAnswerInteract(EventData eventData)
        {
            RiddleAnswerChanged?.Invoke(eventData);
        }

        private void OnstartSound(EventData eventData)
        {
            StartSoundChanged?.Invoke(eventData);
        }
        protected virtual void onMouseLock(EventData eventData)
        {
            lockChanged?.Invoke(eventData);
        }
        private void OnVolume(EventData eventData)
        {
            VolumeChanged?.Invoke(eventData);
        }

        private void OnReset(EventData eventData)
        {
            Reset?.Invoke(eventData);
        }
        //called when a menu change is requested
        protected virtual void OnMenu(EventData eventData)
        {
            //non-null if an object has subscribed to this event
            MenuChanged?.Invoke(eventData);

            /*
             //Old form:
              if (MenuChanged != null)
                MenuChanged(eventData);
             */
        }

        //called when a camera event needs to be generated
        protected virtual void OnCamera(EventData eventData)
        {
            CameraChanged?.Invoke(eventData);
        }

        //called when a screen event needs to be generated (e.g. change screen layout)
        protected virtual void OnScreen(EventData eventData)
        {
            ScreenChanged?.Invoke(eventData);
        }

        //called when a drawn objects opacity changes - which necessitates moving from opaque <-> transparent list in ObjectManager - see ObjectManager::RegisterForEventHandling()
        protected virtual void OnOpacity(EventData eventData)
        {
            OpacityChanged?.Invoke(eventData);
        }

        //called when a drawn objects needs to be added - see PickingManager::DoFireNewObject()
        protected virtual void OnAddActor(EventData eventData)
        {
            AddActorChanged?.Invoke(eventData);
        }

        //called when a drawn objects needs to be removed - see UIMouseObject::HandlePickedObject()
        protected virtual void OnRemoveActor(EventData eventData)
        {
            RemoveActorChanged?.Invoke(eventData);
        }

        //called when a player related event occurs (e.g. win, lose, health increase)
        protected virtual void OnPlayer(EventData eventData)
        {
            System.Diagnostics.Debug.WriteLine("getting event");
            PlayerChanged?.Invoke(eventData);
        }

        //called when a debug related event occurs (e.g. show/hide debug info)
        protected virtual void OnDebug(EventData eventData)
        {
            DebugChanged?.Invoke(eventData);
        }

        //called when a global sound event is sent to set volume by category or mute all sounds
        protected virtual void OnGlobalSound(EventData eventData)
        {
            GlobalSoundChanged?.Invoke(eventData);
        }

        //called when a 3D sound event is sent e.g. play "boom"
        protected virtual void OnSound3D(EventData eventData)
        {
            Sound3DChanged?.Invoke(eventData);
        }

        //called when a 2D sound event is sent e.g. play "menu music"
        protected virtual void OnSound2D(EventData eventData)
        {
            Sound2DChanged?.Invoke(eventData);
        }

        //called when the PickingManager picks an object
        protected virtual void OnObjectPicking(EventData eventData)
        {
            ObjectPickChanged?.Invoke(eventData);
        }

        //called when the we want to set mouse position, appearance etc.
        protected virtual void OnMouse(EventData eventData)
        {
            MouseChanged?.Invoke(eventData);
        }

        //called when the we want to set mouse position, appearance etc.
        protected virtual void OnVideo(EventData eventData)
        {
            VideoChanged?.Invoke(eventData);
        }

        protected virtual void OnInteract(EventData eventData)
        {
            InteractChanged?.Invoke(eventData);
        }

        protected virtual void OnPuzzleStateChange(EventData eventData)
        {
            PuzzleChanged?.Invoke(eventData);
        }

        protected virtual void OnRiddleInteract(EventData eventData)
        {
            RiddleChanged?.Invoke(eventData);
        }

        protected virtual void OnPopUpDown(EventData eventData)
        {
            PopUpChanged?.Invoke(eventData);
        }

        protected virtual void OnCutscene(EventData eventData)
        {
            cutsceneChanged?.Invoke(eventData);
        }
        
        protected virtual void OnAnimation(EventData eventData)
        {
            animationTriggered?.Invoke(eventData);
        }
        
        protected virtual void OnObjective(EventData eventData)
        {
            ObjectiveChanged?.Invoke(eventData);
        }

        protected virtual void OnMessage(EventData eventData)
        {
            MessageChanged?.Invoke(eventData);
        }


    }
}
