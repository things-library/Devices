namespace ThingsLibrary.Device.Sensor.State
{
    public class GasState : SensorState
    {
        /// <summary>
        /// Gas Resistance
        /// </summary>
        public ElectricResistance GasResistance { get; private set; } = default;

        /// <inheritdoc />
        public void Update(ElectricResistance? measurement, DateTimeOffset updatedOn)
        {
            // nothing to do?
            if (this.IsDisabled) { return; }
            if (measurement is null) { return; }

            this.GasResistance = measurement.Value;
            this.Update(measurement.Value.Ohms, updatedOn);            
        }


        public GasState(string key = "g", string name = "Gas", bool isImperial = false) : base(key, name, isImperial)
        {
            this.UnitSymbol = (this.IsImperial ? "in" : "mb");
        }
    }
}
