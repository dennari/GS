using Newtonsoft.Json;
using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

namespace Growthstories.Sync
{
    public enum PhotoOrientation
    {
        LANDSCAPE,
        PORTRAIT
    }

    public interface IPhoto : IEquatable<IPhoto>
    {

        uint Width { get; set; }
        uint Height { get; set; }
        ulong Size { get; set; }
        PhotoOrientation Orientation { get; }
        string Uri { get; }
    }

    public struct Photo : IPhoto
    {

        [JsonProperty]
        public uint Width { get; set; }
        [JsonProperty]
        public uint Height { get; set; }
        [JsonProperty]
        public ulong Size { get; set; }

        [JsonProperty]
        public string LocalFullPath { get; set; }


        [JsonProperty]
        public string LocalUri { get; set; }


        [JsonProperty]
        public string RemoteUri { get; set; }

        [JsonProperty]
        public string BlobKey { get; set; }

        [JsonProperty]
        public string FileName { get; set; }



        [JsonIgnore]
        public PhotoOrientation Orientation
        {
            get
            {
                return Width > Height ? PhotoOrientation.LANDSCAPE : PhotoOrientation.PORTRAIT;
            }
        }

        [JsonIgnore]
        public string Uri
        {
            get
            {
                string local = null;
#if NETFX_CORE // http://suchan.cz/?p=132
                local = LocalUri;
#else
                local = LocalFullPath;
#endif
                return local ?? RemoteUri;

            }
        }
        public static bool operator ==(Photo size1, Photo size2)
        {
            return size1.Equals(size2);
        }
        public static bool operator !=(Photo size1, Photo size2)
        {
            return !size1.Equals(size2);
        }

        public override string ToString()
        {
            return Uri;
        }

        public override int GetHashCode()
        {
            return Uri == null ? 0 : Uri.GetHashCode();
        }

        public override bool Equals(object other)
        {
            return Equals(other as IPhoto);
        }


        public bool Equals(IPhoto other)
        {
            if (other == null)
                return false;
            return this.Uri == other.Uri;
        }
    }

}
