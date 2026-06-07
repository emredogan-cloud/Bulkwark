// BULWARK — MODE SELECT (UI Construction Bible · 06). Presentation-only, REMOVABLE.
//
// Forensic build of design/ModScreenDesign.png per Sections A–O: a row of FIVE equal ornate gold-framed portrait
// cards centred over a soft-focus dusk battlefield, each with a themed art tone, a colour-coded gem name plate at
// its foot, and per-card themed FX — CLASSIC (green plate / parchment), MISSIONS (blue / steel), TOURNAMENT (red /
// fire), ENDLESS (purple / green-undead), MULTIPLAYER (gold-orange / blue-vs-red duel) — plus a gold back button
// top-left. Cards "deal in" left→right. Battle modes hand off through the existing MatchPresentation seam (no
// gameplay change); modes whose destination screen isn't built yet toast "coming soon" (wired in finalization).
// Placeholder champion art is flagged (00/01). NO ECS/gameplay/balance change (§12).

using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-06 landscape five-card mode select. Presentation-only.</summary>
    public sealed class ModeSelectScreen : UiScreen
    {
        private struct ModeCard { public string Name; public Color Plate; public Color ArtA, ArtB; public string Theme; public System.Action Go; }

        protected override void Build()
        {
            // ---- BG (soft-focus dusk) ----
            UiWidgets.Stretch("KeyArt_Base", Rect, UiTheme.Obsidian, "bg_battle");
            UiWidgets.Stretch("Dusk", Rect, UiTheme.A(new Color(0.30f, 0.20f, 0.26f), 0.45f)); // warm grey-violet wash
            UiWidgets.Glow(Rect, UiTheme.A(UiTheme.Ember, 0.18f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), Vector2.zero, new Vector2(1800, 900), 1.6f);
            UiWidgets.Vignette(Rect, 0.55f);

            // ---- Back button (top-left, ornate gold square) ----
            BackSquare();

            var modes = new[]
            {
                new ModeCard{ Name="CLASSIC",     Plate=Hex("#2d4211"), ArtA=UiTheme.Parchment, ArtB=Hex("#8a7440"), Theme="dust", Go=() => MatchPresentation.StartMatch("Classic") },
                new ModeCard{ Name="MISSIONS",    Plate=Hex("#0e3863"), ArtA=UiTheme.IronBlue,  ArtB=UiTheme.IronSteel, Theme="steel", Go=() => Router.Show<CampaignMapScreen>() },
                new ModeCard{ Name="TOURNAMENT",  Plate=Hex("#5d0f09"), ArtA=UiTheme.Ember2,    ArtB=UiTheme.Oxblood, Theme="fire", Go=() => Router.Show<TournamentLadderScreen>() },
                new ModeCard{ Name="ENDLESS",     Plate=Hex("#443053"), ArtA=Hex("#5f9841"),    ArtB=Hex("#2d4211"), Theme="undead", Go=() => MatchPresentation.StartMatch("Endless") },
                new ModeCard{ Name="MULTIPLAYER", Plate=Hex("#864f02"), ArtA=UiTheme.IronBlue,  ArtB=UiTheme.Ember2, Theme="duel", Go=() => Router.Show<OnlineBattleScreen>() },
            };

            float[] cx = { 0.18f, 0.345f, 0.51f, 0.655f, 0.82f };
            for (int i = 0; i < modes.Length; i++) BuildCard(modes[i], cx[i], i);
        }

        private void BuildCard(ModeCard m, float fx, int index)
        {
            const float cardW = 316f, cardH = 663f;
            // Card root = Button (whole card clickable).
            var card = UiWidgets.Rect("Card_" + m.Name, SafeContent, new Vector2(fx, 0.49f), new Vector2(fx, 0.49f), Vector2.zero, new Vector2(cardW, cardH));
            var hit = card.gameObject.AddComponent<Image>(); hit.color = new Color(0, 0, 0, 0.001f); // raycast surface
            var btn = card.gameObject.AddComponent<Button>(); btn.targetGraphic = hit;
            var cb = btn.colors; cb.highlightedColor = new Color(1.06f, 1.06f, 1.06f, 1f); cb.pressedColor = new Color(0.9f, 0.9f, 0.95f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
            var go = m.Go; btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); go?.Invoke(); });
            card.gameObject.AddComponent<PulseScale>().period = 3f + index * 0.4f; // gentle phase-offset float

            // Themed art (clipped inside frame) + per-card FX glow.
            var field = UiWidgets.OrnateFrame(card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(cardW, cardH), UiTheme.FieldDark, false, 18f);
            var art = UiWidgets.Rect("Art", field, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var artImg = art.gameObject.AddComponent<Image>(); artImg.raycastTarget = false;
            artImg.sprite = m.Theme == "duel" ? UiTex.HGradient(m.ArtA, m.ArtB, 64) : UiTex.VGradient(m.ArtA, m.ArtB, 64);
            UiWidgets.Glow(field, UiTheme.A(m.ArtA, 0.35f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), Vector2.zero, new Vector2(cardW, cardH * 0.6f), 1.6f);
            if (m.Theme == "fire") { var ef = UiWidgets.Rect("Flames", field, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 180), new Vector2(cardW, 360)); ef.gameObject.AddComponent<EmberField>().count = 10; }
            if (m.Theme == "undead") { var ef = UiWidgets.Rect("Mist", field, new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), Vector2.zero, new Vector2(cardW, 300)); var e = ef.gameObject.AddComponent<EmberField>(); e.count = 8; e.color = Hex("#7fdc6a"); }

            // Foot name plate (colour-coded gem banner over bottom ~25%).
            var plate = UiWidgets.Rect("NamePlate", card, new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f), Vector2.zero, new Vector2(cardW + 18, 130));
            var pimg = plate.gameObject.AddComponent<Image>(); pimg.raycastTarget = false; pimg.sprite = UiTex.VGradient(UiWidgets.Lighten(m.Plate, 0.18f), UiWidgets.Darken(m.Plate, 0.2f), 32);
            var prim = UiWidgets.Rect("PlateRim", plate, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero); var primg = prim.gameObject.AddComponent<Image>(); primg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 6); primg.type = Image.Type.Sliced; primg.raycastTarget = false;
            // Uniform cream-gold UPPERCASE label (same size/treatment for all five; auto-fit long names).
            int fs = m.Name.Length > 9 ? 30 : 34;
            var lbl = UiWidgets.Label(plate, m.Name, fs, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-12, -40), TextAnchor.MiddleCenter, Hex("#f0e0bb"));
            lbl.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(Hex("#1a0f06"), 0.95f);
        }

        private void BackSquare()
        {
            var rt = UiWidgets.Rect("BackButton", SafeContent, new Vector2(0.052f, 0.908f), new Vector2(0.052f, 0.908f), Vector2.zero, new Vector2(94, 94));
            var img = rt.gameObject.AddComponent<Image>(); img.sprite = UiTex.VGradient(UiTheme.Charcoal, UiTheme.Obsidian, 32);
            var btn = rt.gameObject.AddComponent<Button>(); btn.targetGraphic = img;
            var cb = btn.colors; cb.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); Router.Pop(); });
            var rim = UiWidgets.Rect("Rim", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero); var rimg = rim.gameObject.AddComponent<Image>(); rimg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 8); rimg.type = Image.Type.Sliced; rimg.raycastTarget = false;
            UiWidgets.Label(rt, "<", 56, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, UiTheme.GoldHi);
        }

        public override void OnShow() => AudioManager.Instance?.PlayMenuMusic();

        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}
