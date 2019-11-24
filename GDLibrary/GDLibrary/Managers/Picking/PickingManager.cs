using System;
using JigLibX.Physics;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class PickingManager : PausableGameComponent
    {
        protected static readonly string NoObjectSelectedText = "no object selected";
        protected static readonly float DefaultMinPickPlaceDistance = 20;
        protected static readonly float DefaultMaxPickPlaceDistance = 100;
        private static readonly int DefaultDistanceToTargetPrecision = 1;
        private bool bCurrentlyPicking;
        private Camera3D camera;
        private float cameraPickDistance;
        private readonly Predicate<CollidableObject> collisionPredicate;
        private bool currentlyOpen;

        //local vars
        private CollidableObject currentPickedObject;
        private readonly ConstraintVelocity damperController = new ConstraintVelocity();
        private float distanceToObject;

        private readonly ManagerParameters managerParameters;
        private readonly ConstraintWorldPoint objectController = new ConstraintWorldPoint();
        private readonly float pickEndDistance;
        private readonly PickingBehaviourType pickingBehaviourType;
        private readonly float pickStartDistance;
        private Vector3 pos, normal;

        public PickingManager(Game game, EventDispatcher eventDispatcher, StatusType statusType,
            ManagerParameters managerParameters, PickingBehaviourType pickingBehaviourType, float pickStartDistance,
            float pickEndDistance, Predicate<CollidableObject> collisionPredicate)
            : base(game, eventDispatcher, statusType)
        {
            this.managerParameters = managerParameters;

            this.pickingBehaviourType = pickingBehaviourType;
            this.pickStartDistance = pickStartDistance;
            this.pickEndDistance = pickEndDistance;
            this.collisionPredicate = collisionPredicate;

            RegisterForEventHandlingPicking(eventDispatcher);
        }

        public void RegisterForEventHandlingPicking(EventDispatcher eventDispatcher)
        {
            eventDispatcher.RiddleChanged += changeState;
            eventDispatcher.PopUpChanged += changeState;
        }

        public void changeState(EventData eventdata)
        {
            if (currentlyOpen)
                currentlyOpen = false;
            else
                currentlyOpen = true;
        }

        #region Event Handling

        protected override void EventDispatcher_MenuChanged(EventData eventData)
        {
            //did the event come from the main menu and is it a start game event
            if (eventData.EventType == EventActionType.OnStart)
                //turn on update and enable picking
                StatusType = StatusType.Update;
            //did the event come from the main menu and is it a pause game event
            else if (eventData.EventType == EventActionType.OnPause)
                //turn off update to disable picking
                StatusType = StatusType.Off;
            else if (eventData.EventType == EventActionType.OnLose)
                //turn off update and draw i.e. show the menu since the game is paused
                StatusType = StatusType.Off;
        }

        #endregion

        protected override void HandleInput(GameTime gameTime)
        {
            HandleMouse(gameTime);
            HandleKeyboard(gameTime);
            HandleGamePad(gameTime);
        }

        protected override void ApplyUpdate(GameTime gameTime)
        {
            //listen to input and check for picking from mouse and any input from gamepad and keyboard
            HandleInput(gameTime);

            base.ApplyUpdate(gameTime);
        }

        protected override void HandleMouse(GameTime gameTime)
        {
            if (pickingBehaviourType == PickingBehaviourType.InteractWithObject) InteractWithObject(gameTime);
        }

        /**
         * Tomas
         * Finds a suitable object with the interactable enum and sends it to the interact event
         */
        private void InteractWithObject(GameTime gameTime)
        {
            if (managerParameters.MouseManager.IsLeftButtonClickedOnce())
            {
                camera = managerParameters.CameraManager.ActiveCamera;
                currentPickedObject = managerParameters.MouseManager.GetPickedObject(camera, camera.ViewportCentre,
                    pickStartDistance, pickEndDistance, out pos, out normal) as CollidableObject;

                if (currentPickedObject != null && currentPickedObject.ActorType == ActorType.Interactable)
                {
                    //generate event to tell object manager and physics manager to remove the object
                    EventDispatcher.Publish(new EventData(currentPickedObject, EventActionType.Interact,
                        EventCategoryType.Interactive));
                    //Console.WriteLine("Interacting");
                }
                else if (currentPickedObject != null && currentPickedObject.ActorType == ActorType.PopUP)
                {
                    if (!currentlyOpen)
                        //EventDispatcher.Publish(new EventData(EventActionType.OnLose,EventCategoryType.MainMenu));
                        //EventDispatcher.Publish(new EventData(EventActionType.OnLose,EventCategoryType.mouseLock));
                        EventDispatcher.Publish(new EventData(EventActionType.OnOpen, EventCategoryType.Riddle));
                }
                else if (currentPickedObject != null && currentPickedObject.ActorType == ActorType.CollidablePickup)
                {
                    EventDispatcher.Publish(new EventData(currentPickedObject, EventActionType.RiddleSolved,
                        EventCategoryType.RiddleAnswer));
                    EventDispatcher.Publish(new EventData(EventActionType.OpenBookcase, EventCategoryType.Animator));
                }

                //Console.WriteLine("Hello");
            }
        }

        private void DoPickAndPlace(GameTime gameTime)
        {
            if (managerParameters.MouseManager.IsMiddleButtonClicked())
            {
                if (!bCurrentlyPicking)
                {
                    camera = managerParameters.CameraManager.ActiveCamera;
                    currentPickedObject = managerParameters.MouseManager.GetPickedObject(camera, camera.ViewportCentre,
                        pickStartDistance, pickEndDistance, out pos, out normal) as CollidableObject;

                    distanceToObject = (float) Math.Round(Vector3.Distance(camera.Transform.Translation, pos),
                        DefaultDistanceToTargetPrecision);

                    if (currentPickedObject != null && IsValidCollision(currentPickedObject, pos, normal))
                    {
                        var vectorDeltaFromCentreOfMass = pos - currentPickedObject.Collision.Owner.Position;
                        vectorDeltaFromCentreOfMass = Vector3.Transform(vectorDeltaFromCentreOfMass,
                            Matrix.Transpose(currentPickedObject.Collision.Owner.Orientation));
                        cameraPickDistance = (managerParameters.CameraManager.ActiveCamera.Transform.Translation - pos)
                            .Length();

                        //remove any controller from any previous pick-release 
                        objectController.Destroy();
                        damperController.Destroy();

                        currentPickedObject.Collision.Owner.SetActive();
                        //move object by pos (i.e. point of collision and not centre of mass)
                        objectController.Initialise(currentPickedObject.Collision.Owner, vectorDeltaFromCentreOfMass,
                            pos);
                        //dampen velocity (linear and angular) on object to Zero
                        damperController.Initialise(currentPickedObject.Collision.Owner,
                            ConstraintVelocity.ReferenceFrame.Body, Vector3.Zero, Vector3.Zero);
                        objectController.EnableConstraint();
                        damperController.EnableConstraint();
                        //we're picking a valid object for the first time
                        bCurrentlyPicking = true;

                        //update mouse text
                        object[] additionalParameters = {currentPickedObject, distanceToObject};
                        EventDispatcher.Publish(new EventData(EventActionType.OnObjectPicked,
                            EventCategoryType.ObjectPicking, additionalParameters));
                    }
                }

                //if we have an object picked from the last update then move it according to the mouse pointer
                if (objectController.IsConstraintEnabled && objectController.Body != null)
                {
                    // Vector3 delta = objectController.Body.Position - this.managerParameters.CameraManager.ActiveCamera.Transform.Translation;
                    var direction = managerParameters.MouseManager
                        .GetMouseRay(managerParameters.CameraManager.ActiveCamera).Direction;
                    cameraPickDistance += managerParameters.MouseManager.GetDeltaFromScrollWheel() * 0.1f;
                    var result = managerParameters.CameraManager.ActiveCamera.Transform.Translation +
                                 cameraPickDistance * direction;
                    //set the desired world position
                    objectController.WorldPosition =
                        managerParameters.CameraManager.ActiveCamera.Transform.Translation +
                        cameraPickDistance * direction;
                    objectController.Body.SetActive();
                }
            }
            else //releasing object
            {
                if (bCurrentlyPicking)
                {
                    //release object from constraints and allow to behave as defined by gravity etc
                    objectController.DisableConstraint();
                    damperController.DisableConstraint();

                    //notify listeners that we're no longer picking
                    object[] additionalParameters = {NoObjectSelectedText};
                    EventDispatcher.Publish(new EventData(EventActionType.OnNonePicked, EventCategoryType.ObjectPicking,
                        additionalParameters));

                    bCurrentlyPicking = false;
                }
            }
        }

        protected override void HandleKeyboard(GameTime gameTime)
        {
        }

        protected override void HandleGamePad(GameTime gameTime)
        {
        }

        //called when over collidable/pickable object
        protected virtual bool IsValidCollision(CollidableObject collidableObject, Vector3 pos, Vector3 normal)
        {
            //if not null then call method to see if its an object that conforms to our predicate (e.g. ActorType::CollidablePickup), otherwise return false
            return collidableObject != null ? collisionPredicate(collidableObject) : false;
        }
    }
}