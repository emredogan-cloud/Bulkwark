# BULWARK — Sezonsal Live-Ops, Battle Pass & Tutundurma (Retention) Çerçevesi (Gelecek Araştırma)

> **⚠️ STATÜ: GELECEK ARAŞTIRMA İZİ — YALNIZCA TAVSİYE NİTELİĞİNDE.**
> Bu belge **aktif geliştirme akışının parçası DEĞİLDİR**. Aktif akış şudur: **CI/CD doğrulama · APK üretimi · Unity doğrulama · kalan Phase 0–3 kapı borç-eritimi.**
> **Bu belge HİÇBİR ŞEYİ:** roadmap'i değiştiremez · kanonu değiştiremez · karar günlüğünü (decision log) değiştiremez · gelecek özellik yetkilendiremez · üretim önceliklerini değiştiremez · implementasyon başlatamaz.
> **Bu belge YALNIZCA:** keşfeder · analiz eder · tavsiye eder · değerlendirir.
> **Konum:** `future/004-liveops-and-battlepass/` (hiçbir `report/*.md`, `docs/adr/*`, `decision log` dosyası okundu-ama-DEĞİŞTİRİLMEDİ).
> **Kanonik yuvalar (bu araştırma birden çok faza yayılır):**
> - **Battle Pass S0 + kozmetik + dükkân + sandık + günlük/haftalık görev + temel hafta-sonu modifikatörleri = Roadmap §13 Phase 4** ("Monetization & Live-ops Shell"). **Phase 4 = NOT STARTED**; **Phase 4'e yetki = WITHHELD** (ADR-2-001 §Decision-1; phase-3 raporu §10).
> - **Sezon 1 (8-hafta canlı kadans) = Roadmap §13 Phase 6.2.** Entry = GATE 5 (SCALE-OR-STOP) PASS.
> - **Tam Event Engine + Event-token para birimi + Sezonsal mod takvimi = Roadmap §7 / §13 Phase 7.6 (DEFER).** MVP/launch yalnızca **temel hafta-sonu modifikatörleri** taşır.
> - **Ranked sezonları + *Honor* para birimi + deterministic replay = Roadmap §13 Phase 7.1 (DEFER).**
> Bu tetikleyicilerin **hiçbiri henüz ateşlenmemiştir** → bu rapordaki hiçbir şey bugün implemente edilmez, kanona girmez veya öncelik değiştirir.
> **Tarih:** 2026-06-03 · **Dil:** Türkçe · **Kalite çıtası:** Lead Economy Designer + Live-Ops Director + Monetization Designer + Systems Designer.
> **Yöntem:** Aşağıdaki tüm tasarımlar, **5 kanonik döküman + 5 ADR + 9 execution-prompt + Phase 0–3 implementasyon `.asset`/`.cs` verisi + 4 faz-tamamlanma raporu + ilk-derleme (CI) raporu**nun okunmasına dayanır. Hiçbir sayı uydurulmadı; provizyonel değerler **LSD/LP-owned + RemoteConfig-tunable** olarak işaretlendi.

---

# Proje Derin Analizi

*(ZORUNLU ön-araştırma fazı — atlanmadı. Bu bölüm; mevcut durumu yeniden-kurar (current state reconstruction), kanonu denetler (canon audit) ve çelişki kontrolü (contradiction check) yapar. Ancak bundan sonra tasarım/öneri üretilir.)*

## 1.1 Mevcut proje durumu (rekonstrüksiyon)

BULWARK, *Stick War* halefi; mobil-öncelikli (Android/iOS), tek-cepheli taktiksel **RTS-lite** (Unity 6 LTS · IL2CPP · URP 2D · **ECS/DOTS battle-sim**, MonoBehaviour/UGUI meta). Çekirdek döngü: **maden → eğit → ittir → heykeli yık** (2–5 dk maçlar, suspend/resume, tek-elle-oynanabilir). Üretim felsefesi: kanıtlanmış çekirdeği koru, iki şeyi modernize et (sığ savaş alanı → terrain/formasyon/counter/spell-draft; kırılgan client-trust ekonomi → **server-authoritative**), ve **etik, kozmetik-öncelikli** monetizasyon.

| Eksen | Kanonik değer (kaynak) |
|---|---|
| **Pillar'lar** | **P1 Agency · P2 Readable depth · P3 Fair mastery · P4 Respect the player** (Roadmap §2, Blueprint §1) |
| **Monetizasyon felsefesi** | **Kozmetik + battle-pass led; şeffaf; yalnızca opt-in rewarded reklam; ASLA güç satmaz** (Roadmap §2, §10) |
| **Anti-kimlik (NON-GOAL)** | Clash-Royale-tipi gerçek-zaman-PvP-öncelikli DEĞİL · whale SLG/4X DEĞİL · pasif autobattler DEĞİL · gacha/loot-box DEĞİL · **P2W DEĞİL** · energy-gated DEĞİL (Roadmap §2) |
| **MVP içeriği** | 2 fraksiyon (Iron Pact, Ashen Horde) · 12 birim (6+6) · 12 büyü · **2 komutan (1/fraksiyon)** · 3 harita · 20-seviye Kampanya (Act 1) · Endless · Async Ghost Ladder · **4 para birimi** · 5 kozmetik kademe |
| **Aktif geliştirme akışı** | CI/CD doğrulama · APK üretimi · Unity doğrulama · Phase 0–3 kapı borç-eritimi |
| **Bu araştırmanın hedefi** | Phase 4 (shell) + Phase 6 (S1) + Phase 7.6 (event engine) live-ops/battle-pass/sezonsal-tutundurma çerçevesini **ön-tasarlamak** — bugünü değiştirmeden |

## 1.2 Faz durumu, blokerler, ertelenmiş kapılar & doğrulama borcu

**Kullanıcı beyanı (roadmap status):** Phase 0 COMPLETE · Phase 1 COMPLETE · Phase 2 COMPLETE · Phase 3 COMPLETE · **Phase 4 NOT STARTED.**

**İmplementasyon gerçeği (ADR'ler + faz raporları + ilk-derleme raporundan — dürüst rekonstrüksiyon):** Tüm Phase 0–3 deliverable'ları **AUTHORED & commit edildi ve artık CI'da derleniyor (compile PASS, 0 hata, Unity 6000.0.75f1)**, ancak **çalışma-zamanı (runtime) doğrulama kapıları DEFERRED** — çünkü bu ortamda başlangıçta Unity editörü / cihaz / BaaS yoktu (ADR-0-001 Blocker A), ve APK/oynanış-doğrulaması hâlâ borçlu.

| Kapı / Aşama | Durum | Kaynak |
|---|---|---|
| Phase 0 çıkış | **CONDITIONALLY ACCEPTED** (deferred validations outstanding) | ADR-0-002 |
| **GATE 1 (FUN)** — "combat eğlenceli mi? bir el daha?" | **OPEN / DEFERRED** (on-device fun-verdict çalıştırılmadı) | ADR-1-001, phase-1 raporu |
| **GATE 2** (external playtest: ≥%40 session-2 + "okunur & eğlenceli") | **DEFERRED** (çalıştırılmadı) | ADR-2-001, phase-2 raporu |
| **GATE 3** (MVP feature-complete; ekonomi server-validated) | **DEFERRED**; **Phase 4'e yetki = WITHHELD** | ADR-2-001, phase-3 raporu §10 |
| **Phase 0–3 derleme (compile)** | **✅ PASS** (CI, 0 hata, EILPP+Burst çalıştı) | FIRST_COMPILE_REPORT §2 |
| Android build / APK | **FAIL → DEFERRED** (post-compile editör-config: kaydedilmemiş Scene / Addressables / URP global settings / Android SDK) | FIRST_COMPILE_REPORT §3, §7 |
| EditMode/PlayMode testleri | **ÇALIŞMADI → DEFERRED** (runner setup'ta abort) | FIRST_COMPILE_REPORT §4 |
| FormationMember kablolaması | **ERTELENDİ** (formasyonlar authored, üyelik ataması yok) | FormationMember_wiring_plan.md, phase-2/3 raporları |

**Aktif blokerler (doğrulama borcu):**
1. **Tek seferlik Unity-editör konfigürasyonu** — URP global settings asset'i, en az bir kaydedilmiş Scene/SubScene (`BattleBootstrap` etrafında), Addressables grupları, Android SDK hedef API seviyesi. *(Not: en son iki commit — `1b1b12b "Unity Editor initial configuration done"` ve `b74afee "valid asset YAML (Addressables) + Android SDK levels"` — bu §7 blokerlerine doğrudan saldırıyor; bu, aktif akışın canlı olduğunun kanıtı.)*
2. **BaaS canlı entegrasyonu** (PlayFab/Nakama SDK + CloudScript/Title Data) — server-authoritative ekonominin runtime ispatı için.
3. **Konsolide oynanış-doğrulama geçişi** — GATE 1 (fun) → GATE 2 (depth playtest) → GATE 3 (server-validated ekonomi), sırayla.

> **Bu araştırma için anlam (kritik):** Bu raporun tasarladığı **her şey Phase 4 ve sonrasıdır.** Phase 4'e yetki **WITHHELD**'dir ve önce GATE 1/2/3 + (Phase 5) soft-launch **SCALE-OR-STOP LTV kapısı** geçmelidir. Yani battle-pass/live-ops, **kanıtlanmış eğlence + kanıtlanmış tutundurma + kanıtlanmış LTV** üzerine kurulur. Bu rapor o günü **önceden** hazırlar; bugünün önceliğini (compile→APK→Unity doğrulama→kapı eritimi) **değiştirmez.**

## 1.3 Zaten-tasarlanmış sistemler (canon — bu raporun tabanı)

### Ekonomi sistemleri (Roadmap §8/§9/§10, Blueprint §7, implementasyon)
- **4 MVP para birimi (yalnızca bunlar — §15.1 "no invented currency"):** `EconomyTypes.cs` ile teyitli —
  - **Gold** (`Currency.Gold=0`): **maç-içi, KALICI DEĞİL** (ECS `GoldStore`); sunucu cüzdanında asla yer almaz; birim eğitir.
  - **Silver** (`=1`): **meta yumuşak para** (server-owned); **capped birim yükseltmeleri** + komutan talent'lerini satın alır.
  - **Gems** (`=2`): **premium para** (server-owned); kazanılır + (Phase 4) satın alınabilir; **ASLA güç satın almaz** (§9).
  - **PassXP** (`=3`): **battle-pass iz xp'si** (server-owned, sezon-ömürlü).
- **Post-launch para birimleri (Phase 7, henüz YOK):** *Honor* (ranked, 7.1) ve *Event tokens* (sezonsal, 7.6). **İkisi de earned-only · server-authoritative · yalnızca-kozmetik · asla-güç** (§9). Bu rapor bunları yalnızca Phase 7 yuvalarında kullanır.
- **Implementasyon değerleri (`EconomyConfig.asset` + `UpgradesConfig.asset` — PROVISIONAL/LSD-owned/RC-tunable):** `firstClearGemBase=20`, `gemDiminishDecrement=5`, `gemDiminishFloor=5` → ilk-geçiş gem = `max(5, 20−5×replays)` (dossier'ın azalan-getiri eğrisi); `silverPerEndlessWave=8`; `passXpPerBattle=10`; `silverPerLadderWin=40`; `gemsPerLadderSeasonReward=50`; kampanya `silverReward=60`, `passXpReward=11` (level 01).
- **Sandık mimarisi (§8 — etik):** Wood / Silver / Gold / Seasonal; **disclosed odds**; **hiçbir sandıkta güç YOK** (yalnızca kozmetik + currency); **dupe → cosmetic-craft shard**; **paralı rastgele kutu YOK** (paralı = see-what-you-buy); Gem yalnızca **zamanlayıcı atlar (convenience)**.

### Progression sistemleri (Roadmap §3/§13 P3.2, implementasyon)
- **Capped birim yükseltmeleri:** `UpgradesConfig.asset` — birim başına stat track'leri (Health/Damage), `maxLevel=5`, `baseCostSilver=120/150`, `costGrowth=1.6`, `deltaPerLevel=8` (HP) / `1.5` (Damage); **server-side hard-cap** (toplam ≈ +%24; sonsuz güç YOK; **ranked normalize eder**). P2W sızıntısı yok (phase-3 raporu §5: PASS).
- **Komutan seviyeleri:** `commanderMaxLevel=10`, `commanderBaseCostSilver=200`, `commanderCostGrowth=1.5`; **§6 güç-bütçesine clamp'li** (ADR-2-002).

### Komutan sistemleri (Roadmap §5.5/§6, Blueprint §6, implementasyon, ADR-2-002)
- **2 komutan (MVP):** Iron Warden (Iron Pact — Rally/Quartermaster, budget 0.12) · Ashen Warchief (Ashen Horde — WarCry/Bloodthirst, budget 0.13).
- **Güç bütçesi ≤ %10–15** (`k_PowerBudgetCeiling=0.15`, HARD-CLAMP, **İHLAL EDİLEMEZ**); komutan = **kimlik + force-multiplier, süper-birim DEĞİL**.
- **Earnable** (play / battle pass); **premium = yalnızca skin/VFX/voice**; komutan **KOLEKSİYONU = Phase 7.5 (DEFER)** — `future/001-commander-talent-system/` bu izi ön-tasarlar (kardeş araştırma; bu rapor onunla uyumludur, komutan-kozmetik gelirini ondan miras alır).

### Kozmetik stratejisi (Roadmap §6, Blueprint §10, `future/000-assets-roadmap/`)
- **Kozmetik-güvenlik kuralı (İHLAL EDİLEMEZ, §6):** bir kozmetik **palet, materyal/doku, trim, partikül/VFX-rengi, idle/zafer süslemesi** değiştirebilir; **silüet, birim boyutu, hitbox, animasyon zamanlaması, yetenek-VFX okunabilirliği, fraksiyon-renk kimliği**ni **DEĞİŞTİREMEZ**.
- **5 outfit kademe:** Standard → Veteran → Elite → Legendary → Mythic (recolor/material/trim/VFX-renk; üst kademe daha zengin VFX, **aynı silüet**).
- **5 nadirlik:** Common / Rare / Epic / Legendary / Mythic (yalnızca prestij + görsel zenginlik; **stat/okuma avantajı YOK**).
- **Varyant tipleri:** zırh varyantları (görsel), renk şemaları, silah skin'leri (aynı silüet), VFX recolor, **emote/banner (savaş-dışı)**, **portreler**, profil özelleştirme.
- **Ranked "clarity mode":** rakipler standart okunur-skin'lerle render edilir → hiçbir kozmetik rekabetçi okumayı asla bozamaz.
- **Fraksiyon paletleri:** Iron Pact = çelik/kobalt (steel/cobalt) · Ashen Horde = kor/öküzkanı (ember/oxblood).
- **İçerik-hızı motoru:** paylaşımlı arketip iskeleti + fraksiyon reskin + material/recolor kademeleri (Blueprint §10; assets-raporu Phase 4 BUY/KITBASH/GENERATE).

## 1.4 Monetizasyon kısıtları & roadmap limitleri (bağlayıcı)

| Kısıt | Kanon kuralı | Kaynak |
|---|---|---|
| **NO Pay-To-Win** | Premium asla raw güç satmaz; yükseltmeler capped; ranked normalize | §2, §9, §10, §15.3 (İHLAL EDİLEMEZ) |
| **Loot box / gacha YOK** | İlkeli CUT; sandıklar earned + disclosed-odds + cosmetic-only | §8, Decision Log §1, §15.3 |
| **Interstitial reklam YOK** | Yalnızca **opt-in rewarded** reklam | §10, Decision Log §1 |
| **Energy/stamina gate YOK** | Session-respecting tasarım | §10, Decision Log §1, §15.3 |
| **Paralı rastgele kutu YOK** | Disclosed odds + dupe-protection olmadan asla | §8, §15.3 |
| **Gems CANNOT** | raw güç · cap-üstü yükseltme · birim/komutan *gücü* · ranked avantajı · gacha-for-power | §9 (hard prohibitions) |
| **Battle Pass biçimi** | 50 tier · **8-hafta sezon** · dual-track (free+premium) · earn-by-play · premium ≈ **$9.99 / ~950 Gems** | §10, Blueprint §7, Phase-4 prompt |
| **Sezon kadansı** | 8 hafta; sezon başına **TEK** yeni içerik slotu (`{unit \| commander \| map}`) | §13 6.2, Decision Log §4 |
| **Full event engine** | Phase 7.6 (DEFER); MVP yalnızca **temel hafta-sonu modifikatörleri** (data-driven) | §7, §13 P4.5/P7.6 |
| **Modlar tek combat çekirdeği paylaşır** | Modifikatörler **data**, yeni sistem değil; hiçbir mod balance'ı çatallamaz | §7 |
| **Ekonomi otoritesi** | **Server-authoritative**; client = obscured CRDT cache (deterrence) | §9, §12 |
| **Disclosed odds** | Rastgelelik olan her yerde açıklanmış olasılık | §8, §10, §15.4 |
| **Reklam geliri** | Opt-in rewarded; gem ödülü (~10/reklam); savaşı kesmez | §9, §10 |
| **Ticari gerçeklik** | Adil monetizasyon ARPU'yu sınırlar → **kozmetik içerik-hızı + tutundurma** başlığı taşır (§16 #1 risk) | §10, §16, Blueprint §12 |

## 1.5 Canon audit — korunan / modernize / yasak / non-goal

| Eksen | Bulgu |
|---|---|
| **Korunan sistemler (PRESERVE)** | Login-streak / günlük kadans / Play-Pass kavramı (ChangeLog §1); diminishing-returns ilk-geçiş ödülleri; 3-tier RemoteConfig resolver (canlı retune, anti-grind); CRDT currency ledger (client cache); HTTP-Date trusted-time anchor; async-first rekabet; Spine/okunur içerik disiplini |
| **Modernize sistemler (MODERNIZE)** | Local-only içerik → **Addressables + CDN** (canlı event/kozmetik için zorunlu); client-random A/B → **server-side experiment assignment**; tek `DifficultyToModifier` → çok-eksenli adaptif director; thin kimlik → **fraksiyon + capped komutan + outfit-class kozmetik** |
| **Yasak sistemler (CUT — İHLAL EDİLEMEZ)** | loot box/gacha-for-power · interstitial reklam · energy/stamina gate · P2W/satılabilir güç · disclosed-odds+dupe-protection'sız paralı kutu · save-state logging · client-authoritative currency · Phase-7 öncesi gerçek-zaman PvP · tetikleyici öncesi biome/clan/3.fraksiyon/komutan-koleksiyonu |
| **Non-goal'lar (hard constraint)** | gerçek-zaman-PvP-öncelikli değil · whale SLG/4X değil · pasif autobattler değil · gacha değil · P2W değil · energy-gated değil |
| **Monetizasyon kısıtı** | kozmetik + battle-pass led; şeffaf; opt-in rewarded reklam; value ladder ($0.99→$99.99) bonus %'si mütevazı yükselir (predatory değil) |
| **Battle pass kısıtı** | dual-track, earn-by-play, 50 tier / 8 hafta, premium ~$9.99/950 Gems, ödüller kozmetik + currency, grindiness'e karşı §8/§9/§10 |
| **Ekonomi kısıtı** | currency soup'tan kaçın (yalnızca 4 MVP currency); diminishing-returns; server-authoritative; capped upgrades; ranked-normalize |
| **Live-ops kısıtı** | 8-hafta kadans (6–8 kişilik ekip için sürdürülebilir, 2-hafta treadmill DEĞİL); temel hafta-sonu modifikatörleri (launch) → tam event engine (Phase 7.6); modifikatörler data |
| **P2W kısıtları** | skill > spend; yükseltmeler capped; ranked normalize; kozmetik gameplay-safe; Gems güç satmaz; komutan ≤%15 |
| **Teknik kısıtlar** | Unity 6 LTS/IL2CPP/URP2D; ECS battle-sim / MonoBehaviour-UI sınırı; server-authoritative ekonomi; never log save-state; mid-range telefon perf bütçesi (sert kapı her faz); content = data (SO→export); Addressables+CDN |

## 1.6 Çelişki kontrolü (contradiction check)

Bu raporun **tüm** tavsiyeleri aşağıdaki kurallara uyacak şekilde tasarlandı. Tam denetim Part 14'tedir; aşağıda ön-tarama:

| Kontrol | Bu raporun duruşu |
|---|---|
| Roadmap ihlali? | **Hayır** — Phase 4/6/7.6'yı ön-tasarlar; faz kapılarını/öncelikleri/sırayı değiştirmez. Phase 4 yetkisinin WITHHELD olduğunu açıkça korur. |
| ADR ihlali? | **Hayır** — ADR-2-001'in "no Phase-4 work / no monetization expansion / no economy inflation" kısıtlarına saygılı; bunları *implemente etmez*, *önerir*. |
| Decision Log ihlali? | **Hayır** — full event engine'i 7.6'ya, ranked/Honor'u 7.1'e, komutan-koleksiyonunu 7.5'e bağlı tutar; hiçbirini öne çekmez. |
| Yeni para birimi uydurma? | **Hayır** — yalnızca 4 MVP currency + (Phase 7) Honor/Event-token + (§8) cosmetic-craft shard kullanır. Yeni currency icat edilmedi. |
| P2W yaratma? | **Hayır** — tüm ödüller kozmetik VEYA capped/ranked-normalize currency; premium yalnızca kozmetik + convenience + (cosmetic-only) Gems. Part 3 + Part 12 ispatı. |
| Ekonomi şişirme? | **Hayır** — implementasyon değerlerine yaslanır; diminishing-returns + günlük cap + server-otorite + RC-tune ile enflasyon önlenir (Part 9/10/14). |
| Monetizasyon kuralı ihlali? | **Hayır** — no loot box, no interstitial, no energy gate, no paid-random-box, disclosed odds, no sellable power. |
| Yetkisiz mekanik? | **İşaretlendi** — bazı event tipleri **yeni sim-hook** önerir (ör. fog-of-war / vision). Bunlar ⚙️ **[YENİ SİM-HOOK — Phase 7.6 ADR gerektirir]** etiketlidir; mevcut mekanikle karşılananlar ♻️ **[MEVCUT MEKANİK]** etiketlidir. Hiçbiri bugün kanona girmez. |
| Gelecek canon çatışması? | **Önlendi** — event-token/Honor yalnızca Phase-7 yuvalarında; battle-pass shell yalnızca 4 MVP currency + kozmetik + shard kullanır. |

> **Önemli dürüstlük notu (Part 9 ön-uyarısı):** Görevin "Gold Economy" başlığı, F2P jargonunda **meta yumuşak-para ekonomisini** kasteder. BULWARK kanonunda meta yumuşak-para **Silver**'dır; **Gold** ise **maç-içi, kalıcı-olmayan** üretim parasıdır (`EconomyTypes.cs`). Part 9 bu ayrımı açıkça korur ve **her ikisini** analiz eder — kanona sadakatin garantisi.

---

# PART 2 — Live-Ops Felsefesi

## 2.1 Başarılı mobil RTS/lane live-ops desenleri — ne işe yarar, neden çöker

| Desen | Örnek tür | **Korunacak (preserve)** | **Kaçınılacak (avoid)** |
|---|---|---|---|
| **Sezonsal battle pass** (dual-track, earn-by-play) | Clash Royale Pass Royale, Brawl Stars Brawl Pass, Fortnite/Halo pass | Öngörülebilir gelir; **zamana saygı** (oynayarak biten iz); kozmetik prestij; "kendini-fonlayan pass" goodwill'i | "Premium+/Ultimate" katmanlarının zorunlu hissettirilmesi; pass'in güç vermesi (CR'nin eski kart-XP tartışması); FOMO-baskısı |
| **Haftalık/hafta-sonu modları** (data-driven modifier) | Clash Royale özel-challenge'lar, Brawl Stars rotasyonu | Taze meta; düşük içerik-maliyeti (data); "her hafta yeni bir sebep"; **mevcut çekirdeği yeniden-çerçeveler** | Balance'ı çatallayan bespoke mekanik; pay-to-enter challenge; "kazanmak için harca" |
| **Sezonsal tema + koleksiyon** | Fortnite sezonları, Apex sezonları | Sezon-heyecanı; kimlik; koleksiyon hedefi; takvimin ritmi | Tema-altında P2W tanıtmak; aşırı-sık tema (içerik-hızı çöküşü) |
| **Günlük/streak tutundurma** | Neredeyse tüm F2P | Düşük-sürtünme alışkanlık döngüsü; login-streak; günlük görev | "Kaçırırsan cezalandırılırsın" sertliği; streak-kaybı paniği (dark pattern) |
| **Ranked sezonları** (async/ghost → leagues) | CR ladder, autobattler ranked | Beceri-ifadesi; sezon-resetı; prestij ödülleri | Ranked'de P2W; pay-to-rank; smurf/exploit; **(BULWARK'ta Phase 7.1, deterministic-replay-gated)** |
| **SLG/whale live-ops** (limited-time bundle, gacha) | Rise of Kingdoms, gacha-RPG | (hiçbiri tam değil) — yalnızca *takvim disiplini* öğrenilir | **Parayla-güç · gacha · stat-merdiveni · sonsuz power-creep · FOMO-coercion** — BULWARK'ın anti-kimliği (Successor §10) |

**BULWARK'ın doğru pozisyonu (kanon §2/§10 + Successor §10):** Live-ops, **beceri-temelli rekabeti ve okunurluğu koruyarak** *heyecan, ifade ve alışkanlık* üretmeli. Gelir **kozmetik içerik-hızı + tutundurma**dan gelir — güçten değil. Bu, oyunun ticari kimliğidir: anti-SLG, anti-whale, anti-gacha.

## 2.2 Tutundurma döngüleri (retention loops) — kanonik kadans

Blueprint §3'ün üç-katmanlı döngüsü, live-ops'un tutundurma omurgasıdır:

```
DAKİKA-DAKİKA (1 maç, 2–5 dk):  maden→eğit→ittir→heykel; spell-draft; possess; komutan
OTURUM-OTURUM (10–30 dk):       mod seç → savaş → Silver+PassXP (+ilk-geçiş Gem) → capped upgrade / kozmetik → günlük görev
META-META (haftalar/sezon):     capped upgrade · BATTLE PASS izi · async LADDER rank · KOZMETİK koleksiyon · kampanya · komutan seviyesi
```

**Tutundurma kadansı:** günlük (görev, login-streak) → haftalık (görev, öne-çıkan mod, **hafta-sonu modifikatörü**) → sezonsal (battle pass, ranked-reset [P7.1], yeni içerik slotu, kozmetik hattı, balance patch). Bu, orijinalin kanıtlanmış günlük/streak/pass döngülerini taşır (dossier §6) ve üstüne **sezonsal + (post-launch) sosyal** omurga ekler.

## 2.3 Event sistemleri — kanonik sınır

- **Launch'ta (Phase 4.5 + 6):** yalnızca **temel hafta-sonu modifikatörleri** (data-driven). Bunlar **mevcut combat çekirdeğini yeniden-çerçeveler** (roster kısıtı, ekonomi-modifier, terrain-yoğunluğu, spell-modifier) — **yeni sistem değil** (§7 "modifiers are data").
- **Phase 7.6'da (DEFER):** **tam event engine** — takvim/event-authoring tooling + sezonsal modlar + **Event-token** para birimi (earned-only, cosmetic-only). Bu rapor tam takvimi (Part 8) *ön-tasarlar*; ancak tooling+token Phase 7.6 tetikleyicisine ("live-ops bandwidth + tooling ready") bağlıdır.

## 2.4 Sezonsal progression — neden 8 hafta

Decision Log §4: sezon kadansı = **8 hafta**, çünkü 6–8 kişilik ekip için **sürdürülebilir** (2-hafta treadmill değil). 8 hafta: bir battle pass'i bitirmeye yetecek kadar uzun (burnout'suz), taze tema-heyecanını koruyacak kadar kısa. Yılda **~6.5 sezon** değil, kasıtlı olarak **4 sezon/yıl** (Part 15) — her sezona nefes ve cila alanı.

## 2.5 BULWARK'ın KORUYACAĞI şeyler

1. **Kozmetik-öncelikli, güç-asla monetizasyon** (§2/§10) — gelirin ahlaki temeli.
2. **Earn-by-play battle pass** + **kendini-fonlayan goodwill** (premium iz yeterli Gem iade eder → adanmış F2P bir sonraki pass'i kazanır; Part 3).
3. **Diminishing-returns + günlük cap** (anti-grind; dossier §6) — "no-life'la zirveye" engellenir → **session respect (P4)**.
4. **Disclosed-odds, cosmetic-only earned sandıklar** (§8) — şeffaflık + güven.
5. **Data-driven modifikatörler** (§7) — düşük-maliyet taze meta; balance çatallanmaz.
6. **Server-authority + RC live-tune** — ekonomi güvenle ayarlanır; exploit'e karşı sağlam.
7. **8-hafta sürdürülebilir kadans** — ekip-ölçeği gerçekçi.

## 2.6 BULWARK'ın KAÇINACAĞI şeyler

1. **Battle pass'te güç** (cap-by-pass stat / pass-exclusive birim gücü) — P2W, İHLAL EDİLEMEZ CUT.
2. **FOMO-coercion** — sahte sayaç, "son şans!" karanlık deseni; yalnızca **dürüst limited-time sezonsal** kabul (§10).
3. **Streak-kaybı paniği / energy gate** — session-disrespect; CUT.
4. **Currency soup** — 4 MVP currency'nin ötesine geçmek (yalnızca Phase-7 Honor/Event-token istisnası).
5. **Pay-to-enter event** / pay-to-rank — beceri-rekabetini bozar.
6. **2-hafta treadmill** — ekip için sürdürülemez; kalite düşer (§16 #8 risk).
7. **Gacha/loot-box bağımlılığı** — ilkeli CUT (Decision Log §1).
8. **Aşırı-sık tema** — içerik-hızı çöküşü (§16 #1 risk); 8-hafta disiplini korur.

---

# PART 3 — Battle Pass Mimarisi

> **Kanonik yuva:** Battle Pass S0 = **Phase 4.2** (shell, NOT STARTED, yetki WITHHELD). Sezonsal pass'ler (S1+) = Phase 6+. Aşağıdaki tüm sayılar **PROVISIONAL · LP/LSD-owned · RC-tunable**; Phase 4 prompt'unun "no hidden monetization changes; pricing/fairness changes need an ADR" kuralına tabidir.

## 3.1 BULWARK Battle Pass Çerçevesi — özet

| Parametre | Değer | Gerekçe / kaynak |
|---|---|---|
| **Tier sayısı** | **50** | Kanon (§10, Blueprint §7, Phase-4 prompt) |
| **Sezon süresi** | **8 hafta (56 gün)** | Kanon (Decision Log §4) |
| **İz yapısı** | **Dual-track:** Free + Premium | Kanon (§10) |
| **Premium fiyatı** | **~$9.99 / ~950 Gems** | Kanon (§10, Blueprint §7) |
| **Kazanım** | **Earn-by-play** (PassXP) — savaş + günlük/haftalık görev + ilk-geçiş + event | Kanon §9 PassXP source'ları |
| **Ödül tipleri** | Kozmetik (skin/banner/portre/emote/profil) · Gems · cosmetic-craft shard · (P7.6) Event-token · küçük Silver (free-track, capped-upgrade yakıtı) | §6, §8, §10 — **güç YOK** |
| **Tier maliyeti** | **~1.000 PassXP/tier** (sabit, okunur) → toplam **~50.000 PassXP** | Provisional; basit + dengelenebilir |
| **Premium iade** | Premium iz boyunca **~1.000–1.300 Gems** (≈ bir sonraki pass'i fonlar) | "Kendini-fonlayan pass" goodwill'i (§2.5) |

## 3.2 İlerleme hızı (progression speed) — sağlıklı eğri

**Tasarım hedefi:** *adanmış* bir F2P oyuncu sezon sonunda **free izi tamamlar ve premium'un çoğunu** alır; *gündelik* bir oyuncu **catch-up + event** ile yetişir; **hiç kimse "no-life'la zirveye" çıkmaz** (session respect, P4).

**PassXP kaynakları (provisional, RC-tunable; `passXpPerBattle=10` tabanı):**

| Kaynak | PassXP | Cap / not |
|---|---|---|
| **Maç (taban)** | kazanç ~20 · kayıp ~10 | Günlük **yumuşak cap** (~250/gün maç-XP'si sonrası azalır) → no-life engeli |
| **Günlük görev** (3/gün) | ~120/görev → ~360/gün | Sıfırlanır; **session respect**'in kalbi |
| **Haftalık görev** (~6/hafta) | ~150/görev → ~900/hafta (~128/gün eşdeğer) | Hafta-boyu esnek |
| **Login streak** | ~40/gün (artan) | Düşük-sürtünme alışkanlık |
| **İlk-geçiş (kampanya, S1 front-loaded)** | level başına ~50 PassXP | Tek-seferlik; yeni oyuncuya hız |
| **Hafta-sonu event katılımı** | ~300–500/event | Catch-up köprüsü (Part 7/8) |

**Sonuç (provisional modelleme):**
- **Adanmış F2P (~750–1.000 PassXP/gün):** ~50.000'i **~50–56 günde** bitirir → free iz + premium tamam. Sağlıklı-sıkı.
- **Gündelik (~300–400/gün):** ~tier 30–35'e ulaşır → catch-up + event ile sezon-sonu tamamlar.
- **Çok-hafif (~150/gün):** ~tier 20 → milestone kozmetiklerin çoğunu yine de görür (her 5 tier'da milestone).

> **İlke:** İz **oynayarak** ilerler, **cüzdanla değil.** Gem ile tier-atlama (level skip) bir **convenience**'tir (see-what-you-buy); ödüller kozmetik olduğu için tier-atlamak **güç vermez** — yalnızca kozmetiği erken alır. Bu, §9 "convenience only" sınırının içindedir.

## 3.3 XP kazanımı — anti-grind & anti-burnout

- **Günlük yumuşak cap:** maç-XP'si günlük eşiğe (~250) ulaşınca azalır → "bütün gün oyna yoksa geri kalırsın" baskısı yok; günde **~5–8 anlamlı maç** izi sağlıklı taşır.
- **Görev odaklı:** PassXP'nin çoğu **görev**lerden gelir (sabit, öngörülebilir, oturum-dostu), saf maç-grind'inden değil.
- **Rested/Catch-up XP (bkz. 3.4):** oynamadığın günlerde küçük bir bonus havuzu birikir → dönüşte hızlı yetişme.
- **Burnout sigortası:** 8-hafta + günlük-cap + görev-odaklılık → 56 gün boyunca "her gün biraz" sürdürülebilir; "ilk hafta no-life sonra bırak" deseni teşvik edilmez.

## 3.4 Catch-up mekanikleri & geç-katılım desteği

| Mekanik | Tasarım | Neden adil |
|---|---|---|
| **Dinlenmiş PassXP (Rested)** | Oynamadığın her gün küçük bir bonus-havuz birikir (capped); dönüşte XP **+%50** (havuz tükenene dek) | Lapsed oyuncuyu yakalatır; aktif oyuncuyu cezalandırmaz |
| **Geç-katılım retroaktif unlock** | Premium'u sezon ortasında/sonunda almak, **o ana dek ulaşılan tüm premium ödülleri geriye-dönük açar** | Geç ödeyen erken ödeyenle aynı değeri alır → güven |
| **Tier-atlama (level skip)** | Gem ile tier satın al (convenience; see-what-you-buy) | Ödüller kozmetik → **güç vermez**; §9 convenience |
| **Hafta-sonu event burst'ü** | Eventler büyük PassXP verir → geri kalanlar hızla köprüler | Erişilebilir; herkese açık |
| **Sezon-sonu lütuf (grace) penceresi** | Sezon bitince **~48 saat** ödül-talep penceresi | Son-gün adaletsizliğini önler |
| **"Sezon hikâyesi özeti"** | Geç-katılana tema/narrative recap'i | Onboarding + bağ |

## 3.5 Neden BULWARK Battle Pass'i PAY-TO-WIN DEĞİLDİR — ispat

1. **Hiçbir tier güç vermez.** İz ödülleri yalnızca: kozmetik (silüet-kilitli, clarity-mode'lu), Gems (yalnızca kozmetik/convenience — §9), cosmetic-craft shard, (P7.6) event-token (cosmetic-only), ve **free-track'te mütevazı Silver**. **Stat-boost / birim-gücü / pass-exclusive-güç YOK** (Part 6 tabloları doğrular).
2. **Silver bile P2W değildir.** Silver yalnızca **capped (maxLevel=5, ~+%24) ve ranked-normalize** yükseltmeleri besler; hem **free iz** hem de **normal oyun** Silver verir; tavanlar herkes için aynıdır. Premium pass Silver'ı **hızlandırabilir** ama **tavanı yükseltemez** → kalıcı güç-farkı yaratmaz (§3, §9, ADR-2-002 disiplini).
3. **Gems güç satın alamaz.** §9 hard-prohibition: Gems raw güç / cap-üstü yükseltme / birim-komutan *gücü* / ranked avantajı satın **alamaz**. Premium-pass Gems'i de bu kurala tabidir.
4. **Ranked nötralize eder.** Clarity-mode (rakip okunur-skin) + talent/upgrade normalizasyonu → pass'in görsel/ekonomik ödülleri **rekabette sıfır avantaj**.
5. **Free iz de kozmetik kazandırır.** Ödemeyen oyuncu da koleksiyon + ifade kazanır → "ödeme = avantaj" değil, "ödeme = daha fazla *prestij/ifade*."
6. **GATE 4 (Fairness Audit) zorunlu.** Phase-4 prompt: "any monetization touching power or readability is a FAIL." Bu pass, o denetimi geçecek şekilde tasarlandı.

> **Sonuç:** BULWARK Battle Pass = **prestij + ifade + alışkanlık motoru**, *güç merdiveni değil*. Bu, P3-Fair-mastery pillar'ının doğrudan ifadesidir.

---

# PART 4 — Sezon 1 Tasarımı: **"DEMİR YEMİNİ"** (The Iron Oath)

> **Kanonik yuva:** İlk canlı sezon = **Phase 6.2** (Entry: GATE 5 SCALE-OR-STOP PASS). 8 hafta. **Tek içerik slotu önerisi: yeni HARİTA** (en düşük balance-riski; launch için en güvenli — Part 16). Tüm tema/kozmetik **advisory**; GD/Art-Director onayı şart.

## 4.1 Tema & narrative

**Tema:** *Savaşın başladığı an — yemin, körük ve ilk seferberlik.* Sınır bir nesildir sessizdi; şimdi Ashen Horde'un akın-bandları Iron Pact'in sınır-kalelerini yokluyor (bu, kampanya seviyesi `1-01 "The Ashen probe the border"` ile birebir örtüşür — narrative süreklilik). Sezon, **ilk büyük seferberliği** anlatır: iki ordu da silahını biliyor, yeminini yeniliyor, körüğünü yakıyor.

**Narrative ark (4 perde, sezonun 8 haftasına yayılır):**
- **Hafta 1–2 (Yemin):** Iron Pact garnizonları "bir adım geri yok" yeminini eder; Ashen şamanları kül-ayinini başlatır. *(Onboarding tonu: dünyaya giriş.)*
- **Hafta 3–4 (Körük):** Demirhaneler gece-gündüz çalışır; ilk çatışmalar sınırı tutuşturur.
- **Hafta 5–6 (İlk Kan):** Geçitlerde ilk büyük muharebeler; her iki taraf da kayıp verir, sertleşir.
- **Hafta 7–8 (Finale: "Sınırda Şafak"):** Sezon-finali topluluk-eventi — sunucu-genel bir "cephe itişi" (Part 8). Kazanan fraksiyon teması bir sonraki sezona "anlatısal momentum" taşır (kozmetik/bayrak, **güç değil**).

**Ton:** Ağırbaşlı, epik, "savaşın eşiği." Iron Pact = disiplin & yemin; Ashen = ritüel & açlık. Bu, iki fraksiyonun **kanonik doktrinini** (§5.1) görselleştirir.

## 4.2 Görseller (visual identity)

| Eksen | Iron Pact (steel/cobalt) | Ashen Horde (ember/oxblood) |
|---|---|---|
| **Sezon motifi** | Körük-ateşi, dövülmüş çelik, **yemin-sancakları**, mum/meşale | Kül-ayini, savaş-boyası, **kemik-totem**, kor |
| **Kozmetik aksanı** | Kobalt + **altın yemin-sigili** (parıltısız, ağırbaşlı) | Öküzkanı + **kül-beyazı ritüel-çizgileri** |
| **VFX recolor (kozmetik)** | Mavi-altın körük-kıvılcımı (idle/zafer süslemesi) | Kül-savruntusu, kor-iz (idle/zafer) |
| **Çevre dressing (event-only, okunurluk-güvenli)** | Garnizon-sancakları, körük-ışığı parallax | Ayin-ateşleri, kül-sis parallax |
| **UI tema** | Demir-mavi pass-paneli, yemin-mührü ilerleme çubuğu | (paylaşılan; fraksiyon-seçimine göre aksan) |

> **Okunurluk kilidi (İHLAL EDİLEMEZ, §6/§11):** Tüm sezon-görselleri **silüet/boyut/hitbox/animasyon-zamanlaması/yetenek-VFX-okunabilirliği/fraksiyon-renk-kimliğini KORUR.** Çevre dressing'i birim-okumasını asla kapatmaz; ranked'de clarity-mode aktif. Kozmetik yalnızca palet/material/trim/VFX-renk/süsleme.

## 4.3 Kozmetikler — "Yemin" (Oath) hattı

| Slot | İçerik (örnek) | Kademe/Nadirlik | Kaynak |
|---|---|---|---|
| **İmza birim skini ×2** (her fraksiyon 1 birim) | Iron: Shieldman "Yeminli Muhafız"; Ashen: Raider "Kül-Yemini" | Legendary (Lv 50 milestone) | Premium pass kapstone |
| **Komutan skini ×2** | Iron Warden "Demir-Yemin Töreni"; Ashen Warchief "Kül-Şaman" | Epic/Legendary | Pass + dükkân |
| **Silah skin'leri** (aynı silüet) | Körük-dövülmüş varyantlar (4–6 birim) | Rare→Epic | Pass + shard-craft |
| **Banner'lar** | Yemin-sancağı, körük-amblemi (savaş-dışı) | Common→Epic | Free + premium iz |
| **Portreler** | İki komutanın yemin-regalia portresi + birim kart-portreleri | Epic | Pass milestone |
| **Emote'lar** (savaş-dışı) | "Yemin et" (kalkan-vuruşu), "Kül-savur" | Rare | Pass + dükkân |
| **Profil özelleştirme** | Yemin-mührü çerçeve, sezon-rozeti, isim-rengi | Common→Legendary | Pass + başarım |
| **VFX recolor paketi** | Mavi-altın / kül-kor idle+zafer süslemesi | Epic | Premium kapstone yakını |

**Tema-tutarlılığı:** Tüm S1 kozmetikleri **aynı "Yemin/Körük" görsel dilini** paylaşır (içerik-hızı: paylaşımlı material/trim kütüphanesi, assets-raporu GENERATE kademesi). Bu, **koleksiyon-tutarlılığı** (set toplama dopamini) + **düşük üretim-maliyeti** (Part 13) sağlar.

## 4.4 Progression hedefleri (S1)

| Hedef | Tasarım | Ödül (kozmetik/non-power) |
|---|---|---|
| **Pass kapstone (Lv 50)** | "Demir Yemini" Mythic-kademe imza-skin + VFX | Mythic kozmetik (silüet-kilitli) |
| **Koleksiyon seti** | "Yemin" hattının 8 parçasını topla | Özel profil-çerçeve + animasyonlu banner |
| **Sezon başarımları** | "10 choke savunması kazan", "ilk-geçiş Act 1 tamamla" | Gems + portre + rozet |
| **Async ladder sezon-ödülü** | Lig-bazlı sezon-sonu kozmetik (clarity-safe) | `gemsPerLadderSeasonReward=50` + ranked-banner *(ranked = P7.1; S1'de async ghost ladder ödülü olarak shell-uyumlu)* |
| **Onboarding hedefi (yeni oyuncu)** | İlk hafta "5 milestone aç" rehberli akış | Hızlı ilk-kozmetik (D1/D7 tutundurma, Part 11) |

> **S1 felsefesi:** İlk sezon **erişilebilir + öğretici + düşük-riskli** olmalı. Yeni içerik = **harita** (balance-nötr). Kozmetik hattı tek-temalı (üretim-dostu). Hedef: oyuncuya "battle pass nasıl çalışır + koleksiyon neden tatmin edici" öğretmek, **güç vaadi olmadan.**

---

# PART 5 — Sezon 2 Tasarımı: **"KÜLLERİN YÜKSELİŞİ"** (Rise of the Ashes)

> **Kanonik yuva:** Phase 6+ (S1'den sonra). 8 hafta. **Tek içerik slotu önerisi: yeni KOMUTAN** (Decision Log §2 komutan-koleksiyon tetikleyicisi "MVP retention validated; commander bones stable" S1-S2'de ateşlenebilir; `future/001` izini izler — capped ≤%15, earnable, premium yalnızca skin). Alternatif: yeni harita. **Advisory.**

## 5.1 Tema & narrative

**Tema:** *Yükseliş ve kuşatma — kül fırtınası cepheyi yutar.* S1'in ilk çatışmalarından sonra Ashen Horde **kabarır**: bir kül-fırtınası/volkanik uyanış sürüyü ileri sürer; sınır yanar. Iron Pact **kuşatma altında** sertleşir — körük-yemini artık bir hayatta-kalma savaşı. Bu sezon **karmaşıklığı artırır** (görevin "progression of complexity" emri): daha fazla event-tipi, mid-season event, ve (öneri) yeni komutan ile taktiksel çeşitlilik.

**Narrative ark (4 perde):**
- **Hafta 1–2 (Kıvılcım Büyür):** Hazard-terrain (lav/kül) genişler; cephe ısınır.
- **Hafta 3–4 (Kuşatma):** Iron Pact kaleleri kuşatılır; "Kırılmaz Sur" doktrini sınanır.
- **Hafta 5–6 (Kül Gelgiti):** Ashen sürü-baskısı zirvede; trade-savaşları.
- **Hafta 7–8 (Finale: "Küllerin Şafağı"):** Topluluk-eventi — kuşatmayı kır / sürüyü dağıt. S1 finalinin kazananı buraya **anlatısal** bağlanır (kozmetik momentum, güç değil).

**Ton:** Daha karanlık, daha sıcak, "savaşın doruğu." S1 (yemin/körük) → S2 (eruption/kuşatma) **görsel kontrast** sağlar — koleksiyon-çeşitliliği + sezon-heyecanı.

## 5.2 Görseller

| Eksen | Iron Pact (kuşatma altında) | Ashen Horde (yükselen) |
|---|---|---|
| **Sezon motifi** | Kuşatılmış kale, kobalt-buz kontrastı, kalkan-duvar, kırık-sur | **Volkanik kül**, kor-altın, eruption, kül-yağışı |
| **Kozmetik aksanı** | Buz-mavi + **isli çelik** (kuşatma-aşınması) | Kor-altın + **erimiş-cam/obsidyen** parıltı |
| **VFX recolor** | Kül-altında-soğuk-mavi savunma-aurası süslemesi | Erime/kül-savruntu zafer süslemesi |
| **Çevre dressing (event-only)** | Kuşatma-iskeleleri, kalkan-sis | Eruption-arka-planı, kül-yağışı parallax |
| **UI tema** | Kül-yağışı pass-paneli, eruption ilerleme-çubuğu | (paylaşılan; fraksiyon-aksanı) |

> Aynı okunurluk-kilidi geçerlidir (§6/§11). Volkanik tema **Hazard terrain'in (lav)** mevcut görsel diliyle uyumludur — yeni mekanik değil, **mevcut TerrainKind'ın tema-dressing'i.**

## 5.3 Kozmetikler — "Kül" (Ash) / "Volkanik" hattı

| Slot | İçerik (örnek) | Kademe/Nadirlik | Kaynak |
|---|---|---|---|
| **İmza birim skini ×2** | Iron: Ironclad "Kuşatma-Kalkanı"; Ashen: Razorbeast "Volkan-Canavarı" | Legendary (Lv 50) | Premium kapstone |
| **Komutan skini** | Yeni S2 komutanı (öneri) "Kül-Lordu" + mevcut 2 komutana volkanik varyant | Epic/Legendary | Pass + dükkân |
| **Silah skin'leri** | Obsidyen/erimiş-cam varyantlar | Rare→Epic | Pass + shard |
| **Banner / portre / emote / profil** | Eruption-sancağı, kül-portre, "kül-savur" emote, volkanik çerçeve | Common→Legendary | Free + premium |
| **VFX recolor paketi** | Kor-altın / buz-mavi idle+zafer | Epic | Kapstone yakını |
| **Mythic kapstone** | "Küllerin Efendisi" — en zengin VFX-recolor + animasyonlu profil | Mythic | Lv 50 premium |

## 5.4 Progression hedefleri (S2) & artan karmaşıklık

| Hedef | S1'den fark (artan karmaşıklık) |
|---|---|
| **Pass kapstone (Lv 50)** | "Küllerin Efendisi" Mythic — S1'le set-tamamlama bonusu (cross-season koleksiyon dopamini) |
| **Koleksiyon meta-hedefi** | S1+S2 hatlarını birleştiren "Demir & Kül" master-set ödülü (uzun-vadeli collector hedefi, Part 11 D90+) |
| **Yeni komutan ustalığı** | (Öneri) S2 komutanı için mastery-yolu (kozmetik prestij; güç değil; `future/001` yetenek-ağacı çerçevesi) |
| **Daha fazla event** | S2 daha çok event-tipi açar (Part 7): Commander Clash, Double Draft, mid-season "Kuşatma" event'i |
| **Sezonsal ladder + (yaklaşan) ranked** | S2 sonu, P7.1 ranked-sezon altyapısının "yumuşak ön-izlemesi" olabilir (advisory; deterministic-replay-gated) |

> **S2 felsefesi:** Karmaşıklığı **yatay** artır (daha çok event, daha çok kozmetik-çeşit, opsiyonel yeni komutan), **dikey güç** değil. Tema-kontrastı (yemin→kül) sezon-heyecanını besler; cross-season set-tamamlama uzun-vadeli koleksiyon-bağı kurar (anti-churn, Part 11).

---

# PART 6 — Battle Pass Ödül İzleri (S1 & S2, Lv 1–50)

> **İzlenebilirlik & güvenlik:** Aşağıdaki **hiçbir** ödül güç, stat-boost veya oynanış-avantajı içermez (görev kısıtı + §6/§9/§10 + GATE 4). İzin verilen tipler: **kozmetik** (skin/banner/portre/emote/profil-özelleştirme), **Gems** (yalnızca kozmetik/convenience — §9), **cosmetic-craft shard** (§8 dupe-craft materyali), **Event-token** (⚠️ **P7.6 currency — DEFER**; shell'de yerine shard verilir), ve **Silver** (yalnızca *free-track*; capped+ranked-normalize yükseltme yakıtı — Part 3.5 ispatı uyarınca **güç-avantajı DEĞİL**).

**Lejant:** 🎨 = kozmetik · 🪙 = Gems · 💠 = cosmetic-craft shard · 🎟️ = Event-token *(P7.6 — bugün yerine 💠)* · 🔩 = Silver *(free-only; non-power)* · ⭐ = **milestone** (her 5/10 tier) · 👑 = sezon kapstone.
**Tüm miktarlar PROVISIONAL · LP/LSD-owned · RC-tunable.** Premium iz tasarım-hedefi: toplam **~1.000–1.100 🪙** (≈ bir sonraki pass'i fonlar — kendini-fonlayan goodwill, §2.5/§3.1). Free iz: ~**150–185 🪙** + düzenli kozmetik.

## 6.1 Sezon 1 — "DEMİR YEMİNİ" ödül izi

| Tier | FREE iz | PREMIUM iz |
|---|---|---|
| 1 | 💠 Yemin-shard ×10 + 🔩 200 | 🎨 "Yemin" banner (Common) + 💠 ×15 |
| 2 | 🎨 Profil-rozet (Common) | 🪙 40 + 💠 ×15 |
| 3 | 💠 ×10 | 🎨 Körük silah-skini parçası (Rare) |
| 4 | 🔩 200 | 🪙 40 + 💠 ×20 |
| ⭐5 | 🪙 15 + 🎨 Yemin-banner (Common) | ⭐ 🎨 Birim-skin "Yeminli Çırak" (Rare) + 🪙 50 |
| 6 | 💠 ×10 | 💠 ×20 + 🎨 emote "Yemin et" (Rare) |
| 7 | 🎨 İsim-rengi (mavi) | 🪙 40 + 🎨 Profil-çerçeve (Rare) |
| 8 | 🔩 250 | 🪙 40 + 💠 ×20 |
| 9 | 💠 ×12 | 🎟️ ×30 *(→💠 shell'de)* + 💠 ×15 |
| ⭐10 | ⭐ 🎨 Banner "Körük" (Rare) + 🪙 20 | ⭐ 🎨 **Komutan portre** "Iron Warden: Tören" (Epic) + 🪙 75 |
| 11 | 💠 ×12 | 🪙 40 + 💠 ×20 |
| 12 | 🔩 250 | 🎨 Silah-skin "Dövülmüş" (Rare) + 💠 ×15 |
| 13 | 🎨 Profil-rozet (Rare) | 🪙 50 + 💠 ×20 |
| 14 | 💠 ×12 | 🪙 50 + 🎨 emote (Rare) |
| ⭐15 | 🪙 15 + 🎨 Birim-renk-şeması (Rare) | ⭐ 🎨 Birim-skin "Yeminli Muhafız" (Epic) + 🪙 50 |
| 16 | 💠 ×12 | 💠 ×25 + 🎟️ ×30 *(→💠)* |
| 17 | 🔩 300 | 🪙 40 + 🎨 Profil-çerçeve (Epic) |
| 18 | 🎨 Banner (Rare) | 🪙 40 + 💠 ×20 |
| 19 | 💠 ×15 | 🎨 VFX-recolor parçası "Mavi-Altın Kıvılcım" (Epic) |
| ⭐20 | ⭐ 🪙 25 + 🎨 Portre (Rare) | ⭐ 🎨 **Komutan skin** "Ashen Warchief: Kül-Şaman" (Epic) + 🪙 75 |
| 21 | 💠 ×15 | 🪙 40 + 💠 ×20 |
| 22 | 🔩 300 | 🎨 emote (Epic) + 💠 ×15 |
| 23 | 🎨 İsim-rengi (altın) | 🪙 40 + 💠 ×25 |
| 24 | 💠 ×15 | 🪙 50 + 🎨 Banner (Epic) |
| ⭐25 | 🪙 20 + 🎨 Birim-skin "Çırak" (Rare, free milestone) | ⭐ 🎨 Silah-skin seti "Körük-Ustası" (Epic) + 🪙 50 |
| 26 | 💠 ×15 | 🪙 50 + 💠 ×20 |
| 27 | 🔩 350 | 🎨 Profil-çerçeve animasyonlu (Epic) |
| 28 | 🎨 Profil-rozet (Epic) | 🪙 40 + 💠 ×25 |
| 29 | 💠 ×18 | 🎟️ ×40 *(→💠)* + 💠 ×15 |
| ⭐30 | ⭐ 🎨 Banner animasyonlu (Epic) + 🪙 15 | ⭐ 🎨 **Birim-skin** "Yemin-Lejyoneri" (Legendary) + 🪙 75 |
| 31 | 💠 ×18 | 🪙 40 + 💠 ×25 |
| 32 | 🔩 350 | 🎨 VFX-recolor parçası (Epic) + 💠 ×15 |
| 33 | 🎨 Portre (Epic) | 🪙 50 + 💠 ×25 |
| 34 | 💠 ×18 | 🪙 50 + 🎨 emote (Epic) |
| ⭐35 | 🪙 20 + 🎨 Renk-şeması (Epic) | ⭐ 🎨 **Komutan VFX** "Yemin-Aurası recolor" (Legendary) + 🪙 75 |
| 36 | 💠 ×18 | 💠 ×30 + 🎟️ ×40 *(→💠)* |
| 37 | 🔩 400 | 🪙 50 + 🎨 Profil-çerçeve (Legendary) |
| 38 | 🎨 Banner (Epic) | 🪙 40 + 💠 ×25 |
| 39 | 💠 ×20 | 🎨 Silah-skin "Yemin-Çeliği" (Legendary parça) |
| ⭐40 | ⭐ 🪙 15 + 🎨 Birim-skin (Epic, free milestone) | ⭐ 🎨 **Birim-skin** "Kül-Akıncısı" (Legendary) + 🪙 75 |
| 41 | 💠 ×20 | 🪙 50 + 💠 ×25 |
| 42 | 🔩 400 | 🎨 emote animasyonlu (Legendary) + 💠 ×15 |
| 43 | 🎨 Portre (Epic) | 🪙 50 + 💠 ×30 |
| 44 | 💠 ×20 | 🪙 50 + 🎨 Banner (Legendary) |
| ⭐45 | 🪙 15 + 🎨 Profil-çerçeve (Epic) | ⭐ 🎨 VFX-recolor tam-set "Mavi-Altın" (Legendary) + 🪙 75 |
| 46 | 💠 ×22 | 🪙 50 + 💠 ×30 |
| 47 | 🔩 450 | 🎨 Komutan-skin varyant (Legendary) + 💠 ×20 |
| 48 | 🎨 Banner (Epic) | 🪙 60 + 💠 ×30 |
| 49 | 💠 ×25 | 🎟️ ×50 *(→💠)* + 🎨 Portre (Legendary) |
| 👑50 | 👑 🎨 Birim-skin "Yeminli" (Legendary, free kapstone) + 🪙 25 | 👑 🎨 **MYTHIC kapstone:** "Demir Yemini" imza-skin + animasyonlu profil + en-zengin VFX-recolor + 🪙 100 |

**S1 toplam (provisional):** Premium ≈ **~1.050 🪙** + ~22 kozmetik parça + 6 milestone-skin/portre/komutan + 👑 Mythic. Free ≈ **~165 🪙** + ~2.350 🔩 + düzenli shard + 3 free-skin (Lv 25/40/50) + bol banner/portre/profil.

## 6.2 Sezon 2 — "KÜLLERİN YÜKSELİŞİ" ödül izi

*(Aynı yapı, aynı güvenlik kuralları, aynı ~1.000–1.100 🪙 premium hedefi. Tema: kül/volkanik. S1'le **cross-season set-tamamlama** ödülleri D90+ collector-bağı kurar, Part 11.)*

| Tier | FREE iz | PREMIUM iz |
|---|---|---|
| 1 | 💠 Kül-shard ×10 + 🔩 200 | 🎨 "Kül" banner (Common) + 💠 ×15 |
| 2 | 🎨 Profil-rozet (Common) | 🪙 40 + 💠 ×15 |
| 3 | 💠 ×10 | 🎨 Obsidyen silah-skin parçası (Rare) |
| 4 | 🔩 200 | 🪙 40 + 💠 ×20 |
| ⭐5 | 🪙 15 + 🎨 Kül-banner (Common) | ⭐ 🎨 Birim-skin "Kül-Çırağı" (Rare) + 🪙 50 |
| 6 | 💠 ×10 | 💠 ×20 + 🎨 emote "Kül-savur" (Rare) |
| 7 | 🎨 İsim-rengi (kor-altın) | 🪙 40 + 🎨 Profil-çerçeve (Rare) |
| 8 | 🔩 250 | 🪙 40 + 💠 ×20 |
| 9 | 💠 ×12 | 🎟️ ×30 *(→💠 shell'de)* + 💠 ×15 |
| ⭐10 | ⭐ 🎨 Banner "Eruption" (Rare) + 🪙 20 | ⭐ 🎨 **Komutan portre** "Ashen Warchief: Kül-Lordu" (Epic) + 🪙 75 |
| 11 | 💠 ×12 | 🪙 40 + 💠 ×20 |
| 12 | 🔩 250 | 🎨 Silah-skin "Erimiş-Cam" (Rare) + 💠 ×15 |
| 13 | 🎨 Profil-rozet (Rare) | 🪙 50 + 💠 ×20 |
| 14 | 💠 ×12 | 🪙 50 + 🎨 emote (Rare) |
| ⭐15 | 🪙 15 + 🎨 Birim-renk-şeması (Rare) | ⭐ 🎨 Birim-skin "Volkan-Akıncısı" (Epic) + 🪙 50 |
| 16 | 💠 ×12 | 💠 ×25 + 🎟️ ×30 *(→💠)* |
| 17 | 🔩 300 | 🪙 40 + 🎨 Profil-çerçeve (Epic) |
| 18 | 🎨 Banner (Rare) | 🪙 40 + 💠 ×20 |
| 19 | 💠 ×15 | 🎨 VFX-recolor parçası "Kor-Altın" (Epic) |
| ⭐20 | ⭐ 🪙 25 + 🎨 Portre (Rare) | ⭐ 🎨 **Yeni-komutan skin** (öneri) "Kül-Lordu: Tören" (Epic) + 🪙 75 |
| 21 | 💠 ×15 | 🪙 40 + 💠 ×20 |
| 22 | 🔩 300 | 🎨 emote (Epic) + 💠 ×15 |
| 23 | 🎨 İsim-rengi (obsidyen) | 🪙 40 + 💠 ×25 |
| 24 | 💠 ×15 | 🪙 50 + 🎨 Banner (Epic) |
| ⭐25 | 🪙 20 + 🎨 Birim-skin "Kül-Çırağı" (Rare, free milestone) | ⭐ 🎨 Silah-skin seti "Obsidyen-Usta" (Epic) + 🪙 50 |
| 26 | 💠 ×15 | 🪙 50 + 💠 ×20 |
| 27 | 🔩 350 | 🎨 Profil-çerçeve animasyonlu (Epic) |
| 28 | 🎨 Profil-rozet (Epic) | 🪙 40 + 💠 ×25 |
| 29 | 💠 ×18 | 🎟️ ×40 *(→💠)* + 💠 ×15 |
| ⭐30 | ⭐ 🎨 Banner animasyonlu (Epic) + 🪙 15 | ⭐ 🎨 **Birim-skin** "Kuşatma-Kalkanı: Ironclad" (Legendary) + 🪙 75 |
| 31 | 💠 ×18 | 🪙 40 + 💠 ×25 |
| 32 | 🔩 350 | 🎨 VFX-recolor parçası (Epic) + 💠 ×15 |
| 33 | 🎨 Portre (Epic) | 🪙 50 + 💠 ×25 |
| 34 | 💠 ×18 | 🪙 50 + 🎨 emote (Epic) |
| ⭐35 | 🪙 20 + 🎨 Renk-şeması (Epic) | ⭐ 🎨 **Komutan VFX** "Eruption-Aurası recolor" (Legendary) + 🪙 75 |
| 36 | 💠 ×18 | 💠 ×30 + 🎟️ ×40 *(→💠)* |
| 37 | 🔩 400 | 🪙 50 + 🎨 Profil-çerçeve (Legendary) |
| 38 | 🎨 Banner (Epic) | 🪙 40 + 💠 ×25 |
| 39 | 💠 ×20 | 🎨 Silah-skin "Volkanik-Çelik" (Legendary parça) |
| ⭐40 | ⭐ 🪙 15 + 🎨 Birim-skin (Epic, free milestone) | ⭐ 🎨 **Birim-skin** "Volkan-Canavarı: Razorbeast" (Legendary) + 🪙 75 |
| 41 | 💠 ×20 | 🪙 50 + 💠 ×25 |
| 42 | 🔩 400 | 🎨 emote animasyonlu (Legendary) + 💠 ×15 |
| 43 | 🎨 Portre (Epic) | 🪙 50 + 💠 ×30 |
| 44 | 💠 ×20 | 🪙 50 + 🎨 Banner (Legendary) |
| ⭐45 | 🪙 15 + 🎨 Profil-çerçeve (Epic) | ⭐ 🎨 VFX-recolor tam-set "Kor-Altın" (Legendary) + 🪙 75 |
| 46 | 💠 ×22 | 🪙 50 + 💠 ×30 |
| 47 | 🔩 450 | 🎨 Komutan-skin varyant (Legendary) + 💠 ×20 |
| 48 | 🎨 Banner (Epic) | 🪙 60 + 💠 ×30 |
| 49 | 💠 ×25 | 🎟️ ×50 *(→💠)* + 🎨 Portre (Legendary) |
| 👑50 | 👑 🎨 Birim-skin "Kül-Yükselişi" (Legendary, free kapstone) + 🪙 25 | 👑 🎨 **MYTHIC kapstone:** "Küllerin Efendisi" imza-skin + animasyonlu profil + en-zengin VFX + **"Demir & Kül" cross-season master-set bonusu** (S1+S2 sahibine özel profil-prestiji) + 🪙 100 |

**S2 toplam (provisional):** Premium ≈ **~1.050 🪙** + ~22 kozmetik + 6 milestone + 👑 Mythic + cross-season set-bonusu. Free ≈ **~165 🪙** + ~2.350 🔩 + shard + 3 free-skin + banner/portre/profil.

## 6.3 İz tasarım notları (her iki sezon)

- **Milestone ritmi:** her 5 tier'da bir gözle-görülür kozmetik; her 10 tier'da bir "büyük" parça (skin/komutan/VFX-set); Lv 50 = 👑 Mythic. Bu, sürekli "bir sonraki ödül yakın" hissi verir (dopamine cadence) — **predatory değil, sadece okunur pacing.**
- **Free iz onurludur:** ödemeyen oyuncu da her birkaç tier'da kozmetik + düzenli 🔩 (capped-upgrade yakıtı) + ~165 🪙 alır → "F2P de koleksiyon yapar" (Part 12).
- **Premium kendini-fonlar:** ~1.050 🪙 ≈ 950-Gem'lik bir sonraki pass'e yeter → **adanmış F2P, ilk pass'i kazandıktan sonra Gem-tasarrufuyla zincirleme premium kalabilir** (sektörün en güçlü goodwill mekaniği). Bu, ARPU'yu *düşürür* gibi görünür ama **tutundurma + ağızdan-ağıza güveni** maksimize eder (kanon ticari stratejisi, §10).
- **Event-token (🎟️) bugün YOK:** P7.6 currency olduğu için, Phase 4/6 shell'inde **yerine cosmetic-craft shard (💠) verilir**; tablolar 7.6 geldiğinde token'a geçecek şekilde yazıldı (ileri-uyumlu).
- **Hiçbir satırda güç yok:** tablo, GATE 4 fairness-audit'ini (zero P2W) geçecek şekilde adversarial tasarlandı (Part 14 denetimi).

---

# PART 7 — Hafta-Sonu Event Sistemi (≥12 tip)

> **Kanonik sınır (KRİTİK):** Launch'ta yalnızca **temel hafta-sonu modifikatörleri** ship eder (Phase 4.5/6); bunlar **data-driven** olmalı ve **mevcut combat çekirdeğini yeniden-çerçevelemeli** — "modifiers are data, not new systems" (§7). Hiçbiri balance'ı çatallamaz, bespoke mekanik eklemez. **Tam event engine + Event-token ödülü = Phase 7.6 (DEFER).**
> **Etiketler:** ♻️ **[MEVCUT MEKANİK]** = bugünkü roster/terrain/formasyon/spell/statue/ekonomi data'sıyla karşılanır (RC/data-config). ⚙️ **[YENİ SİM-HOOK]** = Phase 7.6'da ADR + yeni sim-hook gerektirir (öneri, implementasyon değil).
> **Ödül kuralı:** TÜM event ödülleri **kozmetik + PassXP + Gems + shard + (P7.6) Event-token** — **güç YOK** (no stat-modifier kalıcı; modifikatör yalnızca o event-maçında, hem oyuncuya hem rakibe **simetrik** uygulanır → avantaj satılamaz).

## 7.1 Event kataloğu (14 tip)

| # | Event (TR / EN) | Kurallar & Modifikatör (data) | Ödüller (non-power) | Süre | Tekrar-oynanabilirlik | Mekanik |
|---|---|---|---|---|---|---|
| 1 | **Büyücü Ayini** / *Mage Only* | Yalnızca Caster (Battlemage/Hexcaster) + Miner eğitilebilir; roster-filtre. AoE/sinerji-ağırlıklı meta | Caster-temalı 🎨 + PassXP + 🪙 | 48 s (Cmt–Pz) | **Yüksek** — her draft farklı sinerji | ♻️ roster-filtre (Training data) |
| 2 | **Komutan Düellosu** / *Commander Clash* | Komutan active cooldown −%40 (zamanlama-yoğun); **güç-bütçesi DEĞİŞMEZ (≤%15 inviolable)** | Komutan kozmetik shard + 🎨 + PassXP | 72 s (Cum–Pz) | **Yüksek** — ability-timing ustalığı | ♻️ cd komutan-asset data'sı |
| 3 | **Sonsuz Kuşatma** / *Endless Survival* | Endless modu; adaptif director sertleştirilir (`endlessGrowthPerWave` ↑, band sabit); hafta-sonu **leaderboard** | Lig-tier kozmetik + 🪙 + 🔩 (+P7.6 🎟️) | 72 s | **Çok yüksek** — skor-zirvesi + run-varyasyonu | ♻️ Endless + director-band data |
| 4 | **Kör Cephe** / *Fog of War Extreme* | Rakip ordu-kompozisyonu temas-öncesi gizli; vision/reveal kısıtlı; keşif/konum ödüllü | Keşif-temalı 🎨 + PassXP | 48 s | **Çok yüksek** — bilgi-savaşı her maç farklı | ⚙️ **YENİ:** player-facing fog/vision hook (influence-map var, oyuncu-fog'u yok) → **P7.6 ADR** |
| 5 | **Kıtlık** / *Economic Crisis* | Maden-yield −%40 + miner-cap −1; sıkı-ekonomi, verimli oyun | Ekonomi-temalı 🎨 + PassXP + 🪙 | 48 s | **Yüksek** — kıt-kaynak kararları | ♻️ mine-yield + miner-cap (RC/data) |
| 6 | **Çifte Kehanet** / *Double Draft* | 3 yerine **6 büyü** draft edilir (veya 2× charge); combo-patlaması | Büyü-temalı 🎨 + shard + PassXP | 72 s | **Çok yüksek** — sinerji-uzayı patlar | ♻️ draft-count config (3→6) |
| 7 | **Altın Hücum** / *Gold Rush* | Maden-yield 2×, hızlı makro, büyük ordular (perf-cap `ladderMaxUnitsPerBattle=60`) | Altın-temalı 🎨 + 🪙 | 48 s | **Yüksek** — tempo/komposizyon | ♻️ mine-yield + GoldBoost + unit-cap data |
| 8 | **Dar Geçit** / *Choke Gauntlet* | Tüm maçlar Choke-yoğun haritada; formasyon-disiplini kritik | Formasyon-temalı 🎨 + PassXP | 48 s | **Yüksek** — pozisyonel ustalık | ♻️ harita-seçim + terrain-yoğunluk data |
| 9 | **Sırt Savaşı** / *Highground War* | HighGround-yoğun (Ridgeline); ranged-değer + konum zirvesi | Ranged-temalı 🎨 + PassXP | 48 s | **Orta-Yüksek** — yükselti kontrolü | ♻️ terrain-yoğunluk data |
| 10 | **Horde Gecesi** / *Horde Night* | Eğitim-maliyeti −%30 + miner-cap +1; kütle-sürü (perf-cap'li) | Sürü-temalı 🎨 + shard | 48 s | **Yüksek** — kütle-yönetimi | ♻️ train-cost + cap data |
| 11 | **Yanan Cephe** / *Scorched Front* | Hazard (lav/zehir) bölgeleri genişler; DoT-terrain her yerde; konum/kaçınma | Hazard-temalı 🎨 + PassXP | 48 s | **Yüksek** — terrain-okuma | ♻️ Hazard-yoğunluk + Burning/Poisoned data |
| 12 | **Ayna Cephesi** / *Mirror Match* | İki taraf da aynı fraksiyon + aynalanmış komposizyon; saf-beceri | Beceri-prestij 🎨 + PassXP + 🪙 | 72 s | **Çok yüksek** — saf-skill, no-comp-avantajı | ♻️ faction-lock data |
| 13 | **Kanıt** / *The Proving (No Upgrades)* | Tüm birimler **base-stat'a normalize** (capped-upgrade'ler devre-dışı; `RankedNormalized` kancası casual-event'e uygulanır); saf-beceri; **büyük PassXP** | Prestij 🎨 + **büyük PassXP** + 🪙 | 72 s | **Çok yüksek** — anti-P2W vitrin maçı | ♻️ RankedNormalized hook |
| 14 | **Lidersiz Ordu** / *No Commander* | Komutan devre-dışı; saf-çekirdek combat sınanır | Çekirdek-combat 🎨 + PassXP | 48 s | **Orta-Yüksek** — komutansız taktik | ♻️ komutan-toggle |

> **Yedek tipler (Part 8 takvimini doldurmak için, aynı disiplin):** *Şimşek Kuşatması* (statue HP −%40 + kısa kalkan-fazı; 60–90 sn hızlı maçlar; ♻️ statue data) · *Kalkan Duvarı* (Frontline/Shielded ucuz + formasyon-bonusu güçlü; savunma meta; ♻️ cost+formasyon data). Toplam **16 modifikatör havuzu** → 16-haftalık takvim için bol çeşit (Part 8).

## 7.2 Event tasarım ilkeleri (tümü)

1. **Modifikatör = data, sistem değil (§7).** Her event mevcut roster/terrain/formasyon/spell/statue/ekonomi data'sını RC/config ile yeniden-çerçeveler. Yeni combat-sistemi yok → **balance çatallanmaz.**
2. **Simetrik & geçici.** Modifikatör hem oyuncuya hem AI/rakibe aynı uygulanır ve **yalnızca o event-maçında** geçerlidir → **kalıcı güç-avantajı satılamaz** (P2W değil).
3. **Ödül yalnızca kozmetik/PassXP/Gems/shard/(P7.6)token.** Hiçbir event birim-gücü, cap-üstü yükseltme veya ranked-avantajı vermez (Part 14 denetimi).
4. **Pay-to-enter YOK.** Tüm eventler **tüm oyunculara açık + ücretsiz**; premium-pass sahibine "bonus PassXP" gibi *kozmetik-ivme* olabilir ama **erişim/güç farkı yok.**
5. **⚙️ etiketli hook'lar Phase 7.6'da ADR ister.** Bugün yalnızca ♻️ etiketliler shell-uyumlu; #4 (fog) gibi yeni-hook eventler **gelecek tasarım önerisi**, bugünün kanonu değil.
6. **Tekrar-oynanabilirlik = varyasyon kaynağı.** En yüksek-replay eventler (Mirror, Proving, Double Draft, Fog) **beceri/bilgi/combo** uzayını genişletir — RNG-grind değil.

---

# PART 8 — Özel Event Takvimi (16-Hafta Master Takvim)

> 16 hafta = **S1 (hafta 1–8) + S2 (hafta 9–16)**. Dört kadans-katmanı: **Haftalık** (hafta-sonu modifikatörü) · **Aylık** (büyük mid-event) · **Sezon-finali** (hafta 8 & 16) · **Topluluk** (sunucu-genel hedef). Tüm ödüller **kozmetik/PassXP/Gems/shard** — güç YOK. Dükkân **haftalık rotasyon** (öne-çıkan kozmetik + şeffaf-fiyatlı bundle; FOMO-coercion YOK, yalnızca dürüst limited-time, §10).

## 8.1 Görsel takvim

| Hafta | Sezon / Perde | 🗓️ Hafta-Sonu Event | ⭐ Aylık / Özel | 🛒 Dükkân Rotasyonu | 📜 Narrative beat |
|---|---|---|---|---|---|
| **1** | S1 · Yemin | **Ayna Cephesi** (adil-launch, saf-beceri) | 🎉 **Launch kutlaması** "İlk Seferberlik" (topluluk) | "Yemin" hattı debut + starter-offer | Garnizonlar yemin eder |
| **2** | S1 · Yemin | **Dar Geçit** | — | Yemin silah-skin bundle | Ashen sınırı yokluyor |
| **3** | S1 · Körük | **Büyücü Ayini** | ⭐ **Körük Haftası** (büyük büyü-event'i) | Caster-kozmetik vitrin | Demirhaneler yanar |
| **4** | S1 · Körük | **Altın Hücum** | — | Komutan-portre bundle | İlk kıvılcımlar |
| **5** | S1 · İlk Kan | **Çifte Kehanet** | 🤝 **Topluluk hedefi** "İlk Kan" (sunucu-genel kozmetik unlock) | Banner/emote vitrin | Geçitlerde ilk muharebe |
| **6** | S1 · İlk Kan | **Sırt Savaşı** | — | VFX-recolor vitrin | Her iki taraf sertleşir |
| **7** | S1 · İlk Kan | **Komutan Düellosu** | — | Legendary-skin vitrin | Komutanlar sahaya iner |
| **8** | S1 · **FİNALE** | **Sonsuz Kuşatma** (leaderboard finali) | 🏁 **SEZON FİNALİ** "Sınırda Şafak" (sunucu-genel cephe-itişi; kazanan fraksiyon → S2 anlatısal momentum, **kozmetik**) | Finale-bundle + S1 son-şans (dürüst) | Sınırda şafak söker |
| **9** | S2 · Kül | **Ayna Cephesi** (adil-launch) | 🎉 **S2 launch** + S1→S2 recap (geç-katılana) | "Kül" hattı debut + starter | Kül-fırtınası kabarır |
| **10** | S2 · Kül | **Yanan Cephe** | — | Obsidyen silah-skin bundle | Hazard-cephe genişler |
| **11** | S2 · Kuşatma | **Kıtlık** | ⭐ **Kuşatma Haftası** (savunma-meta büyük-event) | Iron-kuşatma kozmetik vitrin | Kaleler kuşatılır |
| **12** | S2 · Kuşatma | **Horde Gecesi** | — | Komutan-skin bundle | "Kırılmaz Sur" sınanır |
| **13** | S2 · Kül Gelgiti | **Kanıt** (anti-P2W vitrin + büyük PassXP) | 🤝 **Topluluk hedefi** "Kül Gelgiti" | Prestij-kozmetik vitrin | Sürü-baskısı zirvede |
| **14** | S2 · Kül Gelgiti | **Kör Cephe** ⚙️*(P7.6 — fog hook; o güne dek yerine başka modifikatör)* | — | VFX-recolor vitrin | Sis ve trade-savaşı |
| **15** | S2 · Kül Gelgiti | **Lidersiz Ordu** | — | Legendary-skin vitrin | Son hazırlık |
| **16** | S2 · **FİNALE** | **Sonsuz Kuşatma** (leaderboard finali) | 🏁 **SEZON FİNALİ** "Küllerin Şafağı" + **"Demir & Kül" cross-season master-set reveal** | Finale-bundle + master-set vitrin | Küllerin şafağı |

## 8.2 Kadans-katmanı kuralları

| Katman | Sıklık | İçerik | Kanon-yuvası |
|---|---|---|---|
| **Haftalık event** | Her hafta-sonu (Cum/Cmt–Pz) | 16-tip havuzdan rotasyon (Part 7); 48–72 s | Phase 4.5 temel modifikatör → 7.6 engine |
| **Dükkân rotasyonu** | Haftalık | Öne-çıkan kozmetik + şeffaf bundle; sezonsal hat; **dürüst** limited-time | §10 shop rotation |
| **Aylık büyük-event** | ~Her 4 hafta (W3, W11) | Tema-yoğun büyük event (Körük/Kuşatma Haftası) | Phase 7.6 (DEFER) — shell'de "güçlendirilmiş hafta-sonu" |
| **Topluluk hedefi** | Sezon-ortası (W5, W13) | Sunucu-genel **kozmetik** unlock (kolektif katkı → herkese kozmetik) | Phase 7.6 community-event |
| **Sezon finali** | W8, W16 | Sunucu-genel cephe-itişi + leaderboard + finale-bundle | Phase 6 sezon-kapanışı / 7.6 |

> **Sürdürülebilirlik notu:** Bu takvim **8-hafta-sezon kadansına** (Decision Log §4) ve **6–8 kişilik ekibe** kalibre edildi. Haftalık eventler **data-rotasyonu** (sıfır yeni-sanat); aylık/finale eventler mevcut tema-kozmetiklerini yeniden-paketler. **2-hafta treadmill'i bilinçli REDDEDİLDİ** (§16 #8 risk). Topluluk + finale eventleri **Phase 7.6 tooling**'e bağlıdır → launch'ta basit-modifikatör versiyonları, tooling gelince zenginleşir.

---

# PART 9 — "Gold" Ekonomi Analizi (maç-içi Gold + meta-soft Silver)

> **Kanon ayrımı (KRİTİK — §15.1 sadakati):** BULWARK'ta **iki ayrı yumuşak-ekonomi** var (`EconomyTypes.cs`):
> - **Gold** = **maç-içi, KALICI DEĞİL** (ECS `GoldStore`); madenle kazanılır, birim eğitir, **maç bitince yok olur**; sunucu cüzdanında yer almaz.
> - **Silver** = **meta yumuşak-para** (server-owned, kalıcı); savaş/görevle kazanılır, **capped yükseltme + komutan talent** satın alır.
> F2P jargonundaki "gold economy" (meta progression) = BULWARK'ta **Silver**'dır. Bu bölüm **her ikisini** analiz eder.

## 9.1 Maç-içi Gold ekonomisi (üretim döngüsü)

**Nasıl kazanılır:** Miner birimleri sabit maden-düğümlerini (harita başına 2–4) işler; **miner-cap** + **contestable** (kim kontrol ediyor) konum (Roadmap §11, `MineNode.Occupants/Capacity`). Gold → eğitim-sırasına harcanır (counter-komposizyon).

**Sağlıklı oran tasarımı:**
| Risk | Mekanizma | Mitigasyon |
|---|---|---|
| **Maç-içi snowball (Gold-enflasyonu)** | Erken-üstünlük → daha çok maden → ezici ordu | **Contestable mines** (madeni kaybet → gelir düşer) + **statue-objektifi** (saldırıya zorlar) + komutan comeback-tempo (`future/001` Korrash, P7.5) |
| **Ekonomi-pasifliği** | Sadece madenle oyalanma | Statue baskısı + Endless director + AI aggression-ekseni |
| **Maden-yield dengesizliği** | Çok yüksek/düşük yield → maç ritmi bozulur | **RC-tunable** maden-yield (3-tier resolver, §12); event'lerle yeniden-çerçevelenir (Altın Hücum/Kıtlık, Part 7) |

> Gold **kalıcı olmadığı** için meta-enflasyon riski YOK (her maç sıfırdan başlar). Tüm denge **maç-içi** ve **RC ile canlı-ayarlanır** — orijinalin live-tunable ekonomi gücü (dossier §6, [PRESERVE]).

## 9.2 Meta Silver ekonomisi (progression döngüsü)

**Kazanım kaynakları (implementasyon değerleri, PROVISIONAL/RC-tunable):**
| Kaynak | Silver | Kaynak-değer |
|---|---|---|
| Ladder/maç galibiyeti | ~40 | `silverPerLadderWin=40` (Blueprint: win ≈40, loss ≈15) |
| Endless dalga | 8/dalga | `silverPerEndlessWave=8` |
| Kampanya ilk-geçiş | ~60 | level-01 `silverReward=60` |
| Günlük görev | ~100 | Blueprint §7 (~100 Silver / 20 Gems) |
| Battle pass **free** iz | ~2.350/sezon | Part 6 (🔩 satırları) |

**Harcama (sink) — capped, ranked-normalize (no P2W):**
| Sink | Maliyet | Kaynak |
|---|---|---|
| Birim stat-track (HP veya Dmg) L1→5 | ~1.111 Silver (120×1.6^n; `costGrowth=1.6`, `maxLevel=5`) | `UpgradesConfig.asset` |
| Bir birimi tam-yükselt (2 track) | ~2.222 Silver | — |
| **12 birimi tam-yükselt** | ~26.600 Silver | hesaplanan |
| Komutan L1→10 | ~15.000 Silver (200×1.5^n; `commanderCostGrowth=1.5`, `commanderMaxLevel=10`) | `UpgradesConfig.asset` |
| **Her şeyi maks (12 birim + 2 komutan)** | **~56.000 Silver** | uzun-vadeli toplam-sink |

**Sağlıklı progression oranı (provisional modelleme):**
- Adanmış oyuncu ~**700–900 Silver/gün** (10 galibiyet + 3 günlük görev + endless) → **bir birimi maks ≈ 1.5 gün**; **her şeyi maks ≈ 60–80 gün** (çok-aylık, sağlıklı hedef).
- **Ranked normalize ettiği için** (capped + clarity), Silver-ilerlemesi **PvE konfor + async-ladder rahatlığı** verir, rekabette **kalıcı üstünlük değil** → "maks olmayan oyuncu da rekabette eşit."

## 9.3 Aşırı-grind, enflasyon ve ödül-açlığını önleme

| Tehlike | Önleme |
|---|---|
| **Aşırı-grind** | Diminishing-returns ilk-geçiş (`max(5,20−5×replays)`); günlük-görev cap'i; PassXP'nin görev-odaklılığı (Part 3.3); "her şeyi maks ≠ rekabet-zorunluluğu" (ranked normalize) |
| **Enflasyon** | Silver server-authoritative + RC-tune; capped-sink (sonsuz harcama yok); kazanç-oranları telemetri ile canlı-ayar |
| **Silver-doygunluğu (geç-oyun)** | Maks-out sonrası Silver vestijiyel kalır. *(Advisory tuning fikri, Part 16:* küçük **Silver→cosmetic-craft-shard** dönüşüm-sink'i — **güç-sink DEĞİL**, yalnızca kozmetik; gelecek LSD/LP kararı.)* |
| **Ödül-açlığı (erken-oyun)** | Front-loaded ilk-geçişler + cömert erken-görev + free-iz Silver + ilk-hafta onboarding hedefleri (Part 11 D1/D7) |
| **Pay-to-progress baskısı** | Silver **satın alınamaz** (yalnızca Gems satın alınır, Gems Silver'a çevrilmez) → "ödeyerek progression atla" yok; yalnızca kozmetik/convenience |

> **Sonuç:** Gold (maç-içi, kalıcı-değil, snowball-dengeli) ve Silver (meta, capped, ranked-normalize) ekonomileri **enflasyona ve P2W'ye karşı yapısal olarak korunaklı**dır. Silver progression'ı *konfor + koleksiyon-erişimi* verir, *rekabet-avantajı değil* — P3-Fair-mastery'nin ekonomik ifadesi.

---

# PART 10 — Gem Ekonomisi Analizi

> **Kanon:** Gems = premium-para (server-owned); **kazanılır + (Phase 4) satın alınabilir**; harcanır: kozmetik, battle-pass premium, **convenience** (chest-skip, slot), komutan-skin. **Gems CANNOT:** raw güç · cap-üstü yükseltme · birim/komutan *gücü* · ranked avantajı · gacha-for-power (§9 hard prohibitions).

## 10.1 Gem kazanım kaynakları (günlük / haftalık / sezonsal / başarım)

| Periyot | Kaynak | Gem (provisional) | Kaynak-değer |
|---|---|---|---|
| **Günlük** | Günlük görev (3) | ~20/gün → ~600/ay | Blueprint §7 |
| **Günlük** | Login streak | ~5–10/gün (artan) → ~200/ay | §9 login-streak |
| **Günlük** | Opt-in rewarded reklam | 10/reklam, cap ~3/gün → ~900/ay (maks) | `gems_per_ad_watch`≈10 (§9); **opt-in, interstitial değil** |
| **Haftalık** | Haftalık görev | ~50/hafta → ~200/ay | §9 weekly |
| **Haftalık** | Hafta-sonu event | ~50–100/event → ~300/ay | Part 7 |
| **Sezonsal** | Battle pass **free** iz | ~165/sezon (~80/ay) | Part 6 |
| **Sezonsal** | Battle pass **premium** iz | ~1.050/sezon | Part 6 (kendini-fonlayan) |
| **Sezonsal** | Async-ladder sezon-ödülü | 50/sezon | `gemsPerLadderSeasonReward=50` |
| **Tek-seferlik** | Kampanya ilk-geçiş (S1) | ~400 toplam | `max(5,20−5×replays)` × 20 level |
| **Tek-seferlik** | Başarımlar | ~300–500 (yayılı) | §9 achievements |

**Aylık F2P gem-geliri (provisional):**
- **Reklamsız:** ~1.500–2.000 Gems/ay
- **Reklamlı (opt-in maks):** ~2.400–2.900 Gems/ay
- **Premium pass maliyeti:** 950 Gems / 8 hafta (~475/ay eşdeğer)

## 10.2 Oyuncu-iyiniyeti (goodwill) modeli

| Mekanik | Etki | Neden iyiniyet |
|---|---|---|
| **Kendini-fonlayan pass** | Premium iz ~1.050 Gem iade → adanmış F2P bir sonraki pass'i **kazanır** | "Bir kez al, premium kal" — sektörün en sevilen modeli (Fortnite/Halo deseni) |
| **Cömert günlük/event gem'i** | F2P aylık ~1.500–2.900 Gem | Oyuncu *düzenli* kozmetik alabilir → "ödemeyen de koleksiyon yapar" (Part 12) |
| **Opt-in reklam = oyuncu seçimi** | İsteyen reklam-izleyip Gem kazanır; interstitial YOK | Saygılı; savaşı kesmez (§10) |
| **Gem güç satın ALMAZ** | Yalnızca kozmetik/convenience/prestij | Ödeme-baskısı yok; "ödemezsen kaybedersin" duygusu yok → güven |
| **Şeffaf value ladder** | $0.99→$99.99, gösterilen-değer, mütevazı bonus-% | Predatory değil (§10) |

## 10.3 Gem harcama (sink) — yalnızca kozmetik/convenience

| Sink | Örnek fiyat | Güç? |
|---|---|---|
| Battle-pass premium | ~950 Gems | ❌ kozmetik+Gem iade |
| Birim/komutan skin | gem-fiyatlı ($4.99–9.99 eşdeğer) | ❌ silüet-kilitli kozmetik |
| Banner/emote/portre/profil | ucuz ($0.99–2.99 eşdeğer) | ❌ ifade |
| Chest-skip (zamanlayıcı atla) | küçük | ❌ convenience (§8) |
| Ekstra kozmetik-slot | küçük | ❌ convenience |
| Dükkân öne-çıkan / bundle | değişken | ❌ kozmetik |

> **Sonuç:** Gem-ekonomisi **cömert kazanım + yalnızca-kozmetik harcama** ile oyuncu-iyiniyetini maksimize eder. F2P oyuncu düzenli kozmetik + her sezon premium-pass erişimi alır; ödeme **prestij/ifade/zaman** satın alır, **asla güç**. Bu, "ARPU'yu kısan ama tutundurmayı ve güveni büyüten" kanonik ticari stratejinin (§10/§16) ekonomik motorudur.

---

# PART 11 — Tutundurma (Retention) Mimarisi

> **Temel:** Blueprint §3 üç-katmanlı döngü (dakika/oturum/sezon) + dossier'ın kanıtlanmış günlük/streak/pass döngüleri. **Tutundurma, P2W frustrasyonu olmadan; saygı (P4) ile.** Her kademe, *oyuncunun geri dönme sebebini* katmanlar.

| Gün | Hedef | Geri-dönüş motoru (neden döner) | Kanon-kaynağı |
|---|---|---|---|
| **Day 1** | "Bir el daha" + ilk-tat | (1) Combat-eğlencesi (GATE 1 hook — agency + okunur-derinlik); (2) tutorial 2–3 maça dokunmuş (el-tutmasız, §9); (3) **ilk-geçiş Gem'i** + ilk Wood-sandık açılışı; (4) ilk günlük-görev + ilk pass-tier (anında ödül); (5) ilk kozmetik-tadı | §3 loop, §9 onboarding, §8 chest |
| **Day 7** | Alışkanlık kurulumu | (1) **Login-streak** ivmesi (kaçırma-cezası YOK, sadece artan-bonus); (2) ilk **haftalık görev** + ilk **hafta-sonu event**; (3) pass Lv 5/10 milestone-kozmetikleri; (4) async-ladder yerleşimi (rekabet-tadı); (5) ilk koleksiyon-parça seti | §6 streak, Part 7 event, Part 6 pass |
| **Day 30** | Orta-vade yatırım | (1) Pass Lv ~25 + koleksiyon-seti yarısı; (2) ilk **aylık büyük-event + topluluk hedefi** (Part 8 W3/W5); (3) ladder-tırmanışı + görünür upgrade-progression; (4) "set-tamamlama" yakınlığı (collection dopamini); (5) ekonomi-akışı oturmuş (Part 9/10) | Part 6/8, §3 meta |
| **Day 90** | Sezon-geçişi heyecanı | (1) **S1→S2 tema-değişimi** (yemin→kül görsel-kontrast = tazelik); (2) yeni içerik-slotu (harita/komutan); (3) **cross-season set** başlangıcı; (4) returning-player **catch-up** (Rested XP, recap); (5) mastery/prestij yolu (komutan, `future/001` P7.5) | Part 4/5, §13 6.2 |
| **Day 180** | Koleksiyoncu-kimliği & topluluk | (1) **"Demir & Kül" master-set** (2+ sezon kozmetik prestiji); (2) çoklu-sezon collector-kimliği; (3) **(post-launch) ranked-ligler + clan-hedefleri** (P7.1/7.2 — gated); (4) sezon-finali ritüelleri (sunucu-genel); (5) "ustalaştım" ifadesi (mastery, güç değil) | Part 6.2, §7, `future/001` |

## 11.1 Neden oyuncular geri döner — tutundurma-tezi

1. **Katmanlı hedefler:** her oturum (maç-eğlencesi) + her gün (görev/streak) + her hafta (event/pass) + her sezon (tema/koleksiyon/ladder) bir geri-dönüş-sebebi → "her zaman yapacak bir şey var, ama hiçbiri zorunlu değil."
2. **Koleksiyon > güç:** uzun-vade hedef **kozmetik koleksiyon + mastery prestiji**, sonsuz power-grind değil → **burnout düşük, churn düşük** (anti-treadmill).
3. **Adalet = anti-churn:** P2W olmadığı için "ödeyen rakip beni eziyor" frustrasyonu YOK → beceriyle kaybeden "tekrar denerim" der, cüzdanla kaybeden değil (P3 pillar).
4. **Saygı = sürdürülebilir alışkanlık:** günlük-cap + diminishing + 8-hafta kadans → "her gün biraz" 6 ay sürdürülebilir; "ilk-hafta-no-life-sonra-bırak" teşvik edilmez (P4).
5. **Sezon-ritmi:** 8-hafta tema-döngüsü taze-heyecan + öngörülebilir-ritim dengesi (ne sıkıcı tekrar, ne tükenme-hızı).

---

# PART 12 — Whale / Dolphin / F2P Analizi

> **Temel ilke (kanon §10):** BULWARK'ta **harcama tavanı = yalnızca kozmetik + convenience.** Bir "whale" bile **GÜÇ SATIN ALAMAZ** → her segment **avantaj DEĞİL, prestij/ifade/zaman** alır. Bu, ARPU'yu kısar (kabul edilen #1 ticari risk, §16/Blueprint §12) ama **rekabet-bütünlüğünü ve oyuncu-güvenini** korur — kanonik ticari kimlik.

## 12.1 Segment modeli

| Segment | Aylık harcama (illüstratif) | Ne alır | Nasıl değer alır | P2W? |
|---|---|---|---|---|
| **F2P** | $0 | Full oyun erişimi; free-pass kozmetikleri; ~1.500–2.900 Gem/ay → düzenli kozmetik; ranked-eşit | Tam oynanış + yavaş-ama-gerçek koleksiyon + rekabet-eşitliği | ❌ Hayır — güç herkese eşit (capped+normalize) |
| **Light spender (Dolphin)** | ~$5–15 | Sezonsal premium-pass + ara-sıra kozmetik | Premium koleksiyon + **kendini-fonlayan pass** + convenience | ❌ Hayır — premium yalnızca kozmetik/Gem |
| **Battle-pass alıcısı** | ~$10/sezon (~$5/ay) | Her sezon premium-pass | İstikrarlı kozmetik-akışı + Gem-iade (zincirleme premium) | ❌ Hayır — pass güç vermez (Part 3.5 ispatı) |
| **Collector / Whale** | $50–200+ | Tüm kozmetikler, tüm pass'ler, Gem-paketleri, tier-skip, dükkân-bundle | **Tam koleksiyon + Mythic prestij + erken-erişim + ifade** | ❌ Hayır — **güç satın alınamaz**; whale = gönüllü patronaj |

## 12.2 Her segment P2W OLMADAN nasıl değer alır

- **F2P:** "Oyunun tamamını oynuyorum, rekabette eşitim, sabırla koleksiyon yapıyorum." Değer = **erişim + adalet + yavaş-koleksiyon.** (Free iz onuru, Part 6.)
- **Dolphin:** "Sevdiğim sezonun pass'ini aldım, kendini-fonladı, biraz convenience aldım." Değer = **kolaylık + premium-ifade + goodwill.**
- **Pass-alıcısı:** "Her sezon pass alıyorum, Gem-iadesiyle zincirleme premium kalıyorum." Değer = **istikrarlı kozmetik + sıfır-net-maliyet hissi.**
- **Whale/Collector:** "Her kozmetiği topluyorum, en nadir Mythic'lerle prestij gösteriyorum, oyunu destekliyorum." Değer = **tam-koleksiyon + prestij-flex + patronaj** — *ama sahada benden zayıf bir F2P beni yenebilir* (bu, modelin ahlaki çekirdeği).

## 12.3 Whale-tavanı ve ticari gerçeklik

| Boyut | BULWARK duruşu |
|---|---|
| **Whale spend tavanı** | Kozmetik + convenience ile **doğal sınırlı** (sonsuz power-merdiveni YOK → whale "harcayacak güç" bulamaz) |
| **ARPU etkisi** | Daha düşük ARPU (kabul, §16 #1) — **kozmetik içerik-hızı + tutundurma** telafi eder |
| **Rekabet-bütünlüğü** | **Korunur** — whale parası ranked'i etkilemez (clarity + normalize) |
| **Etik konumlanma** | Anti-SLG/anti-whale-predation (Successor §10) → güven + ağızdan-ağıza + uzun-ömür |
| **Risk-azaltma** | Etik-sınırlı fallback kaldıraçları: *daha çok kozmetik/bundle/sezon*, **asla P2W** (§16 mitigasyon) |

> **Sonuç:** Dört segmentin **tümü** değer alır, **hiçbiri** güç satın alamaz. Bu, "fair monetization caps ARPU" gerçeğini (Blueprint §12) kabul eder ve onu **tutundurma + içerik-hızı + güven**le karşılar — kanonik strateji. Whale = oyunun *patronu*, rakiplerin *efendisi* değil.

---

# PART 13 — İçerik Üretim Maliyeti (S1 & S2)

> **Temel:** `future/000-assets-roadmap/` üretim-hattı (BUY/KITBASH/**GENERATE**; paylaşımlı-iskelet reskin; **×5 kozmetik kademe** material/recolor) + Blueprint §13 ekip-gerçeği ("1–2 ongoing artist for cosmetic cadence"; çekirdek in-house, kozmetik/overflow dışarıdan). **Sayılar PROVISIONAL person-week (PW) tahminleri**; kesin değil, ölçek-hissi için.

## 13.1 Tek-seferlik temel (Phase 4 shell — sezonlardan ÖNCE)

*(Bunlar sezon-tekrarlı değil; battle-pass/dükkân/sandık/event sisteminin bir-kez kurulması. Phase 4 = NOT STARTED.)*

| Sistem | İş | Tahmin (PW) | Karar |
|---|---|---|---|
| Battle Pass UI + entitlement | İkili-iz ekranı + server-validated progress | ~3–4 PW | BUILD (marka UI) + backend |
| Shop + IAP + opt-in reklam | Vitrin + value-ladder + rewarded-ads SDK | ~3–4 PW | KITBASH UI + backend |
| Sandık + Gem-kuralları | Disclosed-odds tablo + dupe→shard + timer | ~2–3 PW | BUY/KITBASH model + backend |
| Quest/Streak/Weekend-modifier | Günlük/haftalık + login + data-driven modifier | ~2–3 PW | backend + data |
| Kozmetik-sistem (outfit-class + clarity) | §6 silüet-kilit + 5-kademe material pipeline + ranked clarity | ~3–4 PW | GENERATE pipeline + BUILD kural |
| **Tek-seferlik toplam** | — | **~13–18 PW** | Phase 4 shell |

## 13.2 Sezon-başına tekrarlı maliyet (S1 ve S2 benzer)

| Alan | İş (sezon başına) | Tahmin (PW) | Not |
|---|---|---|---|
| **Sanat — kozmetik hattı** | ~22 premium + ~12 free kozmetik parça (skin/banner/portre/emote/profil/VFX-recolor); tek-tema | **~6–9 PW** | **GENERATE** recolor/material kademeleri + **KITBASH** skin (paylaşımlı iskelet); en büyük kalem |
| **Sanat — imza/milestone** | 2 imza birim-skin + 1–2 komutan-skin + 👑 Mythic kapstone VFX | ~2–3 PW | BUILD-aksan (yüksek-görünürlük) |
| **Sanat — tema dressing** | Sezon UI-teması + event-only çevre-dressing (okunurluk-güvenli) | ~1–2 PW | KITBASH/recolor |
| **UI** | Sezon-pass tematik reskin + event-kartları | ~1 PW | Şablon-tabanlı (tek-seferlik UI üstüne) |
| **Backend** | Pass-içerik config + entitlement + shop-rotasyon + quest-data (**data, yeni sistem değil**) | ~1–2 PW | RC + Addressables-katalog (§12) |
| **Live-ops** | Takvim authoring + event-config + dükkân-rotasyon + telemetri-izleme (8 hafta) | ~2–3 PW (yayılı) | Phase 7.6 tooling olgunlaşınca azalır |
| **Balancing** | Event-modifier tuning + ekonomi RC-tune; **kozmetik balance'a dokunmaz** | ~1 PW | + **S2'de yeni komutan** ise +1–2 PW (≤%15 clamp + counter + ranked-normalize) |
| **Sezon-başı toplam** | — | **~14–21 PW** | S2 ~+1–2 PW (yeni komutan) |

## 13.3 Maliyet-içgörüleri

- **Sanat = en büyük kalem (~%55–60).** Bu, kanonun #1 ticari riskini doğrular: **kozmetik içerik-hızı** (§16/Blueprint §12). Mitigasyon **kanonik**: paylaşımlı-iskelet + reskin + **GENERATE material/recolor** (×5 kademe ucuz) + tek-tema/sezon (üretim-tutarlılığı).
- **Backend/UI sezon-başına UCUZ** çünkü Phase-4 sistemleri **data-driven** (pass/shop/quest/modifier = config + RC + Addressables, yeni-sistem değil, §7/§12). Tek-seferlik kurulum amortize olur.
- **Live-ops, Phase 7.6 tooling olgunlaştıkça düşer** (takvim/event-authoring otomasyonu).
- **Ekip-uyumu:** Blueprint §13 "1–2 ongoing cosmetic artist + part-time live-ops" → sezon-başına ~14–21 PW, 8-haftalık (≈16 takvim-haftası kapasitesi) pencerede **2 artist + part-time live-ops/backend** ile **sürdürülebilir.** Bu, 8-hafta-kadansın *neden* seçildiğinin maliyet-ispatıdır (Decision Log §4).
- **Balancing düşük** çünkü **kozmetik asla balance'a dokunmaz** (§6) ve eventler **simetrik-geçici modifikatör** (Part 7) → kombinatoryal-denge patlaması YOK. Tek istisna: yeni komutan (S2) → capped + counter + ranked-normalize ile sınırlı.

---

# PART 14 — Exploit & Ekonomi Denetimi

> **Temel savunma (kanon §8/§9/§12):** **server-authoritative** ekonomi (client = obscured CRDT cache, `Wallet.SetBalance` THROWS; reconcile FROM server), **HTTP-Date trusted-time anchor** (anti-rollback), **stat-sanity validator** (`ladderMaxPlausibleDps=1000`, `ladderMaxUnitsPerBattle=60`), **disclosed-odds + dupe→shard** (§8), **diminishing-returns + günlük-cap**, **RC live-tune**. Aşağıdaki her risk bu primitiflere bağlanır.

## 14.1 Denetim tablosu

| # | Risk | Vektör | Mitigasyon (kanon-bağlı) |
|---|---|---|---|
| 1 | **Ödül exploit'i (currency dupe)** | Client cüzdan-mutasyonu / memory-edit | **Server-auth wallet** (phase-3: `Wallet.SetBalance` THROWS; CRDT cache yalnızca server'dan reconcile; tüm grant/spend server-doğrular) — §9/§12 PASS |
| 2 | **Saat-manipülasyonu** (günlük/streak/timer farm) | Cihaz saatini ileri al | **HTTP-Date trusted-time anchor** (server-time grant'ları yönetir; §8/§12) — client-saatine güvenilmez |
| 3 | **Çoklu-hesap / alt-farm** (event/ilk-geçiş Gem) | Yeni hesaplarla front-loaded ödül-farm | Platform+device auth; **diminishing-returns** ilk-geçiş (`max(5,20−5×replays)`); ödül server-grant; davranış-anomali telemetri (§9 server-side) |
| 4 | **Ladder/event leaderboard abuse** | AFK-farm, bot, win-trade, implausible-DPS | **`StatSanityValidator`** (`ladderMaxPlausibleDps`, `ladderMaxUnitsPerBattle`, over-cap-upgrade reddi); detect-client/decide-server; ghost-snapshot doğrulama (phase-3 §3.4) |
| 5 | **Enflasyon (over-grant)** | Reward-oranı çok yüksek → currency değer-kaybı | **RC live-tune** kazanç-oranları; **capped-sink** (Silver upgrade tavanı); server-otorite; telemetri-izleme (Part 9) |
| 6 | **Battle-pass abuse (PassXP exploit)** | Bot-farm, win-trade, hızlı-maç-spam | **Günlük yumuşak-cap** (maç-XP azalır, Part 3.3); PassXP server-grant + anomali-tespiti; görev-odaklı XP (grind-direnci) |
| 7 | **Tier-skip / entitlement exploit** | Sahte premium-unlock veya tier-atlama | **Server-validated entitlements** (Phase-4 prompt: "pass entitlements server-validated"); IAP-receipt doğrulama |
| 8 | **Sandık exploit** | Dupe-flood, odds-manipülasyon | **Disclosed-odds server-side**; **dupe→shard** (dead-pull yok); sandık-içeriği server-grant (§8) |
| 9 | **Event-modifier abuse** | Modifikatörü kalıcı/asimetrik kılma | Modifikatör **server-config + simetrik + maça-özel** (Part 7.2); client modifikatör-state'i yetkilendiremez |
| 10 | **Reklam-ödül farm** | Reklam-callback sahteciliği | Server-doğrulanmış rewarded-ad callback; günlük-cap (~3/gün); opt-in (§9/§10) |

## 14.2 Burnout & güven-riskleri (oyuncu-tarafı)

| Risk | Önleme |
|---|---|
| **Grind-burnout** (pass'i bitirme baskısı) | Günlük-cap + görev-odaklılık + 8-hafta + catch-up/Rested-XP (Part 3.3/3.4) → "her gün biraz, no-life zorunlu değil" |
| **FOMO-coercion** | **Sahte-sayaç/karanlık-desen YOK**; yalnızca **dürüst** limited-time sezonsal (§10); sezon-sonu **grace penceresi** (Part 3.4) |
| **Pay-pressure** | Gem güç satın-alamaz → "ödemezsen kaybedersin" hissi YOK; F2P cömert gem + free-iz onuru (Part 12) |
| **Collection-anxiety** | Cross-season set'ler **uzun-vade** (baskısız); kaçırılan sezon-kozmetiği **adil** geri-dönüşle (returning-player) |
| **Event-fatigue** | 16-tip rotasyon (Part 7) + tek hafta-sonu/event → "her hafta taze ama bunaltmayan" |

## 14.3 Denetim sonucu

Tüm tanımlı exploit-vektörleri **mevcut kanon primitifleriyle** (server-auth, trusted-time, stat-sanity, disclosed-odds, diminishing+cap, RC-tune) kapatılabilir. **Yeni anti-cheat sistemi gerekmez** — Phase 0–3'te yapısal-olarak kurulmuş savunmalar (phase-3 §5 INVIOLABLE-PASS'leri) bu live-ops yükünü taşır. **Tek runtime-koşulu:** bu savunmaların **canlı BaaS'a karşı doğrulanması** (şu an DEFERRED — Part 1.2). Burnout/güven-riskleri tasarım-disipliniyle (cap/grace/no-coercion/no-P2W) önlenir.

---

# PART 15 — Live-Ops Yol Haritası (Yıl 1: S1–S4, artan karmaşıklık)

> **Bağlayıcı uyarı:** Bu, **advisory** bir yıl-1 *taslağıdır.* Her post-launch katman **kendi decision-log tetikleyicisine** bağlıdır ve bu rapor **hiçbirini öne çekmez.** S1 = Phase 6; S2+ = Phase 6 sonrası; ranked/Honor = P7.1; event-engine/Event-token = P7.6; komutan-koleksiyonu = P7.5; 3.fraksiyon = P7.3; biome = P7.4 — **hepsi DEFER/GATE.** Kadans = **8 hafta → 4 sezon/yıl** (Decision Log §4).

| Sezon | Phase-yuvası & tetikleyici | Yeni karmaşıklık-katmanı | İçerik-slotu (TEK) | Tema (öneri) | Ön-koşul (kapı) |
|---|---|---|---|---|---|
| **S1** | **Phase 6.2** (Entry: GATE 5 PASS) | **Temel:** battle-pass S1, temel hafta-sonu modifikatörleri, günlük/haftalık görev, login-streak, async-ladder sezon-ödülü. *En düşük karmaşıklık.* | **Yeni HARİTA** (balance-nötr, en güvenli) | **Demir Yemini** | GATE 1/2/3 + Phase-5 **SCALE-OR-STOP LTV** PASS |
| **S2** | Phase 6+ (+ P7.5 trigger ateşlenirse) | **+ Yeni komutan** (capped ≤%15, `future/001` çerçevesi); + daha çok event-tipi; + aylık/topluluk event; + cross-season koleksiyon | **Yeni KOMUTAN** (veya harita) | **Küllerin Yükselişi** | MVP retention validated; commander-bones stable (DL §2) |
| **S3** | + **P7.1 (ranked + Honor + deterministic-replay)**; + P7.6 (event-engine + Event-token) olgunlaşır | **+ Ranked sezonları** (ligler Bronze→Master, *Honor* earned-only-cosmetic currency); + tam event-engine; + (P7.2 trigger ise) clan-hedefleri | **Yeni HARİTA veya KOMUTAN** | *(öneri:* **"Buz ve Demir"** — kış/kuşatma teması, Chilled-sinerji görsel-dressing) | Deterministic sim landed (DL §3); live-ops tooling+bandwidth ready (DL §2) |
| **S4** | + **P7.3 (3. fraksiyon)** (S2 trigger); + P7.4 biome-varyant | **+ 3. fraksiyon sezonu** (yeni doktrin/kit; en büyük karmaşıklık); + biome-mekanik dressing; + olgun topluluk/finale eventleri | **Yeni FRAKSİYON** (sezon-flagship) | *(öneri:* **"Üçüncü Güç"** — yeni fraksiyonun gelişi) | Two-faction balance stable in telemetry (DL §2) |

**Karmaşıklık-ilerlemesi (kasıtlı):**
- **S1 → S4 yatay genişleme:** shell → +komutan → +ranked/event-engine → +fraksiyon. Her sezon **bir** büyük-sistem katmanı ekler (içerik-slotu disiplini, §13 6.2).
- **Her katman gated:** hiçbir şey tetikleyici-öncesi gelmez; ranked deterministic-replay'siz ship edilmez (P2W/exploit riski); event-token tooling'siz gelmez.
- **Monetizasyon sabit kalır:** S1'den S4'e **aynı etik model** (kozmetik + battle-pass + opt-in ads); karmaşıklık **oynanış/sosyal/event**te artar, **monetizasyon-baskısında DEĞİL.**
- **Sürdürülebilirlik:** 4 sezon/yıl × ~14–21 PW/sezon (Part 13) → 2-artist + part-time live-ops/backend ekibi için gerçekçi; her yeni katman (ranked/clan/fraksiyon) **kendi Phase-7 mühendislik-bütçesini** gerektirir (bu rapor onları yetkilendirmez, sıralar).

---

# PART 16 — Nihai Tavsiye

## 16.1 En iyi monetizasyon felsefesi

**Kozmetik + battle-pass led, şeffaf, P2W-asla — kanonun zaten doğru olan modelini koru ve sertçe uygula.** Üç sütun:
1. **Kozmetik-öncelik:** tüm gelir silüet-kilitli, clarity-mode'lu, gameplay-safe kozmetiklerden + convenience'tan. Güç asla satılmaz (§6/§9/§10, GATE 4).
2. **Kendini-fonlayan battle-pass:** premium iz ~1.050 Gem iade → adanmış F2P zincirleme-premium kalır (en güçlü goodwill; Part 3/6/10).
3. **Şeffaflık + saygı:** disclosed-odds, opt-in rewarded reklam (interstitial yok), dürüst value-ladder, no-FOMO-coercion, grace-pencere (§8/§10, Part 14).

> Bu, ARPU'yu kısar (kabul, §16 #1) ama **tutundurma + içerik-hızı + güven**le karşılanır. Bu, oyunun ticari **kimliği**dir — anti-SLG/anti-whale-predation (Successor §10).

## 16.2 En güvenli launch stratejisi

1. **Önce kapıları geç.** Monetizasyon-shell (Phase 4) **yalnızca** GATE 1 (fun) + GATE 2 (depth-playtest) + GATE 3 (server-validated ekonomi) PASS *sonra* başlar; küresel-ölçek **yalnızca** Phase-5 **SCALE-OR-STOP LTV kapısı** PASS sonra. *(Bugün: hepsi DEFERRED; Phase-4 yetkisi WITHHELD — Part 1.2.)*
2. **GATE 4 fairness-audit'i bağlayıcı.** Hiçbir monetizasyon güce/okunurluğa dokunamaz; dokunan = FAIL + reddet (Phase-4 prompt §K).
3. **S1 içerik-slotu = yeni HARİTA** (balance-nötr) → launch-sezonunu balance-riskinden arındır.
4. **Cömert-launch.** İlk sezon **erişilebilir + öğretici + F2P-onurlu** (free-iz, cömert Gem) → güven + ağızdan-ağıza ilk-izlenim.
5. **Soft-launch'ta LTV-doğrula, sonra ölçekle** (etik-sınırlı fallback: daha çok kozmetik/bundle, **asla P2W**).

## 16.3 En güçlü tutundurma stratejisi

**Katmanlı, adil, koleksiyon-odaklı tutundurma** (Part 11): maç-eğlencesi (GATE 1) × günlük görev/streak × haftalık event-rotasyonu × 8-hafta sezonsal tema/koleksiyon × (post-launch) ranked/clan. **Anahtar:** uzun-vade hedef = **koleksiyon + mastery prestiji**, sonsuz power-grind değil → düşük-burnout, düşük-churn, yüksek-güven. Cross-season set'ler (Demir & Kül) D90–D180 collector-kimliği kurar.

## 16.4 Hemen başlatılacak vs post-launch'a ertelenecek

| **HEMEN (Phase 4 shell + Phase 6 S1 — kapılar PASS sonrası)** | **POST-LAUNCH'A ERTELE (gated)** |
|---|---|
| Dual-track battle pass (50 tier / 8 hafta) | Tam **event engine** + takvim-authoring tooling (**P7.6**) |
| Outfit-class kozmetik + ranked **clarity-mode** | **Event-token** para birimi (**P7.6**, earned-only-cosmetic) |
| Dükkân + şeffaf IAP value-ladder + **opt-in rewarded reklam** | **Ranked sezonları** + ligler + **Honor** + deterministic-replay (**P7.1**) |
| Etik sandıklar (disclosed-odds, dupe→shard, no-power) | **Clan'lar** + clan-hedefleri/wars (**P7.2**) |
| Gem-kuralları (§9 prohibitions) | **Komutan koleksiyonu** + talent-ekonomi (**P7.5**, `future/001`) |
| Günlük/haftalık görev + login-streak | **3./4. fraksiyon** sezonu (**P7.3**) |
| **Temel** hafta-sonu modifikatörleri (data-driven; Part 7 ♻️ tipleri) | **Biome**-mekanik dressing (**P7.4**) |
| Async-ladder sezon-ödülü (shell-uyumlu) | Sunucu-genel **topluluk/finale** eventleri (tooling — **P7.6**) |
| S1 tema/kozmetik hattı + yeni harita | ⚙️ Yeni-sim-hook eventler (ör. fog-of-war; **P7.6 ADR**) |

> **Tek-cümle tavsiye:** *BULWARK'ın live-ops/battle-pass/sezonsal-tutundurma çerçevesini, kanonun kozmetik-öncelikli/P2W-asla modeline sadık kalarak; Phase-4 shell'ini kapılar (GATE 1/2/3 + soft-launch LTV) geçtikten sonra cömert ve şeffaf bir S1 ile başlatın; event-engine/ranked/clan/koleksiyon karmaşıklığını yıl boyunca kendi Phase-7 tetikleyicilerine bağlı olarak katmanlayın — heyecanı, tutundurmayı ve koleksiyonu güç-avantajı yaratmadan büyütün.*

---

## Kapanış — yetki kuralı (yinelenen)

Bu rapor **gelecek araştırmadır; yalnızca tavsiye.** Hiçbir şeyi: roadmap'i · kanonu · decision-log'u değiştirmez; gelecek özellik yetkilendirmez; implementasyon başlatmaz; üretim önceliklerini değiştirmez. Yalnızca: **keşfeder · analiz eder · tavsiye eder · değerlendirir.**

**Aktif öncelik değişmedi:** CI/CD doğrulama · APK üretimi · Unity doğrulama · Phase 0–3 kapı borç-eritimi. **Phase 4 yetkisi WITHHELD** (ADR-2-001); battle-pass/dükkân/sandık/reklam **bugün yasaktır** ve GATE 1/2/3 + (Phase 5) LTV kapısı geçene dek yasak kalır. Bu çerçeve o günü **önceden** hazırlar.

Hiçbir ihlal-edilemez kısıt gevşetilmedi: **readability · fairness/no-P2W · server-authority over currency · no save-state logging · perf budget · §15 CUT list.** Tüm provizyonel değerler **LP/LSD-owned · RC-tunable**; kesin balance/fiyat **bir ADR + GATE 4 fairness-audit** gerektirir (ajan tek başına onaylayamaz).

> *Yalnızca dokümantasyon + tasarım-keşfi — implementasyon yok, kod yok, kanon değişikliği yok. Kardeş araştırmalar: `future/000-assets-roadmap/` (kozmetik üretim-hattı), `future/001-commander-talent-system/` (komutan-koleksiyon/talent, P7.5). Çelişki olduğunda kanon (`report/*.md`) kazanır; bu belge ona tabidir.*
