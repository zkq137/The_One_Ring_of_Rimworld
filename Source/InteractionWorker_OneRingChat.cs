using System.Collections.Generic;
using Verse;
using RimWorld;

namespace Touzi.TheOneRing
{
    public class InteractionWorker_OneRingChat : InteractionWorker
    {
        private const float BaseWeight = 1.0f;

        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            if (initiator == null || recipient == null)
            {
                return 0f;
            }

            if (recipient.Dead || recipient.Spawned == false)
            {
                return 0f;
            }

            if (recipient.apparel == null || recipient.apparel.WornApparelCount == 0)
            {
                return 0f;
            }

            if (recipient.apparel.WornApparel.Any(a => a.def.defName == "OneRing"))
            {
                return BaseWeight;
            }

            return 0f;
        }

        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            base.Interacted(initiator, recipient, extraSentencePacks, out letterText, out letterLabel, out letterDef, out lookTargets);
        }
    }
}
