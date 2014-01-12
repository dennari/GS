using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using Growthstories.UI.ViewModel;

#if DEBUG
using MockIAPLib;
using Store = MockIAPLib;
#else
using Windows.ApplicationModel.Store;
#endif

namespace Growthstories.UI.WindowsPhone
{


    class GSIAP
    {

        public const string BASIC_PRODUCT_ID = "gsbasic";


        public static void PossiblySetupMockIAP()
        {
            #if DEBUG
            MockIAP.Init();

            MockIAP.RunInMockMode(true);
            MockIAP.SetListingInformation(1, "en-us", "Allows adding 4 additional plants your garden", "4.95", "4 More Plants");

            ProductListing p = new ProductListing
            {
                Name = "4 More Plants",
                Description = "Allows adding 4 additional plants to your garden",
                ImageUri = new Uri("/Assets/Icons/IAP.png", UriKind.Relative),
                ProductId = BASIC_PRODUCT_ID,
                ProductType = Windows.ApplicationModel.Store.ProductType.Durable,
                Keywords = new string[] {},
                FormattedPrice = "4.95",
                Tag = string.Empty
            };
            MockIAP.AddProductListing(BASIC_PRODUCT_ID, p);
            #endif
        }


        /**
         * Return true if user has payed for the basic GS IAP product
         */
        public static bool HasPayedBasicProduct()
        {
            foreach (var license in CurrentApp.LicenseInformation.ProductLicenses.Values)
            {
                if (license.ProductId.Equals(BASIC_PRODUCT_ID) && license.IsActive)
                {
                    return true;
                }
            }
            return false;
        }


        /*
         * Go shopping in the Windows Store for the basic GS IAP product 
         */
        public async static Task<bool> ShopForBasicProduct()
        {
            try {
                await CurrentApp.RequestProductPurchaseAsync(BASIC_PRODUCT_ID, false);

            } catch (Exception ex) {
                // thrown when user does not buy the product
                // ( navigates back etc. )
            }

            return HasPayedBasicProduct();
        }


    }
}
