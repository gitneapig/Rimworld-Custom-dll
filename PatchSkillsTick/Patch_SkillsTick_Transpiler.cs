using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

namespace PatchSkillsTick
{
    public static class PatchSkillsTick_Transpiler
    {
        private static bool FirstPatchApplied = false;

        [HarmonyPatch(typeof(Pawn_SkillTracker))]
        [HarmonyPatch("SkillsTick")]
        public static class Patch_SkillsTick_Transpiler
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);

                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldc_I4 && (int)codes[i].operand == 200 &&
                        codes[i + 1].opcode == OpCodes.Call &&
                        codes[i + 2].opcode == OpCodes.Brfalse &&
                        codes[i + 6].opcode == OpCodes.Brtrue_S)
                    {
                        codes[i].operand = 10000;
                        codes.Insert(i + 6, new CodeInstruction(OpCodes.Ldc_I4, 5));
                        codes[i + 7].opcode = OpCodes.Bge_S;
                        FirstPatchApplied = true;
                        return codes;
                    }
                }
                Log.Error("[Patch Skills Tick] was not applied. There is an incompatible mod.\n");
                return codes;
            }
        }

        [HarmonyPatch(typeof(SkillRecord))]
        [HarmonyPatch("Interval")]
        public static class Patch_Interval_Transpiler
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                if (!FirstPatchApplied)
                {
                    return instructions;
                }

                var codes = new List<CodeInstruction>(instructions);

                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldc_R4 && (float)codes[i].operand == 1f &&
                        codes[i + 2].opcode == OpCodes.Ldc_R4 && (float)codes[i + 2].operand == 0.5f &&
                        codes[i - 1].opcode == OpCodes.Brtrue_S &&
                        codes[i - 2].opcode == OpCodes.Callvirt)
                    {
                        codes[i].operand = 50f;
                        codes[i + 2].operand = 25f;
                        return codes;
                    }
                }
                Log.Warning("[Patch Skills Tick] XP Decay Cycle was applied, but Decay amount was not applied. Another mod is already handling. This will not affect the progress of the game, but the balance may be disrupted.\n");
                return codes;
            }
        }
    }
}
