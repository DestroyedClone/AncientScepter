using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AncientScepter.Modules
{
    internal class ItemDisplayConfigurer
    {
        internal ItemDisplayRuleDict CreateDisplayRules()
        {
            SetupMaterials(ItemModel);
            ItemDisplay disp = ItemModel.AddComponent<ItemDisplay>();
            disp.rendererInfos = Assets.SetupRendererInfos(ItemModel);

            displayPrefab = ItemDisplay;
            SetupMaterials(displayPrefab);
            disp = displayPrefab.AddComponent<ItemDisplay>();
            disp.rendererInfos = Assets.SetupRendererInfos(displayPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.1473F, -0.073F, -0.0935F),
                    localAngles = new Vector3(333.2843F, 198.8161F, 165.1177F),
                    localScale = new Vector3(0.2235F, 0.2235F, 0.2235F)
                });

            rules.Add("mdlHuntress", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "Pelvis",
                localPos = new Vector3(0F, 0.0638F, 0.0973F),
                localAngles = new Vector3(76.6907F, 0F, 0F),
                localScale = new Vector3(0.2812F, 0.2812F, 0.2812F)
            });

            rules.Add("mdlMage", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "HandR",
                localPos = new Vector3(-0.0021F, 0.1183F, 0.063F),
                localAngles = new Vector3(0F, 34.1F, 90F),
                localScale = new Vector3(0.4416F, 0.4416F, 0.4416F)
            });

            rules.Add("mdlEngi", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "CannonHeadR",
                localPos = new Vector3(0.0186F, 0.3435F, 0.2246F),
                localAngles = new Vector3(0F, 0F, 0F),
                localScale = new Vector3(0.5614F, 0.5614F, 0.5614F)
            });

            rules.Add("mdlMerc", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "Pelvis",
                localPos = new Vector3(0.1712F, 0F, 0F),
                localAngles = new Vector3(69.8111F, 180F, 180F),
                localScale = new Vector3(0.2679F, 0.2679F, 0.2679F)
            });

            rules.Add("mdlLoader", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "MechLowerArmR",
                localPos = new Vector3(0.0813F, 0.4165F, -0.0212F),
                localAngles = new Vector3(0F, 180F, 180F),
                localScale = new Vector3(0.4063F, 0.4063F, 0.4063F)
            });

            rules.Add("mdlCaptain", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "Chest",
                localPos = new Vector3(-0.0046F, 0.0099F, -0.286F),
                localAngles = new Vector3(10.4706F, 1.6895F, 24.8468F),
                localScale = new Vector3(0.4928F, 0.4928F, 0.4928F)
            });

            rules.Add("mdlToolbot", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "Chest",
                localPos = new Vector3(1.1191F, 0.358F, -1.6717F),
                localAngles = new Vector3(0F, 0F, 270F),
                localScale = new Vector3(2.4696F, 2.4696F, 2.4696F)
            });

            rules.Add("mdlTreebot", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "CalfFrontL",
                localPos = new Vector3(0F, 0.8376F, -0.1766F),
                localAngles = new Vector3(0F, 0F, 0F),
                localScale = new Vector3(0.8037F, 0.8037F, 0.8037F)
            });

            rules.Add("mdlCroco", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "MouthMuzzle",
                localPos = new Vector3(0F, 2.1215F, 2.9939F),
                localAngles = new Vector3(0F, 0F, 270F),
                localScale = new Vector3(5.2969F, 5.2969F, 5.2969F)
            });

            rules.Add("mdlBandit", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "Pelvis",
                localPos = new Vector3(-0.1152f, -0.1278f, 0.2056f),
                localAngles = new Vector3(20F, 285F, 10F),
                localScale = new Vector3(0.2235F, 0.2235F, 0.2235F)
            });

            rules.Add("mdlEnforcer", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "Pelvis",
                localPos = new Vector3(-0.08448F, 0.00357F, -0.35566F),
                localAngles = new Vector3(43.57039F, 347.6845F, 69.64303F),
                localScale = new Vector3(0.31291F, 0.31291F, 0.31291F)
            });

            rules.Add("mdlNemforcer", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "Minigun",
                localPos = new Vector3(0.00287F, -0.00305F, -0.03029F),
                localAngles = new Vector3(358.9499F, 89.5545F, 180.8908F),
                localScale = new Vector3(0.00837F, 0.00837F, 0.00837F)
            });

            rules.Add("mdlVoidSurvivor", new ItemDisplayRule
            {
                childName = "Hand",
                followerPrefab = displayPrefab,
                localPos = new Vector3(-0.02335F, 0.11837F, 0.11306F),
                localAngles = new Vector3(55.42191F, 299.1461F, 266.1845F),
                localScale = new Vector3(0.56092F, 0.56276F, 0.56092F)
            });

            rules.Add("mdlRailGunner", new ItemDisplayRule
            {
                childName = "ThighR",
                followerPrefab = displayPrefab,
                localPos = new Vector3(-0.11836F, 0.17205F, 0.0282F),
                localAngles = new Vector3(353.4687F, 184.4017F, 177.4758F),
                localScale = new Vector3(0.2235F, 0.2235F, 0.2235F)
            });

            rules.Add("mdlNemmando", new ItemDisplayRule
            {
                childName = "Sword",
                followerPrefab = displayPrefab,
                localPos = new Vector3(-0.00005576489F, 0.001674413F, -0.00002617424F),
                localAngles = new Vector3(1.114511F, 204.2958F, 177.8329F),
                localScale = new Vector3(0.0026F, 0.0026F, 0.0026F)
            });

            rules.Add("mdlHeretic", new ItemDisplayRule
            {
                childName = "ThighL",
                followerPrefab = displayPrefab,
                localPos = new Vector3(0.49264F, -0.16267F, -0.14486F),
                localAngles = new Vector3(9.97009F, 351.3801F, 100.2498F),
                localScale = new Vector3(0.5F, 0.5F, 0.5F)
            });

            rules.Add("mdlBrother", new ItemDisplayRule
            {
                childName = "HandL",
                followerPrefab = displayPrefab,
                localPos = new Vector3(-0.05066F, 0.13436F, 0.0282F),
                localAngles = new Vector3(79.95749F, 180F, 230.595F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            return rules;
        }

    }
}
