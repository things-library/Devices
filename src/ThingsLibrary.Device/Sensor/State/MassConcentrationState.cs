namespace ThingsLibrary.Device.Sensor.State
{
    public class MassConcentrationState : SensorState
    {        
        /// <summary>
        /// Particle Count
        /// </summary>
        public MassConcentration ParticleCount { get; set; }

        /// <inheritdoc />
        public void Update(MassConcentration? measurement, DateTimeOffset updatedOn)
        {
            // nothing to do?
            if (this.IsDisabled) { return; }
            if (measurement is null) { return; }

            this.ParticleCount = measurement.Value;
            this.Update(measurement.Value, updatedOn);
        }


        public MassConcentrationState(string key = "parts", string name = "Particles", bool isImperial = false) : base(key, name, isImperial)
        {
            //particles are particles
            this.UnitSymbol = "ppm";
            this.ValuePrecision = 0;            
        }
    }
}
