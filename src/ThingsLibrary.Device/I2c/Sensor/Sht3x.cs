using Iot.Device.Sht3x;

// https://docs.microsoft.com/en-us/dotnet/iot/tutorials/temp-sensor
// https://learn.adafruit.com/adafruit-bmp280-barometric-pressure-plus-temperature-sensor-breakout

namespace ThingsLibrary.Device.I2c.Sensor
{
    public class Sht3xSensor : Base.I2cSensor
    {
        public Sht3x Device { get; set; }

        public TemperatureState TemperatureState { get; init; }
        public HumidityState HumidityState { get; init; }

        /// <inheritdoc/>
        /// <remarks>0x44 is default</remarks>
        public Sht3xSensor(I2cBus i2cBus, int id, string key = "sht3x", string name = "SHT3x", bool isImperial = false) : base(i2cBus, id, "sensor_sht4x", key, name, isImperial)
        {
            this.MinReadInterval = 7; //157hz = 6.37ms

            // States
            this.States = new List<ISensorState>(2)
            {
                { this.TemperatureState = new TemperatureState(isImperial: isImperial) },
                { this.HumidityState = new HumidityState(isImperial: isImperial) }
            };
        }

        public Sht3xSensor(I2cBus i2cBus, string key, IItemDto settings, bool isImperial = false) : base(i2cBus, key, settings, isImperial)
        {
            if (this.Type != "sensor_sht3x") { throw new ArgumentException($"Invalid settings data, expecting type 'sensor_sht3x' not '{this.Type}'."); }

            this.MinReadInterval = 7; //157hz = 6.37ms

            // States
            this.States = new List<ISensorState>(2)
            {
                { this.TemperatureState = new TemperatureState(isImperial: isImperial) },
                { this.HumidityState = new HumidityState(isImperial: isImperial) }
            };
        }

        public override bool Init()
        {
            try
            {
                base.Init();

                this.Device = new Sht3x(this.I2cDevice);

                //this.MinReadInterval = _device.GetMeasurementDuration();

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
                var updatedOn = DateTimeOffset.UtcNow;
                var isStateChanged = false;

                // TEMPERATURE
                var temp = this.Device.Temperature;
                this.TemperatureState.Update(temp.Value, updatedOn);
                
                // HUMIDITY
                var humidity = this.Device.Humidity;
                this.HumidityState.Update(humidity.Value, updatedOn);
                
                isStateChanged = true;
            
                // see if anyone is listening
                if (isStateChanged)
                {
                    this.UpdatedOn = updatedOn;
                    this.StatesChanged?.Invoke(this, this.States);
                }

                return isStateChanged;
            }
            catch (Exception ex)
            {
                this.Meta["$error_fetch"] = ex.Message;

                return false;
            }
        }
    }
}
