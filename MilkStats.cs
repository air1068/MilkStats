using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MilkStats {
    internal static class ModInfo {
        internal const string Guid = "air1068.elin.milkstats";
        internal const string Name = "MilkStats";
        internal const string Version = "0.1.1";
    }

    [BepInPlugin(ModInfo.Guid, ModInfo.Name, ModInfo.Version)]
    public class MilkStats : BaseUnityPlugin {
        private void Awake() {
            var harmony = new Harmony(ModInfo.Guid);
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(Thing), nameof(Thing.WriteNote))]
    class Thing_WriteNote_Patch {
        public const int SKILL_TAMING = 237;

        static string FetchStats(Queue<string> s, int count) {
            string r = "";
            for (int i = 1; i < count && s.Count > 1; i++) {
                r += s.Dequeue() + ", ";
            }
            r += s.Dequeue();
            return r;
        }

        static void Postfix(Thing __instance, UINote n) {
            if (__instance.trait is TraitDrinkMilkMother && !__instance.trait.owner.c_idRefCard.IsNull()) {
                int tmp_uidNext = EClass.game.cards.uidNext;
                EClass.game.cards.uidNext = 1;
                Rand.SetSeed(1);
                Chara c = CharaGen.Create(__instance.trait.owner.c_idRefCard);
                c.SetLv(Mathf.Clamp(5 + __instance.trait.owner.encLV * 5, 1, 20 + EClass.pc.Evalue(SKILL_TAMING)));
                Rand.SetSeed();
                EClass.game.cards.uidNext = tmp_uidNext;

                Queue<string> s = new Queue<string>();
                foreach (Element attribute in c.elements.ListBestAttributes()) {
                    if ((attribute.ValueWithoutLink / 2) > 0) {
                        s.Enqueue(attribute.Name + " " + (attribute.ValueWithoutLink / 2).ToString());
                    }
                }
                foreach (Element skill in c.elements.ListBestSkills()) {
                    if ((skill.ValueWithoutLink / 2) > 0) {
                        s.Enqueue(skill.Name + " " + (skill.ValueWithoutLink / 2).ToString());
                    }
                }

                n.AddText("Milk bonuses: ", FontColor.DontChange);
                while (s.Count > 0) {
                    n.AddText(FetchStats(s, 7), FontColor.DontChange);
                }
            }
        }
    }
}
