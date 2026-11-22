Wersja | Czas CPU podczas 5s pauzy (ms) | Srednie opoznienie reakcji (ms)
A: while(pauza) {}     |                     37156,25 |                         15,0256
B: Thread.Sleep(1)     |                        15,62 |                         30,1905
C: Monitor.Wait        |                         0,00 |                         21,1388

Co się dzieje w wersji A w trybie Release po usunięciu `volatile`?
  Wątki nie zauważają wyłączenia pauzy i zawieszają się w nieskończonej pętli. 
  Kompilator JIT w trybie Release optymalizuje pętlę `while (pauza) {}`, odczytując wartość 
  zmiennej `pauza` z pamięci tylko raz przed pętlą i buforując ją w rejestrze procesora 
  (zamienia kod na `if (pauza) while (true) {}`).

Co może się stać w wersji C, gdy `Monitor.Wait` jest w `if` zamiast `while`?
  1. Fałszywe wybudzenia (spurious wakeups):** system operacyjny może wybudzić wątek bez wywołania `Pulse`/`PulseAll`. 
	 Przy użyciu `if` wątek nie sprawdzi ponownie warunku i ruszy dalej mimo aktywnej pauzy.
  2. Ponowna pauza przed przejęciem blokady:** jeśli wątek główny wznowi pracę (`PulseAll`), 
	ale zdąży ponownie włączyć pauzę zanim obudzony wątek odzyska blokadę `lock`, instrukcja `if` nie
	zweryfikuje aktualnego stanu flagi `pauza`.

