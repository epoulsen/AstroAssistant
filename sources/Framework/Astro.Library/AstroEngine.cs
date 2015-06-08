﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Astro
{
    /// <summary>
    /// Astrology engine
    /// </summary>
    public class AstroEngine : IDisposable
    {
        IEphemerisProvider _EphemerisProvider;

        #region Ctors & Dest

        /// <summary>
        /// Nouveau moteur
        /// </summary>
        public AstroEngine(IEphemerisProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");
            _EphemerisProvider = provider;
        }

        /// <summary>
        /// Libération interne des ressources
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                _EphemerisProvider.Dispose();
        }

        /// <summary>
        /// Libération des ressources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        #region Calcul de thème

        /// <summary>
        /// Calcul un thème natal
        /// </summary>
        public NatalChart CalculateNatalChart(NatalChartDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException();

            // Préparation du résultat
            NatalChart result = new NatalChart {
                Definition = definition
            };

            // Initialisation du provider pour le calcul
            EphemerisProvider.SetTopographic(definition.PositionCenter, definition.BirthPlacePosition);
            EphemerisProvider.HouseSystem = definition.HouseSystem;

            // Calcul des dates et des temps
            result.JulianDay = EphemerisProvider.ToJulianDay(definition.BirthDate);
            result.UniversalTime = EphemerisProvider.ToUniversalTime(result.JulianDay);
            result.EphemerisTime = EphemerisProvider.ToEphemerisTime(result.JulianDay);
            result.SideralTime = EphemerisProvider.ToSideralTime(result.JulianDay, definition.BirthPlacePosition.Longitude);

            // Calculs
            var enValues = EphemerisProvider.CalcEclipticNutation(result.EphemerisTime);
            result.TrueEclipticObliquity = enValues.TrueEclipticObliquity;
            result.MeanEclipticObliquity = enValues.MeanEclipticObliquity;
            result.NutationLongitude = enValues.NutationLongitude;
            result.NutationObliquity = enValues.NutationObliquity;

            // Planets
            foreach (var planet in definition.Planets)
            {
                if (planet == Planet.EclipticNutation) continue;
                if (planet == Planet.Earth && (definition.PositionCenter == PositionCenter.Geocentric || definition.PositionCenter == PositionCenter.Topocentric))
                    continue;   // Exclude Earth if geo or topo
                var pi = EphemerisProvider.CalcPlanet(planet, result.EphemerisTime, result.ARMC, definition.BirthPlacePosition.Longitude, result.TrueEclipticObliquity);
                result.Planets.Add(pi);
            }
            ///*
            //    //* equator position * /
            //    if (fmt.IndexOfAny("aADdQ".ToCharArray()) >= 0) {
            //        iflag2 = iflag | SwissEph.SEFLG_EQUATORIAL;
            //        if (ipl == SwissEph.SE_FIXSTAR)
            //            iflgret = sweph.swe_fixstar(star, tjd_et, iflag2, xequ, ref serr);
            //        else
            //            iflgret = sweph.swe_calc(tjd_et, ipl, iflag2, xequ, ref serr);
            //    }
            //    //* ecliptic cartesian position * /
            //    if (fmt.IndexOfAny("XU".ToCharArray()) >= 0) {
            //        iflag2 = iflag | SwissEph.SEFLG_XYZ;
            //        if (ipl == SwissEph.SE_FIXSTAR)
            //            iflgret = sweph.swe_fixstar(star, tjd_et, iflag2, xcart, ref serr);
            //        else
            //            iflgret = sweph.swe_calc(tjd_et, ipl, iflag2, xcart, ref serr);
            //    }
            //    //* equator cartesian position * /
            //    if (fmt.IndexOfAny("xu".ToCharArray()) >= 0) {
            //        iflag2 = iflag | SwissEph.SEFLG_XYZ | SwissEph.SEFLG_EQUATORIAL;
            //        if (ipl == SwissEph.SE_FIXSTAR)
            //            iflgret = sweph.swe_fixstar(star, tjd_et, iflag2, xcartq, ref serr);
            //        else
            //            iflgret = sweph.swe_calc(tjd_et, ipl, iflag2, xcartq, ref serr);
            //    }
            //    spnam = se_pname;
            // */

            // Houses
            var houses = EphemerisProvider.CalcHouses(result.JulianDay, definition.BirthPlacePosition.Latitude, definition.BirthPlacePosition.Longitude, result.AscMcs);
            result.Houses.AddRange(houses);

            return result;
        }

        #endregion

        #region Propriétés

        /// <summary>
        /// Fournisseur d'éphémérides
        /// </summary>
        public IEphemerisProvider EphemerisProvider { get { return _EphemerisProvider; } }

        #endregion

    }

}
