namespace BoardingSimulationV3.Classes
{
    internal class AnimationFrame
    {
        public int FrameNumber { get; set; } = 0;

        public int DurationSeconds { get; set; } = 1;

        public decimal TimeSeconds { get; set; } = 0.00001m;

        // public string Description { get; set; } = "";
        public string Message { get; set; } = "";

        public int CurrentBoardingGroup { get; set; } = 1;

        public Dictionary<int, string> FamilyLocationMovements { get; set; } = new();

        public List<int> PassengerIDsToSeat { get; set; } = [];

        public List<int> FamiliesAreSeated { get; set; } = [];
    }
}
