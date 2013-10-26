

using System.IO;
using System;
using System.Threading.Tasks;
using Growthstories.Sync;

namespace Growthstories.DomainTests
{
    public sealed class FileOpener : IFileOpener
    {


        public Task<Stream> OpenPhoto(Photo photo)
        {

            return Task.FromResult((Stream)File.Open(photo.LocalFullPath, FileMode.Open));

        }

    }
}
