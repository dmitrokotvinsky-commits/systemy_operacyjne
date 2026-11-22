Obserwacje dla 3 trybów działania (skrzyżowanie 4-wlotowe N, E, S, W):
Tryb 1 (Deadlock): wszystkie 4 samochody zajmują swój pierwszy zasób i czekają na kolejny w cyklu (`N -> E -> S -> W -> N`). 
Po 2 sekundach braku postępu wątek-strażnik wykrywa zakleszczenie i wypisuje stan każdego wątku.
Tryb 2 (Hierarchia blokad): wszystkie 4 samochody bezpiecznie przejeżdżają skrzyżowanie bez zakleszczenia.
Tryb 3 (`Monitor.TryEnter` z limitem czasu i wycofaniem): wszystkie 4 samochody przejeżdżają skrzyżowanie dzięki 
zwalnianiu pierwszego zasobu przy niepowodzeniu zajęcia drugiego.

Który warunek Coffmana łamie globalna kolejność blokad?
  Globalna numeracja zasobów (`N=0, E=1, S=2, W=3`) i wymuszenie zajmowania ich zawsze w kolejności rosnącej łamie 4. 
  warunek Coffmana – cykliczne oczekiwanie (Circular Wait). Wątek trzymający zasób o wyższym indeksie nigdy nie będzie
  czekał na zasób o niższym indeksie, co uniemożliwia powstanie cyklu.

Jaki nowy problem może pojawić się przy `Monitor.TryEnter` i jak go ograniczyć?
  Może pojawić się livelock (aktywne zakleszczenie) – sytuacja, w której wszystkie wątki synchronicznie zajmują swój
  pierwszy zasób, nie mogą zająć drugiego, po upływie limitu czasu jednocześnie się wycofują i powtarzają ten sam schemat w nieskończoność. 
  Problem ogranicza się poprzez dodanie losowego czasu oczekiwania (random backoff) przed ponowną próbą zajęcia zasobów.