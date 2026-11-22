Wersja | Sredni wynik | Sredni czas (ms)
Bez synchronizacji   |      2262961 |          11,21
Z lock               |      8000000 |         277,04
Z Interlocked        |      8000000 |         156,84

Dlaczego `licznik++` nie jest operacją atomową (kod IL)?
  Instrukcja `licznik++` kompiluje się do sekwencji odczytu, dodania i zapisu w pamięci:
  ```il
  ldsfld int32 Program::licznik   // 1. Odczyt wartości z pamięci
  ldc.i4.1                       // 2. Załadowanie stałej 1
  add                            // 3. Dodanie 1
  stsfld int32 Program::licznik   // 4. Zapis wyniku z powrotem do pamięci

  Dlaczego lock jest wolniejszy od Interlocked, ale w projekcie będzie potrzebny częściej?
  Interlocked.Increment korzysta z pojedynczej atomowej instrukcji procesora (LOCK XADD), natomiast lock przy rywalizacji 
  wielu wątków wiąże się z kosztem zarządzania blokadą i przełączaniem kontekstu przez system operacyjny.
  W projekcie lock będzie potrzebny częściej, ponieważ pozwala atomowo chronić całe bloki kodu i spójność wielu zmiennych 
  jednocześnie (np. stan przejazdu, szlabanu i kolejki aut), podczas gdy Interlocked obsługuje tylko pojedyncze zmienne proste.