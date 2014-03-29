using System;
using System.Collections.Generic;
using System.Windows;
using Size = Growthstories.Core.Size;
namespace Growthstories.UI.WindowsPhone.Services
{
    public enum Resolutions { WVGA, WXGA, HD };

    public static class ResolutionHelper
    {
        private static bool IsWvga
        {
            get
            {
                return Application.Current.Host.Content.ScaleFactor == 100;
            }
        }

        private static bool IsWxga
        {
            get
            {
                return Application.Current.Host.Content.ScaleFactor == 160;
            }
        }

        private static bool IsHD
        {
            get
            {
                return Application.Current.Host.Content.ScaleFactor == 150;
            }
        }

        public static Resolutions CurrentResolution
        {
            get
            {
                if (IsWvga) return Resolutions.WVGA;
                else if (IsWxga) return Resolutions.WXGA;
                else if (IsHD) return Resolutions.HD;

                // is this not fucking dangerous?
                //else throw new InvalidOperationException("Unknown resolution");

                else return Resolutions.HD;
            }
        }

        public static Size CurrentResolutionDimensions
        {
            get
            {
                return ResolutionDimensions[CurrentResolution];
            }
        }

        public static Dictionary<Resolutions, Size> ResolutionDimensions = new Dictionary<Resolutions, Size>()
        {
            {Resolutions.WVGA,new Size(480,800)},
            {Resolutions.WXGA,new Size(768,1280)},
            {Resolutions.HD,new Size(720,1280)}
        };

    }
}
