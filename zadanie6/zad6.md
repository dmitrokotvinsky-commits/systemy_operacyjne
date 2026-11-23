Wyniki testu (8 wątków czytających przez 10 s przy losowych zmianach mnożnika 1..2 i pauzach w trybie Release):
 Końcowy czas symulacji: `4 ms`
 Liczba odczytów `Teraz`: `139058138`
 Liczba błędów monotoniczności: `0`

Jak `ZegarSymulacji` zachowuje monotoniczność przy zmianie mnożnika?
  Zamiast mnożyć całkowity czas od startu programu przez bieżący mnożnik (co przy jego zmniejszeniu cofnęłoby zegar),
  klasa przechowuje dotychczas naliczony czas w polu akumulatora (`_akumulowanyCzasMs`) oraz znacznik ostatniej 
  aktualizacji (`_ostatniTick`). Przy każdym odczycie `Teraz`, zmianie mnożnika lub przełączeniu pauzy obliczany 
  jest wyłącznie przyrost czasu od `_ostatniTick`, mnożony przez dotychczasowy mnożnik i dodawany do akumulatora pod 
  blokadą `lock`. Dzięki temu zmiana mnożnika wpływa wyłącznie na przyszły czas, a wartość `Teraz` nigdy nie maleje.