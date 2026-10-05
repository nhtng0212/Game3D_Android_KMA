namespace BlackMarket {
    public static class EncounterData {
        // Exploration floors retain zero guards; combat encounters double their previous population.
        public static readonly int[] Counts={0,4,0,4,6,6,2};
        public static readonly string[] Guards={"Operator","MilitaryHelmet","MilitaryCap","RedBeret","Tactical","Plainclothes"};
        public static readonly string[] Actors={"Alex","Operator","MilitaryHelmet","MilitaryCap","RedBeret","Tactical","Plainclothes","Victor"};
        public static readonly string[] Models={"Male_Adult_07","Security_Male_01","Military_Male_01","Military_Male_05","Police_Male_04","Police_Male_06","Male_Adult_04","Police_Male_02"};
        public static string Guard(int stage,int index)=>Guards[(stage+index)%Guards.Length];
    }
}
