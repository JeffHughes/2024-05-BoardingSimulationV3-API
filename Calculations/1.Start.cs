using BoardingSimulationV3.Classes;

namespace BoardingSimulationV3.Calculations
{
    internal partial class calculations
    {

        public Results runWithConfig(Config config)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }



        public List<AnimationFrame> frames = [];
        List<int> PassengerIDsToSeat = [];
        string Message = "";

        private decimal time = 0.1m;
        private int duration = 1;
        private void saveFrame()
        {
            Dictionary<int, string> FamilyLocationMovements = new();
            foreach (var node in allNodes)
            {
                if (node.FamilyID == 0) continue;

                // get prev frame, if family nodeID is different, add to FamilyLocationMovements
                if (HasMoved(node.FamilyID, node.ID))
                    FamilyLocationMovements.Add(node.FamilyID, node.ID);
            }

            if (FamilyLocationMovements.Count > 0)
            {
                FamilyLocationMovements = FamilyLocationMovements
                    .OrderBy(x => x.Value)
                    .ToDictionary(x => x.Key, x => x.Value);

                time += duration;
                frames.Add(new AnimationFrame
                {
                    FrameNumber = frames.Count + 1,
                    FamilyLocationMovements = FamilyLocationMovements,
                    Message = Message,
                    CurrentBoardingGroup = currentBoardingGroup,
                    DurationSeconds = duration,
                    PassengerIDsToSeat = PassengerIDsToSeat,
                    TimeSeconds = time,
                });
                duration = 1;
                PassengerIDsToSeat = [];
                Message = "";
            }
            else duration++;
        }



    }
}
