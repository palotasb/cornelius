using System.Collections.Generic;
using System.Linq;

namespace Cornelius.Criteria.Expression
{
    /*
     * Kritériumcsoport, ami akkor igaz, ha a tagkritériumaira teljesül a 
     * megfelelő feltétel.
     */
    class MatchGroup : IExpression
    {
        /*
         * Kritérium neve.
         */
        public string Name
            { get; set; }

        /*
         * Rendezéshez használt félév száma.
         */
        public int? Semester 
            { get; set; }

        /*
         * Elméleti kreditérték, ami hiányzik, ha nem teljesül a tárgy.
         */
        public double Credit
            { get; set; }

        /*
         * Az alfeltételekből ennyinek kell teljesülnie. Speciális esetek:
         *  - pozitív n: ennyi feltételnek kell teljesülnie
         *  - nulla: ekkor minden feltételnek teljesülnie kell
         *  - negatív n: ennyi feltétel nem teljesülése engedhető meg legfeljebb
         */
        public int Requirement
            { get; set; }

        /*
         * Súly továbbadása. Ha hamis, akkor a kritérium egy feltétellé
         * "nyomódik össze" az anyakritérium szemében, ami vagy igaz vagy nem.
         */
        public bool PassWeight
            { get; set; }

        /*
         * Minden alfeltétel kiértékelése. Ha igaz, azután is folytatódik a kiértékelődés,
         * ha már van elegendő feltétel az igazzá váláshoz.
         */
        public bool EvaluateAll
            { get; set; }

        /*
         * A feltétel azokra vonatkozik csak, akik a megadott képzésben
         * kezdték mag tanulmányaikat.
         */
        public string Origin
            { get; set; }

        /*
         * Alfeltételek listája.
         */
        public List<IExpression> Children
            { get; protected set; }

        /*
         * A kritérium súlya, vagyis a ténylegesen teljesítendő kritériumok száma
         */
        public int Weight
        {
            get
            {
                if (!this.PassWeight)
                {
                    return 1;
                }
                if (this.Requirement <= 0)
                {
                    return this.Children.Sum(child => child.Weight) + this.Requirement;
                }
                else
                {
                    return this.Requirement;
                }
            }
        }

        /*
         * Kiértékelés
         */
        public Result Evaluate(StudentCourseProxy proxy)
        {
            Result result = new Result(this.Name);
            result.Semester = this.Semester > 0 ? this.Semester : null;

            if (this.Origin != null && proxy.Origin != this.Origin)
            {
                // Ilyenkor nincs meg a kritérium, mégis úgy számít, mintha meglenne
                result.Weight = this.Weight;
                result.Value = false;
                return result;
            }

            // Szükséges igaz feltételek száma
            int required = this.Requirement > 0 ? this.Requirement : this.Children.Sum(child => child.Weight) + this.Requirement;

            foreach (IExpression child in this.Children.OrderByDescending(c => c.Order(proxy)))
            {
                if (required <= 0 && !this.EvaluateAll) break;
                Result subresult = child.Evaluate(proxy);
                required -= subresult.Weight;

                if (subresult || child is MatchGroup)
                {
                    // Az igaz feltételeket és a feltételcsoportokat őrizzük csak meg az eredményfában.
                    result += subresult;
                }
            }

            // Ha elég kritérium megvolt
            result.Value = required <= 0;

            if (!result && result.Credit < this.Credit)
            {
                // Ha nem teljesül a feltétel, hiányoznak a kreditek.
                result.Credit = this.Credit;
            }

            // Érték továbbadás
            if (this.PassWeight)
            {
                result.Weight = (result && !this.EvaluateAll) ? this.Weight : this.Weight - required;
            }
            else
            {
                result.Weight = result ? 1 : 0;
            }

            return result;
        }

        public double Order(StudentCourseProxy source)
        {
            return 0;
        }

        public MatchGroup()
        {
            this.Requirement = 1;
            this.Children = new List<IExpression>();
        }
    }
}
