using System;
using System.Threading;

class Program
{
    const int LiczbaOdcinkow = 20;
    const int LiczbaAut = 10;

    static readonly object[] odcinki = new object[LiczbaOdcinkow];
    static readonly int[] pozycje = new int[LiczbaAut]; // -1 = przed drogą, 0..19 = odcinek, 20 = koniec
    static readonly object stanLock = new object();

    static volatile bool symulacjaTrwa = true;
    static int naruszenia = 0;

    static void Main()
    {
        for (int i = 0; i < LiczbaOdcinkow; i++)
            odcinki[i] = new object();

        for (int i = 0; i < LiczbaAut; i++)
            pozycje[i] = -1;

        // Zmień na false, aby przetestować drugą część zadania (najpierw zwolnienie, potem zajęcie)
        bool rekaZaReka = false;

        Thread kontroler = new Thread(WatekKontrolny);
        kontroler.Start();

        Thread[] auta = new Thread[LiczbaAut];
        for (int i = 0; i < LiczbaAut; i++)
        {
            int id = i;
            auta[i] = new Thread(() => Jedz(id, rekaZaReka));
            auta[i].Start();
            Thread.Sleep(20); // Auta startują co 20 ms
        }

        for (int i = 0; i < LiczbaAut; i++)
            auta[i].Join();

        symulacjaTrwa = false;
        kontroler.Join();

        Console.WriteLine($"Tryb ręka-za-ręką: {rekaZaReka}");
        Console.WriteLine($"Liczba naruszeń (wyprzedzeń): {naruszenia}");
    }

    static void Jedz(int id, bool rekaZaReka)
    {
        Random rnd = new Random(id * 100 + Environment.TickCount);
        int obecny = -1;

        try
        {
            for (int nastepny = 0; nastepny < LiczbaOdcinkow; nastepny++)
            {
                if (rekaZaReka)
                {
                    // 1. Najpierw zajmujemy następny odcinek
                    Monitor.Enter(odcinki[nastepny]);

                    lock (stanLock)
                        pozycje[id] = nastepny;

                    // 2. Dopiero potem zwalniamy poprzedni
                    if (obecny != -1)
                        Monitor.Exit(odcinki[obecny]);

                    obecny = nastepny;
                }
                else
                {
                    // Błędna kolejność: najpierw zwalniamy obecny odcinek
                    if (obecny != -1)
                    {
                        Monitor.Exit(odcinki[obecny]);
                        obecny = -1;
                    }

                    // W tym momencie inne auto może nas wyprzedzić!
                    Monitor.Enter(odcinki[nastepny]);
                    obecny = nastepny;

                    lock (stanLock)
                        pozycje[id] = nastepny;
                }

                // Symulacja przejazdu przez odcinek (5-30 ms)
                Thread.Sleep(rnd.Next(5, 31));
            }
        }
        finally
        {
            // Zwalniamy ostatni trzymany odcinek po zjechaniu z drogi
            if (obecny != -1)
                Monitor.Exit(odcinki[obecny]);

            lock (stanLock)
                pozycje[id] = LiczbaOdcinkow;
        }
    }

    static void WatekKontrolny()
    {
        while (symulacjaTrwa)
        {
            lock (stanLock)
            {
                // Auto i wystartowało wcześniej niż i+1, więc nie może być dalej w tyle (chyba że jeszcze nie wjechało: -1)
                for (int i = 0; i < LiczbaAut - 1; i++)
                {
                    int p1 = pozycje[i];
                    int p2 = pozycje[i + 1];

                    if (p1 != -1 && p2 != -1 && p1 < LiczbaOdcinkow && p2 > p1)
                    {
                        naruszenia++;
                    }
                }
            }
            Thread.Sleep(10);
        }
    }
}