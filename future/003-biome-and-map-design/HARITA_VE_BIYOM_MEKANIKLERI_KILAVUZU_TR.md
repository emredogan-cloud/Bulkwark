# BULWARK — Harita & Biyom Mekanikleri Tasarım Kılavuzu (Gelecek Araştırma)

> **⚠️ STATÜ: GELECEK ARAŞTIRMA İZİ — YALNIZCA TAVSİYE NİTELİĞİNDE.**
> Bu belge **aktif geliştirme akışının parçası DEĞİLDİR.** Aktif akış şudur: **CI/CD doğrulama · APK üretimi · Unity doğrulama · kalan Phase 0–3 kapı borç-eritimi.**
> **Bu belge HİÇBİR ŞEYİ:** roadmap'i değiştiremez · kanonu değiştiremez · karar günlüğünü (decision log) değiştiremez · gelecek özellik yetkilendiremez · üretim önceliklerini değiştiremez · implementasyon başlatamaz · güç bütçelerini değiştiremez · onaylanmamış mekanik ekleyemez.
> **Bu belge YALNIZCA:** keşfeder · analiz eder · tavsiye eder · değerlendirir.
> **Konum:** `future/003-biome-and-map-design/` (canon dökümanlarına dokunulmadı; hiçbir `report/`, `docs/adr/`, `decision log`, `docs/execution/` dosyası değiştirilmedi).
> **Kanonik yuva:** Bu araştırma, **Roadmap §13 Phase 7.4 — "biomes with terrain mechanics tying to §4"** özelliğini önceden tasarlar. Phase 7.4 **ertelenmiştir (DEFER)**; **revisit-trigger (Decision Log §2): _"Terrain layer validated as fun/readable."_** Bu tetikleyici **henüz ateşlenmemiştir** (GATE 2 DEFERRED) → hiçbir şey implemente edilmez. Biome'lar ayrıca **§15 CUT listesinde** "tetikleyici öncesi" yasaktır — yani bu rapor *tasarım* yapar, *implementasyon* değil.
> **Tarih:** 2026-06-03 · **Dil:** Türkçe · **Kalite çıtası:** Lead Level Designer + Lead RTS Designer + Environment Designer + Combat Designer.
> **Etiket sözlüğü:** ♻️ **[MEVCUT MEKANİK]** = bugünkü sim-sözcükleriyle (TerrainKind/StatusKind/FormationType/§4-zinciri) karşılanır · ⚙️ **[YENİ SİM-HOOK]** = Phase 7.4'te bir ADR + yeni sim-hook gerektirir (öneri, implementasyon değil). Bu ayrım, "no implementation / no canon change" kuralına sadakatin garantisidir.

---

# Proje Derin Analizi

*(ZORUNLU ön-araştırma fazı — atlanmadı. Aşağıdaki rekonstrüksiyon şu kaynakların okunmasına dayanır: 5 kanonik döküman [`BULWARK_MASTER_DEVELOPMENT_ROADMAP.md`, `ROADMAP_CHANGELOG.md`, `PRODUCTION_DECISION_LOG.md`, `NEXTGEN_RTS_SUCCESSOR_REPORT.md`, `NEXTGEN_RTS_PRODUCTION_BLUEPRINT.md`] · 5 ADR [0-001, 0-002, 1-001, 2-001, 2-002] · 9 execution-prompt [Phase 0–7 + sistem] · Phase 0–3 implementasyon kodu [`MapDef.cs`, `Phase1Components.cs`, `Phase2Components.cs`, `Terrain.cs`, `Formation.cs`, `InfluenceMap.cs`, `Mining.cs`, `StatueDamage.cs`, `Spell.cs`, `CombatTypes.cs` + `Data/Maps/*.asset`] · 4 faz-tamamlanma raporu · `FIRST_COMPILE_REPORT.md` · `SCAFFOLDING_STATUS.md` · ve kardeş gelecek-araştırma izleri [`future/001`, `future/002`].)*

## 1.1 Mevcut proje durumu (rekonstrüksiyon)

BULWARK, *Stick War* halefi olan mobil-öncelikli (Android/iOS), **tek-cepheli, doğrudan-kontrollü taktiksel RTS-lite** (Unity 6 LTS · IL2CPP · URP-2D · **ECS/DOTS battle-sim**, UI = MonoBehaviour). Çekirdek döngü kanon olarak korunur: **maden → eğit → ittir → heykeli yık** (§3). Üretim felsefesi: on yıl kanıtlanmış çekirdeği koru; orijinali sınırlayan **iki** şeyi modernize et — (a) **sığ savaş alanı → terrain + formasyon + type×armor counter + positional flank + draft-3 büyü sinerjisi**; (b) kırılgan client-trust ekonomi → server-authoritative. Monetizasyon **etik, kozmetik + battle-pass öncülüğünde; asla güç satmaz** (§2, §9, §10).

| Eksen | Kanonik değer |
|---|---|
| **Tür** | Doğrudan-kontrollü taktiksel RTS-lite — **tek-cephe lane-skirmisher**; PvE + async rekabet |
| **Sütunlar (Pillars)** | P1 Agency · P2 Okunabilir derinlik · P3 Adil ustalık · P4 Oyuncuya saygı |
| **İhlal-edilemez kısıtlar** | Okunabilirlik · adalet/no-P2W · para-birimi server-otoritesi · save-state log'lamama · perf bütçesi · §15 CUT listesi |
| **MVP içerik kanonu** | 2 fraksiyon · 12 birim (6+6) · 2 komutan · ~12 büyü (draft 3) · **3 harita (TEK savaş-alanı arketipi, 3 terrain düzeni)** · 4 para birimi |
| **Görsel felsefe** | Temiz stilize 2D, kalın okunur silüet, Spine; **detaydan önce okunabilirlik** |
| **Bu araştırmanın hedefi** | Phase 7.4 (post-launch, ertelenmiş) **biyom-genişleme çerçevesi** — yalnızca ön-tasarım |

## 1.2 Faz durumu & doğrulama borcu (Current State Reconstruction)

**Kullanıcı beyanı:** Phase 0 COMPLETE · Phase 1 COMPLETE · Phase 2 COMPLETE · Phase 3 COMPLETE · Phase 4 NOT STARTED.

**Dürüst implementasyon gerçeği (ADR'ler + faz raporları + ilk-derleme raporundan):** Tüm Phase 0–3 **deliverable'ları AUTHORED, canon-verified, integration-audited ve commit edildi**; kod tabanı `FIRST_COMPILE_REPORT.md` ile **CI'da 0 hata ile DERLENİYOR**. Ancak **çalışma-zamanı doğrulama kapıları DEFERRED** (PASS değil) — bu ortamda Unity 6 editörü / Android cihazı / BaaS yok (ADR-0-001 Blocker A; repo, Blocker B remediation R1 ile açılan temiz `bulwark-clean` deposudur).

| Kapı / Öğe | Durum | Kaynak |
|---|---|---|
| Phase 0 çıkış | **CONDITIONALLY ACCEPTED** | ADR-0-002 |
| Compile (Phase 0–3) | **PASS** (CI-kanıtlı, 0 hata) | FIRST_COMPILE_REPORT |
| Android build / APK | **DEFERRED** (post-compile proje-config) | FIRST_COMPILE_REPORT |
| **GATE 1 (FUN)** | **OPEN / DEFERRED** (on-device "eğlenceli mi?" verdict çalıştırılmadı) | ADR-1-001, ADR-2-001 |
| **GATE 2 (vertical-slice playtest)** | **DEFERRED** (≥%40 session-2 + "okunur & eğlenceli" rubriği çalıştırılmadı) | ADR-2-001 |
| GATE 3 (MVP feature-complete) | **DEFERRED**; **Phase 4'e yetki = WITHHELD** | ADR-2-001 |
| FormationMember kablolaması | **ERTELENDİ** (formasyonlar authored; üyelik ataması yok) | FormationMember_wiring_plan.md |

**Aktif blokerler / kalan doğrulama borcu:** Tek seferlik bir konsolide Unity/cihaz doğrulama geçişi borçlu (CI-green; 200-birim frame-ms; in-editor resolver; BaaS round-trip; **GATE 1 fun-verdict → GATE 2 playtest → GATE 3 server-validated**). Bu borç eritilene dek tüm çalışma-zamanı kapıları DEFERRED. **Hiçbir ihlal-edilemez kısıt gevşetilmedi** — yalnızca *doğrulama zamanlaması* ertelendi (ADR-0-002 §4).

> **Bu araştırma için kritik anlam:** Biyomlar **Phase 7.4'tür** ve revisit-trigger'ları tam olarak **_"terrain layer validated as fun/readable"_**'dir (Decision Log §2). Bu tetikleyicinin ateşlenmesi **GATE 2'nin PASS olmasını** gerektirir — ki GATE 2 şu an **DEFERRED**. Yani biyomlar **çifte-kapılı**dır: (1) terrain/formasyon katmanı playtest'te eğlenceli+okunur kanıtlanmalı (GATE 2), (2) Phase 6 canlı olmalı ve S2+ takvimine girilmeli. Bu rapor o günü **önceden** hazırlar; bugünü değiştirmez. **Pratik sonuç:** biyom tasarımı, mevcut terrain katmanının *üzerine* inşa edilir — terrain katmanı eğlenceli/okunur değilse biyom da olmaz. Bu rapor bu yüzden terrain katmanını "biyomun temel sözcüğü" olarak ele alır.

## 1.3 Mevcut harita / terrain / formasyon / influence-map sistemleri (implementasyon gerçeği)

Biyom sistemi **sıfırdan icat edilmez** — mevcut, *authored & wired* terrain/harita iskeletinin üstüne kurulur. Aşağıdaki sözcükler raporun tamamında doğrudan alıntılanır:

| Sistem | Kanonik/kod sözcüğü | Detay | Durum |
|---|---|---|---|
| **Harita tanımı** | `MapDef` (ScriptableObject) | `frontLength` (≈40), `rows = 3`, `team0StatueX`/`team1StatueX`, `terrain[]`, `mines[]`; 3 MVP haritası: `map_openfield`, `map_chokepass`, `map_ridgeline` | Authored |
| **Savaş-alanı yapısı** | §11 / §4 | **Tek yatay cephe, 3 MANTIKSAL SATIR (row)**, uçlarda heykel, 2–4 maden, harita başına 2–3 terrain özelliği | Kanon |
| **Terrain türleri** | `TerrainKind` | `None, HighGround, Choke, Cover, Hazard` | Authored, wired |
| **Terrain etkisi** | `TerrainFeature` | `AttackMult` (HighGround: giden+), `DefenseMult` (Cover: gelen−), `MoveMult` (Choke: hız−), `HazardDps` (Hazard: DoT) — hepsi **data-driven** | Authored, wired |
| **Terrain işgali** | `TerrainOccupancy` | Birim hangi terrain'de duruyor (Entity.Null = açık zemin); `TerrainSystem` her tick çözer | Authored, wired |
| **Formasyon** | `FormationType` | `Line, Tight, Loose`; `SquadFormation` (Type/Anchor/Facing) + `FormationMember` (SquadId/Slot) | Authored; üyelik **DEFERRED** |
| **Influence-map / hedefleme** | `InfluenceMapSingleton` | Threat-grid: ~32 sütun × **3 satır** × 2 takım, ~4 Hz; aynı-satır tercihi; hedef stickiness ×1.2; `TargetingSystem` hücre okur (O(1)/birim) | Authored, wired |
| **Pozisyonel geometri** | `FacingSystem` / `PositionalSystem` | Facing izlenir → **frontal 1.0 / flank 1.5 / back 2.0** (koni eşikleri `k_FrontConeCos=0.5`, `k_BackConeCos=−0.5`) | Authored, wired |
| **Counter matrisi** | `CounterMatrix` | 5 hasar tipi × 4 zırh = 20 hücre; `final = base × (1+upg) × typeArmor × positional × terrain × difficulty` | Authored, wired |
| **Status efektleri** | `StatusKind` | `None, Chilled, Burning, Poisoned, Stunned, Hasted, Raged, GoldBoost`; `StatusSource {Spell, Commander}` (ADR-2-002) | Authored, wired |
| **Maden** | `MineNode` / `MiningState` | Sabit düğüm, `Capacity` (miner-cap), `Occupants`, `YieldPerSec`, `OwnerTeam` (−1 = çekişmeli); `GoldStore` (savaş-içi, kalıcı değil) | Authored, wired |
| **Heykel** | `StatueState` / `StatuePhase` | `Intact>%66, Cracked %33–66, Breaking 0–33, Destroyed≤0`; `ShieldHealth`/`ShieldActive`; `TrickleThrottle` (küçük-vuruş kısma); Blunt ×1.5 & Fire+burn Structure'a karşı | Authored, wired |
| **Büyü** | `SpellDef` / `Spell.cs` | Draft 3-of-~12; `synergyBonusVsStatus`/`synergyMultiplier` (örn. **Chilled→Shatter**); `telegraphTime>0` zorunlu; kategoriler: Offensive/Control/Economy/Summon/Buff | Authored, wired |
| **Biyom** | — | **Sim'de SIFIR biyom kodu** (Phase 7.4, DEFER; grep ile doğrulandı) | Yok |

> **Mühendislik özeti (raporun temel tezi):** Bir biyom, **§4 zincirine yeni bir hasar yolu eklemez.** Bir biyom = (a) mevcut `TerrainFeature` çarpanlarının harita-genelinde **temalı varsayılanları** + (b) mevcut `StatusKind` sinerjilerinin **bölgesel amplifikasyonu** + (c) az sayıda, açıkça ⚙️-etiketli **yeni sim-hook** (hava-zamanlayıcı, dinamik-hazard, taşıyıcı-zemin, görüş-alanı). Çoğu biyom etkisi **DATA + KOMPOZİSYON**'dur; sistem değil. Bu, "balance explosion (7.3/7.4)" riskine (Phase 7 prompt §I) verilen yapısal yanıttır.

## 1.4 Kanon Denetimi (Canon Audit — harita/biyom merceği)

| Eksen | Kanon |
|---|---|
| **Korunan (PRESERVE)** | **3-satırlı okunur tek-cephe** (lane savaşı, aynı-satır tercihi, statue-priority); maden→eğit→ittir→heykel; doğrudan kontrol; performans-öncelikli O(1) hedefleme; **küçük, okunur içerik seti** (restraint) |
| **Modernize (MODERNIZE)** | Tek statik lane → **cephe + terrain (HighGround/Choke/Cover/Hazard)**; binary backstab → positional flank/back; düz counter → type×armor 5×4; **formasyon (Line/Tight/Loose)** eklendi; heykel → shield-faz + hasar-durumları; maden → çekişmeli miner-cap'li düğüm |
| **Yasak (CUT — §15, ASLA)** | Loot box/gacha-for-power; interstitial reklam; energy kapıları; **pay-to-win / satılabilir güç**; save-state log; client-otoriteli para; **tetikleyici öncesi: real-time PvP, BİYOM, clan, 3. fraksiyon, komutan koleksiyonu** |
| **Non-goals** | Real-time-PvP-öncelikli DEĞİL; whale SLG/4X DEĞİL; pasif autobattler DEĞİL; gacha DEĞİL; P2W DEĞİL; energy-gated DEĞİL |
| **Harita kısıtı** | MVP **3 harita, TEK savaş-alanı arketipi (aynı biyom ailesi)**; sezon tek-içerik-slotu {birim\|komutan\|harita} (§13 6.2); biyom çeşitliliği = post-launch (Decision Log §4) |
| **Biyom kısıtı** | Phase 7.4 (DEFER); trigger = "terrain layer validated as fun/readable"; "**terrain mechanics tying to §4**" (Phase 7 prompt §B.2); content-heavy; **balance-explosion riski** açıkça işaretli |
| **Okunabilirlik kısıtı** | **İHLAL EDİLEMEZ** — silüet, renk-körü-güvenli faksiyon/hasar paleti, telegraph, net can/objektif UI; ranked'de **"clarity mode"** (§6) |
| **Fairness kısıtı** | Skill > spend; **ranked normalize**; simetri zorunlu; strictly-dominant harita/fraksiyon yok |
| **Teknik kısıt** | Battle-sim = ECS/DOTS; perf bütçesi her faz hard-gate (mobil parçacık bütçesi!); içerik **data (SO)→config**; §15.6: **sayısal değerleri icat etme** (LSD-owned, provisional) |
| **Monetizasyon kısıtı** | Biyom/harita asla güç olarak satılmaz; haritalar oynayarak/sezon-slotuyla gelir; biyom-temalı kozmetik **gameplay-safe** olmalı (clarity) |

**Kanon-kutsanmış biyom yönü (Successor §8):** *"Biomes with mechanical teeth. Desert/forest/snow/volcanic that interact with the §4 terrain layer (snow = freeze synergy, forest = cover/ambush, volcanic = burn synergy). Aesthetics **and** tactics."* — Bu rapor bu yönü doğrudan uygular: her biyom **mekanik diş** taşır, salt-görsel değildir.

## 1.5 Çelişki Kontrolü (Contradiction Check) — 2 gerilim açıkça işaretlenir

Profesyonel dürüstlük gereği, görev-çerçevesi ile kapalı-kanon arasında tespit edilen **iki gerilim gizlenmez**, kanon lehine çözülür:

| # | Gerilim | Kapalı kanon ne diyor | Bu raporun çözümü |
|---|---|---|---|
| **G1** | Görev "**3-lane battlefield**" ve "TOP/MIDDLE/BOTTOM lane" der. | §2 Tür: "**single-front lane skirmisher**"; §11 + §4: "**one horizontal front, 3 logical rows**"; kod: `InfluenceMap.Rows=3`, `MapDef.rows=3`. BULWARK bir MOBA değildir; **3 ayrı lane yoktur, tek cephenin 3 mantıksal satırı vardır.** | Görevin "TOP/MIDDLE/BOTTOM lane"'i = **tek cephenin ÜST/ORTA/ALT SATIRI (row/şerit)**. Rapor boyunca "lane" sözcüğü **"satır"** anlamında kullanılır; **yeni bir "3 ayrı lane" mekaniği önerilmez** (bu, tek-cephe türünü ihlal ederdi). Part 8 bu satır-okumasını esas alır. |
| **G2** | Görev Biyom-3 olarak "**Rotfen Marsh (Poison Swamp)**" ister. | Successor §8 biyom örnekleri "Desert/**forest**/snow/volcanic" sayar; kapalı-kanon (Decision Log) **belirli bir biyom listesi taahhüt etmez** — yalnızca "biomes tying to §4" der. | Successor §8 bir **vizyon** dökümanıdır (kapalı kanon §2–12 değil) ve biyomları "örnek" sayar. Dolayısıyla Poison Swamp **geçerli bir aday**dır; "forest" yön ise **Part 14'e (Jungle)** taşınır — ki görevin kendisi de Part 14'te "Jungle" ister. Hiçbir kapalı-kanon taahhüdü ihlal edilmez; 4 spesifik biyom **tavsiye-niteliğinde adaydır**, kanon değil. |

**Yetki Kuralı (binding):** Gelecek araştırma **yalnızca tavsiyedir.** Bu rapor roadmap/canon/decision-log'u değiştiremez, implementasyon/özellik yetkilendiremez; yalnızca **keşfeder, analiz eder, tavsiye eder, değerlendirir.** Buradaki her sayısal değer **PROVISIONAL/LSD-owned**'dır (§15.6/§16); kanon olarak iddia edilmez. Her ⚙️-etiketli sim-hook, Phase 7.4'te bir **ADR** ile değerlendirilmelidir.

**Tüm tavsiyeler şunları İHLAL ETMEZ:** ✗ roadmap ✗ ADR'ler ✗ decision log ✗ güç bütçeleri (komutan ≤%15; ADR-2-002 katman ayrımı korunur) ✗ monetizasyon (biyom = güç satışı **yok**) ✗ readability (telegraph + simetri + clarity zorunlu) ✗ no-P2W ✗ un-counterable/RNG hazard yok ✗ tek-cephe türü (3 satır korunur).

## 1.6 Roadmap kısıtları & gelecek genişleme limitleri

- Biyomlar **post-launch içeriğidir** (Decision Log §2: "S2+"); sezon tek-içerik-slotu {birim\|komutan\|harita} bir biyom-paketini doğrudan içermez → biyom **ayrı bir ADR-kapılı içerik akışıdır** (büyük olasılıkla bir biyom = birden çok sezonun harita-slotlarına yayılır, ya da özel bir biyom-sürümü ADR'si).
- **Balance-explosion** Phase 7 prompt'ta 7.3/7.4'ün açık riskidir → telemetri + RemoteConfig live-tuning + ranked-normalizasyon + **simetri** ile yönetilir (Part 13).
- **Restraint kanonik** (§5): hiçbir terrain/hazard **distinct gameplay etkisi + counter** olmadan gönderilmez. "Salt-görsel biyom" yasak (görev kalite-çıtası).
- **Perf bütçesi** mobil-hard-gate: biyom hava/parçacık efektleri **strict particle budget** içinde kalmalı (§10, §11); görsel zenginlik asla birim-okunabilirliğini gasp edemez.

## 1.7 Bu raporun tasarım pusulası (quality bar)

**Tercih edilen derinlik kaynakları:** formasyon kararı (Line/Tight/Loose takası) · terrain ustalığı (HighGround/Choke/Cover/Hazard kullanımı) · satır-kimliği (üst/orta/alt rolü) · zamanlama (telegraf'lı hava penceresi) · counterplay (her hazard/sinerjinin counter'ı) · **rekabetçi adalet** (simetri + clarity + ranked-normalize).
**Reddedilen kaynaklar:** salt-görsel biyom · kozmetik-only çevre · **rastgele (RNG) hazard** · okunmaz VFX karmaşası · tek-tarafa avantaj veren asimetri · readability'yi gasp eden hava efekti.
**Her biyom:** §4 zincirine bağlanır · **farklı** bir baskın eksen taşır ("every biome must affect gameplay differently") · simetrik & telegraf'lı · ranked-normalize-edilebilir · 3-satır okumasını korur · formasyon kararını **zorlar**.

---

# PART 2 — Biyom Tasarım Felsefesi

## 2.1 RTS biyom/terrain tasarımı — tür analizleri ve BULWARK dersleri

| Kaynak yaklaşım | Örnek tür | **Korunacak (preserve)** | **Kaçınılacak (avoid)** |
|---|---|---|---|
| **Klasik RTS terrain** (high ground, choke, forest, ramp) | SC/AoE/CoH | Terrain'in **kararı değiştirmesi** (pozisyon = avantaj); yüksek-zemin/orman/geçit okuması | Görüş-tamamen-kapatma (mobilde okunmaz); APM-ağır arazi mikro'su |
| **MOBA biyom/objektif** (nehir, çalı, ejderha-çukuru) | MOBA | **Çalı = bilgi oyunu**; tarafsız-bölge çekişmesi; simetrik harita | 3-ayrı-lane yapısı (BULWARK tek-cephe!); jungle-rotası karmaşıklığı |
| **Çevresel hazard** (lav, zehir, fırtına) | ARPG/RTS | Telegraf'lı bölge-reddi; **zamanlama** ustalığı; konumlanma cezası | **RNG hazard** (adil değil); kaçışsız anında-ölüm; görünmez tetikleyici |
| **Hava/iklim sistemleri** (yağmur, kar, sis) | 4X/sim | Periyodik **ritim** (saldırı↔savunma pencereleri); atmosfer | Sürekli/öngörülemez modifiye (okunmaz); tek-tarafa hava avantajı |
| **Biyom-sinerjili savaş** (kar=freeze, lav=burn) | Successor §8 hedefi | **Status-amplifikasyon** = mevcut sinerji ağını derinleştirir; "aesthetics AND tactics" | Sinerjinin **bütçeyi/counter'ı aşması**; tek fraksiyonu zorla-dominant yapması |
| **Survivor/roguelite arena modifiye** | survivor-like | Az içerikle çok varyasyon; **data-driven** modifiye | Görünmez güç; "broken biome draft"; okunmazlık |

## 2.2 BULWARK NEYİ KORUMALI

1. **Tek-cephe, 3-satır okunabilirliği (İHLAL EDİLEMEZ).** Biyom, cepheyi *şekillendirir* (terrain dağılımı, hazard, hava) ama **3-satır okumasını ve aynı-satır hedeflemesini asla bozmaz.** Üst/orta/alt satır kimlikleri biyomla *değişir*, *kaybolmaz* (Part 8).
2. **Tek savaş çekirdeği (§4).** Biyom etkileri `AttackMult/DefenseMult/MoveMult/HazardDps/status-magnitude` yuvalarını besler — **gizli ikinci hasar yolu yok.** Bu, kombinatoryal dengeyi yönetilebilir tutar.
3. **Terrain = mevcut sözlük.** `HighGround/Choke/Cover/Hazard` dört atomu **yeterli**; biyom bunların *temalı dağılımı + amplifikasyonu*dur. Yeni terrain-türü icadı minimumda tutulur (her biri ADR).
4. **Status sinerjisi data-driven.** `synergyBonusVsStatus`/`synergyMultiplier` deseni (Chilled→Shatter) biyom-amplifikasyonunun **doğal kancasıdır** — kar Chilled'i, lav Burning'i, bataklık Poisoned'ı *güçlendirir*; hepsi data.
5. **Simetri + clarity = rekabetçi adalet.** Biyom **her iki oyuncu için aynı kuralı** değiştirir → çeşitlilik adaletsizlik yaratmadan gelir (Part 13).
6. **Restraint.** Harita başına 2–3 terrain özelliği kanonu korunur; biyom bunu *zenginleştirir*, **kalabalıklaştırmaz** (clutter = okunabilirlik düşmanı).

## 2.3 BULWARK NEDEN KAÇINMALI

1. **Salt-görsel biyom (görev yasağı).** Bir biyom yalnızca yeniden-renklendirme değildir; **mekanik diş** taşımalı (Successor §8). Aksi takdirde içerik-maliyeti boşa gider, çeşitlilik sahtedir.
2. **RNG / kaçışsız hazard.** Hiçbir hazard rastgele tetiklenmez veya kaçışsız anında-öldürmez. Her hazard **telegraf'lı + bölgesel + counter'lı + simetrik**. (Kanon §5.3'ün "no un-counterable" ahlakının terrain karşılığı.)
3. **Görüşü tamamen kapatma.** Sis/fırtına **görüş/menzili kısar**, ama mobilde birim-silüetini gizlemez (okunabilirlik). Görüş-azaltma **simetrik + counter'lı (reveal)** olmalı.
4. **Asimetrik harita avantajı.** Bir tarafın daha iyi heykel-konumu/terrain'i olamaz. Aynalı-simetri veya kanıtlanmış-eşdeğer-erişim zorunlu (Part 13).
5. **Biyom-zorlamalı dominant pick.** Bir biyom bir fraksiyonu/komutanı/büyüyü **zorla-kazandıran** hale getirmemeli (örn. bataklık = Hexcaster otomatik-galip). Amplifikasyon **capped + simetrik + ranked-normalize**; counter her zaman var (Part 12, 13).
6. **VFX karmaşası & perf taşması.** Güçlü hava = **daha okunur telegraph**, daha gürültülü değil. Mobil parçacık bütçesi sabit kısıt.
7. **Power-creep'li "yeni biyom = daha güçlü".** Her biyom *farklı* olmalı, *daha güçlü* değil — biyomlar yatay çeşitliliktir, dikey güç merdiveni değil.

---

# PART 3 — Master Biyom Çerçevesi (Scalable Architecture)

**Tasarım temeli:** Biyom, mevcut `MapDef` + `TerrainFeature` + `StatusKind` sistemlerinin üzerine oturan **harita-genelinde bir modifiye katmanıdır.** Tek bir `BiomeProfile` (⚙️ yeni ama **data-only** ScriptableObject) önerilir; dört bileşen-katmanı taşır. Bu mimari, "4 biyom → 8–12 biyom" ölçeklenmesini **sistem eklemeden** sağlar (§3.5).

## 3.1 Biyom Katmanları (4 katman)

| Katman | Ne yapar | Mevcut sözcükle bağ | Etiket |
|---|---|---|---|
| **L1 — Substrat (Zemin)** | Harita-geneli **açık-zemin varsayılan modifiye'si**: temel `MoveMult` (kum/kar/çamur), görüş bias'ı, ve **status-amplifikasyon bias'ı** (hangi StatusKind güçlenir/zayıflar) | `TerrainFeature` varsayılanı (açık zemin = `TerrainKind.None` ama biyom bunu non-nötr yapar) + `StatusQuery` magnitude bias | ⚙️ data-profili + status-bias hook |
| **L2 — Terrain-Karışım** | Biyomun **favori terrain özellikleri** ve temalı görselleri: hangi `TerrainKind` yoğun, nerede, ne sıklıkta | **Tamamen mevcut** `TerrainKind` + `MapDef.terrain[]` | ♻️ (saf data + mevcut sistem) |
| **L3 — Hava/Olay** | Biyom-imzası **periyodik, telegraf'lı, SİMETRİK hava olayı**: tüm savaş-alanını geçici modifiye eder (kum fırtınası, tipi, spor patlaması, püskürme) | Yeni "hava-zamanlayıcı"; etkisi mevcut yuvalara (AttackMult/MoveMult/görüş/status) yazılır | ⚙️ hava-zamanlayıcı hook |
| **L4 — Sinerji (Status-Bias)** | Biyomun **amplifiye ettiği sinerji**: kar→Chilled/Shatter, lav→Burning, bataklık→Poisoned (capped + simetrik) | **Mevcut** `synergyBonusVsStatus`/`synergyMultiplier` deseni; biyom çarpanı = ek capped katsayı | ♻️/⚙️ (mevcut sinerji + bölgesel amplifikasyon-katsayısı) |

> **İlke:** L1+L2+L4 büyük ölçüde **mevcut sistemlerin data-kompozisyonudur** (♻️ ağırlıklı); yalnızca L3 (hava) ve birkaç özel L2 öğesi (taşıyıcı-zemin, dinamik-hazard, görüş-alanı) gerçek **yeni sim-hook** (⚙️) gerektirir. Bu dört hook **bir kez** (ilk biyomlar için) yazılır, sonra **tüm gelecek biyomlar yeniden kullanır** (§3.5).

## 3.2 Gameplay Modifiye'leri — hepsi §4 zincirini besler

Bir biyomun savaş-alanına dokunabileceği **tek kanonik yuva kümesi** (yeni hasar yolu YOK):

| §4 yuvası | Biyom nasıl dokunur | Örnek |
|---|---|---|
| `MoveMult` (hareket) | Substrat + Choke | Kar/çamur açık-zemini yavaşlatır; geçit/köprü throughput'u kısar |
| `AttackMult` (giden hasar) | HighGround + sinerji | Dün/sırt yüksek-zemini; biyom-sinerji status'lü hedefe bonus |
| `DefenseMult` (gelen hasar) | Cover | Orman/kamış/kül-bulutu gelen-menzil hasarını azaltır |
| `HazardDps` (DoT) | Hazard + hava | Lav/zehir-havuzu/yarık; hava-olayı geçici Hazard genişletir |
| `status magnitude/duration` | L4 sinerji + hava | Tipi Chilled uygular; spor Poisoned uzatır; kül Burning büyütür |
| **görüş/reveal** (influence-map acquisition) | L1 substrat + L3 hava | Sis/fırtına/kül hedefleme-menzilini kısar (⚙️ yeni); Cover zaten LoS bloklar |

## 3.3 Çevresel Sistemler (yalnızca 4 yeni ⚙️ hook — hepsi yeniden kullanılabilir)

1. **⚙️ Hava-Zamanlayıcı (`WeatherScheduler`).** Periyodik, **telegraf'lı**, **simetrik** olay penceresi açar; pencere boyunca harita-geneli bir modifiye uygular, sonra söner. Kurallar: süre/cooldown sabit ve **her iki tarafa eşit**; başlangıç **telegraf'lı** (görsel + ses uyarısı); etki **capped**. Tüm biyomlar bunu paylaşır (yalnızca *hangi* modifiye değişir).
2. **⚙️ Dinamik-Hazard (`DynamicHazard`).** Bir `Hazard` bölgesinin **telegraf'la** zaman içinde büyümesi/yer-değiştirmesi (volkanik lav çatlakları). Geometriyi maç-ortasında değiştirir → yeniden-konumlanma kararı. Telegraf zorunlu; asla ani.
3. **⚙️ Taşıyıcı-Zemin (`LoadBearingTerrain`).** Belirli zemin (donmuş nehir) **koşullu** olarak kırılıp Hazard'a dönüşebilir (ağır birim/Tight kütle eşik aşarsa). Formasyon-kritik risk; telegraf'lı çatlak-uyarısı.
4. **⚙️ Görüş-Alanı (`VisionField`).** Influence-map hedefleme-menzilini/reveal'ı bölgesel/geçici kısar (sis/fırtına/kül). Cover'ın mevcut "LoS-blok" tasarımının harita-geneli uzantısı; **simetrik + reveal-counter'lı**.

> Bu dört hook + `BiomeProfile` data-şeması = **biyom altyapısının TAMAMI.** Bunların hepsi Phase 7.4'te tek bir ADR-kümesinde değerlendirilir. Sonrası saf içeriktir (data + art).

## 3.4 Terrain Etkileşimleri (katmanların kompozisyonu)

Biyom derinliği, katmanların **birbirini çarpmasından** doğar — tek tek değil:

- **Glacial örneği:** L3 tipi (Chilled uygular) × L1 derin-kar (MoveMult↓) × L2 donmuş-nehir (taşıyıcı) × L4 Chilled→Shatter amplifikasyonu → "yavaşlamış, donmuş bir hedef kümesi + açılan Shatter penceresi, ama Tight kütle buzu kırma riski taşır." Tek bir terrain bunu vermez; **kompozisyon** verir.
- **Okunabilirlik koruması:** Kompozisyon *taktiksel* derinlik üretir, *görsel* karmaşa değil — her katmanın **ayrı, net bir telegraf'ı** vardır (kar-zemini dokusu, tipi-uyarısı, nehir-çatlak çizgisi, Shatter-parıltısı). Oyuncu her etkiyi *ayrı ayrı* okur.

## 3.5 Genişleme Stratejisi: 4 → 8–12 Biyom (sistem eklemeden)

**Anahtar tez:** İlk 4 biyom, 4 yeniden-kullanılabilir ⚙️ hook'u "öder". 5. biyomdan itibaren **marjinal maliyet = yalnızca data + art** (yeni sistem yok). Her yeni biyom, aynı `BiomeProfile` şemasına oturur:

```
BiomeProfile {
  substrate:  { openGroundMoveMult, visionBias, statusAmplifyBias[] }   // L1
  terrainMix: { favoredTerrainKinds[], density, themedSkins }           // L2 (♻️ mevcut)
  weather:    { event, telegraphTime, duration, cooldown, modifier }    // L3 (paylaşılan hook)
  synergy:    { amplifiedStatus, cappedMultiplier }                     // L4 (♻️ mevcut sinerji)
}
```

| Gelecek biyom | Hangi hook'u yeniden kullanır | Yeni içerik |
|---|---|---|
| Jungle (orman) | VisionField (kanopi=sis), Cover yoğun | data + art (Successor §8 "forest=cover/ambush") |
| Highlands | HighGround yoğun, WeatherScheduler (rüzgâr) | data + art |
| Crystal Wastes | L4 sinerji (Magic-amplify), Cover (kristal LoS) | data + art (+ belki 1 ADR: reflect) |
| Storm Coast | WeatherScheduler (şimşek), DynamicHazard (gelgit) | data + art (+ ADR: Voltaic status = YENİ StatusKind) |
| Necrotic Frontier | L4 (Summon-amplify), HazardDps (çürüme) | data + art (+ ADR varsa) |

> **Sonuç:** 4 hook bir kez yazılır; biyom #5–#12 her biri **bir data-profili + bir modüler art-kiti**dir. Bu, "balance explosion" riskini de yapısal olarak sınırlar: tüm biyomlar **aynı 4-yuva matematiğini** kullandığından, ranked-normalizasyon ve telemetri-ayarı **tek bir sistemle** N biyomu yönetir (Part 13, 15). Yeni status gerektiren biyomlar (Storm Coast=Voltaic) ayrıca ADR-kapılıdır ve bu rapor bunları açıkça işaretler.

---

# PART 4 — Biyom 1: **The Scorched Expanse** (Çöl / "Kavrulmuş Enginlik")

> **Baskın eksen:** *Görüş-ritmi + maruziyet* — açık-hava ranged üstünlüğü ⟷ kum-fırtınası melee-penceresi; turtling cezalandırılır, hareket ödüllendirilir. Diğer 3 biyomdan **farkı**: tek biyom ki görüş-azaltması **periyodiktir** (fırtına ritmi) ve varsayılan zemin **açık/menzilli**dir.

## 4.1 Görsel Kimlik
Uçsuz altın dünler, güneş-ağartmış kemikler, **ısı-titreşimi (heat shimmer)** ufku bulanıklaştırır; eski savaşın paslı zırhları ve **yarı-gömülü heykeller** (oyunun kendi objektif-motifinin yankısı). Palet: kum-sarısı + oksit-turuncu. **Clarity kuralı:** faksiyon renkleri (Iron Pact çelik-kobalt, Ashen kor-okskan) sıcak zemine karşı **yüksek kontrast** kalır; silüetler ısı-titreşiminde bile okunur (titreşim yalnızca *uzak arka-plan* shader'ı, birim-üstü değil).

## 4.2 Çevresel Hikâye Anlatımı
Bu, faksiyon savaşının **ilk** savaş alanı — şimdi bir mezarlık. Mirajlar geçmiş orduları gösterir; kumdan çıkan kırık heykeller, "buranın da bir BULWARK'ı vardı ve düştü" der. Hikâye **mekaniğe gömülüdür**: gömülü heykeller HighGround-dün olarak işlev görür; harabeler Cover/Choke'tur. Salt-dekor yoktur (görev yasağı).

## 4.3 Satır Yapısı (tek cephe, 3 satır)
| Satır | Kimlik | Güç | Zayıflık |
|---|---|---|---|
| **ÜST — Yüksek Dünler** | HighGround-zengin menzil/pozisyon savaşı | `AttackMult`↑ menzilli üstünlük; geniş görüş | Cover yok → **flank'a açık**; fırtınada menzil-bonusu çöker |
| **ORTA — Sert Kil Düzlük (hardpan)** | Hızlı koridor; en yüksek **maruziyet** | En hızlı ilerleme; manevra | Cover yok, flank-eğilimli; **ısı-DoT bias'ı** (turtle cezası) |
| **ALT — Wadi / Harabe** | Cover + dar Choke; pusu/flank | `DefenseMult`↓ gelen-menzil; ambush rotası | Dar → AoE/Choke-baskısına açık; yavaş |

## 4.4 Hava Etkileri — **Kum Fırtınası (Sandstorm)** ⚙️[WeatherScheduler]
Periyodik, **telegraf'lı** (gökyüzü kararır + ses uyarısı), **simetrik**. Pencere boyunca (≈süre/cooldown LSD-owned): görüş & hedefleme-menzili kısalır (⚙️ VisionField) → **HighGround menzil-bonusu bastırılır, ranged değer kaybeder, melee/formasyon penceresi açılır.** Bir **ritim** üretir: dingin açık-hava = ranged-dominant ⟷ fırtına = melee-pencere. Bu ritim, oyuncuyu kompozisyonu ve zamanlamayı *fırtına takvimine göre* okumaya zorlar.

## 4.5 Biyom-Özgü Mekanikler (≥3)
| # | Mekanik | Etki | Etiket |
|---|---|---|---|
| M1 | **Kavurucu Maruziyet (Scorching Exposure)** | L1 substrat: açık hardpan'da uzun kalan birim kademeli ısı alır → **Burning amplifiye + Chilled baskılanır** (çöl ısısı dondurmayı eritir). "Hareket et, turtle'lama" ödülü; statik savunmayı yumuşakça cezalandırır. | ⚙️ status-bias hook |
| M2 | **Dün Yüksek-Zemini (Dune HighGround)** | L2: dünler bol HighGround → ÜST satırda menzilli/pozisyonel savaş baskın; konumlanma ustalığı ödüllü. | ♻️ saf HighGround |
| M3 | **Serap / Görüş-Kısması (Mirage)** | L1+L3: ısı-titreşimi reveal & influence-map hedefleme-menzilini kısar → keşif + komutan-reveal (örn. Vhirek "Mark of Ash") ve scout değer kazanır; "kör push" cezalı. | ⚙️ VisionField |

## 4.6 Biyom-Özgü Hazard'lar (≥3) — *hepsi telegraf'lı, simetrik, counter'lı; RNG yok*
| # | Hazard | Etki | Counter | Etiket |
|---|---|---|---|---|
| H1 | **Yumuşak Kum** | `MoveMult`↓ + hafif `HazardDps` bölgeleri | Etrafından dolaş; Loose ile hızlı geç; sabit-kalma | ♻️ Hazard+MoveMult |
| H2 | **Bataklık-Kum (Quicksand)** | Telegraf'lı karolar ağır birimi/Tight kütleyi **sabitler** (MoveMult≈0 tuzak) | Görsel telegraf'ı oku; Tight'tan kaçın; hafif birimle geç | ⚙️ MoveMult-trap (DynamicHazard varyantı) |
| H3 | **Güneş Maruziyeti (Heat)** | Açık-zemin sürekli Burning-bias'ı (M1'in hazard yüzü); fırtına dışında ısı yükselir | Cover'a/wadi'ye gir; hareketli kal; fırtına-penceresini bekle | ⚙️ status-bias |
| H+ | **Kum Fırtınası** (hava) | Görüş/menzil-reddi penceresi | Melee'ye geç; formasyonu sıkılaştır; fırtına geçene dek bekle | ⚙️ WeatherScheduler |

## 4.7 Taktiksel Fırsatlar (≥3)
1. **Dün-HighGround menzil-yığını:** ÜST satırda Crossbow (Iron Pact/Pierce) veya Slinger menzil+pozisyon üstünlüğü kurar — ama fırtına penceresinde bu yatırım donar (zamanlama riski).
2. **Wadi Cover-pusu:** ALT satırda harabe-Cover'dan flanker (Houndmaster / gelecek-komutan Sythe) ambush; gelen-menzil hasarını `DefenseMult`↓ ile yutar.
3. **Fırtına-zamanlı melee push:** ranged bastırılınca Iron Pact **Line** head-on baskısı zirve yapar; Ashen sürüsü açık hardpan'da fırtına-örtüsünde flank'a koşar. *Fırtına = melee'nin penceresi.*

---

# PART 5 — Biyom 2: **Frostbound Frontier** (Buzul / "Donağacı Sınırı")

> **Baskın eksen:** *Tempo-nötralizasyonu + Chilled/Shatter burst-penceresi + taşıyıcı-buz.* Diğerlerinden **farkı**: yavaşlama burada bir **sinerji-burst ekonomisine** (Freeze→Shatter) ve **formasyon-kritik kırılma riskine** bağlanır — salt yavaşlama değil. **En düşük yeni-hook maliyeti** (mevcut Chilled→Shatter sinerjisini yeniden kullanır → Part 16'da "ilk biyom" önerisinin çekirdeği).

## 5.1 Görsel Kimlik
Mavi-beyaz buz alanları, **donmuş bir nehir** cepheyi keser, aurora gökyüzü, kar yığınları, buz mağaraları. Palet: buz-mavisi + beyaz. **Clarity:** beyaz zemin üzerinde faksiyon renkleri **en yüksek kontrast** sunar (okunabilirlik için en kolay biyom); kar-parçacıkları birim-silüetini gizlemeyen *önplan-altı* katmanda.

## 5.2 Çevresel Hikâye Anlatımı
Savaşın ortasında **donmuş** bir cephe: buz içinde yarı-görünen askerler, **buzula gömülü bir heykel**. Zaman durmuş gibi — ta ki oyuncular onu yeniden başlatana dek. Donmuş nehir bir "ölü cephe hattı"dır; üzerinden geçmek geçmişi kırmaktır (taşıyıcı-zemin mekaniğinin hikâye-temeli).

## 5.3 Satır Yapısı
| Satır | Kimlik | Güç | Zayıflık |
|---|---|---|---|
| **ÜST — Kar Yığınları (snowdrift)** | Derin-kar attrition koridoru | Savunan için tempo-eşitleyici; yavaş-ama-güvenli | `MoveMult`↓↓ ağır/Tight'ı boğar; push yavaş |
| **ORTA — Donmuş Nehir** | Hızlı flank rotası **AMA** taşıyıcı-zemin riski | `MoveMult`↑ hızlı geçiş; sürpriz flank | Ağır/Tight kütle **buzu kırar → Hazard** (düşme = DoT/kayıp) |
| **ALT — Buz Mağaraları** | Cover savunma/pusu | `DefenseMult`↓ gelen-menzil; LoS-blok | Dar; kuşatma/AoE'ye açık |

## 5.4 Hava Etkileri — **Tipi (Blizzard)** ⚙️[WeatherScheduler]
Periyodik, **telegraf'lı**, **simetrik**. Pencere: harita-geneli **Chilled** stack'i + görüş kısması. Bu, bir **Shatter penceresi** açar (zaten-Chilled hedeflere Shatter ×sinerji) — ama her iki tarafa eşit. Tipi, "yavaş + donmuş + patlamaya-açık" bir savaş-alanı anı yaratır; oyuncu Shatter/Blunt burst'ünü tipiye göre zamanlar.

## 5.5 Biyom-Özgü Mekanikler (≥3)
| # | Mekanik | Etki | Etiket |
|---|---|---|---|
| M1 | **Buz Sinerjisi (Ice Synergy)** | L4: biyom Chilled uygulamasını + **Chilled→Shatter** sinerjisini *capped* amplifiye eder (Successor §8 "snow=freeze synergy"). Glacial = **Chilled biyomu**; mevcut `synergyBonusVsStatus=Chilled` deseninin doğal evi. | ♻️ mevcut sinerji + capped katsayı |
| M2 | **Donmuş Nehir (Frozen River)** | L2: ORTA satır **taşıyıcı-zemin**; hızlı flank verir ama ağır/Tight kütle eşik aşınca kırılır → formasyon-kritik karar. | ⚙️ LoadBearingTerrain |
| M3 | **Derin Kar Yavaşlaması (Deep Snow)** | L1: açık kar herkesi yavaşlatır, en çok ağır/Tight'ı → **Ashen Horde hız-kimliğini nötralize eder** (biyom-counter-identity; çeşitlilik). | ♻️ saf MoveMult substrat |

## 5.6 Biyom-Özgü Hazard'lar (≥3)
| # | Hazard | Etki | Counter | Etiket |
|---|---|---|---|---|
| H1 | **Yarık (Crevasse)** | Derin `HazardDps`/kayıp bölgesi (sabit, telegraf'lı kenar) | Kenarı oku; etrafından dolaş; itme/displacement'tan kaçın | ♻️ Hazard |
| H2 | **Donmuş-Nehir Çatlağı** | Taşıyıcı-zemin kırılması → koşullu Hazard (ağır/Tight tetikler) | Nehri Loose/hafif geç; ağır birimi köprü/kıyıdan yolla | ⚙️ LoadBearingTerrain |
| H3 | **Tipi Chilled** (hava) | Kütlesel `Chilled` (yavaşlama) + görüş-reddi | Shatter'ı *sen* zamanı; Cleanse; tipi-penceresinde temkinli ol | ⚙️ WeatherScheduler |

## 5.7 Taktiksel Fırsatlar (≥3)
1. **Chill→Shatter burst:** Freeze (Chilled) → Shatter (×sinerji) komb'su biyom-amplifiyeli; Glacial'da bu **kanonik sinerji zirve yapar** — ama counter: Cleanse / dağılma / hedefi-ısıtma yok ama timing-okuma var.
2. **Donmuş-nehir hızlı-flank:** hafif birimler (Houndmaster) buzu kırmadan ORTA'dan sürpriz flank; ağır takip ederse buz çatlar (risk-ödül).
3. **Tipi-zamanlı savunma-hold:** herkes yavaşken Iron Pact attrition + (gelecek-komutan) Vael "Kırılmaz Sur" parlar; saldıran taraf tipide tempo kaybeder.

---

# PART 6 — Biyom 3: **Rotfen Marsh** (Zehir Bataklığı / "Rotfen Bataklığı")

> **Baskın eksen:** *Choke-bölge kontrolü + Poison-sinerji + kalıcı sis-pusu (bilgi-reddi) + çamur-pin.* Diğerlerinden **farkı**: tek biyom ki görüş-azaltması **kalıcıdır** (sis) ve varsayılan zemin **dar/Choke-merkezli**dir. **En yüksek balance/clarity riski** (Part 16'da "uzman biyom").

## 6.1 Görsel Kimlik
Hastalıklı yeşil, alçak **sis**, durgun bataklık suyu, çarpık ölü ağaçlar, yükselen **spor bulutları**, batık harabeler. Palet: çürük-yeşil + kahve. **Clarity uyarısı (kritik):** düşük-kontrast yeşil-üstüne-yeşil riski taşır → faksiyon renkleri ve **zehir-yeşili hazard'lar** *ayrı bir doygunluk bandında* tutulmalı; sis birim-silüetini **asla** gizlemez (yalnızca uzak-görüş/menzil kısar). Bu biyom en sıkı **art-direction + clarity-mode** denetimini gerektirir.

## 6.2 Çevresel Hikâye Anlatımı
Veba-lanetli sulak alan — Ashen Hexcaster doktrinin memleketi; batık harabelerden **zehir sızar**; "bataklık ölüleri unutmaz." Sis ve spor, bir **bilgi-reddi** atmosferi kurar: kim nerede, belirsiz. Hikâye doğrudan mekaniktir — zehir-havuzları hazard, kamış Cover, causeway Choke.

## 6.3 Satır Yapısı
| Satır | Kimlik | Güç | Zayıflık |
|---|---|---|---|
| **ÜST — Tahta Geçit (causeway)** | Dar **Choke**; tek "kuru" yol = kill-funnel | Choke-kontrolü + AoE değer-spike'ı; savunulabilir | Çok dar → AoE/Caster'a aşırı açık; tıkanırsa durur |
| **ORTA — Açık Bataklık** | En yavaş satır (çamur) | Geniş ama riskli; flank alanı | `MoveMult`↓↓ çamur; Poison-sızıntı; ağır/Tight pin |
| **ALT — Zehir Havuzları + Kamış** | Hazard + Cover; pusu/bölge-reddi | `DefenseMult`↓ Cover; zehir-zone kontrolü | Zehir-havuzu kendine de risk; yavaş |

## 6.4 Hava Etkileri — **Spor Patlaması (Spore Bloom)** ⚙️[WeatherScheduler]
Periyodik, **telegraf'lı** (spor bulutu yükselir + ses), **simetrik**. Pencere: **Poisoned** amplifikasyonu + görüş kısması (yoğunlaşan sis). Marsh = **Poisoned biyomu**. Patlama, zehir-komp'larının penceresidir — ama görüş-reddi her iki tarafı da kör eder (pusu ↔ karşı-pusu).

## 6.5 Biyom-Özgü Mekanikler (≥3)
| # | Mekanik | Etki | Etiket |
|---|---|---|---|
| M1 | **Zehir Sinerjisi (Poison Synergy)** | L4: biyom `Poisoned` DoT'unu + yayılımı *capped* amplifiye eder (Hexcaster / Poison Cloud home-turf). Marsh = **Poisoned biyomu**. | ♻️ mevcut Poisoned + capped katsayı |
| M2 | **Çamur (Mud)** | L1: açık bataklık ağır yavaşlatır → **causeway-Choke kontrolü + Loose formasyon** ödüllü; ağır/Tight pin riski. | ♻️ saf MoveMult substrat |
| M3 | **Sis (Fog)** | L1+L3: kalıcı + spor-yoğunlaşan görüş-kısması → komutan-reveal (Vhirek) + kamış-pusu değer kazanır; influence-map okuması kısalır. **En güçlü bilgi-savaşı biyomu.** | ⚙️ VisionField |

## 6.6 Biyom-Özgü Hazard'lar (≥3)
| # | Hazard | Etki | Counter | Etiket |
|---|---|---|---|---|
| H1 | **Zehir Havuzu** | `HazardDps` + `Poisoned` uygulaması; bölge-reddi | Causeway'den geç; Cleanse; havuzu *düşmana* kullandır | ♻️ Hazard+status |
| H2 | **Çamur (Mire)** | `MoveMult`↓↓ sabitleme; ağır/Tight pin | Loose'a geç; causeway'i tut; hafif birimle manevra | ♻️ MoveMult |
| H3 | **Spor Patlaması** (hava) | Poison-amplify + sis görüş-reddi | Patlamada dağıl (AoE-Poison riski); reveal kullan; bekle | ⚙️ WeatherScheduler |

## 6.7 Taktiksel Fırsatlar (≥3)
1. **Causeway-Choke hold:** tahta-yol kill-funnel'ı; AoE/Caster (Battlemage, Arrow Storm) değer-spike'ı + Vael-duvarı; düşman sürüsü Choke'ta erir.
2. **Zehir-sinerji komp:** Hexcaster + Poison Cloud + (gelecek-komutan) Vhirek biyom-amplifiyeli zehir — **ama** zorla-dominant olmaması için counter zorunlu: Cleanse, dağılma (Loose), AoE-ile-kütle-temizleme, ranked-normalize (Part 12/13'te denetlenir).
3. **Sis-pusu / bilgi savaşı:** kamıştan flanker; reveal-vs-gizlilik oyunu; "kör push" en çok burada cezalı → scout/komutan-reveal primi.

---

# PART 7 — Biyom 4: **Ember Rift** (Volkanik / "Köz Yarığı")

> **Baskın eksen:** *Değişen savaş-alanı geometrisi (dinamik Hazard) + köprü-Choke savaşı + Fire-burst.* Diğerlerinden **farkı**: tek biyom ki **harita-geometrisi maç-ortasında değişir** (lav çatlakları büyür/kayar) — statik harita varsayımını kırar. **En yüksek spektakl + orta hook-maliyeti** (Part 16'da "ikinci biyom").

## 7.1 Görsel Kimlik
Siyah bazalt, akkor **lav çatlakları**, kül-göğü, yükselen kor parçacıkları, kızıl alt-aydınlatma, obsidyen harabeler. Palet: bazalt-siyahı + lav-kızılı. **Clarity:** kor-kızılı zemin, kızıl hasar-VFX'i ve Ashen kor-renkleriyle çakışma riski → **lav-kızılı yalnızca zeminde**, birim-hasar-telegraf'ı *farklı bir parlaklık/şekil bandında*; HighGround bazalt koyu kalır ki birim-silüeti parlasın.

## 7.2 Çevresel Hikâye Anlatımı
Savaşın **yardığı** topraklar — lav akıntıları yeryüzünü böldü, obsidyen kalıntılar eski bir medeniyeti gömdü. "Savaş dünyayı çatlattı" teması doğrudan mekaniktir: **lav çatlakları büyür** (dinamik hazard), **köprüler** tek geçittir (Choke), kül **yanar** (Burning bias).

## 7.3 Satır Yapısı
| Satır | Kimlik | Güç | Zayıflık |
|---|---|---|---|
| **ÜST — Bazalt Sırtları** | HighGround menzil/kuşatma | `AttackMult`↑; lav-üstü güvenli yüksek-zemin | Köprü-bağımlı erişim; flank rotası kısıtlı |
| **ORTA — Lav-Çatlak Alanı + Köprüler** | **Dinamik Hazard** + bazalt **Choke** köprüler | Köprü-kontrolü maçı kilitler; zoning | Çatlaklar büyür → güvenli alan daralır; köprü tıkanması ölümcül |
| **ALT — Kül Düzlükleri** | Kül-bulutu Cover + Burning-bias | `DefenseMult`↓ Cover; alt-flank rotası | Sürekli Burning-bias; açık |

## 7.4 Hava Etkileri — **Püskürme / Kül Yağışı (Eruption / Ashfall)** ⚙️[WeatherScheduler + DynamicHazard]
Periyodik, **telegraf'lı** (yer titrer + kül yükselir + ses), **simetrik**. Pencere: **lav Hazard'ları genişler** (DynamicHazard), **Burning** amplifiye, kül **görüş** kısar. Bu, savaş-alanını maç-ortasında *yeniden şekillendiren* tek hava olayıdır → köprü-değeri zirve yapar, yeniden-konumlanma zorunlu olur. Telegraf **uzun** tutulur (geometri değişimi adil olsun diye).

## 7.5 Biyom-Özgü Mekanikler (≥3)
| # | Mekanik | Etki | Etiket |
|---|---|---|---|
| M1 | **Ateş Sinerjisi (Fire Synergy)** | L4: biyom `Burning` DoT'unu + Fire hasarını *capped* amplifiye eder (Successor §8 "volcanic=burn synergy"; Battlemage / Lightning Storm home-turf; Fire+burn Structure'a da sinerji). Volcanic = **Burning biyomu**. | ♻️ mevcut Burning + capped katsayı |
| M2 | **Lav Çatlakları (Lava Cracks)** | L2+L3: **dinamik Hazard**; telegraf'la kayar/büyür → maç-ortası geometri değişimi; köprüler kritikleşir; yeniden-konumlanma kararı. | ⚙️ DynamicHazard |
| M3 | **Bazalt Köprüleri (Basalt Bridges)** | L2: lav üstündeki tek güvenli geçit = **aşırı Choke değeri**; formasyon-disiplini + AoE-spike + geçit-denial. | ♻️ saf Choke |

## 7.6 Biyom-Özgü Hazard'lar (≥3)
| # | Hazard | Etki | Counter | Etiket |
|---|---|---|---|---|
| H1 | **Lav Çatlağı** | Yüksek `HazardDps`/anında; **dinamik** (telegraf'la büyür) | Telegraf'ı oku; köprüyü tut; düşmanı çatlağa zorla | ⚙️ DynamicHazard |
| H2 | **Püskürme / Kül** (hava) | Hazard-genişleme + Burning + görüş-reddi | Uzun telegraf'ta yeniden-konumlan; köprüye çekil; dağıl | ⚙️ WeatherScheduler |
| H3 | **Köz Alanları (Ember Fields)** | Substrat Burning-bias (açık zemin) | Bazalt/HighGround'a çık; Cleanse; Fire-komp'a karşı dağıl | ♻️ status-bias |

## 7.7 Taktiksel Fırsatlar (≥3)
1. **Köprü-Choke kontrolü:** bazalt geçidi tut (Iron Pact Line + gelecek-komutan Vael "Kırılmaz Sur" / Orrin "Sur Kırıcı" kuşatma) → düşman lav'a sıkışır.
2. **Fire-sinerji burst:** Battlemage + Lightning Storm (Burning) biyom-amplifiyeli; Structure'a karşı Fire+burn = heykel-baskısı — counter: dağılma (Loose), Cover, hedef-yayma.
3. **Püskürme-zamanlı zoning:** uzun telegraf'ta düşmanı *çatlayacak zemine* manevra ettir; köprüyü kes (Korrash feda-tempo'su veya Orrin gedik-zamanlaması ile) — geometri-okuma ustalığı.

---

# PART 8 — Üç-Satır (3-Lane) Etkileşim Analizi

**Çerçeve (G1 hatırlatması):** BULWARK tek-cephedir; "lane" = **tek cephenin ÜST/ORTA/ALT mantıksal satırı** (`InfluenceMap.Rows=3`; aynı-satır hedeflemesi). Burada "satır kimliği"nin **biyomdan biyoma nasıl döndüğü** analiz edilir — asıl tasarım-değeri budur: aynı 3 satır, her biyomda **farklı bir rol** alır → oyuncu her biyomda satır-önceliğini yeniden öğrenir (terrain ustalığı + adaptasyon).

## 8.1 Master Satır × Biyom Matrisi
| Satır | Desert | Glacial | Marsh | Volcanic |
|---|---|---|---|---|
| **ÜST** | Yüksek dünler — **ranged/HighGround** | Kar yığını — **attrition/yavaş** | Tahta geçit — **Choke/funnel** | Bazalt sırt — **HighGround/kuşatma** |
| **ORTA** | Sert düzlük — **hızlı/maruz** | Donmuş nehir — **hızlı/kırılgan** | Açık bataklık — **en yavaş/pin** | Lav-çatlak — **dinamik/köprü-bağımlı** |
| **ALT** | Wadi/harabe — **Cover/pusu** | Buz mağarası — **Cover/savunma** | Zehir+kamış — **Hazard/bölge-reddi** | Kül düzlüğü — **Cover/Burning** |

> **Okuma:** ÜST satır Desert/Volcanic'te *menzilli yüksek-zemin*, Glacial'da *yavaş attrition*, Marsh'ta *dar Choke* olur — **kimlik tersine döner.** Bir oyuncunun "ÜST satır benim ranged hattım" alışkanlığı Marsh'ta ölümcüldür (orada ÜST bir AoE-tuzağıdır). Bu **kasıtlı** çeşitliliktir.

## 8.2 Per-Biyom Satır Stratejisi (kimlik · güç · zayıflık · zorlanan karar)

### Desert
- **ÜST (dün/HighGround):** Güç = menzil+pozisyon üstünlüğü. Zayıflık = Cover-yok flank + fırtınada menzil çöker. **Karar:** ranged'i ÜST'e yatır ama fırtına-takvimini izle.
- **ORTA (hardpan):** Güç = hız/manevra. Zayıflık = maruziyet/ısı + flank. **Karar:** geç ama durma (turtle cezası).
- **ALT (wadi):** Güç = Cover-pusu/flank. Zayıflık = darlık/yavaş. **Karar:** flanker'ı ALT'tan sok.

### Glacial
- **ÜST (kar):** Güç = savunan için tempo-eşitleyici. Zayıflık = push çok yavaş. **Karar:** burada saldırma, *tut*.
- **ORTA (donmuş nehir):** Güç = hızlı flank. Zayıflık = ağır/Tight buzu kırar. **Karar:** yalnızca hafif/Loose ile geç.
- **ALT (mağara):** Güç = Cover savunma. Zayıflık = AoE'ye açık. **Karar:** Shatter-burst'ünü buradan koru.

### Marsh
- **ÜST (causeway/Choke):** Güç = kill-funnel + AoE-spike. Zayıflık = tıkanırsa durur, AoE'ye aşırı açık. **Karar:** Choke'u Line ile tut ama Tight'la tıkanma.
- **ORTA (çamur):** Güç = geniş flank alanı. Zayıflık = en yavaş + pin + zehir. **Karar:** ağır birimi buradan geçirme.
- **ALT (zehir+kamış):** Güç = bölge-reddi + pusu. Zayıflık = zehir kendine de risk. **Karar:** zehir-zone'u düşmana dayat.

### Volcanic
- **ÜST (bazalt sırt):** Güç = güvenli HighGround. Zayıflık = köprü-bağımlı erişim. **Karar:** sırtı al, köprüyü besle.
- **ORTA (lav+köprü):** Güç = köprü-kontrolü maçı kilitler. Zayıflık = çatlaklar büyür, tıkanma ölümcül. **Karar:** köprüyü tut, püskürmede yeniden-konumlan.
- **ALT (kül):** Güç = Cover alt-flank. Zayıflık = Burning-bias. **Karar:** Fire-komp'a karşı dağıl.

> **Sentez:** Her biyom, **bir satırı "kazanılması gereken anahtar satır" yapar** (Desert: ÜST dün; Glacial: ORTA nehir-kontrolü; Marsh: ÜST causeway; Volcanic: ORTA köprü). Lane-önceliği biyoma göre kayar → tek bir "doğru açılış" yoktur; bu, rekabetçi derinliğin ve harita-okuma ustalığının kaynağıdır.

---

# PART 9 — Formasyon Adaptasyon Kılavuzu

**Kanon formasyonları (`FormationType`):** **Line** (frontal blok; head-on güçlü, flank/AoE'ye zayıf) · **Tight** (melee-yoğun, yüksek DPS, **AoE-savunmasız**) · **Loose** (yayık, **AoE-dirençli**, ranged-dostu). Facing izlenir → flank gerçektir. Aşağıda her biyom, **hangi formasyonu cezalandırıp hangisini ödüllendirdiği** ile çözümlenir — ve oyuncunun **neden formasyon değiştirmek zorunda olduğu** gösterilir.

## 9.1 Çapraz-Biyom Formasyon Tezi
**Tight = en yüksek DPS ama 4 biyomdan 3'ünde en yüksek-riskli formasyon:** Glacial (Shatter-AoE + buz-kırma + derin-kar), Marsh (zehir-yayılım + AoE-spike + çamur-pin), Volcanic (lav-kümeleme + Fire-AoE). Biyomlar oyuncuyu **konforlu Tight DPS-kümesinden çıkarıp** Line (tut) veya Loose (yay) kararına **zorlar**. Desert istisnadır (fırtına melee-penceresinde Line/Tight push parlar). Bu, "formation decisions" pilarının doğrudan uygulamasıdır.

## 9.2 Per-Biyom Formasyon Analizi

### Desert — *"Fırtına ritmine göre değiştir"*
| Formasyon | Değer | Neden değiştirmelisin |
|---|---|---|
| **Line** | Fırtına-penceresinde **güçlü** (ranged bastırılmış, head-on melee push) | Açık-havada ranged'e yem; fırtınada zirve |
| **Tight** | Quicksand/ısı yakınında **riskli**; fırtınada burst-push | Quicksand Tight'ı pinler; ısı kümede stack'lenir |
| **Loose** | Açık hardpan'da **varsayılan** (mobilite, ısı-kaçış, AoE-güvenli) | Flank'a yayık-açık ama maruziyet/AoE'den korunur |
**Örnek:** Açık-havada Loose ile manevra → fırtına telegrafı → Line'a sıkış, ÜST düne melee push. *Formasyon = fırtına saatinin fonksiyonu.*

### Glacial — *"Tight'ı bırak, Loose'a kay"*
| Formasyon | Değer | Neden değiştirmelisin |
|---|---|---|
| **Line** | Tipi-hold'da **güçlü** (yavaş savaşta duvar) | Donmuş nehirde kırılma riski |
| **Tight** | **En riskli** — Shatter-AoE mıknatısı + buz-kırma + kar-yavaşlama | Chill→Shatter penceresinde Tight = toplu ölüm |
| **Loose** | **Tercih** — Shatter-AoE'ye dirençli, buzu daha az zorlar | DPS-yoğunluğu düşer ama hayatta kalır |
**Örnek:** Düşman Freeze attı → **Tight'tan Loose'a geç** yoksa Shatter ×sinerji seni siler; donmuş nehri Loose/hafif geç.

### Marsh — *"Choke'ta Line, gerisinde Loose; Tight = tuzak"*
| Formasyon | Değer | Neden değiştirmelisin |
|---|---|---|
| **Line** | Causeway-Choke **tutmada güçlü** | Choke dışında flank'a açık |
| **Tight** | **Tuzak** — zehir-yayılımı + AoE-spike + çamur-pin hepsi Tight'ı vurur | Causeway'i Tight'la tıkama: AoE + Poison kümede katlanır |
| **Loose** | **Tercih** (açık bataklıkta) — zehir-zinciri + AoE'ye dirençli | Causeway'i tutamaz ama zehir-komp'u yener |
**Örnek:** Causeway'i **Line** ile tut; ama Spore Bloom + Poison Cloud gelirse kütleyi **Loose**'a yay (zehir-zinciri kırılır).

### Volcanic — *"Köprüde Line, püskürmede Loose"*
| Formasyon | Değer | Neden değiştirmelisin |
|---|---|---|
| **Line** | **Köprü-Choke tutmada güçlü** (geçit-denial) | Lav-kenarında Line uzunsa çatlak-genişlemesi yer | 
| **Tight** | **Riskli** — lav-kümeleme + Fire-AoE + push-kümesi | Köprüde Tight = tek Lightning Storm'la silinir |
| **Loose** | **Püskürmede tercih** — Fire-AoE + genişleyen-Hazard'a dirençli | Köprüyü tutamaz ama eruption'ı atlatır |
**Örnek:** Köprüyü **Line** ile kilitle; Eruption telegrafında **Loose**'a yay (genişleyen lav + Burning seni kümeden öldürmesin).

> **Sentez:** Dört biyom, üç formasyonun **üçünü de** farklı anlarda zorunlu kılar — tek bir "en iyi formasyon" yoktur. Glacial/Marsh/Volcanic, konforlu Tight-DPS'i cezalandırarak **Loose'un savunma-değerini** öğretir; Desert + tüm Choke-tutuşları **Line'ın disiplin-değerini** öğretir. Formasyon, biyom × hava × düşman-komp'unun okunması haline gelir.

---

# PART 10 — Terrain Özellik Kütüphanesi (yeniden-kullanılabilir katalog)

Tüm biyomların paylaştığı **modüler terrain-atomu kataloğu.** Her öğe: hangi §4 yuvasına/sözcüğüne bağlandığı · gameplay etkisi · **okunabilirlik etkisi** · **denge riski** · ♻️/⚙️ etiketi. Bu katalog, gelecek biyomların (Part 14) **lego-taşlarıdır**.

| Özellik | §4 yuvası / sözcük | Gameplay etkisi | Okunabilirlik etkisi | Denge riski | Etiket |
|---|---|---|---|---|---|
| **Çamur (mud)** | `MoveMult`↓ substrat | Yavaşlatır, ağır/Tight pinler | Yüksek (doku belli) | Aşırı-yavaşlama tempo'yu öldürür | ♻️ |
| **Dünler (dunes)** | `TerrainKind.HighGround` | +giden/menzil, maruz | Yüksek (yükseklik belli) | Ranged-dominansı | ♻️ |
| **Uçurum/Sırt (cliffs)** | `HighGround` | +giden/menzil, erişim-kısıtlı | Yüksek | Bölge-kilidi (zone lockout) | ♻️ |
| **Donmuş Nehir** | ⚙️ `LoadBearingTerrain` | Hızlı geçiş; ağır/Tight kırar→Hazard | Orta (çatlak-telegrafı şart) | Kırılma-eşiği ayarı hassas | ⚙️ |
| **Zehir Havuzu** | `Hazard`+`Poisoned` | DoT + bölge-reddi | Orta (Marsh'ta clarity riski) | DoT yüksekse "no-go" ölü-alan | ♻️ |
| **Lav Çatlağı** | ⚙️ `DynamicHazard` | Yüksek DoT; **kayar/büyür** | Orta (uzun telegraf şart) | Dinamik geometri = en yüksek tuning riski | ⚙️ |
| **Köprü (bridge)** | `TerrainKind.Choke` | Funnel, AoE-değer-spike, denial | Çok yüksek | Çok dar = pat (stalemate) | ♻️ |
| **Harabe (ruins)** | `Cover`+`Choke` | LoS-blok, pusu, gelen-menzil↓ | Yüksek | Aşırı-Cover ranged'i öldürür | ♻️ |
| **Orman/Kamış (forest/reeds)** | `Cover` | −menzilli hasar, LoS-blok, ambush | Yüksek (ama görüş-etkileşimi) | Pusu-snowball'u | ♻️ |
| **Bataklık-Kum (quicksand)** | ⚙️ `MoveMult`-trap | Ağır/Tight'ı sabitler | Orta (telegraf'sız = adaletsiz hissi) | Telegraf'sızsa feel-bad | ⚙️ |
| **Yarık (crevasse)** | `Hazard` (statik) | Anında/yüksek DoT, sabit kenar | Yüksek (kenar belli) | Anında-öldürme adalet riski | ♻️ |
| **Sis / Görüş-Alanı (fog)** | ⚙️ `VisionField` | Hedefleme/reveal menzilini kısar | **DÜŞÜK — en yüksek risk** (silüet gizleme yasak!) | Görüş-reddi snowball'u | ⚙️ |
| **Köz Alanı (ember field)** | ⚙️ status-bias substrat | Açık-zemin Burning-bias | Yüksek (parıltı belli) | Pasif DoT stack'i | ⚙️ |

**Okunabilirlik bütçesi kuralı:** Bir haritada toplam terrain **2–4 özellik** (kanon §11) — biyom bunu *zenginleştirir, kalabalıklaştırmaz*. ⚙️ `VisionField` (sis) **en sıkı denetlenen** öğedir: birim-silüetini **asla** gizlemez; yalnızca *uzak-hedefleme/reveal* menzilini kısar ve **simetrik + reveal-counter'lı**dır. Ranked'de "clarity mode" (§6) tüm biyom-VFX'ini standart-okunur sürüme indirir.

**Denge-risk kademeleri:** Düşük (♻️ HighGround/Cover/Choke/mud — kanıtlanmış §4 atomları) · Orta (Hazard/quicksand — telegraf + DoT-ayarı) · **Yüksek (⚙️ DynamicHazard + VisionField — yeni sistem + adalet/clarity yükü).** Part 15/16 bu kademelere göre biyom-sırası önerir.

---

# PART 11 — Biyom × Büyü Etkileşim Matrisi

Biyom **status-amplifikasyonu**, mevcut draft-3 büyü sistemiyle (12 MVP büyüsü; bkz. kardeş iz **[[002-spell-synergy-web]]**) doğrudan kesişir. Referans büyüler: *Freeze→Chilled · Shatter (×sinerji vs Chilled) · Poison Cloud→Poisoned · Lightning Storm→Burning · Rage/Haste · Gold Rush/Raise Gold · Summon Pouncer/Giant.* **Temel adalet-kuralı:** biyom-amplifikasyonu **(a) capped, (b) simetrik (her iki oyuncunun büyüsüne eşit), (c) hiçbir sinerjiyi un-counterable yapmaz** (§5.3 + ADR-2-002 katman-disiplini).

## 11.1 Kategori × Biyom Matrisi
| Büyü kategorisi (örnek) | Desert | Glacial | Marsh | Volcanic |
|---|---|---|---|---|
| **Freeze/Control (Freeze→Chilled, Shatter)** | Chilled **baskılanır** (ısı) → Shatter zayıf | **Çift-amplifiye** (biyom + sinerji) → ⚠️ exploit riski | Nötr | Chilled baskılanır → Shatter zayıf |
| **Fire (Lightning Storm→Burning)** | **Amplifiye** (ısı/Burning-bias) | Nötr/hafif-zayıf | Nötr | **Çift-amplifiye** (Burning biyomu) → ⚠️ exploit |
| **Poison (Poison Cloud→Poisoned)** | Nötr | Nötr | **Çift-amplifiye** (Poisoned biyomu) → ⚠️ exploit | Nötr |
| **Vision/Reveal** (komutan-reveal; MVP'de adanmış büyü yok) | **Premium** (serap+fırtına) | Premium (tipi) | **En yüksek premium** (kalıcı sis) | Premium (kül) |
| **Offense-AoE (Arrow Storm)** | Fırtınada **bastırılır** (menzil/görüş↓) | Tipide menzil↓ | Causeway'de **değer-spike** (funnel) | Köprüde değer-spike |
| **Economy (Gold Rush/Raise Gold)** | Maden-erişimi açık | Nötr | Çekişmeli ileri-maden zor | Köprü-arkası maden riskli |
| **Summon (Pouncer/Giant)** | Quicksand summon'u pinler | **Giant donmuş nehri kırar** | Zehir-havuzu summon'u öldürür | **Lav summon'u öldürür** (dinamik!) |

## 11.2 Güçlü Sinerjiler (tasarım-niyeti — capped & counter'lı)
- **Glacial + Freeze→Shatter:** biyomun amblem-combo'su; "snow=freeze synergy" (Successor §8) doğrudan. Tipi-penceresinde kütlesel Chilled → Shatter burst. *Niyet: tatmin edici, telegraf'lı, Cleanse/dağılma ile counter'lı.*
- **Volcanic + Lightning Storm/Fire:** Burning-biyom burst; Structure'a Fire+burn = heykel-baskısı. *Niyet: kuşatma-tempo'su, Cover/dağılma ile counter'lı.*
- **Marsh + Poison Cloud:** zehir-zone hâkimiyeti + yayılım. *Niyet: bölge-reddi, Cleanse/Loose/AoE-temizleme ile counter'lı.*
- **Tüm biyomlar + Reveal/Vision:** görüş-kısan biyomlarda reveal-büyüsü/komutanı (Vhirek) değer kazanır → "bilgi" yeni para birimi.

## 11.3 Tehlikeli Exploit'ler & Denge Riskleri (açıkça işaretli)
| Risk | Senaryo | Mitigasyon (zorunlu) |
|---|---|---|
| **Çift-amplifikasyon kaçağı** | Biyom-amplify × büyü-sinerji × komutan-buff = un-counterable burst (örn. Marsh + Poison Cloud + Vhirek) | **Biyom-amplify ayrı capped katsayı**; ADR-2-002 katman-disiplini korunur (komutan ≤budget, spell ayrı, **biyom 3. capped katman**); ranked-normalize |
| **Off-biyom büyü ölümü** | Glacial'da tüm Fire-build'ler işe yaramaz → "yanlış draft = kayıp" | Amplifikasyon **yumuşak** (×küçük capped), baskılama **kısmi** (sıfırlamaz); draft hâlâ esnek kalır |
| **Summon + dinamik Hazard feel-bad** | Giant'ı lav/donmuş-nehre summon → anında kayıp | Summon **Hazard-farkındalığı** (güvenli-spawn) veya net telegraf; "feel-bad" testi |
| **Vision-denial snowball** | Sis'te reveal'i olan taraf tek-taraflı bilgi → snowball | Görüş-reddi **simetrik**; reveal **her iki tarafa erişilebilir** (komutan/büyü); clarity-mode |
| **Economy-spell biyom-kilidi** | Choke-biyomda ileri-maden erişilemez → Gold Rush ölü | Maden-yerleşimi (`MapDef.mines[]`) her biyomda **dengeli + simetrik** yerleştirilir |

> **Çapraz-referans:** Büyü-sinerji ağının tam denetimi **[[002-spell-synergy-web]]**'dedir; bu rapor yalnızca **biyom-katmanının** o ağa nasıl 3. capped çarpan olarak eklendiğini gösterir. Hiçbir biyom, §5.3 "no un-counterable spell" ahlakını veya ADR-2-002 bütçe-ayrımını gevşetmez.

---

# PART 12 — Biyom × Komutan Etkileşim Denetimi

Komutanların biyomlara göre **nasıl değer kazandığı/kaybettiği.** Referanslar: **MVP komutanları** (Iron Warden: *Rally*+*Quartermaster*; Ashen Warchief: *WarCry*+*Bloodthirst*; her ikisi tempo/buff, ≤budget) ve **gelecek-komutan adayları** (kardeş iz **[[001-commander-talent-system]]** — Iron Pact: Vael-duvar / Venn-ekonomi / Orrin-kuşatma; Ashen: Sythe-flank / Korrash-sürü / Vhirek-veba). *Not: gelecek komutanlar Phase 7.5'tir; burada yalnızca biyom-etkileşimi merceğinden anılır.*

## 12.1 Komutan-Arketipi × Biyom Matrisi (kazanç ▲ / kayıp ▼ / nötr ●)
| Arketip | Desert | Glacial | Marsh | Volcanic |
|---|---|---|---|---|
| **Tempo/buff** (Iron Warden, Warchief) | ● | ● | ● | ● |
| **Savunma-çapası** (Vael) | ▼ (açık/maruz) | ▲ (tipi-hold) | ▲ (causeway-hold) | ▲ (köprü-hold) |
| **Konumsal-ekonomi** (Venn) | ▲ (güvenli maden) | ● | ▼ (çekişmeli maden) | ▼ (lav-kilitli maden) |
| **Kuşatma/anti-structure** (Orrin) | ● | ● | ● | ▲ (köprü+Fire→heykel) |
| **Flank-mobilite** (Sythe, Houndmaster) | ▲ (wadi-pusu) | ▼ (kar/buz yavaş) | ● (sis-pusu ama çamur) | ▼ (köprü rota kısıtı) |
| **Sürü-feda** (Korrash, Warchief) | ● | ▼ (kar sürüyü boğar) | ▼ (çamur+AoE feda'yı siler) | ▲ (eruption-zoning) |
| **Debuff/bilgi-reveal** (Vhirek) | ▲ (serap-reveal) | ▲ (Chill-synergy reveal) | ▲▲ **(zehir-yurdu — ⚠️ risk)** | ▲ (kül-reveal) |

## 12.2 Per-Biyom Favori / Zayıflayan Arketip
- **Desert:** ▲ konumsal-ekonomi (güvenli maden), flank (wadi-Cover), reveal (serap). ▼ savunma-çapası (maruz/açık). *Ders: ekonomi + bilgi + flank biyomu.*
- **Glacial:** ▲ savunma-çapası (tipi-hold), reveal (Chill-synergy okuma), Iron-Pact-attrition. ▼ **sürü-hız/flank (Ashen kimliği biyomla nötralize)**. *Ders: biyom bir fraksiyon-kimliğini counter'lar.*
- **Marsh:** ▲ savunma-çapası (causeway), reveal/debuff (sis+zehir). ▼ sürü-feda (çamur+AoE). *Ders: Choke-kontrolü + bilgi biyomu.*
- **Volcanic:** ▲ kuşatma (köprü+Fire→heykel), savunma-çapası (köprü-hold), eruption-zoning. ▼ konumsal-ekonomi (lav-maden), flank (rota kısıtı). *Ders: kuşatma + zoning biyomu.*

## 12.3 Denge Riskleri (açıkça işaretli)
| Risk | Senaryo | Mitigasyon (zorunlu) |
|---|---|---|
| **Biyom-kimlik çakışması (strictly-dominant)** | Bir komutanın kimliği = biyomun amplifiye ettiği status (örn. **Vhirek + Marsh = zehir-yurdu**; Fire-komutan + Volcanic) → fiilî güç bütçe-tavanını biyom üzerinden aşar | **Biyom-amplifikasyonu AYRI 3. capped katman** (ADR-2-002 mantığının uzantısı: komutan ≤budget, spell ayrı, **biyom ayrı capped**); **ranked-normalize**; **harita-rotasyonu** (hiçbir komutan kalıcı biyom-buff'lı değil); her komutanın **counter'ı korunur** |
| **Faksiyon-kimlik nötralizasyonu** | Glacial Ashen-hızını boğar → Ashen "Glacial'da zayıf" hisseder | Biyom-counter **kısmi/yumuşak** (sıfırlamaz); Ashen'in biyomda **başka kaldıraçları** var (donmuş-nehir hafif-flank, Vhirek Chill-synergy); async-ladder harita-rotasyonu dengeler |
| **Reveal çift-değeri** | Görüş-kısan biyomlarda reveal-komutan tek-taraflı bilgi-snowball'u | Görüş-reddi **simetrik**; reveal **her iki tarafa erişilebilir**; reveal-değeri capped; clarity-mode |
| **Ekonomi-komutan biyom-kilidi** | Choke/lav biyomlarda ileri-maden erişilemez → Venn/Quartermaster ölü | `MapDef.mines[]` her biyomda **dengeli + simetrik**; güvenli-yarı madeni her zaman erişilebilir |

> **Çekirdek kural:** Hiçbir biyom bir komutanı/fraksiyonu **zorla-kazandıran** yapmamalı. Biyom **eğilim** yaratır (kim parlar), **belirleyicilik** değil (kim kazanır). Counter + simetri + ranked-normalize + harita-rotasyonu, eğilimi adil çeşitliliğe çevirir.

---

# PART 13 — Rekabetçi Adalet Denetimi

**Tez:** Biyom mekanikleri **çeşitlilik yaratır, adaletsizlik değil** — çünkü biyom **her iki oyuncu için AYNI kuralı** değiştirir; beceri, değişen kurala **rakipten daha hızlı/iyi adapte olmaktır.** *Hangi* kuralların aktif olduğu (çeşitlilik) ≠ kuralların *bir tarafı kayırması* (adaletsizlik).

## 13.1 Simetri Kuralları
- **Harita geometrisi:** aynalı-simetri **veya** kanıtlanmış-eşdeğer-erişim. Hiçbir taraf daha iyi HighGround/Cover/Choke/heykel-konumu almaz.
- **Maden simetrisi:** `MapDef.mines[]` her iki taraf için dengeli; güvenli-yarı + çekişmeli düğüm dağılımı eşit.
- **Hava/hazard simetrisi:** kum-fırtınası/tipi/spor/püskürme **her iki tarafı eşzamanlı + eşit** etkiler; tek-tarafa hava avantajı yok.
- **Async-ladder notu:** rekabet async ghost-ladder'dır (her iki oyuncu **aynı haritayı** oynar) → simetri *harita-içi* (saldıran/savunan ekonomi dengesi) olarak da tutulur; snapshot stat-sanity ile doğrulanır.

## 13.2 Hazard Kuralları
- **Telegraf'lı:** her hazard görsel + ses + öncel-süre ile uyarır (dinamik-hazard = **uzun** telegraf).
- **Simetrik & counter'lı:** terrain/yeniden-konumlanma/Cleanse ile kaçılabilir.
- **RNG YOK:** hiçbir hazard rastgele tetiklenmez (görev yasağı + §5.3 ahlakının terrain karşılığı).
- **Sınırlı:** DoT capped; **kaçışsız anında-öldürme yok** (yarık/lav bile telegraf'lı kenar + kaçış-penceresi taşır).

## 13.3 Görünürlük (Visibility) Kuralları
- **Simetrik körlük:** görüş-azaltması her iki tarafı eşit kör eder.
- **Reveal her iki tarafa açık:** komutan/büyü reveal'ı tek-fraksiyon tekeli değil.
- **Silüet korunur:** `VisionField` yalnızca *uzak-hedefleme/reveal* menzilini kısar; **birim-silüetini ASLA gizlemez** (okunabilirlik İHLAL EDİLEMEZ).
- **Clarity-mode (ranked):** §6 clarity-mode tüm biyom-VFX'ini ve sis'i standart-okunur sürüme indirir.

## 13.4 Okunabilirlik (Readability) Kuralları
- **Silüet-ayırt-edilebilirliği** her biyomda korunur (per-unit sign-off, §10).
- **Renk-körü-güvenli** faksiyon/hasar paleti; faksiyon renkleri biyom-paletine karşı **yüksek kontrast** (Marsh yeşil-üstüne-yeşil = en sıkı denetim).
- **Her etkinin ayrı net telegraf'ı** (kar-dokusu, fırtına-uyarısı, çatlak-çizgisi, sinerji-parıltısı ayrı okunur).
- **Biyom-VFX birim-okumasını gasp edemez**; mobil parçacık bütçesi sabit kısıt.

## 13.5 Kombinatoryal Dengenin Yönetimi (§16 risk #5'e yanıt)
Biyom × formasyon × counter × büyü × komutan = tuning patlaması. Yapısal yanıt: (1) **tek §4 matematiği** (biyom yeni hasar yolu açmaz → tüm biyomlar aynı sistemle ayarlanır); (2) **telemetri + RemoteConfig live-tuning** (app-update'siz retune); (3) **ranked-normalizasyon** (komutan/upgrade/biyom-amplify capped); (4) **harita-rotasyonu** (hiçbir biyom kalıcı meta-kilidi değil); (5) **simetri** (biyom her iki tarafa eşit). Bu beş kaldıraç, çeşitliliği adalet-içinde tutar.

---

# PART 14 — Gelecek Genişleme Yol Haritası (8–12 biyom)

İlk 4 biyom 4 yeniden-kullanılabilir ⚙️ hook'u (WeatherScheduler, DynamicHazard, LoadBearingTerrain, VisionField) "öder". Sonraki adaylar **yalnızca data + art** (yeni-status gerektirenler ek ADR-kapılı). Her aday: kısa konsept + yeniden-kullanılan hook + yeni-içerik/ADR.

| # | Aday biyom | Konsept özeti | Yeniden-kullanılan hook / sözcük | Yeni-status / ADR |
|---|---|---|---|---|
| 5 | **Jungle (Orman)** | Yoğun kanopi + ambush; Successor §8'in kanon-adlı "forest=cover/ambush" yönü (G2'de Biyom-3'ten buraya taşındı) | VisionField (kanopi), Cover-yoğun, MoveMult (sarmaşık) | Yok |
| 6 | **Highlands (Yaylalar)** | HighGround-ağırlıklı sırt/rampa; rüzgâr menzili saptırır | HighGround-yoğun, WeatherScheduler (rüzgâr) | Yok |
| 7 | **Crystal Wastes (Kristal Çorak)** | Kristal oluşumlar LoS + Magic-amplify; olası yansıma | L4-sinerji (Magic), Cover (kristal) | ⚠️ reflect = yeni mekanik → ADR |
| 8 | **Storm Coast (Fırtına Kıyısı)** | Şimşek-hava + gelgit dinamik-hazard | WeatherScheduler (şimşek), DynamicHazard (gelgit) | ⚠️ **Voltaic = YENİ StatusKind → ADR** |
| 9 | **Necrotic Frontier (Çürüme Sınırı)** | Çürüme-DoT + Summon-amplify; ceset-ekonomisi teması | HazardDps (çürüme), L4 (Summon-amplify) | ⚠️ ceset-mekaniği varsa → ADR |
| 10 | **Salt Flats (Tuz Düzlükleri)** | Aşırı-açık, Cover-yok, ayna-parlaması; uç ranged/flank biyomu | VisionField (parlama), HighGround-yok | Yok |
| 11 | **Fungal Deep (Mantar Derinliği)** | Yeraltı/karanlık + spor-platform; düşük-ışık görüş oyunu | VisionField (karanlık), Cover (mantar) | Yok (Marsh-Poison sözcüğünü paylaşır) |
| 12 | **Tundra-Steppe (Tundra-Bozkır)** | Rüzgâr + hafif-kar hibrit; Glacial-lite + Highlands-lite | WeatherScheduler + hafif Chilled-bias | Yok |

> **Mimari ispatı:** 8–12 biyomun **hiçbiri yeni bir çekirdek-sistem gerektirmez** — yalnızca data-profili + modüler art-kiti (+ yeni-status gerekenler için ayrı ADR). Bu, "4 → 8–12" ölçeklenmesinin **content-velocity** (paylaşımlı-iskelet/reskin) disiplinine oturduğunu kanıtlar (§10, Blueprint §8). Sezon-cadence: bir biyom genellikle **birden çok sezonun harita-slotuna** yayılır (§13 6.2); bu, **[[004-liveops-and-battlepass]]** ile koordine edilmeli.

---

# PART 15 — Implementasyon Hazırlığı

Paylaşımlı altyapı (bir kez): **BiomeProfile data-şeması + 4 ⚙️ hook** (WeatherScheduler, DynamicHazard, LoadBearingTerrain, VisionField) + biyom-amplifikasyon capped-katmanı. Sonrası biyom-başına **data + art + VFX + audio + tuning.** Art/asset maliyeti için bkz. **[[000-assets-roadmap]]** (paylaşımlı-iskelet/modüler-kit disiplini).

## 15.1 Biyom-Başına Maliyet Tahmini (göreli; PROVISIONAL)
| Eksen | Glacial | Volcanic | Desert | Marsh |
|---|---|---|---|---|
| **Yeni gameplay-sistemi** | **En az** (Chilled→Shatter zaten var) | Orta (DynamicHazard) | Orta (VisionField+heat-bias) | Orta-yüksek (VisionField-ağır+Poison-amplify) |
| **Yeni ⚙️ hook** | LoadBearingTerrain (+paylaşılan) | **DynamicHazard** (marquee) | VisionField + status-bias | VisionField-ağır + mud + Poison-amplify |
| **VFX** | Orta (kar/tipi) | **Yüksek** (lav/kül/püskürme — dinamik) | Orta (fırtına/serap-shimmer) | Orta (sis/spor — **clarity-kritik**) |
| **Çevre-art** | Buz/kar modüler kit | Bazalt/lav kit | Dün/harabe kit | Bataklık kit (**clarity-en-zor**) |
| **Audio** | Rüzgâr/tipi | Gümbürtü/püskürme | Rüzgâr/fırtına | Ortam/spor |
| **Teknik karmaşıklık** | **Düşük-Orta** | **Orta-Yüksek** (dinamik geometri) | Orta | Orta |
| **Denge maliyeti** | **Düşük** (yumuşak counter, mevcut sinerji) | Orta | Orta (ranged↔melee ritmi) | **Yüksek** (zehir+sis+komutan-dominans) |
| **Clarity/okunabilirlik yükü** | **En düşük** (beyaz-zemin kontrast) | Orta (kor-kızıl çakışma) | Orta (shimmer) | **En yüksek** (yeşil-üstüne-yeşil) |

## 15.2 Genel Teknik Notlar
- **Perf:** hava-parçacıkları + dinamik-hazard mesh güncellemeleri **mobil parçacık/frame bütçesinde** kalmalı (her faz hard-gate); LOD + instancing zorunlu.
- **Data-driven:** tüm biyom değerleri `BiomeProfile` SO → config (RemoteConfig override); §15.6 "sayı icat etme" → hepsi LSD-owned/provisional.
- **Determinizm:** WeatherScheduler/DynamicHazard **sim-içi deterministik tetikleyici** kullanmalı (sabit takvim, RNG değil) — hem adalet hem (Phase 7.1 gelirse) replay-uyumu için.
- **Server-otorite:** biyom maç-sonucu değiştirmez (kozmetik+terrain); ekonomi/ödül server-auth kalır.

---

# PART 16 — Nihai Tavsiye (Sıralama)

## 16.1 4 Biyomun Sıralaması ve Sınıflandırması
| Sıra | Biyom | Sınıf | Gerekçe-özeti |
|---|---|---|---|
| **1** | **Frostbound Frontier (Glacial)** | **İlk post-launch biyom** | Mevcut **Chilled→Shatter** sinerjisini yeniden kullanır (en az yeni-sistem); tek gerçek-yeni-hook (LoadBearingTerrain); **en yüksek okunabilirlik** (beyaz-zemin); Ashen-hızını temiz counter'lar (anında çeşitlilik); kanon-kutsanmış ("snow=freeze synergy"); **en düşük denge-riski** |
| **2** | **Ember Rift (Volcanic)** | **İkinci post-launch biyom** | Glacial ile ateş/buz ikilisi; mevcut **Burning** sinerjisini kullanır; **DynamicHazard** = gelecek biyomların en değerli yeniden-kullanılabilir hook'u (Storm Coast gelgiti vb.); yüksek spektakl ("biomes with teeth"); orta denge |
| **3** | **The Scorched Expanse (Desert)** | **İleri (advanced) biyom** | VisionField'i tanıtır (sonra her biyom yeniden kullanır); ranged↔melee fırtına-ritmi **yeni ve dikkatli tuning** ister (dün-HighGround ranged-dominans riski); orta clarity |
| **4** | **Rotfen Marsh (Poison Swamp)** | **Uzman (expert) biyom** | **En yüksek denge + clarity riski** (kalıcı sis + Poison-amplify + komutan-dominans [Vhirek] + yeşil-üstüne-yeşil); ancak VisionField (Desert'tan) + amplify-capping (Glacial/Volcanic'ten) + DynamicHazard (Volcanic'ten) kanıtlandıktan **sonra** güvenli — her adalet/clarity kuralını stres-test eder |

## 16.2 Önerilen Geliştirme Sırası & Neden
**Glacial → Volcanic → Desert → Marsh.** Bu sıra **her yeniden-kullanılabilir hook'u en düşük-riskli biyomda kanıtlayıp** en yüksek-riskli biyomu (Marsh) en sona bırakır:
- **Glacial** sinerji-amplifikasyon-capping'i + WeatherScheduler + LoadBearingTerrain'i en güvenli zeminde kanıtlar.
- **Volcanic** DynamicHazard'ı (en zor hook) ekler; Burning-amplify ile capping'i ikinci kez doğrular.
- **Desert** VisionField'i ekler; fırtına-ritmi ile hava-zamanlayıcıyı zenginleştirir.
- **Marsh** her üç kanıtlanmış hook'u (VisionField-ağır + amplify-capping + dinamik) + en sıkı clarity'yi birleştirir → yalnızca diğer üçü stabil olduğunda.

## 16.3 İlk Tavsiye: **Frostbound Frontier (Glacial) önce geliştirilmeli**
**Neden:** (1) **En düşük marjinal maliyet** — kanonik Chilled→Shatter sinerjisi *zaten gönderilmiş* (`synergyBonusVsStatus=Chilled` örneği); biyom yalnızca onu *capped amplifiye* eder + 1 yeni hook. (2) **En güvenli okunabilirlik** — beyaz-zemin faksiyon-kontrastı clarity-mode'u en kolay geçirir. (3) **En öğretici çeşitlilik** — Ashen hız-kimliğini biyomla nötralize ederek oyuncuya "biyom fraksiyon-değerini değiştirir" dersini *ilk* ve *en net* verir. (4) **En düşük denge-patlaması riski** — yeni status yok, yumuşak counter, simetrik yavaşlama. (5) **Kanon-hizalı** — Successor §8'in birebir önerdiği yön. GATE 2 (terrain "fun/readable") PASS olduğunda, Glacial biyom-konseptini **en yüksek-güven/en-düşük-risk** ile kanıtlar.

## 16.4 Bu Tasarımdan Önce DOĞRU Olması Gerekenler (kapı-hatırlatması)
1. **GATE 2 PASS** — terrain/formasyon katmanı playtest'te "fun & readable" kanıtlanmalı (revisit-trigger). *(Şu an DEFERRED.)*
2. **Phase 6 canlı** + S2+ takvimi + biyom-içerik-akışı için ADR.
3. **4 ⚙️ hook için ADR(ler)** (WeatherScheduler, DynamicHazard, LoadBearingTerrain, VisionField) + **biyom-amplifikasyon capped-katman** ADR'si (ADR-2-002 mantığının uzantısı).
4. Yeni-status gerektiren gelecek biyomlar (Storm Coast=Voltaic) için **ayrı ADR**.

---

## Kapanış — Yetki & Statü Beyanı

Bu belge **gelecek araştırma izi 003**'tür: Roadmap §13 **Phase 7.4 (biomes)** için bir **ön-tasarım çerçevesi.** Phase 7.4 **DEFERRED**'dir; revisit-trigger (*"terrain layer validated as fun/readable"*) **henüz ateşlenmemiştir** (GATE 2 DEFERRED) ve biyomlar **§15 CUT listesinde tetikleyici-öncesi yasaktır.** Dolayısıyla bu rapor:

- **Yapmadığı:** roadmap/canon/decision-log değişikliği · özellik/implementasyon yetkilendirmesi · güç-bütçesi/monetizasyon/öncelik değişikliği · yeni kanon mekanik. Hiçbir `report/`, `docs/adr/`, `docs/execution/`, decision-log dosyası **değiştirilmedi**.
- **Yaptığı:** keşif · analiz · tavsiye · değerlendirme. Tüm sayısal değerler **PROVISIONAL/LSD-owned**; her ⚙️ sim-hook Phase 7.4'te **ADR** gerektirir; her tasarım **§4 zincirine bağlanır**, tek-cephe/3-satır/okunabilirlik/fairness/no-P2W kısıtlarını **korur.**

**Kardeş izler:** [[000-assets-roadmap]] (biyom art/asset maliyeti) · [[001-commander-talent-system]] (biyom × komutan, Part 12) · [[002-spell-synergy-web]] (biyom × büyü, Part 11) · [[004-liveops-and-battlepass]] (biyom sezon-cadence'i, Part 14).

*Yalnızca dokümantasyon + tasarım-keşfi — implementasyon yok, kanon değişikliği yok, roadmap değişikliği yok. Aktif geliştirme akışı değişmeden kalır: CI/CD doğrulama · APK üretimi · Unity doğrulama · Phase 0–3 kapı borç-eritimi.*
