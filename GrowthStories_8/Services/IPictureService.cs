using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.WP8.Services
{
    public interface IPictureService
    {

        // Retrieve the image as a bytearray. This is stored in the PictureViewModel object and cleared
        // whenever the object goes out of scope. 
        Task<byte[]> GetImageBytesAsync(string path);

    }
}
