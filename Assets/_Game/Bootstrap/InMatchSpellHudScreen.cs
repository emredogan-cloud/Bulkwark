// BULWARK — IN-MATCH SPELL HUD (UI Construction Bible · 09). Presentation-only, REMOVABLE.
//
// Forensic build of design/InMatchSpellHudDesign.png per Sections A–O: the Bible-08 edge chrome (re-used via
// InMatchChrome at this mockup's values — gold 150, supply 2/8, HP 10,000/10,000 & 8,750/10,000, army 15/50,
// unit costs 40/60/50/80/120, GARRISON/DEFEND/ATTACK) PLUS the spell layer — a 3-slot arcane SpellRow
// (Lightning 12s / Heal 5s / Arrows 8s), a large gold-ringed CommanderOrb (ultimate), and a transient blue
// concentric targeting telegraph over the battlefield. This is a presentation/validation screen with the mock's
// static values; live spell casting is gameplay reconciled with the real spell system outside the §12 UI
// boundary (Section N). NO ECS/gameplay (§12).

using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-09 in-match spell HUD (presentation/validation; static stub values). Presentation-only.</summary>
    public sealed class InMatchSpellHudScreen : UiScreen
    {
        protected override void Build()
        {
            UiWidgets.Stretch("KeyArt_Base", Rect, UiTheme.Obsidian, "bg_battle");

            // Targeting telegraph (battlefield space, transient — shown here in the aiming state from the mock).
            BuildTelegraph();

            // Shared 08 chrome at this mockup's values.
            InMatchChrome.Build(Rect, SafeContent, () => Router.Pop(), 150, "2/8", "10,000 / 10,000", "8,750 / 10,000", "15/50", new[] { 40, 60, 50, 80, 120 });

            // Spell row (3 arcane tiles), bottom band left of the commander orb.
            SpellTile("LIGHTNING", "12s", Hex("#4aa8ff"), 0);
            SpellTile("HEAL", "5s", Hex("#46d06a"), 1);
            SpellTile("ARROWS", "8s", Hex("#ffae4a"), 2);

            // Commander orb (ultimate) bottom-right, largest bottom element.
            BuildCommanderOrb();

            // A back affordance for this standalone presentation screen (the live HUD exits via match flow).
            UiWidgets.BackButton(SafeContent, () => Router.Pop());
        }

        private void BuildTelegraph()
        {
            var tg = UiWidgets.Rect("TargetTelegraph", Rect, new Vector2(0.42f, 0.52f), new Vector2(0.42f, 0.52f), Vector2.zero, new Vector2(454, 454));
            UiWidgets.Glow(tg, UiTheme.A(Hex("#3f8bff"), 0.4f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(640, 640));
            Ring(tg, 454, Hex("#3f8bff"));
            Ring(tg, 282, Hex("#5fa8ff"));
            Ring(tg, 154, Hex("#bcd9ff"));
            tg.gameObject.AddComponent<Spin>().degPerSec = 20f;
            // compass cross
            var cx = UiWidgets.Rect("CompassH", tg, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(282, 4)); cx.gameObject.AddComponent<Image>().color = UiTheme.A(Hex("#5fa8ff"), 0.7f);
            var cy = UiWidgets.Rect("CompassV", tg, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(4, 282)); cy.gameObject.AddComponent<Image>().color = UiTheme.A(Hex("#5fa8ff"), 0.7f);
        }

        private void Ring(Transform parent, float d, Color col)
        {
            var rt = UiWidgets.Rect("Ring", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(d, d));
            var img = rt.gameObject.AddComponent<Image>(); img.raycastTarget = false; img.sprite = UiTex.Frame(col, col, UiTheme.A(col, 0.4f), 64, 5); img.type = Image.Type.Sliced;
        }

        private void SpellTile(string label, string cd, Color glow, int index)
        {
            var rt = UiWidgets.Rect("SpellBtn_" + label, SafeContent, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-560 + index * 182, 90 + 240), new Vector2(168, 117));
            var img = rt.gameObject.AddComponent<Image>(); img.sprite = UiTex.VGradient(UiWidgets.Lighten(glow, 0.1f), Hex("#0b0d16"), 32);
            var btn = rt.gameObject.AddComponent<Button>(); btn.targetGraphic = img; btn.onClick.AddListener(() => AudioManager.Instance?.Click());
            UiWidgets.Glow(rt, UiTheme.A(glow, 0.35f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(180, 130));
            var fr = UiWidgets.Rect("ArcaneFrame", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero); var fi = fr.gameObject.AddComponent<Image>(); fi.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 6); fi.type = Image.Type.Sliced; fi.raycastTarget = false;
            var gly = UiWidgets.Rect("Icon", rt, new Vector2(0.5f, 0.58f), new Vector2(0.5f, 0.58f), Vector2.zero, new Vector2(64, 64)); var gi = gly.gameObject.AddComponent<Image>(); gi.raycastTarget = false; gi.sprite = UiTex.Diamond(UiWidgets.Lighten(glow, 0.2f), 48);
            UiWidgets.Label(rt, cd, 22, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 14), new Vector2(120, 30), TextAnchor.MiddleCenter, glow);
        }

        private void BuildCommanderOrb()
        {
            var orb = UiWidgets.Rect("CommanderOrb", SafeContent, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-110, 90 + 240), new Vector2(178, 178));
            UiWidgets.Glow(orb, UiTheme.A(UiTheme.GoldHi, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(240, 240));
            var img = orb.gameObject.AddComponent<Image>(); img.sprite = UiTex.Disc(UiWidgets.Darken(UiTheme.IronBlue, 0.2f), 64);
            var btn = orb.gameObject.AddComponent<Button>(); btn.targetGraphic = img; btn.onClick.AddListener(() => AudioManager.Instance?.Click());
            var ring = UiWidgets.Rect("Orb_Ring", orb, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero); var ri = ring.gameObject.AddComponent<Image>(); ri.raycastTarget = false; ri.sprite = UiTex.Frame(Hex("#f6dd86"), UiTheme.Gold, Hex("#574114"), 48, 12); ri.type = Image.Type.Sliced;
            var port = UiWidgets.Rect("Orb_Portrait", orb, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(120, 120)); var pi = port.gameObject.AddComponent<Image>(); pi.raycastTarget = false; pi.sprite = UiTex.Diamond(UiTheme.GoldHi, 48);
            var glow = ring.gameObject.AddComponent<PulseScale>(); glow.min = 0.99f; glow.max = 1.02f;
        }

        public override void OnShow() => AudioManager.Instance?.PlayBattleMusic();
        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}
