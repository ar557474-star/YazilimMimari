using System;

// 1. Emitter: Veriyi bir sonraki adıma fırlatan (yayan) iletken.
public interface IEmitter<T>
{
    void Emit(T item);
}

// 2. Stage: Veriyi alıp işleyen ve Emitter ile bir sonraki adıma atan yapı.
public interface IStage<I, O>
{
    void Process(I input, IEmitter<O> output);
    void Open() { }  // Başlangıç ayarları gerekirse diye (C# 8.0+ destekler)
    void Close() { } // Kapanış ayarları gerekirse diye
}

// 3. Source: Verinin kaynağı (Örn: Dosyadan okuma işlemi).
public interface ISource<O>
{
    void Produce(IEmitter<O> output);
}

// 4. Sink: Verinin son durağı (Örn: Konsola yazdırma işlemi).
/*public interface ISink<I>
{
    void Consume(I item);
}*/
public interface ISink<T>
{
    void Open() { }    // Yaşam döngüsü için
    void Close() { }   // Yaşam döngüsü için
    void Consume(T item);
}