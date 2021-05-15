# Manuális kód átvizsgálás elvégzése az alkalmazás egy részére + Statikus analízis eszköz futtatása és jelzett hibák átnézése

Manuális kódátvizsgálás során nem fedeztünk fel hibát. A statikus analíz számos hibát felvetett, ezek egy részét nekiálltunk kijavítani. A legtöbb hiba abból adódott, hogy a megírt kód nem elég biztonságos és létezik rá jobb megoldás. Azonban vol olyan is, hogy a hiba nem bizonyult valósnak.

Eredmény képpen elmondható, hogy többségében véve olyan hibákat javítottunk ki, amik nem befolyásolják a program működését, csupán a kód biztonságossága szempontjából jelentősek. Ezeknek a hibáknak a kijavítása általában nem igényelt nagy munkát, de volt, hogy rontott az átláthatóságon.

Összefoglalva a hibák nem voltak jelentősek és volt olyan is ami megítélésünk alapján nem igényelt módosítást, csupán azt mondtuk, hogy ne kezelje hibaként.