using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using static PulsarModLoader.Patches.HarmonyHelpers;

namespace BetterScrapperTurrets
{
    [HarmonyPatch(typeof(PLShipInfoBase), "TakeDamage")] //Patch: Removes time between scrap drops, modifies drop chance based on damage
    internal class DamagePatch
    {
        static bool PatchMethod(float IncomingDamage)
        {
            //chance is: (dmg ÷ 18) - 1 / 40.  (17 dmg: 0/40 chance, 18 dmg: 1/40 chance, 36 dmg: 2/40 chance)
            return UnityEngine.Random.Range(0, 40) < ((int)IncomingDamage / 18);
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> targetSequence = new List<CodeInstruction>() //target time check, chance check.
            {
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(UnityEngine.Time), "get_time")),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PLShipInfoBase), "LastServerDroppedScrapperScrap")),
                new CodeInstruction(OpCodes.Sub),
                new CodeInstruction(OpCodes.Ldc_R4),
                new CodeInstruction(OpCodes.Ble_Un_S),
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Ldc_I4_S),
                new CodeInstruction(OpCodes.Call),
                new CodeInstruction(OpCodes.Ldc_I4_3),
                new CodeInstruction(OpCodes.Bne_Un_S)
            };

            int targetIndex = FindSequence(instructions, targetSequence, CheckMode.NONNULL);
            Label targetlabel = (Label)instructions.ToArray()[targetIndex - 1].operand;

            List<CodeInstruction> injectedSequence = new List<CodeInstruction>() //insert patch method
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DamagePatch), "PatchMethod")),
                new CodeInstruction(OpCodes.Brfalse_S, targetlabel)
            };
            return PatchBySequence(instructions, targetSequence, injectedSequence, PatchMode.REPLACE, CheckMode.NONNULL);
        }
    }
}
