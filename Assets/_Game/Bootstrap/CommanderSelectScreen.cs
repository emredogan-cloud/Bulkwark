// BULWARK — COMMANDER SELECT (UI Construction Bible · 23). Presentation-only, REMOVABLE.
//
// Forensic build of design/CommanderSelectDesign.png per Bible §23 A–O: a symmetric two-commander face-off —
// LEFT Iron Pact WARDEN (cobalt) vs RIGHT Ashen Horde WARCHIEF (ember) over a cool/warm split background with a
// luminous central seam and a cast-gold VS badge; each ornate-framed panel carries faction crest + names/epithet,
// a full-body commander field, ACTIVE + PASSIVE ability cards, a Commander-Level row with XP bar, and a
// faction-coloured SELECT CTA. Runs the §I enter ceremony (washes → header → panels slide in → VS pop → cards
// → XP fill → SELECT glow) and the §H/§I select-confirm (chosen → "SELECTED" + check, other → idle).
// §12 boundary: NO ECS, NO Unity.Entities, NO gameplay/balance/AI/economy. Commander data is read-only from
// UiStub (display-only, replaced by server-auth CommanderDef/ProgressionService binding at GATE-3); SELECT is a
// presentation-only choice write (UiStub.SelectedCommander) then Router.Pop() — never a local state mutation.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-23 landscape two-panel commander chooser. Presentation-only (display-only data, §12).</summary>
    public sealed class CommanderSelectScreen : UiScreen
    {
        // ---- Canvas geometry (CanvasScaler ref 2340×1080, matchHeight=1.0). Bible §23-E gives positions as
        // fractions of 2340×1080; its Section-E y values are Unity-native (bottom-origin: 1.0 = top, header at
        // y 0.90→1.00 = top, panels y 0.060→0.890), so anchorY = y directly. px = frac × 2340 (x) / × 1080 (y). ----
        private const float W = 2340f, H = 1080f;

        // Panel inner-content metrics (Bible §23-E "Within a panel", Left reference; Right mirrors horizontally).
        // Panel region: x 0.018→0.475 (≈0.457w), y 0.060→0.890. Inner side = toward the centre VS.
        private const float PanelW = 0.457f * W; // ≈1069 px @1080

        // ---- Forensic hexes NOT in UiTheme (Bible §23 B/F/G). UiTheme covers the canonical gold/cobalt/ember. ----
        private static readonly Color BgBase     = Hex("#0a0b0f"); // near-black hall
        private static readonly Color CoolWash   = Hex("#0c1430"); // left half cooled cobalt
        private static readonly Color EmberWash  = Hex("#2a0f0c"); // right half warmed ember
        private static readonly Color SeamCool   = Hex("#4f8bff"); // seam light shaft (left side)
        private static readonly Color SeamWarm   = Hex("#f0742c"); // seam embers (right side)
        private static readonly Color PanelField = Hex("#0c0e16"); // panel interior field
        private static readonly Color IPGlow     = Hex("#4f8bff"); // Iron Pact inner frame glow / rim
        private static readonly Color AshGlow    = Hex("#f0742c"); // Ashen inner frame glow / rim
        private static readonly Color IPName     = Hex("#9fc0ff"); // "IRON PACT" faction text
        private static readonly Color AshName    = Hex("#f0a070"); // "ASHEN HORDE" faction text
        private static readonly Color Epithet    = Hex("#cbb98a"); // commander epithet italic-stand-in
        private static readonly Color Kicker     = Hex("#caa04a"); // "ACTIVE"/"PASSIVE" kicker gold
        private static readonly Color AbilityName = Hex("#f0d27a"); // ability name gold
        private static readonly Color AbilityDesc = Hex("#cdd2da"); // ability description
        private static readonly Color Cooldown   = Hex("#b9c0c8"); // "60s Cooldown"
        private static readonly Color LevelTag   = Hex("#9a8a5a"); // "COMMANDER LEVEL"
        private static readonly Color LevelNum   = Hex("#fff4d6"); // level numeral
        private static readonly Color XpText     = Hex("#f0e8cf"); // XP "x / x,x00 XP"
        private static readonly Color CardWell   = Hex("#12141b"); // ability-card dark slate well
        private static readonly Color XpGroove   = Hex("#1b1d24"); // XP bar dark groove
        private static readonly Color IPSelectHi = Hex("#2b56c8"); // cobalt SELECT body
        private static readonly Color AshSelHi   = Hex("#b5311f"); // ember SELECT body (top of oxblood→ember)
        private static readonly Color Subtitle   = Hex("#c9b27a"); // header subtitle
        private static readonly Color CurrencyV  = Hex("#f4e9c8"); // currency values
        private static readonly Color SilverIcon = Hex("#c2c8d0"); // silver-coin chip swatch

        // ---- Currency values (Bible §23-L drawn strings; only TWO chips — Gold + Silver, NO Gems). Display-only. ----
        private const int GoldValue = 12450; // "12,450"
        private const int SilverValue = 1280; // "1,280"

        // ---- XP fractions (Bible §23-L/G). Warchief = 1,450/4,000 (clear). Warden numerator obscured → near-full. ----
        private const int WardenXpCur = 8400, WardenXpMax = 9000;       // §23-N: bar reads near-full (~0.93)
        private const int WarchiefXpCur = 1450, WarchiefXpMax = 4000;   // §23-L: explicit 1,450 / 4,000

        // ---- Ceremony handles (driven by Reveal / snapped by skip). ----
        private CanvasGroup _headerCg, _vsCg;
        private RectTransform _leftPanel, _rightPanel; private CanvasGroup _leftCg, _rightCg;
        private RectTransform _vsBadge;
        private Side _left, _right;
        private bool _revealed;

        // Per-panel mutable handles for the enter ceremony + select-confirm.
        private struct Side
        {
            public bool Right;
            public Color Faction, Rim, Glow;
            public CanvasGroup CardActiveCg, CardPassiveCg, SelectCg;
            public Image XpFill; public float XpAmount;
            public Button Select; public Text SelectLabel; public RectTransform SelectCheck;
            public Image FrameGlow; public string Name;
        }

        protected override void Build()
        {
            BuildBackground();

            // ---- Header (Bible §23-E: y 0.90→1.00) ----
            var header = UiWidgets.Rect("Header", SafeContent, new Vector2(0f, 0.90f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            header.anchorMin = new Vector2(0f, 0.90f); header.anchorMax = new Vector2(1f, 1f); header.offsetMin = Vector2.zero; header.offsetMax = Vector2.zero;
            _headerCg = header.gameObject.AddComponent<CanvasGroup>();

            // Btn_Back top-left (Bible §23-E x≈0.030, y≈0.955 → top-left chevron via the shared builder).
            UiWidgets.BackButton(SafeContent, () => Router.Pop());

            // Lbl_Title "SELECT COMMANDER" — serif gold display, wide tracking, centred x0.50 / y0.955 (§23-F 59px).
            UiWidgets.TitleLabel(header, "SELECT COMMANDER", 59,
                new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), Vector2.zero, new Vector2(0.6f * W, 90f),
                TextAnchor.MiddleCenter, UiTheme.GoldHi, UiTheme.Gold);

            // Lbl_Subtitle — clean light sans, centred, y≈0.905 (§23-F 22px). Drawn supporting copy.
            UiWidgets.Label(header, "Choose your commander. Each leads an army with unique abilities.", 22,
                new Vector2(0.5f, 0.12f), new Vector2(0.5f, 0.12f), Vector2.zero, new Vector2(0.7f * W, 40f),
                TextAnchor.MiddleCenter, Subtitle);

            // CurrencyChips top-right — ONLY two (Gold rightmost idx0, Silver idx1). §23-L: NO Gems.
            var goldVal = UiWidgets.CurrencyChip(SafeContent, UiTheme.Gold, GoldValue, 0, out _);
            goldVal.color = CurrencyV;
            var silverVal = UiWidgets.CurrencyChip(SafeContent, SilverIcon, SilverValue, 1, out _);
            silverVal.color = CurrencyV;

            // ---- Two mirrored commander panels (Bible §23-C/D/E). Left = Iron Pact WARDEN, Right = Ashen WARCHIEF. ----
            _left = BuildPanel(
                right: false, faction: "IRON PACT", factionCol: IPName, rim: IPGlow, glow: IPGlow,
                name: UiStub.WardenName, epithet: UiStub.WardenTitle,
                active: UiStub.WardenActive, activeDesc: UiStub.WardenActiveDesc,
                passive: UiStub.WardenPassive, passiveDesc: UiStub.WardenPassiveDesc,
                level: UiStub.WardenLevel, xpCur: WardenXpCur, xpMax: WardenXpMax,
                selectTop: UiTheme.IronBlueHi, selectBody: IPSelectHi,
                out _leftPanel, out _leftCg);

            _right = BuildPanel(
                right: true, faction: "ASHEN HORDE", factionCol: AshName, rim: AshGlow, glow: AshGlow,
                name: UiStub.WarchiefName, epithet: UiStub.WarchiefTitle,
                active: UiStub.WarchiefActive, activeDesc: UiStub.WarchiefActiveDesc,
                passive: UiStub.WarchiefPassive, passiveDesc: UiStub.WarchiefPassiveDesc,
                level: UiStub.WarchiefLevel, xpCur: WarchiefXpCur, xpMax: WarchiefXpMax,
                selectTop: UiTheme.Ember2, selectBody: AshSelHi,
                out _rightPanel, out _rightCg);

            // ---- VS badge (Bible §23-E: centred x0.50, y≈0.560, Ø≈0.075w; gold ring + serif "VS" over the seam). ----
            BuildVsBadge();

            // Reflect any already-committed choice (§23-H: equipped commander → "SELECTED").
            ApplyCommittedState();
        }

        // ============================ BACKGROUND (Rect — full-bleed, FX live here) ============================
        private void BuildBackground()
        {
            // BG_FullBleed near-black base.
            UiWidgets.Stretch("BG_FullBleed", Rect, BgBase);

            // BG_LeftCoolWash (left 50%, cobalt) — anchored left-stretch (Bible §23-D order 0).
            var cool = UiWidgets.Rect("BG_LeftCoolWash", Rect, Vector2.zero, new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
            cool.anchorMin = Vector2.zero; cool.anchorMax = new Vector2(0.5f, 1f); cool.offsetMin = Vector2.zero; cool.offsetMax = Vector2.zero;
            var coolImg = cool.gameObject.AddComponent<Image>(); coolImg.raycastTarget = false;
            coolImg.sprite = UiTex.HGradient(CoolWash, UiTheme.A(CoolWash, 0f), 64); // brightest at the outer edge, fading to the seam

            // BG_RightEmberWash (right 50%, ember) — anchored right-stretch (order 1).
            var hot = UiWidgets.Rect("BG_RightEmberWash", Rect, new Vector2(0.5f, 0f), Vector2.one, Vector2.zero, Vector2.zero);
            hot.anchorMin = new Vector2(0.5f, 0f); hot.anchorMax = Vector2.one; hot.offsetMin = Vector2.zero; hot.offsetMax = Vector2.zero;
            var hotImg = hot.gameObject.AddComponent<Image>(); hotImg.raycastTarget = false;
            hotImg.sprite = UiTex.HGradient(UiTheme.A(EmberWash, 0f), EmberWash, 64);

            // BG_CenterSeam — bright vertical light shaft (cool) blending to embers (warm), behind VS (order 2, §23-G/J).
            UiWidgets.Glow(Rect, UiTheme.A(SeamCool, 0.42f), new Vector2(0.485f, 0.5f), new Vector2(0.485f, 0.5f), Vector2.zero, new Vector2(360f, H), 1.4f);
            UiWidgets.Glow(Rect, UiTheme.A(SeamWarm, 0.42f), new Vector2(0.515f, 0.5f), new Vector2(0.515f, 0.5f), Vector2.zero, new Vector2(360f, H), 1.4f);
            UiWidgets.Glow(Rect, UiTheme.A(UiTheme.GoldHi, 0.20f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(640f, 900f), 1.6f);
            // Rising embers on the right of the seam (§23-J Warchief side); cool motes on the left.
            var embers = UiWidgets.Rect("SeamEmbers", Rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(60f, 360f), new Vector2(420f, 720f));
            embers.gameObject.AddComponent<EmberField>().count = 14;
            var motes = UiWidgets.Rect("SeamMotes", Rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-60f, 360f), new Vector2(420f, 720f));
            var moteFx = motes.gameObject.AddComponent<EmberField>(); moteFx.count = 10; moteFx.color = UiTheme.A(SeamCool, 0.8f);

            // BG_Vignette (multiply-ish corner darkening, order 3).
            UiWidgets.Vignette(Rect, 0.55f);
        }

        // ============================ COMMANDER PANEL ============================
        private Side BuildPanel(
            bool right, string faction, Color factionCol, Color rim, Color glow,
            string name, string epithet, string active, string activeDesc, string passive, string passiveDesc,
            int level, int xpCur, int xpMax, Color selectTop, Color selectBody,
            out RectTransform panelRt, out CanvasGroup panelCg)
        {
            var s = new Side { Right = right, Faction = factionCol, Rim = rim, Glow = glow, Name = name };

            // Panel region (Bible §23-E): Left x0.018→0.475, Right x0.525→0.982; y0.060→0.890. Anchored to its side.
            float ax = right ? 0.982f : 0.018f;       // outer anchor edge
            float cx = right ? -PanelW * 0.5f : PanelW * 0.5f; // shift so the outer edge lands on the anchor
            float cy = (0.060f + 0.890f) * 0.5f * H - H * 0.5f; // panel vertical centre offset from screen mid
            float panelH = (0.890f - 0.060f) * H;     // ≈896 px

            var panelGo = new GameObject(right ? "CommanderPanel_Right" : "CommanderPanel_Left", typeof(RectTransform), typeof(CanvasGroup));
            panelRt = (RectTransform)panelGo.transform; panelRt.SetParent(SafeContent, false);
            panelRt.anchorMin = panelRt.anchorMax = new Vector2(ax, 0.5f);
            panelRt.anchoredPosition = new Vector2(cx, cy);
            panelRt.sizeDelta = new Vector2(PanelW, panelH);
            panelCg = panelGo.GetComponent<CanvasGroup>();

            // Panel_Frame — ornate gold molding over a dark field (§23-D order 0, §23-G filigree stand-in).
            UiWidgets.OrnateFrame(panelRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(PanelW, panelH), PanelField, true, 22f);
            // Faction inner glow on the frame (left cobalt #4f8bff / right ember #f0742c, §23-G). Pulses idle (§23-I).
            s.FrameGlow = UiWidgets.Glow(panelRt, UiTheme.A(glow, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(PanelW * 0.95f, panelH * 0.95f), 1.7f);
            var fgp = s.FrameGlow.gameObject.AddComponent<PulseGraphic>(); fgp.target = s.FrameGlow; fgp.min = 0.14f; fgp.max = 0.30f; fgp.period = 2.6f;

            // Local helper: a fraction within the panel rect → anchoredPosition (panel pivot is centre).
            // fx,fy are SCREEN fractions; convert to a panel-local anchor [0..1] across the panel rect.
            float pxL = right ? 0.525f : 0.018f, pxR = right ? 0.982f : 0.475f; // panel x-extent in screen fractions

            // Art_Commander — full-body field on the OUTER ~45% of the panel (Warden left / Warchief right), may bleed
            // under the cards (§23-E x0.030→0.230 on left; mirror on right). Painterly stand-in: faction-rim gradient.
            float artAxMin = right ? 0.55f : 0.02f, artAxMax = right ? 0.98f : 0.45f; // panel-local x band for the art
            var art = PanelChild("Art_Commander", panelRt, artAxMin, 0.16f, artAxMax, 0.88f);
            var artImg = art.gameObject.AddComponent<Image>(); artImg.raycastTarget = false;
            artImg.sprite = UiTex.VGradient(UiTheme.A(UiWidgets.Lighten(glow, 0.15f), 0.55f), UiTheme.A(glow, 0.06f), 64);
            UiWidgets.Glow(panelRt, UiTheme.A(glow, 0.28f),
                new Vector2((artAxMin + artAxMax) * 0.5f, 0.55f), new Vector2((artAxMin + artAxMax) * 0.5f, 0.55f),
                Vector2.zero, new Vector2(PanelW * 0.42f, panelH * 0.6f), 1.7f);

            // Crest_Faction — top-outer corner disc + gold rim (§23-C: top-LEFT of left panel, top-RIGHT of right
            // panel; §23-E Ø≈0.045w). The names sit beside it, the ability column is on the inner side toward VS.
            float crestD = 0.045f * W;
            float crestAx = right ? 0.90f : 0.10f;
            var crest = PanelChild("Crest_Faction", panelRt, crestAx - crestD / PanelW * 0.5f, 0.86f - crestD / panelH * 0.5f, crestAx + crestD / PanelW * 0.5f, 0.86f + crestD / panelH * 0.5f);
            var crestImg = crest.gameObject.AddComponent<Image>(); crestImg.raycastTarget = false; crestImg.sprite = UiTex.Disc(UiWidgets.Darken(s.Faction, 0.35f), 64);
            var crestRim = PanelChild("CrestRim", crest, 0f, 0f, 1f, 1f); var crImg = crestRim.gameObject.AddComponent<Image>(); crImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 6); crImg.type = Image.Type.Sliced; crImg.raycastTarget = false;
            var crestGly = PanelChild("CrestGlyph", crest, 0.25f, 0.25f, 0.75f, 0.75f); var cgImg = crestGly.gameObject.AddComponent<Image>(); cgImg.raycastTarget = false; cgImg.sprite = UiTex.Diamond(UiTheme.GoldHi, 32);

            // Name block beside the crest (text on the side away from the panel's outer art, toward the inner column).
            var nameAlign = right ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
            float textAxMin = right ? 0.30f : 0.20f, textAxMax = right ? 0.80f : 0.70f; // panel-local x for name texts

            // Lbl_FactionName "IRON PACT"/"ASHEN HORDE" (§23-F 26px, faction-tinted, y≈0.870).
            var fLbl = PanelLabel(panelRt, faction, 26, textAxMin, 0.870f, textAxMax, 0.870f, nameAlign, factionCol);
            fLbl.fontStyle = FontStyle.Bold;

            // Lbl_CommanderName WARDEN/WARCHIEF (§23-F 56px serif gold display, y≈0.835).
            UiWidgets.TitleLabel(panelRt, name, 56, new Vector2(textAxMin, 0.835f), new Vector2(textAxMax, 0.835f), Vector2.zero, Vector2.zero, nameAlign, UiTheme.GoldHi, UiTheme.Gold, false);

            // Lbl_CommanderEpithet (§23-F 20px serif italic stand-in, y≈0.805).
            var ep = PanelLabel(panelRt, epithet, 20, textAxMin, 0.805f, textAxMax, 0.805f, nameAlign, Epithet);
            ep.fontStyle = FontStyle.Italic;

            // ---- AbilityCol — two stacked cards on the INNER side of the panel toward VS (§23-E Left x0.250→0.460). ----
            // Card_Active y0.620→0.790 ; Card_Passive y0.430→0.595. Inner-side x band (mirrored for right panel).
            float cardAxMin = right ? 0.04f : 0.42f, cardAxMax = right ? 0.58f : 0.96f;
            s.CardActiveCg  = BuildAbilityCard(panelRt, true,  active,  activeDesc,  cardAxMin, 0.620f, cardAxMax, 0.790f);
            s.CardPassiveCg = BuildAbilityCard(panelRt, false, passive, passiveDesc, cardAxMin, 0.430f, cardAxMax, 0.595f);

            // ---- LevelRow (§23-E y0.330→0.380, spans inner area x0.090→0.460). Tag above the bar at y≈0.385. ----
            float lvAxMin = right ? 0.04f : 0.10f, lvAxMax = right ? 0.90f : 0.96f;
            UiWidgets.Label(panelRt, UiTheme.Track("COMMANDER LEVEL"), 18, new Vector2((lvAxMin + lvAxMax) * 0.5f, 0.392f), new Vector2((lvAxMin + lvAxMax) * 0.5f, 0.392f), Vector2.zero, new Vector2(PanelW * 0.8f, 28f), TextAnchor.MiddleCenter, LevelTag);

            // Badge_Level (circle Ø≈0.045w) at the inner-most end containing the numeral.
            float badgeD = 0.045f * W;
            float badgeAx = right ? lvAxMax - (badgeD / PanelW) * 0.5f : lvAxMin + (badgeD / PanelW) * 0.5f;
            var badge = PanelChild("Badge_Level", panelRt, badgeAx - (badgeD / PanelW) * 0.5f, 0.355f - (badgeD / panelH) * 0.5f, badgeAx + (badgeD / PanelW) * 0.5f, 0.355f + (badgeD / panelH) * 0.5f);
            var badgeImg = badge.gameObject.AddComponent<Image>(); badgeImg.raycastTarget = false; badgeImg.sprite = UiTex.Disc(UiTheme.A(UiTheme.Charcoal, 0.95f), 64);
            var badgeRim = PanelChild("BadgeRim", badge, 0f, 0f, 1f, 1f); var brImg = badgeRim.gameObject.AddComponent<Image>(); brImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 6); brImg.type = Image.Type.Sliced; brImg.raycastTarget = false;
            UiWidgets.Label(badge, level.ToString(), 36, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, LevelNum);

            // XPBar to the inner-badge's far side (§23-E x≈0.175→0.460, height≈0.028h). Dark groove + faction-tinted fill.
            // Convert the §23-E screen-fraction band x0.175→0.460 (left) into panel-local across x0.018→0.475:
            float barMinScreen = right ? 0.525f : 0.175f, barMaxScreen = right ? 0.825f : 0.460f;
            float barLocMin = (barMinScreen - pxL) / (pxR - pxL);
            float barLocMax = (barMaxScreen - pxL) / (pxR - pxL);
            float barH = 0.028f * H;
            var barBg = PanelChild("XPBar", panelRt, barLocMin, 0.355f - (barH / panelH) * 0.5f, barLocMax, 0.355f + (barH / panelH) * 0.5f);
            var grooveImg = barBg.gameObject.AddComponent<Image>(); grooveImg.raycastTarget = false; grooveImg.sprite = UiTex.VGradient(UiTheme.A(XpGroove, 1f), UiTheme.ChannelMid, 32);
            var fillRt = PanelChild("Fill", barBg, 0f, 0f, 1f, 1f); fillRt.sizeDelta = new Vector2(-8f, -8f);
            s.XpFill = fillRt.gameObject.AddComponent<Image>();
            s.XpFill.sprite = UiTex.HGradient(UiWidgets.Lighten(glow, 0.25f), glow, 64);
            s.XpFill.type = Image.Type.Filled; s.XpFill.fillMethod = Image.FillMethod.Horizontal; s.XpFill.fillOrigin = 0; s.XpFill.raycastTarget = false;
            s.XpAmount = xpMax > 0 ? Mathf.Clamp01(xpCur / (float)xpMax) : 0f;
            s.XpFill.fillAmount = 0f; // filled by the enter ceremony (§23-I 0.42s); snapped on skip
            var barRim = PanelChild("XpRim", barBg, 0f, 0f, 1f, 1f); var xrImg = barRim.gameObject.AddComponent<Image>(); xrImg.sprite = UiTex.Frame(UiTheme.A(UiTheme.GoldHi, 0.7f), UiTheme.A(UiTheme.Gold, 0.6f), UiTheme.A(UiTheme.GoldShadow, 0.6f), 48, 4); xrImg.type = Image.Type.Sliced; xrImg.raycastTarget = false;
            UiWidgets.Label(barBg, xpCur.ToString("N0") + " / " + xpMax.ToString("N0") + " XP", 20, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, XpText);

            // ---- Btn_Select (§23-E y0.250→0.320, centred in inner width x≈0.150→0.430; faction enamel, white label). ----
            float selMinScreen = right ? 0.570f : 0.150f, selMaxScreen = right ? 0.850f : 0.430f;
            float selLocMin = (selMinScreen - pxL) / (pxR - pxL);
            float selLocMax = (selMaxScreen - pxL) / (pxR - pxL);
            float selCx = (selLocMin + selLocMax) * 0.5f;
            float selW = (selMaxScreen - selMinScreen) * W;
            float selH = 0.070f * H;
            var selGo = new GameObject("Btn_Select", typeof(RectTransform), typeof(CanvasGroup));
            var selRt = (RectTransform)selGo.transform; selRt.SetParent(panelRt, false);
            selRt.anchorMin = selRt.anchorMax = new Vector2(selCx, 0.285f);
            selRt.anchoredPosition = Vector2.zero; selRt.sizeDelta = new Vector2(selW, selH);
            s.SelectCg = selGo.GetComponent<CanvasGroup>();
            // Idle faction glow behind the CTA (§23-G "idle glow").
            var selGlow = UiWidgets.Glow(selRt, UiTheme.A(UiWidgets.Lighten(selectBody, 0.3f), 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(selW * 1.3f, selH * 1.7f));
            var selGp = selGlow.gameObject.AddComponent<PulseGraphic>(); selGp.target = selGlow; selGp.min = 0.3f; selGp.max = 0.6f; selGp.period = 1.9f;
            // Enamel body (vertical gradient: lighter top edge → dark bottom) + gold rim.
            var selBody = selRt.gameObject.AddComponent<Image>(); selBody.sprite = UiTex.VGradient(UiWidgets.Lighten(selectTop, 0.18f), UiWidgets.Darken(selectBody, 0.4f), 32);
            s.Select = selRt.gameObject.AddComponent<Button>(); s.Select.targetGraphic = selBody;
            var cb = s.Select.colors; cb.normalColor = Color.white; cb.highlightedColor = new Color(1.10f, 1.10f, 1.10f, 1f); cb.pressedColor = new Color(0.85f, 0.85f, 0.9f, 1f); cb.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.7f); cb.fadeDuration = 0.08f; s.Select.colors = cb;
            var selRim = PanelChild("SelectRim", selRt, 0f, 0f, 1f, 1f); var srImg = selRim.gameObject.AddComponent<Image>(); srImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 8); srImg.type = Image.Type.Sliced; srImg.raycastTarget = false;
            s.SelectLabel = UiWidgets.Label(selRt, UiTheme.Track("SELECT"), 32, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Color.white);
            s.SelectLabel.fontStyle = FontStyle.Bold;
            s.SelectLabel.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.StrokeDark, 0.9f);
            // Check-mark (hidden until selected) — §23-H "SELECTED" + check pop.
            var check = PanelChild("SelectCheck", selRt, 0.06f, 0.30f, 0.16f, 0.70f);
            var checkImg = check.gameObject.AddComponent<Image>(); checkImg.raycastTarget = false; checkImg.sprite = UiTex.Diamond(UiTheme.GoldHi, 32);
            s.SelectCheck = check; check.gameObject.SetActive(false);

            string commanderName = name; bool isRight = right;
            s.Select.onClick.AddListener(() => { AudioManager.Instance?.Click(); OnSelect(commanderName, isRight); });

            return s;
        }

        // An ABILITY card (Bible §23-E card internals + §23-F typography). Returns its CanvasGroup (enter fade/slide).
        private CanvasGroup BuildAbilityCard(RectTransform panel, bool isActive, string abilityName, string desc,
            float axMin, float ayMin, float axMax, float ayMax)
        {
            var cardGo = new GameObject(isActive ? "AbilityCard_Active" : "AbilityCard_Passive", typeof(RectTransform), typeof(CanvasGroup));
            var card = (RectTransform)cardGo.transform; card.SetParent(panel, false);
            card.anchorMin = new Vector2(axMin, ayMin); card.anchorMax = new Vector2(axMax, ayMax);
            card.offsetMin = Vector2.zero; card.offsetMax = Vector2.zero;
            var cg = cardGo.GetComponent<CanvasGroup>();

            // Dark slate well + thin gold trim + faint inner vignette (§23-G).
            var well = card.gameObject.AddComponent<Image>(); var ps = UiWidgets.Spr("panel"); if (ps != null) { well.sprite = ps; well.type = Image.Type.Sliced; } well.color = UiTheme.A(CardWell, 0.96f);
            var innerV = PanelChild("CardVignette", card, 0f, 0f, 1f, 1f); var ivImg = innerV.gameObject.AddComponent<Image>(); ivImg.raycastTarget = false; ivImg.sprite = UiTex.Radial(UiTheme.A(Color.black, 0f), UiTheme.A(Color.black, 0.35f), 128, 1.6f);
            var trim = PanelChild("CardTrim", card, 0f, 0f, 1f, 1f); var trImg = trim.gameObject.AddComponent<Image>(); trImg.sprite = UiTex.Frame(UiTheme.A(UiTheme.GoldHi, 0.75f), UiTheme.A(UiTheme.Gold, 0.6f), UiTheme.A(UiTheme.GoldShadow, 0.6f), 48, 5); trImg.type = Image.Type.Sliced; trImg.raycastTarget = false;

            // Kicker "ACTIVE"/"PASSIVE" top-left (§23-F 18px gold, +10% tracking).
            UiWidgets.Label(card, UiTheme.Track(isActive ? "ACTIVE" : "PASSIVE"), 18, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(96f, -26f), new Vector2(260f, 26f), TextAnchor.MiddleLeft, Kicker);

            // Ability icon — small recessed gold-rimmed frame at left (§23-E ≈0.055w square).
            float iconD = 0.055f * W;
            var icon = PanelChild("Icon_Ability", card, 0f, 0f, 0f, 0f);
            icon.anchorMin = icon.anchorMax = new Vector2(0f, 0.5f); icon.sizeDelta = new Vector2(iconD, iconD); icon.anchoredPosition = new Vector2(18f + iconD * 0.5f, 28f);
            var iconBg = icon.gameObject.AddComponent<Image>(); iconBg.raycastTarget = false; iconBg.sprite = UiTex.VGradient(UiTheme.FieldTop, UiTheme.FieldDark, 32);
            var iconRim = PanelChild("IconRim", icon, 0f, 0f, 1f, 1f); var irImg = iconRim.gameObject.AddComponent<Image>(); irImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 5); irImg.type = Image.Type.Sliced; irImg.raycastTarget = false;
            var iconGly = PanelChild("IconGlyph", icon, 0.28f, 0.28f, 0.72f, 0.72f); var igImg = iconGly.gameObject.AddComponent<Image>(); igImg.raycastTarget = false; igImg.sprite = UiTex.Diamond(UiTheme.A(UiTheme.GoldHi, 0.9f), 32);

            // Ability name to the right of the icon (§23-F 30px serif gold bold).
            UiWidgets.TitleLabel(card, abilityName, 30, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -30f), Vector2.zero, TextAnchor.MiddleLeft, UiTheme.GoldHi, AbilityName, false);
            var nameLbl = (RectTransform)card.GetChild(card.childCount - 1); nameLbl.offsetMin = new Vector2(40f + iconD, nameLbl.offsetMin.y); nameLbl.offsetMax = new Vector2(-20f, nameLbl.offsetMax.y);

            // Description (2–3 lines wrapped, §23-F 19px). Wrapped Text under the icon row.
            var descGo = new GameObject("Lbl_AbilityDesc", typeof(RectTransform), typeof(Text));
            var descRt = (RectTransform)descGo.transform; descRt.SetParent(card, false);
            descRt.anchorMin = new Vector2(0f, 0f); descRt.anchorMax = new Vector2(1f, 1f);
            descRt.offsetMin = new Vector2(24f, isActive ? 42f : 18f); descRt.offsetMax = new Vector2(-24f, -(iconD + 36f));
            var descT = descGo.GetComponent<Text>(); descT.font = UiWidgets.Font; descT.text = desc; descT.fontSize = 19; descT.color = AbilityDesc;
            descT.alignment = TextAnchor.UpperLeft; descT.horizontalOverflow = HorizontalWrapMode.Wrap; descT.verticalOverflow = VerticalWrapMode.Truncate; descT.raycastTarget = false;
            var descSh = descGo.AddComponent<Shadow>(); descSh.effectColor = new Color(0, 0, 0, 0.8f); descSh.effectDistance = new Vector2(1, -1);

            // Cooldown line (active only) at the card bottom (§23-F 18px, clock motif via a small disc tick).
            if (isActive)
            {
                UiWidgets.Label(card, "⏱ 60s Cooldown", 18, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 22f), new Vector2(-24f, 26f), TextAnchor.MiddleLeft, Cooldown);
                var cdLbl = (RectTransform)card.GetChild(card.childCount - 1); cdLbl.offsetMin = new Vector2(24f, cdLbl.offsetMin.y); cdLbl.offsetMax = new Vector2(-24f, cdLbl.offsetMax.y);
            }
            return cg;
        }

        // ============================ VS BADGE ============================
        private void BuildVsBadge()
        {
            float d = 0.075f * W; // ≈175 px
            var go = new GameObject("VSBadge", typeof(RectTransform), typeof(CanvasGroup));
            _vsBadge = (RectTransform)go.transform; _vsBadge.SetParent(SafeContent, false);
            _vsBadge.anchorMin = _vsBadge.anchorMax = new Vector2(0.5f, 0.560f);
            _vsBadge.anchoredPosition = Vector2.zero; _vsBadge.sizeDelta = new Vector2(d, d);
            _vsCg = go.GetComponent<CanvasGroup>();

            UiWidgets.Glow(_vsBadge, UiTheme.A(UiTheme.GoldHi, 0.45f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(d * 1.7f, d * 1.7f), 1.6f);
            var disc = PanelChild("VSDisc", _vsBadge, 0.06f, 0.06f, 0.94f, 0.94f); var discImg = disc.gameObject.AddComponent<Image>(); discImg.raycastTarget = false; discImg.sprite = UiTex.Disc(UiTheme.A(UiTheme.Obsidian, 0.9f), 64);
            var ring = PanelChild("VSRing", _vsBadge, 0f, 0f, 1f, 1f); var ringImg = ring.gameObject.AddComponent<Image>(); ringImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 64, 12); ringImg.type = Image.Type.Sliced; ringImg.raycastTarget = false;
            UiWidgets.TitleLabel(_vsBadge, "VS", 64, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(d, d * 0.7f), TextAnchor.MiddleCenter, Hex("#fff2c0"), Hex("#e2a93a"), false);
            // Idle faint pulse (§23-I "VS badge faint pulse").
            _vsBadge.gameObject.AddComponent<PulseScale>().period = 2.2f;
        }

        // ============================ SELECT BEHAVIOR (presentation-only; §12 server-auth request stand-in) ============================
        private void OnSelect(string commanderName, bool right)
        {
            // §K/§12: a real build issues a server-authoritative "set active commander" request; the UI never mutates
            // gameplay/ECS. Here we record the display-only choice and confirm visually, then route back.
            UiStub.SelectedCommander = commanderName;
            SetSelectedVisual(ref _left, !right);
            SetSelectedVisual(ref _right, right);
            // Confirm bloom on the chosen crest/CTA, then short fade-out + pop (§23-I OnSelect 0.25s).
            StartCoroutine(ConfirmAndPop(right ? _rightPanel : _leftPanel, right ? _right.Glow : _left.Glow));
        }

        // Reflect the already-committed commander on first show (§23-H equipped → "SELECTED").
        private void ApplyCommittedState()
        {
            bool rightCommitted = UiStub.SelectedCommander == _right.Name;
            bool leftCommitted = UiStub.SelectedCommander == _left.Name;
            if (leftCommitted) SetSelectedVisual(ref _left, true);
            if (rightCommitted) SetSelectedVisual(ref _right, true);
        }

        private void SetSelectedVisual(ref Side s, bool selected)
        {
            if (s.SelectLabel == null) return;
            s.SelectLabel.text = UiTheme.Track(selected ? "SELECTED" : "SELECT");
            if (s.SelectCheck != null) s.SelectCheck.gameObject.SetActive(selected);
        }

        // ============================ LIFECYCLE / ENTER CEREMONY (§23-I) ============================
        public override void OnShow()
        {
            AudioManager.Instance?.PlayMenuMusic();
            if (!_revealed) StartCoroutine(Reveal());
        }

        // §23-I timeline: washes/seam (implicit, baked) → header → panels slide in (24px) → VS pop → cards
        // fade/slide (active then passive, 0.05 stagger) → XP bars fill → SELECT glow sweep. Tap to skip.
        private IEnumerator Reveal()
        {
            // Start hidden.
            SetCg(_headerCg, 0f); SetCg(_leftCg, 0f); SetCg(_rightCg, 0f); SetCg(_vsCg, 0f);
            SetCg(_left.CardActiveCg, 0f); SetCg(_left.CardPassiveCg, 0f); SetCg(_left.SelectCg, 0f);
            SetCg(_right.CardActiveCg, 0f); SetCg(_right.CardPassiveCg, 0f); SetCg(_right.SelectCg, 0f);
            Vector2 lBase = _leftPanel.anchoredPosition, rBase = _rightPanel.anchoredPosition;
            _leftPanel.anchoredPosition = lBase + new Vector2(-24f, 0f);
            _rightPanel.anchoredPosition = rBase + new Vector2(24f, 0f);
            if (_vsBadge != null) _vsBadge.localScale = Vector3.zero;

            yield return Wait(0.05f);
            // Header (0.22s).
            yield return Tween(0.22f, k => { SetCg(_headerCg, k); });

            // Panels slide in symmetrically (0.25s ease-out) + fade.
            yield return Tween(0.25f, k => {
                float e = EaseOut(k);
                SetCg(_leftCg, k); SetCg(_rightCg, k);
                _leftPanel.anchoredPosition = lBase + new Vector2(Mathf.Lerp(-24f, 0f, e), 0f);
                _rightPanel.anchoredPosition = rBase + new Vector2(Mathf.Lerp(24f, 0f, e), 0f);
            });

            // VS pop (scale 0→1.15→1.0, back-ease) + bloom.
            yield return Tween(0.20f, k => { SetCg(_vsCg, k); if (_vsBadge != null) _vsBadge.localScale = Vector3.one * Mathf.Lerp(0.2f, 1f, Back(k)); });
            if (_vsBadge != null) _vsBadge.localScale = Vector3.one;

            // Ability cards fade/slide up (active then passive, 0.05 stagger).
            yield return FadeCard(_left.CardActiveCg, _right.CardActiveCg, 0.18f);
            yield return Wait(0.05f);
            yield return FadeCard(_left.CardPassiveCg, _right.CardPassiveCg, 0.18f);

            // XP bars fill left→right (0.40s ease-out).
            yield return Tween(0.40f, k => {
                float e = EaseOut(k);
                if (_left.XpFill != null) _left.XpFill.fillAmount = _left.XpAmount * e;
                if (_right.XpFill != null) _right.XpFill.fillAmount = _right.XpAmount * e;
            });

            // SELECT buttons fade in with a single glow sweep.
            yield return Tween(0.22f, k => { SetCg(_left.SelectCg, k); SetCg(_right.SelectCg, k); });

            SnapAll();
            _revealed = true;
        }

        private IEnumerator FadeCard(CanvasGroup a, CanvasGroup b, float d)
        {
            var ra = a != null ? a.transform as RectTransform : null; var rb = b != null ? b.transform as RectTransform : null;
            Vector2 baseA = ra != null ? ra.anchoredPosition : Vector2.zero, baseB = rb != null ? rb.anchoredPosition : Vector2.zero;
            yield return Tween(d, k => {
                SetCg(a, k); SetCg(b, k);
                if (ra != null) ra.anchoredPosition = baseA + new Vector2(0, Mathf.Lerp(12f, 0f, k));
                if (rb != null) rb.anchoredPosition = baseB + new Vector2(0, Mathf.Lerp(12f, 0f, k));
            });
        }

        private IEnumerator ConfirmAndPop(RectTransform panel, Color glow)
        {
            // Confirm bloom around the chosen panel (§23-J faction sparkle stand-in), then fade out the screen + pop.
            if (panel != null)
            {
                var bloom = UiWidgets.Glow(panel, UiTheme.A(UiWidgets.Lighten(glow, 0.3f), 0.7f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, panel.sizeDelta * 0.9f, 1.6f);
                yield return Tween(0.25f, k => { var c = bloom.color; c.a = Mathf.Lerp(0.7f, 0f, k); bloom.color = c; });
            }
            float t = 0f; const float fd = 0.25f;
            while (t < fd && Group != null) { t += Time.unscaledDeltaTime; Group.alpha = 1f - Mathf.Clamp01(t / fd); yield return null; }
            Router.Pop();
        }

        // Tap-to-skip the enter ceremony (resolves the correct end-state).
        private void Update()
        {
            if (_revealed) return;
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0) { SnapAll(); _revealed = true; }
        }

        private void SnapAll()
        {
            SetCg(_headerCg, 1f); SetCg(_leftCg, 1f); SetCg(_rightCg, 1f); SetCg(_vsCg, 1f);
            SetCg(_left.CardActiveCg, 1f); SetCg(_left.CardPassiveCg, 1f); SetCg(_left.SelectCg, 1f);
            SetCg(_right.CardActiveCg, 1f); SetCg(_right.CardPassiveCg, 1f); SetCg(_right.SelectCg, 1f);
            if (_vsBadge != null) _vsBadge.localScale = Vector3.one;
            if (_left.XpFill != null) _left.XpFill.fillAmount = _left.XpAmount;
            if (_right.XpFill != null) _right.XpFill.fillAmount = _right.XpAmount;
        }

        // ============================ helpers ============================
        // A panel-local child spanning [axMin,ayMin]→[axMax,ayMax] of its parent rect (point/box anchored).
        private static RectTransform PanelChild(string name, Transform parent, float axMin, float ayMin, float axMax, float ayMax)
        {
            var rt = UiWidgets.Rect(name, parent, new Vector2(axMin, ayMin), new Vector2(axMax, ayMax), Vector2.zero, Vector2.zero);
            rt.anchorMin = new Vector2(axMin, ayMin); rt.anchorMax = new Vector2(axMax, ayMax); rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            return rt;
        }

        // A shadowed label box-anchored across a panel-local band (lets long names align to the inner edge).
        private static Text PanelLabel(Transform parent, string text, int size, float axMin, float ay, float axMax, float ayTop, TextAnchor align, Color col)
        {
            var t = UiWidgets.Label(parent, text, size, new Vector2(axMin, ay), new Vector2(axMax, ayTop), Vector2.zero, Vector2.zero, align, col);
            var rt = (RectTransform)t.transform; rt.anchorMin = new Vector2(axMin, ay); rt.anchorMax = new Vector2(axMax, ayTop); rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            return t;
        }

        // ---- tiny unscaled tween helpers (menus run at timeScale 0; mirrors CampaignResultScreen). ----
        private static void SetCg(CanvasGroup cg, float a) { if (cg != null) cg.alpha = a; }
        private static float EaseOut(float k) => 1f - (1f - k) * (1f - k); // quad ease-out
        private static float Back(float k) { const float s = 1.70158f; k -= 1f; return k * k * ((s + 1f) * k + s) + 1f; } // back-ease-out
        private IEnumerator Wait(float d) { float t = 0f; while (t < d) { t += Time.unscaledDeltaTime; yield return null; } }
        private IEnumerator Tween(float d, System.Action<float> step) { float t = 0f; while (t < d) { t += Time.unscaledDeltaTime; step(Mathf.Clamp01(t / d)); yield return null; } step(1f); }

        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}
