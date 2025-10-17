namespace ThingsLibrary.Device.Gpio
{
    /// <summary>
    /// Basic switch like sensor that is either on or off.
    /// </summary>
    public sealed class BoolSensor : Base.GpioInputDevice, ISensor
    {
        /// <summary>
        /// Event when the state changes
        /// </summary>
        public ISensor.StatesChangedEventHandler StatesChanged { get; set; }

        #region --- Events --- 

        /// <summary>
        /// Internal event tied to GPIO controller (if supported)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValueChanged(object sender, PinValueChangedEventArgs e)
        {            
            // update the state            
            var state = (e.ChangeType == PinEventTypes.Rising ? PinValue.High : PinValue.Low);

            this.UpdateState(state);            
        }

        #endregion

        /// <summary>
        /// Collection of States
        /// </summary>
        public List<ISensorState> States { get; set; } = new List<ISensorState>();

        /// <summary>
        /// Minimum read interval for this sensor
        /// </summary>
        public int MinReadInterval { get => 0; }

        /// <summary>
        /// Interval to which we read the sensor's state
        /// </summary>
        public int ReadInterval { get; set; }

        /// <summary>
        /// Last time the state changed
        /// </summary>
        public DateTimeOffset LastStateChanged => this.BoolState.StateChangedOn;

        /// <inheritdoc />        
        //public DateTimeOffset StateChangedOn => this.BoolState.StateChangedOn;
         
        /// <inheritdoc />        
        public TimeSpan StateDuration() => this.BoolState.StateDuration();  //technically calculated but basically a property

        /// <inheritdoc />
        public TimeSpan LastStateDuration => this.BoolState.LastStateDuration;

        /// <inheritdoc />        
        public DateTimeOffset UpdatedOn => this.BoolState.UpdatedOn;

        /// <summary>
        /// Boolean sensor..  open/close, on/off
        /// </summary>
        /// <param name="controller"><see cref="GpioController"/></param>
        /// <param name="pinId">Board Pin ID</param>
        /// <param name="key">Unique key value</param>
        /// <param name="displayName">Display Name</param>
        /// <param name="isPullUp">If a pull up resistor should be used. False = pull down resistor, null = floating</param>
        public BoolSensor(GpioController controller, ushort pinId, string key, string displayName, bool? isPullUp) : base(controller, pinId, "sensor_bool", key, displayName, isPullUp)
        {
            this.Key = key;

            // bool sensor specific
            this.IsNormallyLow = !this.IsPullUp;

            // States            
            this.States = new List<ISensorState>(1) { { this.BoolState } };
        }

        public BoolSensor(GpioController controller, string key, IItemDto settings) : base(controller, key, settings)
        {
            if (this.Type != "sensor_bool") { throw new ArgumentException($"Invalid settings data, expecting type 'sensor_bool' not '{this.Type}'."); }
            
            // bool sensor specific
            this.IsNormallyLow = !this.IsPullUp;

            // States            
            this.States = new List<ISensorState>(1) { { this.BoolState } };
        }

        public void EnableEventCallback()
        {
            this.Controller.RegisterCallbackForPinValueChangedEvent(this.PinId, PinEventTypes.None, this.ValueChanged);
        }

        /// <summary>
        /// Is the state normal (aka: not fault)
        /// </summary>
        public bool IsNormal => !this.IsFaulted;

        /// <summary>
        /// Is the state faulted based on if low state is 'fault'
        /// </summary>
        /// <remarks>Example: With a pullup resistor, the default/normal value is HIGH, goes LOW when sensor is 'normal' in NC state</remarks>
        public bool IsFaulted => (this.State == (this.IsNormallyLow ? PinValue.High : PinValue.Low));
                
        /// <summary>
        /// Label for the current state
        /// </summary>
        public string StateStr => (this.IsNormal ? this.BoolState.NormalLabel : this.BoolState.FaultedLabel);


        /// <summary>
        /// Attempt to fetch the device state
        /// </summary>
        /// <returns>True if successful</returns>
        public bool FetchStates()
        {
            if (this.IsDisabled || !this.IsInit) { return false; }

            try
            {
                // get the state off the pin
                var state = this.Controller.Read(this.PinId);

                this.UpdateState(state);

                return true;
            }
            catch (Exception ex)
            {
                this.Meta["$error_fetch"] = ex.Message;                
                return false;
            }
        }

        /// <summary>
        /// Common State update method since an event or a fetch can change the states
        /// </summary>
        /// <param name="state"></param>
        private void UpdateState(PinValue state)
        {
            bool isStateChanged = (state != this.State);

            // set the state
            this.State = state;
            
            // throw events if we are enabled
            if (!this.IsDisabled) { return; }
                   
            var updatedOn = DateTimeOffset.UtcNow;

            this.BoolState.Update(this.IsFaulted, updatedOn);

            if (isStateChanged)
            {
                this.StatesChanged?.Invoke(this, this.States);
            }            
        }
    }
}
