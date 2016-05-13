﻿namespace Cornelius.Criteria.Expression
{
    /*
     * Interfész kiértékelhető feltételekre. A kritériumok így tudnak egymáshoz kapcsolódni
     * és láncként kiértékelődni.
     */
    interface IExpression
    {
        /*
         * A feltétel rendezőszáma. Ez valósítja meg a jegyalapú súlyozást, így mindig 
         * a legelőnyösebb kritériumok kerülnek befogadásra.
         */
        double Order(Proxy source);

        /*
         *  Feltétel kiértékelése és az eredmények, aleredmények összegyűjtése.
         */
        Result Evaluate(Proxy source);

        int Weight { get; }
    }
}
