namespace ThingsLibrary.Device.Sensor.State
{
    public class PressureState : SensorState
    {
        /// <summary>
        /// Native pressure measurement
        /// </summary>
        public Pressure Pressure { get; private set; } = default;

        /// <inheritdoc />
        public void Update(Pressure? measurement, DateTimeOffset updatedOn)
        {
            // nothing to do?
            if (this.IsDisabled) { return; }
            if (measurement is null) { return; }

            this.Pressure = measurement.Value;

            var state = (this.IsImperial ? this.Pressure.InchesOfMercury : this.Pressure.Millibars);            
            base.Update(state, updatedOn);
        }


        public PressureState(string key = "p", string name = "Pressure", bool isImperial = false) : base(key, name, isImperial)
        {
            this.UnitSymbol = (this.IsImperial ? "inHg" : "mb");

            this.ValuePrecision = (byte)(this.IsImperial ? 2 : 0);
        }
    }
}
