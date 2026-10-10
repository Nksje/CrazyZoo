# Crazy Zoo

## Maailm ja eesmärk
Humoorikas WPF-rakendus, kus ahv, pingviin ja lõvi elavad samas loomaaias,
aga käituvad erinevalt. Eesmärk on harjutada pärilust, liideseid ja polümorfismi.

## Käivitamine
1. `git clone <repo-link>`
2. Ava `CrazyZoo.sln` Visual Studios (.NET 10)
3. Määra `CrazyZoo.Wpf` startup-projektiks ja vajuta F5.

## Klassihierarhia
- `Animal` (abstract) → `Monkey`, `Penguin`, `Lion`
- `ISwimmable` – Monkey, Penguin; `IClimb` – Monkey; `IFly` – Penguin
- Monkey ja Penguin rakendavad kahte rolli.

## CrazyAction ja olekureeglid
- Energia ja nälg on 0..100, muuta saab ainult meetodite kaudu (private set).
- Monkey: varastab banaani (nälg ↓, energia ↓), vajab energiat ≥ 10.
- Penguin: libiseb kõhuli (energia ↓).
- Lion: ajab saba taga (vajab energiat ≥ 30, muidu olek ei muutu).

## Kontrollitud kasutusjuhud
1. Loomale tühja nime andmine → veateade, looma ei looda.
2. Söötmine tekstiga "abc" või -5 → veateade, olek ei muutu.
3. Lõvi CrazyAction väsinult → keeldub, energia ei muutu.
4. Lõvile "Ujuda" → "ei oska ujuda" (is-kontroll).
5. Loomade lisamine ja eemaldamine → list uueneb.

## Iseseisvalt õpitud UI elemendid
Kasutasin **DataTemplate** (loomade ilus kuvamine listis ilma ToString'ita)
ja **ProgressBar** (energia ja nälja visuaalne näitamine, sobib olekule 0..100).
