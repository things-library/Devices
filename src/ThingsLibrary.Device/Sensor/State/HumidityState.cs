namespace ThingsLibrary.Device.Sensor.State
{
    public class HumidityState : SensorState
    {
        /// <summary>
        /// Humidity
        /// </summary>
        public RelativeHumidity Humidity { get; private set; } = default;

        /// <inheritdoc />
        public void Update(RelativeHumidity? measurement, DateTimeOffset updatedOn)
        {
            // nothing to do?
            if (this.IsDisabled) { return; }
            if (measurement is null) { return; }

            this.Humidity = measurement.Value;
            base.Update(measurement.Value.Percent, updatedOn);
        }


        public HumidityState(string key = "h", string name = "Humidity", bool isImperial = false) : base(key, name, isImperial)
        {
            this.UnitSymbol = "%";
            this.ValuePrecision = 1;
        }
    }
}
