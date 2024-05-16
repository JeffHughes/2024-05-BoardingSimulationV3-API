using BoardingSimulationV3.Classes;

namespace BoardingSimulationV3.Calculations
{
    internal partial class calculations
    {
        List<Family> AssignBoardingGroup(List<Family> families)
        {
            var boardingGroupHasFamilyOnBoardingOrder = new Dictionary<int, int>();

            foreach (var family in families.Where(p => p.BoardingGroup != 0)) //if we set you to -1 because no luggage, move to BG 1 in order
            {
                boardingGroupHasFamilyOnBoardingOrder.TryAdd(family.BoardingOrder, 0);
                family.BoardingGroup = ++boardingGroupHasFamilyOnBoardingOrder[family.BoardingOrder];
            }

            foreach (var family in families
                         .Where(p => p.BoardingGroup == 0)
                         .OrderBy(f => f.OverheadBin)
                         .ThenBy(f => f.LowestOverheadBinSlotInFamily)
                    )
            {
                boardingGroupHasFamilyOnBoardingOrder.TryAdd(family.OverheadBin, 0);
                family.BoardingGroup = ++boardingGroupHasFamilyOnBoardingOrder[family.OverheadBin];
                family.BoardingOrder = family.OverheadBin;
            }

            // TODO: this method is 2am janky  
            // families = CompressEmptyBoardingGroups(families);

            var orderedFamilies = families
                    .OrderBy(f => f.BoardingGroup)
                    .ThenBy(f => f.BoardingOrder)
                    .ToList();

            // console.write line family id, boarding group, boarding order
            // orderedFamilies.ForEach(f => Console.WriteLine($"Family {f.FamilyID} has boarding group {f.BoardingGroup} and boarding order {f.BoardingOrder}"));

            return orderedFamilies;
        }

        private static List<Family> CompressEmptyBoardingGroups(List<Family> families)
        {
            // starting with the highest boarding group,
            // if the number of passengers in the whole boarding group is less than 6,
            // then we can move the whole boarding group to the prev boarding group
            // make new temp families

            // Sorting families from the biggest to the smallest group and getting distinct, sorted boarding groups
            var boardingGroups = families
                .Where(f => f.BoardingGroup > 6)
                .OrderByDescending(f => f.BoardingGroup)
                .Select(f => f.BoardingGroup)
                .Distinct()
                .ToList();

            // Iterating over each boarding group
            foreach (var boardingGroup in boardingGroups)
            {
                // Filtering families in the current boarding group
                var familiesInGroup = families.Where(f => f.BoardingGroup == boardingGroup).ToList();

                // If the number of families in the group is less than 6, decrease their boarding group by 1
                if (familiesInGroup.Count < 6)
                {
                    familiesInGroup.ForEach(f => f.BoardingGroup--);
                }
            }

            //// make sure there are no gaps in boarding groups
            //var boardingGroupsAfterCompression = families
            //    .OrderByDescending(f => f.BoardingGroup)
            //    .Select(f => f.BoardingGroup)
            //    .Distinct()
            //    .ToList();

            //for (int i = 0; i < boardingGroupsAfterCompression.Count; i++)
            //{
            //    if (boardingGroupsAfterCompression[i] != i + 1)
            //    {
            //        families
            //            .Where(f => f.BoardingGroup == boardingGroupsAfterCompression[i])
            //            .ToList()
            //            .ForEach(f => f.BoardingGroup = i + 1);
            //    }
            //}

            return families;
        }
    }
}
