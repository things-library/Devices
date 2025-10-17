namespace ThingsLibrary.Device.Sensor.State
{        
    /// <summary>
    /// Basic boolean state for keeping track of two states.
    /// </summary>
    /// <example>Door Open/Close, Motion Detected/Not Detected</example>
    public class BoolState : SensorState
    {
        /// <summary>
        /// Current State
        /// </summary>
        public bool IsFaulted => (this.CurrentState != 0);

        /// <summary>
        /// Label for the normal / off state
        /// </summary>
        public string NormalLabel { get; set; } = "Off";

        /// <summary>
        /// Label for the faulted / on state
        /// </summary>
        public string FaultedLabel { get; set; } = "On";
                
        /// <summary>
        /// Last State pretty label
        /// </summary>
        public string LastStateLabel => (this.IsFaulted ? this.NormalLabel : this.FaultedLabel);

        /// <summary>
        /// State Label to show the user
        /// </summary>
        public string StateLabel => (this.IsFaulted ? this.FaultedLabel : this.NormalLabel);
                

        /// <inheritdoc />
        public void Update(bool isFaulted, DateTimeOffset updatedOn)
        {
            this.UpdatedOn = updatedOn;

            // is the state changing?
            if (isFaulted != this.IsFaulted)
            {
                //capture the duration of the previous state since we know we are changing
                this.LastState = (this.IsFaulted ? 1 : 0); // we haven't changed it yet
                this.LastStateDuration = this.StateDuration(updatedOn);

                this.CurrentState = (this.IsFaulted ? 1 : 0);
                this.StateChangedOn = updatedOn;

                // did the state change during this process?
                if (this.StateChangedOn == UpdatedOn)
                {
                    // see if anyone is listening (use provided value to be thread safe)
                    this.StateChanged?.Invoke(this, this.ToEvent());
                }
            }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">ID</param>
        /// <param name="key">Key</param>
        /// <param name="normalLabel">Display label when in the normal state</param>
        /// <param name="faultedLabel">Display label when faulted state</param>
        public BoolState(string key = "sw", string name = "Switch", string normalLabel = "Off", string faultedLabel = "On") : base(key, name, false)
        {
            this.NormalLabel = normalLabel;
            this.FaultedLabel = faultedLabel;

            // make sure 
            var updatedOn = DateTimeOffset.UtcNow;

            this.StateChangedOn = updatedOn;
            this.UpdatedOn = updatedOn;
        }
    }
}
