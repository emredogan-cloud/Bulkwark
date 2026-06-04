# BULWARK — Komutan Koleksiyonu & Yetenek Ağacı Tasarım Raporu (Gelecek Araştırma)

> **⚠️ STATÜ: GELECEK ARAŞTIRMA İZİ — YALNIZCA TAVSİYE NİTELİĞİNDE.**
> Bu belge **aktif geliştirme akışının parçası DEĞİLDİR**. Aktif akış şudur: CI/CD doğrulama, APK üretimi, Unity doğrulama, kalan Phase 0–3 kapı borç-eritimi.
> **Bu belge HİÇBİR ŞEYİ:** roadmap'i değiştiremez · kanonu değiştiremez · karar günlüğünü (decision log) değiştiremez · gelecek özellik yetkilendiremez · üretim önceliklerini değiştiremez · implementasyon başlatamaz.
> **Bu belge YALNIZCA:** keşfeder · analiz eder · tavsiye eder · değerlendirir.
> **Konum:** `future/001-commander-talent-system/` (canon dökümanlarına dokunulmadı; hiçbir `report/`, `docs/adr/`, `decision log` dosyası değiştirilmedi).
> **Kanonik yuva:** Bu araştırma, **Roadmap §13 Phase 7.5 — "commander collection + talent economy (capped power; cosmetic monetization only)"** özelliğini önceden tasarlar. Phase 7.5 **ertelenmiştir (DEFER)**; revisit-trigger (Decision Log §2): *"MVP retention validated; commander system bones stable."* Bu tetikleyici **henüz ateşlenmemiştir** → hiçbir şey implemente edilmez.
> **Tarih:** 2026-06-03 · **Dil:** Türkçe · **Kalite çıtası:** Lead RTS Designer + Lead Systems Designer + LiveOps Designer + Progression Designer.

---

# Proje Derin Analizi

*(ZORUNLU ön-araştırma fazı — atlanmadı. Aşağıdaki rekonstrüksiyon, beş kanonik dökümanın + 5 ADR'nin + 8 execution-prompt'unun + Phase 0–3 implementasyon kodunun + 4 faz-tamamlanma raporunun okunmasına dayanır.)*

## 1.1 Mevcut proje durumu (rekonstrüksiyon)

BULWARK, *Stick War* halefi; mobil-öncelikli (Android/iOS), tek-cepheli taktiksel **RTS-lite** (Unity 6 LTS · IL2CPP · URP · **ECS/DOTS battle-sim**). Çekirdek döngü: **maden → eğit → ittir → heykeli yık**. Felsefe: kanıtlanmış çekirdeği koru, iki şeyi modernize et (sığ savaş alanı → terrain/formasyon/counter/spell-draft; kırılgan client-trust ekonomi → server-authoritative), etik kozmetik monetizasyon.

| Eksen | Durum |
|---|---|
| **Aktif geliştirme akışı** | CI/CD doğrulama · APK üretimi · Unity doğrulama · Phase 0–3 kapı borç-eritimi |
| **İçerik kanonu (MVP)** | 2 fraksiyon (Iron Pact, Ashen Horde) · 12 birim (6+6) · **2 komutan (1/fraksiyon)** · ~12 büyü · 3 harita · 20-seviye Kampanya · Endless · Async Ghost Ladder · 4 para birimi |
| **Bu araştırmanın hedefi** | Phase 7.5 (post-launch, ertelenmiş) **Komutan Koleksiyonu + Yetenek Ekonomisi** — yalnızca ön-tasarım |

## 1.2 Faz durumu & doğrulama borcu (current state reconstruction)

**Kullanıcı beyanı:** Phase 0 COMPLETE · Phase 1 COMPLETE · Phase 2 COMPLETE · Phase 3 COMPLETE · Phase 4 NOT STARTED.

**Implementasyon gerçeği (ADR'ler + faz raporlarından — dürüst rekonstrüksiyon):** Tüm Phase 0–3 **deliverable'ları AUTHORED & commit edildi**, ancak **çalışma-zamanı doğrulama kapıları DEFERRED** (PASS değil) — çünkü bu ortamda Unity 6 editörü / Android build / cihaz / BaaS yok (ADR-0-001 Blocker A). Yani:

| Kapı | Durum | Kaynak |
|---|---|---|
| Phase 0 çıkış | **CONDITIONALLY ACCEPTED** (deferred validations outstanding) | ADR-0-002 |
| GATE 1 (FUN) | **OPEN / DEFERRED** (on-device fun-verdict çalıştırılmadı) | ADR-1-001, ADR-2-001 |
| GATE 2 (playtest) | **DEFERRED** (≥%40 session-2 + "okunur & eğlenceli" rubriği çalıştırılmadı) | ADR-2-001 |
| GATE 3 (MVP feature-complete) | **DEFERRED**; **Phase 4'e yetki = WITHHELD** | ADR-2-001, phase-3 raporu |
| FormationMember kablolaması | **ERTELENDİ** (formasyonlar authored, üyelik ataması yok) | FormationMember_wiring_plan.md |

**Aktif blokerler / doğrulama borcu:** Tek bir konsolide Unity/cihaz doğrulama geçişi borçlu (CI-green; 200-birim frame-ms; in-editor resolver; BaaS round-trip; GATE 1 fun-verdict). Bu borç eritilene kadar tüm çalışma-zamanı kapıları DEFERRED. **Hiçbir ihlal-edilemez kısıt gevşetilmedi** — yalnızca *doğrulama zamanlaması* ertelendi (ADR-0-002 §4).

> **Bu araştırma için anlam:** Komutan koleksiyonu Phase 7.5'tir; **çok daha sonra**. Ondan önce GATE 1/2/3 + soft-launch LTV kapıları geçmeli, MVP retention doğrulanmalı, komutan-bones telemetri ile stabilize edilmeli. Bu rapor o günü **önceden** hazırlar — bugünü değiştirmez.

## 1.3 Komutan-ilgili kanon (canon audit — komutan)

**Roadmap §5.5 + §6 + Blueprint §6 + implementasyon (CommanderDef.cs / CommanderAbilitySystem.cs / ADR-2-002):**

| Kanon kuralı | Kaynak | Bağlayıcılık |
|---|---|---|
| Komutan = **kimlik, güç değil**; force-multiplier, **süper-birim değil** | §6 | Tasarım kısıtı |
| **EXACTLY 1 active + 1 passive** + kozmetik kimlik | §6, CommanderDef.cs | Yapısal |
| **Güç bütçesi ≤ %10–15** (telemetri-zorunlu); kodda `k_PowerBudgetCeiling = 0.15` HARD-CLAMP | §6, CommanderAbilitySystem.cs | **İHLAL EDİLEMEZ** |
| Yetenekler **tempo/utility**, **asla raw stat inflation değil** | §6 | Tasarım kısıtı |
| Seviyeler → ability rank + **küçük talent tree** (utility seçimleri, **sidegrade — power creep değil**) | §6 | Yapısal |
| **Earnable** (play / battle pass); premium = **yalnızca skin/VFX/voice** | §6 | Monetizasyon kısıtı |
| **Ranked normalizes** talents to a capped set; her komutanın **counter'ı var**; **strictly-dominant pick yok** | §6 | Fairness |
| Komutan-kaynaklı buff'lar **birleştirilir ve ≤ bütçeye clamp'lenir** (active+passive bir birimde bütçeyi aşamaz); spell buff'ları **ayrı, §5.3 katmanı** | **ADR-2-002** | **İHLAL EDİLEMEZ** |
| **Side başına TEK `CommanderRuntime`** (roster yok, swap yok) | CommanderAbilitySystem.cs | Yapısal güvenlik |
| Komutan **KOLEKSİYONU = Phase 7.5 (DEFER)**; MVP yalnızca 2 bones | §13 Phase 7, Decision Log §2 | Faz kapısı |
| Mevcut mekanik kelime dağarcığı: **StatusKind** {Chilled, Burning, Poisoned, Stunned, Hasted, Raged, GoldBoost}; **Terrain** {HighGround, Choke, Cover, Hazard}; **Formation** {Line, Tight, Loose}; influence-map targeting; mining; statue-phase | implementasyon | Yeniden-kullanım tabanı |
| Komutan seviyesi **CAP = 10** (`UpgradesConfig.commanderMaxLevel = 10`); maliyet Silver, rising | UpgradesConfig.cs | Progression tabanı |

**Mevcut 2 komutan (bu araştırma bunları ÇOĞALTMAZ — yeni 3+3 tasarlar):**
- **Iron Warden** (Iron Pact): Active *Rally* (Raged+Hasted, attack-weighted, cd45 mag0.12 r5 dur6) · Passive *Quartermaster* (GoldBoost on miners, mag0.08) · budget 0.12.
- **Ashen Warchief** (Ashen Horde): Active *WarCry* (Hasted+Raged, speed-weighted, cd40 mag0.15 r5 dur5) · Passive *Bloodthirst* (Raged on combat units, mag0.08) · budget 0.13.

## 1.4 Çelişki kontrolü (contradiction check — ön-tarama)

Bu raporun tüm tavsiyeleri aşağıdaki kurallara **uyacak şekilde** tasarlandı (tam denetim Part 9 + Part 10 + Part 11'de):

| Kontrol | Bu raporun duruşu |
|---|---|
| Roadmap ihlali? | **Hayır** — Phase 7.5'i ön-tasarlar; faz kapılarını/öncelikleri değiştirmez. |
| ADR ihlali? | **Hayır** — ADR-2-002 stacking kuralına ve ≤0.15 clamp'e tüm tasarımlar uyar. |
| Decision Log ihlali? | **Hayır** — koleksiyonu Phase 7.5/S1-S2 trigger'ına bağlı tutar; öne çekmez. |
| Güç bütçesi ihlali? | **Hayır** — her komutan ≤%15 maç-etkisi (Part 9 ispatı). |
| Monetizasyon kuralı ihlali? | **Hayır** — earnable + cosmetic-only; **no P2W, no power-currency, no gacha-for-power**. |
| Yetkisiz mekanik? | **İşaretlendi** — bazı yetenekler **yeni sim-hook** önerir; bunlar **gelecek ADR gerektirir** ve her biri açıkça "YENİ MEKANİK — implementasyon değil, öneri" olarak etiketlendi. Hiçbiri bugün kanona girmez. |
| Gelecek canon çatışması? | **Önlendi** — yeni StatusKind/mekanik önerileri mevcut kelime dağarcığıyla uyumlu; çatışanlar Part 10'da risk + mitigasyon ile işaretli. |

> **Önemli dürüstlük notu:** Tasarladığım komutanların bazı yetenekleri, mevcut `CommanderActiveKind`/`CommanderPassiveKind` enum'larının ve sim-hook'larının **ötesine** geçer (ör. hedefleme-yönlendirme, konum-bazlı maden verimi, statü-görüsü). Bunlar **bugünün kanonu değildir**; Phase 7.5'te bir ADR ile değerlendirilmesi gereken **gelecek tasarım önerileridir**. Her biri ⚙️ **[YENİ SİM-HOOK]** etiketiyle işaretlendi. Mevcut StatusKind buff'larıyla karşılanabilenler ♻️ **[MEVCUT MEKANİK]** etiketlidir. Bu ayrım, "no implementation / no canon change" kuralına sadakatin garantisidir.

---
# PART 2 — Komutan Tasarım Felsefesi

## 2.1 RTS komutan/kahraman sistemleri — ne işe yarar, neden çöker

| Sistem deseni | Örnek tür | **Korunacak (preserve)** | **Kaçınılacak (avoid)** |
|---|---|---|---|
| **RTS komutan/lider** (Company of Heroes komutan-yetenekleri, Dawn of War kahramanları) | Strateji | Yeteneğin **zamanlama kararı** olması; cephe-okuması ödülü; doktrin ifadesi | Kahramanın orduyu gölgede bırakması ("super-unit"); 1-tuş-kazan |
| **MOBA kahraman** (kimlik + kit) | MOBA | Güçlü **kimlik silüeti**; net counter-ilişkileri; takım-rolü | Tek-birim güç yoğunlaşması; snowball; karmaşıklık-tavanı |
| **Autobattler/SLG komutan** (Rise of Kingdoms vb.) | Mobil SLG | Toplama/ilerleme dopamini; sezonsal taze meta | **P2W** (parayla güç), gacha, stat-şişirme, sonsuz power-creep |
| **Survivor-like meta-yükseltme** | Survivor | Run-içi build-craft; kısa-oturum tatmini | Pasif sayı-yığını; oynanış-dışı güç |

**BULWARK'ın doğru pozisyonu (kanon §6 + Successor §10):** Komutan = **doktrin-ifade eden zamanlama/utility kaldıracı**, MOBA-kahramanı gibi okunur kimlik, **ama asla güç yoğunlaşması değil.** Successor Report §10: BULWARK kasıtlı olarak SLG/whale/P2W modelinin **karşısında** konumlanır — "skill-expressive, fair, fast, tactically deep."

## 2.2 Toplama (collection) sistemleri — sağlıklı vs toksik

- **Sağlıklı toplama (BULWARK hedefi):** komutanlar **earnable** (play/battle pass); fark **playstyle** (oynama-tarzı) açar, **güç** değil; ranked **normalize** eder; premium yalnızca **kozmetik** (skin/VFX/voice). Koleksiyon = *prestij + ifade + taktiksel çeşitlilik*, *avantaj değil*.
- **Toksik toplama (CUT — Decision Log §1):** parayla-güç, gacha-for-power, exclusive-power, sonsuz stat-merdiveni. Bunlar BULWARK kanonunda **kalıcı olarak yasak** (ilkeli CUT).

## 2.3 Yetenek ağacı (talent tree) felsefesi

Kanon §6: *"a small talent tree (utility choices, **sidegrades not power creep**)."* Bu, RPG yetenek-ağaçlarından **kritik bir sapmadır**:

| Geleneksel RPG ağacı (KAÇIN) | BULWARK sidegrade ağacı (KORU) |
|---|---|
| Her node +%X hasar/HP (dikey güç) | Her node **playstyle değiştirir** (yatay seçim) |
| Tüm node'lar açılabilir (kümülatif güç) | **Karşılıklı-dışlayan** kademeler (tier başına 1 seç) |
| Power-creep kaçınılmaz | Güç **sabit kalır**; *nasıl* oynadığın değişir |
| Ranked'de denge patlar | Ranked **normalize** edilebilir (capped set) |

**İlke:** Bir yetenek ağacı node'u, oyuncuya "**daha güçlü**" değil "**farklı**" hissettirmeli. *Sidegrade* = bir avantajı başka bir avantajla takas et (ör. menzil ↔ süre; ekonomi ↔ savaş-uptime). Mobil için **küçük + okunur + dengelenebilir** (Part 7).

## 2.4 Neden komutan gücü **%15'in altında** kalmalı — 4 gerekçe

Bu, kanonun en sert komutan-kuralıdır (`k_PowerBudgetCeiling = 0.15`, ihlal-edilemez). Gerekçeler:

1. **Pillar P3 — Fair Mastery (skill > spend).** Komutan earnable + premium-kozmetik olduğu için, komutan gücü maçı belirlerse, *koleksiyon = güç = P2W* olur. ≤%15 tavanı, **beceriyi belirleyici tutar**; komutan onu *eğer* değil. Bu, oyunun ticari kimliğidir (anti-SLG, Successor §10).
2. **Pillar P2 — Readable Depth.** Komutan etkisi büyürse, cephe-okuması "komutan ne yaptı?" belirsizliğine boğulur. ≤%15, savaşın **birim/terrain/formasyon/counter** okumasını birincil tutar; komutan onu *renklendirir*, *gasp etmez*.
3. **Denge çözülebilirliği (§16 risk #5).** N komutan × yetenek-ağacı = kombinatoryal denge patlaması. Düşük güç-tavanı, **hatalı dengeyi affeder** — yanlış-ayarlı bir komutan ≤%15 ile maçı bozamaz. Yüksek tavanda her yeni komutan bir balans-bombasıdır.
4. **Power-creep'e karşı sigorta (Phase 7.5 riski).** Execution Prompt §7 açıkça uyarır: *"Power-creep via commander collection (7.5 — enforce caps)."* Sabit ≤%15 tavan, koleksiyon büyüdükçe **her yeni komutanın eskisini geçememesini** garanti eder — "yeni = daha güçlü" treadmill'ini öldürür.

> **Tasarım sonucu:** Komutan gücü **stat'ta değil, taktikte** aranmalı. ≤%15 stat-fraksiyonu çok düşük olduğundan, gerçek değer **konumlandırma, zamanlama, ekonomi-verimi, bilgi-avantajı, savaş-alanı-kontrolü**nden gelmeli — bunlar bütçeyi *farklı bir para birimiyle* harcar (Part 9'da modellenir). Bu yüzden bu raporun 6 komutanı **stat-şişirici değil, taktiksel kaldıraçtır.**

## 2.5 Bu raporun tasarım pusulası (quality bar)

**Tercih edilen güç kaynakları:** formasyon-farkındalığı · konumlandırma (high-ground/flank/choke) · zamanlama (active penceresi) · ekonomi-verimi (maden/eğitim tempo) · savaş-alanı kontrolü (bölge/geçit) · **bilgi avantajı** (reveal/telegraph/intent).
**Reddedilen güç kaynakları:** jenerik +hasar/+HP · raw stat inflation · power-creep · oynanış-dışı pasif sayı-yığını.
**Her komutan:** net counter'lı · strictly-dominant değil · ≤%15 · earnable · ranked-normalize-edilebilir · **bir doktrin-facet'i ifade eder** (faraziyeyi bölme: aynı fraksiyonun farklı yüzleri).

---
# PART 3 — IRON PACT (3 yeni komutan)

*Iron Pact doktrini (§5.1): disiplinli lejyon — formasyon, kalkan, yıpratma, dayanıklılık, frontal-denial. Zayıflık: flank, Magic, mobilite, burst-tempo. Üç yeni komutan, bu doktrinin **üç farklı yüzünü** ifade eder: savunma-çapası, lojistik-tempo, kuşatma-tempo. Hiçbiri Iron Warden'ın (Rally/Quartermaster) kopyası değil.*

## 3.1 — Castellan Hadrec Vael — "Kırılmaz Sur"

| Alan | Tanım |
|---|---|
| **Ad** | Castellan Hadrec Vael |
| **Unvan** | *Kırılmaz Sur* (The Unbroken Wall) |
| **Lore özeti** | Pact'in en uzun kuşatmasını 200 gün boyunca tek bir geçidi tutarak kazanan kale-komutanı. "Bir adım geri yok" doktrininin yaşayan timsali. Saldırmaz; **düşmanı kendi duvarına kırdırır.** |
| **Görsel kimlik** | Ağır tören-plaka, kule-kalkan sırtında; miğferi kapalı, yüzsüz-otorite. Çelik+kobalt, **sönük** (parıltısız) — bir anıt gibi durur. Silüet: en geniş, en hareketsiz komutan; "yerinden oynamaz" okuması. |
| **Savaş-alanı rolü** | **Savunma çapası / geçit-denial.** Tuttuğu terrain'i (özellikle Choke/HighGround) kaleye çevirir. |
| **Taktiksel uzmanlık** | **Formasyon + terrain sinerjisi.** Line formasyonundaki ve tutulan-terrain'deki birimleri *koşullu* dayanıklı kılar — saldırmaz, *reddeder*. |
| **Güçlü yönler** | Choke/HighGround savunması; attrition; düşman push'unu kırma; geç-oyun sur |
| **Zayıf yönler** | **Pasiflik cezası** — saldırı tempo'su yok; terrain'den sökülürse değeri düşer; **flank'a açık** (buff frontal); ekonomi-baskısına yavaş yanıt; mobil değil |
| **Maç-etki tahmini** | **~%13** (savunma-konumlu; çoğu değer *koşullu* + *terrain-bağlı* → bütçe içinde, Part 9) |

> **Faset:** Iron Warden *ileri* iter (Rally). Vael *yerinde durur* (Mevzi). İkisi aynı fraksiyonun saldırı/savunma kutupları.

## 3.2 — Provost Aldric Venn — "Demir Defter"

| Alan | Tanım |
|---|---|
| **Ad** | Provost Aldric Venn |
| **Unvan** | *Demir Defter* (The Iron Ledger) |
| **Lore özeti** | Pact'in baş-levazımcısı; savaşları cephede değil **ikmal hattında** kazandığına inanır. "Aç bir ordu yenilmiştir." Her madeni, her saniyeyi defterine işler. Soğuk, hesapçı, sabırlı. |
| **Görsel kimlik** | Hafif tören-zırh + harita-tüpü/defter; bir elinde teraziyi andıran levazım-asası. Çelik+kobalt + **altın hesap-detayları** (zenginlik değil, *muhasebe* okuması). Silüet: dik ama savaşçı-değil; "yönetici" okuması. |
| **Savaş-alanı rolü** | **Ekonomi-tempo motoru / lojistik kontrol.** |
| **Taktiksel uzmanlık** | **Konumsal ekonomi.** Güvenli-tarafta (kendi statue'sine yakın) tutulan madenleri hızlandırır; çekişmeli/ileri madenleri **değil** — oyuncuyu ekonomiyi *konumlandırarak* güvenceye almaya iter. Quartermaster'ın düz miner-bonusundan farklı: **harita-kontrolü ödüllü.** |
| **Güçlü yönler** | Ekonomik snowball; eğitim-tempo; uzun maçta birim-üstünlüğü; güvenli-ekonomi |
| **Zayıf yönler** | **Doğrudan savaşta zayıf**; erken-rush'a açık (ekonomi compound etmeden); değeri *zaman* ister; agresif Ashen'e karşı tehlikeli |
| **Maç-etki tahmini** | **~%12–14** (ekonomi-konumlu; değer *tempo* olarak birikir, *stat* olarak değil → Part 9) |

> **Faset:** Quartermaster (Iron Warden pasifi) ekonomiye *düz* dokunur. Venn ekonomiyi *konuma* bağlar — daha derin, daha counter'lanabilir karar.

## 3.3 — Magnus Orrin — "Sur Kırıcı"

| Alan | Tanım |
|---|---|
| **Ad** | Magnus Orrin |
| **Unvan** | *Sur Kırıcı* (The Breaker) |
| **Lore özeti** | Pact'in kuşatma-ustası; düşman heykellerini bir cerrah gibi yıkar. "Her surun bir çatlağı vardır; ben onu bulurum." Hesaplı saldırgan — disiplinli bir koçbaşı. |
| **Görsel kimlik** | Ağır plaka + sırtında kuşatma-aletleri/koçbaşı-motifi; miğferinde kırık-sur amblemi. Çelik+kobalt + **çekiç-aşınması** detayları. Silüet: Ironclad-vari ağır ama *yönlü* (ileri-eğik) — "kırıcı" okuması. |
| **Savaş-alanı rolü** | **Objektif/kuşatma tempo'su — anti-structure.** |
| **Taktiksel uzmanlık** | **Hedefleme + konum manipülasyonu.** Heavy/Ranged birimlerin hedef-önceliğini kısa süre heykele/geçide yönlendirir; ve HighGround'dan saldıran Heavy'lere Structure'a-karşı *koşullu* avantaj. Stat değil, **odak + konum.** |
| **Güçlü yönler** | Heykel-baskısı; kuşatma-zamanlaması; kalkan-fazı kırma; bitiş-tempo'su |
| **Zayıf yönler** | **Orduyu öne taahhüt eder** (counter-push'a açık); savunması zayıf; telegraf'lı; yanlış-zamanlı active = açık |
| **Maç-etki tahmini** | **~%13–15** (objektif-tempo; en yüksek-uç, çünkü heykel = kazanç-koşulu → Part 9'da sıkı denetlenir) |

> **Faset:** Iron Pact'in üç komutanı bir **kuşatma-üçgeni** oluşturur: Vael *tutar*, Venn *besler*, Orrin *kırar* — disiplin doktrininin savunma/ekonomi/saldırı eksenleri.

---
# PART 4 — ASHEN HORDE (3 yeni komutan)

*Ashen Horde doktrini (§5.1): sürü saldırısı — hız, flank, ucuz-kütle, flanking-burst. Zayıflık: sürekli savunma, AoE, attrition, durmuş-ekonomi. Üç yeni komutan, bu doktrinin **üç farklı yüzünü** ifade eder: flank-mobilite, sürü-feda-tempo, debuff-bilgi-kontrol. Hiçbiri Ashen Warchief'in (WarCry/Bloodthirst) kopyası değil.*

## 4.1 — Sythe — "Kül Rüzgârı"

| Alan | Tanım |
|---|---|
| **Ad** | Sythe (tek-ad; kabilede "isimsiz koşucu") |
| **Unvan** | *Kül Rüzgârı* (The Wind of Ash) |
| **Lore özeti** | Horde'un en hızlı iz-sürücüsü; hiçbir cephe onu önden görmez. "Duvarın bir arkası vardır" der. Sabırsız, kıvrak, asla durmaz — rüzgâr gibi yandan gelir. |
| **Görsel kimlik** | Minimal deri+kürk, hafif; sırtında çapraz-bıçaklar; yüzünde kül-çizgileri. Öküzkanı+kor, **toz-bulanık** hareket-okuması. Silüet: en ince, en kıvrak komutan; "koşucu" okuması. |
| **Savaş-alanı rolü** | **Flank icra / mobilite kontrol.** |
| **Taktiksel uzmanlık** | **Konumlandırma-tutarlılığı.** Flank/back konumundan saldıran birimlerin *mevcut* pozisyonel çarpanını (1.5/2.0 geometri) daha güvenilir uygular ve **choke yavaşlamasını** kısa süre yok sayarak yeniden-konumlanma sağlar. Raw hasar değil — **geometri tutarlılığı.** |
| **Güçlü yönler** | Flank icra; hızlı yeniden-konumlanma; ranged/caster avı; choke-baskısını aşma |
| **Zayıf yönler** | **Önden zayıf** (head-on); attrition'a açık; **Cover/terrain flank'ı bloklar** (counter); kümelenince AoE yemi |
| **Maç-etki tahmini** | **~%12–14** (pozisyonel; mevcut geometri tavanını *aşmaz*, *tutarlılaştırır* → Part 9) |

> **Faset:** Ashen Warchief *hızlandırır* (WarCry, düz tempo). Sythe *konumlandırır* (flank-geometri) — hız → *yön*.

## 4.2 — Korrash — "Kül Dalgası"

| Alan | Tanım |
|---|---|
| **Ad** | Korrash |
| **Unvan** | *Kül Dalgası* (The Ash Tide) |
| **Lore özeti** | "Bir savaşçı düşer, on tanesi onun külünden doğar." Horde'un feda-doktrininin peygamberi; kaybı *yakıta* çevirir. Acımasız, sayıların efendisi — tek tek birim umrunda değil, *dalga* önemli. |
| **Görsel kimlik** | Kemik-zırh, kül-pelerin (düşen yoldaşların külü); elinde dalga-totemi. Öküzkanı+kor + **kül-beyazı**. Silüet: ortaboy ama *çok-okuması* (etrafında daima birim) — "sürü-lideri." |
| **Savaş-alanı rolü** | **Sürü-tempo / harcanabilir kütle motoru.** |
| **Taktiksel uzmanlık** | **Feda → tempo dönüşümü.** Yakında bir müttefik öldüğünde, çevredeki müttefiklere kısa tempo (Hasted) verir — sürünün harcanabilirliğini *momentum'a* çevirir; ve ordu küçükken eğitim-tempo'su artar (comeback). **Stat-yığını değil, timing/feda.** |
| **Güçlü yönler** | Amansız baskı; trade-ekonomisi; comeback-tempo; düşük-HP momentum |
| **Zayıf yönler** | **Sustain'e karşı erir** (Iron Pact attrition); **AoE feda-değerini siler** (counter); kuşatılınca durur (faraziye-zayıflığı); kendi-kendine snowball'u yok |
| **Maç-etki tahmini** | **~%13–14** (tempo-konumlu; değer *trade-verimi*nde, koşullu → Part 9) |

> **Faset:** Bloodthirst (Warchief pasifi) düz savaş-uptime verir. Korrash *ölümü* kaynağa çevirir — Ashen'in "harcanabilir" kimliğinin derin yüzü.

## 4.3 — Vhirek — "Fısıldayan Veba"

| Alan | Tanım |
|---|---|
| **Ad** | Vhirek |
| **Unvan** | *Fısıldayan Veba* (The Whispering Plague) |
| **Lore özeti** | Horde'un hex-lordu; düşmanın zayıflığını *görür* ve fısıltıyla yayar. "Zehir kılıçtan sabırlıdır." Savaşı bilgi ve çürümeyle kazanır — doğrudan vurmaz, *hazırlar.* |
| **Görsel kimlik** | Kapüşonlu hex-cüppe, kemik-totem, yüzünde göz-dövmesi; çevresinde zehir-yeşili sis. Öküzkanı+kor + **zehir-yeşili** (kontrast aksan). Silüet: eğik, totem-yukarı (Hexcaster-vari ama daha büyük) — "kâhin" okuması. |
| **Savaş-alanı rolü** | **Debuff-sinerji etkinleştirici / bilgi savaşı.** |
| **Taktiksel uzmanlık** | **Bilgi + kontrol.** Düşman statü-zamanlayıcılarını ve sinerji-açık hedefleri (ör. Chilled olanları) müttefik tarafına *gösterir*; ve kendi debuff'larının (Poison/Chill) süresini hafif uzatır. Hasar değil — **bilgi + sinerji-kurulumu.** |
| **Güçlü yönler** | Debuff-sinerji (Chill→Shatter setup); bilgi-avantajı; caster-takım; kontrol |
| **Zayıf yönler** | **Düşük doğrudan hasar**; kuruluma bağımlı; **cleanse + dağılma counter'ı**; tek başına bitiremez (takım-bağımlı) |
| **Maç-etki tahmini** | **~%11–13** (bilgi/kontrol-konumlu; en düşük raw-stat, en yüksek *bilgi* → Part 9) |

> **Faset:** Ashen üçgeni: Sythe *yandan gelir*, Korrash *dalgayla ezer*, Vhirek *zayıflığı hazırlar* — sürü doktrininin mobilite/kütle/bilgi eksenleri.

---
# PART 5 — Komutan Active Yetenekleri

*Her active: EXACTLY 1 (kanon §6). Tüm sayılar PROVISIONAL / LSD-owned (mevcut komutan .asset'leriyle aynı disiplin; cd~40–55, dur~5–8, magnitude ≤0.15, radius~5, spawn-anchored). Etiketler: ♻️ [MEVCUT MEKANİK] = bugünkü StatusKind/sistemle karşılanır · ⚙️ [YENİ SİM-HOOK] = Phase 7.5'te ADR + yeni sim-hook gerektirir (öneri, implementasyon değil).*

## 5.1 Vael — "Mevzi Emri" (Hold Order) ⚙️[YENİ SİM-HOOK: `Fortified` statü]
> *Not: Bu, Blueprint §6'nın kendi komutan-örneğidir — "Iron Pact Shield Wall: +armor to a formation for 6s." Kanon-temelli; yalnızca yeni bir defensive `StatusKind` (`Fortified` = gelen-hasar azaltma) gerektirir.*

| Alan | Değer |
|---|---|
| **Effect** | Müttefikler **Line formasyonunda VEYA tutulan HighGround/Choke'ta** ise → gelen-hasar −X% (`Fortified`), aksi halde etkisiz. **Koşullu** (formasyon+terrain). |
| **Duration** | ~6 s · **Cooldown** ~50 s · **Radius** 5 (spawn-anchored) · **Magnitude** ≤0.15 (clamp'li) |
| **Counterplay** | **Flank et** (buff frontal-disiplin okuması); birimleri terrain'den **sök/çek**; **bekle** (süre dolar); telegraf'ta **burst** indir |
| **Görsel dil** | Kobalt **sur-halkası** yerden yükselir; `Fortified` birimlerde kalkan-emaye parıltısı (okunur, abartısız) |
| **Taktiksel kullanım** | Choke savunması; düşman push'unu kırma; **kalkan-fazını koruma**; geç-oyun sur tutma |

## 5.2 Venn — "Seferberlik" (Mobilize) ⚙️[YENİ SİM-HOOK: training-speed pencere]

| Alan | Değer |
|---|---|
| **Effect** | Side'ın **eğitim-sırası** X% daha hızlı ilerler (ekonomik tempo burst). **Savaş-statı DEĞİL** — yalnızca üretim hızı. |
| **Duration** | ~8 s · **Cooldown** ~55 s · (radius yok — ekonomi geneli) · **Eşdeğer-güç** ≤0.15 tempo-fraksiyonu |
| **Counterplay** | **Ekonomik** → compound etmeden **rush**; madenleri **deny** et; ürettiği ordu yine de **dövüşü kazanmalı** (tempo ≠ güç) |
| **Görsel dil** | Spawn-yapısında **seferberlik bayrağı** (altın-kobalt); eğitim-sırası ikonlarında hız-parıltısı |
| **Taktiksel kullanım** | Kritik-anda birim-dalgası basma; savunma-takviyesi; ekonomik-pencereyi orduya çevirme |

## 5.3 Orrin — "Gedik Emri" (Breaching Order) ⚙️[YENİ SİM-HOOK: targeting-priority pencere]

| Alan | Değer |
|---|---|
| **Effect** | Side'ın **Heavy + Ranged** birimleri kısa süre **hedef-önceliğini heykele/Structure'a** verir (influence-map targeting geçici override). Stat değil — **odak yönlendirme.** |
| **Duration** | ~5 s · **Cooldown** ~50 s · **Radius** 5 · (hasar büyütmez; yalnızca hedef seçer) |
| **Counterplay** | Orduyu **öne taahhüt eder** → **counter-push**; skirmisher'la **intercept** et; yönlendirme sırasında birimler savunmasız |
| **Görsel dil** | Düşman heykelinde kızıl **gedik-işareti**; yönlendirilen birimlerde odak-oku |
| **Taktiksel kullanım** | **Kalkan-fazı sonrası bitiş**; geçit-kırma; heykel-baskısı zirvesi |

## 5.4 Sythe — "Gölge Adım" (Ghoststep) ⚙️[YENİ SİM-HOOK: choke-ignore + reposition] (kısmen ♻️ Hasted)

| Alan | Değer |
|---|---|
| **Effect** | Müttefikler (radius) kısa süre **Choke MoveMult yavaşlamasını yok sayar** + hafif `Hasted` → hızlı **flank yeniden-konumlanma**. |
| **Duration** | ~5 s · **Cooldown** ~45 s · **Radius** 5 · **Magnitude** (Hasted kısmı) ≤0.15 |
| **Counterplay** | **Cover/terrain hâlâ flank'ı bloklar**; kümelenen birimleri **AoE'le**; flank-rotasını **zone'la**; LoS |
| **Görsel dil** | Birimlerde **kül-iz / hayalet-bulanıklık**; choke'ta toz-geçiş izi |
| **Taktiksel kullanım** | Ani flank; choke-aşma; ranged/caster baskını; tehlikeden hızlı çıkış |

## 5.5 Korrash — "Son Nefes" (Last Breath) ⚙️[YENİ SİM-HOOK: on-death tempo]

| Alan | Değer |
|---|---|
| **Effect** | Pencere boyunca: **ölen müttefik**, yakın müttefiklere kısa `Hasted` yayar (feda → momentum). Sürünün harcanabilirliğini tempo'ya çevirir. |
| **Duration** | ~6 s pencere · **Cooldown** ~50 s · **Radius** 5 · **Magnitude** ≤0.15 |
| **Counterplay** | Onların şartlarında **trade etme**; **AoE** kütleyi feda-değeri vermeden siler; pencereyi **baitle**; sustain'le **beklet** |
| **Görsel dil** | Ölen birimden **kül-patlaması** → yakınlara kor-iz dalgası |
| **Taktiksel kullanım** | Büyük trade'i momentum'a çevir; sürü-push zirvesi; çaresiz-savunmada feda-tempo |

## 5.6 Vhirek — "Kül Damgası" (Mark of Ash) ⚙️[YENİ SİM-HOOK: mark + reveal + sinerji-güvenilirlik]

| Alan | Değer |
|---|---|
| **Effect** | Hedef-alan **damgalanır** (telegraf); damgalı düşmanlar müttefik tarafına **görünür (reveal)** + üzerlerindeki **sinerji** (Chill→Shatter vb.) daha güvenilir tetiklenir. Raw hasar değil — **bilgi + sinerji-kurulumu.** |
| **Duration** | ~6 s · **Cooldown** ~45 s · **Radius** ~3 |
| **Counterplay** | **Dağıl** (alan-etkili); **cleanse**; telegraf'ı **dodge**; LoS/Cover'a gir |
| **Görsel dil** | Yerde zehir-yeşili **damga-halkası**; damgalı düşmanlarda görünür işaret (okunur, dürüst telegraf) |
| **Taktiksel kullanım** | Shatter/AoE öncesi **kurulum**; bilgi-penceresi; caster-takım koordinasyonu |

> **Active tasarım ilkesi (tümü):** Her active bir **zamanlama kararı**dır (cd + telegraph + counter), 1-tuş-kazan değil. Hiçbiri raw-stat'ı bütçe-tavanının üstüne çıkarmaz; değer **koşul + konum + bilgi**de. ⚙️ etiketli hook'lar Phase 7.5 ADR'sinde değerlendirilir — bugün hiçbiri kanon değildir.

---
# PART 6 — Komutan Pasif Yetenekleri

*Her pasif: EXACTLY 1 (kanon §6). **Zorunluluk:** taktiksel · pozisyonel · formasyon-farkında · ekonomi-farkında · bilgi-farkında. **Yasak:** basit hasar-buff'ı · jenerik stat-şişirme. Aşağıdaki her pasif, en az 2 zorunlu-boyutu karşılar ve **hiçbiri raw hasar/HP vermez.** Pasifler sürekli-yenilenen, komutan gidince kendiliğinden sönen yapıdadır (CommanderAbilitySystem deseni).*

## 6.1 Vael — "Tahkimat" (Entrenchment) — `formasyon + pozisyonel + bilgi`
| Alan | Değer |
|---|---|
| **Effect** | **Line formasyonunda + sabit duran** (≥N sn hareketsiz) müttefikler **kademeli** dayanıklılık kazanır (gelen-hasar azalır), hareket edince **sıfırlanır**. + Tutulan terrain'e yaklaşan düşmanı **reveal** eder. |
| **Boyutlar** | Formasyon-farkında (Line) · Pozisyonel (sabit-duruş + terrain) · Bilgi (reveal) |
| **Neden stat-inflation değil** | Tamamen **koşullu** (formasyon + hareketsizlik); saldırınca/ilerleyince yok olur → "duvar tut" ödülü, "daha güçlü ol" değil. Tavan ≤budget. |
| **Mekanik** | ⚙️[YENİ: `Fortified` kademe + reveal-hook] |

## 6.2 Venn — "İkmal Hattı" (Supply Line) — `ekonomi + pozisyonel + bilgi`
| Alan | Değer |
|---|---|
| **Effect** | **Kendi statue'süne yakın (güvenli-yarı)** tutulan madenler daha hızlı yield verir; **çekişmeli/ileri** madenler vermez. + Tüm madenlerin **çekişme durumunu** (kim kontrol ediyor) gösterir. |
| **Boyutlar** | Ekonomi-farkında (maden-yield) · Pozisyonel (güvenli-yarı koşulu) · Bilgi (çekişme okuması) |
| **Neden stat-inflation değil** | Savaş-statına **dokunmaz**; yalnızca ekonomiyi **konuma** bağlar. Oyuncu ekonomiyi *konumlandırarak* kazanır → harita-kontrolü kararı. |
| **Mekanik** | ⚙️[YENİ: konum-bazlı maden-yield + çekişme-reveal] |

## 6.3 Orrin — "İstihkâm Gözü" (Sapper's Eye) — `bilgi + pozisyonel + taktiksel`
| Alan | Değer |
|---|---|
| **Effect** | Düşman heykelinin **kalkan/faz zamanlamasını** (ne zaman kırılabilir) müttefik tarafına **gösterir**. + **HighGround'dan** Structure'a saldıran Heavy birimlere **koşullu** anti-Structure avantajı (yalnızca yüksek-zemin + yalnızca Structure'a). |
| **Boyutlar** | Bilgi (kalkan/faz okuması) · Pozisyonel (HighGround koşulu) · Taktiksel (kuşatma zamanlaması) |
| **Neden stat-inflation değil** | Avantaj **çift-koşullu** (HighGround **ve** Structure-hedef); birim-vs-birim savaşı etkilemez → kuşatma-zamanlaması bilgisi, genel güç değil. Capped. |
| **Mekanik** | ⚙️[YENİ: statue-faz reveal + koşullu Structure-mult] |

## 6.4 Sythe — "İz Sürücü" (Pathfinder) — `pozisyonel + bilgi`
| Alan | Değer |
|---|---|
| **Effect** | **Flank/back** konumundan saldıran müttefiklerin **mevcut** pozisyonel çarpanı (1.5/2.0 geometri) daha **güvenilir** uygulanır (kenar-durumlarda kaçırmaz). + Savunmasız düşman **flank'larını reveal** eder. |
| **Boyutlar** | Pozisyonel (flank-geometri tutarlılığı) · Bilgi (açık-flank okuması) |
| **Neden stat-inflation değil** | Pozisyonel **tavanı yükseltmez** (1.5/2.0 sabit); yalnızca *zaten hak edilen* geometriyi tutarlılaştırır → "doğru konumlan" ödülü. Önden hiçbir bonus yok. |
| **Mekanik** | ⚙️[YENİ: flank-tutarlılık + flank-reveal] (pozisyonel-mult zaten Phase 2'de var) |

## 6.5 Korrash — "Harcanabilir" (Expendable) — `ekonomi + taktiksel (comeback)`
| Alan | Değer |
|---|---|
| **Effect** | Ordu **kütlesi düştükçe** (canlı birim sayısı azaldıkça) eğitim **maliyeti hafif düşer + hızlanır** (comeback tempo). Ordu büyükken **etkisiz** (snowball'a yardım etmez). |
| **Boyutlar** | Ekonomi-farkında (eğitim maliyet/hız) · Taktiksel (comeback timing) · Anti-snowball |
| **Neden stat-inflation değil** | Savaş-statı yok; yalnızca **ekonomik comeback**; *kaybedene* tempo verir, *kazanana* değil → maçı uzatır, snowball'u değil. |
| **Mekanik** | ⚙️[YENİ: ordu-kütlesi-bazlı eğitim-ölçek] |

## 6.6 Vhirek — "Veba Görüsü" (Plaguesight) — `bilgi + kontrol-farkında`
| Alan | Değer |
|---|---|
| **Effect** | Düşman **statü-efekt zamanlayıcılarını** ve **sinerji-açık** hedefleri (ör. Chilled → Shatter'a açık) müttefik tarafına **gösterir**. + Müttefik **debuff** süreleri (Poison/Chill) hafif uzar (kontrol, hasar değil). |
| **Boyutlar** | Bilgi (statü/sinerji okuması) · Kontrol-farkında (debuff-süre) |
| **Neden stat-inflation değil** | Hasar **büyütmez**; debuff = **kontrol** (yavaşlat/sinerji-kur), raw damage değil; capped. Asıl güç **bilgi**de. |
| **Mekanik** | ⚙️[YENİ: statü-reveal + debuff-süre uzatma (capped)] |

> **Pasif tasarım ilkesi (tümü):** Hiçbiri "+%X hasar" değil. Güç dağılımı: **2 ekonomi, 3 bilgi-ağırlıklı, 4 pozisyonel, 1 formasyon, 1 comeback, 1 kontrol** — kanonun "tactical/utility, not stat inflation" emrini doğrudan uygular. Her pasif **koşullu** (sürekli değil) → ≤budget ispatı Part 9'da.

---
# PART 7 — Yetenek Ağacı Mimarisi

**Tasarım temeli (implementasyon-uyumlu):** `UpgradesConfig.commanderMaxLevel = 10` (HARD CAP) · maliyet Silver, rising (`commanderBaseCostSilver=200`, `commanderCostGrowth=1.5`) · §6: *"small talent tree, utility choices, **sidegrades not power creep**"* · ranked **normalize**. Mimari bu kısıtların **hepsine** uyacak şekilde minimal tutuldu.

## 7.1 Tier yapısı — 4 tier + Mastery (her tier'da 1/2 seç)

| Tier | Açılış (komutan seviyesi) | Tema | Seçim |
|---|---|---|---|
| **Tier 1** | Lv 2 | **Active rafine** | 2 sidegrade'den **1** seç |
| **Tier 2** | Lv 4 | **Passive rafine** | 2 sidegrade'den **1** seç |
| **Tier 3** | Lv 6 | **Taktiksel sidegrade** | 2 sidegrade'den **1** seç |
| **Tier 4** | Lv 8 | **Capstone sidegrade** | 2 sidegrade'den **1** seç |
| **Mastery** | Lv 10 | **Kimlik + prestij** | 1 capstone-rafine (güç değil) + kozmetik kademe |

**Sonuç:** Tam-seviye bir komutan = **4 seçilmiş talent** (tier başına 1) + Mastery. Toplam ağaç = **8 node + mastery** → küçük, okunur, dengelenebilir. Komutan başına içerik-maliyeti sabit (Part 12).

## 7.2 Sidegrade ilkesi (en kritik kural)

Her tier'daki 2 seçenek **karşılıklı-dışlayan** ve **eş-güçlü** (eşit bütçe, farklı oynama-tarzı). Örnek kalıp:
- *"Active'in **menzili** artar"* ↔ *"Active'in **süresi** artar"* (menzil ↔ süre takası)
- *"Pasif **ekonomiye** kayar"* ↔ *"Pasif **savaş-uptime'ına** kayar"* (eko ↔ savaş takası)
- *"Active **daha sık** ama **daha zayıf**"* ↔ *"**daha seyrek** ama **daha güçlü**"* (frekans ↔ yoğunluk; toplam-güç sabit)

**Hiçbir node "+%X hasar" değildir.** Node'lar **bir avantajı başka bir avantajla takas eder** → toplam güç ≤budget sabit kalır, yalnızca *dağılımı* değişir. Bu, power-creep'i yapısal olarak imkânsız kılar (Part 10).

## 7.3 İlerleme kuralları

| Kural | Tanım |
|---|---|
| **Kazanım** | Komutan **kullanılarak** XP biriktirir (oyna → seviye); tier-node açmak Silver (capped maliyet, UpgradesConfig). **Premium parayla güç ASLA** (§6); para yalnızca kozmetik. |
| **Cap** | Lv 10'da durur (sonsuz ağaç yok); Mastery prestij/kozmetik olarak devam eder (güç değil). |
| **Respec (yeniden-dağıt)** | **Serbest veya küçük-Silver** ile talent sıfırlama. Sidegrade'ler *seçim* olmalı, *tuzak* değil → respec ucuz tutulur (oyuncu denesin). |
| **Ranked normalizasyon** | Ranked, talent setini **capped/standart** sürüme normalize eder (playstyle korunur, güç-seviyesi eşitlenir) — mevcut `RankedNormalized` kancasının doğal uzantısı. |
| **Sunucu otoritesi** | Komutan seviye/talent/mastery **server-authoritative** (§12); client-trust yok (capped upgrade deseni, ladder stat-sanity ile uyumlu). |

## 7.4 Mastery felsefesi

Mastery = **prestij + kimlik, güç DEĞİL.** Lv 10'da:
- **Capstone-rafine:** Tier 4 seçimini *keskinleştirir* (yine sidegrade — playstyle netleşir, güç artmaz).
- **Kozmetik kademe:** komutan skin/VFX/voice prestiji (§6 sınırı).
- **Mastery seviyeleri (Lv 10 ötesi):** yalnızca **başlık/rozet/kozmetik** — sayısal güç yok. "Bu komutanı *ustalaştırdım*" ifadesi, "daha güçlüyüm" değil.

> Bu, koleksiyonun uzun-vadeli hedefini **ustalık + ifade** yapar (Part 11), *güç-merdiveni* değil — anti-P2W'nin progression-tarafı.

## 7.5 Gelecek ölçeklenebilirliği

- **Tekdüze çerçeve:** Her yeni komutan **aynı 4-tier + mastery** kalıbına oturur → öngörülebilir denge-yüzeyi, öngörülebilir UI, **komutan başına sabit içerik-maliyeti** (8 node + mastery).
- **Denge ölçeklenir:** Tüm ağaçlar aynı yapıda olduğundan, ranked-normalizasyon ve telemetri-ayarı tek bir sistemle N komutanı yönetir (§16 kombinatoryal-denge riskine yapısal yanıt).
- **Mobil/okunabilirlik:** 4 ikili-seçim + mastery = küçük, başparmak-dostu ekran; RPG-ağı değil. Her node tek-satır okunur.

---
# PART 8 — Bireysel Yetenek Ağaçları

*Her ağaç: 4 tier × 2 karşılıklı-dışlayan sidegrade + Mastery. **Hiçbir node raw hasar/HP vermez** — hepsi takas. Unlock = komutan seviyesi (T1=Lv2, T2=Lv4, T3=Lv6, T4=Lv8, Mastery=Lv10). Tüm değerler PROVISIONAL/LSD-owned, ≤budget.*

## 8.1 IRON PACT ağaçları

### Castellan Vael — "Kırılmaz Sur" (savunma çapası)
| Tier | Node | Effect (sidegrade) | Amaç | Active/Pasif etkileşimi |
|---|---|---|---|---|
| T1-A | **Geniş Mevzi** | Mevzi Emri radius↑, süre↓ | Geniş cepheyi kısa-süre koru | Mevzi'yi cephe-savunmasına çevirir |
| T1-B | **Derin Mevzi** | Mevzi Emri süre↑, radius↓ | Dar geçidi uzun tut | Mevzi'yi choke-savunmasına odaklar |
| T2-A | **Sabırlı Sur** | Tahkimat dayanıklılığı hızlı birikir, tavan aynı | Hızlı-kurulan savunma | Kısa-duruşları ödüllendirir |
| T2-B | **Gözcü Sur** | Tahkimat reveal-menzili↑, dayanıklılık-birikimi↓ | Erken uyarı (bilgi↔dayanıklılık) | Pasifi bilgi-odaklı yapar |
| T3-A | **Çapa** | Terrain'de duran birimler itilemez/yavaşlatılamaz | Bölge-kontrol | Terrain-denial'i güçlendirir |
| T3-B | **Geri Çekilme Disiplini** | Line bozulup ricat ederken kısa Fortified taşınır | Kontrollü ricat | Savunmayı esnetir (mobilite-zayıflığını yamalar) |
| T4-A | **Kuşatma Reddi** | Heykel yakınında Fortified daha etkili | Son-savunma uzmanı | Mevzi+Tahkimat'ı statue-savunmaya bağlar |
| T4-B | **Karşı-Mevzi** | Fortified sırasında ilk düşman vuruşu yavaşlatılır | Aktif-savunma | Savunmaya counter-okuması ekler |
| Mastery | **Kırılmaz** | Seçilen capstone keskinleşir + kozmetik kademe | Kimlik/prestij | Güç artmaz; playstyle netleşir |

### Provost Venn — "Demir Defter" (ekonomi-tempo)
| Tier | Node | Effect (sidegrade) | Amaç | Active/Pasif etkileşimi |
|---|---|---|---|---|
| T1-A | **Hızlı Seferberlik** | Mobilize cd↓, etki↓ | Sürekli küçük tempo | Mobilize'ı ritmik yapar |
| T1-B | **Tam Seferberlik** | Mobilize cd↑, etki↑ | Tek büyük üretim-dalgası | Mobilize'ı zirve-anına saklar |
| T2-A | **Güvenli Hat** | İkmal yield-bonusu↑ ama yalnızca en-yakın maden | Tek-maden snowball | Pasifi yoğunlaştırır |
| T2-B | **Geniş Hat** | Bonus tüm güvenli madenlere yayılır ama↓ | Çok-maden ekonomi | Pasifi yayar |
| T3-A | **İstihbarat Ağı** | Düşman ekonomi/birim-değer farkını gösterir | Ekonomi-okuması (bilgi) | Harita-kararını besler |
| T3-B | **Acil İkmal** | Düşük-Gold'da kısa eğitim-indirimi | Kriz-yönetimi | Mobilize ile comeback combo |
| T4-A | **Lojistik Üstünlük** | Mobilize sırasında maden-yield de artar | Eko-burst combo | Active+Pasif sinerjisi |
| T4-B | **Sabit İkmal** | Pasif bonusu çekişmeli-madende de küçük çalışır | İleri-ekonomi (riskli) | Pasifi agresifleştirir |
| Mastery | **Demir Defter** | Seçilen capstone keskinleşir + kozmetik | Kimlik/prestij | Güç artmaz |

### Magnus Orrin — "Sur Kırıcı" (kuşatma-tempo)
| Tier | Node | Effect (sidegrade) | Amaç | Active/Pasif etkileşimi |
|---|---|---|---|---|
| T1-A | **Geniş Gedik** | Breaching Order radius↑, süre↓ | Çok birimi kısa-süre yönlendir | Toplu-kuşatma |
| T1-B | **Sürekli Gedik** | Breaching Order süre↑, radius↓ | Az birimi uzun-süre odakla | Sürekli-baskı |
| T2-A | **Kalkan Analizi** | Faz-reveal erken/detaylı, Structure-bonus koşulu sıkılaşır | Bilgi↔savaş takası | Pasifi bilgi-odaklı yapar |
| T2-B | **Yüksek Mevzi Ustası** | HighGround Structure-bonusu↑, reveal kaybolur | Savaş↔bilgi takası | Pasifi kuşatma-odaklı yapar |
| T3-A | **Koçbaşı** | Heavy birimler (yalnızca) heykele karşı hız/uptime | Kuşatma-tempo | Breaching ile hız combo |
| T3-B | **Hedef İşareti** | Breaching'in heykeli müttefiklere işaretlenir | Koordineli-kuşatma (bilgi) | Takım-odak |
| T4-A | **Sur Yıkıcı** | Breaching kalkan-fazını daha hızlı tüketmeye yardım | Objektif-bitiş | Active-güç odak (Part 9'da sıkı denetim) |
| T4-B | **İstihkâm Disiplini** | Breaching sırasında birimlere kısa dayanıklılık | Taahhüt-koruması | Counter-push riskini azaltır |
| Mastery | **Sur Kırıcı** | Seçilen capstone keskinleşir + kozmetik | Kimlik/prestij | Güç artmaz |

---
## 8.2 ASHEN HORDE ağaçları

### Sythe — "Kül Rüzgârı" (flank-mobilite)
| Tier | Node | Effect (sidegrade) | Amaç | Active/Pasif etkileşimi |
|---|---|---|---|---|
| T1-A | **Uzun Adım** | Ghoststep radius↑, süre↓ | Geniş yeniden-konumlanma | Toplu-flank |
| T1-B | **Hızlı Adım** | Hasted↑, choke-ignore süresi↓ | Keskin hız-patlaması | Tekil-flank icra |
| T2-A | **Keskin İz** | Flank-tutarlılık↑, reveal-menzili↓ | Geometri↔bilgi takası | Pasifi icra-odaklı yapar |
| T2-B | **Geniş Görü** | Flank-reveal↑, tutarlılık-bonusu↓ | Bilgi↔geometri takası | Pasifi keşif-odaklı yapar |
| T3-A | **Sızma** | Ghoststep Cover'dan geçmeyi kolaylaştırır | Terrain-aşma (flank-rota açar) | Cover-counter'ını yamalar |
| T3-B | **Kovalama** | Flank'tan kaçan düşmana kısa takip-hızı | Flank-kill garantisi | İcra-zinciri |
| T4-A | **Rüzgâr Dönüşü** | Ghoststep bitince kısa geri-konumlanma | Vur-kaç (hit-and-run) | Active'i güvenli yapar |
| T4-B | **Sürü Adımı** | Ghoststep çok birimi etkiler, Hasted↓ | Kütle↔yoğunluk takası | Toplu-flank push |
| Mastery | **Kül Rüzgârı** | Seçilen capstone keskinleşir + kozmetik | Kimlik/prestij | Güç artmaz |

### Korrash — "Kül Dalgası" (sürü-feda-tempo)
| Tier | Node | Effect (sidegrade) | Amaç | Active/Pasif etkileşimi |
|---|---|---|---|---|
| T1-A | **Geniş Nefes** | Last Breath radius↑, momentum↓ | Geniş feda-yayılımı | Toplu-tempo |
| T1-B | **Derin Nefes** | Momentum (Hasted)↑, radius↓ | Yoğun yerel-tempo | Odaklı-push |
| T2-A | **Ucuz Kütle** | Eğitim-maliyet-indirimi↑, hız-bonusu↓ | Maliyet↔hız takası | Pasifi maliyet-odaklı |
| T2-B | **Hızlı Üreme** | Eğitim-hızı↑, maliyet-indirimi↓ | Hız↔maliyet takası | Pasifi tempo-odaklı |
| T3-A | **Kül Hasadı** | Last Breath penceresinde ölümler kısa eğitim-iadesi | Feda→ekonomi | Active'i ekonomiye bağlar |
| T3-B | **Çığ** | Düşük-kütle comeback eşiği yükselir (erken tetik) | Comeback-erken | Pasifi erkene çeker |
| T4-A | **Sonsuz Dalga** | Last Breath momentum'u zincirlenebilir | Push-zirvesi | Active-yoğunluk (Part 9 denetim) |
| T4-B | **Feda Disiplini** | Ölümler kısa Raged de verir, Hasted↓ | Agresif-trade | Tempo↔saldırı takası |
| Mastery | **Kül Dalgası** | Seçilen capstone keskinleşir + kozmetik | Kimlik/prestij | Güç artmaz |

### Vhirek — "Fısıldayan Veba" (debuff-bilgi-kontrol)
| Tier | Node | Effect (sidegrade) | Amaç | Active/Pasif etkileşimi |
|---|---|---|---|---|
| T1-A | **Geniş Damga** | Mark of Ash radius↑, süre↓ | Geniş alan-işareti | Toplu-kurulum |
| T1-B | **Kalıcı Damga** | Mark of Ash süre↑, radius↓ | Uzun tekil-işaret | Odaklı-sinerji |
| T2-A | **Derin Görü** | Statü/sinerji reveal-detayı↑, debuff-süre-bonusu↓ | Bilgi↔kontrol takası | Pasifi bilgi-odaklı |
| T2-B | **Uzun Çürüme** | Debuff-süre-bonusu↑, reveal↓ | Kontrol↔bilgi takası | Pasifi kontrol-odaklı |
| T3-A | **Sinerji Çağrısı** | Damgalı hedeflerde sinerji müttefiklere işaretlenir | Takım-sinerji (koordinasyon) | Active'i takıma bağlar |
| T3-B | **Bulaşma** | Damga yakın düşmanlara kısa yayılır, zayıflar | Çoklu-hedef alan-kontrol | Active'i yayar |
| T4-A | **Veba Lordu** | Damgalı düşmanların debuff'ları biraz daha uzun | Kontrol-zirvesi | Active+Pasif sinerjisi |
| T4-B | **Kâhin** | Görü düşman komutan-active hazırlığını da gösterir, debuff-bonus↓ | Üst-bilgi (counter-okuması) | Bilgi-zirvesi |
| Mastery | **Fısıldayan Veba** | Seçilen capstone keskinleşir + kozmetik | Kimlik/prestij | Güç artmaz |

> **48 node + 6 mastery denetimi:** Hiçbiri "+%X hasar/HP" içermez. Her tier **karşılıklı-dışlayan** (1 seç) → bir oyuncu tüm avantajları **yığamaz**; yalnızca *playstyle*'ını seçer. Toplam komutan-gücü her seçimde ≤budget **sabit** kalır (Part 9). Ranked tüm setleri normalize eder.

---
# PART 9 — %15 Güç Bütçesi Denetimi (KRİTİK)

## 9.1 Metodoloji — maç-etkisini 4 kanala ayır

Komutan maç-etkisi **tek bir sayı değildir**; dört kanaldan oluşur. Kanon clamp'i (`k_PowerBudgetCeiling=0.15` + ADR-2-002) yalnızca **stat-kanalını** mekanik olarak sınırlar; diğer üç kanal **tasarımla** sınırlanır (koşulluluk + counterplay + tek-komutan-kuralı).

| Kanal | Simge | Nasıl sınırlanır |
|---|---|---|
| **Stat/savaş** | Cs | **Mekanik** — ADR-2-002 commander-sourced buff'ları birleştirir ve ≤0.15'e clamp'ler. Aşılamaz. |
| **Taktiksel/pozisyonel** | Ct | **Tasarım** — koşullu (formasyon/terrain/konum); counterplay bedava değeri siler → beklenen-değer tepe-değerin altında |
| **Ekonomi** | Ce | **Tasarım** — yavaş birikir; rush/maden-deny ile counter'lanır; tempo ≠ anlık güç |
| **Bilgi** | Ci | **Tasarım** — tavanlı; bilgi *kararı* iyileştirir, dövüşü *kazanmaz*; takım onu kullanmalı |

**Güvenlik tabanı (tahmin hatası olsa bile):** (1) Cs **mekanik olarak** ≤0.15; (2) **side başına TEK komutan** → komutan-vs-komutan stacking imkânsız (yapısal); (3) tüm Ct/Ce/Ci **koşullu** → counter'lanabilir; (4) **ranked normalize**. Yani aşağıdaki Ct/Ce/Ci tahminleri yanılsa dahi, çok-kanallı toplam **yapısal olarak** ≤~%15 bandında kalır. *Tüm sayılar PROVISIONAL/LSD-owned + telemetri-ayarlı (kanon disiplini).*

## 9.2 Komutan başına bütçe denetimi (tahmini maç-etki katkısı)

| Komutan | Cs (stat) | Ct (taktik) | Ce (ekonomi) | Ci (bilgi) | **Toplam** | Gücün kasıtlı kısıtlandığı yer |
|---|---|---|---|---|---|---|
| **Vael** | ~6–8% (Fortified, **koşullu**) | ~3–4% (terrain-denial) | 0% | ~1–2% (reveal) | **~13%** | Buff yalnızca Line+terrain+sabit-duruşta; **saldırınca/mobilken sıfır**; **flank bypass eder** |
| **Venn** | ~0% | ~2% (tempo) | ~8–10% (konum-eko) | ~2% (maden-okuma) | **~12–14%** | **Sıfır savaş-gücü**; eko **yavaş + konuma bağlı**; rush/maden-deny counter'lar |
| **Orrin** | ~4–5% (**çift-koşullu** anti-Structure) | ~5–6% (hedef-yönlendirme + kuşatma) | 0% | ~2–3% (faz-reveal) | **~13–15%** | En yüksek-uç (objektif=kazanç). Active **orduyu öne taahhüt eder** (öz-risk); anti-Structure **HighGround+Structure** çift-koşullu; birim-savaşına dokunmaz. **Telemetri-izleme önceliği.** |
| **Sythe** | ~3–4% (Ghoststep Hasted, kısa) | ~5–6% (flank-tutarlılık) | 0% | ~2% (flank-reveal) | **~12–14%** | Pasif **1.5/2.0 tavanını YÜKSELTMEZ** (tutarlılaştırır); **önden sıfır**; Cover/terrain counter'lar |
| **Korrash** | ~5–6% (Last Breath, **ölüm-koşullu**) | ~3–4% (trade-momentum) | ~3–4% (comeback) | 0% | **~13–14%** | Pasif **yalnızca kaybederken** (anti-snowball); active **ölüm gerektirir** (trade'i sen kontrol et); **AoE counter'lar** |
| **Vhirek** | ~0% (hasar-buff yok) | ~3–4% (debuff=kontrol) | 0% | ~6–7% (reveal/sinerji) | **~11–13%** | **En düşük raw güç**; bilgi/kurulumu **takım çevirmeli**; **cleanse+dağılma counter'lar** |

## 9.3 Yetenek-ağacının bütçeye etkisi (kritik)

Talent ağacı **bütçeyi BÜYÜTMEZ.** Her tier **karşılıklı-dışlayan** (1/2 seç) ve seçenekler **eş-güçlü takas** → oyuncu gücü *yığamaz*, yalnızca *dağıtır*. Örnek: Vael T1-A (radius↑/süre↓) ile T1-B (süre↑/radius↓) **aynı toplam-güçtedir**, farklı şekildedir. Dolayısıyla tam-talent komutan ≈ talent-siz komutan **güç-seviyesi** (≤budget); yalnızca *playstyle* netleşir. Ranked, seti normalize ederek bunu garanti eder.

## 9.4 Gücün kasıtlı kısıtlandığı yerler (özet)

1. **Stat-kanalı mekanik clamp** (ADR-2-002): hiçbir komutan-buff'ı, birleşik, ≤0.15'i aşamaz — kod-seviyesinde.
2. **Koşulluluk:** her ana-yetenek bir koşula bağlı (formasyon/terrain/konum/ölüm/güvenli-maden) → sürekli-değer yok.
3. **Counterplay:** her active'in telegraf+counter'ı var (§5.3 disiplini komutanlara taşındı); flank/AoE/cleanse/rush bedava-değeri siler.
4. **Tek-komutan-kuralı:** side başına 1 `CommanderRuntime` → in-battle stacking yapısal olarak imkânsız.
5. **Sidegrade-ağacı:** talent güç yığmaz, takas eder.
6. **Ranked normalizasyon:** capped set, playstyle korur güç eşitler.
7. **Telemetri-ayarı:** tüm sayılar LSD-owned; Orrin (en yüksek-uç) öncelikli izleme; yanlış-ayar ≤budget tavanıyla affedilir (§16 risk #5'e yapısal yanıt).

> **Sonuç:** Altı komutan da **~%11–15** bandında, tavanın altında. En riskli (Orrin, objektif-tempo) bile **çift-koşul + öz-risk + birim-savaşına-dokunmama** ile sınırlı. Hiçbiri "super-unit" değil; hepsi **force-multiplier** (§6). Bütçe **airtight**.

---
# PART 10 — Exploit & Meta Denetimi

## 10.1 Stacking riskleri

| Risk | Durum | Mitigasyon |
|---|---|---|
| **Komutan active + passive aynı-tür buff** | **Çözülmüş** — ADR-2-002 commander-sourced'ı birleştirir ve ≤budget'e clamp'ler | Kod-seviyesi; ek önlem gerekmez |
| **Komutan-vs-komutan stacking** (bir orduda 2 komutan) | **Yapısal olarak imkânsız** — side başına 1 `CommanderRuntime` | Yapı; koleksiyon bunu değiştirmez (savaşta yine 1 seçili) |
| **Komutan buff + spell buff** | **Kasıtlı ayrı katman** (ADR-2-002); spell §5.3 counter'lı | Spell tarafı telegraf+counter+cd ile sınırlı; §6 leak yok |
| **Talent stacking** | **İmkânsız** — tier'lar karşılıklı-dışlayan (1/2 seç) | Tasarım; tüm avantajlar yığılamaz |

## 10.2 Komutan + spell-draft sinerjileri (kasıtlı derinlik — ama izlenmeli)

Tek-komutan kuralı komutan-çiftlerini engeller; ama komutan + **draft-3 spell** sinerjileri taktiksel derinliktir. İzleme listesi:

| Sinerji | Risk | Mitigasyon |
|---|---|---|
| **Vhirek "Kül Damgası" + Freeze→Shatter draft** | Damga + sinerji-burst | Mark **telegraflı**; spell katmanı **counterable** (dağıl/cleanse); Vhirek raw-hasarı düşük |
| **Korrash "Sonsuz Dalga" + Summon-spam (ölüm-döngüsü)** | Zincirleme momentum | Zincir **pencere-sınırlı**; Hasted **clamp'li**; **AoE** feda-değerini siler |
| **Orrin "Sur Yıkıcı" + Heavy-spam statue-rush** | Hızlı heykel-bitiş | Anti-Structure **çift-koşullu**; active **orduyu öne taahhüt eder** → **counter-push penceresi** |
| **Venn eko-snowball + güvenli-choke harita** | Durdurulamaz ekonomi | Yalnızca **güvenli-yarı** madenler; **rush/maden-deny**; sıfır savaş-gücü |
| **Vael + choke-harita stall** (maç-uzatma) | Kırılmaz duvar → timeout | **Flank** (frontal-koşul); terrain'den **çek**; **Orrin/siege counter**; mod kazanç-koşulları |

## 10.3 Talent suistimal vakaları

| Vaka | Mitigasyon |
|---|---|
| **Bir tier seçeneği diğerinden kesin-üstün** (denge hatası) | **Eş-güç tasarımı** + telemetri-izleme + ranked-normalizasyon; gerekirse RC ile yeniden-ayar (app-update'siz) |
| **Maç-içi respec ile anlık-optimize** | **Talent maç-başında kilitli**; respec yalnızca savaş-dışı → maç-içi exploit yok |
| **"Must-pick" capstone meta'sı** | Her capstone bir *playstyle*; counter'ı var; telemetri pick/win-rate izler; dominant çıkarsa normalize/ayar |

## 10.4 Gelecek komutan etkileşimleri (roster büyüdükçe)

Koleksiyon büyüdükçe (Phase 7.5+) riskler ve yapısal cevaplar:
- **Dominant-pick riski** → her komutan **net counter'lı** tasarlanır (10.5); ≤%15 tavanı **yanlış-dengeyi affeder**; telemetri+RC+normalizasyon sürekli ayarlar.
- **Harita/komp-trivialize riski** → komutan gücü **birim-savaşına dokunmaz** (çoğu Ct/Ce/Ci kanalı); harita-çeşitliliği + counter'lar korur.
- **Kombinatoryal patlama (§16 risk #5)** → **tekdüze 4-tier çerçeve** + ≤budget tavanı, N komutanı tek denge-sistemiyle yönetir.

## 10.5 Meta-denge: 8-komutan taş-kâğıt-makas (strictly-dominant yok — §6)

Tam roster (2 mevcut + 6 yeni) bir counter-ağı oluşturur (kabaca):
- **Agresif** (Sythe flank, Korrash sürü, Ashen Warchief) → **ekonomi**yi (Venn) ezer (compound etmeden).
- **Savunma** (Vael) → **agresif**i kırar (duvar+attrition).
- **Kuşatma** (Orrin) → **savunma**yı (Vael stall) kırar (objektif-tempo).
- **Bilgi/kontrol** (Vhirek) → sinerji-komplara güç verir ama tek-başına zayıf → **takım-bağımlı**.
- **Ekonomi** (Venn, Iron Warden-Quartermaster) → uzun-maçta **kuşatma/savunma**yı besler.

Hiçbiri tüm matchup'larda üstün değil → **strictly-dominant pick yok** (§6 fairness). Denge **telemetri + RC + ranked-normalizasyon** ile sürdürülür (kanonun mevcut araçları).

## 10.6 Koleksiyon-sistemi riskleri (P2W-drift)

| Risk | Mitigasyon (kanon-zorunlu) |
|---|---|
| **Parayla-güç (koleksiyon = güç)** | Komutanlar **earnable**; premium **yalnızca kozmetik**; ≤%15 tavan → koleksiyon *çeşitlilik*, *güç değil* |
| **Power-creep (yeni = güçlü)** | **Sabit ≤%15 tavan** + **sidegrade ağaç** → yeni komutan eskisini *geçemez*, yalnızca *farklılaşır* |
| **"Accelerate-purchase" → pay-for-power kayması** | Para yalnızca **zaman/kozmetik** (§9 convenience); **güç-kilidi parayla açılamaz**; ranked normalize |
| **Exclusive-power / FOMO** | **CUT** (Decision Log §1); tüm komutanlar earnable; sezonsal yalnızca **kozmetik** |
| **Gacha-for-power** | **CUT** (ilkeli, kalıcı); komutan açılışı **deterministik** (disclosed), random-power-box değil |

---
# PART 11 — Komutan Koleksiyon Sistemi

*Phase 7.5 ön-tasarımı. Kanon zorunlulukları: **no P2W · roadmap-uyumlu · future-proof · earnable · cosmetic-only monetization · server-authoritative · capped · ranked-normalized.***

## 11.1 Açılış yolu (unlock) — earnable-first

| Mekanizma | Detay | Kanon |
|---|---|---|
| **Oynayarak kazan** | Komutanlar **earned** komutan-token/shard ile açılır (quest/pass/event/başarım/kampanya-kilometre-taşı). **Para-güç ASLA.** | §6, §9 (earned-only) |
| **Battle Pass** | Sezon pass'i (free+premium track) komutan-token verebilir; premium yalnızca **kozmetik+convenience**, komutanın *kendisi* free-track'ten de erişilebilir | §10 |
| **Opsiyonel hızlandırma** | Gems ile **zaman-atla** (daha hızlı aç) — ama komutan **her zaman free-earnable** ve **exclusive-power yok** | Blueprint §6 ("optional accelerate-purchase, never exclusive power") |
| **Deterministik** | Açılış **disclosed/deterministik** (gacha-for-power DEĞİL) | Decision Log §1 (CUT) |

> **P2W-güvenliği:** Para komutanı *erken* açabilir ama *daha güçlü* yapamaz (≤%15 + ranked-normalize). Sıfır exclusive-power. Bu, §6/§9/§10'un birleşik kanonu.

## 11.2 İlerleme yolu (progression) — capped

- **Komutan XP:** komutanı **kullanarak** XP → seviye 1→10 (`commanderMaxLevel=10`).
- **Talent açılışı:** seviye atladıkça tier açılır; node-açmak **Silver** (capped maliyet, `commanderBaseCostSilver=200`, growth 1.5).
- **Cap:** Lv10'da durur → **sonsuz güç-merdiveni yok**. Silver-sink sağlıklı (meta-ekonomi).
- **Server-authoritative:** seviye/talent/mastery **sunucuda** (§12); ladder stat-sanity ile uyumlu (caps doğrulanır).

## 11.3 Ustalık yolu (mastery) — prestij, güç değil

- Lv10 → **Mastery seviyeleri:** yalnızca **başlık/rozet/kozmetik kademe** (sayısal güç YOK).
- "Bu komutanı ustalaştırdım" = *prestij + ifade*, *avantaj değil*.
- **Koleksiyon meta-hedefi:** tüm komutanları Lv10 + mastery → uzun-vadeli prestij-yolu (P2W değil).

## 11.4 Kozmetik yolu — gelir motoru (§6/§10)

- **Komutan kozmetikleri:** skin / VFX-renk / voice (§6 sınırı: silüet/okunabilirlik/güç değişmez).
- **Edinim:** earned (pass/event/free-chest) **ve** satın-alınabilir (shop, gem, **see-what-you-buy** — gacha değil).
- **Kademeler:** Standard→Mythic (§6 outfit-class), yalnızca görsel zenginlik.
- **Clarity-mode:** ranked'de standart okunur komutan-görseli (kozmetik okuma-avantajı sıfır).

## 11.5 Uzun-vadeli koleksiyon hedefleri

| Hedef | Tip | Ödül tipi |
|---|---|---|
| Tüm komutanları **aç** | Toplama | Roster tamamlama (earned) |
| Hepsini **Lv10** | İlerleme | Talent-çeşitliliği (playstyle) |
| Hepsini **ustalaştır** | Prestij | Başlık/rozet/kozmetik |
| **Kozmetik setleri** tamamla | İfade | Görsel prestij |
| Her komutanla **ranked tırman** | Beceri | Sezon-ödülü (kozmetik/Honor — §9 Phase 7) |

**Hepsi prestij + ifade + çeşitlilik** ekseninde — **güç-merdiveni değil.** Bu, anti-SLG kimliğinin (Successor §10) progression-tarafıdır.

## 11.6 Roadmap-uyumu & future-proofing

- **Yuva:** Phase 7.5 (DEFER); trigger: "MVP retention validated; commander bones stable." **Tetik ateşlenmeden implemente edilmez.**
- **Para birimi:** mevcut 4 (Silver=talent, Gems=kozmetik/convenience); **yeni power-currency YOK** (§9 İHLAL EDİLEMEZ). (Honor=ranked, Phase 7.1 — kozmetik-only.)
- **Tekdüze çerçeve:** her yeni komutan aynı yapı → öngörülebilir denge/UI/maliyet.
- **MVP bones uyumu:** mevcut Iron Warden + Ashen Warchief bu sisteme **sorunsuz** girer (aynı 1-active+1-passive + talent çerçevesi) → koleksiyon onları *ilk iki üye* yapar, yeniden-yazım gerekmez.

---
# PART 12 — Gelecek İmplementasyon Maliyeti

*Phase 7.5 maliyet tahmini. "Mevcut" = bugünkü `CommanderAbilitySystem` (1 active+1 passive, StatusEffect, ADR-2-002 clamp). Karmaşıklık: 🟢 Düşük (mevcut mekanik reuse) · 🟡 Orta (yeni hook) · 🔴 Yüksek (yeni sistem).*

## 12.1 Gameplay sistemleri gerekli

| Sistem | Karmaşıklık | Not |
|---|---|---|
| Yeni `CommanderActiveKind` / `CommanderPassiveKind` enum'ları (6+6) | 🟢 | Veri-genişletme |
| **`Fortified` StatusKind** (savunma azaltma; Vael) | 🟡 | Yeni statü + Combat okuması (Blueprint §6 zaten öngörür) |
| **Training-speed hook** (Venn Mobilize/Expendable) | 🟡 | TrainingSystem komutan-modifier |
| **Targeting-priority override** (Orrin Breaching) | 🔴 | Influence-map targeting'e geçici override — en hassas |
| **On-death tempo event** (Korrash Last Breath) | 🟡 | Ölüm-olayı kancası → yakın-müttefik buff |
| **Choke-ignore + reposition** (Sythe Ghoststep) | 🟡 | Movement/Terrain MoveMult bypass |
| **Mark + reveal + sinerji-güvenilirlik** (Vhirek) | 🔴 | Yeni mark-component + sinerji-tetik güvenilirliği |
| **Konum-bazlı maden-yield** (Venn Supply Line) | 🟡 | Mining'e güvenli-yarı koşulu |
| **Formasyon-durum + stationary tracking** (Vael Entrenchment) | 🟡 | Formasyon + hareketsizlik sayacı |
| **Flank-tutarlılık** (Sythe Pathfinder) | 🟢 | Pozisyonel-mult zaten var; güvenilirlik-ayarı |
| **Bilgi/Reveal katmanı** (statü-timer / maden-çekişme / statue-faz / flank / komutan-cd) | 🔴 | Gizli-info'yu UI'ya yüzeyleyen yeni info-display sistemi (fog değil; tek-cephe görünür, ama *timer/intent/vulnerability* gizli) |
| **Talent sistemi** (tanım-veri + uygulama-katmanı + respec + ranked-normalize) | 🔴 | Active/passive parametrelerini değiştiren veri-sürücülü katman |

> **Maliyet sinyali (Part 13 için):** Vael (Fortified+formasyon) ve Sythe (pozisyonel reuse) **en ucuz**; Orrin (targeting-override) ve Vhirek (mark+reveak) **en pahalı**. Reveal-katmanı + talent-sistemi tüm komutanlarca paylaşılan **tek-seferlik altyapı** maliyetidir.

## 12.2 Backend etkisi

| Alan | Karmaşıklık | Not |
|---|---|---|
| Komutan sahipliği + seviye + talent + mastery (profil alanları) | 🟡 | Server-authoritative profil genişletme (§12) |
| Earned komutan-token grant doğrulama | 🟡 | Mevcut server-grant deseni (Silver/Gems gibi); **yeni power-currency YOK** |
| Talent ranked-normalizasyon (capped-set çözümü) | 🟡 | Mevcut `UpgradeCapTable` + ladder stat-sanity genişletme |
| Komutan kozmetik envanteri | 🟢 | Mevcut kozmetik-envanter deseni |

**Net:** Mevcut server-auth profil/ekonomi/ladder-doğrulama **genişletilir**; yeni backend mimarisi gerekmez (BaaS yeterli, §12).

## 12.3 Save-data etkisi

- Oyuncu başına: `unlockedCommanders[]` + komutan başına `{level, xp, talent[4], masteryLevel}` + komutan-kozmetik sahipliği + fraksiyon-başı seçili loadout.
- **Sınırlı/lineer:** roster ile doğru orantılı, küçük kayıt; server-profile'a sığar.
- **Ladder ghost:** `GhostSnapshot` zaten `commanderLevel` + cap-doğrulama taşır → **talent-set** alanı ekle (normalizasyon için). Mevcut yapıyla uyumlu.

## 12.4 UI etkisi

| Ekran | Karmaşıklık |
|---|---|
| Komutan **koleksiyon** ekranı (grid, kilitli/açık, mastery rozet) | 🟡 |
| **Talent ağacı** ekranı (4 tier × 2, respec, mobil-dostu) | 🟡 |
| Komutan **detay/loadout** (active/passive önizleme, lore, kozmetik) | 🟢 |
| In-battle **reveal/info overlay** (statü-timer, maden-çekişme, statue-faz, flank, komutan-cd) | 🔴 |
| **Mastery + kozmetik** ekranları | 🟢 |

> In-battle reveal overlay **okunabilirlik bütçesini** zorlamamalı (§11/§6) — bilgi *katmanlı/isteğe-bağlı* gösterilmeli (clutter İHLAL EDİLEMEZ değil ama risk).

## 12.5 Denge eforu

| Eksen | Efor | Not |
|---|---|---|
| 8 komutan × 4 talent-seçimi | 🟡 | ≤%15 tavan + sidegrade + normalizasyon **sınırlar** |
| Telemetri (pick/win-rate komutan+talent) | 🟡 | Mevcut analytics-pipeline genişletme |
| RC canlı-ayar | 🟢 | Mevcut 3-tier resolver (app-update'siz) |
| **Bilgi-kanalı dengesi** | 🔴 | Info-değeri **dolaylı** → telemetri ile ölçmesi en zor; Vhirek/reveal'ı yakın izle |
| Her yeni komutan = tekrarlayan denge-geçişi | 🟡 | Tekdüze çerçeve maliyeti öngörülebilir kılar |

**Özet:** Toplam maliyet **orta-yüksek**, ama **tek-seferlik altyapı** (talent-sistemi + reveal-katmanı) + **komutan-başına artımlı** (enum+hook+veri+denge) olarak ayrışır. İlk komutan pahalı (altyapı), sonrakiler ucuzlar (çerçeve). Bu, Part 13'ün "önce hangisi" kararını besler.

---
# PART 13 — Nihai Tavsiye

## 13.1 Kanon-netliği: "MVP" donmuştur

**Kanon, MVP'yi 2 komutanla kilitler** (Iron Warden + Ashen Warchief, §5.5). Bu raporun 6 komutanı **hepsi post-MVP'dir** (Phase 7.5 koleksiyon). Dolayısıyla aşağıdaki sınıflandırma, **koleksiyon-özelliği açıldığında** (trigger ateşlendiğinde) hangi dalgada geleceklerini belirtir — oyun-MVP'sini değil. *"Launch candidate" = koleksiyonun ilk dalgası (S1–S2); "Post-launch" = sonraki dalgalar.*

## 13.2 Sıralama (composite skor)

Eksenler: **Bütçe-güvenliği** (≤%15 netliği) · **İmplementasyon maliyeti** (ucuz=erken) · **Ayırt-edicilik** (mevcut 2'den farklılık) · **Okunabilirlik/onboarding** · **Fraksiyon-dengesi**.

| Sıra | Komutan | Fraksiyon | Bütçe | Maliyet | Ayırt-edicilik | Okunur | **Sınıf** |
|---|---|---|---|---|---|---|---|
| **1** | **Sythe** | Ashen | ★★★★★ (cap yükseltmez) | 🟢 en ucuz (pozisyonel reuse) | ★★★★ (flank≠WarCry hız) | ★★★★ | **İlk dalga (Launch)** |
| **2** | **Vael** | Iron | ★★★★★ (koşullu savunma) | 🟢–🟡 (Fortified+formasyon) | ★★★★★ (savunma≠Rally saldırı) | ★★★★★ | **İlk dalga (Launch)** |
| **3** | **Korrash** | Ashen | ★★★★ (ölüm-koşullu) | 🟡 (on-death event) | ★★★★ (feda≠Bloodthirst) | ★★★★ | **2. dalga (Post-launch erken)** |
| **4** | **Venn** | Iron | ★★★★★ (sıfır savaş-gücü) | 🟡 (maden/eğitim hook) | ★★★★ (konum-eko≠Quartermaster) | ★★★ (subtle) | **2. dalga (Post-launch erken)** |
| **5** | **Vhirek** | Ashen | ★★★★ (düşük raw) | 🔴 (mark+reveal) | ★★★★★ (bilgi-savaşı, yeni) | ★★★ (takım-bağımlı) | **3. dalga (Post-launch)** |
| **6** | **Orrin** | Iron | ★★★ (objektif-tempo, en sıkı ayar) | 🔴 (targeting-override) | ★★★★ (kuşatma) | ★★★ | **3. dalga (Post-launch)** |

## 13.3 Dalga planı (fraksiyon-paritesi korunur)

| Dalga | Komutanlar | Gerekçe |
|---|---|---|
| **İlk dalga (koleksiyon açılışı, S1–S2)** | **Sythe (Ashen) + Vael (Iron)** | Fraksiyon başına 1; en ucuz; en bütçe-güvenli; en okunur; mevcut 2'nin **karşı-kutbu** (saldırı↔savunma, hız↔flank). Talent+koleksiyon çerçevesini **ucuza doğrular.** |
| **2. dalga** | **Korrash (Ashen) + Venn (Iron)** | Orta-maliyet; yeni eksen (feda-tempo, konum-ekonomi); altyapı oturduktan sonra. |
| **3. dalga** | **Vhirek (Ashen) + Orrin (Iron)** | En pahalı + en hassas-denge (bilgi-kanalı, objektif-tempo); reveal-altyapısı + telemetri olgunlaştıktan sonra. |

## 13.4 İLK implemente edilecek komutan: **Sythe** ("Kül Rüzgârı")

**Neden Sythe (tek bir komutanla başlanacaksa):**
1. **En düşük maliyet** — "Pathfinder" pasifi **Phase-2'de zaten var olan pozisyonel-mult sistemini** yeniden kullanır (yeni savaş-matematiği yok); "Ghoststep" yalnızca Movement/Terrain MoveMult ayarıdır. Yeni `StatusKind` veya targeting-override gerektirmez.
2. **En net bütçe-güvenliği** — pozisyonel **tavanı yükseltmez** (1.5/2.0 sabit), yalnızca *tutarlılaştırır* → ≤%15 ispatı en kolay; denge-riski en düşük.
3. **Çerçeveyi ucuza doğrular** — talent-sistemi + koleksiyon-UI + ranked-normalizasyon altyapısı, en basit komutanla test edilir → riski tek-seferlik altyapıya yayar, karmaşık komutana değil.
4. **Anında okunur kimlik** — "yandan gelen rüzgâr" WarCry'ın düz-hızından açıkça farklı; oyuncu farkı **ilk maçta** anlar.
5. **Ashen kimliğini derinleştirir** — fraksiyonun flank-zayıflığını-güce-çeviren doktrin-faseti; mevcut Warchief'i tamamlar.

**Hemen ardından: Vael** (ilk Iron Pact koleksiyon üyesi) → fraksiyon-paritesi + savunma-kutbu, hâlâ düşük-maliyet.

> **Karşı-görüş (kayıt için):** Eğer öncelik **Iron Pact derinliği** veya **savunma-meta'sı test etmek** ise, **Vael** önce gelebilir (neredeyse-eşit ucuz, en okunur kimlik). Sythe vs Vael, "hangi fraksiyona/playstyle'a önce yatırım" tercihidir — ikisi de güvenli ilk-adım.

## 13.5 Nihai uyum beyanı (authority rule)

Bu rapor **yalnızca tavsiyedir.** Şunları **yapmadı / yapamaz:** roadmap/kanon/decision-log değişikliği · özellik yetkilendirme · implementasyon · öncelik değişikliği. Şunları **yaptı:** keşfetti · analiz etti · tavsiye etti · değerlendirdi.

| Doğrulama | Sonuç |
|---|---|
| Roadmap/ADR/Decision-Log ihlali | **Yok** — Phase 7.5'i ön-tasarlar, değiştirmez |
| Güç-bütçesi (≤%15) | **Korundu** — 6 komutan ~%11–15, çok-kanal denetimi (Part 9) |
| Monetizasyon (no P2W) | **Korundu** — earnable + cosmetic-only + no power-currency |
| Yetkisiz mekanik | **İşaretlendi** — ⚙️ etiketli hook'lar **gelecek ADR gerektirir**; bugün kanon değil |
| Canon dosyaları değişti mi | **Hayır** — yalnızca `future/001-...` altına yazıldı |
| Yeni komutan kanona girdi mi | **Hayır** — hepsi advisory; Phase 7.5 trigger ateşlenmeden implemente edilmez |

**Sonraki adım (eğer/ne zaman Phase 7.5 trigger ateşlenirse):** Bu rapor, o günün **ADR taslağı + tasarım-tabanı** olarak kullanılabilir. Trigger (Decision Log §2: "MVP retention validated; commander bones stable") **henüz ateşlenmedi** → bugün hiçbir eylem yok. Aktif akış değişmeden devam eder: CI/CD · APK · Unity doğrulama · Phase 0–3 kapı borç-eritimi.

---

*Belge: Gelecek araştırma izi (advisory). Hiçbir kanon/roadmap/decision-log/öncelik değiştirilmedi. Tüm sayısal değerler PROVISIONAL/LSD-owned + telemetri-ayarlı. ⚙️ yeni-sim-hook önerileri Phase 7.5 ADR'sine tabidir. `future/001-commander-talent-system/`.*

