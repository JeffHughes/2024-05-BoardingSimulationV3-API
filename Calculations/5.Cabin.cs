using BoardingSimulationV3.Classes;
using System.Linq;
using System.Numerics;

namespace BoardingSimulationV3.Calculations
{
    internal partial class calculations
    {
        List<PathLimitingNode> allNodes = new List<PathLimitingNode>();

        int currentBoardingGroup = 1;

        private List<Family> MoveToCabin(List<Family> familiesWithBoardingGroups, Config config)
        {
            seats = new bool[config.PassengerRows * config.SeatsPerPassengerRow]; 

            setupNodes(config);
            moveFamiliesToTheirGateLane(familiesWithBoardingGroups, 1);
            duration = 0;
            saveFrame();
            // timer doesn't start until the first passenger moves to walkway
            moveFamiliesToTheirGateLane(familiesWithBoardingGroups, 2);
            duration = 0;
            saveFrame();
            duration = 1;

            var maxFramesToProcess = 60 * 30; // 30 minutes of boarding
            var boarding = true;
            while (boarding && maxFramesToProcess-- > 0)
            {
                callNextBoardingGroup(familiesWithBoardingGroups);

                allNodes.ForEach(moveFromNode =>
                {
                    if (moveFromNode.FamilyID != 0)
                    {
                        var family = familiesWithBoardingGroups.Find(x => x.FamilyID == moveFromNode.FamilyID);
                        if (family.BoardingGroup <= currentBoardingGroup)
                        {
                            var moveToNode = allNodes.Find(x => x.NodeID == moveFromNode.NextNodeID);

                            if (atOverheadBin(family, moveFromNode))
                                stowBagsAndSitDown(family, moveFromNode, config);
                            else
                                moveToNextNodeInPath(moveFromNode, moveToNode);
                        }
                    }
                });

                saveFrame();
            }

            return familiesWithBoardingGroups;
        }

        private void stowBagsAndSitDown(Family family, PathLimitingNode node, Config config)
        {
            //TODO: move this to family generation 
            var luggageHandlerIDs = getPassengerIDsforFamilyLuggageHandlers(family);
            var nonLuggageHandlerIDs = getFamilyMemberNonLuggageHandlers(family, luggageHandlerIDs);

            if (node.BottleNeckCountdown == 0)
            {
                // setup countdown for stowing bags
                // let everyone but luggage handlers sit down 
                PassengerIDsToSeat.AddRange(nonLuggageHandlerIDs);
                findSeatsFor(family, nonLuggageHandlerIDs, config);

                var carryOns = family.FamilyMembers.Count(x => x.HasCarryOn);
                if (carryOns > 0)
                {
                    var secsPerBagPerHandler =
                        (int)Math.Ceiling(decimal.Divide(config.secondsPerBagLoad, luggageHandlerIDs.Count));
                    node.BottleNeckCountdown = carryOns * secsPerBagPerHandler;
                }
                else
                {
                    node.FamilyID = 0;
                }
            }
            else
            {
                if (node.BottleNeckCountdown == 1)
                {
                    // let luggage handlers sit down
                    PassengerIDsToSeat.AddRange(luggageHandlerIDs);
                    node.FamilyID = 0;
                }

                node.BottleNeckCountdown--;
            }
        }

        List<int> getPassengerIDsforFamilyLuggageHandlers(Family family)
        {
            return family.FamilyMembers
                .Where(x => (x.Age == 0 || x.Age > 10) && x.HasCarryOn)
                .Take((int)Math.Ceiling(decimal.Divide(family.FamilyMembers.Count, 3)))
                .Select(x => x.PassengerID).ToList();
        }

        List<int> getFamilyMemberNonLuggageHandlers(Family family, List<int> luggageHandlerIDs)
        {
            return family.FamilyMembers
                .Where(x => !luggageHandlerIDs.Contains(x.PassengerID)).Select(x => x.PassengerID).ToList();
        }


        private void moveToNextNodeInPath(PathLimitingNode moveFromNode, PathLimitingNode moveToNode)
        {
            // TODO: if not blocked by passenger trying to sit down
            //if (backwardsMovingPassengerNodeBlocks.Contains(node.NodeID)) return;
            if (moveToNode?.FamilyID == 0)
            {
                moveToNode.FamilyID = moveFromNode.FamilyID;
                moveFromNode.FamilyID = 0;
            }
        }

        private bool atOverheadBin(Family family, PathLimitingNode node)
        {
            if (node.PathLocationType != PathLocationType.Cabin) return false;
            if (family.OverheadBin < 0) return true;
            var atOverheadBinBool = node.WalkwayBoardingOrderOrBinNumber == family.OverheadBin;
            return atOverheadBinBool;
        }

        private void callNextBoardingGroup(List<Family> familiesWithBoardingGroups)
        {
            var maxBoardingGroup = familiesWithBoardingGroups.Max(x => x.BoardingGroup);
            var currentBoardingGroupClearOfGateLane = laneIsEmpty(getLaneForBoardingGroup(currentBoardingGroup));
            if (currentBoardingGroupClearOfGateLane && currentBoardingGroup <= maxBoardingGroup)
            {
                currentBoardingGroup++;
                Console.WriteLine("Now boarding group " + currentBoardingGroup);

                if (currentBoardingGroup > 1 && currentBoardingGroup + 1 <= maxBoardingGroup) // 1 and 2 are already in their lanes
                    moveFamiliesToTheirGateLane(familiesWithBoardingGroups, currentBoardingGroup + 1);
            }
        }

        private bool laneIsEmpty(int lane)
        {
            var nodeIDsforLane = getNodeIDsforLane(lane);
            return allNodes.Count(x => nodeIDsforLane.Contains(x.NodeID) && x.FamilyID != 0) == 0;
        }

        public List<int> getNodeIDsforLane(int lane)
        {   //todo: memoize this?
            var laneNodes = allNodes
                .FindAll(x => x.PathLocationType == PathLocationType.GateLane && x.LaneNumber == lane);

            // print lane nodes for debugging
            // with moveFromNode ID, lane number, and familyID
            // laneNodes.ForEach(x => Console.WriteLine(x.NodeID + " " + x.LaneNumber + " " + x.FamilyID));

            var IDs = laneNodes.Select(x => x.NodeID).ToList();
            return IDs;
        }

        private void moveFamiliesToTheirGateLane(List<Family> familiesWithBoardingGroups, int boardingGroup)
        {
            var lane = getLaneForBoardingGroup(boardingGroup);

            Message = "Passengers in Boarding group " + (char)(boardingGroup + 65) + " please line up on your number in the " + (lane == 1 ? "left" : "right") + " lane ";

            var familiesInBoardingGroup = familiesWithBoardingGroups.FindAll(f => f.BoardingGroup == boardingGroup);
            foreach (var family in familiesInBoardingGroup)
            {
                var node = getNodeByGateLaneBoardingOrder(family.BoardingOrder, lane);
                node.FamilyID = family.FamilyID;
            }
        }

        char convertIntToString(int i)
        {
            // use ascii char 65 to 90
            return (char)(i + 65);
        }

        int getLaneForBoardingGroup(int boardingGroup = 1)
        {
            return boardingGroup % 2 == 0 ? 2 : 1;
        }

        PathLimitingNode getNodeByGateLaneBoardingOrder(int binNumber, int lane = 1)
        {
            return allNodes.Find(x => x.PathLocationType == PathLocationType.GateLane && x.WalkwayBoardingOrderOrBinNumber == binNumber && x.LaneNumber == lane)!;
        }




        private bool HasMoved(int familyID, string ID)
        {
            // get prev frame, if family nodeID is different, add to FamilyLocationMovements
            var hasMoved = true;
            if (frames.Count > 0)
            {
                var frameCounter = frames.Count;
                while (hasMoved && frameCounter > 0)
                {
                    var prevFrame = frames[frameCounter-- - 1];
                    if (prevFrame.FamilyLocationMovements.ContainsKey(familyID))
                        if (prevFrame.FamilyLocationMovements[familyID] == ID)
                            hasMoved = false; // TODO: we could also count UP from here ! 
                }
            }
            return hasMoved;
        }

        private void setupNodes(Config config)
        {
            var currentNodeID = 1;

            for (var lane = 1; lane <= 2; lane++)
                for (var locationNumber = 1; locationNumber <= config.ContiguousOverheadBins; locationNumber++)
                {
                    allNodes.Add(new PathLimitingNode
                    {
                        NodeID = currentNodeID++,

                        WalkwayBoardingOrderOrBinNumber = config.ContiguousOverheadBins + 1 - locationNumber,
                        PathLocationType = PathLocationType.GateLane,
                        ID = "GateLane-" + lane + "-" + (config.ContiguousOverheadBins + 1 - locationNumber),

                        GlobalOrderNumber = locationNumber,
                        LaneNumber = lane,

                        NextNodeID = locationNumber < config.ContiguousOverheadBins ? currentNodeID :
                            config.ContiguousOverheadBins * 2 + 1
                    });
                }

            var nextGlobalOrderNumber = allNodes.Max(x => x.GlobalOrderNumber);
            for (var locationNumber = 1; locationNumber <= config.WalkwayDistance; locationNumber++)
            {
                allNodes.Add(new PathLimitingNode
                {
                    NodeID = currentNodeID++,

                    WalkwayBoardingOrderOrBinNumber = locationNumber,
                    PathLocationType = PathLocationType.Walkway,
                    ID = "Walkway-" + locationNumber,

                    GlobalOrderNumber = nextGlobalOrderNumber + locationNumber,

                    NextNodeID = currentNodeID
                });
            }

            nextGlobalOrderNumber = allNodes.Max(x => x.GlobalOrderNumber);
            for (var locationNumber = 1; locationNumber <= config.ContiguousOverheadBins; locationNumber++)
            {
                allNodes.Add(new PathLimitingNode
                {
                    NodeID = currentNodeID++,

                    WalkwayBoardingOrderOrBinNumber = config.ContiguousOverheadBins + 1 - locationNumber,
                    PathLocationType = PathLocationType.Cabin,
                    ID = "Cabin-" + (config.ContiguousOverheadBins + 1 - locationNumber),

                    GlobalOrderNumber = nextGlobalOrderNumber + locationNumber,

                    NextNodeID = currentNodeID
                });
            }

            allNodes.Reverse();

            foreach (var node in allNodes)
                Console.WriteLine(node.NodeID + " " + node.ID + " next: " + node.NextNodeID);
        }
    }
}
