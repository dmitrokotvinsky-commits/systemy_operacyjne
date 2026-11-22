Kolejność wykonywania wątków jest niedeterministyczna, ponieważ zarządza nią planista systemu operacyjnego. 
Wątki pierwszoplanowe (domyślne) z użyciem Join() gwarantują poprawne dokończenie wszystkich zadań przed zamknięciem programu.
Ustawienie IsBackground = true bez Join() powoduje natychmiastowe zakończenie procesu wraz z wątkiem 
Main i przerwanie pracy wątków w tle, co w projekcie prowadziłoby do utraty spójności danych.


Pomiary czasu wykonania dla N = 8 (5 uruchomień w trybie Release):
1. Uruchomienie 1: 104 ms
2. Uruchomienie 2: 99 ms
3. Uruchomienie 3: 105 ms
4. Uruchomienie 4: 100 ms
5. Uruchomienie 5: 90 ms