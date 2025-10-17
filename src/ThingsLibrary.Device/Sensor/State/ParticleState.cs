namespace ThingsLibrary.Device.Sensor.State
{
    public class ParticleState : SensorState
    {   
        /// <inheritdoc />
        public void Update(double? measurement, DateTimeOffset updatedOn)
        {
            // nothing to do?
            if (this.IsDisabled) { return; }
            if (measurement is null) { return; }

            base.Update(measurement.Value, updatedOn);
        }

        public ParticleState(string key = "parts", string name = "Particles", bool isImperial = false) : base(key, name, isImperial)
        {
            //particles are particles
            this.UnitSymbol = "ppm";
            this.ValuePrecision = 0;
        }
    }
}
