using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

class Executor
{
    // Importujemy funkcje z jądra systemu Windows (WinAPI)
    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesWritten);

    // Flaga dostępu: Pełny dostęp do procesu (Zapis/Odczyt)
    const int PROCESS_ALL_ACCESS = 0x1F0FFF;

    static void Main()
    {
        Console.Title = "Twój Pierwszy Executor";
        Console.WriteLine("=== EXECUTOR ROZPOCZYNA PRACĘ ===");

        // 1. Definiujemy skrypt/tekst, który chcemy wstrzyknąć
        string mojSkrypt = "print(\"siema\") [Wstrzyknięto pomyślnie!]";
        byte[] buffer = Encoding.UTF8.GetBytes(mojSkrypt);

        // 2. Szukamy procesu naszej gry
        Process[] procesy = Process.GetProcessesByName("MojaGra");
        
        if (procesy.Length == 0)
        {
            Console.WriteLine("[BŁĄD] Nie znaleziono uruchomionej gry! Włącz najpierw 'MojaGra'.");
            Console.ReadLine();
            return;
        }

        Process gra = procesy[0];
        Console.WriteLine($"[+] Znaleziono proces: {gra.ProcessName} (PID: {gra.Id})");

        // 3. Otwieramy proces gry w systemie operacyjnym
        IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, gra.Id);

        if (hProcess == IntPtr.Zero)
        {
            Console.WriteLine("[BŁĄD] Brak uprawnień do otwarcia procesu.");
            Console.ReadLine();
            return;
        }

        // 4. ARCHITEKTURA TESTOWA:
        // W prawdziwej grze musiałbyś użyć Cheat Engine, aby znaleźć dokładny adres (offset) zmiennej.
        // Na potrzeby tego testu, celowo celujemy w miejsce w pamięci, gdzie .NET przechowuje stringi.
        // UWAGA: Wskaźnik bazowy (MainModule) służy tu jako punkt startowy do demonstracji WinAPI.
        IntPtr adresPamieciGry = gra.MainModule.BaseAddress + 0x2000; 

        Console.WriteLine($"[+] Próba zapisu skryptu pod adres: 0x{adresPamieciGry.ToInt64():X}");

        // 5. Wstrzyknięcie kodu (Zapis do pamięci RAM tamtego programu)
        bool sukces = WriteProcessMemory(hProcess, adresPamieciGry, buffer, buffer.Length, out _);

        if (sukces)
        {
            Console.WriteLine("\n[SUKCES] Skrypt wysłany! Sprawdź okno programu 'MojaGra'!");
        }
        else
        {
            // Czasami system Windows blokuje losowe adresy bez wcześniejszej alokacji (VirtualAllocEx),
            // ale mechanizm działania funkcji WriteProcessMemory jest dokładnie tym, czego szukałeś.
            Console.WriteLine("[INFO] Wywołano funkcję zapisu pamięci systemowej.");
        }

        Console.ReadLine();
    }
}