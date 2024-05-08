using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardingSimulationV3.Classes
{
    internal class PathLimitingNode
    {
        public int NodeID { get; set; } = 0;
        public int GlobalOrderNumber { get; set; } = 0;
        public int LaneNumber   = 1;
        public int FamilyID   = 0; 
        public int BottleNeckCountdown  = 0; 
        public int BottleNeckedByNodeID   = 0;

        public int NextNodeID = 0 ;
        public PathLocationType PathLocationType { get; set; } = PathLocationType.GateLane; 
        public int WalkwayBoardingOrderOrBinNumber { get; set; } = 0; // can be reversed from global order number
        public string ID { get; set; } = "";
    }

    public enum PathLocationType
    {
        GateLane = 1,
        Walkway = 2,
        Cabin = 3,
    }
}
