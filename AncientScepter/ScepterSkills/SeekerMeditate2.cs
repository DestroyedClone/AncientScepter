using R2API;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using static AncientScepter.SkillUtil;

namespace AncientScepter
{
    public class SeekerMeditate2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }

        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Increases max Tranquility cap to 13.</color>";

        public override string targetBody => "SeekerBody";

        public override SkillSlot targetSlot => SkillSlot.Special;

        public override int targetVariantIndex => 0;

        internal override void SetupAttributes()
        {
            var oldDef = Addressables.LoadAssetAsync<SkillDef>("af3d023afcb582b4ea6c2709b16dc5ad").WaitForCompletion();
            myDef = CloneSkillDef(oldDef);

            var nameToken = "ANCIENTSCEPTER_SEEKER_MEDITATENAME";
            newDescToken = "ANCIENTSCEPTER_SEEKER_MEDITATEDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var nameStr = "Ascend";
            LanguageAPI.Add(nameToken, nameStr);

            myDef.skillName = $"{oldDef.skillName}Scepter";
            (myDef as ScriptableObject).name = myDef.skillName;
            myDef.skillNameToken = nameToken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.SpriteAssets.SeekerMeditate2;

            ContentAddition.AddSkillDef(myDef);
        }

        internal override void LoadBehavior()
        {
            // On.EntityStates.Seeker.Meditate.OnEnter += Meditate_OnEnter;
            On.EntityStates.Seeker.Meditate.CalculateBlastEffectScale += Meditate_CalculateBlastEffectScale;
            On.RoR2.SeekerController.Start += SeekerController_Start;
            On.RoR2.SeekerController.CmdIncrementChakraGate += SeekerController_CmdIncrementChakraGate;
            // On.RoR2.SeekerController.CmdTriggerHealPulse += SeekerController_CmdTriggerHealPulse; // this is just unneccesary lol
        }

        // also handling palm strike:
        private void SeekerController_CmdIncrementChakraGate(On.RoR2.SeekerController.orig_CmdIncrementChakraGate orig, SeekerController self)
        {
            if (self.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef == myDef || self.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef == SeekerPalmBlast2.skillDef)
            {
                if (self.characterBody.master.seekerChakraGate < 13)
                {
                    self.characterBody.master.seekerChakraGate++;
                }

                if (self.specialSkillSlot.baseSkill == self.meditateSkillDef || self.specialSkillSlot == myDef)
                {
                    self.UnlockGateEffects(self.characterBody.master.seekerChakraGate);
                }
                else if (self.specialSkillSlot.baseSkill == self.palmBlastSkillDef || self.specialSkillSlot == SeekerPalmBlast2.skillDef)
                {
                    self.UnlockPalmBlastEffects(self.characterBody.master.seekerChakraGate);
                }

                return;
            }

            orig(self);
        }

        // again, contains palm strike, too:
        private void SeekerController_Start(On.RoR2.SeekerController.orig_Start orig, SeekerController self)
        {
            if (NetworkServer.active)
            {
                if (self.characterBody.master.seekerChakraGate > 0 && (self.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef == myDef || self.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef == SeekerPalmBlast2.skillDef))
                {
                    for (int chakraAmount = 1; chakraAmount <= self.characterBody.master.seekerChakraGate; chakraAmount++)
                    {
                        if (chakraAmount < 13)
                        {
                            self.characterBody.AddBuff(DLC2Content.Buffs.ChakraBuff);
                        }
                    }

                    self.RpcSetPedalsValue(self.characterBody.master.seekerChakraGate);
                    return;
                }
            }
        }

        private float Meditate_CalculateBlastEffectScale(On.EntityStates.Seeker.Meditate.orig_CalculateBlastEffectScale orig, EntityStates.Seeker.Meditate self, int chakraAmount)
        {
            if (self.outer.commonComponents.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef == myDef)
            {
                Mathf.Clamp(chakraAmount, 1, 13);
                if (chakraAmount <= 0)
                {
                    chakraAmount = 1;
                }

                float fxScale = (1.0f + (self.blastRadiusScaling * (chakraAmount - 1)));

                return fxScale;
            }

            return orig(self, chakraAmount);
        }

        internal override void UnloadBehavior()
        {
            // On.EntityStates.Seeker.Meditate.OnEnter -= Meditate_OnEnter;

            On.EntityStates.Seeker.Meditate.CalculateBlastEffectScale -= Meditate_CalculateBlastEffectScale;
            On.RoR2.SeekerController.Start -= SeekerController_Start;
            On.RoR2.SeekerController.CmdIncrementChakraGate -= SeekerController_CmdIncrementChakraGate;
        }
    }
}
