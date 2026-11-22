using System;
using System.Diagnostics;
using System.Threading;

class Program
{
    const int LiczbaWatkow = 16;

    static volatile bool pauza = false;
    static volatile bool koniec = false;
    static readonly object pauzaLock = new object();

    static long czasWznowieniaTicks = 0;
    static long sumaOpoznienTicks = 0;
    static int liczbaObudzonych = 0;

    static void Main()
    {
        Console.WriteLine("Wersja | Czas CPU podczas 5s pauzy (ms) | Srednie opoznienie reakcji (ms)");

        TestujWersje("A: while(pauza) {}", 'A');
        TestujWersje("B: Thread.Sleep(1)", 'B');
        TestujWersje("C: Monitor.Wait", 'C');
    }

    static void TestujWersje(string nazwa, char tryb)
    {
        pauza = false;
        koniec = false;
        sumaOpoznienTicks = 0;
        liczbaObudzonych = 0;

        Thread[] watki = new Thread[LiczbaWatkow];
        for (int i = 0; i < LiczbaWatkow; i++)
        {
            watki[i] = new Thread(() => Pracownik(tryb));
            watki[i].Start();
        }

        Thread.Sleep(200); // Chwila normalnej pracy

        // 1. Włączamy pauzę
        if (tryb == 'C')
        {
            lock (pauzaLock) { pauza = true; }
        }
        else
        {
            pauza = true;
        }

        Thread.Sleep(100); // Czekamy, aż wszystkie wątki wejdą w stan pauzy

        // 2. Mierzymy zużycie CPU przez 5 sekund trwania pauzy
        Process proc = Process.GetCurrentProcess();
        TimeSpan cpuPrzed = proc.TotalProcessorTime;
        Thread.Sleep(5000);
        TimeSpan cpuPo = proc.TotalProcessorTime;
        double zuzycieCpuMs = (cpuPo - cpuPrzed).TotalMilliseconds;

        // 3. Wznawiamy pracę i mierzymy czas reakcji
        czasWznowieniaTicks = Stopwatch.GetTimestamp();
        if (tryb == 'C')
        {
            lock (pauzaLock)
            {
                pauza = false;
                Monitor.PulseAll(pauzaLock);
            }
        }
        else
        {
            pauza = false;
        }

        // Czekamy, aż wszystkie wątki odnotują wybudzenie
        while (Volatile.Read(ref liczbaObudzonych) < LiczbaWatkow)
            Thread.Sleep(1);

        double srednieOpoznienieMs = (double)sumaOpoznienTicks / LiczbaWatkow / Stopwatch.Frequency * 1000.0;

        koniec = true;
        for (int i = 0; i < LiczbaWatkow; i++)
            watki[i].Join();

        Console.WriteLine($"{nazwa,-22} | {zuzycieCpuMs,28:F2} | {srednieOpoznienieMs,31:F4}");
    }

    static void Pracownik(char tryb)
    {
        bool bylWPauzie = false;

        while (!koniec)
        {
            if (pauza)
            {
                bylWPauzie = true;

                if (tryb == 'A')
                {
                    while (pauza && !koniec) { } // Aktywne czekanie (busy-waiting)
                }
                else if (tryb == 'B')
                {
                    while (pauza && !koniec)
                        Thread.Sleep(1);
                }
                else if (tryb == 'C')
                {
                    lock (pauzaLock)
                    {
                        while (pauza && !koniec)
                            Monitor.Wait(pauzaLock);
                    }
                }
            }

            if (bylWPauzie)
            {
                long opoznienie = Stopwatch.GetTimestamp() - Volatile.Read(ref czasWznowieniaTicks);
                Interlocked.Add(ref sumaOpoznienTicks, opoznienie);
                Interlocked.Increment(ref liczbaObudzonych);
                bylWPauzie = false;
            }

            // Krótka symulacja pracy
            Thread.SpinWait(100);
        }
    }
}