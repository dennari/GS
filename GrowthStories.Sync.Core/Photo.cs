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

    public enum DimensionsType
    {
        PHYSICAL = 0,
        LOGICAL = 1
    }

    public interface IPhoto : IEquatable<IPhoto>
    {

        uint Width { get; set; }
        uint Height { get; set; }
        ulong Size { get; set; }
        PhotoOrientation Orientation { get; }
        DimensionsType DimensionsType { get; }
        string Uri { get; }

    }


    public sealed class Photo : IPhoto
    {

        [JsonProperty(PropertyName = "width", Required = Required.Default)]
        public uint Width { get; set; }
        [JsonProperty(PropertyName = "height", Required = Required.Default)]
        public uint Height { get; set; }

        [JsonProperty]
        public DimensionsType DimensionsType { get; set; }

        [JsonProperty]
        public ulong Size { get; set; }

        [JsonProperty]
        public string LocalFullPath { get; set; }


        [JsonProperty]
        public string LocalUri { get; set; }


        [JsonProperty(PropertyName = "servingUrl", Required = Required.Default)]
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
        public static bool operator ==(Photo a, Photo b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }
            return a.Equals(b);
        }
        public static bool operator !=(Photo a, Photo b)
        {
            return !(a == b);
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
