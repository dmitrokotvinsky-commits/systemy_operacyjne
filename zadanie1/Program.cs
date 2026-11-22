using System;
using System.Diagnostics;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        int n = args.Length > 0 ? int.Parse(args[0]) : 8; // Pobieramy N z wiersza poleceń (lub domyślnie 8)
        Thread[] watki = new Thread[n];

        Stopwatch sw = Stopwatch.StartNew();

        for (int i = 0; i < n; i++)
        {
            watki[i] = new Thread(() =>
            {
                for (int krok = 1; krok <= 5; krok++)
                {
                    Console.WriteLine($"{Thread.CurrentThread.Name} (ID: {Thread.CurrentThread.ManagedThreadId}) - krok {krok}");
                    Thread.Sleep(10);
                }
            });

            watki[i].Name = $"Auto-{i + 1:D2}"; // Auto-01, Auto-02...
            watki[i].Start();
        }

        // Główny wątek czeka na wszystkie utworzone wątki
        for (int i = 0; i < n; i++)
        {
            watki[i].Join();
        }

        sw.Stop();
        Console.WriteLine($"Łączny czas: {sw.ElapsedMilliseconds} ms");
    }
}