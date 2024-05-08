namespace BoardingSimulationV3.Classes
{
    internal class Family
    {
        public int FamilyID { get; set; }  

        public int OverheadBin { get; set; }

        public int LowestOverheadBinSlotInFamily;

        public int BoardingGroup { get; set; }  

        public int BoardingOrder { get; set; }

        public List<FamilyPassenger> FamilyMembers { get; set; } = new List<FamilyPassenger>();
 
    }
}
