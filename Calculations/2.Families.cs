using BoardingSimulationV3.Classes;

namespace BoardingSimulationV3.Calculations
{
    internal partial class calculations
    {
        public List<Family> CreateFamilies(Config config)
        {
            var familySizeDistributions = GenerateFamilySizeHistogramDistributions(config.TotalPassengers, 6);

            //double mean = 30.0;  // Mean of the distribution
            //double stdDev = 10.0; // Standard deviation of the distribution
            //int minValue = 18;    // Minimum value of age
            //int maxValue = 80;  // Maximum value of age
            // int[] ages = NormalDistribution.GenerateNormalDistributionArray(config.TotalPassengers, mean, stdDev, minValue, maxValue);
            // TODO: use the ages of passengers to approximate speed of boarding

            var families = new List<Family>();
            var familyIDs = 1;
            var passengerIDs = 1;
            foreach (var distribution in familySizeDistributions)
            {
                for (var i = 0; i < distribution.FrequencyOfThisMaxPassengerCount; i++)
                {
                    var passengers = new List<FamilyPassenger>();
                    for (var j = 0; j < distribution.PassengerCount; j++)
                    {
                        var passenger = new FamilyPassenger
                        {
                            // Age = ages[passengerIDs - 1],
                            PassengerID = passengerIDs++,
                        };
                        passengers.Add(passenger);
                    }

                    var family = new Family
                    {
                        FamilyID = familyIDs++,
                        FamilyMembers = passengers,
                    };
                    families.Add(family);
                }
            }

            addSmallChildrenToMix(families);
            // add elderly, disabled, and other special needs passengers
            // add kids travelling alone (unaccompanied minors)

            return families;
        }

        private static void addSmallChildrenToMix(List<Family> families)
        {
            // find the first family of 4 and set the ages of the children to 1 and 4
            var familyOfFour = families.FirstOrDefault(f => f.FamilyMembers.Count == 4);
            if (familyOfFour != null)
            {
                familyOfFour.FamilyMembers[0].Age = 24;
                familyOfFour.FamilyMembers[1].Age = 27;
                familyOfFour.FamilyMembers[2].Age = 1;
                familyOfFour.FamilyMembers[3].Age = 4;

            }

            // find the first family of 3 and set the ages of the child to 2
            var familyOfThree = families.FirstOrDefault(f => f.FamilyMembers.Count == 3);
            if (familyOfThree != null)
            {
                familyOfFour.FamilyMembers[0].Age = 21;
                familyOfFour.FamilyMembers[1].Age = 20;
                familyOfThree.FamilyMembers[2].Age = 2;
            }
        }


        private static List<FamilyDistribution> GenerateFamilySizeHistogramDistributions(int totalPassengers, int maxFamilySize = 6)
        {
            var distribution = new List<FamilyDistribution>();
            var remainingPassengers = totalPassengers;
            const int groupMaxPassengerCountFallOffRatio = 3; // Ratio to reduce the number of groups as the FamilySize increases

            // Start with FamilySize 2 and work upwards
            for (var MaxPassengerCount = 2; MaxPassengerCount <= maxFamilySize && remainingPassengers >= MaxPassengerCount; MaxPassengerCount++)
            {
                var count = remainingPassengers / (MaxPassengerCount * groupMaxPassengerCountFallOffRatio);

                // Ensure there's at least one group of the current FamilySize
                if (MaxPassengerCount != 2 && count == 0 && remainingPassengers >= MaxPassengerCount) count = 1;

                distribution.Add(new FamilyDistribution(MaxPassengerCount, count));
                remainingPassengers -= MaxPassengerCount * count;
            }

            Console.WriteLine($"singles: {remainingPassengers}");

            // After allocating groups of FamilySize 2-6, fill in the remaining passengers with groups of FamilySize 1
            if (remainingPassengers > 0)
                distribution.Insert(0, new FamilyDistribution(1, remainingPassengers)); // Insert at the beginning

            return distribution;
        }


    }

    public class FamilyDistribution
    {
        public int PassengerCount { get; set; }
        public int FrequencyOfThisMaxPassengerCount { get; set; }

        public FamilyDistribution(int passengerCount, int frequencyOfThisMaxPassengerCount)
        {
            PassengerCount = passengerCount;
            FrequencyOfThisMaxPassengerCount = frequencyOfThisMaxPassengerCount;
        }
    }
}
