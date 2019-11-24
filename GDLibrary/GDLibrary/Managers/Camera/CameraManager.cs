/*
Function: 		Stores and organises the cameras available within the game (used single and split screen layouts) 
                WORK IN PROGRESS - at present this class is only a barebones class to be used by the ObjectManager 
Author: 		NMCG
Version:		1.1
Date Updated:	
Bugs:			None
Fixes:			Added IEnumberable
*/

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    //See http://www.claudiobernasconi.ch/2013/07/22/when-to-use-ienumerable-icollection-ilist-and-list/
    public class CameraManager : GameComponent, IEnumerable<Camera3D>
    {
        public CameraManager(Game game, int initialSize, EventDispatcher eventDispatcher)
            : base(game)
        {
            cameraList = new List<Camera3D>(initialSize);

            //register with the event dispatcher for the events of interest
            RegisterForEventHandling(eventDispatcher);
        }

        public IEnumerator<Camera3D> GetEnumerator()
        {
            return cameraList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Camera3D camera)
        {
            //first time in ensures that we have a default active camera
            if (cameraList.Count == 0)
                activeCameraIndex = 0;

            cameraList.Add(camera);
        }

        public bool Remove(Predicate<Camera3D> predicate)
        {
            var foundCamera = cameraList.Find(predicate);
            if (foundCamera != null)
                return cameraList.Remove(foundCamera);

            return false;
        }

        public int RemoveAll(Predicate<Camera3D> predicate)
        {
            return cameraList.RemoveAll(predicate);
        }

        public bool SetActiveCamera(Predicate<Camera3D> predicate)
        {
            var index = cameraList.FindIndex(predicate);
            ActiveCameraIndex = index;
            return index != -1 ? true : false;
        }

        public void CycleActiveCamera()
        {
            ActiveCameraIndex = activeCameraIndex + 1;
        }

        //sorts cameras by Camera3D::drawDepth - used for PIP screen layout - see ScreenManager
        public void SortByDepth(SortDirectionType sortDirectionType)
        {
            cameraList.Sort(new CameraDepthComparer(sortDirectionType));
        }

        public override void Update(GameTime gameTime)
        {
            /* 
             * Update all the cameras in the list.
             * Remember that at the moment only 1 camera is visible so this foreach loop seems counter-intuitive.
             * Assuming each camera in the list had some form of automatic movement (e.g. like a security camera) then what would happen if we only updated the active camera?
             */
            foreach (var camera in cameraList)
                if ((camera.StatusType & StatusType.Update) != 0) //if update flag is set
                    camera.Update(gameTime);

            base.Update(gameTime);
        }

        #region Fields

        private readonly List<Camera3D> cameraList;
        private int activeCameraIndex = -1;

        #endregion

        #region Properties

        public Camera3D ActiveCamera => cameraList[activeCameraIndex];

        public int ActiveCameraIndex
        {
            get => activeCameraIndex;
            set => activeCameraIndex = value >= 0 && value < cameraList.Count ? value : 0;
        }

        #endregion

        #region Event Handling

        protected void RegisterForEventHandling(EventDispatcher eventDispatcher)
        {
            eventDispatcher.CameraChanged += EventDispatcher_CameraChanged;
        }

        protected void EventDispatcher_CameraChanged(EventData eventData)
        {
            //cycle to the next camera
            if (eventData.EventType == EventActionType.OnCameraCycle)
                CycleActiveCamera();
            else if (eventData.EventType == EventActionType.OnCameraSetActive)
                //using the additional parameters channel of the event data object - ensure that the ID is set as first element in the array
                SetActiveCamera(x => x.ID.Equals(eventData.AdditionalParameters[0] as string));
        }

        #endregion
    }
}