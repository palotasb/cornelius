namespace Cornelius.Criteria.Expression
{
    /// <summary>
    /// Interfész kiértékelhető feltételekre. A kritériumok így tudnak egymáshoz kapcsolódni
    /// és láncként kiértékelődni.
    /// </summary>
    interface IExpression
    {
        /// <summary>
        /// A feltétel rendezőszáma. Ez valósítja meg a jegyalapú súlyozást, így mindig
        /// a legelőnyösebb kritériumok kerülnek befogadásra.
        /// </summary>
        /// <param name="source">A hallgatót és kurzust összekötő proxy.</param>
        /// <returns>Rendezőszám.</returns>
        double Order(StudentCourseProxy source);

        /// <summary>
        /// Feltétel kiértékelése és az eredmények, aleredmények összegyűjtése.
        /// </summary>
        /// <param name="source">A hallgatót és kurzust összekötő proxy.</param>
        /// <returns>A kiértékelés eredménye.</returns>
        Result Evaluate(StudentCourseProxy source);

        /// <summary>
        /// A kiértékelhető feltétel súlya.
        /// </summary>
        int Weight { get; }
    }
}
