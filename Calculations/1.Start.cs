using BoardingSimulationV3.Classes;

namespace BoardingSimulationV3.Calculations
{
    internal partial class calculations
    {

        public Results runWithConfig(Config config)
        {
            var families = CreateFamilies(config);
            var familiesWithBins = AssignBins(families, config);
            var familiesWithBoardingGroups = AssignBoardingGroup(familiesWithBins);
            var familiesWithSeats = MoveToCabin(familiesWithBoardingGroups, config);

            return new Results()
            {
                Config = config,
                Families = familiesWithSeats, 
                AnimationFrames = frames,
            };
        }
    }
}
