using Iot.Device.Board;

using Iot.Device.Ft232H;
using Iot.Device.Mcp23xxx;

using System.Device.Gpio;
using System.Device.I2c;
using System.Text.Json;

using ThingsLibrary.DataType.Extensions;

using ThingsLibrary.Device.Gpio;
using ThingsLibrary.Device.I2c;
using ThingsLibrary.Device.I2c.Base;
using ThingsLibrary.Device.Sensor;
using ThingsLibrary.Device.Sensor.Events;

using ThingsLibrary.Schema.Library;
using ThingsLibrary.Schema.Library.Base;
using ThingsLibrary.Schema.Library.Interfaces;
using Ft = Iot.Device.FtCommon;

namespace Device.Tester
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            // get the different devices that are plugged in and available
            Log.Information("================================================================================");
            Log.Information("Getting GPIO Controller devices...");
            var devices = Ft.FtCommon.GetDevices();

            Log.Information("Loading Settings...");
            var settings = GetSettings();


            if (devices.Any())
            {
                foreach (var device in devices)
                {
                    Log.Information($"+ {device.Description} ({device.Flags}, Id: {device.Id}, Serial: {device.SerialNumber}, Type: {device.Type})");
                }
            }
            else
            {
                Log.Error("- No GPIO Controller device connected!");
                return;
            }

            // pick just the first one
            var ftDevice = new Ft232HDevice(devices.First());
            
            var gpioController = ftDevice.CreateGpioController();
            //GpioTests(gpioController);

            var i2cBus = ftDevice.CreateOrGetI2cBus(0);
            I2cBusScan(i2cBus);
            
            var sensors = Sensors.Parse(gpioController, i2cBus, settings.Items["sensors"].Items.ToDictionary(x => x.Key, x => (IItemDto)x.Value));
            TestSensors(sensors);

            //McpTests(i2cBus, gpioController);
            
            //I2cTests(i2cBus);
        }

        public static RootItemDto GetSettings()
        {
            if (!File.Exists("settings.json")) { throw new ArgumentException("Unable to find settings-debug.json"); }

            var json = File.ReadAllText("settings.json");
            var settings = JsonSerializer.Deserialize<RootItemDto>(json, SchemaBase.JsonSerializerOptions) ?? throw new ArgumentException("Unable to deserialize options");

            return settings;
        }

        

        public static void TestSensors(Dictionary<string, ISensor> sensors)
        {
            Log.Information("================================================================================");
            Log.Information("Sensor Initialization...");

            foreach (var sensor in sensors.Values)
            {
                if (sensor.IsDisabled) 
                {
                    Log.Information($"+ {sensor.Name}: Disabled");
                    continue; 
                }

                // try to initialize
                if (sensor.Init())
                {
                    Log.Information($"+ {sensor.Name}: Initialized");
                }
                else
                {
                    Log.Information($"- {sensor.Name}: {sensor["$error_init", true]}");
                }
            }

            Log.Information("================================================================================");
            Log.Information("Sensor Loop...");
            // DO SENSOR LOOP
            while (true)
            {
                foreach (var sensor in sensors.Values)
                {
                    if (sensor.IsDisabled || !sensor.IsInit) { continue; }

                    if(!sensor.FetchStates()) { continue; }
                    
                    Log.Information($"{sensor.ToTelemetryEvent(sensor.Key)}");
                }

                Thread.Sleep(2000);
                Log.Information("================================================================================");
            }
        }

        public static void I2cBusScan(I2cBus i2cBus)
        {
            //====================================================
            // I2C BUS SCAN (hex values)
            //====================================================
            //     0  1  2  3  4  5  6  7  8  9  a  b  c  d  e  f
            //00: -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
            //10: -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
            //20: -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
            //30: -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
            //40: -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
            //50: -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
            //60: -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
            //70: -- -- -- -- -- -- 76 77 -- -- -- -- -- -- -- --
            //====================================================

            var results = i2cBus.PerformBusScan();

            Console.WriteLine("====================================================");
            Console.WriteLine(" I2C BUS SCAN (hex values)");
            Console.WriteLine("====================================================");
            Console.WriteLine("     0  1  2  3  4  5  6  7  8  9  a  b  c  d  e  f");

            for (int startingRowAddress = 0; startingRowAddress < 128; startingRowAddress += 16)
            {
                Console.Write($"{startingRowAddress:x2}: ");  // Beginning of row.

                for (int rowAddress = 0; rowAddress < 16; rowAddress++)
                {
                    int deviceAddress = startingRowAddress + rowAddress;

                    if (results.Contains(deviceAddress))
                    {
                        Console.Write($"{deviceAddress:x2} ");
                    }
                    else
                    {
                        Console.Write("-- ");
                    }
                }

                Console.WriteLine("");
            }
            Console.WriteLine("====================================================");
        }


        public static void GpioTests(GpioController gpioController)
        {
            var sensors = new List<BoolSensor>();
            sensors.Add(new BoolSensor(gpioController, 0, "motion", "Motion", false));

            Log.Information("Initializing {SensorCount} Sensors...", sensors.Count);

            // Initialize all the sensors
            sensors.ForEach((sensor) => 
            { 
                sensor.Init(); 
                
            });

            sensors[0].BoolState.StateChanged += StateChanged;

            Log.Information("================================================================================");
            Log.Information("Reading Sensor Telemetry...");

            // DO SENSOR LOOP
            while (true)
            {
                foreach (var sensor in sensors)
                {
                    if (!sensor.IsDisabled)
                    {
                        Log.Information($"{sensor.Name}: Not Enabled");
                        continue;
                    }

                    sensor.FetchStates();

                    Log.Information($"{sensor.Name}: {sensor.StateStr} ({sensor.BoolState.StateDuration().ToHHMMSS()})");
                }

                Thread.Sleep(2000);
            }
        }

        public static void StateChanged(object sender, StateEvent e)
        {
            //this.CurrentState.ToString($"n{this.ValuePrecision}")}

            if(e.LastState != null)
            {
                Log.Information($"{e.Id}: {e.State} (Last: {e.LastState?.ToString($"n{e.ValuePrecision}")}, Dur: {e.LastStateDuration.ToHHMMSS()}");
                
            }
            else
            {
                Log.Information($"{e.Id}: {e.State}");
            }
        }

        //public static void SwitchTests(Mcp230xx mcp)
        //{
        //    // set up the input/output modes
        //    mcp.Device.WriteByte(Register.IODIR, 0b1111_1111, Port.PortA); // Switch Matrix            
        //    mcp.Device.WriteByte(Register.GPPU, 0b1111_1111, Port.PortA); // pullups

        //    byte value;
        //    while (true)
        //    {
        //        value = mcp.Device.ReadByte(Register.GPIO, Port.PortA);

        //        Console.WriteLine($"B:{Convert.ToString(value, 2)}");
        //        Thread.Sleep(500);
        //    }
        //}

        //public static void LedTests(Mcp23s17 mcp)
        //{
        //    // set up the input/output modes
        //    mcp.Device.WriteByte(Register.IODIR, 0b0000_0000, Port.PortA); // Switch Matrix            
        //    mcp.Device.WriteByte(Register.IODIR, 0b000_0000, Port.PortB); // LEDs
        //    mcp.Device.WriteByte(Register.GPPU, 0b0000_0000, Port.PortB); // pullups

        //    for (byte i = 1; i <= 6; i++)
        //    {
        //        ShowSelection(mcp.Device, i);
        //        Thread.Sleep(200);
        //    }

        //    for (byte i = 6; i > 0; i--)
        //    {
        //        ShowSelection(mcp.Device, i);
        //        Thread.Sleep(200);
        //    }
            
        //    // COLS
        //    mcp.Device.WriteByte(Register.GPIO, 0b0000_1001, Port.PortB);
        //    Thread.Sleep(400);
        //    mcp.Device.WriteByte(Register.GPIO, 0b0001_0010, Port.PortB);
        //    Thread.Sleep(400);
        //    mcp.Device.WriteByte(Register.GPIO, 0b0010_0100, Port.PortB);
        //    Thread.Sleep(400);
        //    mcp.Device.WriteByte(Register.GPIO, 0b0001_0010, Port.PortB);
        //    Thread.Sleep(400);
        //    mcp.Device.WriteByte(Register.GPIO, 0b0000_1001, Port.PortB);
        //    Thread.Sleep(400);

        //    // ROWS
        //    mcp.Device.WriteByte(Register.GPIO, 0b0000_0111, Port.PortB);
        //    Thread.Sleep(400);
        //    mcp.Device.WriteByte(Register.GPIO, 0b0011_1000, Port.PortB);
        //    Thread.Sleep(400);
        //    mcp.Device.WriteByte(Register.GPIO, 0b0000_0111, Port.PortB);
        //    Thread.Sleep(400);

        //    mcp.Device.WriteByte(Register.GPIO, 0b1111_1111, Port.PortB);
        //    Thread.Sleep(1000);
        //    mcp.Device.WriteByte(Register.GPIO, 0b0000_0000, Port.PortB);
        //    Thread.Sleep(500);
        //}

        public static void McpTests(I2cBus i2cBus, GpioController gpioController)
        {
            var interruptPinA = gpioController.OpenPin(4, PinMode.Input);
            var interruptPinB = gpioController.OpenPin(5, PinMode.Input);
            
            var i2cDevice = i2cBus.CreateDevice(0x26);
            var expander = new GpioExpander(i2cDevice);

            expander.AttachInterrupts(interruptPinA, interruptPinB, true);

            var ledPin = expander.OpenPin(7, PinMode.Output, PinValue.High);
            var switchPin = expander.OpenPin(15, PinMode.InputPullUp);

            switchPin.ValueChanged += PinValueChanged;

            //// set up the input/output modes
            //mcp.Device.WriteByte(Register.IODIR, 0b0000_0000, Port.PortA); // Switch Matrix            
            //mcp.Device.WriteByte(Register.IODIR, 0b1111_1111, Port.PortB); // LEDs
            //mcp.Device.WriteByte(Register.GPPU, 0b1111_1111, Port.PortB); // pullups

            ////mcp.Device.WriteByte(Register.GPIO, 0b1111_0000, Port.PortB);

            //byte value;
            //while (true)
            //{
            //    value = mcp.Device.ReadByte(Register.GPIO, Port.PortB);

            //    Console.WriteLine($"B:{Convert.ToString(value, 2)}");
            //    Thread.Sleep(500);
            //}


            //SwitchTests(mcp);

            while (true)
            {
                expander.FetchStates();
                ledPin.Toggle();
                Thread.Sleep(1000);                
            }
        }        

        public static void PinValueChanged(object sender, PinValueChangedEventArgs e)
        {
            Console.WriteLine($"Pin Changed: {e.PinNumber}: {e.ChangeType}");
        }
       

        //public static void ShowSelection(Mcp23017 mcp)
        //{
        //    mcp.WriteByte(Register.GPIO, 0b0000_0000, Port.PortB);


        //    byte result = 0;
        //    result |= (byte)(bool1 ? 1 << 0 : 0);
        //    result |= (byte)(bool2 ? 1 << 1 : 0);
        //    result |= (byte)(bool3 ? 1 << 2 : 0);
        //    result |= (byte)(bool4 ? 1 << 3 : 0);
        //    result |= (byte)(bool5 ? 1 << 4 : 0);
        //    result |= (byte)(bool6 ? 1 << 5 : 0);
        //    result |= (byte)(bool7 ? 1 << 6 : 0);
        //    result |= (byte)(bool8 ? 1 << 7 : 0);

        //    mcp.WriteByte(Register.GPIO, 0b0000_0000, Port.PortB);
        //}


        public static void ShowSelection(Mcp23017 mcp, byte id)
        {            
            mcp.WriteByte(Register.GPIO, (byte)(1 << (byte)(id - 1)), Port.PortB);
        }

        public static void ShowRow(byte row)
        {
            
        }


        public static void I2cTests(I2cBus i2cBus) 
        {         
            // Bus scan to show that the device(s) are plugged in and seen
            Log.Information("================================================================================");
            Log.Information("Performing I2C Bus Scan...");

            var i2cDevices = i2cBus.PerformBusScan();            
            foreach (var i2cDeviceAddress in i2cDevices)
            {
                Log.Information($"+ Addr: {i2cDeviceAddress} (0x{i2cDeviceAddress:X})");
            }

            // TEST STACK:
            // - 0x76 - Bme688 (T,H,Vox,)
            // - 0x62 - Scd40 (T,H,CO2)
            // - 0x77 - BMP280 (T,P)
            // - 0x44 - SHT41 (T,H)
            // - 0x29 - Vl53l0xSensor (D)
            // - 0x12 - Pmsx003 (AQI)
            // - 0x69 - Sen5x (AQI)

            // M5 ENV IV Sensor
            // - 0x44 - SHT41 (T,H)
            // - 0x76 - BMP280 (T,P)
            

            var sensors = new List<I2cSensor>();
            //sensors.Add(new Bme680Sensor(i2cBus, 0x76, "BME680", true));
            //sensors.Add(new Scd40Sensor(i2cBus, 0x62));
            //sensors.Add(new Bmp280Sensor(i2cBus, 0x77));
            //sensors.Add(new Sht4xSensor(i2cBus, 0x44));
            //sensors.Add(new Vl53l0xSensor(i2cBus));
            //sensors.Add(new Vl53l1xSensor(i2cBus));
            
            //sensors.Add(new Pmsx003Sensor(i2cBus));
            //sensors.Add(new Sen5xSensor(i2cBus));
                        
            // M5 ENV IV Sensor Module
            //sensors.Add(new Sht4xSensor(i2cBus, 0x44, "SHT41", true));
            //sensors.Add(new Bmp280Sensor(i2cBus, 0x76, "BMP280", true));

            Log.Information("Initializing {SensorCount} Sensors...", sensors.Count);

            // Initialize all the sensors
            sensors.ForEach((sensor) => { sensor.Init(); });

            // show any errors 
            foreach (var sensor in sensors)
            {
                if (sensor.IsDisabled) { continue; }
                if (!sensor.Meta.ContainsKey("$error_init")) { continue; }

                Log.Information($"{sensor.Name}: {sensor["$error_init"]} (Not Initialized)");
            }

            Log.Information("================================================================================");
            Log.Information("Reading Sensor Telemetry...");
                        
            // DO SENSOR LOOP
            while (true)            
            {
                foreach (var sensor in sensors)
                {                    
                    if (!sensor.IsDisabled) { continue; }

                    if (sensor.FetchStates())
                    {
                        //Log.Information($"{sensor.Name}:");
                        //Log.Information(sensor.ToTelemetryString());                        
                    }
                }

                Thread.Sleep(1000);
            }
        }
    }
}