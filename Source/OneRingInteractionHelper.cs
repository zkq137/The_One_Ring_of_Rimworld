using Verse;
using RimWorld;

namespace Touzi.TheOneRing
{
    public static class OneRingInteractionHelper
    {
        public static bool IsWearingOneRing(Pawn pawn)
        {
            if (pawn == null || pawn.Dead || !pawn.Spawned)
                return false;
            
            if (pawn.apparel == null || pawn.apparel.WornApparelCount == 0)
                return false;
            
            return pawn.apparel.WornApparel.Any(a => a.def.defName == "OneRing");
        }

        public static bool HasHighCorrosionStage(Pawn pawn)
        {
            if (pawn == null || pawn.Dead)
                return false;

            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("OneRingStage3")) 
                      ?? pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("OneRingStage4"));
            
            return hediff != null;
        }
    }
}
