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
        createUser
    }
}
