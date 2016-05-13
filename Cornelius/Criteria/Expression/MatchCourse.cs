using Cornelius.Data;

namespace Cornelius.Criteria.Expression
{
    /*
     * Tárgykritériumot definiáló osztály. Ellenőrzi, hogy egy adott tárgyból
     * megvan-e a teljesítés vagy az aláírás. Rendezőelvként a kreditsúlyozott 
     * osztályzatot használja, a kizárásos tárgyakat a végére dobva negatív értékekkel.
     */
    class MatchCourse : IExpression
    {
        /*
         * A tárgy neve.
         */
        public string Name 
            { get; set; }   
     
        /*
         * A tárgy kódja.
         */
        public string Code 
            { get; set; }

        /*
         * Elegendő-e csak az aláírás.
         */
        public bool Signature 
            { get; set; }

        /*
         * Kizárásos tárgy, vagyis csak egyetlen kritériumba számíthat egyszerre.
         * (Vigyázzat, a kizárásos tárgyak csak egymást zárják ki, az azonos
         * tárgykódú, de nem kizárásos feltételek nem érintik egymást!)
         */
        public bool Exclusive
            { get; set; }

        /*
         * A tárgy hosszú neve TÁRGYNÉV (TÁRGYKÓD) formátumban.
         */
        public string LongName
        {
            get
            {
                return this.Name + " (" + this.Code + ")";
            }
        }

        /*
         * A sorbarendezéshez a kreditsúlyozott osztályzatot használjuk. Az aláírás értéke, ahogy a nem
         * teljesített tárgyé is, nulla. A kizárásos tárgyak súlyozása negatív.
         */
        public double Order(Proxy proxy)
        {
            if (!this.Signature)
            {
                Course course = proxy.Check(this.Code);
                if (course != null && course.HasCompleted)
                {
                    return course.Credit * course.Grade + (this.Exclusive ? -100 : 0);
                }
            }
            return 0;
        }

        /*
         * Feltétel kiértékelése.
         */
        public Result Evaluate(Proxy proxy)
        {
            Course course = proxy.Check(this.Code);
            if (course == null)
            {
                // Ha nem volt ilyen kurzusa
                return new Result(this.LongName, false);
            }
            else
            {
                if (this.Signature || !course.HasCompleted || (this.Exclusive && !proxy.Lock(this.Code)))
                {
                    // Ha volt ilyen kurzusa, és
                    // - aláírás kell csak belőle
                    // - nem teljesítette
                    // - kizárás miatt nem használható
                    // Ez az eredmény nem módosítja az átlagot.
                    return new Result(this.LongName, this.Signature ? course.HasSignature : false);
                }
                else
                {
                    // Ha a kurzus használható és teljesített,
                    // az eredmény módosítja az átlagot.
                    return new Result(course, !proxy.Use(this.Code));
                }
            }
        }

        public MatchCourse(string code, string name, bool signature, bool exclusive)
        {
            this.Code = code;
            this.Name = name;
            this.Signature = signature;
            this.Exclusive = exclusive;
        }


        public int Weight
        {
            get { return 1; }
        }
    }
}
