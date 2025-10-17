namespace ThingsLibrary.Device.Sensor.Interfaces
{
    /// <summary>
    /// Specific state (Humidity, Temperature, CO2, etc)
    /// </summary>
    public interface ISensorState
    {
        /// <summary>
        /// Collection Unique Key
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Sensor Display Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Output Imperial Values?
        /// </summary>
        public bool IsImperial { get; }

        /// <summary>
        /// Unit Symbol
        /// </summary>
        public string UnitSymbol { get; }

        /// <summary>
        /// Number of digits after the decimal, also how much the data is scaled for telemetry (0 = no scaling)
        /// </summary>
        /// <example>1 = for temp so 78.1 becomes 781 in telemetry data</example>
        public byte ValuePrecision { get; }

        /// <summary>
        /// should this sensor state be emitted in telemetry and other outputs?
        /// </summary>
        public bool IsDisabled { get; }


        /// <summary>
        /// Last time a fetch occured regardless of the state actually changing
        /// </summary>
        public DateTimeOffset UpdatedOn { get; }


        // ================================================================================
        // CURRENT STATE 
        // ================================================================================

        /// <summary>
        /// State Value
        /// </summary>
        public abstract double CurrentState { get; }

        /// <summary>
        /// Last time the state was changed (null = never)
        /// </summary>
        public DateTimeOffset StateChangedOn { get; }

        // ================================================================================
        // HISTORICAL 
        // ================================================================================

        /// <summary>
        /// Last State
        /// </summary>
        public double? LastState { get; }

        /// <summary>
        /// Last State Duration
        /// </summary>
        public TimeSpan LastStateDuration { get; }


        // ================================================================================
        // METHODS 
        // ================================================================================

        /// <summary>
        /// Value as a string with unit symbol
        /// </summary>
        /// <returns>Format returned: {Value:Precision} {UnitSymbol}</returns>
        public abstract string DisplayValue();

        /// <summary>
        /// Value as a string with unit symbol
        /// </summary>
        /// <returns>Format returned: {Value:Precision} {UnitSymbol}</returns>
        public abstract TimeSpan StateDuration(DateTimeOffset? updatedOn);

        /// <summary>
        /// State Update
        /// </summary>
        public abstract void Update(double state, DateTimeOffset updatedOn);
    }
}
