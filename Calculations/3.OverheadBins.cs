using BoardingSimulationV3.Classes;

namespace BoardingSimulationV3.Calculations
{
    internal partial class calculations
    {
        private List<Family> AssignBins(List<Family> families, Config config)
        {
            families = DetermineWhoHasCarryOn(families);

            var totalLuggageSpots = config.ContiguousOverheadBins * 2 * config.CarryOnLuggageSlotsPerOverheadBin;

            var filledSlotsByBin = new int[totalLuggageSpots + 1]; // 1-based index, we're ignoring 0 for simplicity below
            var binAssignments = new Dictionary<int, int>();

            var familiesSortedBiggestToSmallest = families
                .OrderByDescending(f => f.FamilyMembers.Any(m => m.Age > 0 && m.Age < 6))
                .ThenByDescending(f => f.FamilyMembers.Count)
                .ToList();

            // log to console family size and if they have small children
            familiesSortedBiggestToSmallest.ForEach(f => Console.WriteLine($"Family {f.FamilyID} has {f.FamilyMembers.Count} members and {(f.FamilyMembers.Any(m => m.Age > 0 && m.Age < 6) ? "small children" : "no small children")}"));

            foreach (var family in familiesSortedBiggestToSmallest)
            {
                int attempts = 0;
                int currentBin = config.ContiguousOverheadBins;
                // if family has small children,  
                var familyHasSmallChildren = family.FamilyMembers.Any(m => m.Age > 0 && m.Age < 6);

                var familyMembersWithCarryOns = family.FamilyMembers.Where(m => m.HasCarryOn).ToList();

                var moveBackToFront = familyHasSmallChildren 
                || (familyMembersWithCarryOns.Count >= 1 && family.FamilyMembers.Count <= 2);
                if (moveBackToFront) currentBin = 1;

                while (true)
                {
                    if (family.OverheadBin == -1) break;

                    if (currentBin < 1) currentBin = config.ContiguousOverheadBins;
                    if (currentBin > config.ContiguousOverheadBins) currentBin = 1;

                    var availableSlotsInCurrentBin = config.CarryOnLuggageSlotsPerOverheadBin * 2 - filledSlotsByBin[currentBin];

                    if (availableSlotsInCurrentBin >= familyMembersWithCarryOns.Count)
                    {
                        family.OverheadBin = currentBin;
                        family.LowestOverheadBinSlotInFamily = filledSlotsByBin[currentBin] + 1;

                        binAssignments.Add(family.FamilyID, currentBin);
                        foreach (var member in familyMembersWithCarryOns)
                            member.OverheadBinSlot = ++filledSlotsByBin[currentBin];

                        break;
                    }

                    if (moveBackToFront) currentBin++; else currentBin--;
                    attempts++;
                    if (attempts >= config.ContiguousOverheadBins) // Prevent infinite loop  
                        break;
                    // TODO:  handle the case where no bins have space 
                }
            }

            // Experimental 
            // MoveAFewGroupsBackToReduceTotalBoardingGroupCount(config, familiesSortedBiggestToSmallest, filledSlotsByBin, binAssignments);

            return familiesSortedBiggestToSmallest;
        }

        private void MoveAFewGroupsBackToReduceTotalBoardingGroupCount(Config config,
            List<Family> familiesSortedBiggestToSmallest, int[] filledSlotsByBin, Dictionary<int, int> binAssignments)
        {
            // if there are a bunch of families of 1 in bin > 3 AND bin 1 and 2 have space, move them to bin 1 or 2
            var familiesOf1or2 = familiesSortedBiggestToSmallest.Where(f => f.FamilyMembers.Count <= 2).ToList();
            var familiesOf1or2InBin3OrHigher = familiesOf1or2.Where(f => f.OverheadBin > 3).ToList();

            familiesOf1or2InBin3OrHigher = familiesOf1or2InBin3OrHigher.OrderBy(x => x.OverheadBin).ToList();
            // start w the passengers in 3, then 4, then 5, etc.

            foreach (var family in familiesOf1or2InBin3OrHigher)
            {
                var familyMembersWithCarryOns = family.FamilyMembers.Where(m => m.HasCarryOn).ToList();

                for (var bin = 1; bin <= 2; bin++)
                {
                    var availableSlotsInBin = config.CarryOnLuggageSlotsPerOverheadBin * 2 - filledSlotsByBin[bin];
                    if (availableSlotsInBin >= familyMembersWithCarryOns.Count)
                    {
                        // filledSlotsByBin[family.OverheadBin] -= familyMembersWithCarryOns.Count;

                        family.OverheadBin = bin;
                        family.LowestOverheadBinSlotInFamily = filledSlotsByBin[bin] + 1;
                        binAssignments[family.FamilyID] = bin;
                        foreach (var member in familyMembersWithCarryOns)
                            member.OverheadBinSlot = ++filledSlotsByBin[bin];
                        break;
                    }
                }
            }

        }


        //&& 
        //filledSlotsByBin[1] + filledSlotsByBin[2] <
        //config.CarryOnLuggageSlotsPerOverheadBin* 2


        private List<Family> DetermineWhoHasCarryOn(List<Family> families)
        {
            int partiesOf1 = 3;
            int partiesOf2 = 2;
            int partiesOfMoreThan2 = 5;

            var total = partiesOf1 + partiesOf2 + partiesOfMoreThan2;

            if (families.Count < total)
                throw new Exception("Not enough families to assign overhead bins");

            if (total > 12)
                throw new Exception("too many families for the the first boarding group");

            var baglessFamilies = new List<Family>();
            // find the first 3 families with 1 member 
            var familiesWith1 = families.Where(f => f.FamilyMembers.Count == 1).Take(partiesOf1).ToList();
            baglessFamilies.AddRange(familiesWith1);

            // find the first 2 families with 2 members
            var familiesWith2 = families.Where(f => f.FamilyMembers.Count == 2).Take(partiesOf2).ToList();
            baglessFamilies.AddRange(familiesWith2);

            // find the first 6 families with more than 2 members
            var familiesWithMoreThan2 = families
                .Where(f => f.FamilyMembers.Count > 2)
                .Where(f => f.FamilyMembers.All(m => m.Age == 0))
                .OrderBy(f => Guid.NewGuid())
                .Take(partiesOfMoreThan2).ToList();
            baglessFamilies.AddRange(familiesWithMoreThan2);


            baglessFamilies.ForEach(family =>
            {
                family.BoardingGroup = 1;
                family.BoardingOrder = baglessFamilies.IndexOf(family) + 1;
                family.OverheadBin = -1;
                family.LowestOverheadBinSlotInFamily = -1;

                family.FamilyMembers.ForEach(member =>
                {
                    member.HasCarryOn = false;
                    member.OverheadBinSlot = -1;
                });
            });

            return families;
        }
    }
}
