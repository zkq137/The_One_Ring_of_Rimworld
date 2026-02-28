using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;

namespace Touzi.TheOneRing
{
    public class IncidentWorker_OneRingWhisper : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
                return false;

            Map map = (Map)parms.target;
            return HasUnwornOneRing(map) && HasIdleColonist(map);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            
            // 1. 找到未佩戴的魔戒
            Thing oneRing = FindUnwornOneRing(map);
            if (oneRing == null)
                return false;

            // 2. 找到所有空闲的殖民者
            var idlePawns = GetIdlePawns(map);
            if (!idlePawns.Any())
                return false;

            // 3. 随机选择一个
            Pawn targetPawn = idlePawns.RandomElement();

            // 4. 分配Wear任务
            Job job = JobMaker.MakeJob(JobDefOf.Wear, oneRing);
            targetPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);

            // 5. 显示消息
            SendStandardLetter(
                def.letterLabel,
                def.letterText.Formatted(targetPawn.Name.ToStringShort).AdjustedFor(targetPawn),
                LetterDefOf.NegativeEvent,
                parms,
                targetPawn
            );

            return true;
        }

        private bool HasUnwornOneRing(Map map)
        {
            return map.listerThings.AllThings
                .Any(t => t.def.defName == "OneRing" && t.Spawned && !t.IsForbidden(Faction.OfPlayer));
        }

        private Thing FindUnwornOneRing(Map map)
        {
            return map.listerThings.AllThings
                .FirstOrDefault(t => t.def.defName == "OneRing" && t.Spawned && !t.IsForbidden(Faction.OfPlayer));
        }

        private bool HasIdleColonist(Map map)
        {
            return GetIdlePawns(map).Any();
        }

        private List<Pawn> GetIdlePawns(Map map)
        {
            Thing oneRing = FindUnwornOneRing(map);
            if (oneRing == null)
                return new List<Pawn>();

            return map.mapPawns.FreeColonistsSpawned
                .Where(p => 
                    !p.Dead && 
                    !p.InMentalState
                )
                .ToList();
        }
    }
}
