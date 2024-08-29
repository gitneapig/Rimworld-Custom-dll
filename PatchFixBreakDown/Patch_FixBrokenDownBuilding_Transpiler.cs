using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace PatchFixBreakDown
{
    [HarmonyPatch(typeof(WorkGiver_FixBrokenDownBuilding))]
    [HarmonyPatch("HasJobOnThing")]
    public static class Patch_FixBrokenDownBuilding_Transpiler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo mi && mi.Name == "get_Item" &&
                    codes[i + 1].opcode == OpCodes.Brtrue_S &&
                    codes[i + 7].opcode == OpCodes.Ldarg_1 &&
                    codes[i + 8].opcode == OpCodes.Ldloc_0 &&
                    codes[i + 9].opcode == OpCodes.Call && codes[i + 9].operand is MethodInfo mi2 && mi2.Name == "op_Implicit")
                {
                    var labelnotforced = (Label)codes[i + 1].operand;
                    var newInstructions = new List<CodeInstruction>

                    {
                        new CodeInstruction(OpCodes.Ldarg_3),
                        new CodeInstruction(OpCodes.Brtrue_S, labelnotforced)
                    };
                    codes.InsertRange(i + 2, newInstructions);
                    return codes;
                }
            }
            Log.Error("[Patch FixBreakDown] was not applied. There is an incompatible mod.\n");
            return codes;
        }
    }
}
