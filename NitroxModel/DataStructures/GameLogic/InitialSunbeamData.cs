using System;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    public class InitialSunbeamData
    {
        public bool CountdownActive { get; set; }
        public double CountdownStartingTimeMs { get; set; }

        protected InitialSunbeamData()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public InitialSunbeamData(bool countdownActive, double countdownStartingTimeMs)
        {
            CountdownActive = countdownActive;
            CountdownStartingTimeMs = countdownStartingTimeMs;
        }
    }
}
