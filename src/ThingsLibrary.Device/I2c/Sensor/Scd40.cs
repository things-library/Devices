using Iot.Device.Common;
using Iot.Device.Scd4x;

namespace ThingsLibrary.Device.I2c.Sensor
{
    public class Scd40Sensor : Base.I2cSensor
    {
        public Scd4x Device { get; set; }

        // States
        public Co2State Co2State { get; init; }
        public TemperatureState TemperatureState { get; init; }
        public HumidityState HumidityState { get; init; }

        // Calculated from Temp + Humidity
        public TemperatureState HeatIndexState { get; init; }
        public TemperatureState DewPointState { get; init; }


        public Scd40Sensor(I2cBus i2cBus, int id = 0x62, string key = "scd40", string name = "SCD40", bool isImperial = false) : base(i2cBus, id, "sensor_scd40", key, name, isImperial)
        {
            // States            
            this.States = new List<ISensorState>(5)
            {
                {   this.Co2State = new Co2State(isImperial: isImperial) },
                {   this.TemperatureState = new TemperatureState(isImperial: isImperial) },
                {   this.HumidityState = new HumidityState(isImperial: isImperial)  },
                
                // Calculated
                {   this.HeatIndexState = new TemperatureState("Heat Index", "hx", isImperial: isImperial) { IsDisabled = true } },
                {   this.DewPointState = new TemperatureState("Dew Point", "dp", isImperial: isImperial) { IsDisabled = true } }
            };
        }

        public Scd40Sensor(I2cBus i2cBus, string key, IItemDto settings, bool isImperial = false) : base(i2cBus, key, settings, isImperial)
        {
            if (this.Type != "sensor_scd40") { throw new ArgumentException($"Invalid settings data, expecting type 'sensor_scd40' not '{this.Type}'."); }

            // States            
            this.States = new List<ISensorState>(5)
            {
                {   this.Co2State = new Co2State(isImperial: isImperial) },
                {   this.TemperatureState = new TemperatureState(isImperial: isImperial) },
                {   this.HumidityState = new HumidityState(isImperial: isImperial)  },
                
                // Calculated
                {   this.HeatIndexState = new TemperatureState("Heat Index", "hx", isImperial: isImperial) { IsDisabled = true } },
                {   this.DewPointState = new TemperatureState("Dew Point", "dp", isImperial: isImperial) { IsDisabled = true } }
            };
        }

        public override bool Init()
        {
            try
            {
                base.Init();

                this.Device = new Scd4x(this.I2cDevice);

                this.MinReadInterval = (int)Scd4x.MeasurementPeriod.TotalMilliseconds;
                if (this.ReadInterval < this.MinReadInterval)
                {
                    this.ReadInterval = this.MinReadInterval;
                    //throw new ArgumentException($"Read interval '{this.ReadInterval} ms' can not be less then min read interval '{this.MinReadInterval} ms' of sensor."); }
                }

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
                var readResult = this.Device.ReadPeriodicMeasurement();

                var updatedOn = DateTime.UtcNow;
                var isStateChanged = false;

                // CO2
                if (readResult.CarbonDioxide.HasValue)
                {
                    this.Co2State.Update(readResult.CarbonDioxide.Value, updatedOn);
                    isStateChanged = true;
                }

                // TEMPERATURE
                if (readResult.Temperature.HasValue)
                {
                    this.TemperatureState.Update(readResult.Temperature.Value, updatedOn);
                    isStateChanged = true;
                }

                // HUMIDITY
                if (readResult.RelativeHumidity.HasValue)
                {
                    this.HumidityState.Update(readResult.RelativeHumidity.Value, updatedOn);
                    isStateChanged = true;
                }

                // if all we need it temp and humidity then great!
                if (readResult.Temperature.HasValue && readResult.RelativeHumidity.HasValue)
                {
                    // only calculate if we actually want it
                    if (!this.HeatIndexState.IsDisabled)
                    {
                        var heatIndex = WeatherHelper.CalculateHeatIndex(this.TemperatureState.Temperature, this.HumidityState.Humidity);
                        this.HeatIndexState.Update(heatIndex, updatedOn);
                    }

                    if (!this.DewPointState.IsDisabled)
                    {
                        var dewPoint = WeatherHelper.CalculateDewPoint(this.TemperatureState.Temperature, this.HumidityState.Humidity);
                        this.DewPointState.Update(dewPoint, updatedOn);
                    }
                }

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
