# BULWARK — UI CONSTRUCTION SPEC · 35 · Clan
Source: design/ClanScreenDesign.png · 1829×860 (≈2.13:1) · Analysis-only forensic spec.

> Normalize to 2340×1080 (≈19.5:9). CanvasScaler ref 2340×1080, `matchWidthOrHeight = 1.0` (match HEIGHT). FRACTION-BASED layout; px quoted at 1080-tall height. Full-bleed BG under cutout; interactive content inside `Screen.safeArea`.

---

## A. SCREEN PURPOSE
The **real Clan hub** (resolves the prior "Clan == Leaderboard duplicate" defect). A three-column social home for the player's clan **DRAGONFORGE**: (LEFT) clan identity — crest/banner, name, motto, vital stats; (CENTER) tabbed content **MEMBERS / Activity / Clan War / Clan Chest** with the member roster (rank, avatar, name, level, role, trophies, online status) and a bottom action bar **CLAN WAR · DONATE · CLAN SHOP · MANAGE · LEAVE CLAN**; (RIGHT) a live **CHAT / Announcements** panel with a message composer. Server-authoritative social data; the client never edits roles/trophies. Reached from the Main-Menu rail. No gameplay/ECS interaction.

## B. VISUAL DNA (screen-specific, inherits GLOBAL DNA)
- Dark heroic high-fantasy; near-black charcoal field (#0a0b0f→#14161e) over a faint dusk **castle wall** backdrop, vignetted.
- **Brushed gold/antique bronze ornate frames** divide the three columns; thin gold hairline dividers between sections.
- **Faction banner identity:** the left column is dominated by a tall **royal/cobalt-blue heraldic banner** with a **gold dragon crest** and stitched gold trim (Iron-Pact-leaning palette for this clan).
- **Royal blue** = selected tab, online accents and the chat send affordance area; **ember/oxblood red** = the **LEAVE CLAN** danger button only. **Gold** = roles emphasis (Leader), trophy values, headers. **Green** (#3fd07a) = "Online" status dot.
- Role pills are tinted chips: **Leader** (gold), **Officer** (blue/steel), **Veteran** (bronze/grey), **Member** (muted grey). The **(You)** member row is highlighted (blue tint + outline).
- Low-key lighting; focal glow on the dragon crest; gold rim-light on frames; subtle banner cloth shading.

## C. SCREEN DECOMPOSITION (ASCII node tree — every node)
```
ClanScreen (UiScreen root, CanvasGroup)
└─ SafeAreaRoot (SafeAreaFitter)
   ├─ BackgroundLayer (full-bleed)
   │  ├─ BG_CastleWall (Image)
   │  └─ BG_Vignette (Image)
   ├─ TopBar
   │  ├─ BackButton (gold square + left chevron)
   │  ├─ Title_ClanName ("DRAGONFORGE", serif gold)
   │  └─ TopRightCluster
   │     ├─ MemberCountChip (Icon_People + "48/50")
   │     ├─ ClanGoldChip (Icon_Coin + "125,680")
   │     ├─ LanguagePill ("English")
   │     └─ SettingsGear (Button)
   ├─ ContentRow (3 columns)
   │  ├─ LeftColumn_Identity (panel)
   │  │  ├─ ClanBanner (tall blue banner Image)
   │  │  │  ├─ DragonCrest (gold dragon emblem)
   │  │  │  └─ Banner_ClanName ("DRAGONFORGE")
   │  │  ├─ Lbl_Motto ("United in Fire, Forged in Glory.")
   │  │  └─ StatsList
   │  │     ├─ Stat_ClanLevel ("Clan Level" / "15")  [+ progress bar fragment ~9,xxx/?]
   │  │     ├─ Stat_ClanType ("Clan Type" / "Open")
   │  │     ├─ Stat_RequiredLevel ("Required Level" / "30")
   │  │     ├─ Stat_ClanRegion ("Clan Region" / "North America")
   │  │     └─ Stat_Created ("Created" / "2024-02-14")
   │  ├─ CenterColumn_Roster (panel)
   │  │  ├─ TabRow
   │  │  │  ├─ Tab_MEMBERS (selected)
   │  │  │  ├─ Tab_Activity
   │  │  │  ├─ Tab_ClanWar
   │  │  │  └─ Tab_ClanChest
   │  │  ├─ RosterHeaderRow
   │  │  │  ├─ Lbl_MembersCount ("MEMBERS 48/50")
   │  │  │  ├─ Col_ROLE
   │  │  │  └─ Col_TROPHIES
   │  │  ├─ ScrollView (vertical)
   │  │  │  └─ Content (VerticalLayoutGroup)
   │  │  │     ├─ MemberRow_1 (RankBadge1 · Thalion · L60 · Leader(gold) · 24,560 · Online)
   │  │  │     ├─ MemberRow_2 (RankBadge2 · Valyra · L58 · Officer · 21,340 · Online)
   │  │  │     ├─ MemberRow_3 (RankBadge3 · Ragnor · L57 · Officer · 19,870 · In Battle)
   │  │  │     ├─ MemberRow_4 (4 · Eldric · L55 · Veteran · 18,450 · Online)
   │  │  │     ├─ MemberRow_5 (5 · Seraphine · L54 · Veteran · 16,720 · 1h ago)
   │  │  │     ├─ MemberRow_You (12 · Aric Stormblade (You) · L52 · Member · 12,680)  [highlighted]
   │  │  │     └─ MemberRow_7 (13 · Mortis · L51 · Member · 11,240 · 2h ago)
   │  │  └─ ActionBar
   │  │     ├─ Btn_ClanWar (icon + "CLAN WAR")
   │  │     ├─ Btn_Donate (icon + "DONATE")
   │  │     ├─ Btn_ClanShop (icon + "CLAN SHOP")
   │  │     ├─ Btn_Manage (icon + "MANAGE")
   │  │     └─ Btn_LeaveClan (red, "LEAVE CLAN")
   │  └─ RightColumn_Chat (panel)
   │     ├─ ChatTabRow
   │     │  ├─ Tab_Chat (selected)
   │     │  └─ Tab_Announcements
   │     ├─ ChatScrollView (vertical)
   │     │  └─ Content (VerticalLayoutGroup)
   │     │     ├─ ChatMsg (Thalion · "Welcome to Dragonforge! Let's conquer together!" · ts)
   │     │     ├─ ChatMsg (Valyra · "Great war tonight! 🔥" · ts)
   │     │     ├─ ChatMsg (Eldric · "Thanks everyone! GG all around." · ts)
   │     │     ├─ ChatMsg (Ragnor · "Let's keep the momentum going!" · ts)
   │     │     └─ ChatMsg (Seraphine · "I donated to the Clan Chest. Let's unlock those rewards!" · ts)
   │     └─ ChatComposer
   │        ├─ Input_Message ("Tap to message...")
   │        ├─ Btn_Emoji (😊)
   │        └─ Btn_Send ("SEND")
```

## D. UNITY HIERARCHY SPEC (per node)
- **ClanScreen** — parent UiRouter canvas. Empty `RectTransform` + `CanvasGroup`. Stretch-all. Full screen (NOT a modal).
- **SafeAreaRoot** — `SafeAreaFitter`, stretch-all. Children: Background, TopBar, ContentRow.
- **BackgroundLayer** (BG_CastleWall, BG_Vignette) — parent ClanScreen (outside safe area → full-bleed). Stretch-all, `Image`, raycast off. Vignette later sibling.
- **TopBar** — parent SafeAreaRoot. Anchor top-stretch (0,1)-(1,1) pivot 0.5,1, fixed height (E). BackButton top-left; Title left-of-center after the back button; TopRightCluster top-right `HorizontalLayoutGroup` (chips + gear).
- **BackButton** — `Button`, anchor (0,1) pivot 0,1, ≥88×88.
- **Title_ClanName** — `Text` serif gold, anchor left (after back btn), pivot 0,1, left-aligned.
- **TopRightCluster** — `HorizontalLayoutGroup` (spacing ~16), anchor (1,1) pivot 1,1. Children: MemberCountChip, ClanGoldChip, LanguagePill, SettingsGear. Each chip = `Image` capsule + icon + `Text`.
- **ContentRow** — parent SafeAreaRoot. Anchor stretch-all below TopBar (min (0, 0) max (1, topBarBottomFrac)) pivot 0.5. `HorizontalLayoutGroup` is possible but FIXED fractional widths via three anchored panels is preferred (the columns differ in width). Child order: LeftColumn, CenterColumn, RightColumn.
- **LeftColumn_Identity** — anchor left band (E). `Image` panel (dark inset + gold frame). `VerticalLayoutGroup` top-aligned: Banner (large), Motto, StatsList.
  - **ClanBanner** — `Image` tall blue banner (9-slice or bespoke), anchor top-center, pivot 0.5,1, `preserveAspect`. **DragonCrest** child `Image` centered on banner upper-third; **Banner_ClanName** `Text` overlaid lower on the banner.
  - **Lbl_Motto** — `Text` italic-leaning cream, center, below banner.
  - **StatsList** — `VerticalLayoutGroup` of label/value rows; each row = `HorizontalLayoutGroup` (caption left grey, value right gold). ClanLevel row may include a small `Image` fill bar (progress) under it.
- **CenterColumn_Roster** — anchor center band (E), widest column. `Image` panel. Child order: TabRow (top), RosterHeaderRow, ScrollView (fills), ActionBar (bottom).
  - **TabRow** — `HorizontalLayoutGroup` / `ToggleGroup`, top-stretch, fixed height. MEMBERS selected.
  - **RosterHeaderRow** — top-stretch under tabs, fixed ~42 px. Left `Text` "MEMBERS 48/50"; right two column labels ROLE, TROPHIES aligned to the row's role/trophy x-fractions.
  - **ScrollView** — `ScrollRect` vertical, `RectMask2D` viewport, Content `VerticalLayoutGroup` (spacing ~8) + `ContentSizeFitter`.
  - **MemberRow_N** — `RectTransform` + `Button` (tap → member context, optional). Internal `HorizontalLayoutGroup`-style positioned by x-fractions: RankBadge | Avatar | Name+LevelChip | RolePill | TrophyValue | StatusTag. Fixed height (E). The **(You)** row uses a blue-tinted bg `Image` + 2px blue `Outline`.
  - **RankBadge** — ranks 1-3 medallion `Image` (gold/silver/bronze) + numeral; 4+ plain numeral `Text`.
  - **LevelChip** — small dark capsule `Image` + level `Text` (e.g. "60").
  - **RolePill** — capsule `Image` tinted by role + role `Text`.
  - **StatusTag** — green dot `Image` + `Text` ("Online") OR amber dot + "In Battle" OR grey + "1h ago"/"2h ago"; the (You) row has no status tag in source (omit).
  - **ActionBar** — bottom-stretch, fixed height. `HorizontalLayoutGroup`: four icon+label buttons (ClanWar, Donate, ClanShop, Manage) sharing equal width, then **Btn_LeaveClan** (red) wider/right-weighted.
- **RightColumn_Chat** — anchor right band (E). `Image` panel. Child order: ChatTabRow (top), ChatScrollView (fills), ChatComposer (bottom, fixed height).
  - **ChatTabRow** — two tabs (Chat selected, Announcements). `ToggleGroup`.
  - **ChatScrollView** — `ScrollRect` vertical; Content `VerticalLayoutGroup` (spacing ~12) + `ContentSizeFitter`; auto-scroll to bottom on new message.
  - **ChatMsg** — `VerticalLayoutGroup` bubble: header row (small Avatar `Image` + sender `Text` gold + timestamp `Text` grey right) over body `Text` (wrapping cream).
  - **ChatComposer** — bottom-stretch `HorizontalLayoutGroup`: Input_Message (`InputField`/TMP_InputField, flexible width, placeholder "Tap to message..."), Btn_Emoji (square), Btn_Send (blue/gold capsule "SEND").
- **Responsive:** match-height keeps row/message heights stable. On ultrawide, the **center roster** flexes wider (extra width goes to PLAYER/name area); left and right columns keep min widths. On narrower-than-ref (rare in landscape), right chat column can collapse to a toggle drawer (note as fallback). Notch handled by SafeAreaRoot; BG full-bleed.

## E. LAYOUT MATHEMATICS (fractions of 2340×1080)
- **TopBar height:** ≈ 0.095 (~103 px). BackButton ≈ 88×88, left-inset ~1.4%. Title baseline centered to bar. TopRightCluster right-inset ~1.4%; chips ~190–230 px wide each, gear ~72×72.
- **ContentRow:** y from ~0.10 to ~0.99 of canvas (below TopBar to bottom inset ~1%).
- **Three-column widths (fractions of canvas width, with ~1.2% gutters):**
  - LeftColumn_Identity: ≈ 0.205 (~480 px), x≈[0.012, 0.217]
  - CenterColumn_Roster: ≈ 0.475 (~1112 px), x≈[0.229, 0.704]
  - RightColumn_Chat: ≈ 0.272 (~636 px), x≈[0.716, 0.988]
- **LeftColumn internal:** Banner occupies top ~0.42 of the column height (tall, ~aspect 0.55:1); crest centered at ~0.30 of banner height; motto one line below banner; StatsList = 5 rows, each ~0.07 of column height, caption left / value right, with a thin divider between rows.
- **CenterColumn internal:** TabRow band height ≈ 64 px (4 tabs equal width across the column, ~24% each). RosterHeaderRow ≈ 42 px. ActionBar height ≈ 92 px. ScrollView fills the remainder.
  - **Roster row x-fractions (relative to center column width):** RANK center ≈ 0.045 (w 0.09) · Avatar center ≈ 0.135 (Ø ~52 px) · Name block starts ≈ 0.18 with LevelChip immediately right of the name · RolePill center ≈ 0.62 (w ~0.16) · TrophyValue (icon+number) center ≈ 0.83 right-aligned · StatusTag center ≈ 0.95 right-aligned.
  - **Roster row height:** ≈ 80 px; gap ~8 px; seven seed rows; scroll for more.
  - **ActionBar:** five buttons; ClanWar/Donate/ClanShop/Manage each ≈ 0.155 of column width (icon over/left of label), LeaveClan ≈ 0.22 width, gap ~12 px; LeaveClan right-aligned and red.
- **RightColumn internal:** ChatTabRow height ≈ 56 px (two tabs ~50% each). ChatComposer height ≈ 80 px (Input flexible, Emoji ~64×64, Send ~120×64). ChatScrollView fills the rest. Message bubble: avatar Ø ~36 px; sender+timestamp header ~28 px; body wraps; bubble vertical padding ~10 px.
- **Tablet 4:3:** columns keep fractions; extra height grows the scroll areas. **Ultrawide 21:9:** clamp overall content to max-width ~0.97·2340 and grow CenterColumn. **Notch:** inside SafeAreaRoot; BG bleeds.

## F. TYPOGRAPHY (per text)
> Intended: serif display SDF (Cinzel/Trajan-like) for clan name/headers; Roboto-Medium SDF for body/chat/numbers. Shipped fallback legacy Text (note upgrade in L).

| Element | Personality | Weight | Caps | Kerning | Line-sp | Glow/Stroke/Shadow | px@1080 | Hex |
|---|---|---|---|---|---|---|---|---|
| Title clan name "DRAGONFORGE" (topbar) | prestige serif | Heavy | UPPER | +6% | 1.0 | gold bevel + bloom + dark stroke | ~52 | #f0d27a / stroke #3a2c0e |
| Banner clan name (on banner) | heraldic serif | Bold | UPPER | +4% | 1.0 | gold + soft shadow on cloth | ~40 | #f4dd8c |
| Motto "United in Fire, Forged in Glory." | flavor | Regular/italic | Title | +2% | 1.1 | none | ~24 | #cdbf9e |
| Stat captions ("Clan Level" etc.) | label | Medium | Title | +1% | 1.0 | none | ~22 | #9a8a6a |
| Stat values ("15","Open","North America"...) | data | Bold | Title | 0 | 1.0 | faint shadow | ~24 | #e9dcc0 |
| Member/Gold chips ("48/50","125,680") | data | Bold | — | +1% | 1.0 | shadow | ~26 | #f0d27a |
| Language pill ("English") | utility | Medium | Title | +2% | 1.0 | none | ~24 | #d8cdb2 |
| Tab labels (MEMBERS/Activity/Clan War/Clan Chest) | sans caps | Bold | UPPER(MEMBERS)/Title | +3% | 1.0 | sel white glow / unsel gold-grey | ~28 | sel #ffffff / unsel #9a8a6a |
| Roster header "MEMBERS 48/50" | heading | Bold | UPPER | +4% | 1.0 | gold | ~26 | #f0d27a |
| Column labels (ROLE/TROPHIES) | small caps | Medium | UPPER | +6% | 1.0 | none | ~22 | #9a8a6a |
| Member name ("Thalion"…) | name | Bold | Title | +1% | 1.0 | shadow | ~28 | #f3ead2 |
| "(You)" suffix on Aric | emphasis | Bold | Title | 0 | 1.0 | blue tint | ~24 | #9fc0ff |
| Level chip number ("60") | data | Bold | — | 0 | 1.0 | shadow | ~22 | #e9dcc0 |
| Role pill text (Leader/Officer/Veteran/Member) | label | Bold | Title | +2% | 1.0 | role-tinted, faint glow | ~22 | Leader #f0d27a / Officer #8fb3ff / Vet #cdb88a / Member #b8b0a0 |
| Trophy value ("24,560") | data | Bold | — | +1% tabular | 1.0 | shadow | ~26 | #f0d27a |
| Status tag (Online/In Battle/1h ago) | status | Medium | Title | +1% | 1.0 | dot-colored | ~22 | Online #3fd07a / InBattle #f0a23a / ago #8c8270 |
| Action button labels (CLAN WAR/DONATE/CLAN SHOP/MANAGE) | CTA caps | Bold | UPPER | +3% | 1.0 | gold on dark, shadow | ~24 | #f0d27a |
| LEAVE CLAN label | danger CTA | Bold | UPPER | +3% | 1.0 | white on red + shadow | ~24 | #ffffff |
| Chat tab labels (Chat/Announcements) | sans caps | Bold | UPPER | +3% | 1.0 | sel white / unsel grey | ~24 | sel #ffffff / unsel #9a8a6a |
| Chat sender name | name | Bold | Title | +1% | 1.0 | gold | ~24 | #e7cf94 |
| Chat timestamp | meta | Regular | — | 0 | 1.0 | none | ~20 | #7a7060 |
| Chat body text | body | Regular | Sentence | 0 | 1.2 | none | ~24 | #d9d2c2 |
| Composer placeholder "Tap to message..." | placeholder | Regular/italic | Sentence | 0 | 1.0 | none | ~24 | #6f685a |
| SEND label | CTA | Bold | UPPER | +3% | 1.0 | white/gold | ~24 | #ffffff |

## G. MATERIALS
- **Frames/dividers:** brushed gold/antique bronze (base #8a6a28, hi #f0d27a, sh #5a3f12), satin roughness, worn edges, engraved filigree on column rails; thin gold hairlines between sections.
- **Panel fills:** obsidian #0c0e14→#14161e gradient, low reflectivity, inner shadow at frame edges.
- **Clan banner:** royal-blue cloth (#1d2f7a core, #2b56c8 mid, #0f1b4a shadow) with stitched gold trim and a subtle cloth weave/fold shading; bottom edge swallow-tail cut. **DragonCrest:** polished gold dragon relief (#f6dd86 hi, #b8862c mid, #6b4a14 sh) with a small blue gem and focal bloom.
- **Medallion rank badges:** gold/silver/bronze cast disks (same palette as spec 34).
- **Role pills:** Leader = gold gradient chip (glossy); Officer = steel-blue chip; Veteran = bronze/grey chip; Member = muted dark-grey chip; all with thin metal rim.
- **Avatars:** semi-realistic portrait sprites in beveled gold rings.
- **Status dots:** Online emissive green (#3fd07a, faint glow); In Battle amber (#f0a23a); ago dim grey.
- **Trophy icon:** gold cup, satin.
- **Action buttons:** dark stone/metal capsules with gold icon+label; **LeaveClan** = oxblood/ember red gradient (#7a1f1a→#d8452b), glossy, danger glow.
- **Chat panel:** slightly darker inset; message bubbles nearly transparent dark with a faint left gold accent on the sender row; **Send** button blue or gold gloss; **Input** field a dark recessed pill with inner shadow.

## H. COMPONENTS (states: idle/hover/pressed/disabled/selected)
- **BackButton / SettingsGear:** idle gold on dark; hover +brightness+glow; pressed scale 0.94; SFX click.
- **TopRightCluster chips / LanguagePill:** mostly display; LanguagePill is a `Button` (opens language picker) — hover faint glow, pressed scale 0.97.
- **Roster tabs (MEMBERS/Activity/Clan War/Clan Chest):** selected = blue fill + white label + raise/glow; idle = ghost capsule + gold-grey label; hover brightens; pressed 0.97; disabled 40%. Switching cross-fades the center content (roster vs activity feed vs war panel vs chest panel).
- **Member rows:** idle subtle stripe; hover faint gold tint + 1px edge; pressed 0.99; the **(You)** row is sustained blue tint + outline; tap (optional) opens member actions (promote/kick if permitted — gated by role, server-validated).
- **Action buttons (ClanWar/Donate/ClanShop/Manage):** idle stone capsule + gold; hover +brightness+glow; pressed scale 0.96 + inset; disabled desaturated @50% (e.g., Manage disabled for non-officers). **LeaveClan (danger):** idle red gradient; hover brighter red + glow; pressed darken+0.96; always triggers a Confirm modal (spec 37) before acting.
- **Chat tabs (Chat/Announcements):** same toggle pattern; Announcements may be read-only (no composer) for non-officers.
- **Chat composer:** Input idle recessed; focused = gold/blue ring + caret; Emoji button opens picker; **Send** idle blue, hover brighter, pressed 0.96, **disabled when input empty** (desaturated). Sending appends a bubble and clears input.
- **ScrollViews:** elastic overscroll; chat auto-scrolls to newest; momentum flick.
- **Feedback:** ≥88px touch; pressed ≤80ms scale; hover only on pointer platforms.

## I. ANIMATION TIMELINE
- **OnShow:** 0 ms CanvasGroup α 0→1 over 180 ms. 40 ms TopBar slide −16px→0 over 200 ms. 80 ms LeftColumn slide −24px→0 + α over 220 ms (ease-out); banner crest soft bloom pulse one-shot at 220 ms. 120–420 ms roster rows stagger-in (α + slide +12px), 30 ms apart, 160 ms each. 140 ms RightColumn chat slides +24px→0 + α over 220 ms; latest chat bubbles pop subtly. 260 ms ActionBar fades/raises in (180 ms). 300 ms (You) row blue-glow flash (220 ms).
- **Tab switch (center):** old content α 1→0 + slide −10px (120 ms); new content α 0→1 + slide +10px→0 (160 ms); selected capsule glides (140 ms). Re-stagger rows 15 ms apart.
- **Chat tab switch:** cross-fade lists 140 ms.
- **New chat message arrives:** bubble α 0→1 + slide +10px→0 + scale 0.98→1 over 180 ms; auto-scroll to bottom (160 ms ease-out).
- **Send pressed:** button scale 0.96 (80 ms), input clears, bubble appended with the arrive anim.
- **LeaveClan pressed:** triggers Confirm modal slide-in (per spec 37 I).
- **OnHide:** CanvasGroup α 1→0 over 140 ms; columns slight outward slide.
- **Easing:** ease-out enters, ease-in exits, gentle back on banner/crest pop.

## J. PARTICLE & FX
- **DragonCrest:** steady focal bloom + slow specular sweep + 1–2 sparkle motes (low rate).
- **ClanBanner:** very subtle **cloth sway** (1–2° rotation at the free edge, ~3 s sine loop) + soft shadow shift — keep gentle.
- **Online status dots:** faint pulsing green glow (±10%, 1.8 s).
- **Selected tab / Send button:** subtle pulsing rim glow.
- **(You) row:** breathing blue outline glow (±8%, 2 s).
- **LeaveClan:** faint ember flicker on hover only. Keep all FX low-key — roster/chat legibility first.

## K. EVENT BEHAVIOR
- **OnShow:** fetch clan summary (name, crest, motto, stats, member count, clan gold), the member roster, and recent chat/announcements from the server-auth clan service; bind region/language; subscribe to chat stream.
- **OnTabChanged(MEMBERS/Activity/Clan War/Clan Chest):** load+show that sub-view (Activity feed, Clan War status/bracket, Clan Chest progress/contribution); MEMBERS is default.
- **OnChatTabChanged(Chat/Announcements):** swap stream; hide composer for read-only Announcements when not permitted.
- **OnSend:** validate non-empty → send message to server → optimistic append → reconcile on ack; clear input; auto-scroll.
- **OnEmoji:** open emoji picker; insert glyph into input.
- **OnMemberTap:** open member context (view profile; promote/demote/kick if the local player's role permits — all server-validated).
- **OnAction:**
  - **ClanWar** → open Clan War screen/flow.
  - **Donate** → open donation requests (contribute units/resources; server-validated, never client-mint).
  - **ClanShop** → open clan shop (spend clan currency).
  - **Manage** → clan settings (officer+ only; permission-gated/disabled otherwise).
  - **LeaveClan** → Confirm modal (spec 37); on confirm, call server `leaveClan`, then route to the clan-finder/join screen.
- **OnSettingsGear:** open clan notification/preference settings (or clan-level admin if permitted).
- **OnLanguagePill:** open language/region filter for the clan.
- **OnBack:** pop screen → Main Menu.
- **Failure:** network failure → NetworkError (spec 39) or inline retry; never fabricate roster/chat.

## L. NEGATIVE RULES
- Do NOT let the client edit roles, trophies, member count, or clan gold — **display only**, server-authoritative; role/permission gating must be enforced server-side too.
- Do NOT skip the **Confirm modal** before LEAVE CLAN (destructive).
- Do NOT replace this with a modal; it's a full screen.
- Do NOT use real brand text or stick figures; keep palette within DNA. Faction-blue banner is fine (clan identity).
- Do NOT enable **Send** with an empty input; do NOT show the composer on read-only Announcements without permission.
- Do NOT stretch avatars/banner on ultrawide — grow the center column instead.
- Do NOT over-animate the banner sway or status glow (legibility first).
- Shipped reality: legacy Text/LegacyRuntime.ttf won't render the gold-bevel serif clan name well — **flag the TMP SDF upgrade**; don't block.

## M. ACCEPTANCE CRITERIA (≥95% fidelity)
1. Three-column layout (Identity / Roster / Chat) with widths within ±2% of the E fractions; gold frames + dividers.
2. TopBar: back chevron + "DRAGONFORGE" + member chip "48/50" + gold "125,680" + "English" pill + gear, in source order.
3. Left column: blue dragon banner + "DRAGONFORGE" + motto "United in Fire, Forged in Glory." + the five stats (Clan Level 15, Clan Type Open, Required Level 30, Clan Region North America, Created 2024-02-14).
4. Center: tabs MEMBERS(selected)/Activity/Clan War/Clan Chest; header "MEMBERS 48/50" + ROLE/TROPHIES columns; the seven seed member rows with EXACT names/levels/roles/trophies/status; ranks 1-3 medallions; the (You) row (12 · Aric Stormblade · L52 · Member · 12,680) highlighted.
5. Action bar: CLAN WAR · DONATE · CLAN SHOP · MANAGE (gold icon buttons) + LEAVE CLAN (red); Leave triggers Confirm.
6. Right: CHAT(selected)/Announcements tabs; the five seed chat messages with senders/timestamps/text; composer "Tap to message..." + emoji + SEND; Send disabled when empty.
7. Role pills color-coded (Leader gold/Officer blue/Veteran bronze/Member grey); Online green / In Battle amber / "Nh ago" grey statuses.
8. Colors within DNA hex ranges; fraction-based + match-height stable; safe-area respected; BG full-bleed; animations per Section I.

## N. IMPLEMENTATION CONFIDENCE
**90/100.** High: tri-column structure, roster columns, role/status taxonomy, action bar, chat layout all read clearly. Risks: bespoke banner/dragon-crest + medallion + role-pill artwork (-4); gold-bevel serif needs TMP SDF (-2); live chat stream + permission gating is engineering beyond the visual (-2); exact stat progress-bar fragment under "Clan Level" is partially occluded/interpretive (-2).

## O. SELF-CHECKLIST
- [x] Read source PNG before writing.
- [x] All sections A–O present, substantive, in order.
- [x] Member-list AND chat layout specified (Section D/E call-out per the SOCIAL batch).
- [x] Fraction-based layout → 2340×1080; match-height; safe-area; full-bleed BG.
- [x] Exact strings/numbers (clan name, motto, stats, all 7 members, all 5 chat messages, action labels) recorded.
- [x] Typography table + hex; materials (crest/banner) with hex/finish.
- [x] States, animation, FX (banner sway), events (OnSend/OnLeave/OnShow…), negative rules.
- [x] No code/assets/scenes; analysis-only; invented nothing.
