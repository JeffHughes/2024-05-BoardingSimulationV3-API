namespace BoardingSimulationV3.Classes
{
    internal class Results
    {
        public Config Config { get; set; } = new Config();
        public List<Family> Families { get; set; } = new List<Family>();
        //public List<FamilyMemberPassenger> Passengers { get; set; } = new List<FamilyMemberPassenger>();
        public List<AnimationFrame> AnimationFrames { get; set; } = new List<AnimationFrame>(); 
    }
}
