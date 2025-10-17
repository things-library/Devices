using ThingsLibrary.Schema.Library.Telemetry;
using ThingsLibrary.Schema.Library.Interfaces;

namespace ThingsLibrary.Device.Sensor.Interfaces
{
    /// <summary>
    /// Sensor Interface
    /// </summary>
    public interface ISensor : IRootItemDto
    { 
        /// <summary>
        /// Sensor Active?
        /// </summary>
        public bool IsDisabled { get; }

        /// <summary>
        /// Sensor Initialized?
        /// </summary>
        public bool IsInit { get; }

        // ================================================================================
        // Events
        // ================================================================================
        public delegate void StatesChangedEventHandler(object sender, List<ISensorState> states);

        /// <summary>
        /// Event when the sensor states change
        /// </summary>
        public ISensor.StatesChangedEventHandler StatesChanged { get; set; }

        // ================================================================================
        // Properties
        // ================================================================================

        /// <summary>
        /// Collection of States
        /// </summary>
        public List<ISensorState> States { get; }

        /// <summary>
        /// Minimum Device Read Interval
        /// </summary>
        public int MinReadInterval { get; }

        /// <summary>
        /// Read Interval (in miliseconds)
        /// </summary>
        public int ReadInterval { get; }

        /// <summary>
        /// Last time a state actually changed
        /// </summary>
        public DateTimeOffset LastStateChanged { get; }

        /// <summary>
        /// Last State Fetch / Update Timestamp
        /// </summary>
        public DateTimeOffset UpdatedOn { get; }


        // ================================================================================
        // Methods 
        // ================================================================================

        /// <summary>
        /// Initiailize the sensor
        /// </summary>
        public abstract bool Init();
        
        /// <summary>
        /// Attempt to fetch all sensor states
        /// </summary>
        /// <returns></returns>
        public abstract bool FetchStates();


        /// <summary>
        /// Convert to a Telemetry Event
        /// </summary>
        /// <returns></returns>
        public TelemetryEventDto ToTelemetryEvent(string typeKey)
        {
            var telemetryEvent = new TelemetryEventDto(typeKey, this.UpdatedOn)
            {
                Tags = new Dictionary<string, string>(this.States.Count)
            };

            foreach (var state in this.States)
            {
                if (state.IsDisabled) { continue; }

                telemetryEvent.Tags[state.Key] = $"{this.ScaleValue(state.CurrentState, state.ValuePrecision)}";
            }

            return telemetryEvent;
        }

        private long ScaleValue(double value, byte precision)
        {
            // Scale the value by 10 raised to the power of precision
            var scaledValue = value * System.Math.Pow(10, precision);

            // Round the result to the nearest whole number and cast it to long
            return (long)System.Math.Round(scaledValue);
        }
    }
}
