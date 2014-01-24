using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using Growthstories.UI.ViewModel;
using Windows.ApplicationModel.Store;
using Growthstories.Core;

namespace Growthstories.UI.WindowsPhone
{


    public class GSIAP : IIAPService
    {

        public const string BASIC_PRODUCT_ID = "gsbasic";


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
