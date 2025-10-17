using SMath = System.Math;

using ThingsLibrary.Device.Sensor.Events;

namespace ThingsLibrary.Device.Sensor
{
    /// <summary>
    /// Specific element state (Humidity, Temperature, CO2, etc)
    /// </summary>
    public abstract class SensorState : ISensorState
    {
        #region --- Events ---

        public delegate void StateChangedEventHandler(object sender, StateEvent args);

        /// <summary>
        /// Event when the state changes
        /// </summary>
        public StateChangedEventHandler StateChanged { get; set; }

        #endregion
                
        /// <inheritdoc />
        public string Key { get; init; }

        /// <inheritdoc />
        public string Name { get; init; }

        /// <inheritdoc />        
        public bool IsImperial { get; init; }
        
        /// <inheritdoc />
        public string UnitSymbol { get; init; }

        /// <inheritdoc />
        public byte ValuePrecision { get; internal set; }

        /// <inheritdoc />
        public bool IsDisabled { get; set; }

        /// <inheritdoc />
        public DateTimeOffset UpdatedOn { get; internal set; }

        // ================================================================================
        // CURRENT STATE 
        // ================================================================================

        /// <inheritdoc />
        public double CurrentState { get; internal set; }

        /// <inheritdoc />
        public DateTimeOffset StateChangedOn { get; internal set; }

        // ================================================================================
        // HISTORICAL 
        // ================================================================================

        /// <inheritdoc />
        public double? LastState { get; internal set; }

        /// <inheritdoc />
        public TimeSpan LastStateDuration { get; internal set; }


        // ================================================================================
        // METHODS 
        // ================================================================================

        /// <inheritdoc />
        public void Update(double state, DateTimeOffset updatedOn)
        {
            if (this.IsDisabled) { return; }

            this.UpdatedOn = updatedOn;

            // state changing?
            if (state != this.CurrentState)
            {
                //capture the duration of the previous state since we know we are changing
                this.LastState = this.CurrentState; // we haven't changed it yet
                this.LastStateDuration = this.StateDuration(updatedOn);

                this.CurrentState = state;
                this.StateChangedOn = updatedOn;
            }

            // see if anyone is listening (use provided value to be thread safe)
            this.StateChanged?.Invoke(this, this.ToEvent());
        }

        /// <inheritdoc />
        public string DisplayValue() => $"{this.CurrentState.ToString($"n{this.ValuePrecision}")} {this.UnitSymbol}";

        /// <inheritdoc />
        public TimeSpan StateDuration(DateTimeOffset? updatedOn = null)
        {
            // if not provided then use UTC now 
            if (updatedOn == null)
            {
                updatedOn = DateTimeOffset.UtcNow;
            }

            return updatedOn.Value.Subtract(this.StateChangedOn);           
        }
        

        public override string ToString() => this.DisplayValue();
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">ID</param>
        /// <param name="key">Key</param>
        /// <param name="isImperical">Metric or Imperical</param>
        public SensorState(string key, string name, bool isImperical)
        {
            this.Key = key;
            this.Name = name;
            
            this.IsImperial = isImperical;

            // lets just assume that now is the first we have known about this state (aka: prevent null reference)
            var updatedOn = DateTimeOffset.UtcNow;
            
            this.StateChangedOn = updatedOn;
            this.UpdatedOn = updatedOn;
        }


        /// <summary>
        /// Create a snapshot event object
        /// </summary>
        /// <returns></returns>
        public Events.StateEvent ToEvent()
        {
            return new Events.StateEvent
            {
                Id = this.Name,
                Key = this.Key,
                ValuePrecision = this.ValuePrecision,

                State = SMath.Round(this.CurrentState, this.ValuePrecision),
                StateChangedOn = this.StateChangedOn,

                LastState = (this.LastState != null ? SMath.Round(this.LastState.Value, this.ValuePrecision) : null),
                LastStateDuration = this.LastStateDuration
            };
        }
    }
}
