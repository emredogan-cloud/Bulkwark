// BULWARK — UNITS / ARMY (UI Construction Bible · 22). Presentation-only, REMOVABLE.
//
// Forensic code-built uGUI build of design/UnitsArmyDesign.png per Sections A–O on the existing UiRouter shell:
// a war-table armory — ARMY title + Silver/Gold/Gems chips (header), Iron Pact / Ashen Horde faction tabs, an
// "All + 5 class" filter strip with a "Collected 48/58" counter, a 5×2 scrollable rarity-framed unit-card grid
// (Shieldman SELECTED with a cobalt halo), a right-⅓ detail panel (portrait + prev/next, LEVEL 12/20, three
// stat bars with green "+N" previews, an Upgrade Progress tier row 10–14, the UPGRADE — 200 CTA + hold hint),
// and a Rarity/Collection/Disenchant bottom bar. The roster + stats are clearly DISPLAY-ONLY local stub data;
// upgrade/disenchant/select are display-only requests (Toast / UiStub) — NO ECS, NO gameplay/balance/economy
// mutation, server-authoritative meta only (§12). Roster-count discrepancy (canon 12 units vs the 58 drawn) is
// reproduced as-drawn per §L, not "corrected".

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-22 landscape Army / Units collection + upgrade screen. Presentation-only.</summary>
    public sealed class UnitsArmyScreen : UiScreen
    {
        // ---- Rarity tints (§B / §G palette anchors) ----
        private enum Rarity { Common, Uncommon, Rare, Epic, Legendary }

        // ---- DISPLAY-ONLY roster stub (NOT gameplay/balance). Mirrors the 10 cards drawn on the mockup; the
        //      remaining locked entries (to the drawn "58" total) are filler so the grid scrolls per §A/§E.
        //      Per §L only the drawn numbers are real — everything else is inert layout data. ----
        private struct UnitDef
        {
            public string Name; public int Level; public int MaxLevel; public Rarity Rar; public string Klass;
            public string Role; public int MatHave, MatNeed; public bool Owned; public bool Upgradable;
            public int Health, HealthUp, Damage, DamageUp, Speed, SpeedUp; public int Cost;
        }

        private static readonly UnitDef[] IronPact =
        {
            // The fully-specified SELECTED unit — every number is the exact value drawn (§L).
            new UnitDef{ Name="SHIELDMAN",  Level=12, MaxLevel=20, Rar=Rarity.Rare,      Klass="Shield", Role="Frontline • Defensive", MatHave=125, MatNeed=300, Owned=true,  Upgradable=true,
                         Health=3240, HealthUp=180, Damage=210, DamageUp=12, Speed=86, SpeedUp=4, Cost=200 },
            new UnitDef{ Name="SENTINEL",   Level=11, MaxLevel=20, Rar=Rarity.Common,    Klass="Shield", Role="Frontline • Guard",     MatHave=45,  MatNeed=250, Owned=true,  Upgradable=false,
                         Health=2980, HealthUp=160, Damage=185, DamageUp=10, Speed=80, SpeedUp=3, Cost=180 },
            new UnitDef{ Name="IRON ARCHER",Level=11, MaxLevel=20, Rar=Rarity.Uncommon,  Klass="Bow",    Role="Ranged • Skirmisher",   MatHave=160, MatNeed=250, Owned=true,  Upgradable=false,
                         Health=1640, HealthUp=90,  Damage=240, DamageUp=14, Speed=104,SpeedUp=5, Cost=180 },
            new UnitDef{ Name="HEAVY GUARD",Level=9,  MaxLevel=20, Rar=Rarity.Rare,      Klass="Sword",  Role="Frontline • Bruiser",   MatHave=80,  MatNeed=200, Owned=true,  Upgradable=false,
                         Health=3010, HealthUp=170, Damage=205, DamageUp=12, Speed=72, SpeedUp=3, Cost=160 },
            new UnitDef{ Name="RUNIC ADEPT",Level=8,  MaxLevel=20, Rar=Rarity.Epic,      Klass="Magic",  Role="Caster • Support",      MatHave=30,  MatNeed=200, Owned=true,  Upgradable=false,
                         Health=1380, HealthUp=80,  Damage=265, DamageUp=16, Speed=92, SpeedUp=4, Cost=160 },
            new UnitDef{ Name="MINER",      Level=9,  MaxLevel=20, Rar=Rarity.Common,    Klass="Special",Role="Utility • Economy",     MatHave=210, MatNeed=200, Owned=true,  Upgradable=true,
                         Health=1520, HealthUp=80,  Damage=120, DamageUp=8,  Speed=96, SpeedUp=4, Cost=200 },
            new UnitDef{ Name="WARDEN",     Level=9,  MaxLevel=20, Rar=Rarity.Legendary, Klass="Shield", Role="Commander • Aura",      MatHave=5,   MatNeed=150, Owned=true,  Upgradable=false,
                         Health=4120, HealthUp=210, Damage=260, DamageUp=15, Speed=78, SpeedUp=3, Cost=150 },
            new UnitDef{ Name="CROSSBOWMAN",Level=9,  MaxLevel=20, Rar=Rarity.Uncommon,  Klass="Bow",    Role="Ranged • Piercing",     MatHave=35,  MatNeed=150, Owned=true,  Upgradable=false,
                         Health=1580, HealthUp=90,  Damage=255, DamageUp=15, Speed=98, SpeedUp=5, Cost=150 },
            new UnitDef{ Name="OATHBREAKER",Level=8,  MaxLevel=20, Rar=Rarity.Epic,      Klass="Sword",  Role="Melee • Berserker",     MatHave=10,  MatNeed=100, Owned=true,  Upgradable=false,
                         Health=2760, HealthUp=150, Damage=290, DamageUp=18, Speed=108,SpeedUp=5, Cost=100 },
            new UnitDef{ Name="FLAMECALLER",Level=8,  MaxLevel=20, Rar=Rarity.Legendary, Klass="Magic",  Role="Caster • Area",         MatHave=5,   MatNeed=100, Owned=true,  Upgradable=false,
                         Health=1460, HealthUp=80,  Damage=310, DamageUp=20, Speed=88, SpeedUp=4, Cost=100 },
            // Locked filler entries (silhouetted, no count bar) so the roster scrolls past the 10 visible (§E/§H).
            new UnitDef{ Name="LOCKED", Klass="Sword",  Rar=Rarity.Common, Owned=false },
            new UnitDef{ Name="LOCKED", Klass="Bow",    Rar=Rarity.Common, Owned=false },
            new UnitDef{ Name="LOCKED", Klass="Magic",  Rar=Rarity.Rare,   Owned=false },
            new UnitDef{ Name="LOCKED", Klass="Shield", Rar=Rarity.Common, Owned=false },
            new UnitDef{ Name="LOCKED", Klass="Special",Rar=Rarity.Epic,   Owned=false },
        };

        private static readonly UnitDef[] AshenHorde =
        {
            new UnitDef{ Name="MARAUDER",   Level=10, MaxLevel=20, Rar=Rarity.Common,    Klass="Sword",  Role="Melee • Raider",     MatHave=70,  MatNeed=250, Owned=true, Upgradable=false,
                         Health=2540, HealthUp=140, Damage=230, DamageUp=13, Speed=110, SpeedUp=5, Cost=180 },
            new UnitDef{ Name="ASH SLINGER",Level=9,  MaxLevel=20, Rar=Rarity.Uncommon,  Klass="Bow",    Role="Ranged • Incendiary",MatHave=90,  MatNeed=200, Owned=true, Upgradable=false,
                         Health=1420, HealthUp=80,  Damage=250, DamageUp=15, Speed=100, SpeedUp=5, Cost=160 },
            new UnitDef{ Name="BONE SHAMAN",Level=8,  MaxLevel=20, Rar=Rarity.Epic,      Klass="Magic",  Role="Caster • Curse",     MatHave=20,  MatNeed=200, Owned=true, Upgradable=false,
                         Health=1300, HealthUp=70,  Damage=280, DamageUp=17, Speed=90,  SpeedUp=4, Cost=160 },
            new UnitDef{ Name="DREADGUARD", Level=8,  MaxLevel=20, Rar=Rarity.Rare,      Klass="Shield", Role="Frontline • Dread",  MatHave=60,  MatNeed=200, Owned=true, Upgradable=false,
                         Health=3120, HealthUp=170, Damage=200, DamageUp=12, Speed=70,  SpeedUp=3, Cost=160 },
            new UnitDef{ Name="WARCHIEF",   Level=9,  MaxLevel=20, Rar=Rarity.Legendary, Klass="Sword",  Role="Commander • Roar",   MatHave=5,   MatNeed=150, Owned=true, Upgradable=false,
                         Health=3980, HealthUp=200, Damage=300, DamageUp=18, Speed=96,  SpeedUp=5, Cost=150 },
            new UnitDef{ Name="LOCKED", Klass="Special",Rar=Rarity.Common, Owned=false },
            new UnitDef{ Name="LOCKED", Klass="Shield", Rar=Rarity.Common, Owned=false },
            new UnitDef{ Name="LOCKED", Klass="Bow",    Rar=Rarity.Uncommon, Owned=false },
            new UnitDef{ Name="LOCKED", Klass="Magic",  Rar=Rarity.Epic,   Owned=false },
            new UnitDef{ Name="LOCKED", Klass="Sword",  Rar=Rarity.Rare,   Owned=false },
        };

        // Filter strip classes (radio). "All" + 5 class chips per §C/§E.
        private static readonly string[] FilterKeys = { "All", "Shield", "Sword", "Bow", "Magic", "Special" };

        // Tier node labels under the Upgrade Progress row (§E: 10,11,12,13,14 with 12 current).
        private const int CurrentTier = 12;

        // ---- Runtime state ----
        private UnitDef[] _roster = IronPact;
        private int _faction;            // 0 = Iron Pact, 1 = Ashen Horde
        private int _filter;             // index into FilterKeys (0 = All)
        private int _selected;           // index into _roster of the selected card

        private Text _goldText, _gemText;
        private RectTransform _gridContent;          // ScrollRect content (GridLayoutGroup host)
        private RectTransform _detailHost;           // detail-panel content host (rebuilt on select)
        private Text _collectedVal;

        // Canvas pixel helpers (spec: x = frac×2340, y = frac×1080; anchorY = 1 − fy_from_top).
        private const float CW = 2340f, CHt = 1080f;

        // =====================================================================================================
        protected override void Build()
        {
            BuildBackground();
            BuildHeader();
            BuildFactionTabs();
            BuildFilterStrip();
            BuildCollectedCounter();
            BuildRosterScroll();
            BuildDetailPanel();
            BuildBottomBar();

            PopulateGrid();
            PopulateDetail();
        }

        public override void OnShow()
        {
            // Read-only balance refresh (display-only; never mutated here).
            if (_goldText != null) _goldText.text = UiStub.Gold.ToString("N0");
            if (_gemText != null) _gemText.text = UiStub.Gems.ToString("N0");
            AudioManager.Instance?.PlayMenuMusic();
        }

        // ---- BG_FullBleed: charcoal vertical gradient + corner vignette (§B/§G). Full-bleed → Rect. ----
        private void BuildBackground()
        {
            var bg = UiWidgets.Stretch("BG_FullBleed", Rect, UiTheme.Obsidian);
            bg.sprite = UiTex.VGradient(Hex("#0a0b0f"), Hex("#14161e"), 64); // top → bottom (§G)
            bg.raycastTarget = false;
            // Faint ruined-castle vista bleeding through under the right detail panel (§B).
            UiWidgets.Glow(Rect, UiTheme.A(Hex("#2b3a6a"), 0.10f), new Vector2(0.82f, 0.62f), new Vector2(0.82f, 0.62f), Vector2.zero, new Vector2(900, 1100), 1.7f);
            UiWidgets.Vignette(Rect, 0.45f); // ≈45% corner darken (§G)
        }

        // ---- Header: Back (TL), ARMY title, currency chips (TR) ----
        private void BuildHeader()
        {
            // Back button (top-left ornate corner affordance) → pop to Main Menu (§C/§K).
            UiWidgets.BackButton(SafeContent, () => Router.Pop());

            // "ARMY" serif gold-bevel UPPERCASE title, left edge x≈0.075, baseline y≈0.955 (§E/§F, 56px grad).
            UiWidgets.TitleLabel(SafeContent, "ARMY", 56, new Vector2(0f, 0.955f), new Vector2(0f, 0.955f),
                new Vector2(0.075f * CW, 0f), new Vector2(520, 80), TextAnchor.MiddleLeft, Hex("#f0d27a"), Hex("#caa04a"));

            // Currency chips top-right. Order drawn L→R: Silver, Gold, Gems → rightmost = Gems (idx 0).
            UiWidgets.CurrencyChip(SafeContent, Hex("#9aa3ad"), 12450, 2, out _);  // Silver (display-only; §L value)
            _goldText = UiWidgets.CurrencyChip(SafeContent, UiTheme.Gold, UiStub.Gold, 1, out _);
            _gemText = UiWidgets.CurrencyChip(SafeContent, UiTheme.AmethystHi, UiStub.Gems, 0, out _);
        }

        // ---- Faction tabs (band y 0.855→0.915): two pills, single-select; Iron Pact selected (§E/§H). ----
        private void BuildFactionTabs()
        {
            FactionPill(0, "IRON PACT",   0.020f, 0.185f);
            FactionPill(1, "ASHEN HORDE", 0.193f, 0.358f);
        }

        private void FactionPill(int faction, string label, float xL, float xR)
        {
            bool sel = _faction == faction;
            float fy = 0.885f; // band centre (0.855→0.915 from top) → anchorY 0.115
            float w = (xR - xL) * CW, cx = (xL + xR) * 0.5f;
            // Iron Pact = cobalt enamel; Ashen = oxblood (never recolor faction identity — §L).
            Color body = faction == 0 ? UiTheme.IronBlue : UiTheme.Oxblood;
            var rt = UiWidgets.Rect("Tab_" + label, SafeContent, new Vector2(cx, 1f - fy), new Vector2(cx, 1f - fy), Vector2.zero, new Vector2(w, 0.052f * CHt));
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = UiTex.VGradient(UiWidgets.Lighten(body, sel ? 0.22f : 0.02f), UiWidgets.Darken(body, sel ? 0.18f : 0.45f), 32);
            if (!sel) img.color = new Color(1f, 1f, 1f, 0.7f); // idle desaturated/darkened
            var btn = rt.gameObject.AddComponent<Button>(); btn.targetGraphic = img;
            var cb = btn.colors; cb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); cb.pressedColor = new Color(0.85f, 0.85f, 0.9f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
            int f = faction; btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); SwapFaction(f); });
            // Gold-rim molding (cyan rim on Iron Pact selected, ember on Ashen).
            var rim = UiWidgets.Rect("Rim", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rimImg = rim.gameObject.AddComponent<Image>();
            Color rimC = faction == 0 ? Hex("#7fb0ff") : Hex("#d8452b");
            rimImg.sprite = UiTex.Frame(sel ? rimC : UiTheme.A(UiTheme.Gold, 0.5f), sel ? UiWidgets.Darken(rimC, 0.2f) : UiTheme.A(UiTheme.Gold, 0.4f), UiTheme.GoldShadow, 48, 5);
            rimImg.type = Image.Type.Sliced; rimImg.raycastTarget = false;
            // Crest disc (left) + tab label.
            var crest = UiWidgets.Rect("Crest", rt, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(40, 0), new Vector2(42, 42));
            var crImg = crest.gameObject.AddComponent<Image>(); crImg.raycastTarget = false; crImg.sprite = UiTex.Diamond(sel ? UiTheme.GoldHi : UiTheme.A(UiTheme.Gold, 0.6f), 48);
            UiWidgets.Label(rt, label, 26, new Vector2(0, 0), new Vector2(1, 1), new Vector2(28, 0), Vector2.zero, TextAnchor.MiddleCenter, sel ? Hex("#eaf2ff") : Hex("#c9b27a"));
        }

        // ---- Filter strip (band y 0.790→0.845): "All" pill + 5 square class chips, single-select (§E/§H). ----
        private void BuildFilterStrip()
        {
            float fy = 0.8175f, ay = 1f - fy;
            // "All" pill (≈0.050w wide) at x≈0.020.
            FilterChip(0, 0.020f, 0.050f * CW, ay, "All", true);
            // 5 square class chips ≈0.038w each, gap 0.008w, running to x≈0.40.
            float x = 0.080f, sq = 0.038f, gap = 0.008f;
            for (int i = 1; i < FilterKeys.Length; i++)
            {
                FilterChip(i, x, sq * CW, ay, FilterKeys[i], false);
                x += sq + gap;
            }
        }

        private void FilterChip(int idx, float xL, float w, float ay, string key, bool isAllPill)
        {
            bool sel = _filter == idx;
            float h = 0.045f * CHt, cx = xL + (w * 0.5f) / CW;
            var rt = UiWidgets.Rect("Filter_" + key, SafeContent, new Vector2(cx, ay), new Vector2(cx, ay), Vector2.zero, new Vector2(w, h));
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = UiTex.VGradient(UiTheme.A(UiTheme.Charcoal, sel ? 1f : 0.85f), UiTheme.A(UiTheme.Obsidian, sel ? 1f : 0.85f), 32);
            var btn = rt.gameObject.AddComponent<Button>(); btn.targetGraphic = img;
            var cb = btn.colors; cb.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
            int i = idx; btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); SetFilter(i); });
            // Gold ring (lit when selected, dim idle).
            var ring = UiWidgets.Rect("Ring", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rImg = ring.gameObject.AddComponent<Image>();
            rImg.sprite = UiTex.Frame(sel ? UiTheme.GoldHi : UiTheme.A(UiTheme.Gold, 0.35f), sel ? UiTheme.Gold : UiTheme.A(UiTheme.Gold, 0.3f), UiTheme.GoldShadow, 48, 4);
            rImg.type = Image.Type.Sliced; rImg.raycastTarget = false;
            if (isAllPill)
                UiWidgets.Label(rt, "All", 22, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Hex("#d9c79a"));
            else
            {
                // Class glyph (icon-only chips: shield/swords/bow/magic/special — read as diamonds per §N).
                var gly = UiWidgets.Rect("Glyph", rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(w * 0.5f, w * 0.5f));
                var gImg = gly.gameObject.AddComponent<Image>(); gImg.raycastTarget = false;
                gImg.sprite = UiTex.Diamond(sel ? UiTheme.GoldHi : Hex("#9aa3ad"), 48);
            }
        }

        // ---- CollectedCounter (anchored to grid right edge x≈0.625, top y≈0.840): "Collected" over "48/58". ----
        private void BuildCollectedCounter()
        {
            float ax = 0.625f;
            UiWidgets.Label(SafeContent, "Collected", 20, new Vector2(ax, 1f - 0.835f), new Vector2(ax, 1f - 0.835f), Vector2.zero, new Vector2(240, 30), TextAnchor.MiddleRight, Hex("#b9c0c8"));
            _collectedVal = UiWidgets.Label(SafeContent, "48/58", 26, new Vector2(ax, 1f - 0.865f), new Vector2(ax, 1f - 0.865f), Vector2.zero, new Vector2(240, 36), TextAnchor.MiddleRight, Hex("#f4e9c8"));
        }

        // ---- RosterScroll: a standard uGUI ScrollRect (vertical) hosting a GridLayoutGroup (5 cols). ----
        private void BuildRosterScroll()
        {
            // Grid region x 0.018→0.630, y(from top) 0.130→0.775 (§E).
            float xL = 0.018f, xR = 0.630f, fyTop = 0.130f, fyBot = 0.775f;
            float wPx = (xR - xL) * CW, hPx = (fyBot - fyTop) * CHt;
            float cx = (xL + xR) * 0.5f, cyTop = 1f - fyTop, cyBot = 1f - fyBot;
            float cy = (cyTop + cyBot) * 0.5f;

            // Viewport (masked).
            var view = UiWidgets.Rect("RosterScroll", SafeContent, new Vector2(cx, cy), new Vector2(cx, cy), Vector2.zero, new Vector2(wPx, hPx));
            var viewImg = view.gameObject.AddComponent<Image>(); viewImg.color = new Color(0, 0, 0, 0.001f); // raycast surface for scroll drag
            view.gameObject.AddComponent<RectMask2D>();
            var scroll = view.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false; scroll.vertical = true; scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 28f; scroll.viewport = view;

            // Content (top-stretch; GridLayoutGroup grows downward).
            var content = UiWidgets.Rect("RosterGrid", view, new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, new Vector2(0, hPx));
            content.pivot = new Vector2(0.5f, 1f);
            _gridContent = content;
            scroll.content = content;

            var grid = content.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(264f, 324f);            // §E cellW≈264, cellH≈324 @1080
            grid.spacing = new Vector2(28f, 32f);               // 0.012w col gap, 0.030h row gap
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        // =====================================================================================================
        // GRID POPULATION (rebuilt on faction/filter change)
        // =====================================================================================================
        private void PopulateGrid()
        {
            // Clear existing cards.
            for (int i = _gridContent.childCount - 1; i >= 0; i--) Object.Destroy(_gridContent.GetChild(i).gameObject);

            int shown = 0;
            for (int i = 0; i < _roster.Length; i++)
            {
                if (_filter != 0 && _roster[i].Klass != FilterKeys[_filter]) continue;
                BuildUnitCard(i, shown);
                shown++;
            }
        }

        private void BuildUnitCard(int rosterIndex, int gridIndex)
        {
            var u = _roster[rosterIndex];
            bool selected = rosterIndex == _selected;

            // Card root = Button (whole card clickable). Cell sized by GridLayoutGroup.
            var card = UiWidgets.Rect("UnitCard_" + u.Name, _gridContent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(264, 324));
            // Cast-iron well fill.
            var well = card.gameObject.AddComponent<Image>();
            well.sprite = UiTex.VGradient(Hex("#1b1d26"), Hex("#0d0e14"), 32); well.color = u.Owned ? Color.white : new Color(0.7f, 0.7f, 0.72f, 1f);
            var btn = card.gameObject.AddComponent<Button>(); btn.targetGraphic = well;
            var cb = btn.colors; cb.highlightedColor = new Color(1.06f, 1.06f, 1.06f, 1f); cb.pressedColor = new Color(0.9f, 0.9f, 0.95f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
            int idx = rosterIndex; btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); SelectCard(idx); });

            // SelectGlow (child 2) — cobalt halo, only on the selected owned card (§C/§H). Overscan beyond bounds.
            if (selected && u.Owned)
            {
                var glow = UiWidgets.Glow(card, UiTheme.A(Hex("#4f8bff"), 0.85f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(330, 400), 1.5f);
                var pg = glow.gameObject.AddComponent<PulseGraphic>(); pg.target = glow; pg.min = 0.55f; pg.max = 0.9f; pg.period = 2f; // faint pulse (§J)
                glow.transform.SetAsFirstSibling();
                // Slow rising spark motes over the selected card (§J: 2–3 motes).
                var motes = UiWidgets.Rect("SelMotes", card, new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), Vector2.zero, new Vector2(220, 260));
                var ef = motes.gameObject.AddComponent<EmberField>(); ef.count = 3; ef.color = Hex("#7fb0ff");
            }

            // Portrait (masked into a recessed well, inset ≈10% of cell).
            float pad = 0.10f * 264f;
            var portrait = UiWidgets.Rect("Portrait", card, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-pad, -pad));
            var pImg = portrait.gameObject.AddComponent<Image>(); pImg.raycastTarget = false;
            Color rar = RarityColor(u.Rar);
            if (u.Owned)
                pImg.sprite = UiTex.VGradient(UiWidgets.Lighten(rar, 0.05f), Hex("#101218"), 64); // lit portrait stand-in tinted by rarity
            else
                pImg.sprite = UiTex.VGradient(Hex("#26282f"), Hex("#0c0d12"), 64); // dark silhouette (locked)

            // Rarity-tinted bevel frame (child 0, drawn over the card bounds).
            var frame = UiWidgets.Rect("Frame_Rarity", card, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var fImg = frame.gameObject.AddComponent<Image>();
            Color fHi = u.Owned ? UiWidgets.Lighten(rar, 0.4f) : UiTheme.A(Hex("#5a5e66"), 0.7f);
            Color fMid = u.Owned ? (selected ? UiTheme.GoldHi : rar) : UiTheme.A(Hex("#3a3d44"), 0.7f);
            fImg.sprite = UiTex.Frame(selected ? UiTheme.GoldHi : fHi, fMid, UiTheme.GoldShadow, 64, selected ? 10 : 7);
            fImg.type = Image.Type.Sliced; fImg.raycastTarget = false;

            // Rarity shimmer: a slow diagonal specular sweep on epic/legendary frames (§J — common = none).
            if (u.Owned && (u.Rar == Rarity.Epic || u.Rar == Rarity.Legendary))
            {
                var sweepHost = UiWidgets.Rect("Shimmer", card, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                var sweep = UiWidgets.Rect("Sheen", sweepHost, new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, new Vector2(70, 0));
                var sImg = sweep.gameObject.AddComponent<Image>(); sImg.raycastTarget = false;
                sImg.sprite = UiTex.HGradient(UiTheme.A(Color.white, 0f), UiTheme.A(Color.white, u.Rar == Rarity.Legendary ? 0.45f : 0.3f), 32);
                var sh = sweepHost.gameObject.AddComponent<Sheen>(); sh.target = sweep; sh.minX = -150f; sh.maxX = 150f; sh.period = 6f; // ~6s loop
            }

            if (u.Owned)
            {
                // Badge_Level (child 3): bottom-left circle Ø ≈ 0.34·cellW (≈90px), number centered (§E).
                var badge = UiWidgets.Rect("Badge_Level", card, new Vector2(0, 0), new Vector2(0, 0), new Vector2(6 + 45, 60 + 45), new Vector2(90, 90));
                var bImg = badge.gameObject.AddComponent<Image>(); bImg.raycastTarget = false; bImg.sprite = UiTex.Disc(Hex("#16181f"), 64);
                var bRing = UiWidgets.Rect("BadgeRing", badge, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                var brImg = bRing.gameObject.AddComponent<Image>(); brImg.raycastTarget = false; brImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 5); brImg.type = Image.Type.Sliced;
                var lv = UiWidgets.Label(badge, u.Level.ToString(), 24, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Hex("#fff4d6"));
                lv.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.StrokeDark, 0.95f);

                // Lbl_Name (child 4): band at ~78% card height, centered UPPERCASE (cap≈18px) (§E/§F).
                var name = UiWidgets.Label(card, u.Name, 18, new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), Vector2.zero, new Vector2(252, 26), TextAnchor.MiddleCenter, Hex("#e8e2cf"));
                name.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.StrokeDark, 0.9f);

                // CountBar (child 5): bottom strip full width inset 6px, height ≈ 0.075·cellH (≈24px) (§E).
                BuildCountBar(card, u);
            }
            else
            {
                // Locked: 🔒 in place of the badge, no count bar (§H).
                var lockRt = UiWidgets.Rect("Lock", card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(70, 70));
                var lImg = lockRt.gameObject.AddComponent<Image>(); lImg.raycastTarget = false; lImg.sprite = UiTex.Diamond(UiTheme.A(Hex("#8a8d94"), 0.85f), 48);
                UiWidgets.Label(card, "LOCKED", 16, new Vector2(0.5f, 0.16f), new Vector2(0.5f, 0.16f), Vector2.zero, new Vector2(252, 24), TextAnchor.MiddleCenter, Hex("#8a8d94"));
            }

            // OnShow enter timeline: stagger card scale 0.92→1.0 + fade (left→right, top→bottom) (§I).
            StaggerIn(card, gridIndex);
        }

        // CountBar: groove track + rarity-tinted fill + centered "have / need" + ▲ up-arrow when upgradable (§E/§H).
        private void BuildCountBar(RectTransform card, UnitDef u)
        {
            var barBg = UiWidgets.Rect("CountBar", card, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 14), new Vector2(252, 24));
            var bgImg = barBg.gameObject.AddComponent<Image>(); bgImg.raycastTarget = false; bgImg.sprite = UiTex.VGradient(Hex("#1b1d24"), Hex("#0c0d12"), 16);
            float amt = u.MatNeed > 0 ? Mathf.Clamp01(u.MatHave / (float)u.MatNeed) : 0f;
            var fillRt = UiWidgets.Rect("Fill", barBg, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-4, -4));
            var fImg = fillRt.gameObject.AddComponent<Image>(); fImg.raycastTarget = false;
            // Full (ready) green-cream, low (short) oxblood-red per §F count colours.
            Color fillCol = u.Upgradable ? Hex("#7fc04a") : Hex("#cf6b5a");
            fImg.sprite = UiTex.HGradient(UiWidgets.Darken(fillCol, 0.1f), UiWidgets.Lighten(fillCol, 0.15f), 32);
            fImg.type = Image.Type.Filled; fImg.fillMethod = Image.FillMethod.Horizontal; fImg.fillOrigin = 0; fImg.fillAmount = amt;
            var txt = UiWidgets.Label(barBg, u.MatHave + " / " + u.MatNeed, 17, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, u.Upgradable ? Hex("#e8f0c0") : Hex("#e8e2cf"));
            txt.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.StrokeDark, 0.85f);
            if (u.Upgradable)
            {
                // Small green ▲ up-arrow at the right end (§E/§H — Shieldman, Miner show it).
                var up = UiWidgets.Label(barBg, "▲", 20, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-14, 0), new Vector2(28, 28), TextAnchor.MiddleCenter, Hex("#7ff06a"));
                up.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.StrokeDark, 0.9f);
            }
        }

        // =====================================================================================================
        // DETAIL PANEL (right ⅓): static frame built once, content rebuilt on selection
        // =====================================================================================================
        private void BuildDetailPanel()
        {
            // Panel region x 0.660→0.982, y(from top) 0.060→0.945 (§E).
            float xL = 0.660f, xR = 0.982f, fyTop = 0.060f, fyBot = 0.945f;
            float wPx = (xR - xL) * CW, hPx = (fyBot - fyTop) * CHt;
            float cx = (xL + xR) * 0.5f, cy = 1f - (fyTop + fyBot) * 0.5f;

            var panel = UiWidgets.Rect("DetailPanel", SafeContent, new Vector2(cx, cy), new Vector2(cx, cy), Vector2.zero, new Vector2(wPx, hPx));
            // Detail_BG: dark stone slab + faint vista (§G).
            var bg = panel.gameObject.AddComponent<Image>();
            bg.sprite = UiTex.VGradient(Hex("#141621"), Hex("#0c0d14"), 64); bg.raycastTarget = true;
            UiWidgets.Glow(panel, UiTheme.A(Hex("#2b3a6a"), 0.18f), new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), Vector2.zero, new Vector2(wPx, hPx * 0.7f), 1.8f);
            // Gold filigree frame.
            var rim = UiWidgets.Rect("Rim", panel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rimImg = rim.gameObject.AddComponent<Image>(); rimImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 64, 10); rimImg.type = Image.Type.Sliced; rimImg.raycastTarget = false;
            UiWidgets.Vignette(panel, 0.35f); // soft inner vignette

            // Content host (insets the frame molding) — rebuilt on each selection.
            var host = UiWidgets.Rect("DetailContent", panel, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-36, -36));
            _detailHost = host;
        }

        private void PopulateDetail()
        {
            var host = _detailHost;
            for (int i = host.childCount - 1; i >= 0; i--) Object.Destroy(host.GetChild(i).gameObject);

            var u = _roster[_selected];
            // Panel-local fractions are mapped to the panel's normalized 0..1 (the panel spans canvas 0.060→0.945
            // vertically); we re-express each §E panel y as a local anchorY within the host.
            // Panel top-frac = 0.060, bottom-frac = 0.945; helper maps a screen-frac-from-top to host anchorY.
            float pTop = 0.060f, pBot = 0.945f, span = pBot - pTop;
            System.Func<float, float> ay = fyTop => 1f - (fyTop - pTop) / span;

            // Detail_Crest (top-right small class crest).
            var crest = UiWidgets.Rect("Detail_Crest", host, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -16), new Vector2(56, 56));
            var crImg = crest.gameObject.AddComponent<Image>(); crImg.raycastTarget = false; crImg.sprite = UiTex.Diamond(RarityColor(u.Rar), 48);

            // Detail_Name (serif gold, baseline y≈0.905) + Detail_Role (y≈0.875).
            UiWidgets.TitleLabel(host, u.Name, 43, new Vector2(0.5f, ay(0.905f)), new Vector2(0.5f, ay(0.905f)), Vector2.zero, new Vector2(640, 60), TextAnchor.MiddleCenter, Hex("#f0d27a"), Hex("#caa04a"));
            UiWidgets.Label(host, UiTheme.Track(u.Role, 1), 22, new Vector2(0.5f, ay(0.875f)), new Vector2(0.5f, ay(0.875f)), Vector2.zero, new Vector2(640, 34), TextAnchor.MiddleCenter, Hex("#c9b27a"));

            // Detail_Portrait (centered, region y 0.640→0.860, width ≈0.20w). Prev/Next chevrons at its mid.
            float portTop = ay(0.640f), portBot = ay(0.860f), portCy = (portTop + portBot) * 0.5f;
            var port = UiWidgets.Rect("Detail_Portrait", host, new Vector2(0.5f, portCy), new Vector2(0.5f, portCy), Vector2.zero, new Vector2(0.20f * CW, (0.860f - 0.640f) * CHt));
            var portImg = port.gameObject.AddComponent<Image>(); portImg.raycastTarget = false;
            portImg.sprite = UiTex.VGradient(UiWidgets.Lighten(RarityColor(u.Rar), 0.08f), Hex("#0c0d14"), 64);
            var portRim = UiWidgets.Rect("PortRim", port, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var prImg = portRim.gameObject.AddComponent<Image>(); prImg.raycastTarget = false; prImg.sprite = UiTex.Frame(UiTheme.A(UiTheme.GoldHi, 0.7f), UiTheme.A(UiTheme.Gold, 0.6f), UiTheme.GoldShadow, 48, 6); prImg.type = Image.Type.Sliced;
            UiWidgets.Glow(port, UiTheme.A(UiWidgets.Lighten(RarityColor(u.Rar), 0.2f), 0.3f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), Vector2.zero, new Vector2(360, 300), 1.7f); // volumetric backlight (§J)
            var dust = UiWidgets.Rect("Dust", port, new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), Vector2.zero, new Vector2(300, 260));
            var dustEf = dust.gameObject.AddComponent<EmberField>(); dustEf.count = 2; dustEf.color = Hex("#cdb887"); // 1–2 dust motes
            // Prev / Next chevrons pinned to panel inner left/right at the portrait mid.
            Chevron(host, "<", new Vector2(0.04f, portCy), -1);
            Chevron(host, ">", new Vector2(0.96f, portCy), +1);

            // Detail_LevelRow (y≈0.610): "LEVEL" + big "12" + "/20" + ⓘ.
            float lvY = ay(0.610f);
            UiWidgets.Label(host, UiTheme.Track("LEVEL"), 24, new Vector2(0.32f, lvY), new Vector2(0.32f, lvY), Vector2.zero, new Vector2(180, 40), TextAnchor.MiddleRight, Hex("#f0d27a"));
            var bigLv = UiWidgets.Label(host, u.Level.ToString(), 40, new Vector2(0.52f, lvY), new Vector2(0.52f, lvY), Vector2.zero, new Vector2(90, 56), TextAnchor.MiddleCenter, Hex("#fff4d6"));
            bigLv.gameObject.AddComponent<UiGradientText>().top = Hex("#fff4d6");
            UiWidgets.Label(host, "/" + u.MaxLevel, 24, new Vector2(0.62f, lvY), new Vector2(0.62f, lvY), Vector2.zero, new Vector2(90, 40), TextAnchor.MiddleLeft, Hex("#9a8a5a"));
            var info = UiWidgets.Rect("Btn_Info", host, new Vector2(0.80f, lvY), new Vector2(0.80f, lvY), Vector2.zero, new Vector2(44, 44));
            var infoImg = info.gameObject.AddComponent<Image>(); infoImg.sprite = UiTex.Disc(UiTheme.A(UiTheme.Charcoal, 0.95f), 48);
            var infoBtn = info.gameObject.AddComponent<Button>(); infoBtn.targetGraphic = infoImg;
            var icb = infoBtn.colors; icb.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f); infoBtn.colors = icb;
            infoBtn.onClick.AddListener(() => { AudioManager.Instance?.Click(); Router.Toast(u.Name + " — stat breakdown (coming soon)"); });
            UiWidgets.Label(info, "i", 28, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, UiTheme.GoldHi);

            // StatRows (y 0.460→0.585, 3 rows, pitch ≈0.042h): Health / Damage / Speed (§E/§F/§G).
            StatRow(host, ay(0.470f), "♥", Hex("#5fd35a"), u.Health, u.HealthUp); // ♥
            StatRow(host, ay(0.518f), "⚔", Hex("#f0a93a"), u.Damage, u.DamageUp); // ⚔
            StatRow(host, ay(0.566f), "»", Hex("#4fb0ff"), u.Speed, u.SpeedUp);   // »

            // UpgradeProgress (label y≈0.430, TierNodeRow y 0.370→0.405): nodes 10..14, current=12 (§E/§H).
            UiWidgets.Label(host, UiTheme.Track("UPGRADE PROGRESS"), 18, new Vector2(0.5f, ay(0.430f)), new Vector2(0.5f, ay(0.430f)), Vector2.zero, new Vector2(560, 28), TextAnchor.MiddleCenter, Hex("#9a8a5a"));
            BuildTierNodeRow(host, ay(0.388f), u);

            // Btn_Upgrade (y 0.250→0.320, height ≈0.070h, spans inner width): cobalt CTA "UPGRADE — N" (§E/§G).
            BuildUpgradeCta(host, ay(0.285f), u);

            // Lbl_HoldHint (y≈0.225, small italic).
            UiWidgets.Label(host, UiTheme.Track("Hold to upgrade quickly"), 18, new Vector2(0.5f, ay(0.225f)), new Vector2(0.5f, ay(0.225f)), Vector2.zero, new Vector2(560, 26), TextAnchor.MiddleCenter, Hex("#9aa3ad"));

            // OnSelect enter: detail content fades in (cross-fade stand-in) (§I).
            var cg = host.gameObject.GetComponent<CanvasGroup>(); if (cg == null) cg = host.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f; StartCoroutine(FadeGroup(cg, 0.18f));
        }

        private void Chevron(RectTransform host, string glyph, Vector2 anchor, int dir)
        {
            var rt = UiWidgets.Rect("Btn_Chevron", host, anchor, anchor, Vector2.zero, new Vector2(56, 80));
            var img = rt.gameObject.AddComponent<Image>(); img.color = new Color(0, 0, 0, 0.001f);
            var btn = rt.gameObject.AddComponent<Button>(); btn.targetGraphic = img;
            var cb = btn.colors; cb.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f); btn.colors = cb;
            int d = dir; btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); StepSelection(d); });
            UiWidgets.Label(rt, glyph, 48, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, UiTheme.A(UiTheme.GoldHi, 0.85f));
        }

        // Stat row: icon (left) | groove track + glassy fill (+ brighter "+N" cap) | value | green "+N" (§E/§F/§G).
        private void StatRow(RectTransform host, float ay, string icon, Color fillCol, int value, int up)
        {
            // Icon at left (x≈0.04 local).
            UiWidgets.Label(host, icon, 30, new Vector2(0.04f, ay), new Vector2(0.04f, ay), Vector2.zero, new Vector2(44, 44), TextAnchor.MiddleCenter, fillCol);
            // Track from x≈0.12→0.62 local (maps the §E screen track x 0.700→0.870 into the host).
            float tL = 0.12f, tR = 0.62f, tCx = (tL + tR) * 0.5f;
            var track = UiWidgets.Rect("StatTrack", host, new Vector2(tCx, ay), new Vector2(tCx, ay), Vector2.zero, new Vector2(0, 22));
            // size by anchors-relative width: use sizeDelta via fraction of host width is awkward with point anchor,
            // so stretch the track horizontally between tL and tR explicitly:
            track.anchorMin = new Vector2(tL, ay); track.anchorMax = new Vector2(tR, ay); track.sizeDelta = new Vector2(0, 22); track.anchoredPosition = Vector2.zero;
            var tImg = track.gameObject.AddComponent<Image>(); tImg.raycastTarget = false; tImg.sprite = UiTex.VGradient(Hex("#1b1d24"), Hex("#0c0d12"), 16);
            // Glassy fill (proportional to value within a display-only nominal max of 4500 for HP-scale legibility).
            float baseAmt = Mathf.Clamp01(value / DisplayMax(value));
            var fillRt = UiWidgets.Rect("Fill", track, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-4, -4));
            var fImg = fillRt.gameObject.AddComponent<Image>(); fImg.raycastTarget = false;
            fImg.sprite = UiTex.VGradient(UiWidgets.Lighten(fillCol, 0.25f), UiWidgets.Darken(fillCol, 0.1f), 16);
            fImg.type = Image.Type.Filled; fImg.fillMethod = Image.FillMethod.Horizontal; fImg.fillOrigin = 0; fImg.fillAmount = 0f;
            // Brighter "+N" preview cap at the end of the fill (§G — lighter cap on the fill).
            var cap = UiWidgets.Rect("UpCap", track, new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(10, 18));
            var capImg = cap.gameObject.AddComponent<Image>(); capImg.raycastTarget = false; capImg.color = Hex("#7ff06a");
            var capPulse = cap.gameObject.AddComponent<PulseGraphic>(); capPulse.target = capImg; capPulse.min = 0.35f; capPulse.max = 0.9f; capPulse.period = 1.3f; // "+N" segment pulses once-ish
            // Animate the bar fill left→right to its current value (§I — 0.45s ease-out). Coroutine on this screen
            // (no new MonoBehaviour type): if the bar is destroyed mid-tween the null guards stop it cleanly.
            StartCoroutine(FillBar(fImg, track, cap, baseAmt, 0.45f));

            // Value (right-aligned) + green "+N" to its right (§E x≈0.905 / x≈0.945).
            UiWidgets.Label(host, value.ToString("N0"), 26, new Vector2(0.78f, ay), new Vector2(0.78f, ay), Vector2.zero, new Vector2(140, 36), TextAnchor.MiddleRight, Hex("#f4f0e2"));
            var plus = UiWidgets.Label(host, "+" + up, 22, new Vector2(0.90f, ay), new Vector2(0.90f, ay), Vector2.zero, new Vector2(90, 32), TextAnchor.MiddleLeft, Hex("#7ff06a"));
            plus.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(Hex("#1a3a14"), 0.6f); // subtle green glow stand-in
        }

        // Display-only nominal scale so stat bars read proportionally (NOT a balance value — pure layout).
        private static float DisplayMax(int value) => value > 1000 ? 4500f : (value > 200 ? 350f : 130f);

        private void BuildTierNodeRow(RectTransform host, float ay, UnitDef u)
        {
            int[] tiers = { 10, 11, 12, 13, 14 };
            float xC = 0.16f, step = 0.17f; // 5 nodes spread across the panel inner width
            for (int i = 0; i < tiers.Length; i++)
            {
                float x = xC + i * step;
                bool passed = tiers[i] < CurrentTier;
                bool current = tiers[i] == CurrentTier;
                float d = current ? 0.038f * CW * 1.15f : 0.038f * CW; // current ×1.15 (§E)
                // Connector bar to previous node (gold up to current) (§H).
                if (i > 0)
                {
                    bool conPassed = tiers[i] <= CurrentTier;
                    float conX = xC + (i - 0.5f) * step;
                    var con = UiWidgets.Rect("Connector", host, new Vector2(conX, ay), new Vector2(conX, ay), Vector2.zero, new Vector2(step * 480f, 8));
                    var cImg = con.gameObject.AddComponent<Image>(); cImg.raycastTarget = false;
                    cImg.sprite = UiTex.HGradient(conPassed ? UiTheme.Gold : Hex("#2b2e36"), conPassed ? UiTheme.GoldHi : Hex("#2b2e36"), 32);
                }
                var node = UiWidgets.Rect("Tier_" + tiers[i], host, new Vector2(x, ay), new Vector2(x, ay), Vector2.zero, new Vector2(d, d));
                var nImg = node.gameObject.AddComponent<Image>(); nImg.raycastTarget = false;
                if (passed || current) nImg.sprite = UiTex.Disc(current ? UiTheme.GoldHi : UiTheme.Gold, 64);
                else nImg.sprite = UiTex.Disc(Hex("#2b2e36"), 64);
                var nRing = UiWidgets.Rect("NodeRing", node, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                var nrImg = nRing.gameObject.AddComponent<Image>(); nrImg.raycastTarget = false;
                nrImg.sprite = UiTex.Frame(current ? UiTheme.GoldHi : (passed ? UiTheme.Gold : UiTheme.A(Hex("#9aa3ad"), 0.6f)), current ? UiTheme.Gold : UiTheme.GoldShadow, UiTheme.GoldShadow, 48, 5);
                nrImg.type = Image.Type.Sliced;
                if (current)
                {
                    // Continuous soft gold glow-ring pulse on the current node (§J).
                    var glow = UiWidgets.Glow(node, UiTheme.A(UiTheme.GoldHi, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(d * 1.6f, d * 1.6f), 1.6f);
                    var pg = glow.gameObject.AddComponent<PulseGraphic>(); pg.target = glow; pg.min = 0.4f; pg.max = 0.8f; pg.period = 1.8f;
                    glow.transform.SetAsFirstSibling();
                    node.gameObject.AddComponent<PulseScale>().period = 1.8f; // current glow-ring scale 1.0→1.15 (§I)
                }
                // Node label beneath (10..14; 12 highlighted gold).
                UiWidgets.Label(node, tiers[i].ToString(), 20, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, -18), new Vector2(60, 26), TextAnchor.UpperCenter, current ? Hex("#f0d27a") : Hex("#9aa3ad"));
                // Passed nodes get a check-style centre pip (§H "check-style").
                if (passed)
                {
                    var chk = UiWidgets.Label(node, "✓", 22, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Hex("#1a1206"));
                    chk.raycastTarget = false;
                }
                else if (current)
                {
                    var num = UiWidgets.Label(node, tiers[i].ToString(), 20, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Hex("#3a2a08"));
                    num.raycastTarget = false;
                }
            }
        }

        private void BuildUpgradeCta(RectTransform host, float ay, UnitDef u)
        {
            bool maxed = u.Level >= u.MaxLevel || u.Cost <= 0;
            bool affordable = !maxed && UiStub.Gold >= u.Cost;

            var cta = UiWidgets.Rect("Btn_Upgrade", host, new Vector2(0.5f, ay), new Vector2(0.5f, ay), Vector2.zero, new Vector2(0, 0.070f * CHt));
            cta.anchorMin = new Vector2(0.04f, ay); cta.anchorMax = new Vector2(0.96f, ay); cta.sizeDelta = new Vector2(0, 0.070f * CHt); cta.anchoredPosition = Vector2.zero;

            // Idle bloom behind the CTA (the brightest interactive element after the selected card — §M).
            if (affordable)
            {
                var glow = UiWidgets.Glow(cta, UiTheme.A(Hex("#5f8bff"), 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(640, 130), 1.7f);
                var pg = glow.gameObject.AddComponent<PulseGraphic>(); pg.target = glow; pg.min = 0.3f; pg.max = 0.6f; pg.period = 1.8f;
            }
            var body = cta.gameObject.AddComponent<Image>();
            // Cobalt #2f5fd6 with lighter top #5f8bff, dark bottom #1b367f (§G). Disabled = desaturated grey-blue.
            body.sprite = maxed ? UiTex.VGradient(Hex("#3a3f52"), Hex("#222637"), 32)
                : UiTex.VGradient(Hex("#5f8bff"), Hex("#1b367f"), 32);
            if (!affordable && !maxed) body.color = new Color(0.75f, 0.78f, 0.85f, 1f); // can't afford = dim
            var btn = cta.gameObject.AddComponent<Button>(); btn.targetGraphic = body;
            var cb = btn.colors; cb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); cb.pressedColor = new Color(0.85f, 0.85f, 0.9f, 1f); btn.colors = cb;
            var u2 = u; btn.onClick.AddListener(() => OnUpgrade(u2, body, cta));
            // Gold thin trim.
            var rim = UiWidgets.Rect("Rim", cta, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rImg = rim.gameObject.AddComponent<Image>(); rImg.raycastTarget = false; rImg.sprite = UiTex.Frame(UiTheme.A(UiTheme.GoldHi, maxed ? 0.4f : 1f), UiTheme.A(UiTheme.Gold, maxed ? 0.35f : 1f), UiTheme.GoldShadow, 48, 6); rImg.type = Image.Type.Sliced;

            // Label "UPGRADE — N" + inset gold-coin icon (§F: white text + dark stroke, gold icon).
            string label = maxed ? "MAX LEVEL" : "UPGRADE — " + u.Cost;
            var lbl = UiWidgets.Label(cta, UiTheme.Track(label, 1), 32, Vector2.zero, Vector2.one, new Vector2(maxed ? 0 : -20, 0), Vector2.zero, TextAnchor.MiddleCenter, maxed ? new Color(0.8f, 0.8f, 0.85f, 1f) : Color.white);
            lbl.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(UiTheme.StrokeDark, 0.9f);
            if (!maxed)
            {
                var coin = UiWidgets.Rect("CoinIcon", cta, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-44, 0), new Vector2(40, 40));
                var coinImg = coin.gameObject.AddComponent<Image>(); coinImg.raycastTarget = false; coinImg.sprite = UiTex.Disc(UiTheme.Gold, 48);
                var coinIn = UiWidgets.Rect("CoinInner", coin, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(24, 24));
                var coinInImg = coinIn.gameObject.AddComponent<Image>(); coinInImg.raycastTarget = false; coinInImg.sprite = UiTex.Disc(UiTheme.GoldHi, 48);
            }
        }

        // ---- Bottom bar (band y 0.025→0.085, spans grid width): Rarity · Collection · Disenchant (§E). ----
        private void BuildBottomBar()
        {
            float xL = 0.018f, fy = 0.055f, ay = 1f - fy;
            float each = 0.195f, gap = 0.012f;
            BottomButton("Rarity",     xL + each * 0.5f, ay, Hex("#d9c79a"), () => Router.Toast("Rarity legend — coming soon"));
            BottomButton("Collection", xL + each * 1.5f + gap, ay, Hex("#d9c79a"), () => Router.Toast("Collection — coming soon"));
            BottomButton("Disenchant", xL + each * 2.5f + gap * 2f, ay, Hex("#c8a6ff"), () => Router.Toast("Disenchant — server-authoritative (coming soon)"), true);
        }

        private void BottomButton(string label, float cx, float ay, Color textCol, UnityEngine.Events.UnityAction onClick, bool violet = false)
        {
            float w = 0.195f * CW, h = 0.060f * CHt;
            var rt = UiWidgets.Rect("Btn_" + label, SafeContent, new Vector2(cx, ay), new Vector2(cx, ay), Vector2.zero, new Vector2(w, h));
            var img = rt.gameObject.AddComponent<Image>(); img.sprite = UiTex.VGradient(UiTheme.A(UiTheme.Charcoal, 0.95f), UiTheme.A(UiTheme.Obsidian, 0.95f), 32);
            var btn = rt.gameObject.AddComponent<Button>(); btn.targetGraphic = img;
            var cb = btn.colors; cb.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f); cb.pressedColor = new Color(0.85f, 0.85f, 0.9f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); onClick?.Invoke(); });
            var rim = UiWidgets.Rect("Rim", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rImg = rim.gameObject.AddComponent<Image>(); rImg.raycastTarget = false;
            rImg.sprite = UiTex.Frame(violet ? UiTheme.AmethystHi : UiTheme.A(UiTheme.GoldHi, 0.7f), violet ? UiTheme.Amethyst : UiTheme.A(UiTheme.Gold, 0.6f), UiTheme.GoldShadow, 48, 5);
            rImg.type = Image.Type.Sliced;
            // Leading icon glyph + label.
            var gly = UiWidgets.Rect("Glyph", rt, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(40, 0), new Vector2(36, 36));
            var gImg = gly.gameObject.AddComponent<Image>(); gImg.raycastTarget = false; gImg.sprite = UiTex.Diamond(violet ? UiTheme.AmethystHi : UiTheme.GoldHi, 48);
            UiWidgets.Label(rt, label, 22, new Vector2(0, 0), new Vector2(1, 1), new Vector2(36, 0), Vector2.zero, TextAnchor.MiddleCenter, textCol);
        }

        // =====================================================================================================
        // INTERACTION (display-only; §12: no balance/ECS mutation — requests/toasts only)
        // =====================================================================================================
        private void SwapFaction(int faction)
        {
            if (faction == _faction) return;
            _faction = faction;
            _roster = faction == 0 ? IronPact : AshenHorde;
            _filter = 0;          // reset filter to All (§K)
            _selected = FirstOwned();
            RebuildAll();
        }

        private void SetFilter(int filter)
        {
            if (filter == _filter) return;
            _filter = filter;
            // Keep selection if still visible, else select first match (§K).
            if (_filter != 0 && _roster[_selected].Klass != FilterKeys[_filter]) _selected = FirstMatch();
            RebuildAll();
        }

        private void SelectCard(int rosterIndex)
        {
            if (!_roster[rosterIndex].Owned) { Router.Toast(_roster[rosterIndex].Name + " — how to unlock (coming soon)"); return; }
            if (rosterIndex == _selected) return;
            _selected = rosterIndex;
            PopulateGrid();    // refresh halos (select cross-fade stand-in — §I)
            PopulateDetail();
        }

        private void StepSelection(int dir)
        {
            // Move within the current filtered + owned list (wraps) (§K OnPrev/Next).
            int count = 0;
            for (int i = 0; i < _roster.Length; i++)
                if (IsVisibleOwned(i)) count++;
            if (count == 0) return;
            // Find the ordinal of the current selection within the visible-owned sequence.
            int cur = 0, seen = 0;
            for (int i = 0; i < _roster.Length; i++)
            {
                if (!IsVisibleOwned(i)) continue;
                if (i == _selected) { cur = seen; break; }
                seen++;
            }
            int next = (cur + dir + count) % count;
            // Walk to the next-th visible-owned roster index.
            int hit = 0;
            for (int i = 0; i < _roster.Length; i++)
            {
                if (!IsVisibleOwned(i)) continue;
                if (hit == next) { SelectCard(i); return; }
                hit++;
            }
        }

        private bool IsVisibleOwned(int i)
            => _roster[i].Owned && (_filter == 0 || _roster[i].Klass == FilterKeys[_filter]);

        private void OnUpgrade(UnitDef u, Image body, RectTransform cta)
        {
            bool maxed = u.Level >= u.MaxLevel || u.Cost <= 0;
            if (maxed) { Router.Toast(u.Name + " is at max level"); return; }
            if (UiStub.Gold < u.Cost)
            {
                // Disabled-shake + toast; routes to Store via chip + (§H). NO local balance mutation.
                StartCoroutine(ShakeRect(cta, 14f, 0.3f));
                Router.Toast("Not enough Gold");
                return;
            }
            // Server-authoritative request stand-in (§K/§12): we DO NOT mutate the balance or stats here.
            AudioManager.Instance?.Click();
            Router.Toast("Upgrade requested — " + u.Name + " (server-authoritative)");
        }

        // =====================================================================================================
        private void RebuildAll()
        {
            // Rebuild faction tabs + filter strip + counter (selection-styling refresh) and grid + detail.
            for (int i = SafeContent.childCount - 1; i >= 0; i--)
            {
                var n = SafeContent.GetChild(i).name;
                if (n.StartsWith("Tab_") || n.StartsWith("Filter_")) Object.Destroy(SafeContent.GetChild(i).gameObject);
            }
            BuildFactionTabs();
            BuildFilterStrip();
            PopulateGrid();
            PopulateDetail();
        }

        private int FirstOwned() { for (int i = 0; i < _roster.Length; i++) if (_roster[i].Owned) return i; return 0; }
        private int FirstMatch()
        {
            for (int i = 0; i < _roster.Length; i++)
                if (_roster[i].Owned && (_filter == 0 || _roster[i].Klass == FilterKeys[_filter])) return i;
            return FirstOwned();
        }

        private void StaggerIn(RectTransform card, int gridIndex)
        {
            // Card stagger-in: scale 0.92→1.0 + fade, left→right (§I). Coroutine on this screen (no new MB type).
            var cg = card.gameObject.AddComponent<CanvasGroup>(); cg.alpha = 0f;
            card.localScale = Vector3.one * 0.92f;
            StartCoroutine(StaggerCard(card, cg, 0.12f + gridIndex * 0.018f, 0.18f));
        }

        // ---- Rarity tint lookup (§B/§G). ----
        private static Color RarityColor(Rarity r)
        {
            switch (r)
            {
                case Rarity.Common:    return Hex("#9aa3ad");
                case Rarity.Uncommon:  return Hex("#4caf50");
                case Rarity.Rare:      return Hex("#3d7fe0");
                case Rarity.Epic:      return Hex("#9e6bf0");
                case Rarity.Legendary: return Hex("#f0a93a");
                default:               return Hex("#9aa3ad");
            }
        }

        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }

        // ---- One-shot presentation-only tweens (unscaled-time; §I/§H). Coroutines on THIS screen — NO new
        //      MonoBehaviour types. Every frame guards its (Unity-null-checked) target in case the rebuilt grid /
        //      detail destroyed it mid-tween. Purely cosmetic; never touches balance/ECS (§12). ----
        private static IEnumerator FadeGroup(CanvasGroup group, float duration)
        {
            float t = 0f;
            while (group != null && t < duration)
            {
                t += Time.unscaledDeltaTime;
                group.alpha = Mathf.Clamp01(t / Mathf.Max(0.01f, duration));
                yield return null;
            }
            if (group != null) group.alpha = 1f;
        }

        private static IEnumerator StaggerCard(RectTransform card, CanvasGroup group, float delay, float duration)
        {
            float t = 0f;
            while (card != null && t < delay) { t += Time.unscaledDeltaTime; yield return null; }
            t = 0f;
            while (card != null && t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / Mathf.Max(0.01f, duration));
                float eased = 1f - Mathf.Pow(1f - k, 3f);                 // ease-out
                float scale = Mathf.Lerp(0.92f, 1.0f, eased) + Mathf.Sin(k * Mathf.PI) * 0.02f; // overshoot ~1.02
                card.localScale = Vector3.one * scale;
                if (group != null) group.alpha = k;
                yield return null;
            }
            if (card != null) card.localScale = Vector3.one;
            if (group != null) group.alpha = 1f;
        }

        private static IEnumerator FillBar(Image fill, RectTransform track, RectTransform cap, float target, float duration)
        {
            float t = 0f;
            while (fill != null && track != null && t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / Mathf.Max(0.01f, duration));
                float amt = Mathf.Lerp(0f, target, 1f - Mathf.Pow(1f - k, 2f)); // ease-out
                fill.fillAmount = amt;
                if (cap != null) cap.anchoredPosition = new Vector2(track.rect.width * amt, 0f);
                yield return null;
            }
            if (fill != null) fill.fillAmount = target;
            if (cap != null && track != null) cap.anchoredPosition = new Vector2(track.rect.width * target, 0f);
        }

        private static IEnumerator ShakeRect(RectTransform rt, float amplitude, float duration)
        {
            if (rt == null) yield break;
            Vector2 baseP = rt.anchoredPosition;
            float t = 0f;
            while (rt != null && t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = 1f - Mathf.Clamp01(t / duration);
                rt.anchoredPosition = baseP + new Vector2(Mathf.Sin(t * 60f) * amplitude * k, 0f);
                yield return null;
            }
            if (rt != null) rt.anchoredPosition = baseP;
        }
    }
}
