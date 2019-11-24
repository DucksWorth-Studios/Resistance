/*
Function: 		Encapsulates manager parameters for those classes (e.g. UIMouseObject) that need access to a large number of managers.
                Used by UIMouseObject.
Author: 		NMCG
Version:		1.0
Date Updated:	25/11/17
Bugs:			None
Fixes:			None
*/

namespace GDLibrary
{
    public class ManagerParameters
    {
        //useful for objects that need access to ALL managers
        public ManagerParameters(ObjectManager objectManager, CameraManager cameraManager,
            MouseManager mouseManager, KeyboardManager keyboardManager, GamePadManager gamePadManager,
            ScreenManager screenManager, SoundManager soundManager)
        {
            ObjectManager = objectManager;
            CameraManager = cameraManager;
            MouseManager = mouseManager;
            KeyboardManager = keyboardManager;
            GamePadManager = gamePadManager;
            ScreenManager = screenManager;
            SoundManager = soundManager;
        }

        #region Fields

        #endregion

        #region Properties

        //only getters since we would rarely want to re-define a manager during gameplay
        public ObjectManager ObjectManager { get; }

        public CameraManager CameraManager { get; }

        public MouseManager MouseManager { get; }

        public KeyboardManager KeyboardManager { get; }

        public GamePadManager GamePadManager { get; }

        public ScreenManager ScreenManager { get; }

        public SoundManager SoundManager { get; }

        #endregion
    }
}