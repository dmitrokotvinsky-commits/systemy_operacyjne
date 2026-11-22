using System;
using System.Diagnostics;
using System.Threading;

class Program
{
    static int licznik = 0;
    static readonly object blokada = new object();

    static void Main()
    {
        const int liczbaWatkow = 8;
        const int inkrementacje = 1_000_000;
        const int proby = 5;

        Console.WriteLine("Wersja | Sredni wynik | Sredni czas (ms)");

        UruchomTest("Bez synchronizacji", proby, liczbaWatkow, () =>
        {
            for (int i = 0; i < inkrementacje; i++)
                licznik++;
        });

        UruchomTest("Z lock", proby, liczbaWatkow, () =>
        {
            for (int i = 0; i < inkrementacje; i++)
            {
                lock (blokada)
                {
                    licznik++;
                }
            }
        });

        UruchomTest("Z Interlocked", proby, liczbaWatkow, () =>
        {
            for (int i = 0; i < inkrementacje; i++)
                Interlocked.Increment(ref licznik);
        });
    }

    static void UruchomTest(string nazwa, int proby, int liczbaWatkow, ThreadStart akcja)
    {
        long sumaWynikow = 0;
        double sumaCzasuMs = 0;

        for (int p = 0; p < proby; p++)
        {
            licznik = 0;
            Thread[] watki = new Thread[liczbaWatkow];

            Stopwatch sw = Stopwatch.StartNew();

            for (int i = 0; i < liczbaWatkow; i++)
            {
                watki[i] = new Thread(akcja);
                watki[i].Start();
            }

            for (int i = 0; i < liczbaWatkow; i++)
                watki[i].Join();

            sw.Stop();

            sumaWynikow += licznik;
            sumaCzasuMs += sw.Elapsed.TotalMilliseconds;
        }

        Console.WriteLine($"{nazwa,-20} | {sumaWynikow / proby,12} | {sumaCzasuMs / proby,14:F2}");
    }
}
