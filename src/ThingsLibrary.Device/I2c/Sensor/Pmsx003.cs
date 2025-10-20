using Iot.Device.Pmsx003;

namespace ThingsLibrary.Device.I2c.Sensor
{
    //https://learn.adafruit.com/pmsa003i

    public class Pmsx003Sensor : Base.I2cSensor
    {
        // Part Number Definition:   (Example: PMSA003 = PMS {A0} {03})
        // PMS - sensor type
        // ## = Model/Version
        // ## - Min distinguishable particle diameter 


        /// <summary>
        /// Direct Device Object
        /// </summary>
        public Pmsx003 Device { get; set; }

        /// <summary>
        /// PM1.0 concentration unit: µg/𝑚3 (standard particle)
        /// </summary>
        public MassConcentrationState StandardPm10 { get; init; }

        /// <summary>
        /// PM2.5 concentration unit: µg/𝑚3 (standard particle)
        /// </summary>
        public MassConcentrationState StandardPm25 { get; init; }

        /// <summary>
        /// PM10.0 concentration unit: µg/𝑚3 (standard particle)
        /// </summary>
        public MassConcentrationState StandardPm100 { get; init; }


        // ======================================================================
        // Atmospheric Environment
        // ======================================================================
        // NOTE: Atospheric values is the raw data as it is now (whatever temperature and pressure there is currently)
        // Air being a gas is compressible which means that it changes its volume when the pressure changes so when
        // you report concentrations as mass per volume of air it is relevant at what pressure that volume is calculated.

        /// <summary>
        /// PM1.0 concentration unit：µg/𝑚3 (under atmospheric environment)
        /// </summary>
        public MassConcentrationState EnvironmentPm10 { get; init; }

        /// <summary>
        /// PM2.5 concentration unit：µg/𝑚3 (under atmospheric environment)
        /// </summary>
        public MassConcentrationState EnvironmentPm25 { get; init; }

        /// <summary>
        /// PM10.0 concentration unit：µg/𝑚3 (under atmospheric environment)
        /// </summary>
        public MassConcentrationState EnvironmentPm100 { get; init; }


        // ======================================================================
        // Particle Concentrations (Number of particles with diameter beyond xx.x µ𝑚 in 0.1L of air)
        // ======================================================================

        /// <summary>
        /// Number of particles with diameter beyond 0.3 µ𝑚 in 0.1L of air
        /// </summary>
        public ParticleState Particles03 { get; init; }

        /// <summary>
        /// Number of particles with diameter beyond 0.5 µ𝑚 in 0.1L of air
        /// </summary>
        public ParticleState Particles05 { get; init; }

        /// <summary>
        /// Number of particles with diameter beyond 1.0 µ𝑚 in 0.1L of air
        /// </summary>
        public ParticleState Particles10 { get; init; }

        /// <summary>
        /// Number of particles with diameter beyond 2.5 µ𝑚 in 0.1L of air
        /// </summary>
        public ParticleState Particles25 { get; init; }

        /// <summary>
        /// Number of particles with diameter beyond 5.0 µ𝑚 in 0.1L of air
        /// </summary>
        public ParticleState Particles50 { get; init; }

        /// <summary>
        /// Number of particles with diameter beyond 10.0 µ𝑚 in 0.1L of air
        /// </summary>
        public ParticleState Particles100 { get; init; }


        public Pmsx003Sensor(I2cBus i2cBus, int id = Pmsx003.DefaultI2cAddress, string key = "pmsx003", string name = "PMSX003", bool isImperial = false) : base(i2cBus, id, "sensor_pmsx003", key, name, isImperial)
        {
            // States
            this.States = new List<ISensorState>(12)
            {   
                // Standard Concentration States
                {   this.StandardPm10 = new MassConcentrationState("pm3_std", "Standard PM 1.0", isImperial: isImperial) { UnitSymbol = "mcg/m3" } },
                {   this.StandardPm25 = new MassConcentrationState("pm2_5_std", "Standard PM 2.5", isImperial: isImperial) { UnitSymbol = "mcg/m3" } },
                {   this.StandardPm100 = new MassConcentrationState("pm10_std", "Standard PM 10.0", isImperial: isImperial) { UnitSymbol = "mcg/m3" } },
                
                // Environment States 
                {   this.EnvironmentPm10 = new MassConcentrationState("pm1", "PM 1.0", isImperial: isImperial) { UnitSymbol = "mcg/m3" } },
                {   this.EnvironmentPm25 = new MassConcentrationState("pm2_5", "PM 2.5", isImperial: isImperial) { UnitSymbol = "mcg/m3" } },
                {   this.EnvironmentPm100 = new MassConcentrationState("pm10", "PM 10.0", isImperial: isImperial) { UnitSymbol = "mcg/m3" } },
                
                // Particle Counts
                {   this.Particles03 = new ParticleState("pm0_3ct", "Particles > 0.3 µm", isImperial: isImperial) { UnitSymbol = "/0.1L" } },
                {   this.Particles05 = new ParticleState("pm0_5ct", "Particles > 0.5 µm", isImperial: isImperial) { UnitSymbol = "/0.1L" } },
                {   this.Particles10 = new ParticleState("pm1ct", "Particles > 1.0 µm", isImperial: isImperial) { UnitSymbol = "/0.1L" } },
                {   this.Particles25 = new ParticleState("pm2_5ct", "Particles > 2.5 µm", isImperial: isImperial) { UnitSymbol = "/0.1L" } },
                {   this.Particles50 = new ParticleState("pm5ct", "Particles > 5.0 µm", isImperial: isImperial) { UnitSymbol = "/0.1L" } },
                {   this.Particles100 = new ParticleState("pm10ct", "Particles > 10.0 µm", isImperial: isImperial) { UnitSymbol = "/0.1L" } }
            };
        }
        public Pmsx003Sensor(I2cBus i2cBus, string key, IItemDto settings, bool isImperial = false) : base(i2cBus, key, settings, isImperial)
        {
            if (this.Type != "sensor_pmsx003") { throw new ArgumentException($"Invalid settings data, expecting type 'sensor_pmsx003' not '{this.Type}'."); }
            
            // States
            this.States = new List<ISensorState>(12)
            {   
                // Standard Concentration States
                {   this.StandardPm10 = new MassConcentrationState("pm3_std", "Standard PM 1.0", isImperial: isImperial) { UnitSymbol = "mcg/m3" } },
                {   this.StandardPm25 = new MassConcentrationState("pm2_5_std", "Standard PM 2.5", isImperial: isImperial) { UnitSymbol = "mcg/m3" } },
                {   this.StandardPm100 = new MassConcentrationState("pm10_std", "Standard PM 10.0", isImperial: isImperial) { UnitSymbol = "mcg/m3" } },
                
                // Environment States 
                {   this.EnvironmentPm10 = new MassConcentrationState("pm1", "PM 1.0", isImperial: isImperial) { UnitSymbol = "mcg/m3" } },
                {   this.EnvironmentPm25 = new MassConcentrationState("pm2_5", "PM 2.5", isImperial: isImperial) { UnitSymbol = "mcg/m3" } },
                {   this.EnvironmentPm100 = new MassConcentrationState("pm10", "PM 10.0", isImperial: isImperial) { UnitSymbol = "mcg/m3" } },
                
                // Particle Counts
                {   this.Particles03 = new ParticleState("pm0_3ct", "Particles > 0.3 µm", isImperial: isImperial) { UnitSymbol = "/0.1L" } },
                {   this.Particles05 = new ParticleState("pm0_5ct", "Particles > 0.5 µm", isImperial: isImperial) { UnitSymbol = "/0.1L" } },
                {   this.Particles10 = new ParticleState("pm1ct", "Particles > 1.0 µm", isImperial: isImperial) { UnitSymbol = "/0.1L" } },
                {   this.Particles25 = new ParticleState("pm2_5ct", "Particles > 2.5 µm", isImperial: isImperial) { UnitSymbol = "/0.1L" } },
                {   this.Particles50 = new ParticleState("pm5ct", "Particles > 5.0 µm", isImperial: isImperial) { UnitSymbol = "/0.1L" } },
                {   this.Particles100 = new ParticleState("pm10ct", "Particles > 10.0 µm", isImperial: isImperial) { UnitSymbol = "/0.1L" } }
            };
        }

        public override bool Init()
        {
            try
            {
                base.Init();

                this.Device = new Pmsx003(this.I2cDevice);

                //this.MinReadInterval = (int)Scd4x.MeasurementPeriod.TotalMilliseconds;
                //if (this.ReadInterval < this.MinReadInterval) { throw new ArgumentException($"Read interval '{this.ReadInterval} ms' can not be less then min read interval '{this.MinReadInterval} ms' of sensor."); }

                //TODO:

                this.IsInit = true;

                return true;
            }
            catch (Exception ex)
            {
                this.Meta["$error_init"] = ex.Message;
                this.IsDisabled = true; //disable the device

                return false;
            }
        }

        public override bool FetchStates()
        {
            if (this.IsDisabled || !this.IsInit) { return false; }
            if (DateTimeOffset.UtcNow < this.NextReadOn) { return false; }

            try
            {
                var readResult = this.Device.Read();
                if (readResult == null) { return false; }

                this.UpdatedOn = DateTimeOffset.UtcNow;

                this.StandardPm10.Update(readResult.StandardPm10, this.UpdatedOn);
                this.StandardPm25.Update(readResult.StandardPm25, this.UpdatedOn);
                this.StandardPm100.Update(readResult.StandardPm100, this.UpdatedOn);

                this.EnvironmentPm10.Update(readResult.EnvironmentPm10, this.UpdatedOn);
                this.EnvironmentPm25.Update(readResult.EnvironmentPm25, this.UpdatedOn);
                this.EnvironmentPm100.Update(readResult.EnvironmentPm100, this.UpdatedOn);

                this.Particles03.Update(readResult.Particles03, this.UpdatedOn);
                this.Particles05.Update(readResult.Particles03, this.UpdatedOn);
                this.Particles10.Update(readResult.Particles10, this.UpdatedOn);
                this.Particles25.Update(readResult.Particles25, this.UpdatedOn);
                this.Particles50.Update(readResult.Particles50, this.UpdatedOn);
                this.Particles100.Update(readResult.Particles100, this.UpdatedOn);

                // see if anyone is listening
                this.StatesChanged?.Invoke(this, this.States);

                // if we get here the state has changed
                return true;
            }
            catch (Exception ex)
            {
                this.Meta["$error_fetch"] = ex.Message;
                return false;
            }
        }
    }
}
