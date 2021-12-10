using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.Utility;

namespace NitroxServer.GameLogic
{
    [TestClass]
    public class EventTriggererTest
    {
        [TestMethod]
        public void VerifyEventsOrder()
        {
            EventTriggerer eventTriggerer = new(null, 0.0, null);
            List<string> eventsOrder = new() { "Story_AuroraWarning1", "Story_AuroraWarning2", "Story_AuroraWarning3", "Story_AuroraWarning4", "Story_AuroraExplosion" };
            List<string> eventTriggererEvents = new(eventTriggerer.eventTimers.Keys);

            Assert.AreEqual(eventsOrder.Count, eventTriggererEvents.Count, "eventsOrder and eventTriggererEvents are not the same size.");
            for (int i = 0; i < eventsOrder.Count; i++)
            {
                Assert.AreEqual(eventsOrder[i], eventTriggererEvents[i], $"The {i} item of eventsOrder was not equals to eventTriggererEvents");
            }
        }

        [TestMethod]
        public void AuroraExplosionAndWarningTime()
        {
            EventTriggerer eventTriggerer = new(null, 0.0, 30d);
            Assert.AreEqual(eventTriggerer.eventTimers["Story_AuroraWarning4"].Interval, eventTriggerer.eventTimers["Story_AuroraExplosion"].Interval);
        }

        [TestMethod]
        public void RealElapsedTimeMeasureUnit()
        {
            EventTriggerer eventTriggerer = new(null,20d, null);
            eventTriggerer.stopWatch.amplifiedElapsedMilliseconds = 500L;
            Assert.AreEqual(eventTriggerer.stopWatch.ElapsedMilliseconds + eventTriggerer.elapsedTimeOutsideStopWatch, eventTriggerer.ElapsedTime);
        }

        [TestMethod]
        public void TestSpeedChanges()
        {
            const double INITIAL_INTERVAL = 100000d;
            long elapsedTime = 50000L;
            AdjustableTimer timer = new() {
                Interval = INITIAL_INTERVAL,
                Enabled = true,
                AutoReset = false
            };
            timer.SetSpeed(5, elapsedTime);
            elapsedTime += 10000L; // 2 seconds at speed x5
            elapsedTime += 5000L; // 5 seconds at speed x1
            timer.SetSpeed(1, elapsedTime);
            timer.SetSpeed(3, elapsedTime);
            timer.SetSpeed(2, elapsedTime);
            elapsedTime += 15000L; // 5 seconds at speed x3

            double result = Math.Abs((INITIAL_INTERVAL - elapsedTime) / 2 - timer.Interval); // divide by 2 because current speed would be x2
            result.Should().BeLessThan(1);

            timer.Dispose();
        }
    }
}
