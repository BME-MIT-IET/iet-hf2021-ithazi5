# Nem-funkcionális jellemzők vizsgálata

## Teljesítménytesztelés

Különböző mennyiségű triplák létrehozásának lefutási, illetve a szepmuveszeti.n3 fájl beolvasásának ideje lett lemérve.

## Stresszteszt
A teszt célja, az volt, hogy megállapítsuk mennyire erőforrás igényes az adatbázis betöltése, maximum hány szálon tudjuk a gépen betölteni, hányan tudnak így konkuresnsen hozzáférni. A tesztek során a cpu kihasználtságát és a memória kihasználtságát néztük meg, különböző mennyiségű egyszerre elindított betöltéssel.
## Biztonság tesztelése

## Használhatóság tesztelése

A teszt konkrét célja egy RDF adatbázis kiírása n3 fájlba, és egy SPARQL lekérdezés megírása volt, de akár ide vehetnénk a triplák létrehozását, és egy adatbázis beolvasását is, amit már korábban sikeresen csináltunk, és nem volt velük különösebb probléma, szóval azok használhatósága kifogástalan. RDF adatbázis kiíratásával sem volt probléma, ellenben egy SPARQL lekérdezés megírása az szerintünk túl sok időbe került.
