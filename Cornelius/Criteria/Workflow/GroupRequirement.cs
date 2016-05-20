namespace Cornelius.Criteria.Workflow
{
    /// <summary>
    /// Egy csoportkövetelmény. Vonatkozhat kreditre és tárgyteljesítésre,
    /// lényegében csak tárol egy csoportot és egy hozzá társított számot.
    /// </summary>
    class GroupRequirement
    {
        /// <summary>
        /// Csoportkövetelmény azonosítója.
        /// </summary>
        public string Identifier;

        // TODO: ezt konkrétabban leírni.
        /// <summary>
        /// Mennyiség.
        /// </summary>
        public int Amount;

        /// <summary>
        /// Csoportkövetelményt hoz létre.
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="amount"></param>
        public GroupRequirement(string identifier, int amount)
        {
            this.Identifier = identifier;
            this.Amount = amount;
        }
    }
}
