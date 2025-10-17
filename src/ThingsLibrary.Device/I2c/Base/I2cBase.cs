// Check Raspberry Pi I2C devices by adding i2c-tools:
//   sudo apt install i2c-tools
//   sudo i2cdetect -y 1

using System.Device.Gpio;
using System.Globalization;

namespace ThingsLibrary.Device.I2c.Base
{
    /// <summary>
    /// Base I2C Inherited Class
    /// </summary>
    public abstract class I2cBase
    {
        //public static int DefaultAddress { get; set; }
        //public static int? SecondaryAddress { get; set; }

        #region --- Item Properties ---

        /// <summary>
        /// Item Type - describes what type of item we are talking about.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Collection unique key
        /// </summary>        
        public string Key { get; set; }

        /// <summary>
        /// Display Name of the sensor
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Tags
        /// </summary> 
        /// <remarks>Value must be a string or array of strings</remarks>        
        public IDictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Generic metadata which is a simple key-value dictionary
        /// </summary>        
        public IDictionary<string, string> Meta { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Easy lookup and empty string lookup
        /// </summary>
        /// <param name="key">Dictionary Key</param>
        /// <param name="isMeta">If the value from metadata</param>
        /// <returns></returns>
        public string this[string key, bool isMeta = false]
        {
            get
            {
                if (isMeta)
                {
                    if (!this.Meta.ContainsKey(key)) { return string.Empty; }

                    return this.Meta[key];
                }
                else
                {
                    if (!this.Tags.ContainsKey(key)) { return string.Empty; }

                    return this.Tags[key];
                }
            }
        }

        #endregion

        /// <summary>
        /// I2C Bus
        /// </summary>
        public I2cBus I2cBus { get; init; }

        /// <summary>
        /// I2C Device
        /// </summary>
        public I2cDevice I2cDevice { get; set; }

        /// <summary>
        /// The address id of the device on the I2C bus
        /// </summary>
        public int AddressId { get; private set; }
                             
        /// <summary>
        /// Is the device currently disabled?
        /// </summary>
        public bool IsDisabled { get; set; }

        /// <inheritdoc />        
        public bool IsInit { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="i2cBus">I2C Bus</param>
        /// <param name="addressId">Device Address</param>
        /// <param name="key">Uniue key</param>
        /// <param name="name">Device Name</param>
        public I2cBase(I2cBus i2cBus, int addressId, string type, string key, string name)
        {
            //0x00-0x07 and 0xF0-0xFF
            if (addressId <= 7 && addressId >= 240) { throw new ArgumentException($"Invalid I2C address: {addressId}.  Id must be between 7 and 240 (0x07-0f0)", "id"); }

            this.I2cBus = i2cBus;

            this.AddressId = addressId;   // address Id
            this.Key = key;
            this.Name = name;
            this.Type = type;
        }


        public I2cBase(I2cBus i2cBus, string key, IItemDto settings)
        {
            this.I2cBus = i2cBus ?? throw new ArgumentNullException(nameof(i2cBus));
            
            // DEVICE ID            
            this.AddressId = this.GetDeviceId(settings["device_id"]);

            // SENSOR Object
            this.Type = settings.Type;
            this.Key = key;
            this.Name = settings.Name;
            this.Tags = settings.Tags;
            this.Meta = settings.Meta;

            if (settings.Tags.ContainsKey("disabled"))
            {
                this.IsDisabled = bool.Parse(settings["disabled"]);
            }
        }

        /// <summary>
        /// Parse the device id based on if it is a hex string or a integer
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private int GetDeviceId(string id)
        {
            int deviceId;

            // see if we have a hex value
            if (id.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                id = id.Substring(2); // Remove "0x"
                if (!int.TryParse(id, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out deviceId)) { throw new ArgumentException("Unable to parse 'device_id' to integer."); }
            }
            else
            {
                if (!int.TryParse(id, out deviceId)) { throw new ArgumentException("Unable to parse 'device_id' to integer."); }                
            }

            return deviceId;
        }

        /// <summary>
        /// Initialize Device
        /// </summary>
        /// <param name="enableDevice"></param>
        public virtual bool Init()
        {         
            this.I2cDevice = this.I2cBus.CreateDevice(this.AddressId);

            return true;
        }        
    }
}
