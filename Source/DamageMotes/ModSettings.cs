using UnityEngine;
using Verse;

namespace DamageMotes
{
    public class DMModSettings : ModSettings
    {
        public bool EnableIndicatorNeutralFaction;
        public bool DisplayPawnsOnly;
        public bool DisplayPawnsInstigatorOnly;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref EnableIndicatorNeutralFaction, "EnableIndicatorNeutralFaction", false, true);
            Scribe_Values.Look(ref DisplayPawnsOnly, "DisplayPawnsOnly", false, true);
            Scribe_Values.Look(ref DisplayPawnsInstigatorOnly, "DisplayPawnsInstigatorOnly", true, true);
        }

        public void DoWindowContents(Rect inRect)
        {
            var list = new Listing_Standard()
            {
                ColumnWidth = inRect.width
            };
            list.Begin(inRect);
            list.CheckboxLabeled("EnableIndicatorNeutralFactions".Translate(), ref EnableIndicatorNeutralFaction, "EnableIndicatorNeutralFactions_Desc".Translate());
            list.CheckboxLabeled("DisplayPawnsOnly".Translate(), ref DisplayPawnsOnly, "DisplayPawnsOnly_Desc".Translate());
            list.CheckboxLabeled("DisplayPawnsInstigatorOnly".Translate(), ref DisplayPawnsInstigatorOnly, "DisplayPawnsInstigatorOnly_Desc".Translate());
            list.End();
        }

        public bool ShouldDisplayDamageAccordingToSettings(Thing target, Thing instigator)
        {
            if (!EnableIndicatorNeutralFaction && (target.Faction == null || (instigator != null && instigator.Faction == null)))
                return false;
            if (DisplayPawnsOnly && !(target is Pawn))
                return false;
            if (DisplayPawnsInstigatorOnly && (instigator == null || !(instigator is Pawn)))
                return false;
            return true;
        }
    }
    public class DMMod : Mod
    {
        public DMModSettings settings = new DMModSettings();

        public DMMod(ModContentPack content) : base(content)
        {
            Pack = content;
            settings = GetSettings<DMModSettings>();
        }

        public ModContentPack Pack { get; }

        public override string SettingsCategory() => Pack.Name;

        public override void DoSettingsWindowContents(Rect inRect) => settings.DoWindowContents(inRect);
    }
}
