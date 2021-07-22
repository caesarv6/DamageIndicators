using Verse;
using UnityEngine;
using RimWorld;
using HarmonyLib;

namespace DamageMotes
{
    [StaticConstructorOnStartup, HarmonyPatch(typeof(Thing), nameof(Thing.TakeDamage))]
    public static class DamageMotes_Patch
    {
        static DamageMotes_Patch()
        {
            var harmony = new Harmony("com.CaesarV6.DamageMotes.Patch");
            harmony.PatchAll();
            Log.Message("Damage Indicators initialized.");
        }
        public static void Postfix(DamageInfo dinfo, Thing __instance, DamageWorker.DamageResult __result)
        {
            float damage = __result.totalDamageDealt;
            if (damage > 0.01f && __instance.Map != null && __instance.ShouldDisplayDamage(dinfo.Instigator))
                ThrowDamageMote(damage, __instance.Map, __instance.DrawPos, damage.ToString("F0"));
        }
        public static void ThrowDamageMote(float damage, Map map, Vector3 loc, string text)
        {
            Color color = Color.white;
            //Determine colour
            if (damage >= 90f)
                color = Color.cyan;
            else if (damage >= 70f)
                color = Color.magenta;
            else if (damage >= 50f)
                color = Color.red;
            else if (damage >= 30f)
                color = Color.Lerp(Color.red, Color.yellow, 0.5f);//orange
            else if (damage >= 10f)
                color = Color.yellow;

            MoteMaker.ThrowText(loc, map, text, color, 3.65f);
        }
    }

    [HarmonyPatch(typeof(ShieldBelt), nameof(ShieldBelt.CheckPreAbsorbDamage))]
    public static class ShieldBelt_Patch
    {
        static void Postfix(DamageInfo dinfo, bool __result, ShieldBelt __instance)
        {
            if (__result && __instance.Wearer != null && __instance.Wearer.Map != null)
            {
                if (dinfo.Def != DamageDefOf.EMP)
                {
                    var amount = dinfo.Amount * Traverse.Create(__instance).Field("EnergyLossPerDamage").GetValue<float>() * 100;
                    MoteMaker.ThrowText(__instance.Wearer.DrawPos, __instance.Wearer.Map, ShieldBeltOutputString(__instance, amount), 3.65f);
                }
                if (__instance.ShieldState == ShieldState.Resetting)
                {
                    MoteMaker.ThrowText(__instance.Wearer.DrawPos, __instance.Wearer.Map, "PERSONALSHIELD_BROKEN".Translate(), 3.65f);
                }
            }
        }
        public static string ShieldBeltOutputString(Thing __instance, float amount)
        {
            return "(- " + amount.ToString("F0") + "/ " + (__instance.GetStatValue(StatDefOf.EnergyShieldEnergyMax, true) * 100) + ")";
        }
    }

    [HarmonyPatch(typeof(Verb_MeleeAttack), "SoundMiss")]
    public static class Verb_MeleeAttack_Patch
    {
        static void Postfix(Verb_MeleeAttack __instance)
        {
            LocalTargetInfo currentTarget = Traverse.Create(__instance).Field("currentTarget").GetValue<LocalTargetInfo>();
            if (currentTarget != null && currentTarget.HasThing)
            {
                Thing t = currentTarget.Thing;
                MoteMaker.ThrowText(t.DrawPos, t.Map, "DM_MISS".Translate(), 3.65f);
            }
        }
    }

    static class DamageMotesUtil
    {
        /// <summary>
        /// Used on both the instigator and the target.
        /// </summary>
        public static bool ShouldDisplayDamage(this Thing target, Thing instigator)
        {
            return LoadedModManager.GetMod<DMMod>().settings.ShouldDisplayDamageAccordingToSettings(target, instigator);
        }
    }
}
