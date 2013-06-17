using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Sync
{
    public enum DTOType
    {
        user,
        addUser,
        plant,
        addPlant,
        relationship,
        addRelationship,
        referenceSchedule,
        addReferenceSchedule,
        intervalSchedule,
        addIntervalSchedule,
        comment,
        addComment,
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
        photoMetadata
    }
}
