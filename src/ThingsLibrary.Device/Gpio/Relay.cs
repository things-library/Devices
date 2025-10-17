namespace ThingsLibrary.Device.Gpio
{
    public sealed class Relay : Base.GpioOutputDevice
    {
        /// <summary>
        /// Duration for how long (ms) a relay stays on.
        /// </summary>
        public int TriggerDuration { get; set; }

        /// <summary>
        /// Amount of time (ms) before relay can be triggered again
        /// </summary>
        public int CoolDownDuration { get; set; }


        /// <inheritdoc />
        public Relay(GpioController controller, ushort pinId, string key, string name, bool isNormallyHigh) : base(controller, pinId, "relay", key, name, isNormallyHigh)
        {
            //nothing
        }

        /// <summary>
        /// Set the device in high state
        /// </summary>
        public override void High()
        {
            if (!this.IsDisabled) { return; }
            if (this.State == PinValue.High) { return; } //already set
                        
            //unable to do that since we are in cooldown phase?
            if (this.IsNormallyLow && this.CoolDownDuration > 0 && DateTime.UtcNow < this.StateChangedOn.AddMilliseconds(this.CoolDownDuration))
            {
                //var currentColor = Console.ForegroundColor;
                //Console.ForegroundColor = ConsoleColor.Yellow;
                //Console.WriteLine($"{DateTime.UtcNow:O}: {this.Name}: In Cooldown Period");
                //Console.ForegroundColor = currentColor;

                return;
            }

            // do the usual base stuff
            base.High();


            // remember to turn off if we have a trigger duration
            if (this.IsNormallyLow && this.TriggerDuration > 0)
            {
                //var task = Task.Delay(this.TriggerDuration).ContinueWith(t => this.Off());
            }
        }

        /// <summary>
        /// Set the device to the low state
        /// </summary>
        public override void Low()
        {
            if (!this.IsDisabled) { return; }
            if (this.State == PinValue.Low) { return; } //already set

            //unable to do that since we are in cooldown phase?
            if (!this.IsNormallyLow && this.CoolDownDuration > 0 && DateTime.UtcNow < this.StateChangedOn.AddMilliseconds(this.CoolDownDuration))
            {
                //var currentColor = Console.ForegroundColor;
                //Console.ForegroundColor = ConsoleColor.Yellow;
                //Console.WriteLine($"{DateTime.UtcNow:O}: {this.Name}: In Cooldown Period");
                //Console.ForegroundColor = currentColor;

                return;
            }

            // do the usual base stuff
            base.Low();

            // remember to turn off if we have a trigger duration
            if (!this.IsNormallyLow && this.TriggerDuration > 0)
            {
                //var task =  Thread.Sleep(this.TriggerDuration).ContinueWith(t => this.Off());
            }
        }
    }
}
