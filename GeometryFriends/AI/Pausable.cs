
namespace GeometryFriends.AI
{
    /// <summary>
    /// Classes implementing this interface will be able to start, stop pause and resume arbitrarily during the game execution
    /// </summary>
    public abstract class Pausable
    {
        protected bool isRunning = false;
        public bool Running
        {
            get
            {
                return this.isRunning;
            }
        }

        public bool hasStarted = false;
        public bool Started
        {
            get
            {
               return this.hasStarted;
            }

        }

        /// <summary>
        /// this method should be called whenever a fresh start is required. It will reset all date in the class
        /// sets both <i>isRunning</i> and <i>hasStarted</i> to <b>true</b>
        /// has no effect if <i>hasStarted</i> is <b>true</b>
        /// <returns> whether it has been able to successfuly start or not</returns>
        /// </summary>
        public virtual bool Start()
        {
            if (this.hasStarted)
                return false;
            else
            {
                this.hasStarted = true;
                this.isRunning = true;
                return true;
            }
        }

        /// <summary>
        /// Will stop class completely. It is not resumable after a stop, which means a start will be needed to restart
        /// sets both <i>isRunning</i> and <i>hasStarted</i> to <b>false</b>
        /// has no effect if <i>hasStarted</i> is <b>false</b>
        /// <returns>Whether this method has successfuly stopped or not</returns>
        /// </summary>
        public virtual bool Stop()
        {
            if (!this.hasStarted)
                return false;
            else
            {
                this.hasStarted = false;
                this.isRunning = false;
                return true;
            }
        }

        /// <summary>
        /// Temporarily halts the execution, keeping its current state. class needs to be running in order for it to have any effect
        /// sets <i>isRunning</i> to <b>false</b>
        /// has no effect if <i>isRunning</i> or <i> hasStarted</i> is <b>false</b>
        /// <returns> whether or not it was paused sucessfuly</returns>
        /// </summary> 
        public virtual bool Pause()
        {
            if (!this.isRunning)
                return false;
            else
            {                
                this.isRunning = false;
                return true;
            }
        }

        /// <summary>
        /// Resumes a temporary halted execution issued by the pause method. Does not work on a stopped class and has no effect
        /// on an already running class
        /// sets <i>isRunning</i> to <b>true</b>
        /// has no effect if <i>isRunning</i> is <b>true</b> or <i>hasStarted</i> is <b>false</b>  
        /// <returns> whether or not it was able to resume</returns>
        /// </summary>
        public virtual bool Resume()
        {
            if (this.isRunning)
                return false;
            else
            {                
                this.isRunning = true;
                return true;
            }
        }
    }
}
