
using Windows.Storage.Streams;
using System.IO;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;

namespace Growthstories.UI.WindowsPhone
{
    public sealed class FileOpener : IFileOpener
    {


        public async Task<Stream> OpenPhoto(Photo photo)
        {
            var imgFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(ImagingExtensions.IMG_FOLDER, CreationCollisionOption.OpenIfExists);
            return await imgFolder.OpenStreamForReadAsync(photo.FileName);
        }

    }
}
