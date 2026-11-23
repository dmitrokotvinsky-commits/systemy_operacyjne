using System;
using System.Diagnostics;
using System.Threading;

public sealed class ZegarSymulacji
{
    private readonly object _lock = new object();
    private long _akumulowanyCzasMs = 0;
    private long _ostatniTick = Stopwatch.GetTimestamp();
    private double _mnoznik = 1.0;
    private bool _pauza = false;

    public long Teraz
    {
        get
        {
            lock (_lock)
            {
                Aktualizuj();
                return _akumulowanyCzasMs;
            }
        }
    }

    public double Mnoznik
    {
        get
        {
            lock (_lock) return _mnoznik;
        }
        set
        {
            if (value < 1.0 || value > 2.0)
                throw new ArgumentOutOfRangeException(nameof(value), "Mnożnik musi być w zakresie 1..2");

            lock (_lock)
            {
                Aktualizuj(); // Zapisujemy czas wypracowany na starym mnożniku
                _mnoznik = value;
            }
        }
    }

    public void Pauza()
    {
        lock (_lock)
        {
            if (!_pauza)
            {
                Aktualizuj(); // Zapisujemy czas przed zatrzymaniem
                _pauza = true;
            }
        }
    }

    public void Wznow()
    {
        lock (_lock)
        {
            if (_pauza)
            {
                _ostatniTick = Stopwatch.GetTimestamp(); // Resetujemy punkt odniesienia bez dodawania czasu pauzy
                _pauza = false;
            }
        }
    }

    private void Aktualizuj()
    {
        long terazTick = Stopwatch.GetTimestamp();
        if (!_pauza)
        {
            long deltaTicks = terazTick - _ostatniTick;
            double deltaMs = (double)deltaTicks / Stopwatch.Frequency * 1000.0;
            _akumulowanyCzasMs += (long)(deltaMs * _mnoznik);
        }
        _ostatniTick = terazTick;
    }
}

class Program
{
    static void Main()
    {
        ZegarSymulacji zegar = new ZegarSymulacji();
        bool koniec = false;
        int bledyMonotonicznosci = 0;
        long liczbaOdczytow = 0;

        // 8 wątków przez 10 s odczytuje Teraz i sprawdza monotoniczność
        Thread[] czytelnicy = new Thread[8];
        for (int i = 0; i < 8; i++)
        {
            czytelnicy[i] = new Thread(() =>
            {
                long poprzedniCzas = 0;
                while (!Volatile.Read(ref koniec))
                {
                    long aktualnyCzas = zegar.Teraz;
                    if (aktualnyCzas < poprzedniCzas)
                    {
                        Interlocked.Increment(ref bledyMonotonicznosci);
                    }
                    poprzedniCzas = aktualnyCzas;
                    Interlocked.Increment(ref liczbaOdczytow);
                }
            });
            czytelnicy[i].Start();
        }

        // Wątek główny przez 10 s co 300 ms losowo zmienia mnożnik i pauzuje
        Random rnd = new Random();
        Stopwatch testSw = Stopwatch.StartNew();
        bool wPauzie = false;

        while (testSw.ElapsedMilliseconds < 10_000)
        {
            Thread.Sleep(300);

            if (rnd.NextDouble() < 0.3) // 30% szans na przełączenie pauzy
            {
                if (!wPauzie)
                {
                    zegar.Pauza();
                    wPauzie = true;
                }
                else
                {
                    zegar.Wznow();
                    wPauzie = false;
                }
            }
            else
            {
                double nowyMnoznik = 1.0 + rnd.NextDouble(); // Losowo 1.0 .. 2.0
                zegar.Mnoznik = nowyMnoznik;
            }
        }

        koniec = true;
        for (int i = 0; i < 8; i++)
            czytelnicy[i].Join();

        Console.WriteLine($"Końcowy czas symulacji: {zegar.Teraz} ms");
        Console.WriteLine($"Liczba odczytów: {liczbaOdczytow}");
        Console.WriteLine($"Błędy monotoniczności: {bledyMonotonicznosci}");
    }
}