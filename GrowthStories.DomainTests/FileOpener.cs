

using System.IO;
using System;
using System.Threading.Tasks;
using Growthstories.Sync;

namespace Growthstories.Sync
{
    public sealed class PhotoHandler : IPhotoHandler
    {


        public Task<Stream> ReadPhoto(Photo photo)
        {

            return Task.FromResult((Stream)File.Open(photo.LocalFullPath, FileMode.Open));

        }

        public Task<Stream> WritePhoto(Photo photo)
        {

            return Task.FromResult((Stream)File.Open(photo.LocalFullPath, FileMode.Create));

        }

    }
}
