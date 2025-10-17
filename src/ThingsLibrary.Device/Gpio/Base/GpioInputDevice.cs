namespace ThingsLibrary.Device.Gpio.Base
{
    public class GpioInputDevice : GpioBase
    {
        //public bool IsEventCallback { get; set; }

        /// <summary>
        /// If a pullup resistor should be used
        /// </summary>
        public bool IsPullUp { get; set; }

        /// <summary>
        /// If a pulldown resistor should be used
        /// </summary>
        public bool IsPullDown { get; set; }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="controller"><see cref="GpioController"/></param>
        /// <param name="pinId">Pin Number</param>
        /// <param name="displayName">Name of the device</param>
        /// <param name="type">Type</param>
        /// <param name="isPullUp">If a pullup resistor should be used, false if a pull down resistor should be used, null if neither</param>
        public GpioInputDevice(GpioController controller, ushort pinId, string key, string displayName, string type, bool? isPullUp) : base(controller, pinId, key, displayName, type, (!isPullUp ?? true))
        {
            // do we have a value
            if (isPullUp != null)
            {
                this.IsPullUp = isPullUp.Value;
                this.IsPullDown = !isPullUp.Value;
            }            
        }

        public GpioInputDevice(GpioController controller, string key, IItemDto settings) : base(controller, key, settings)
        {
            bool isPullUp;
            _ = bool.TryParse(settings["is_pullup"], out isPullUp);

            bool isPullDown;
            _ = bool.TryParse(settings["is_pulldown"], out isPullDown);

            if(isPullUp)
            {
                this.IsPullUp = true;
                this.IsPullDown = false;
            }
            else if(isPullDown)
            {                
                this.IsPullUp = false;
                this.IsPullDown = true;
            }            
        }


        /// <summary>
        /// Inititalize the input device
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public override bool Init()
        {
            try
            {
                var pinMode = PinMode.Input;

                if (this.IsPullUp) { pinMode = PinMode.InputPullUp; }
                else if (this.IsPullDown) { pinMode = PinMode.InputPullDown; }

                if (!this.Controller.IsPinModeSupported(this.PinId, pinMode))
                {
                    throw new ArgumentException($"Pin Mode '{pinMode}' not supported on pin # '{this.PinId}'.");
                }

                //init the pin            
                this.Controller.OpenPin(this.PinId, pinMode);
                                
                this.IsInit = true;

                return true;
            }
            catch(Exception ex)
            {
                this.Meta["$error_init"] = ex.Message;
                this.IsDisabled = true; //disable the device

                return false;
            }            
        }        
    }
}
