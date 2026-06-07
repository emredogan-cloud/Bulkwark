// BULWARK — CAMPAIGN MAP (UI Construction Bible · 27). Presentation-only, REMOVABLE.
//
// Forensic build of design/CampaignMapDesign.png per Bible §27 A–O: a full-bleed painted world map (left haunted
// swamp teal/green → central ruined castle stone-grey → right volcanic lava ember-red) with a glowing antique-gold
// path snaking bottom-left → right, studded with numbered level nodes (1,2,3,4,5,6,6,7,9,10,11,12,12 — transcribed
// EXACTLY as drawn incl. the skipped-8 / repeated-6&12 anomaly, flagged-not-fixed §L) each crowned by 1–3 gold
// stars (6b = 2/3); node 7 is the cobalt CURRENT node (cyan rim + energy plume + Iron-Pact hero standee, the
// brightest focal point); a far-right steel padlock locked node. Overlay chrome (inside safe area): top-left back +
// NORMAL(selected)/HARD difficulty toggle, top-right Gems 2,340 / Silver 12,450 / Gold 1,850 chips, bottom-left
// chest + "36/39" star total, bottom-right Heroes/Rewards/Quests nav. Tapping an unlocked node opens a centred
// LevelDetailPopup (earned stars + reward preview + cobalt PLAY); locked nodes toast the unlock requirement. Runs
// the §I enter ceremony (path-draw → node-pop stagger → current-node bloom → chrome slide → star count-up). §12:
// progression is read-only display-only local stub data; PLAY is a meta navigation request through the existing
// §12-safe MatchPresentation seam; NO ECS, NO gameplay/balance/economy/progress mutation, NO Unity.Entities.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bulwark.Bootstrap
{
    /// <summary>Bible-27 landscape campaign map / level-select. Presentation-only (node data display-only stubs, §12).</summary>
    public sealed class CampaignMapScreen : UiScreen
    {
        // ============================ DISPLAY-ONLY STUB DATA (§12: read-only; NOT progression/balance) ============================
        // Per §K these arrive read-only from a server-authoritative campaign-progress query; here they are inert local
        // constants matching the mock. NO progress/star count is ever written. Numbers/names imply no economy/balance.
        private struct CampNode
        {
            public int Display;     // numeral as DRAWN (note: 1,2,3,4,5,6,6,7,9,10,11,12,12 — skips 8, repeats 6 & 12)
            public float Fx, Fy;    // fraction of 2340×1080 (Fy measured from TOP — converted to anchorY = 1 − Fy)
            public int Stars;       // earned gold stars 0..3 (display-only)
            public bool Current;    // node 7 = cobalt CURRENT focal node (hero standee + plume)
            public bool Locked;     // far-right padlock node (no popup PLAY → toast unlock requirement)
        }

        // Spec §E node table (fractions read off the painting; tune to final art). Order is the PATH ORDER 1→…→locked.
        private static readonly CampNode[] Nodes =
        {
            new CampNode{ Display=1,  Fx=0.115f, Fy=0.135f, Stars=3 },
            new CampNode{ Display=2,  Fx=0.230f, Fy=0.155f, Stars=3 },
            new CampNode{ Display=3,  Fx=0.330f, Fy=0.180f, Stars=3 },
            new CampNode{ Display=4,  Fx=0.315f, Fy=0.300f, Stars=3 },
            new CampNode{ Display=5,  Fx=0.290f, Fy=0.405f, Stars=3 },
            new CampNode{ Display=6,  Fx=0.270f, Fy=0.560f, Stars=3 }, // 6a
            new CampNode{ Display=6,  Fx=0.310f, Fy=0.700f, Stars=2 }, // 6b — shows 2/3
            new CampNode{ Display=7,  Fx=0.385f, Fy=0.640f, Stars=0, Current=true }, // CURRENT (hero)
            new CampNode{ Display=9,  Fx=0.500f, Fy=0.470f, Stars=3 },
            new CampNode{ Display=10, Fx=0.570f, Fy=0.390f, Stars=3 },
            new CampNode{ Display=11, Fx=0.640f, Fy=0.255f, Stars=3 },
            new CampNode{ Display=12, Fx=0.730f, Fy=0.460f, Stars=3 }, // 12a
            new CampNode{ Display=12, Fx=0.800f, Fy=0.470f, Stars=3 }, // 12b
            new CampNode{ Display=0,  Fx=0.880f, Fy=0.420f, Stars=0, Locked=true }, // far-right padlock
        };

        // Star total (§A/L) — DISPLAY ONLY. 36 of 39 stars. Counts up in the §I ceremony.
        private const int StarsCollected = 36;
        private const int StarsPossible   = 39;

        // Currency chip values (§A/L) — DISPLAY ONLY (three chips: Gems + Silver + Gold).
        private const int GemsValue   = 2340;
        private const int SilverValue = 12450;
        private const int GoldValue   = 1850;

        // Node geometry (§E): ring Ø ≈ 0.040·W (≈94px@1080); current node ×1.4; stars ~0.045·H above the ring.
        private const float RingD       = 94f;   // 0.040 * 2340
        private const float CurrentMul  = 1.4f;
        private const float StarAbove   = 0.045f * 1080f; // ≈49px above ring centre

        // ---- Forensic hexes not in UiTheme (§B/F/G ranges). ----
        private static readonly Color SwampDeep   = Hex("#16302a"); // left haunted swamp / forest (cool teal-green)
        private static readonly Color SwampGlow   = Hex("#2f6e4a"); // eerie teal wisp glow
        private static readonly Color StoneGrey   = Hex("#2a2c30"); // central ruined-castle cold stone
        private static readonly Color LavaDeep    = Hex("#7a1f0a"); // right volcanic oxblood rock
        private static readonly Color LavaHot     = Hex("#f0742c"); // lava / ember haze
        private static readonly Color PathGold    = Hex("#caa04a"); // golden path body
        private static readonly Color PathGoldHi  = Hex("#f0d27a"); // golden path highlight
        private static readonly Color CobaltDeep  = Hex("#2b56c8"); // current node cobalt
        private static readonly Color CobaltHi    = Hex("#4f8bff"); // current node bright cobalt / cyan rim
        private static readonly Color SteelRing   = Hex("#5a626c"); // locked node desaturated steel ring
        private static readonly Color NumGold     = Hex("#fff4d6"); // node numerals
        private static readonly Color CurNumWhite = Hex("#ffffff"); // current node "7"
        private static readonly Color DiffSel     = Hex("#f0d27a"); // NORMAL selected gold
        private static readonly Color DiffIdle    = Hex("#9aa3ad"); // HARD idle dim
        private static readonly Color CurrencyTxt = Hex("#f4e9c8"); // currency values
        private static readonly Color StarTotGold = Hex("#f0d27a"); // "36/39"
        private static readonly Color NavLabel    = Hex("#e8e2cf"); // bottom-nav labels
        private static readonly Color PopupTitle  = Hex("#f0d27a"); // popup "Level N"
        private static readonly Color PlayWhite   = Hex("#ffffff"); // popup "PLAY"
        private static readonly Color StrokeDark  = Hex("#100b04"); // thin dark stroke for over-art legibility
        private static readonly Color SilverCoin  = Hex("#9aa0a8"); // silver-coin chip icon
        private static readonly Color GemViolet   = Hex("#9e6bf0"); // gem chip icon

        // ---- Ceremony handles (driven by Reveal / snapped by skip). ----
        private RectTransform _nodeLayer, _pathLayer;
        private readonly System.Collections.Generic.List<RectTransform> _nodeRoots = new System.Collections.Generic.List<RectTransform>(16);
        private readonly System.Collections.Generic.List<RectTransform> _starRows  = new System.Collections.Generic.List<RectTransform>(16);
        private RectTransform _heroStandee, _heroPlume, _currentGlow;
        private CanvasGroup _chromeCg, _navCg, _starTotalCg;
        private Text _starTotalText;
        private CountUp _starCounter;

        // ---- LevelDetailPopup handles ----
        private RectTransform _popup;
        private CanvasGroup _popupCg, _popupPanelCg;
        private Text _popupTitle;
        private RectTransform _popupStars;
        private bool _revealed; // enter-ceremony skip guard

        // ============================ BUILD ============================
        protected override void Build()
        {
            BuildMapBody();   // §C MapBody_FullBleed → Map_Art + PathLayer + NodeLayer (full-bleed under cutout, on Rect)
            BuildPath();      // §C PathLayer — glowing golden path connecting nodes 1→…→locked in order
            BuildNodes();     // §C NodeLayer — all level nodes (rings/numbers/stars/current/locked)
            BuildChrome();    // §C SafeAreaRoot overlay chrome (back + difficulty + currency chips)
            BuildStarTotal(); // §C StarTotal (chest + "36/39") bottom-left
            BuildBottomNav(); // §C BottomNav (Heroes / Rewards / Quests) bottom-right
            BuildPopup();     // §C LevelDetailPopup (hidden until a node is tapped)
        }

        // ---- MapBody_FullBleed: the painted world map. Full-bleed on Rect so the art bleeds under the cutout (§D/E).
        //      Terrain reads left→right: swamp/forest (teal-green) → ruined castle (stone) → volcanic lava (ember). ----
        private void BuildMapBody()
        {
            // Base painting stand-in: horizontal terrain gradient swamp → stone → lava (placeholder for the hero art).
            var baseImg = UiWidgets.Stretch("Map_Art", Rect, StoneGrey, "bg_battle");
            baseImg.color = new Color(1f, 1f, 1f, 0.55f); // let the terrain washes read over the placeholder
            // Left swamp wash (teal-green, fades toward centre).
            var swamp = UiWidgets.Rect("Terrain_Swamp", Rect, new Vector2(0, 0), new Vector2(0.42f, 1), Vector2.zero, Vector2.zero);
            var swampImg = swamp.gameObject.AddComponent<Image>(); swampImg.raycastTarget = false;
            swampImg.sprite = UiTex.HGradient(UiTheme.A(SwampDeep, 0.92f), UiTheme.A(SwampDeep, 0f), 64);
            // Central ruined-castle cold stone (subtle vertical stone wash, blue moon-glow on top).
            var stone = UiWidgets.Rect("Terrain_Stone", Rect, new Vector2(0.34f, 0), new Vector2(0.66f, 1), Vector2.zero, Vector2.zero);
            var stoneImg = stone.gameObject.AddComponent<Image>(); stoneImg.raycastTarget = false;
            stoneImg.sprite = UiTex.VGradient(UiTheme.A(Hex("#3a4452"), 0.55f), UiTheme.A(StoneGrey, 0.85f), 64);
            // Right volcanic wastes (lava cracks deep oxblood → ember haze, fades toward centre).
            var lava = UiWidgets.Rect("Terrain_Lava", Rect, new Vector2(0.58f, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            var lavaImg = lava.gameObject.AddComponent<Image>(); lavaImg.raycastTarget = false;
            lavaImg.sprite = UiTex.HGradient(UiTheme.A(LavaDeep, 0f), UiTheme.A(LavaDeep, 0.9f), 64);

            // §J terrain ambient — swamp wisps/fireflies (left, teal) + lava embers + heat haze (right).
            var swampWisps = UiWidgets.Rect("Swamp_Wisps", Rect, new Vector2(0.06f, 0.18f), new Vector2(0.06f, 0.18f), Vector2.zero, new Vector2(640, 700));
            var sw = swampWisps.gameObject.AddComponent<EmberField>(); sw.count = 9; sw.color = UiTheme.A(SwampGlow, 0.7f);
            UiWidgets.Glow(Rect, UiTheme.A(SwampGlow, 0.22f), new Vector2(0.12f, 0.30f), new Vector2(0.12f, 0.30f), Vector2.zero, new Vector2(900, 900), 1.6f);
            var lavaEmbers = UiWidgets.Rect("Lava_Embers", Rect, new Vector2(0.85f, 0.20f), new Vector2(0.85f, 0.20f), Vector2.zero, new Vector2(680, 760));
            var le = lavaEmbers.gameObject.AddComponent<EmberField>(); le.count = 12; le.color = UiTheme.A(LavaHot, 0.8f);
            UiWidgets.Glow(Rect, UiTheme.A(LavaHot, 0.24f), new Vector2(0.9f, 0.28f), new Vector2(0.9f, 0.28f), Vector2.zero, new Vector2(1000, 1000), 1.6f);

            // Strong vignette toward the edges (§B/G) — the painting is low-key with luminous focal nodes.
            UiWidgets.Vignette(Rect, 0.6f);

            // NodeLayer + PathLayer parented to a full-bleed container so nodes anchor by fraction TO THE ART (§D/E/M-8).
            var mapBody = UiWidgets.Rect("NodeLayer_Root", Rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _pathLayer = UiWidgets.Rect("PathLayer", mapBody, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _nodeLayer = UiWidgets.Rect("NodeLayer", mapBody, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        }

        // ---- PathLayer: a glowing antique-gold ribbon connecting the nodes in number order 1→…→locked (§E/G/J).
        //      Drawn as a chain of thin rotated gold segments (a light glint sweeps along it in the §I ceremony). ----
        private void BuildPath()
        {
            for (int i = 0; i < Nodes.Length - 1; i++)
            {
                Vector2 a = AnchorOf(Nodes[i]);
                Vector2 b = AnchorOf(Nodes[i + 1]);
                bool nearCurrent = Nodes[i].Current || Nodes[i + 1].Current; // brighter segment near the current node (§G)
                PathSegment(a, b, nearCurrent, i);
            }
        }

        // A single gold path segment between two fractional anchors (positioned/rotated in px on the full-bleed layer).
        private void PathSegment(Vector2 aFrac, Vector2 bFrac, bool bright, int order)
        {
            // Convert fractional anchors → px (canvas 2340×1080) for length/rotation.
            Vector2 aPx = new Vector2(aFrac.x * 2340f, aFrac.y * 1080f);
            Vector2 bPx = new Vector2(bFrac.x * 2340f, bFrac.y * 1080f);
            Vector2 mid = (aFrac + bFrac) * 0.5f;
            Vector2 delta = bPx - aPx;
            float len = delta.magnitude;
            float ang = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

            // soft outer bloom under the ribbon.
            var glowRt = UiWidgets.Rect("PathGlow_" + order, _pathLayer, mid, mid, Vector2.zero, new Vector2(len + 40f, 46f));
            glowRt.localRotation = Quaternion.Euler(0, 0, ang);
            var gimg = glowRt.gameObject.AddComponent<Image>(); gimg.raycastTarget = false;
            gimg.sprite = UiTex.VGradient(UiTheme.A(bright ? PathGoldHi : PathGold, 0f), UiTheme.A(bright ? PathGoldHi : PathGold, 0.35f), 32);

            // the ribbon itself (worn-gold body, brighter near the current node).
            var seg = UiWidgets.Rect("PathSeg_" + order, _pathLayer, mid, mid, Vector2.zero, new Vector2(len, 18f));
            seg.localRotation = Quaternion.Euler(0, 0, ang);
            var img = seg.gameObject.AddComponent<Image>(); img.raycastTarget = false;
            img.sprite = UiTex.VGradient(bright ? PathGoldHi : PathGold, UiWidgets.Darken(PathGold, 0.35f), 32);

            // a travelling glint highlight on the ribbon (slow loop ~ §J 6s; sheen slides along the segment).
            var glintGo = UiWidgets.Rect("PathGlint", seg, new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, new Vector2(60f, 0f));
            var glImg = glintGo.gameObject.AddComponent<Image>(); glImg.raycastTarget = false;
            glImg.sprite = UiTex.HGradient(UiTheme.A(Color.white, 0f), UiTheme.A(Color.white, 0.5f), 32);
            var sheen = seg.gameObject.AddComponent<Sheen>(); sheen.target = glintGo; sheen.minX = -len * 0.5f; sheen.maxX = len * 0.5f; sheen.period = 6f;
        }

        // ---- NodeLayer: every level node positioned by fraction on the art. Completed = gold ring + numeral + stars;
        //      current (7) = cobalt glow + cyan rim + hero standee + plume; locked = steel ring + padlock (§C/E/H). ----
        private void BuildNodes()
        {
            for (int i = 0; i < Nodes.Length; i++) BuildNode(Nodes[i]);
        }

        private void BuildNode(CampNode n)
        {
            Vector2 anchor = AnchorOf(n);
            float d = n.Current ? RingD * CurrentMul : RingD;

            // Node root = a Button (whole node clickable; §D Node_* = Button). Transparent hit disc sized to the ring.
            var node = UiWidgets.Rect("Node_" + (n.Locked ? "Locked" : n.Display.ToString()), _nodeLayer, anchor, anchor, Vector2.zero, new Vector2(d, d));
            var hit = node.gameObject.AddComponent<Image>(); hit.sprite = UiTex.Disc(new Color(0, 0, 0, 0.001f), 48); // raycast surface
            var btn = node.gameObject.AddComponent<Button>(); btn.targetGraphic = hit;
            var cb = btn.colors; cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f); cb.pressedColor = new Color(0.95f, 0.95f, 0.95f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb; // hover +15% / press 0.95 (§H)
            var cap = n; // capture
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); OnNodeTap(cap); });
            _nodeRoots.Add(node);

            if (n.Current)
            {
                // CURRENT node (7): the map's brightest element — cobalt glow + cyan rim + plume + hero standee (§G/J).
                _currentGlow = UiWidgets.Glow(node, UiTheme.A(CobaltHi, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(d * 2.4f, d * 2.4f), 1.5f).rectTransform;
                var pulse = _currentGlow.gameObject.AddComponent<PulseGraphic>(); pulse.target = _currentGlow.GetComponent<Image>(); pulse.min = 0.45f; pulse.max = 0.85f; pulse.period = 1.8f; // beacon pulse (§J)
                // cobalt ring body.
                AddRingBody(node, d, UiTex.VGradient(CobaltHi, CobaltDeep, 32));
                // cyan beveled rim over the ring.
                AddRingBevel(node, CobaltHi, CobaltHi, CobaltDeep);
                // vertical blue energy plume rising from the node (§G/J).
                _heroPlume = UiWidgets.Rect("Plume", node, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, d * 0.9f), new Vector2(d * 0.5f, d * 1.6f));
                var plumeImg = _heroPlume.gameObject.AddComponent<Image>(); plumeImg.raycastTarget = false;
                plumeImg.sprite = UiTex.VGradient(UiTheme.A(CobaltHi, 0.7f), UiTheme.A(CobaltHi, 0f), 32);
                var plumeFx = _heroPlume.gameObject.AddComponent<PulseGraphic>(); plumeFx.target = plumeImg; plumeFx.min = 0.4f; plumeFx.max = 0.85f; plumeFx.period = 1.4f;
                var motes = _heroPlume.gameObject.AddComponent<EmberField>(); motes.count = 6; motes.color = UiTheme.A(CobaltHi, 0.9f); // rising spark motes (§J)
                // small Iron-Pact hero standee on the node (diamond stand-in over a cobalt under-glow).
                _heroStandee = UiWidgets.Rect("HeroStandee", node, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, d * 0.18f), new Vector2(d * 0.5f, d * 0.78f));
                var heroImg = _heroStandee.gameObject.AddComponent<Image>(); heroImg.raycastTarget = false; heroImg.sprite = UiTex.Diamond(UiTheme.IronSteel, 48);
                var heroHead = UiWidgets.Rect("Head", _heroStandee, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -d * 0.14f), new Vector2(d * 0.26f, d * 0.26f));
                var hh = heroHead.gameObject.AddComponent<Image>(); hh.raycastTarget = false; hh.sprite = UiTex.Disc(UiTheme.GoldHi, 48);
                // current numeral "7" — black-weight white over cobalt with a dark stroke (§F).
                var num = UiWidgets.Label(node, n.Display.ToString(), 34, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(d, d), TextAnchor.MiddleCenter, CurNumWhite);
                var nol = num.gameObject.AddComponent<Outline>(); nol.effectColor = UiTheme.A(StrokeDark, 0.95f); nol.effectDistance = new Vector2(2f, -2f);
            }
            else if (n.Locked)
            {
                // LOCKED node: desaturated steel ring + gold padlock, no glow (§G/H).
                AddRingBody(node, d, UiTex.VGradient(UiWidgets.Lighten(SteelRing, 0.1f), UiWidgets.Darken(SteelRing, 0.35f), 32));
                AddRingBevel(node, UiWidgets.Lighten(SteelRing, 0.3f), SteelRing, UiWidgets.Darken(SteelRing, 0.4f));
                // padlock: a small gold body + shackle.
                var lockBody = UiWidgets.Rect("Lock_Body", node, new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), Vector2.zero, new Vector2(d * 0.42f, d * 0.34f));
                var lb = lockBody.gameObject.AddComponent<Image>(); lb.raycastTarget = false; lb.sprite = UiTex.VGradient(UiTheme.GoldHi, UiTheme.Gold, 32);
                var shackle = UiWidgets.Rect("Lock_Shackle", node, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(d * 0.26f, d * 0.26f));
                var sk = shackle.gameObject.AddComponent<Image>(); sk.raycastTarget = false; sk.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 7); sk.type = Image.Type.Sliced;
            }
            else
            {
                // COMPLETED / available node: lit cast-gold ring, dark centre, gold numeral, gentle invite pulse (§H).
                AddRingBody(node, d, UiTex.VGradient(UiTheme.A(UiTheme.Obsidian, 0.92f), UiTheme.A(Hex("#241a08"), 0.92f), 32));
                AddRingBevel(node, UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow);
                UiWidgets.Glow(node, UiTheme.A(UiTheme.GoldHi, 0.28f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(d * 1.5f, d * 1.5f), 1.7f);
                node.gameObject.AddComponent<PulseScale>().period = 3f + (n.Display % 4) * 0.35f; // phase-offset breathing
                var num = UiWidgets.Label(node, n.Display.ToString(), 30, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(d, d), TextAnchor.MiddleCenter, NumGold);
                var nol = num.gameObject.AddComponent<Outline>(); nol.effectColor = UiTheme.A(StrokeDark, 0.95f); nol.effectDistance = new Vector2(2f, -2f);
            }

            // StarRow above the ring (up to 3 gold pips; earned bright vs missing dim) — use UiWidgets.StarRating (§task).
            // Locked/current nodes carry no star row.
            if (!n.Locked && !n.Current)
            {
                const float starSize = 26f, gap = 6f;
                float rowW = 3 * starSize + 2 * gap;
                var row = UiWidgets.Rect("StarRow_" + n.Display, node, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-rowW * 0.5f, d * 0.5f + StarAbove), new Vector2(rowW, starSize));
                UiWidgets.StarRating(row, new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, 3, n.Stars, starSize, gap);
                _starRows.Add(row);
            }
            else _starRows.Add(null);
        }

        // Ring body disc (dark/cobalt/steel centre fill).
        private Image AddRingBody(Transform node, float d, Sprite fill)
        {
            var body = UiWidgets.Rect("Node_Ring", node, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(d, d));
            var img = body.gameObject.AddComponent<Image>(); img.raycastTarget = false; img.sprite = fill;
            return img;
        }

        // Beveled ring rim (9-slice gold/cobalt/steel molding over the ring edge).
        private void AddRingBevel(Transform node, Color hi, Color mid, Color shadow)
        {
            var rim = UiWidgets.Rect("Ring_Bevel", node, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var img = rim.gameObject.AddComponent<Image>(); img.raycastTarget = false;
            img.sprite = UiTex.Frame(hi, mid, shadow, 48, 8); img.type = Image.Type.Sliced;
        }

        // ---- TopLeftChrome + CurrencyChips (§C/D/E). Back chevron, NORMAL/HARD difficulty toggle, three currency chips. ----
        private void BuildChrome()
        {
            var grpGo = new GameObject("Chrome_Group", typeof(RectTransform), typeof(CanvasGroup));
            var grp = (RectTransform)grpGo.transform; grp.SetParent(SafeContent, false);
            grp.anchorMin = Vector2.zero; grp.anchorMax = Vector2.one; grp.offsetMin = Vector2.zero; grp.offsetMax = Vector2.zero;
            _chromeCg = grpGo.GetComponent<CanvasGroup>();

            // Btn_Back (top-left chevron) → Pop to Main Menu (§K OnBack).
            UiWidgets.BackButton(grp, () => Router.Pop());

            // DifficultyToggle (single-select): NORMAL (selected, gold-lit) at fy 0.840 / HARD (idle, dim) at fy 0.760.
            DifficultyChip("NORMAL", 0.840f, true);
            DifficultyChip("HARD",   0.760f, false);

            // CurrencyChips (top-right): Gold rightmost (idx 0), Silver (idx 1), Gems (idx 2) — three chips (§L).
            // Recolor each value to the §F tabular-currency hex (#f4e9c8) over the builder's default white.
            UiWidgets.CurrencyChip(grp, UiTheme.Gold, GoldValue,   0, out _).color = CurrencyTxt;
            UiWidgets.CurrencyChip(grp, SilverCoin,   SilverValue, 1, out _).color = CurrencyTxt;
            UiWidgets.CurrencyChip(grp, GemViolet,    GemsValue,   2, out _).color = CurrencyTxt;
        }

        // A stacked difficulty toggle chip: icon over an UPPERCASE label; selected = gold-lit frame, idle = dim (§E/H).
        private void DifficultyChip(string label, float fy, bool selected)
        {
            float anchorY = 1f - fy;
            var rt = UiWidgets.Rect("Diff_" + label, _chromeCg.transform, new Vector2(0.030f, anchorY), new Vector2(0.030f, anchorY), Vector2.zero, new Vector2(0.060f * 2340f, 0.070f * 1080f));
            var bg = rt.gameObject.AddComponent<Image>();
            bg.sprite = UiTex.VGradient(UiTheme.A(UiTheme.Charcoal, selected ? 0.95f : 0.7f), UiTheme.A(UiTheme.Obsidian, selected ? 0.95f : 0.7f), 32);
            var btn = rt.gameObject.AddComponent<Button>(); btn.targetGraphic = bg;
            var cb = btn.colors; cb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); cb.pressedColor = new Color(0.9f, 0.9f, 0.94f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
            string lbl = label;
            // OnDifficulty reloads node states per difficulty (§K); progress is read-only → presentation toast here.
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); Router.Toast(lbl + " — campaign difficulty (progress is read-only)"); });
            // gold-lit rim when selected, dim when idle.
            var rim = UiWidgets.Rect("Rim", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rimImg = rim.gameObject.AddComponent<Image>(); rimImg.raycastTarget = false; rimImg.type = Image.Type.Sliced;
            rimImg.sprite = selected ? UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 6)
                                     : UiTex.Frame(UiTheme.A(DiffIdle, 0.5f), UiTheme.A(DiffIdle, 0.35f), UiTheme.A(UiTheme.Obsidian, 0.6f), 48, 5);
            // icon (skull/swords stand-in) over the label.
            var icon = UiWidgets.Rect("Icon", rt, new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.68f), Vector2.zero, new Vector2(36, 36));
            var iimg = icon.gameObject.AddComponent<Image>(); iimg.raycastTarget = false; iimg.sprite = UiTex.Diamond(selected ? DiffSel : DiffIdle, 48);
            // label (UPPERCASE, tracked +6%, dark stroke).
            var t = UiWidgets.Label(rt, UiTheme.Track(label), 18, new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), Vector2.zero, new Vector2(0.060f * 2340f, 30f), TextAnchor.MiddleCenter, selected ? DiffSel : DiffIdle);
            t.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(StrokeDark, 0.9f);
        }

        // ---- StarTotal (bottom-left): gold chest icon + "36/39" (counts up in the §I ceremony) (§C/E/F). ----
        private void BuildStarTotal()
        {
            var grpGo = new GameObject("StarTotal", typeof(RectTransform), typeof(CanvasGroup));
            var grp = (RectTransform)grpGo.transform; grp.SetParent(SafeContent, false);
            grp.anchorMin = grp.anchorMax = new Vector2(0.020f, 0.045f); grp.anchoredPosition = Vector2.zero; grp.sizeDelta = new Vector2(360f, 90f);
            grp.pivot = new Vector2(0f, 0.5f);
            _starTotalCg = grpGo.GetComponent<CanvasGroup>();

            // gold chest icon (Ø ≈ 0.05·W).
            float chestD = 0.05f * 2340f;
            var chest = UiWidgets.Rect("Chest", grp, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(chestD * 0.5f, 0), new Vector2(chestD, chestD * 0.82f));
            var cimg = chest.gameObject.AddComponent<Image>(); cimg.raycastTarget = false; cimg.sprite = UiTex.VGradient(UiTheme.GoldHi, UiTheme.GoldShadow, 32);
            var chestLid = UiWidgets.Rect("Lid", chest, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -6), new Vector2(chestD * 0.92f, chestD * 0.26f));
            var lid = chestLid.gameObject.AddComponent<Image>(); lid.raycastTarget = false; lid.sprite = UiTex.VGradient(UiTheme.Gold, UiTheme.GoldShadow, 32);
            UiWidgets.Glow(grp, UiTheme.A(StarTotGold, 0.3f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(chestD * 0.5f, 0), new Vector2(chestD * 1.6f, chestD * 1.6f), 1.7f);

            // "36/39" gold-glow + dark-stroke star total to the right of the chest.
            _starTotalText = UiWidgets.Label(grp, StarsCollected + "/" + StarsPossible, 30, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(chestD + 18f, 0), new Vector2(220f, 60f), TextAnchor.MiddleLeft, StarTotGold);
            _starTotalText.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(StrokeDark, 0.9f);
            _starCounter = _starTotalText.gameObject.AddComponent<CountUp>();
            _starCounter.Bind(_starTotalText, v => v + "/" + StarsPossible, 0f);
        }

        // ---- BottomNav (bottom-right): Heroes / Rewards / Quests icon+label chips (§C/E/K). ----
        private void BuildBottomNav()
        {
            var grpGo = new GameObject("BottomNav", typeof(RectTransform), typeof(CanvasGroup));
            var grp = (RectTransform)grpGo.transform; grp.SetParent(SafeContent, false);
            // bottom-right, right edge x 0.972, y 0.060 (§E); pivot on the right edge so chips lay out leftward.
            grp.pivot = new Vector2(1f, 0.5f);
            grp.anchorMin = grp.anchorMax = new Vector2(0.972f, 0.060f);
            grp.anchoredPosition = Vector2.zero;
            grp.sizeDelta = new Vector2(0.075f * 3 * 2340f + 0.020f * 2 * 2340f, 0.13f * 1080f);
            _navCg = grpGo.GetComponent<CanvasGroup>();

            float chipW = 0.075f * 2340f, gap = 0.010f * 2340f;
            // right-aligned: Quests rightmost, then Rewards, then Heroes (left → right reads Heroes · Rewards · Quests).
            NavChip(grp, "Quests",  -(chipW * 0.5f),                     chipW, () => Router.Show<QuestsScreen>());
            NavChip(grp, "Rewards", -(chipW * 1.5f + gap),              chipW, () => Router.Toast("Rewards — coming soon"));
            NavChip(grp, "Heroes",  -(chipW * 2.5f + gap * 2f),          chipW, () => Router.Toast("Heroes — coming soon"));
        }

        // A small gold-rimmed dark nav chip: icon over a Title-case label (≈22px, +2%, dark stroke) (§F/G).
        private void NavChip(Transform grp, string label, float xFromRight, float chipW, UnityEngine.Events.UnityAction onClick)
        {
            float chipH = 0.11f * 1080f;
            var rt = UiWidgets.Rect("Nav_" + label, grp, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(xFromRight, 0), new Vector2(chipW, chipH));
            var bg = rt.gameObject.AddComponent<Image>(); bg.sprite = UiTex.VGradient(UiTheme.A(UiTheme.Charcoal, 0.95f), UiTheme.A(UiTheme.Obsidian, 0.95f), 32);
            var btn = rt.gameObject.AddComponent<Button>(); btn.targetGraphic = bg;
            var cb = btn.colors; cb.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f); cb.pressedColor = new Color(0.88f, 0.88f, 0.92f, 1f); cb.fadeDuration = 0.08f; btn.colors = cb;
            btn.onClick.AddListener(() => { AudioManager.Instance?.Click(); onClick?.Invoke(); });
            var rim = UiWidgets.Rect("Rim", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rimImg = rim.gameObject.AddComponent<Image>(); rimImg.raycastTarget = false; rimImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 6); rimImg.type = Image.Type.Sliced;
            // icon (helm/chest/scroll stand-in) over the label.
            var icon = UiWidgets.Rect("Icon", rt, new Vector2(0.5f, 0.66f), new Vector2(0.5f, 0.66f), Vector2.zero, new Vector2(chipH * 0.4f, chipH * 0.4f));
            var iimg = icon.gameObject.AddComponent<Image>(); iimg.raycastTarget = false; iimg.sprite = UiTex.Diamond(UiTheme.A(UiTheme.GoldHi, 0.9f), 48);
            // label (Title-case, dark stroke).
            var t = UiWidgets.Label(rt, label, 22, new Vector2(0.5f, 0.20f), new Vector2(0.5f, 0.20f), Vector2.zero, new Vector2(chipW, 30f), TextAnchor.MiddleCenter, NavLabel);
            t.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(StrokeDark, 0.85f);
        }

        // ============================ LEVEL DETAIL POPUP (§C/E/H — hidden until a node is tapped) ============================
        private void BuildPopup()
        {
            var go = new GameObject("LevelDetailPopup", typeof(RectTransform), typeof(CanvasGroup));
            _popup = (RectTransform)go.transform; _popup.SetParent(SafeContent, false);
            _popup.anchorMin = Vector2.zero; _popup.anchorMax = Vector2.one; _popup.offsetMin = Vector2.zero; _popup.offsetMax = Vector2.zero;
            _popupCg = go.GetComponent<CanvasGroup>();
            _popupCg.alpha = 0f; _popupCg.blocksRaycasts = false; _popupCg.interactable = false;

            // Popup_BG: full-bleed dim that also closes the popup on an outside tap.
            var dim = UiWidgets.Stretch("Popup_BG", _popup, UiTheme.A(UiTheme.Vignette, 0.7f));
            var dimBtn = dim.gameObject.AddComponent<Button>(); dimBtn.targetGraphic = dim;
            var dcb = dimBtn.colors; dcb.highlightedColor = Color.white; dcb.pressedColor = Color.white; dcb.fadeDuration = 0f; dimBtn.colors = dcb;
            dimBtn.onClick.AddListener(ClosePopup);

            // Popup_Panel: centred ornate modal ≈0.42·W × 0.52·H.
            float pw = 0.42f * 2340f, ph = 0.52f * 1080f;
            var panelGo = new GameObject("Popup_Panel", typeof(RectTransform), typeof(CanvasGroup));
            var panel = (RectTransform)panelGo.transform; panel.SetParent(_popup, false);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f); panel.anchoredPosition = Vector2.zero; panel.sizeDelta = new Vector2(pw, ph);
            _popupPanelCg = panelGo.GetComponent<CanvasGroup>();
            var field = UiWidgets.OrnateFrame(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(pw, ph), Hex("#0c0e14"), false, 22f);

            // Lbl_LevelTitle "Level N" (serif display, UPPERCASE, gold bevel + bloom).
            _popupTitle = UiWidgets.TitleLabel(field, "LEVEL 7", 40, new Vector2(0.5f, 0.86f), new Vector2(0.5f, 0.86f), Vector2.zero, new Vector2(pw * 0.8f, 80f), TextAnchor.MiddleCenter, Hex("#f7e7b0"), PopupTitle);

            // StarRow_Earned (up to 3) — rebuilt per node in OnNodeTap.
            _popupStars = UiWidgets.Rect("StarRow_Earned", field, new Vector2(0.5f, 0.66f), new Vector2(0.5f, 0.66f), Vector2.zero, new Vector2(pw * 0.6f, 70f));

            // RewardPreview (silver + gem reward icon stand-ins).
            var rewards = UiWidgets.Rect("RewardPreview", field, new Vector2(0.5f, 0.44f), new Vector2(0.5f, 0.44f), Vector2.zero, new Vector2(pw * 0.6f, 110f));
            var coin = UiWidgets.Rect("Reward_Silver", rewards, new Vector2(0.34f, 0.5f), new Vector2(0.34f, 0.5f), Vector2.zero, new Vector2(80, 80));
            var coinImg = coin.gameObject.AddComponent<Image>(); coinImg.raycastTarget = false; coinImg.sprite = UiTex.Disc(SilverCoin, 48);
            var gem = UiWidgets.Rect("Reward_Gem", rewards, new Vector2(0.64f, 0.5f), new Vector2(0.64f, 0.5f), Vector2.zero, new Vector2(76, 76));
            var gemImg = gem.gameObject.AddComponent<Image>(); gemImg.raycastTarget = false; gemImg.sprite = UiTex.Diamond(GemViolet, 48);
            UiWidgets.Label(field, UiTheme.Track("REWARDS"), 24, new Vector2(0.5f, 0.56f), new Vector2(0.5f, 0.56f), Vector2.zero, new Vector2(pw * 0.6f, 40f), TextAnchor.MiddleCenter, UiTheme.ParchGold);

            // Btn_Play: wide cobalt CTA (≈0.30·W × 0.070·H) with a breathing glow + white "PLAY" (§E/H).
            float playW = 0.30f * 2340f, playH = 0.070f * 1080f;
            var playRt = UiWidgets.Rect("Btn_Play", field, new Vector2(0.5f, 0.14f), new Vector2(0.5f, 0.14f), Vector2.zero, new Vector2(playW, playH));
            var pgl = UiWidgets.Glow(playRt, UiTheme.A(CobaltHi, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(playW * 1.2f, playH * 1.8f), 1.6f);
            var pgFx = pgl.gameObject.AddComponent<PulseGraphic>(); pgFx.target = pgl; pgFx.min = 0.3f; pgFx.max = 0.65f; pgFx.period = 1.8f;
            var playImg = playRt.gameObject.AddComponent<Image>(); playImg.sprite = UiTex.VGradient(CobaltHi, CobaltDeep, 32);
            var playBtn = playRt.gameObject.AddComponent<Button>(); playBtn.targetGraphic = playImg;
            var pcb = playBtn.colors; pcb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); pcb.pressedColor = new Color(0.88f, 0.88f, 0.92f, 1f); pcb.fadeDuration = 0.08f; playBtn.colors = pcb;
            // OnPlay → Match Intro via the existing §12-safe battle seam (a meta navigation request; writes nothing).
            playBtn.onClick.AddListener(() => { AudioManager.Instance?.Click(); MatchPresentation.StartMatch("Campaign"); });
            var playRim = UiWidgets.Rect("Rim", playRt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var playRimImg = playRim.gameObject.AddComponent<Image>(); playRimImg.raycastTarget = false; playRimImg.sprite = UiTex.Frame(UiTheme.GoldHi, UiTheme.Gold, UiTheme.GoldShadow, 48, 8); playRimImg.type = Image.Type.Sliced;
            var playLbl = UiWidgets.Label(playRt, UiTheme.Track("PLAY"), 32, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, PlayWhite);
            playLbl.gameObject.AddComponent<Outline>().effectColor = UiTheme.A(Hex("#0a1f44"), 0.9f);

            // Close affordance (top-right of the panel).
            var close = UiWidgets.Rect("Popup_Close", field, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30, -30), new Vector2(64, 64));
            var closeImg = close.gameObject.AddComponent<Image>(); closeImg.sprite = UiTex.Disc(UiTheme.A(UiTheme.Charcoal, 0.95f), 48);
            var closeBtn = close.gameObject.AddComponent<Button>(); closeBtn.targetGraphic = closeImg;
            closeBtn.onClick.AddListener(ClosePopup);
            UiWidgets.Label(close, "x", 40, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, UiTheme.ParchGold);

            _popup.SetAsLastSibling();
        }

        // §K OnNodeTap: unlocked → open the LevelDetailPopup (stars + PLAY); locked → toast unlock requirement (§L).
        private void OnNodeTap(CampNode n)
        {
            if (n.Locked)
            {
                Router.Toast("Locked — clear the previous level"); // locked nodes show a locked message (no popup PLAY)
                return;
            }
            // Rebuild the popup's earned-star row for this node (current node = the next playable level → show 0 earned).
            for (int i = _popupStars.childCount - 1; i >= 0; i--) Object.Destroy(_popupStars.GetChild(i).gameObject);
            const float ps = 56f, pgap = 12f; float rowW = 3 * ps + 2 * pgap;
            UiWidgets.StarRating(_popupStars, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-rowW * 0.5f, 0), 3, Mathf.Clamp(n.Stars, 0, 3), ps, pgap);
            if (_popupTitle != null) _popupTitle.text = UiTheme.Track("LEVEL " + n.Display);
            OpenPopup();
        }

        private void OpenPopup()
        {
            _popup.SetAsLastSibling();
            _popupCg.blocksRaycasts = true; _popupCg.interactable = true;
            StartCoroutine(PopupIn());
        }

        private void ClosePopup()
        {
            AudioManager.Instance?.Click();
            _popupCg.blocksRaycasts = false; _popupCg.interactable = false;
            StartCoroutine(PopupOut());
        }

        // OnSelectNode (§I ~0.25s): dim-in + panel scale 0.94→1.0.
        private IEnumerator PopupIn()
        {
            if (_popupPanelCg != null) _popupPanelCg.transform.localScale = Vector3.one * 0.94f;
            yield return Tween(0.22f, k =>
            {
                if (_popupCg != null) _popupCg.alpha = k;
                if (_popupPanelCg != null) _popupPanelCg.transform.localScale = Vector3.one * Mathf.Lerp(0.94f, 1f, Back(k));
            });
            if (_popupCg != null) _popupCg.alpha = 1f;
            if (_popupPanelCg != null) _popupPanelCg.transform.localScale = Vector3.one;
        }

        private IEnumerator PopupOut()
        {
            yield return Tween(0.16f, k => { if (_popupCg != null) _popupCg.alpha = 1f - k; });
            if (_popupCg != null) _popupCg.alpha = 0f;
        }

        // ============================ LIFECYCLE + ENTER CEREMONY (§I) ============================
        public override void OnShow()
        {
            AudioManager.Instance?.PlayMenuMusic();
            StartCoroutine(Reveal());
        }

        // §I enter timeline (~0.7s): path draws on → nodes pop in number order (stars a beat later) → current-node
        // bloom → top chrome slides in → star total counts up + bottom nav slides up. Tap anywhere snaps to end-state.
        private IEnumerator Reveal()
        {
            // start hidden.
            if (_pathLayer != null) for (int i = 0; i < _pathLayer.childCount; i++) SetChildAlpha(_pathLayer.GetChild(i), 0f);
            for (int i = 0; i < _nodeRoots.Count; i++) if (_nodeRoots[i] != null) _nodeRoots[i].localScale = Vector3.zero;
            for (int i = 0; i < _starRows.Count; i++) if (_starRows[i] != null) _starRows[i].localScale = Vector3.zero;
            if (_heroStandee != null) _heroStandee.localScale = Vector3.zero;
            if (_currentGlow != null) { var g = _currentGlow.GetComponent<Image>(); if (g != null) { var c = g.color; c.a = 0f; g.color = c; } }
            SetCg(_chromeCg, 0f); SetCg(_navCg, 0f); SetCg(_starTotalCg, 0f);

            // 0.10s path draws on (segments sweep in 1→…→locked order).
            yield return Wait(0.10f);
            if (_pathLayer != null)
            {
                for (int i = 0; i < _pathLayer.childCount; i++)
                {
                    var child = _pathLayer.GetChild(i);
                    yield return Tween(0.05f, k => SetChildAlpha(child, k));
                }
            }

            // 0.15s nodes pop in number order (scale 0→1.1→1.0), stars a beat after their node.
            for (int i = 0; i < _nodeRoots.Count; i++)
            {
                var node = _nodeRoots[i];
                if (node != null) yield return Tween(0.12f, k => node.localScale = Vector3.one * Mathf.Lerp(0.2f, 1f, Overshoot(k)));
                if (i < _starRows.Count && _starRows[i] != null)
                {
                    var row = _starRows[i];
                    yield return Tween(0.08f, k => row.localScale = Vector3.one * Mathf.Lerp(0.4f, 1f, Back(k)));
                }
            }

            // 0.30s current-node 7 stronger arrival: cobalt glow blooms + hero standee + plume rise.
            if (_currentGlow != null)
            {
                var g = _currentGlow.GetComponent<Image>();
                yield return Tween(0.30f, k => { if (g != null) { var c = g.color; c.a = Mathf.Lerp(0f, 0.85f, k); g.color = c; } if (_heroStandee != null) _heroStandee.localScale = Vector3.one * k; });
            }

            // 0.20s top chrome (back, difficulty, chips) fade/slide in.
            yield return FadeUp(_chromeCg, 0.20f);
            // 0.40s star total counts up to 36/39 + bottom nav slides up.
            SetCg(_starTotalCg, 1f);
            _starCounter?.To(StarsCollected, 0.6f);
            yield return FadeUp(_navCg, 0.20f);

            SnapAll();
        }

        private void SnapAll()
        {
            if (_revealed) return;
            _revealed = true;
            StopAllCoroutines();
            if (_pathLayer != null) for (int i = 0; i < _pathLayer.childCount; i++) SetChildAlpha(_pathLayer.GetChild(i), 1f);
            for (int i = 0; i < _nodeRoots.Count; i++) if (_nodeRoots[i] != null) _nodeRoots[i].localScale = Vector3.one;
            for (int i = 0; i < _starRows.Count; i++) if (_starRows[i] != null) _starRows[i].localScale = Vector3.one;
            if (_heroStandee != null) _heroStandee.localScale = Vector3.one;
            if (_currentGlow != null) { var g = _currentGlow.GetComponent<Image>(); if (g != null) { var c = g.color; c.a = 0.85f; g.color = c; } }
            SetCg(_chromeCg, 1f); SetCg(_navCg, 1f); SetCg(_starTotalCg, 1f);
            _starCounter?.Snap(StarsCollected);
            if (_starTotalText != null) _starTotalText.text = StarsCollected + "/" + StarsPossible;
        }

        // Tap anywhere during the enter ceremony snaps it to the end-state (popup is opened separately, post-reveal).
        private void Update()
        {
            if (_revealed) return;
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0) SnapAll();
        }

        // ---- tiny unscaled tween helpers (menus run at timeScale 0; mirrors the CampaignResultScreen pattern) ----
        private static void SetCg(CanvasGroup cg, float a) { if (cg != null) cg.alpha = a; }
        private static void SetChildAlpha(Transform t, float a)
        {
            var img = t.GetComponent<Image>(); if (img != null) { var c = img.color; c.a = Mathf.Clamp01(a); img.color = c; }
        }
        private static float Back(float k) { const float s = 1.70158f; k -= 1f; return k * k * ((s + 1f) * k + s) + 1f; } // back-ease-out
        private static float Overshoot(float k) { return k < 0.7f ? Mathf.Lerp(0f, 1.1f, k / 0.7f) : Mathf.Lerp(1.1f, 1f, (k - 0.7f) / 0.3f); } // 0→1.1→1.0 pop
        private IEnumerator Wait(float d) { float t = 0f; while (t < d) { t += Time.unscaledDeltaTime; yield return null; } }
        private IEnumerator Tween(float d, System.Action<float> step) { float t = 0f; while (t < d) { t += Time.unscaledDeltaTime; step(Mathf.Clamp01(t / d)); yield return null; } step(1f); }
        private IEnumerator FadeUp(CanvasGroup cg, float d)
        {
            if (cg == null) yield break;
            var rt = cg.transform as RectTransform; Vector2 baseP = rt != null ? rt.anchoredPosition : Vector2.zero;
            yield return Tween(d, k => { cg.alpha = k; if (rt != null) rt.anchoredPosition = baseP + new Vector2(0, Mathf.Lerp(12f, 0f, k)); });
        }

        // Fractional anchor of a node (anchorY = 1 − fy_from_top); shared by the path + node builders (§E layout math).
        private static Vector2 AnchorOf(CampNode n) => new Vector2(n.Fx, 1f - n.Fy);

        private static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}
