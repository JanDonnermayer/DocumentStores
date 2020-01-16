using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace DocumentStores.Internal
{
    internal class ObservableMemoryStream : MemoryStream
    {
        private readonly ISubject<byte[]> subject =
            new Subject<byte[]>();

        public IObservable<byte[]> OnDispose() =>
            subject.AsObservable();

        protected override void Dispose(bool disposing) =>
            subject.OnNext(this.ToArray());
    }
}
