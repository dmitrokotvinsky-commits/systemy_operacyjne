using System;
using System.Threading;

class Program
{
    static readonly string[] nazwyZasobow = { "N", "E", "S", "W" };
    static readonly object[] zasoby = { new object(), new object(), new object(), new object() };

    // Dane diagnostyczne dla wątku-strażnika (watchdog)
    static readonly int[] trzyma = { -1, -1, -1, -1 };
    static readonly int[] czekaNa = { -1, -1, -1, -1 };
    static readonly object diagLock = new object();
    static long ostatniPostepTick = Environment.TickCount64;

    static volatile bool koniec = false;

    static void Main()
    {
        // Zmień tryb:
        // 1 = Deadlock (wykryty przez strażnika po 2s)
        // 2 = Hierarchia blokad (brak zakleszczenia)
        // 3 = Monitor.TryEnter z limitem czasu i wycofaniem
        int tryb = 3;

        Thread straznik = new Thread(WatekStraznik);
        straznik.IsBackground = true;
        straznik.Start();

        Thread[] auta = new Thread[4];
        for (int i = 0; i < 4; i++)
        {
            int id = i;
            // Auto i wjeżdża z kierunku i, a skręcając w lewo potrzebuje też zasobu (i + 1) % 4
            int z1 = id;
            int z2 = (id + 1) % 4;

            auta[i] = new Thread(() =>
            {
                if (tryb == 1) SkretDeadlock(id, z1, z2);
                else if (tryb == 2) SkretHierarchia(id, z1, z2);
                else if (tryb == 3) SkretTryEnter(id, z1, z2);
            });
            auta[i].Start();
        }

        for (int i = 0; i < 4; i++)
            auta[i].Join();

        koniec = true;
        Console.WriteLine("Wszystkie samochody przejechały skrzyżowanie!");
    }

    // TRYB 1: Klasyczny deadlock
    static void SkretDeadlock(int id, int z1, int z2)
    {
        UstawStan(id, -1, z1);
        lock (zasoby[z1])
        {
            UstawStan(id, z1, -1);
            Thread.Sleep(50); // Czas na to, by pozostałe wątki zajęły swoje pierwsze zasoby

            UstawStan(id, z1, z2);
            lock (zasoby[z2])
            {
                UstawStan(id, -1, -1);
                Console.WriteLine($"Auto-{id} ({nazwyZasobow[z1]}->{nazwyZasobow[z2]}) przejechało.");
            }
        }
    }

    // TRYB 2: Globalna kolejność blokad (zawsze najpierw zasób o mniejszym indeksie)
    static void SkretHierarchia(int id, int z1, int z2)
    {
        int pierwszy = Math.Min(z1, z2);
        int drugi = Math.Max(z1, z2);

        lock (zasoby[pierwszy])
        {
            Thread.Sleep(50);
            lock (zasoby[drugi])
            {
                Interlocked.Exchange(ref ostatniPostepTick, Environment.TickCount64);
                Console.WriteLine($"Auto-{id} ({nazwyZasobow[z1]}->{nazwyZasobow[z2]}) przejechało.");
            }
        }
    }

    // TRYB 3: Monitor.TryEnter z wycofaniem i losowym opóźnieniem (unikamy livelock)
    static void SkretTryEnter(int id, int z1, int z2)
    {
        Random rnd = new Random(id * 100 + Environment.TickCount);

        while (true)
        {
            bool mamZ1 = false;
            bool mamZ2 = false;

            try
            {
                Monitor.TryEnter(zasoby[z1], 50, ref mamZ1);
                if (mamZ1)
                {
                    Thread.Sleep(50);
                    Monitor.TryEnter(zasoby[z2], 50, ref mamZ2);
                    if (mamZ2)
                    {
                        Interlocked.Exchange(ref ostatniPostepTick, Environment.TickCount64);
                        Console.WriteLine($"Auto-{id} ({nazwyZasobow[z1]}->{nazwyZasobow[z2]}) przejechało.");
                        return;
                    }
                }
            }
            finally
            {
                if (mamZ2) Monitor.Exit(zasoby[z2]);
                if (mamZ1) Monitor.Exit(zasoby[z1]);
            }

            // Losowy backoff zmniejsza ryzyko livelocku
            Thread.Sleep(rnd.Next(10, 40));
        }
    }

    static void UstawStan(int id, int trzymany, int oczekiwany)
    {
        lock (diagLock)
        {
            trzyma[id] = trzymany;
            czekaNa[id] = oczekiwany;
            ostatniPostepTick = Environment.TickCount64;
        }
    }

    static void WatekStraznik()
    {
        while (!koniec)
        {
            Thread.Sleep(200);
            lock (diagLock)
            {
                if (Environment.TickCount64 - ostatniPostepTick >= 2000)
                {
                    Console.WriteLine("\n[STRAŻNIK] Wykryto zakleszczenie (brak postępu przez 2s)!");
                    for (int i = 0; i < 4; i++)
                    {
                        string t = trzyma[i] != -1 ? nazwyZasobow[trzyma[i]] : "brak";
                        string c = czekaNa[i] != -1 ? nazwyZasobow[czekaNa[i]] : "brak";
                        Console.WriteLine($"  Auto-{i}: trzyma [{t}], czeka na [{c}]");
                    }
                    Environment.Exit(1); // Kończymy zawieszony proces po wypisaniu diagnostyki
                }
            }
        }
    }
}