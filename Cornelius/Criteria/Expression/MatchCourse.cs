using Cornelius.Data;

namespace Cornelius.Criteria.Expression
{
    /// <summary>
    /// Tárgykritériumot definiáló osztály. Ellenőrzi, hogy egy adott tárgyból
    /// megvan-e a teljesítés vagy az aláírás. Rendezőelvként a kreditsúlyozott
    /// osztályzatot használja, a kizárásos tárgyakat a végére dobva negatív értékekkel.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{ToString()}")]
    class MatchCourse : IExpression
    {
        /// <summary>
        /// A tárgy neve.
        /// </summary>
        public string Name 
            { get; set; }   
     
        /// <summary>
        /// A tárgy kódja.
        /// </summary>
        public string Code 
            { get; set; }

        /// <summary>
        /// Elegendő-e csak az aláírás.
        /// </summary>
        public bool Signature 
            { get; set; }

        /// <summary>
        /// Kizárásos tárgy, vagyis csak egyetlen kritériumba számíthat egyszerre.
        /// (Vigyázzat, a kizárásos tárgyak csak egymást zárják ki, az azonos
        /// tárgykódú, de nem kizárásos feltételek nem érintik egymást!)
        /// </summary>
        public bool Exclusive
            { get; set; }

        /// <summary>
        /// A tárgy hosszú neve TÁRGYNÉV (TÁRGYKÓD) formátumban.
        /// </summary>
        public string LongName
        {
            get
            {
                return this.Name + " (" + this.Code + ")";
            }
        }

        /// <summary>
        /// A sorbarendezéshez a kreditsúlyozott osztályzatot használjuk. Az aláírás értéke, ahogy a nem
        /// teljesített tárgyé is, nulla. A kizárásos tárgyak súlyozása negatív.
        /// </summary>
        /// <param name="proxy">A hallgatókat kurzusokkal összekötő proxy.</param>
        /// <returns>Kreditsúlyozott átlag mint rendezőszám. [?]</returns>
        public double Order(StudentCourseProxy proxy)
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

        /// <summary>
        /// Feltétel kiértékelése.
        /// </summary>
        /// <param name="proxy">A hallgatót és kurzusokat összekötő proxy.</param>
        /// <returns>A kiértékelés eredménye.</returns>
        public Result Evaluate(StudentCourseProxy proxy)
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

        /// <summary>
        /// Tárgykritériumot definiáló osztály.
        /// </summary>
        /// <param name="code">Tárgykód</param>
        /// <param name="name">Tárgynév</param>
        /// <param name="signature">Csak aláírás kell-e</param>
        /// <param name="exclusive">Exkluzív követelmény-e</param>
        public MatchCourse(string code, string name, bool signature, bool exclusive)
        {
            this.Code = code;
            this.Name = name;
            this.Signature = signature;
            this.Exclusive = exclusive;
        }

        /// <summary>
        /// A kiértékelhető feltétel súlya.
        /// </summary>
        public int Weight
        {
            get { return 1; }
        }

        public override string ToString()
        {
            return LongName;
        }
    }
}
