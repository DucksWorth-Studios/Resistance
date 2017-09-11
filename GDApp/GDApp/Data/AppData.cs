/*
Function: 		Stores common hard-coded variable values used within the game e.g. key mappings, mouse sensitivity
Author: 		NMCG
Version:		1.0
Date Updated:	5/10/16
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace GDLibrary
{
    public class AppData
    {
        #region Mouse
        //defines how much the mouse has to move in pixels before a movement is registered - see MouseManager::HasMoved()
        public static readonly float MouseSensitivity = 1;
        #endregion

        #region Common
        public static int IndexMoveForward = 0;
        public static int IndexMoveBackward = 1;
        public static int IndexRotateLeft = 2;
        public static int IndexRotateRight = 3;
        public static int IndexMoveJump = 4;
        public static int IndexMoveCrouch = 5;
        public static int IndexStrafeLeft = 6;
        public static int IndexStrafeRight = 7;
        #endregion
        
        #region Camera
        public static readonly float CameraRotationSpeed = 0.005f;
        public static readonly float CameraMoveSpeed = 0.05f;
        public static readonly float CameraStrafeSpeed = 0.025f;

        public static readonly float CameraJumpHeight = 30;
        public static readonly float CollidableCameraRotationSpeed = 0.05f;
        public static readonly float CollidableCameraMoveSpeed = 0.5f;
        public static readonly float CollidableCameraStrafeSpeed = 0.4f;


        public static readonly Keys[] CameraMoveKeys = { Keys.W, Keys.S, Keys.A, Keys.D, 
                                         Keys.Space, Keys.C, Keys.LeftShift, Keys.RightShift};
        public static readonly Keys[] CameraMoveKeys_Alt1 = { Keys.T, Keys.G, Keys.F, Keys.H };

        public static readonly float CameraLerpSpeedSlow = 0.05f;
        public static readonly float CameraLerpSpeedMedium = 0.1f;
        public static readonly float CameraLerpSpeedFast = 0.2f;

        public static readonly float CameraThirdPersonScrollSpeedDistanceMultiplier = 0.01f;
        public static readonly float CameraThirdPersonScrollSpeedElevatationMultiplier = 0.1f;

        public static readonly float SecurityCameraRotationSpeedSlow = 0.5f;
        public static readonly float SecurityCameraRotationSpeedMedium = 2 * SecurityCameraRotationSpeedSlow;
        public static readonly float SecurityCameraRotationSpeedFast = 2 * SecurityCameraRotationSpeedMedium;

        //yaw means to rotate around the Y-axis - this will confuse you at first since we're using UnitX but you need to look at Transform3D::RotateBy()
        public static readonly Vector3 SecurityCameraRotationAxisYaw = Vector3.UnitX;
        public static readonly Vector3 SecurityCameraRotationAxisPitch = Vector3.UnitY;
        public static readonly Vector3 SecurityCameraRotationAxisRoll = Vector3.UnitZ;

        #endregion

        #region Player
        public static readonly Keys[] PlayerMoveKeys = { Keys.U, Keys.J, Keys.H, Keys.K, Keys.Y, Keys.I, Keys.N, Keys.M};
        public static readonly float PlayerMoveSpeed = 0.02f;
        public static readonly float PlayerStrafeSpeed = 0.7f * PlayerMoveSpeed;
        public static readonly float PlayerRotationSpeed = 0.08f;
        #endregion

        #region Menu
        public static readonly Keys KeyPauseShowMenu = Keys.Escape;
        #endregion
    }
}
