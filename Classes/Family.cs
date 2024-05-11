namespace BoardingSimulationV3.Classes
{
    internal class Family
    {
        public int FamilyID { get; set; }  

        public int OverheadBin { get; set; }

        public int LowestOverheadBinSlotInFamily;

        public int BoardingGroup { get; set; }  

        public int BoardingOrder { get; set; }

        public List<FamilyMemberPassenger> FamilyMembers { get; set; } = new List<FamilyMemberPassenger>();

        public List<int> LuggageHandlerIDs = new List<int>();
        public List<int> NonLuggageHandlerIDs = new List<int>();

        public bool SeatsFound = false;
        public bool IsSeated = false;
        public bool LuggageHandlersSeated = false;
        public bool NonLuggageHandlersSeated = false;

    }
}
