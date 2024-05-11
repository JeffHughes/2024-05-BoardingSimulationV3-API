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
        List<int> FamiliesAreSeated = [];
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

            if (FamilyLocationMovements.Count > 0 || PassengerIDsToSeat.Count > 0 || FamiliesAreSeated.Count > 0)
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
                    FamiliesAreSeated = FamiliesAreSeated,
                    TimeSeconds = time,
                });
                duration = 1;
                PassengerIDsToSeat = [];
                FamiliesAreSeated = [];
                Message = "";
            }
            else duration++;
        }


        private void saveFinalFrame(List<Family> allFamilies)
        {
            // find all unseated passengers
            var familyIDOnNodes = allNodes
                .Where(x => x.FamilyID != 0)
                .Select(x => x.FamilyID)
                .ToList();

            familyIDOnNodes.ForEach(familyID =>
              {
                  var family = allFamilies.FirstOrDefault(x => x.FamilyID == familyID);
                  if (!family.NonLuggageHandlersSeated)
                      seatLuggageHandlers(family);

                  if (!family.NonLuggageHandlersSeated)
                      seatNonLuggageHandlers(family);
              });

            var allFamiliesWUnseated = allFamilies.Where(f => !f.NonLuggageHandlersSeated || !f.LuggageHandlersSeated);
            foreach (var family in allFamiliesWUnseated)
            {
                if (!family.NonLuggageHandlersSeated)
                    seatLuggageHandlers(family);

                if (!family.NonLuggageHandlersSeated)
                    seatNonLuggageHandlers(family);
            }

            if (PassengerIDsToSeat.Count > 0) saveFrame();

        }
    }
}
