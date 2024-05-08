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
            var binAssignments = new Dictionary<int, List<int>>();

            var familiesSortedBiggestToSmallest = families.OrderByDescending(f => f.FamilyMembers.Count).ToList();
             
            foreach (var family in familiesSortedBiggestToSmallest)
            {
                int attempts = 0;
                int currentBin = config.ContiguousOverheadBins;
                // if family has small children,  
                var familyHasSmallChildren = family.FamilyMembers.Any(m => m.Age > 0 && m.Age < 6);
                var familyMembersWithCarryOns = family.FamilyMembers.Where(m => m.HasCarryOn).ToList();

                while (true)
                {
                    if (family.OverheadBin == -1) break;

                    if (currentBin < 1) currentBin = config.ContiguousOverheadBins;
                    if (familyHasSmallChildren) currentBin = 1;

                    var availableSlotsInCurrentBin = config.CarryOnLuggageSlotsPerOverheadBin * 2 - filledSlotsByBin[currentBin];

                    if (availableSlotsInCurrentBin >= familyMembersWithCarryOns.Count)
                    {
                        family.OverheadBin = currentBin;
                        family.LowestOverheadBinSlotInFamily = filledSlotsByBin[currentBin] + 1;

                        binAssignments.Add(family.FamilyID, new List<int> { currentBin });
                        foreach (var member in familyMembersWithCarryOns)
                            member.OverheadBinSlot = ++filledSlotsByBin[currentBin];

                        break;
                    }

                    if (familyHasSmallChildren) currentBin++; else currentBin--;
                    attempts++;
                    if (attempts >= config.ContiguousOverheadBins) // Prevent infinite loop  
                        break;
                    // TODO:  handle the case where no bins have space 
                }
            } 

            return familiesSortedBiggestToSmallest;
        }
             
            
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
