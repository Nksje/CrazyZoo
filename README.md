# Crazy Zoo

## Maailm ja eesmärk
Humoorikas WPF-rakendus, kus ahv, pingviin ja lõvi elavad samas loomaaias,
aga käituvad erinevalt. Eesmärk on harjutada pärilust, liideseid ja polümorfismi.

## Käivitamine
1. `git clone https://github.com/Nksje/CrazyZoo.git`
2. Ava `CrazyZoo.slnx` Visual Studios (.NET 10)
3. Määra `CrazyZoo.Wpf` startup-projektiks ja vajuta F5.

## Klassihierarhia
- `Animal` (abstract) → `Monkey`, `Penguin`, `Lion`
- `IRepository<T>` / `Repository<T> where T : Animal` – tüübipõhine hoidla (Add, Remove, GetAll, Find)
- `AnimalSection<T>` ja `ZooWorld` – iga loomatüüp saab oma hoidla (`Repository<Monkey>`, `Repository<Penguin>`, `Repository<Lion>`)
- `ZooQueries` – LINQ-päringud
- `ISwimmable` – Monkey, Penguin; `IClimb` – Monkey; `IFly` – Penguin
- Monkey ja Penguin rakendavad kahte rolli.

## CrazyAction ja olekureeglid
- Energia ja nälg on 0..100, muuta saab ainult meetodite kaudu (private set).
- Monkey: varastab banaani (nälg ↓, energia ↓), vajab energiat ≥ 10.
- Penguin: libiseb kõhuli (energia ↓).
- Lion: ajab saba taga (vajab energiat ≥ 30, muidu olek ei muutu).

## Hoidlad, sündmused ja reaktsioonid
Iga loomatüüp elab oma hoidlas. Vale tüüpi looma lisamist takistab `where T : Animal` ja
tüübikontroll kompileerimisel, mitte UI nimevõrdlus.

Hoidla annab kolm sündmust (`AnimalEventArgs` sisaldab looma, kes tegevuse põhjustas):
- **Lisamine** (`ItemAdded`): sama hoidla senised loomad tervitavad uut looma. Uus loom ei tervita ennast.
- **Eemaldamine** (`ItemRemoved`): allesjäänud loomad jätavad eemaldatuga hüvasti. Eemaldatud loom lõpetab tellimise.
- **CrazyAction** (`ItemCrazyAction`): teised sama hoidla loomad reageerivad. Põhjustaja ei reageeri. Kui loom keeldus (olek ei muutunud), siis keegi ei reageeri.

Teise hoidla loomad ei reageeri. Iga reaktsioon kirjutab logisse rea ja muudab reageerija olekut
(`Mood` ja `ReactionCount`). Monkey, Penguin ja Lion reageerivad erinevalt (`override`).

## LINQ
Kolm päringut (vahekaardid UI-s), mis töötavad kõigi hoidlate loomadel:
- **Where**: otsing nime või liigi järgi.
- **OrderByDescending**: loomad energia järgi.
- **GroupBy**: loomade arv igas liigis.

## MVVM
- **Model/Core**: loomad, hoidlad, sündmused, reaktsioonid, LINQ (`CrazyZoo.Core`, ei sõltu WPF-ist).
- **ViewModel**: `MainViewModel` (valitud hoidla ja loom, loendid, logi, päringute tulemused, `ICommand`-käsud, `INotifyPropertyChanged`).
- **View**: `MainWindow.xaml` seotud binding'ute kaudu, kooditaguses pole äriloogikat.
- `RelayCommand.CanExecute` keelab nupu, kui looma pole valitud või sisend on vigane.

## Kontrollitud kasutusjuhud
1. Loomale tühja nime andmine → veateade, looma ei looda.
2. Söötmine tekstiga "abc" või -5 → veateade, olek ei muutu.
3. Lõvi CrazyAction väsinult → keeldub, energia ei muutu.
4. Lõvile "Ujuda" → "ei oska ujuda" (is-kontroll).
5. Loomade lisamine ja eemaldamine → list uueneb.
6. Samas hoidlas on A ja B, lisatakse C → A ja B tervitavad, C mitte.
7. C CrazyAction → A ja B reageerivad, C mitte.
8. B eemaldamine → A ja C jätavad hüvasti, B enam ei reageeri.
9. Teise hoidla loomade olek ja logi ei muutu punktides 6–8.
10. Väsinud Lõvi CrazyAction (keeldub) → teised loomad ei reageeri.

## Iseseisvalt õpitud UI elemendid
(`TabControl` on lisatud logi ja LINQ-tulemuste jaoks.)
Kasutasin **DataTemplate** (loomade ilus kuvamine listis ilma ToString'ita)
ja **ProgressBar** (energia ja nälja visuaalne näitamine, sobib olekule 0..100).

## Koostöö kaasüliõpilasega

Kaasüliõpilane: Egor Mikhailov (GitHub: <Egokio>)

### Minu projekt (kaasüliõpilane täiendas)
- Issue: <https://github.com/Nksje/CrazyZoo/issues/1>
- Pull request: <https://github.com/Nksje/CrazyZoo/pull/2>
- Ülesanne: lisada uus alamklass `Parrot : Animal` (IFly) koos CrazyAction'iga.

### Kaasüliõpilase projekt (mina täiendasin)
Repositoorium: https://github.com/Egokio/CarsWorld
- Issue: https://github.com/Egokio/CarsWorld/issues/1
- Pull request: https://github.com/Egokio/CarsWorld/pull/2
- Ülesanne: lisada uus alamklass `Ambulance : Car`, mis rakendab `IRefuelable`,
  override'ib `Drive()` ja `CrazyAction()` ning valideerib kütusekoguse `ActionResult` abil.
- Töö tehti harus `feature/ambulance`, muudatused esitati pull request'iga, `main`/`master` harusse otse ei commit'itud.

## Tehisintellekti kasutamine
Tööriist: Claude. Kasutasin seda Kodutöö 3 koodi kavandamiseks (hoidlad, sündmused,
LINQ, MVVM) ja README täiendamiseks. Enda kontrollid ja muudatused: <kirjuta, mida ise testisid ja muutsid>.
