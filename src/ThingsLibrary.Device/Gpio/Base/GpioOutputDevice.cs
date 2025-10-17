namespace ThingsLibrary.Device.Gpio.Base
{
    /// <summary>
    /// GPIO Output Device
    /// </summary>
    public class GpioOutputDevice : GpioBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="controller"><see cref="GpioController"/></param>
        /// <param name="pinId">Pin Number</param>
        /// <param name="type">Type of device</param>
        /// <param name="key">Unique device key</param>
        /// <param name="name">Name of the device</param>
        /// <param name="isNormallyLow">If the device is normally low state</param>
        public GpioOutputDevice(GpioController controller, ushort pinId, string type, string key, string name, bool isNormallyLow) : base(controller, pinId, type, key, name, isNormallyLow)
        {           
            //this.IsNormallyLow = isNormallyLow;
        }

        /// <summary>
        /// Initialize the device
        /// </summary>        
        public override bool Init()
        {   
            //set pin mode
            this.Controller.OpenPin(this.PinId, PinMode.Output, (this.IsNormallyLow ? PinValue.Low : PinValue.High));

            return true;
        }

        /// <summary>
        /// Set the device to high state
        /// </summary>
        public virtual void High()
        {
            if (!this.IsDisabled) { return; }
            if (this.State == PinValue.High) { return; } //already set

            this.Controller.Write(this.PinId, PinValue.High);

            // get the state off the pin
            this.State = this.Controller.Read(this.PinId);
        }

        /// <summary>
        /// Set the device to the low state
        /// </summary>
        public virtual void Low()
        {
            if (!this.IsDisabled) { return; }
            if (this.State == PinValue.Low) { return; } //already set

            this.Controller.Write(this.PinId, PinValue.Low);

            // get the state off the pin
            this.State = this.Controller.Read(this.PinId);
        }

        /// <summary>
        /// Turn on the output
        /// </summary>
        public void On()
        {
            if (this.IsNormallyLow) { this.High(); }
            else { this.Low(); }
        }

        /// <summary>
        /// Turn off the output
        /// </summary>
        public void Off()
        {
            if (this.IsNormallyLow) { this.Low(); }
            else { this.High(); }
        }

        /// <summary>
        /// Is Output On
        /// </summary>
        public bool IsOn => (this.IsNormallyLow ? this.State == PinValue.High : this.State == PinValue.Low);
        
        /// <summary>
        /// Is Output Off
        /// </summary>
        public bool IsOff => (this.IsNormallyLow ? this.State == PinValue.Low : this.State == PinValue.High);
    }
}
