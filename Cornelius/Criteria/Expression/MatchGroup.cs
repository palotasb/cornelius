using System.Collections.Generic;
using System.Linq;

namespace Cornelius.Criteria.Expression
{
    /// <summary>
    /// Kritériumcsoport, ami akkor igaz, ha a tagkritériumaira teljesül a 
    /// megfelelő feltétel.
    /// </summary>
    class MatchGroup : IExpression
    {
        /// <summary>
        /// Kritérium neve.
        /// </summary>
        public string Name
            { get; set; }

        /// <summary>
        /// Rendezéshez használt félév száma.
        /// </summary>
        public int? Semester 
            { get; set; }

        /// <summary>
        /// Elméleti kreditérték, ami hiányzik, ha nem teljesül a tárgy.
        /// </summary>
        public double Credit
            { get; set; }

        /// <summary>
        /// Az alfeltételekből ennyinek kell teljesülnie. Speciális esetek:
        ///  - pozitív n: ennyi feltételnek kell teljesülnie
        ///  - nulla: ekkor minden feltételnek teljesülnie kell
        ///  - negatív n: ennyi feltétel nem teljesülése engedhető meg legfeljebb
        /// </summary>
        public int Requirement
            { get; set; }

        /// <summary>
        /// Súly továbbadása. Ha hamis, akkor a kritérium egy feltétellé
        /// "nyomódik össze" az anyakritérium szemében, ami vagy igaz vagy nem.
        /// </summary>
        public bool PassWeight
            { get; set; }

        /// <summary>
        /// Minden alfeltétel kiértékelése. Ha igaz, azután is folytatódik a kiértékelődés,
        /// ha már van elegendő feltétel az igazzá váláshoz.
        /// </summary>
        public bool EvaluateAll
            { get; set; }

        /// <summary>
        /// A feltétel azokra vonatkozik csak, akik a megadott képzésben
        /// kezdték mag tanulmányaikat.
        /// </summary>
        public string OriginalEducationProgram
            { get; set; }

        /// <summary>
        /// A feltételek listája.
        /// </summary>
        public List<IExpression> Children
            { get; protected set; }

        /// <summary>
        /// A kritérium súlya, vagyis a ténylegesen teljesítendő kritériumok száma
        /// </summary>
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

        /// <summary>
        /// Kiértékelés
        /// </summary>
        public Result Evaluate(StudentCourseProxy proxy)
        {
            Result result = new Result(this.Name);
            result.Semester = this.Semester > 0 ? this.Semester : null;

            if (this.OriginalEducationProgram != null && proxy.OriginalEduProgramCode != this.OriginalEducationProgram)
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

        /// <summary>
        /// A feltétel rendezőszáma. Ez valósítja meg a jegyalapú súlyozást, így mindig
        /// a legelőnyösebb kritériumok kerülnek befogadásra.
        /// </summary>
        /// <param name="source">A hallgatót és kurzust összekötő proxy.</param>
        /// <returns>Rendezőszám.</returns>
        public double Order(StudentCourseProxy source)
        {
            return 0;
        }

        /// <summary>
        /// A kiértékelhető feltétel súlya.
        /// </summary>
        public MatchGroup()
        {
            this.Requirement = 1;
            this.Children = new List<IExpression>();
        }
    }
}
