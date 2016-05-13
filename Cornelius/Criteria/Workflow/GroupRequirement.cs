namespace Cornelius.Criteria.Workflow
{
    /*
     * Egy csoportkövetelmény. Vonatkozhat kreditre és tárgyteljesítésre,
     * lényegében csak tárol egy csoportot és egy hozzá társított számot.
     */
    class GroupRequirement
    {
        public string Identifier;
        public int Amount;

        public GroupRequirement(string identifier, int amount)
        {
            this.Identifier = identifier;
            this.Amount = amount;
        }
    }
}
