using Iot.Device.Sen5x;
using ThingsLibrary.Device.Sensor.Interfaces;

namespace ThingsLibrary.Device.I2c.Sensor
{
    public class Sen5xSensor : Base.I2cSensor
    {
        // Part Number Definition:   (Example: PMSA003 = PMS {A0} {03})
        // PMS - sensor type
        // ## = Model/Version
        // ## - Min distinguishable particle diameter 


        /// <summary>
        /// Direct Device Object
        /// </summary>
        public Sen5x Device { get; set; }

        /// <summary>
        /// PM1.0 concentration unit: µg/𝑚3 (standard particle)
        /// </summary>
        public MassConcentrationState Pm1 { get; init; }

        /// <summary>
        /// PM2.5 concentration unit: µg/𝑚3 (standard particle)
        /// </summary>
        public MassConcentrationState Pm2_5 { get; init; }


        /// <summary>
        /// PM4.0 concentration unit: µg/𝑚3 (standard particle)
        /// </summary>
        public MassConcentrationState Pm4 { get; init; }

        /// <summary>
        /// PM10.0 concentration unit: µg/𝑚3 (standard particle)
        /// </summary>
        public MassConcentrationState Pm10 { get; init; }

        /// <summary>
        /// Gets or sets the VOC Index.
        /// </summary>
        /// <remarks>
        /// The VOC Index mimics the human nose’s perception of odors with a relative intensity compared to recent history.The VOC Index is also sensitive to odorless VOCs, but it cannot discriminate between them.
        /// Sensirion's VOC Index is explained in more detail here: https://sensirion.com/media/documents/02232963/6294E043/Info_Note_VOC_Index.pdf .
        /// </remarks>
        public ParticleState VocIndex { get; init; }

        /// <summary>
        /// Gets or sets the NOx Index.
        /// </summary>
        public ParticleState NoxIndex { get; init; }

        /// <summary>
        /// Temperature State
        /// </summary>
        public TemperatureState TemperatureState { get; init; }

        /// <summary>
        /// Humidity State
        /// </summary>
        public HumidityState HumidityState { get; init; }


        public Sen5xSensor(I2cBus i2cBus, int id = Sen5x.DefaultI2cAddress, string key = "sen5x", string name = "SEN5x", bool isImperial = false) : base(i2cBus, id, "sensor_sen5x", key, name, isImperial)
        {
            // States
            this.States = new List<ISensorState>(8)
            {
                {   this.Pm1 = new MassConcentrationState("PM 1.0", "pm1", isImperial: isImperial) { ValuePrecision = 1, UnitSymbol = "mcg/m3" }    },
                {   this.Pm2_5 = new MassConcentrationState("PM 2.5", "pm2_5", isImperial: isImperial) { ValuePrecision = 1, UnitSymbol = "mcg/m3" }    },
                {   this.Pm4 = new MassConcentrationState("PM 4.0", "pm4", isImperial: isImperial) { ValuePrecision = 1, UnitSymbol = "mcg/m3" }    },
                {   this.Pm10 = new MassConcentrationState("PM 10.0", "pm10", isImperial: isImperial) { ValuePrecision = 1, UnitSymbol = "mcg/m3" } },

                {   this.VocIndex = new ParticleState("VOC", "voc", isImperial: isImperial) { UnitSymbol = string.Empty } },
                {   this.NoxIndex = new ParticleState("NOx", "nox", isImperial: isImperial) { UnitSymbol = string.Empty } },

                {   this.TemperatureState = new TemperatureState(isImperial: isImperial) },
                {   this.HumidityState = new HumidityState(isImperial: isImperial) },
            };
        }

        public Sen5xSensor(I2cBus i2cBus, string key, IItemDto settings, bool isImperial = false) : base(i2cBus, key, settings, isImperial)
        {
            if (this.Type != "sensor_sen5x") { throw new ArgumentException($"Invalid settings data, expecting type 'sensor_sen5x' not '{this.Type}'."); }

            // States
            this.States = new List<ISensorState>(8)
            {
                {   this.Pm1 = new MassConcentrationState("PM 1.0", "pm1", isImperial: isImperial) { ValuePrecision = 1, UnitSymbol = "mcg/m3" }    },
                {   this.Pm2_5 = new MassConcentrationState("PM 2.5", "pm2_5", isImperial: isImperial) { ValuePrecision = 1, UnitSymbol = "mcg/m3" }    },
                {   this.Pm4 = new MassConcentrationState("PM 4.0", "pm4", isImperial: isImperial) { ValuePrecision = 1, UnitSymbol = "mcg/m3" }    },
                {   this.Pm10 = new MassConcentrationState("PM 10.0", "pm10", isImperial: isImperial) { ValuePrecision = 1, UnitSymbol = "mcg/m3" } },

                {   this.VocIndex = new ParticleState("VOC", "voc", isImperial: isImperial) { UnitSymbol = string.Empty } },
                {   this.NoxIndex = new ParticleState("NOx", "nox", isImperial: isImperial) { UnitSymbol = string.Empty } },

                {   this.TemperatureState = new TemperatureState(isImperial: isImperial) },
                {   this.HumidityState = new HumidityState(isImperial: isImperial) },
            };
        }

        public override bool Init()
        {
            try
            {
                base.Init();

                this.Device = new Sen5x(this.I2cDevice);

                this.Device.StartMeasurement();

                this.IsInit = true;

                return true;
            }
            catch (Exception ex)
            {
                this.Meta["$error_init"] = ex.Message;
                this.IsDisabled = true;

                return false;
            }
        }

        public override bool FetchStates()
        {
            if (this.IsDisabled || !this.IsInit) { return false; }
            if (DateTimeOffset.UtcNow < this.NextReadOn) { return false; }

            try
            {
                if (!this.Device.ReadDataReadyFlag())
                {
                    this.Meta["$error_fetch"] = "Data Not Ready!";
                    return false;
                }

                var readResult = this.Device.ReadMeasurement();
                if (readResult == null) { return false; }

                this.UpdatedOn = DateTimeOffset.UtcNow;

                this.Pm1.Update(readResult.Pm1_0, this.UpdatedOn);
                this.Pm2_5.Update(readResult.Pm2_5, this.UpdatedOn);
                this.Pm4.Update(readResult.Pm4_0, this.UpdatedOn);
                this.Pm10.Update(readResult.Pm10_0, this.UpdatedOn);

                this.VocIndex.Update(readResult.VocIndex, this.UpdatedOn);
                this.NoxIndex.Update(readResult.NoxIndex, this.UpdatedOn);

                this.TemperatureState.Update(readResult.Temperature, this.UpdatedOn);
                this.HumidityState.Update(readResult.Humidity, this.UpdatedOn);

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
