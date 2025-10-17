namespace ThingsLibrary.Device.Sensor.State
{
    public class LengthState : SensorState
    {
        /// <summary>
        /// Length
        /// </summary>
        public Length Length { get; private set; } = default;

        /// <inheritdoc />
        public void Update(Length? measurement, DateTimeOffset updatedOn)
        {
            // nothing to do?
            if (this.IsDisabled) { return; }
            if (measurement is null) { return; }

            this.Length = measurement.Value;

            var state = (this.IsImperial ? measurement.Value.Feet : measurement.Value.Meters);
            this.Update(state, updatedOn);
        }

        public LengthState(string key = "l", string name = "Length", bool isImperial = false, byte telemetryScaleFactor = 0) : base(key, name, isImperial)
        {
            this.UnitSymbol = (this.IsImperial ? "ft" : "m");
            this.ValuePrecision = (byte)(this.IsImperial ? 2 : 3);
        }
    }
}
