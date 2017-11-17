﻿/*
Function: 		Enum to define event types generated by game entities e.g. menu, camera manager
Author: 		NMCG
Version:		1.0
Date Updated:	11/10/17
Bugs:			None
Fixes:			None
*/

namespace GDLibrary
{
    public enum EventActionType : sbyte
    {
        //sent by audio, video
        OnPlay,
        OnStop,
        OnPause,

        //sent by menu manager
        OnStart,
        OnRestart,
        OnVolumeUp,
        OnVolumeDown,
        OnMute,
        OnExit,

        //send by mouse or gamepad manager
        OnClick,
        OnHover,

        //sent by camera manager
        OnCameraSetActive,
        OnCameraCycle,

        //sent by player when gains or loses health 
        OnHealthDelta,
        //sent to set to a specific start/end value
        OnHealthSet, 

        //sent by game state manager
        OnLose,
        OnWin,

        OnPickup,
        OnOpen,
        OnClose,

        //sent whenever we want to change from single to multi-screen and vice verse
        OnScreenLayoutChange,

        //sent whenever we change the opacity of a drawn object - remember ObjectManager has two draw lists (opaque and transparent)
        OnOpaqueToTransparent,
        OnTransparentToOpaque,

        //sent when we want to add/remove an Actor from the game - see UIMouseObject::HandlePickedObject()
        OnAddActor,
        OnRemoveActor,

        //sent to show/hide info
        OnToggleDebug,

    }
}
