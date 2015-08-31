﻿namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System;
    using System.Threading.Tasks;

    using ff14bot;
    using ff14bot.Managers;

    [GatheringRotation("Collect550", 600, 34)]
    public class Collect550GatheringRotation : DefaultCollectGatheringRotation
    {
        public override bool ForceGatherIfMissingGpOrTime
        {
            get
            {
                return true;
            }
        }

        public override async Task<bool> ExecuteRotation(GatherCollectableTag tag)
        {
            // Not level 60.
            if (tag.GatherItem.Chance > 95)
            {
                await Impulsive(tag);
                await Impulsive(tag);
                await Methodical(tag);

                // level 58 only
                if (tag.GatherItem.Chance < 99 && (Core.Player.CurrentGP >= 650 || (Core.Player.MaxGP - Core.Player.CurrentGP) <= 50))
                {
                    await tag.Cast(Ability.IncreaseGatherChance5);
                }

                return true;
            }

            var appraisalsRemaining = 4;
            await Impulsive(tag);
            appraisalsRemaining--;

            if (HasDiscerningEye)
            {
                await SingleMindUtmostMethodical(tag);
                appraisalsRemaining--;
            }

            await Impulsive(tag);
            appraisalsRemaining--;

            if (HasDiscerningEye)
            {
                await SingleMindUtmostMethodical(tag);
                appraisalsRemaining--;
            }

            if (appraisalsRemaining == 2)
            {
                await Methodical(tag);
            }

            if (appraisalsRemaining == 1)
            {
                await DiscerningUtmostMethodical(tag);
            }

            await IncreaseChance(tag);

            return true;
        }

        public override int ShouldOverrideSelectedGatheringRotation(GatherCollectableTag tag)
        {
            if (tag.IsUnspoiled())
            {
                // We need 5 swings to use this rotation
                if (GatheringManager.SwingsRemaining < 5)
                {
                    return -1;
                }
            }

            if (tag.IsEphemeral())
            {
                // We need 4 swings to use this rotation
                if (GatheringManager.SwingsRemaining < 4)
                {
                    return -1;
                }
            }

            // if we have a collectable && the collectable value is greater than or equal to 550: Priority 550
            if (tag.CollectableItem != null && tag.CollectableItem.Value >= 550)
            {
                return 550;
            }

            return -1;
        }
    }
}