using System;

namespace Avalonia;

internal class CompositeDisposable : IDisposable
{
    private readonly IDisposable _disposable1;
    private readonly IDisposable _disposable2;

    public CompositeDisposable(IDisposable disposable1, IDisposable disposable2)
    {
        _disposable1 = disposable1;
        _disposable2 = disposable2;
    }

    public void Dispose()
    {
        _disposable1.Dispose();
        _disposable2.Dispose();
    }
}
