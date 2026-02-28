using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace Touzi.TheOneRing
{
    public class QuestNode_Root_OneRingRuins : QuestNode
    {
        private const int MinDistanceFromColony = 2;
        private const int MaxDistanceFromColony = 10;

        protected override void RunInt()
        {
            if (!ModLister.CheckIdeology("One Ring ruins"))
            {
                return;
            }

            if (HasPlayerObtainedRing())
            {
                return;
            }

            Quest quest = QuestGen.quest;
            Slate slate = QuestGen.slate;
            Map map = QuestGen_Get.GetMap(mustBeInfestable: false, null, canBeSpace: true);
            float points = slate.Get("points", 0f);

            if (!TryFindSiteTile(out var tile))
            {
                return;
            }

            Thing ring = ThingMaker.MakeThing(ThingDef.Named("OneRing"));

            // 添加任务奖励显示
            Reward_Items rewardItem = new Reward_Items
            {
                items = { ring }
            };
            QuestPart_Choice questPartChoice = quest.RewardChoice();
            QuestPart_Choice.Choice choice = new QuestPart_Choice.Choice
            {
                rewards = { rewardItem }
            };
            questPartChoice.choices.Add(choice);

            string awakenSecurityThreatsSignal = QuestGen.GenerateNewSignal("AwakenSecurityThreats");
            string relicLostSignal = QuestGen.GenerateNewSignal("RelicLostFromMap");

            bool allowViolentQuests = Find.Storyteller.difficulty.allowViolentQuests;
            float num = allowViolentQuests ? points : 0f;

            SitePartParams sitePartParams = new SitePartParams
            {
                points = num,
                relicThing = ring,
                triggerSecuritySignal = awakenSecurityThreatsSignal,
                relicLostSignal = relicLostSignal
            };

            if (num > 0f)
            {
                sitePartParams.exteriorThreatPoints = 500f;
                sitePartParams.interiorThreatPoints = 300f;
            }

            Site site = QuestGen_Sites.GenerateSite(
                Gen.YieldSingle(new SitePartDefWithParams(SitePartDefOf.AncientAltar, sitePartParams)),
                tile,
                Faction.OfAncientsHostile
            );

            quest.SpawnWorldObject(site);

            TaggedString letterText = "你在远古遗迹中发现了至尊魔戒的埋藏地点。\n\n古老的传说似乎并非虚构，那枚传说中的魔戒就在这座遗迹深处。";

            quest.Letter(LetterDefOf.PositiveEvent, quest.AddedSignal, null, null, null, 
                useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOrNotYetAccepted, 
                label: "至尊魔戒的遗迹", text: letterText, lookTargets: Gen.YieldSingle(site));

            quest.DescriptionPart("[questDescriptionPartBeforeDiscovery]", quest.AddedSignal, quest.AddedSignal, QuestPart.SignalListenMode.OngoingOrNotYetAccepted);

            if (allowViolentQuests)
            {
                quest.DescriptionPart("小心！这些远古遗迹中可能隐藏着各种威胁。");
            }

            quest.End(QuestEndOutcome.Success, 0, null, relicLostSignal, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
            quest.End(QuestEndOutcome.Fail, 0, null, QuestGenUtility.HardcodedSignalWithQuestID("site.MapRemoved"), QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);

            slate.Set("site", site);
            slate.Set("ring", ring);
        }

        private bool TryFindSiteTile(out PlanetTile tile)
        {
            return TileFinder.TryFindNewSiteTile(out tile, MinDistanceFromColony, MaxDistanceFromColony, allowCaravans: false, null, 0.5f, canSelectComboLandmarks: true, TileFinderMode.Near, exitOnFirstTileFound: false);
        }

        private bool HasPlayerObtainedRing()
        {
            foreach (var map in Find.Maps)
            {
                foreach (var thing in map.listerThings.AllThings)
                {
                    if (thing.def?.defName == "OneRing")
                    {
                        if (thing.Faction == Faction.OfPlayer || thing.EverSeenByPlayer)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        protected override bool TestRunInt(Slate slate)
        {
            return !HasPlayerObtainedRing() && TryFindSiteTile(out var _);
        }
    }
}
