﻿using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Astro.Library.Tests
{
    public class AstroEngineTest
    {
        [Fact]
        public void TestCreate()
        {
            var mockProvider = new Mock<IEphemerisProvider>();
            var provider = mockProvider.Object;

            using (var engine = new AstroEngine(provider))
            {
                Assert.Same(provider, engine.EphemerisProvider);
            }

            mockProvider.Verify(p => p.Dispose(), Times.Once());

            Assert.Throws<ArgumentNullException>(() => new AstroEngine(null));
        }

        [Fact]
        public void TestCalculateNatalChart()
        {
            var mockProvider = new Mock<IEphemerisProvider>();
            mockProvider.Setup(p => p.CalcEclipticNutation(It.IsAny<EphemerisTime>())).Returns(new EclipticNutationValues {
                MeanEclipticObliquity = 1,
                NutationLongitude = 2,
                NutationObliquity = 3,
                TrueEclipticObliquity = 4
            });
            var provider = mockProvider.Object;
            using (var engine = new AstroEngine(provider))
            {
                var def = new NatalChartDefinition {
                    Name = "Test",
                    BirthPlacePosition = new GeoPosition {
                        Longitude = 47.2,
                        Latitude = 3.23,
                        Altitude = 123
                    },
                    PositionCenter = PositionCenter.Topocentric
                };
                def.BirthDate.Year = 2015;
                def.BirthDate.Month = 6;
                def.BirthDate.Day = 7;
                def.BirthDate.Hour = 14;
                def.BirthDate.Minute = 6;
                def.BirthDate.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");
                def.Planets.Add(Planet.EclipticNutation);
                def.Planets.Add(Planet.Earth);
                Assert.Equal(17, def.Planets.Count);

                var theme = engine.CalculateNatalChart(def);
                Assert.Same(def, theme.Definition);

                Assert.Equal(0, theme.JulianDay);
                Assert.Equal(new DateTime(), theme.UniversalTime);
                Assert.Equal(0d, theme.EphemerisTime);
                Assert.Equal(0d, theme.DeltaTSec);
                Assert.Equal(3.14666666666667, (Double)theme.SideralTime, 14);
                Assert.Equal(47.2, (Double)theme.SideralTimeInDegrees, 14);
                Assert.Equal(47.2, (Double)theme.ARMC, 14);
                Assert.Equal(1, theme.MeanEclipticObliquity);
                Assert.Equal(2, theme.NutationLongitude);
                Assert.Equal(3, theme.NutationObliquity);
                Assert.Equal(4, theme.TrueEclipticObliquity);

                Assert.Equal(14, theme.Planets.Count);

                Assert.Throws<ArgumentNullException>(() => engine.CalculateNatalChart(null));
            }

        }

    }
}
