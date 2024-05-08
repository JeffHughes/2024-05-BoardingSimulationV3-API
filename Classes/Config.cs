namespace BoardingSimulationV3.Classes
{
    internal class Config
    {
        public int PassengerRows { get; set; } = 24;

        public int SeatsPerPassengerRow { get; set; } = 6;

        public int TotalPassengers{ get; set; } = 143;

        public int ContiguousOverheadBins { get; set; } = 12;

        public int CarryOnLuggageSlotsPerOverheadBin { get; set; } = 6;
         
        public int WalkwayDistance { get; set; } = 15;

        public int secondsPerBagLoad { get; set; } = 4;

    }
}
