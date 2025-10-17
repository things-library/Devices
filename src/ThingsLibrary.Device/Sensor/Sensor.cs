namespace ThingsLibrary.Device.Sensor
{
    public static class Sensors
    {
        public static Dictionary<string, ISensor> Parse(GpioController gpioController, I2cBus i2cBus, Dictionary<string, IItemDto> sensorSettings)
        {
            var sensors = new Dictionary<string, ISensor>();

            foreach (var sensorNode in sensorSettings)
            {
                ISensor sensor = sensorNode.Value.Type switch
                {
                    // GPIO
                    "sensor_bool" => new BoolSensor(gpioController, sensorNode.Key, sensorNode.Value),

                    // I2C                       
                    "sensor_bme680" => new Bme680Sensor(i2cBus, sensorNode.Key, sensorNode.Value),
                    "sensor_bmp280" => new Bmp280Sensor(i2cBus, sensorNode.Key, sensorNode.Value),

                    "sensor_scd40" => new Scd40Sensor(i2cBus, sensorNode.Key, sensorNode.Value),
                    "sensor_pmsx003" => new Scd40Sensor(i2cBus, sensorNode.Key, sensorNode.Value),
                    "sensor_sen5x" => new Sen5xSensor(i2cBus, sensorNode.Key, sensorNode.Value),
                    "sensor_sht4x" => new Sht4xSensor(i2cBus, sensorNode.Key, sensorNode.Value),
                    "sensor_vl53l0x" => new Vl53l0xSensor(i2cBus, sensorNode.Key, sensorNode.Value),
                    "sensor_vl53l1x" => new Vl53l1xSensor(i2cBus, sensorNode.Key, sensorNode.Value),

                    _ => throw new ArgumentException($"Unexpected sensor type '{sensorNode.Value.Type}'.")
                };

                if (sensor != null)
                {
                    sensors[sensorNode.Key] = sensor;
                }
            }

            return sensors;
        }     
    }
}
