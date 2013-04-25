// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IServiceManager.cs" company="saramgsilva">
//   Copyright (c) 2012 saramgsilva. All rights reserved.
// </copyright>
// <summary>
//   Defines the service manager interface
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Growthstories.WP8.Services
{
    using Growthstories.WP8.Domain.Entities;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the service manager interface
    /// </summary>
    public interface IPlantDataService
    {
        /// <summary>
        /// Loads the new dvd movies async.
        /// </summary>
        /// <returns>
        /// Represents an asynchronous operation that returns a IEnumerable.
        /// </returns>
        Task<IList<PlantData>> LoadPlantDataAsync(string genus);

    }
}
