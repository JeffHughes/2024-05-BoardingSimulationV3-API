using BoardingSimulationV3.Classes;
using System.Collections.Generic;
using System.Numerics;

namespace BoardingSimulationV3.Calculations
{
    internal partial class calculations
    {
        bool[] seats;
        List<int> backwardsMovingPassengerNodeBlocks = new();


        private void findSeatsFor(Family family, Config config)
        {
            var targetRow = getTargetRowFromOverheadBin(family.OverheadBin, config);
            var minRow = targetRow <= 0 ? 0 : getMinimumUnblockedRow(family.OverheadBin);

            var familySeats = findSeatsForFamilyWilmaOrdered(family, config, targetRow, minRow);

            // sort FamilyMembers by those in immediateSeatingPassengerIDs first, so they get the outside seatsPerRow
            family.FamilyMembers = family.FamilyMembers.OrderBy(m => family.NonLuggageHandlerIDs.Contains(m.PassengerID) ? 0 : 1).ToList();

            // console  write out the IDs of the luggage handlers
            Console.WriteLine("All Family Members: " + string.Join(", ", family.FamilyMembers.Select(m => m.PassengerID)));

            for (int i = 0; i < familySeats.Count; i++)
            {
                seats[familySeats[i].seatInt] = true;

                family.FamilyMembers[i].Row = familySeats[i].rowInt;
                family.FamilyMembers[i].SeatLetter = familySeats[i].seatLetter.ToString();
            }

            family.SeatsFound = true;
        }

        private List<(int seatInt, int rowInt, char seatLetter)> findSeatsForFamilyWilmaOrdered(Family family, Config config, int targetRow, int minRow)
        {
            Console.Write($"Family {family.FamilyID} has {family.FamilyMembers.Count} member(s)" +
                              (family.FamilyMembers.Any(m => m.Age is > 0 and < 6) ? " and has small children" : ""));
            Console.Write(", bg:" + family.BoardingGroup + "." + family.BoardingOrder + " bin:" + family.OverheadBin + ", row:" + targetRow);

            var foundSeats = new List<(int seatInt, int rowInt, char seatLetter)>();
            var seatPrefs = GetSeatPreferences(family.FamilyMembers.Count, config.PassengerRows);

            // log all seat preferences to console, break at config.PassengerRows
            //foreach (var seatSet in seatPrefs.Take(20))
            //{
            //    foreach (var seat in seatSet)
            //    {
            //        Console.Write(seat + " ");
            //    }
            //    Console.WriteLine();
            //}

            Func<int, int> seatAdjustmentForTargetRow = seat => seat + (targetRow * config.SeatsPerPassengerRow);

            //Console.Write(" minRow: " + minRow);
            //if (minRow > config.PassengerRows - 6)
            //{
            //    minRow = config.PassengerRows - 6;
            //    Console.Write(" new minRow: " + minRow);
            //}

            var min = seatAdjustmentForTargetRow(minRow);
            var max = config.PassengerRows * config.SeatsPerPassengerRow;

            Console.Write(" min: " + min + ", max: " + max);

            foreach (var seatSet in seatPrefs)
            {
                var allSeatsAvailable = true;
                foreach (var seat in seatSet.Select(seatAdjustmentForTargetRow))
                {
                    if (checkSeat(seat < min, seat, " less than min " + min) ||
                        checkSeat(seat >= max, seat, " greater than max " + max) ||
                        checkSeat(seats[seat], seat, " occupied"))
                    {
                        allSeatsAvailable = false;
                        break;
                    }
                }

                if (!allSeatsAvailable) continue;

                var seatsWithLetters = new List<(int seatInt, int rowInt, char seatLetter)>();

                Console.Write($", found : ");
                foreach (var seat in seatSet.Select(seatAdjustmentForTargetRow))
                {
                    var seatAndLetter = convertSeatIntToRowAndSeatLetter(seat, config.SeatsPerPassengerRow);
                    Console.Write(seatAndLetter.row.ToString() + "" + seatAndLetter.seatLetter + ", ");
                    seatsWithLetters.Add((seat, seatAndLetter.row, seatAndLetter.seatLetter));
                }

                if (targetRow > seatsWithLetters.First().rowInt)
                    Console.Write(" BACKTRACK! " + (seatsWithLetters.First().rowInt - targetRow) + ", ");

                // Sort the array using the custom indices // update if seatsPerRow changes to 8
                var wilma = new Dictionary<char, int> { { 'A', 0 }, { 'F', 1 }, { 'B', 2 }, { 'E', 3 }, { 'C', 4 }, { 'D', 5 } };
                var seatsSortedWilma = seatsWithLetters.OrderBy(l => wilma[l.seatLetter]);

                Console.WriteLine("");
                return seatsSortedWilma.ToList();
            }

            //if (foundSeats.Count == 0) //TODO: we could walk back more slowly than jumping to 0, minRow -2
            //    return findSeatsForFamilyWilmaOrdered(family, config, targetRow - 2, minRow - 4);

            // if that still doesn't work, try to find seatsPerRow for smaller groups;
            foreach (var familyMember in family.FamilyMembers)
            {
                if (familyMember.Age > 0 && familyMember.Age < 6)
                {
                    // todo: we need to find a seat for this small child w a parent 
                    // very difficult to get to this state, but it's possible
                }

                Console.Write(", family separated!: ");
                var smallFamily = new Family { FamilyMembers = [familyMember], FamilyID = family.FamilyID };
                var smallFamilySeats = findSeatsForFamilyWilmaOrdered(smallFamily, config, 0, 0);

                seats[smallFamilySeats.First().seatInt] = true;
                foundSeats.AddRange(smallFamilySeats);
            }

            return foundSeats;

            bool checkSeat(bool test, int seatInt, string message)
            {
                if (test)
                {
                    //var seatAndLetter = convertSeatIntToRowAndSeatLetter(seatInt, config.PassengerRows, config.SeatsPerPassengerRow);
                    //Console.Write("seat " + seatAndLetter.row + seatAndLetter.seatLetter + " " + seatInt + " ");
                    //Console.WriteLine(message); 
                    return true;
                }

                return false;
            }
        }

        //TODO: Memoize this
        private List<List<int>> GetSeatPreferences(int familyMembersCount, int passengerRowCount)
        {
            var seatsForFamilyCount = PreferredSeats(familyMembersCount);

            var preferredSeats = seatsForFamilyCount[0];
            var secondarySeats = seatsForFamilyCount[1];

            var orderOfPref1 = new List<RowOrder>
            {
                new RowOrder { StartRow = 1, EndRow = 1, Order = 1, IsActive = true },
                new RowOrder { StartRow = 2, EndRow = 3, Order = 3, IsActive = true },
                new RowOrder { StartRow = 4, EndRow = 6, Order = 5, IsActive = true }, 
                // new RowOrder { StartRow = 7, EndRow = 8, Order = 11, IsActive = false },
                // new RowOrder { StartRow = 9, EndRow = 10, Order = 15, IsActive = false },

                new RowOrder { StartRow = -1, EndRow = -1, Order = 2, IsActive = true },
                new RowOrder { StartRow = -2, EndRow = -2, Order = 4, IsActive = true },
                new RowOrder { StartRow = -3, EndRow = -4, Order = 9, IsActive = true },
                new RowOrder { StartRow = -5, EndRow = -6, Order = 11, IsActive = true }, 
                // new RowOrder { StartRow = -7, EndRow = -8, Order = 12, IsActive = false },
                // new RowOrder { StartRow = -9, EndRow = -10, Order = 16, IsActive = false },
            };

            var orderOfPref2 = new List<RowOrder>
            {
                new RowOrder { StartRow = 1, EndRow = 3, Order = 6, IsActive = true },
                new RowOrder { StartRow = 4, EndRow = 6, Order = 8, IsActive = true },  
                // new RowOrder { StartRow = 7, EndRow = 8, Order = 13, IsActive = false },
                // new RowOrder { StartRow = 9, EndRow = 10, Order = 18, IsActive = false },

                new RowOrder { StartRow = -1, EndRow = -2, Order = 7, IsActive = true },
                new RowOrder { StartRow = -3, EndRow = -4, Order = 14, IsActive = true },
                new RowOrder { StartRow = -5, EndRow = -6, Order = 16, IsActive = true },  
                // new RowOrder { StartRow = -7, EndRow = -8, Order = 17, IsActive = false },
                // new RowOrder { StartRow = -9, EndRow = -10, Order = 19, IsActive = false },
            };

            orderOfPref1.ForEach(x => x.Seats = preferredSeats);
            orderOfPref2.ForEach(x => x.Seats = secondarySeats);

            var orderOfPref = orderOfPref1.Concat(orderOfPref2).ToList();
            orderOfPref.OrderBy(x => x.Order);

            var result = new List<List<int>>();

            var highestRow = 0;
            var lowestRow = 0;

            var counter = 1;

            while (highestRow <= passengerRowCount || lowestRow >= -passengerRowCount)
            {
                foreach (var order in orderOfPref)
                {
                    var startRow = order.StartRow * counter;
                    var endRow = order.EndRow * counter;

                    if (startRow > 0)
                        AddPrefSeatsInHigherNumberedRows(result, order.Seats, startRow, endRow);
                    else
                        AddPrefSeatsInLowerNumberedRows(result, order.Seats, startRow, endRow);

                    if (endRow * counter > highestRow) highestRow = endRow;
                    if (startRow * counter < lowestRow) lowestRow = startRow;
                }
                counter++;
            }

            return result;
        }


        public int getMinimumUnblockedRow(int overheadBin)
        {
            var minimumUnblockedRow = allNodes
                .FindAll(x => x.PathLocationType == PathLocationType.Cabin
                              && x.WalkwayBoardingOrderOrBinNumber > overheadBin)
                .OrderBy(x => x.WalkwayBoardingOrderOrBinNumber);

            var lastUnblockedRow = overheadBin;
            foreach (var pathLimitingNode in minimumUnblockedRow)
            {
                if (pathLimitingNode.FamilyID == 0 && pathLimitingNode.WalkwayBoardingOrderOrBinNumber < lastUnblockedRow)
                    lastUnblockedRow = pathLimitingNode.WalkwayBoardingOrderOrBinNumber;
                else
                    return lastUnblockedRow;
            }

            return lastUnblockedRow;
        }

        private int getTargetRowFromOverheadBin(int familyOverheadBin, Config config)
        {
            if (familyOverheadBin < 1) return 0;

            /*
            1 = 23
            2 = 21
            3 = 19
            4 = 17
            5 = 15
            6 = 13
            7 = 11
            8 = 9
            9 = 7
            10 = 5
            11 = 3
            12 = 1
            */

            var targetRow = OverheadBinToPassengerRow(familyOverheadBin, config);
            return targetRow;
        }

        private static int OverheadBinToPassengerRow(int familyOverheadBin, Config config)
        {
            return (config.PassengerRows + 1) - (familyOverheadBin * 2);
        }


        (int row, char seatLetter) convertSeatIntToRowAndSeatLetter(int foundSeat, int seatsPerRow)
        {
            var row = (int)Math.Floor(decimal.Divide(foundSeat, seatsPerRow)) + 1;
            var seatLetter = ((char)(foundSeat % seatsPerRow + 65));

            return (row, seatLetter);
        }

        private void AddPrefSeatsInHigherNumberedRows(List<List<int>> result, List<List<int>> seats, int startRow, int endRow)
        {
            for (var i = startRow - 1; i < endRow; i++)
                seats.ForEach(row => result.Add(row.Select(x => x + i * 6).ToList()));

        }

        private void AddPrefSeatsInLowerNumberedRows(List<List<int>> result, List<List<int>> seats, int startRow, int endRow)
        {
            for (var i = startRow; i > endRow - 1; i--)
                seats.ForEach(row => result.Add(row.Select(x => x + i * 6).ToList()));
        }


        private List<List<List<int>>> PreferredSeats(int groupSize)
        {
            switch (groupSize)
            {
                case 2:
                    return PreferredSeatsPairs();
                case 3:
                    return PreferredSeats3();
                case 4:
                    return PreferredSeats4();
                case 5:
                    return PreferredSeats5();
                case 6:
                    return PreferredSeats6();
                default:
                    return PreferredSeatsSingles();
            }
        }

        Random random = new Random();
        private List<List<List<int>>> PreferredSeatsSingles()
        {
            // Singles Preferred seating window priority port:
            var AFCD = LettersToSeats("A,F,C,D");

            // Singles Preferred seating window priority starboard:
            var FADC = LettersToSeats("F,A,D,C");

            // Singles Preferred seating isle then window port:
            var CDAF = LettersToSeats("C,D,A,F");

            // Singles Preferred seating isle then window starboard:
            var DCFA = LettersToSeats("D,C,F,A");

            // Singles Secondary Preference seating window priority starboard:
            var BE = LettersToSeats("B,E");

            // Singles Secondary Preference seating isle then window port:
            var EB = LettersToSeats("E,B");

            //var wilma = random.NextDouble();
            //var portStarbord = random.NextDouble();
            //if (wilma < 0.8)
            //{
            //    if (portStarbord < 0.3)
            return [AFCD, EB];  // Port

            //    return [FADC, BE];  // Starboard
            //}
            //else
            //{
            //    if (portStarbord < 0.7)
            //        return [CDAF, BE];  // Port 

            //    return [DCFA, BE];  // Starboard
            //}


        }

        private List<List<List<int>>> PreferredSeatsPairs()
        {
            var ABEF = LettersToSeats("AB,EF");
            var BCDE = LettersToSeats("BC,DE");
     
            var FEBA = LettersToSeats("FE,BA");
            var DECB = LettersToSeats("DE,CB");
            //TODO: come up w secondary seatsPerRow

            // 60% of the time, return port
            // 40% of the time, return aft

            //if (random.NextDouble() < 0.6)
            //{
            return [ABEF, BCDE];  // Port
            //}
            //else
            //{
            //    return [BCCDEEFA, ABBCDEEF];  // Starboard
            //}
        }

        private List<List<List<int>>> PreferredSeats3()
        {
            var ABCDEF = LettersToSeats("ABC,DEF");
            var DEFABC = LettersToSeats("DEF,ABC");

            var crossIsle = LettersToSeats("BCD,CDE");
            var crossIsleReversed = LettersToSeats("CDE,BCD");

            //TODO: come up w secondary seatsPerRow, e.g. 2x1s or 1x2s

            // 60% of the time, return port
            // 40% of the time, return aft

            if (random.NextDouble() < 0.6)
            {
                return [ABCDEF, crossIsle];  // Port
            }
            else
            {
                return [DEFABC, crossIsleReversed];  // Starboard
            }
        }

        private List<List<List<int>>> PreferredSeats4()
        {
            var ABCDBCDECDEF = LettersToSeats("ABCD,BCDE,CDEF");
            var BCDECDCEDEF = LettersToSeats("BCDE,CDCE,CDEF");

            var ABGHBCHICDJKDEKL = LettersToSeats("ABGH,BCHI,DEJK,EFKL");
            var BCGHCDHIDEKJDEKL = LettersToSeats("BCGH,CDHI,DEKJ,DEKL");

            // 60% of the time, return port
            // 40% of the time, return aft

            if (random.NextDouble() < 0.6)
            {
                return [ABCDBCDECDEF, ABGHBCHICDJKDEKL];  // Port
            }
            else
            {
                return [BCDECDCEDEF, BCGHCDHIDEKJDEKL];  // Starboard
            }
        }

        private List<List<List<int>>> PreferredSeats5()
        {
            var ABCDE_BCDEF = LettersToSeats("ABCDE,BCDEF");
            var BCDEF_ABCDE = LettersToSeats("BCDEF,ABCDE");

            var AllOtherCombos = LettersToSeats("ABCGH,ABCHI,DEFJK,DEFKL");
            var AllOtherCombosReversed = LettersToSeats("BCDGH,BCDHI,JKDEF,LKDEF");

            // 60% of the time, return port
            // 40% of the time, return aft

            if (random.NextDouble() < 0.6)
            {
                return [ABCDE_BCDEF, AllOtherCombos];  // Port
            }
            else
            {
                return [BCDEF_ABCDE, AllOtherCombosReversed];  // Starboard
            }
        }

        private List<List<List<int>>> PreferredSeats6()
        {
            var ABCDE_BCDEF = LettersToSeats("ABCDEF,GHIJKL");
            var BCDEF_ABCDE = LettersToSeats("GHIJKL,ABCDEF");

            var AllOtherCombos = LettersToSeats("ABCGHI,DEFJKL");
            var AllOtherCombosReversed = LettersToSeats("IHGCBA,LKJFED");

            // 60% of the time, return port
            // 40% of the time, return aft

            if (random.NextDouble() < 0.6)
            {
                return [ABCDE_BCDEF, AllOtherCombos];  // Port
            }
            else
            {
                return [BCDEF_ABCDE, AllOtherCombosReversed];  // Starboard
            }
        }


        private Dictionary<string, List<List<int>>> lettersToSeatsDictionary = new();
        public List<List<int>> LettersToSeats(string letters)
        {
            if (lettersToSeatsDictionary.ContainsKey(letters))
                return lettersToSeatsDictionary[letters];

            // Split the input string into groups separated by commas
            var result = letters.Split(',')
                // For each group, trim whitespace and convert each character to its numeric form, then collect into a list
                .Select(group => group.Trim().Select(letter => (int)letter - 65).ToList())
                // Collect the results into a list of lists of integers
                .ToList();

            //// Print each list in the result list for verification
            //Console.WriteLine($"{letters} = ");
            //foreach (var list in result) 
            //    Console.WriteLine($"[{string.Join(", ", list)}]"); 

            lettersToSeatsDictionary[letters] = result;
            return result;
        }


    }


    public class RowOrder
    {
        public int StartRow { get; set; }
        public int EndRow { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; } = true; // Indicates if the row order is active
        public List<List<int>> Seats { get; set; }
    }
}
