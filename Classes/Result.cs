namespace BoardingSimulationV3.Classes
{
    internal class Results
    {
        public Config Config { get; set; } = new Config();
        public List<Family> Families { get; set; } = new List<Family>();
        //public List<FamilyPassenger> Passengers { get; set; } = new List<FamilyPassenger>();
        public List<AnimationFrame> AnimationFrames { get; set; } = new List<AnimationFrame>(); 
    }
}
