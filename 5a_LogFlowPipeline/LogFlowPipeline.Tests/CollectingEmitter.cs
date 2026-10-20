namespace LogFlowPipeline.Tests;

using System.Collections.Generic;
using LogFlowPipeline; // Ana projedeki IEmitter'ı görebilmesi için

public class CollectingEmitter<T> : IEmitter<T>
{
    public List<T> Items { get; } = new List<T>();

    public void Emit(T item)
    {
        Items.Add(item);
    }
}