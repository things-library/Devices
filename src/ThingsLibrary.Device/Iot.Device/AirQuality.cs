﻿using UnitsNet;

namespace Iot.Device
{
    public class AirQuality
    {
        // ======================================================================
        // Standard Particles (PM1.0, PM2.5 and PM10.0) (µg/𝑚3)
        // ======================================================================
        // NOTE: "Standard" refers to the concentration "corrected" to the "standard atmosphere" which
        // in the US is DEFINED as "having a temperature of 288.15 K at the sea level 0 km geo-potential height and 1013.25 hPa"

        /// <summary>
        /// PM1.0 concentration unit: µg/𝑚3 (standard particle)
        /// </summary>
        public ushort Standard_Pm10 { get; init; }

        /// <summary>
        /// PM2.5 concentration unit: µg/𝑚3 (standard particle)
        /// </summary>
        public ushort Standard_Pm25 { get; init; }

        /// <summary>
        /// PM10.0 concentration unit: µg/𝑚3 (standard particle)
        /// </summary>
        public ushort Standard_Pm100 { get; init; }


        // ======================================================================
        // Atmospheric Environment
        // ======================================================================
        // NOTE: Atospheric values is the raw data as it is now (whatever temperature and pressure there is currently)
        // Air being a gas is compressible which means that it changes its volume when the pressure changes so when
        // you report concentrations as mass per volume of air it is relevant at what pressure that volume is calculated.

        /// <summary>
        /// PM1.0 concentration unit：µg/𝑚3 (under atmospheric environment)
        /// </summary>
        public ushort Environment_Pm10 { get; init; }

        /// <summary>
        /// PM2.5 concentration unit：µg/𝑚3 (under atmospheric environment)
        /// </summary>
        public ushort Environment_Pm25 { get; init; }

        /// <summary>
        /// PM10.0 concentration unit：µg/𝑚3 (under atmospheric environment)
        /// </summary>
        public ushort Environment_Pm100 { get; init; }


        // ======================================================================
        // Particle Concentrations (Number of particles with diameter beyond xx.x µ𝑚 in 0.1L of air)
        // ======================================================================

        /// <summary>
        /// Number of particles with diameter beyond 0.3 µ𝑚 in 0.1L of air
        /// </summary>
        public ushort Particles_03 { get; init; }

        /// <summary>
        /// Number of particles with diameter beyond 0.5 µ𝑚 in 0.1L of air
        /// </summary>
        public ushort Particles_05 { get; init; }

        /// <summary>
        /// Number of particles with diameter beyond 1.0 µ𝑚 in 0.1L of air
        /// </summary>
        public ushort Particles_10 { get; init; }

        /// <summary>
        /// Number of particles with diameter beyond 2.5 µ𝑚 in 0.1L of air
        /// </summary>
        public ushort Particles_25 { get; init; }

        /// <summary>
        /// Number of particles with diameter beyond 5.0 µ𝑚 in 0.1L of air
        /// </summary>
        public ushort Particles_50 { get; init; }

        /// <summary>
        /// Number of particles with diameter beyond 10.0 µ𝑚 in 0.1L of air
        /// </summary>
        public ushort Particles_100 { get; init; }
    }
}
