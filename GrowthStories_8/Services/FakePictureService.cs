using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Growthstories.PCL.Services
{
    public class FakePictureService
    {
        public ImageSource GetImageBytesAsync(string path)
        {
            return new BitmapImage(new Uri(path));
        }
    }
}
