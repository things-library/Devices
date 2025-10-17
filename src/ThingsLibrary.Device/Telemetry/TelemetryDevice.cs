using ThingsLibrary.Schema.Library.Telemetry;

namespace ThingsLibrary.Device.Telemetry
{
    public class TelemetryDevice
    {
        /// <summary>
        /// I2C Bus Sensors such as humidit, pressure, gas, pressure, voc, nox, 
        /// </summary>
        public List<ISensor> Sensors { get; set; }


        public int UpdateInterval { get; set; }

        public DateTimeOffset NextReportOn { get; set; } = DateTimeOffset.Now;  //assume we haven't sent one yet

        /// <summary>
        /// Periodic sensor pool for updates to sensor values based on sensor update intervals
        /// </summary>
        public void Fetch()
        {

        }

        /// <summary>
        /// List of events that have occured
        /// </summary>
        public Queue<TelemetryEventDto> Events { get; set; } = new Queue<TelemetryEventDto>();

        /// <summary>
        /// Clear all Telemetry Events, if date provided it will clear all older than the provided date
        /// </summary>
        /// <param name="toDate"></param>
        public void ClearEvents(DateTimeOffset? expirationDate = null)
        {
            if(expirationDate != null)
            {
                for (int i = 0; i < this.Events.Count; i++)
                {
                    // all done?
                    if (this.Events.Peek().Date >= expirationDate) { return; }

                    // remove the item off the top
                    this.Events.Dequeue();
                }
            }
            else 
            {
                this.Events.Clear();
            }
        }
    }
}
