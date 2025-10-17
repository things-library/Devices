namespace ThingsLibrary.Device.Sensor.State
{
    public class Co2State : SensorState
    {
        public VolumeConcentration Co2 { get; private set; } = default;
                
        /// <inheritdoc />
        public void Update(VolumeConcentration? measurement, DateTimeOffset updatedOn)
        {
            if(measurement is null) { return; }

            this.Co2 = measurement.Value;

            base.Update(this.Co2.PartsPerMillion, updatedOn);
        }

        public Co2State(string key = "co2", string name = "CO2", bool isImperial = false) : base(key, name, isImperial)
        {
            this.UnitSymbol = "ppm";
            this.ValuePrecision = 0;
        }
    }
}
