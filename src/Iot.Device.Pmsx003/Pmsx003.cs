using Iot.Device.Pmsx003.Extensions;
using System.Buffers.Binary;
using System.IO.Ports;
using UnitsNet;

namespace Iot.Device.Pmsx003
{
    /// <summary>
    /// PMSx003 Series - PM2.5 AQI (Air quality sensor)
    /// </summary>
    /// <remarks> 
    /// Stable data should be got at least 30 seconds after the sensor wakeup from the sleep mode because of the fan’s performance.
    /// 
    /// Mainly output as the quality and number of each particles with different size per unit volume, the unit volume of 
    /// particle number is 0.1L and the unit of mass concentration is μg/m³. 
    /// 
    /// There are two options for digital output: passive and active.
    /// 
    /// Default mode is active after power up. In this mode sensor would send serial data to the host automatically. 
    /// The active mode is divided into two sub-modes: stable mode and fast mode. 
    /// If the concentration change is small the sensor would run at stable mode with the real interval of 2.3s. 
    /// And if the change is big the sensor would be changed to fast mode automatically with the interval of 200~800ms, 
    /// the higher of the concentration, the shorter of the interval.
    /// </remarks>
    public sealed class Pmsx003 : IDisposable
    {
        /// NOTE: This is a Plantower Sensor
        /// http://github.com/adafruit/Adafruit_PM25AQI
        /// http://github.com/adafruit/Adafruit_CircuitPython_PM25

        /// <summary>
        /// The default I²C address of this device.
        /// </summary>
        public const byte DefaultI2cAddress = 0x12; // PMSA003I has only one I2C address (18 or 0x12)

        public I2cDevice? I2cDevice { get; internal set; }
        public SerialPort? SerialPort { get; internal set; }

        /// <summary>
        /// Last Error Message that occured.  This is cleared every time there is a Read() event
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        private bool IsDisposed { get; set; } = false;

        //public int ResetPin { get; set; }       // Module signal reset. Reset low.
        //public int SetPin { get; set; }         // Set pin. High when working status, low level is sleeping mode.

        //private byte Version { get; set; }
        //private byte ErrorCode { get; set; }
        //private ushort Checksum { get; set; }         // Packet checksum

        /// <summary>
        /// Instantiates a new <see cref="Pmsx003"/>
        /// </summary>
        /// <param name="i2cDevice">The I²C device to operate on.</param>
        public Pmsx003(I2cDevice i2cDevice)
        {
            this.I2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));

            //TODO: read device signature 

        }

        /// <summary>
        /// Instantiates a new <see cref="Pmsx003"/>
        /// </summary>
        /// <param name="i2cDevice">The I²C device to operate on.</param>
        public Pmsx003(SerialPort serialPort)
        {
            this.SerialPort = serialPort ?? throw new ArgumentNullException(nameof(serialPort));

            //TODO: read device signature 

        }



        /// <summary>
        /// Reads data from the device
        /// </summary>
        /// <returns><see cref="AirQuality"/> on success, null on CRC failure</returns>
        public AirQuality? Read()
        {
            // Reference: https://cdn-shop.adafruit.com/product-files/4632/4505_PMSA003I_series_data_manual_English_V2.6.pdf
            //  Section 5, Register Definition

            // see if we are actually using UART.. otherwise default to I2C
            if (this.SerialPort != null)
            {
                return this.ReadUart();
            }
            else
            {
                return this.ReadI2c();
            }

        }

        /// <summary>
        /// Reads data from the device over I2C
        /// </summary>
        /// <returns><see cref="AirQuality"/> on success, null on CRC failure</returns>
        private AirQuality? ReadI2c()
        {
            // Reference: https://cdn-shop.adafruit.com/product-files/4632/4505_PMSA003I_series_data_manual_English_V2.6.pdf
            //  Section 5, Register Definition

            Span<Byte> data = new byte[2];
            //BinaryPrimitives.WriteUInt16BigEndian(data, (ushort)cmd);
            this.I2cDevice!.Write(data);

            //this.I2cDevice!.WriteByte((byte)0x00);
            //_ = this.I2cDevice.ReadByte();

            Thread.Sleep(TimeSpan.FromMilliseconds(20));

            // read all at once since we are dealing with a checksum
            Span<byte> readBuffer = stackalloc byte[32];
            this.I2cDevice!.Read(readBuffer);

            return this.ParseBuffer(readBuffer);
        }

        /// <summary>
        /// Reads data from the device over UART
        /// </summary>
        /// <returns><see cref="AirQuality"/> on success, null on CRC failure</returns>
        public AirQuality? ReadUart()
        {
            //https://github.com/adafruit/Adafruit_PM25AQI/blob/master/Adafruit_PM25AQI.h

            // Determine which sensor we are reading from.. Cubic PM1006 only has 20 byte buffer (last 12 ignored)


            throw new NotImplementedException("UART interface is not yet implemented.");
        }


        /// <summary>
        /// Reads data from the device
        /// </summary>
        /// <returns><see cref="AirQuality"/> on success, null on CRC failure</returns> 
        private AirQuality? ParseBuffer(Span<byte> readBuffer)
        {
            this.ErrorMessage = string.Empty;

            // calculate checksum
            if (!CrcCheck(readBuffer))
            {
                this.ErrorMessage = "CRC Check failed on read buffer.";
                return null;
            }

            var frameLength = ToUshort(readBuffer[(byte)Pmsx003Register.FRAME_HIGH], readBuffer[(byte)Pmsx003Register.FRAME_LOW]);
            Debug.WriteLine($"Frame Length: {frameLength}");

            var airQuality = new AirQuality
            {
                // Standard Particles   
                StandardPm10 = MassConcentration.FromMicrogramsPerCubicMeter(readBuffer.ToUshort(0x04)),  //PM1.0 concentration unit: µg /𝑚3
                StandardPm25 = MassConcentration.FromMicrogramsPerCubicMeter(readBuffer.ToUshort(0x06)),  //PM2.5 concentration unit：µg /𝑚3
                StandardPm100 = MassConcentration.FromMicrogramsPerCubicMeter(readBuffer.ToUshort(0x08)), //PM10 concentration unit：µg /𝑚3 

                // Atmospheric Environment
                EnvironmentPm10 = MassConcentration.FromMicrogramsPerCubicMeter(readBuffer.ToUshort(0x0a)),  //PM1.0 concentration unit：µg /𝑚3 (under atmospheric environment)
                EnvironmentPm25 = MassConcentration.FromMicrogramsPerCubicMeter(readBuffer.ToUshort(0x0c)),  //PM2.5 concentration unit：µg /𝑚3 (under atmospheric environment)
                EnvironmentPm100 = MassConcentration.FromMicrogramsPerCubicMeter(readBuffer.ToUshort(0x0e)), //PM10.0 concentration unit：µg /𝑚3 (under atmospheric environment)

                // Particle Concentrations (Number of particles with diameter beyond xx.x µ𝑚 in 0.1L of air)
                Particles03 = readBuffer.ToUshort(0x10),  //  > 0.3 µ𝑚
                Particles05 = readBuffer.ToUshort(0x12),  //  > 0.5 µ𝑚
                Particles10 = readBuffer.ToUshort(0x14),  //  > 1.0 µ𝑚
                Particles25 = readBuffer.ToUshort(0x16),  //  > 2.5 µ𝑚
                Particles50 = readBuffer.ToUshort(0x18),  //  > 5.0 µ𝑚
                Particles100 = readBuffer.ToUshort(0x1a)  // > 10.0 µ𝑚 
            };

            Debug.WriteLine($"Version: {readBuffer[0x1c]}");
            Debug.WriteLine($"Error Code: {readBuffer[0x1d]}");
            Debug.WriteLine($"Checksum: {readBuffer.ToUshort(0x1e)}");

            return airQuality;
        }

        private bool CrcCheck(Span<byte> readBuffer)
        {
            // Check that start bytes are correct! (these are fixed values by the manufacture)
            // The data comes in Endian so largest bit then smallest (register mapping below instead of assumptions)
            if (readBuffer[(byte)Pmsx003Register.DIGIT_1] != (byte)Pmsx003Register.FIXED_DIGIT_1 ||
                readBuffer[(byte)Pmsx003Register.DIGIT_2] != (byte)Pmsx003Register.FIXED_DIGIT_1)
            {
                this.ErrorMessage = "Fixed registers mismatch.";
                return false;
            }

            var frameLength = ToUshort(readBuffer[(byte)Pmsx003Register.FRAME_HIGH], readBuffer[(byte)Pmsx003Register.FRAME_LOW]);
            Debug.WriteLine($"Frame Length: {frameLength}");

            // calculate checksum
            ushort checksum = 0;
            for (byte i = 0; i < 30; i++)   //don't include the checksum in the calculation
            {
                checksum += readBuffer[i];
            }

            // no point on continuing if the checksum doesn't 'check' out :)
            var expectedCrc = readBuffer.ToUshort(0x1e); // (ushort)(readBuffer[0x1e] << 8 | readBuffer[0x1f]);
            if (checksum != expectedCrc)
            {
                this.ErrorMessage = $"CRC does not match.  Expected: {expectedCrc}, Calculated: {checksum}";
                return false;
            }

            // success! 
            return true;
        }

        private static ushort ToUshort(byte hi, byte low)
        {
            // The data comes in Endian so largest bit then smallest (register mapping below instead of assumptions)

            return (ushort)(hi << 8 | low);
        }

        /// <summary>
        /// Cleanup resources.
        /// </summary>        
        public void Dispose()
        {
            if (this.IsDisposed) { return; }

            // clear the memory reference
            if (this.SerialPort != null)
            {
                this.SerialPort.Dispose();
                this.SerialPort = null;
            }

            if (this.I2cDevice != null)
            {
                this.I2cDevice.Dispose();
                this.I2cDevice = null;
            }

            GC.SuppressFinalize(this);

            this.IsDisposed = true;
        }
    }
}
