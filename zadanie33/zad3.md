Wyniki testów (20 odcinków, 10 samochodów):
Tryb `ręka-za-ręką` (najpierw `Monitor.Enter(nastepny)`, potem `Monitor.Exit(obecny)`): liczba naruszeń = `0`
Tryb błędny (najpierw `Monitor.Exit(obecny)`, potem `Monitor.Enter(nastepny)`): liczba naruszeń = `25`

Jak wątek kontrolny musi czytać stan, aby nie zgłaszać fałszywych naruszeń?
  Wątek kontrolny musi pobierać pozycje wszystkich samochodów atomowo jako spójną migawkę pod wspólną blokadą (`lock (stanLock)`). 
  Bez tej blokady mógłby odczytać tablicę w trakcie aktualizacji pozycji przez inne wątki i zgłosić fałszywe wyprzedzenie.

Co się stanie, jeśli samochód najpierw zwolni bieżący odcinek, a dopiero potem zajmie następny?
  Pomiędzy wywołaniem `Monitor.Exit` a `Monitor.Enter` wątek nie posiada żadnej blokady. 
  W tym oknie czasowym szybszy samochód jadący z tyłu może zająć kolejny odcinek jako pierwszy, co prowadzi
  do wyprzedzenia i złamania niezmiennika kolejności.