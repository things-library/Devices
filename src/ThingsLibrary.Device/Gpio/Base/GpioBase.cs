using System.Globalization;

namespace ThingsLibrary.Device.Gpio.Base
{
    /// <summary>
    /// GPIO base class
    /// </summary>
    public abstract class GpioBase : IRootItemDto, IGpioDevice
    {
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
        /// Keep track of the bool state
        /// </summary>
        public BoolState BoolState { get; init; } = new BoolState();
       
        #region --- Interface Properties ---

        /// <inheritdoc />        
        public GpioController Controller { get; init; }

        /// <inheritdoc />        
        public int PinId { get; init; }

        /// <inheritdoc />        
        public PinValue State
        {
            get => _state;
            set
            {
                // nothing is changing?
                if (_state == value) { return; }

                _state = value;
                this.StateChangedOn = DateTimeOffset.UtcNow;
            }
        }
        private PinValue _state;

        /// <inheritdoc />        
        public DateTimeOffset StateChangedOn { get; set; }

        /// <inheritdoc />        
        public bool IsDisabled { get; set; }

        /// <inheritdoc />        
        public bool IsInit { get; set; }

        /// <inheritdoc />        
        public bool IsHigh => (this.State == PinValue.High);

        /// <inheritdoc />
        public bool IsLow => (this.State == PinValue.Low);

        #endregion

        /// <summary>
        /// What state is considered 'normal' or default
        /// </summary>
        public bool IsNormallyLow { get; init; } = false;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gpioController">GPIO Controller</param>
        /// <param name="pinId">Board Pin Number</param>
        /// <param name="key">Unique key for the sensor</param>
        /// <param name="type">Type of sensor</param>
        /// <param name="displayName">Display Name</param>
        /// <param name="isNormallyLow">Is the devive normally low?</param>
        protected GpioBase(GpioController gpioController, int pinId, string key, string displayName, string type, bool isNormallyLow)
        {
            this.Controller = gpioController ?? throw new ArgumentNullException(nameof(gpioController));

            this.PinId = pinId;
            
            this.Key = key;
            this.Name = displayName;
            this.Type = type;

            this.IsNormallyLow = isNormallyLow;            
        }

        protected GpioBase(GpioController gpioController, string key, IItemDto settings)
        {
            this.Controller = gpioController ?? throw new ArgumentNullException(nameof(gpioController));

            // DEVICE ID            
            this.PinId = this.GetPinId(settings["pin"]);

            // SENSOR Object
            this.Type = settings.Type;
            this.Key = key;
            this.Name = settings.Name;
            this.Tags = settings.Tags;
            this.Meta = settings.Meta;

            if (settings.Tags.ContainsKey("disabled")) { this.IsDisabled = bool.Parse(settings["disabled"]); }
            if (settings.Tags.ContainsKey("is_normally_low")) { this.IsNormallyLow = bool.Parse(settings["is_normally_low"]); }

            // set the labels for the state
            if (settings.Tags.ContainsKey("normal")) { this.BoolState.NormalLabel = settings["normal"]; }
            if (settings.Tags.ContainsKey("fault")) { this.BoolState.FaultedLabel = settings["fault"]; }
        }

        /// <summary>
        /// Parse the device id based on if it is a hex string or a integer
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private int GetPinId(string id)
        {
            int deviceId;

            // see if we have a hex value
            if (id.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                id = id.Substring(2); // Remove "0x"
                if (!int.TryParse(id, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out deviceId)) { throw new ArgumentException("Unable to parse 'pin' to integer."); }
            }
            else
            {
                if (!int.TryParse(id, out deviceId)) { throw new ArgumentException("Unable to parse 'pin' to integer."); }
            }

            return deviceId;
        }

        /// <summary>
        /// Set up the device and enable it if requested
        /// </summary>
        public abstract bool Init();
    }
}