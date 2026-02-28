using System.Collections.Generic;
using Verse;
using RimWorld;

namespace Touzi.TheOneRing
{
    public class InteractionWorker_OneRingAwe : InteractionWorker
    {
        private const float BaseWeight = 0.5f;

        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            if (initiator == null || recipient == null)
                return 0f;

            if (recipient.Dead || !recipient.Spawned)
                return 0f;

            if (!OneRingInteractionHelper.IsWearingOneRing(recipient))
                return 0f;

            return BaseWeight;
        }
    }

    public class InteractionWorker_OneRingJealousy : InteractionWorker
    {
        private const float BaseWeight = 2.0f;

        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            if (initiator == null || recipient == null)
                return 0f;

            if (recipient.Dead || !recipient.Spawned)
                return 0f;

            if (!OneRingInteractionHelper.IsWearingOneRing(recipient))
                return 0f;

            return BaseWeight;
        }
    }

    public class InteractionWorker_OneRingFear : InteractionWorker
    {
        private const float BaseWeight = 0.5f;

        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            if (initiator == null || recipient == null)
                return 0f;

            if (recipient.Dead || !recipient.Spawned)
                return 0f;

            if (!OneRingInteractionHelper.IsWearingOneRing(recipient))
                return 0f;

            if (!OneRingInteractionHelper.HasHighCorrosionStage(recipient))
                return 0f;

            return BaseWeight;
        }
    }

    public class InteractionWorker_OneRingDesire : InteractionWorker
    {
        private const float BaseWeight = 1.0f;

        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            if (initiator == null || recipient == null)
                return 0f;

            if (recipient.Dead || !recipient.Spawned)
                return 0f;

            if (!OneRingInteractionHelper.IsWearingOneRing(recipient))
                return 0f;

            return BaseWeight;
        }
    }
}
