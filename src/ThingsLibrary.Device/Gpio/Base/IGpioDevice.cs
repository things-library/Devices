namespace ThingsLibrary.Device.Gpio.Base
{
    /// <summary>
    /// GPIO Device Interface
    /// </summary>
    public interface IGpioDevice
    {
        /// <summary>
        /// GPIO Controller
        /// </summary>
        public GpioController Controller { get; init; }

        /// <summary>
        /// Pin # on the board
        /// </summary>
        public int PinId { get; }

        /// <summary>
        /// Unique device key
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Display Name for the device
        /// </summary>
        public string Name { get; }
                
        /// <summary>
        /// Set the state
        /// </summary>
        public PinValue State { get; }

        /// <summary>
        /// Last time the state changed
        /// </summary>
        public DateTimeOffset StateChangedOn { get; }

        /// <summary>
        /// If the device is currently enabled
        /// </summary>
        public bool IsDisabled { get; }

        /// <summary>
        /// Is State High
        /// </summary>
        public bool IsHigh { get; }

        /// <summary>
        /// Is State Low
        /// </summary>
        public bool IsLow { get; }
    }
}
