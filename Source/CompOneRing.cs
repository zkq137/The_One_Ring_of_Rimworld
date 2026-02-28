using System;
using System.Linq;
using Verse;
using RimWorld;

namespace Touzi.TheOneRing
{
    public class CompOneRing : ThingComp
    {
        // 调试开关 - 设置为true以启用详细日志
        private const bool DEBUG_MODE = true;
        
        public bool everOwnedByPlayer = false;
        
        /// <summary>
        /// 调试日志方法
        /// </summary>
        private void DebugLog(string message)
        {
            if (DEBUG_MODE)
            {
                Log.Message($"[TheOneRing DEBUG] {message}");
            }
        }
        
        /// <summary>
        /// 错误日志方法
        /// </summary>
        private void ErrorLog(string message)
        {
            Log.Error($"[TheOneRing ERROR] {message}");
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
        }
        
        /// <summary>
        /// 验证Hediff是否成功添加
        /// </summary>
        private bool VerifyHediffAdded(Pawn pawn, HediffDef hediffDef)
        {
            if (pawn.health.hediffSet.HasHediff(hediffDef))
            {
                DebugLog($"验证成功: Hediff {hediffDef.defName} 已添加到殖民者 {pawn.Name}");
                return true;
            }
            else
            {
                ErrorLog($"验证失败: Hediff {hediffDef.defName} 未添加到殖民者 {pawn.Name}");
                return false;
            }
        }
        
        /// <summary>
        /// 清理所有魔戒相关的状态（除了崩溃历史）
        /// </summary>
        private void CleanAllRingStates(Pawn pawn)
        {
            if (pawn == null || pawn.Dead)
                return;
                
            // 移除所有OneRing开头的Hediff（除了崩溃历史）
            var ringHediffs = pawn.health.hediffSet.hediffs
                .Where(h => h.def.defName.StartsWith("OneRing") && 
                            h.def.defName != "OneRingBerserked")
                .ToList();
            
            foreach (var hediff in ringHediffs)
            {
                pawn.health.RemoveHediff(hediff);
                DebugLog($"清理Hediff: {hediff.def.defName}");
            }
            
            // 额外移除心灵隐身效果
            var invisibilityHediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("PsychicInvisibility"));
            if (invisibilityHediff != null)
            {
                pawn.health.RemoveHediff(invisibilityHediff);
                DebugLog("清理隐身Hediff: PsychicInvisibility");
            }
            
            // Hediff移除会自动清理对应的思想，无需手动清理
            DebugLog($"清理完成，移除了 {ringHediffs.Count} 个魔戒相关Hediff");
        }
        
        /// <summary>
        /// 授予魔戒技能给殖民者
        /// </summary>
        private void GrantRingAbilities(Pawn pawn)
        {
            if (pawn == null || pawn.abilities == null)
                return;
            
            var abilityDefInvisibility = DefDatabase<AbilityDef>.GetNamedSilentFail("OneRingInvisibility");
            var abilityDefVertigo = DefDatabase<AbilityDef>.GetNamedSilentFail("OneRingVertigoPulse");
            
            if (abilityDefInvisibility != null && pawn.abilities.GetAbility(abilityDefInvisibility) == null)
            {
                pawn.abilities.GainAbility(abilityDefInvisibility);
                DebugLog($"授予技能: OneRingInvisibility");
            }
            
            if (abilityDefVertigo != null && pawn.abilities.GetAbility(abilityDefVertigo) == null)
            {
                pawn.abilities.GainAbility(abilityDefVertigo);
                DebugLog($"授予技能: OneRingVertigoPulse");
            }
        }
        
        /// <summary>
        /// 移除魔戒技能
        /// </summary>
        private void RemoveRingAbilities(Pawn pawn)
        {
            if (pawn == null || pawn.abilities == null)
                return;
            
            var abilityDefInvisibility = DefDatabase<AbilityDef>.GetNamedSilentFail("OneRingInvisibility");
            var abilityDefVertigo = DefDatabase<AbilityDef>.GetNamedSilentFail("OneRingVertigoPulse");
            
            if (abilityDefInvisibility != null)
            {
                pawn.abilities.RemoveAbility(abilityDefInvisibility);
                DebugLog($"移除技能: OneRingInvisibility");
            }
            
            if (abilityDefVertigo != null)
            {
                pawn.abilities.RemoveAbility(abilityDefVertigo);
                DebugLog($"移除技能: OneRingVertigoPulse");
            }
        }
        
        /// <summary>
        /// 授予启灵神经（确保可以使用灵能技能）
        /// </summary>
        private void GrantPsylink(Pawn pawn)
        {
            if (pawn == null || pawn.health == null)
                return;
            
            // 检查是否已有启灵神经
            var existingPsylink = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicAmplifier);
            if (existingPsylink != null)
            {
                // 已有启灵神经，不处理
                return;
            }
            
            // 添加1级启灵神经
            var brain = pawn.health.hediffSet.GetBrain();
            if (brain != null)
            {
                var psylinkHediff = HediffMaker.MakeHediff(HediffDefOf.PsychicAmplifier, pawn, brain) as Hediff_Psylink;
                if (psylinkHediff != null)
                {
                    psylinkHediff.SetLevelTo(1);
                    pawn.health.AddHediff(psylinkHediff);
                    DebugLog("授予1级启灵神经");
                }
            }
        }
        
        /// <summary>
        /// 获取殖民者当前的魔戒阶段 (1-4)，0表示无阶段
        /// </summary>
        private int GetCurrentStage(Pawn pawn)
        {
            if (pawn == null || pawn.health == null)
                return 0;
            
            // 检查各个阶段
            if (pawn.health.hediffSet.HasHediff(HediffDef.Named("OneRingStage1_New")))
                return 1;
            if (pawn.health.hediffSet.HasHediff(HediffDef.Named("OneRingStage2")))
                return 2;
            if (pawn.health.hediffSet.HasHediff(HediffDef.Named("OneRingStage3")))
                return 3;
            if (pawn.health.hediffSet.HasHediff(HediffDef.Named("OneRingStage4")))
                return 4;
            
            return 0;
        }
        
        /// <summary>
        /// 根据崩溃历史记录获取应该进入的阶段
        /// </summary>
        private int GetStageFromBerserkedHistory(Pawn pawn)
        {
            if (pawn == null || pawn.health == null)
                return 2; // 默认进入阶段2
            
            var berserked = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("OneRingBerserked"));
            if (berserked == null)
                return 1; // 无崩溃历史，从阶段1开始
            
            var stageComp = berserked.TryGetComp<CompOneRing_BerserkedStage>();
            if (stageComp == null)
                return 2; // 无阶段记录，默认进入阶段2
            
            int lastStage = stageComp.lastStage;
            DebugLog($"读取到上次崩溃阶段: {lastStage}");
            
            // 边界检查
            if (lastStage <= 1)
                return 2; // 之前是阶段1，再次戴上直接进阶段2
            if (lastStage > 4)
                return 2; // 异常情况，默认进阶段2
            
            return lastStage; // 恢复之前的阶段
        }
        
        /// <summary>
        /// 根据阶段获取对应的HediffDef
        /// </summary>
        private HediffDef GetStageHediffDef(int stage)
        {
            return stage switch
            {
                1 => HediffDef.Named("OneRingStage1_New"),
                2 => HediffDef.Named("OneRingStage2"),
                3 => HediffDef.Named("OneRingStage3"),
                4 => HediffDef.Named("OneRingStage4"),
                _ => HediffDef.Named("OneRingStage2")
            };
        }
        
        public override void Notify_Equipped(Pawn pawn)
        {
            DebugLog($"开始执行 Notify_Equipped - 殖民者: {pawn?.Name}, 物品: {parent?.def?.defName}");
            
            everOwnedByPlayer = true;
            
            base.Notify_Equipped(pawn);
            
            if (pawn == null)
            {
                ErrorLog("pawn参数为null");
                return;
            }
            
            if (pawn.Dead)
            {
                DebugLog("殖民者已死亡，跳过处理");
                return;
            }
            
            // 1. 清理所有魔戒相关状态（除了崩溃历史）
            DebugLog("开始清理所有魔戒相关状态");
            CleanAllRingStates(pawn);
            
            // 2. 检查是否有崩溃历史，并根据记录的阶段决定起始阶段
            bool hasBerserked = pawn.health.hediffSet.HasHediff(HediffDef.Named("OneRingBerserked"));
            DebugLog($"检查崩溃历史: {hasBerserked}");
            
            // 3. 根据崩溃历史记录决定Hediff
            HediffDef stageHediff;
            int targetStage;
            
            if (hasBerserked)
            {
                // 有崩溃历史：根据记录的阶段决定
                targetStage = GetStageFromBerserkedHistory(pawn);
                DebugLog($"有崩溃历史，根据记录确定进入阶段: {targetStage}");
            }
            else
            {
                // 无崩溃历史：从阶段1开始
                targetStage = 1;
                DebugLog("无崩溃历史，从阶段1开始");
            }
            
            stageHediff = GetStageHediffDef(targetStage);
            
            if (stageHediff == null)
            {
                ErrorLog($"无法找到Hediff定义: {stageHediff?.defName}");
                return;
            }
            
            DebugLog($"将添加Hediff: {stageHediff.defName} 给殖民者 {pawn.Name}");
            
            // 4. 添加Hediff（思想由Hediff的ThoughtSetter自动添加）
            Hediff addedHediff = pawn.health.AddHediff(stageHediff);
            if (addedHediff == null)
            {
                ErrorLog($"添加Hediff失败: {stageHediff.defName}");
                return;
            }
            
            DebugLog($"Hediff添加成功: {addedHediff.def.defName}, ID: {addedHediff.loadID}");
            
            // 5. 验证Hediff是否真的被添加
            if (!VerifyHediffAdded(pawn, stageHediff))
            {
                ErrorLog("Hediff验证失败，尝试重新添加");
                addedHediff = pawn.health.AddHediff(stageHediff);
                if (addedHediff == null)
                {
                    ErrorLog("重新添加Hediff也失败");
                }
            }
            
            // 6. 检查殖民者当前的所有思想（验证ThoughtSetter是否工作）
            if (pawn.needs?.mood?.thoughts?.memories != null)
            {
                var currentThoughts = pawn.needs.mood.thoughts.memories.Memories
                    .Where(m => m.def.defName.StartsWith("OneRing"))
                    .ToList();
                
                DebugLog($"殖民者当前与魔戒相关的思想数量: {currentThoughts.Count}");
                foreach (var thought in currentThoughts)
                {
                    DebugLog($"  - 思想: {thought.def.defName}, 心情效果: {thought.MoodOffset()}");
                }
            }
            
            // 8. 授予启灵神经（确保可以使用灵能技能）
            GrantPsylink(pawn);
            
            DebugLog("Notify_Equipped 执行完成");
        }
        
        public override void Notify_Unequipped(Pawn pawn)
        {
            DebugLog($"开始执行 Notify_Unequipped - 殖民者: {pawn?.Name}, 物品: {parent?.def?.defName}");
            base.Notify_Unequipped(pawn);
            
            if (pawn == null || pawn.Dead)
            {
                DebugLog($"pawn为null或已死亡，跳过处理 (pawn: {pawn?.Name}, Dead: {pawn?.Dead})");
                return;
            }
            
            // 使用Hediff机制延迟执行，避免干扰Job系统
            Hediff delayedHediff = pawn.health.AddHediff(HediffDef.Named("OneRingDelayedAction"));
            if (delayedHediff != null)
            {
                var delayedComp = delayedHediff.TryGetComp<CompOneRing_DelayedAction>();
                if (delayedComp != null)
                {
                    Pawn pawnCopy = pawn;
                    delayedComp.SetAction(() => {
                        DebugLog("Hediff延迟执行卸下逻辑");
                        HandleUnequip(pawnCopy);
                    });
                }
            }
            
            DebugLog("Notify_Unequipped 执行完成");
        }
        
        private void HandleUnequip(Pawn pawn)
        {
            DebugLog($"开始处理卸下逻辑 - 殖民者: {pawn.Name}");
            
            DebugLog("清理所有魔戒相关状态");
            CleanAllRingStates(pawn);
            
            RemoveRingAbilities(pawn);
            DebugLog("移除魔戒技能");
            
            // 获取当前阶段并记录
            int currentStage = GetCurrentStage(pawn);
            DebugLog($"记录当前阶段: {currentStage}");
            
            // 添加或更新崩溃历史，记录阶段
            var berserked = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("OneRingBerserked"));
            if (berserked == null)
            {
                berserked = pawn.health.AddHediff(HediffDef.Named("OneRingBerserked"));
            }
            
            var stageComp = berserked?.TryGetComp<CompOneRing_BerserkedStage>();
            if (stageComp != null)
            {
                stageComp.lastStage = currentStage;
                DebugLog($"已记录阶段到崩溃历史: {currentStage}");
            }
            else
            {
                DebugLog("警告：无法获取阶段记录组件");
            }
            
            DebugLog("添加戒断反应Hediff: OneRingWithdrawal");
            pawn.health.AddHediff(HediffDef.Named("OneRingWithdrawal"));
            
            Messages.Message(
                "至尊魔戒已被摘下！殖民者陷入了深深的痛苦之中...",
                pawn,
                MessageTypeDefOf.NegativeEvent
            );
            
            DebugLog("卸下逻辑处理完成");
        }
        
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref everOwnedByPlayer, "everOwnedByPlayer");
        }
    }
    
    public class CompProperties_OneRing : CompProperties
    {
        public CompProperties_OneRing()
        {
            compClass = typeof(CompOneRing);
        }
    }
    
    // 延迟执行动作的HediffComp
    public class CompOneRing_DelayedAction : HediffComp
    {
        private Action action;
        
        public void SetAction(Action action)
        {
            this.action = action;
        }
        
        public override void CompPostPostRemoved()
        {
            if (action != null)
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    Log.Error($"[OneRing] Delayed action error: {ex.Message}");
                }
                action = null;
            }
        }
    }
    
    // HediffComp的Properties
    public class CompProperties_OneRing_DelayedAction : HediffCompProperties
    {
        public CompProperties_OneRing_DelayedAction()
        {
            compClass = typeof(CompOneRing_DelayedAction);
        }
    }
    
    // 记录崩溃时所在的阶段
    public class CompOneRing_BerserkedStage : HediffComp
    {
        public int lastStage = 1; // 默认记录为阶段1
        
        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref lastStage, "lastStage", 1);
        }
    }
    
    public class CompProperties_OneRing_BerserkedStage : HediffCompProperties
    {
        public CompProperties_OneRing_BerserkedStage()
        {
            compClass = typeof(CompOneRing_BerserkedStage);
        }
    }
}
