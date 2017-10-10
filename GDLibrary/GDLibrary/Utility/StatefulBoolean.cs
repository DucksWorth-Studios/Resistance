using System;

namespace GDLibrary
{
    public class StatefulBoolean
    {
        private bool currentState;
        private bool previousState;

        public StatefulBoolean() : this(false, false)
        {

        }

        public StatefulBoolean(bool currentState, bool previousState)
        {
            SetState(currentState, previousState);
        }

        public void SetState(bool currentState, bool previousState)
        {
            SetCurrentState(currentState);
            SetPreviousState(previousState);
        }

        public void SetCurrentState(bool currentState)
        {
            this.currentState = currentState;
        }

        public void SetPreviousState(bool previousState)
        {
            this.previousState = previousState;
        }

        //has changed across update
        public bool HasChanged()
        {
            return !(this.currentState == this.previousState);
        }

        public bool Low()
        {
            return this.currentState == this.previousState;
        }

        //has gone from disabled to enabled
        public bool HasEnabled()
        {
            if (this.currentState && !this.previousState)
                return true;
            else
                return false;
        }

        public override string ToString()
        {
            return "[Current: " + this.currentState
                + ", Previous: " + this.previousState + "]";
        }

        public bool JustActive()
        {
            return (this.currentState && !this.previousState);
        }
    }
}
