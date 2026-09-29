using System;

class Program
{
    static void Main(string[] args)
    {
        // Komut satırından dosya adı gelirse onu al, gelmezse varsayılan dosyayı kullan
        string filePath = args.Length > 0 ? args[0] : "data/access-small.log";

        // 1. Bileşenleri oluştur
        var source = new FileLineSource(filePath);
        var sink = new ConsoleSink();

        // 2. Pipeline'ı monte et
        var pipeline = new Pipeline(source, sink);

        // 3. Çalıştır
        pipeline.Run();
    }
}