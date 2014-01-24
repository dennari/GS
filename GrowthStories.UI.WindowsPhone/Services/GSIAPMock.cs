using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using Growthstories.UI.ViewModel;
using MockIAPLib;
using Growthstories.Core;


namespace Growthstories.UI.WindowsPhone
{


    public class GSIAPMock : IIAPService
    {

        public const string BASIC_PRODUCT_ID = "gsbasic";


        public GSIAPMock()
        {
            MockIAP.Init();
            MockIAP.RunInMockMode(true);
            MockIAP.SetListingInformation(1, "en-us", "Allows adding 4 additional plants your garden", "4.75", "4 More Plants");

            ProductListing p = new ProductListing
            {
                Name = "4 More Plants",
                Description = "Allows adding 4 additional plants to your garden",
                ImageUri = new Uri("/Assets/Icons/IAP.png", UriKind.Relative),
                ProductId = BASIC_PRODUCT_ID,
                ProductType = Windows.ApplicationModel.Store.ProductType.Durable,
                Keywords = new string[] { },
                FormattedPrice = "4.90",
                Tag = string.Empty
            };
            MockIAP.AddProductListing(BASIC_PRODUCT_ID, p);
        }


        public async Task<string> FormattedPrice()
        {
            try
            {
                var list = new string[] { BASIC_PRODUCT_ID };
                var listingInfo = await CurrentApp.LoadListingInformationByProductIdsAsync(list);
                var productListing = listingInfo.ProductListings[BASIC_PRODUCT_ID];

                return productListing.FormattedPrice;
            }
            catch
            {
                // something wrong with listing informations
                return null;
            }
        }


        /**
         * Return true if user has payed for the basic GS IAP product
         */
        public bool HasPaidBasicProduct()
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
        public async Task<bool> ShopForBasicProduct()
        {
            try
            {
                await CurrentApp.RequestProductPurchaseAsync(BASIC_PRODUCT_ID, false);

            }
            catch (Exception)
            {
                // thrown when user does not buy the product
                // ( navigates back etc. )
            }

            return HasPaidBasicProduct();
        }


    }
}
