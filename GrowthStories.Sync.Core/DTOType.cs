using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Sync
{
    public enum DTOType
    {
        NOTYPE,
        nullEvent,
        user,
        garden,
        createGarden,
        plant,
        createPlant,
        addPlant,
        relationship,
        addRelationship,
        referenceSchedule,
        addReferenceSchedule,
        intervalSchedule,
        createIntervalSchedule,
        comment,
        addComment,
        addFBComment,
        photo,
        addPhoto,
        watering,
        addWatering,
        fertilizing,
        addFertilizing,
        measurement,
        addMeasurement,
        delProperty,
        setProperty,
        plantData,
        photoMetadata,
        createUser,
        blooming,
        addBlooming,
        deceased,
        addDeceased,
        harvesting,
        addHarvesting,
        pollination,
        addPollination,
        pruning,
        addPruning,
        sprouting,
        addSprouting,
        transfer,
        addTransfer,
        misting,
        addMisting,
        delEntity
    }
}
