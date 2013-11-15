using Growthstories.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Sync
{
    public interface IEventDTO
    {
        Guid AggregateId { get; set; }
        int AggregateVersion { get; set; }

        Guid? EntityId { get; set; }

        Guid MessageId { get; set; }
        long Created { get; set; }

        DTOType EventType { get; set; }

        Guid? AncestorId { get; set; }
        //Guid? StreamEntity { get; set; }
        Guid? StreamAncestor { get; set; }
        Guid? ParentId { get; set; }
        Guid? ParentAncestorId { get; set; }
    }

    public interface IAddEntityDTO : IEventDTO
    {

    }


    #region Concrete message types
    public interface ISetPropertyDTO : IEventDTO
    {
        DTOType EntityType { get; set; }
        string PropertyName { get; set; }
        dynamic PropertyValue { get; set; }
        Guid PropertyEntityId { get; set; }
    }

    public interface IDelPropertyDTO : IEventDTO
    {
        DTOType EntityType { get; set; }
        string PropertyName { get; set; }
    }

    public interface IDelEntityDTO : IEventDTO
    {

    }

    public interface ICreateUserDTO : IEventDTO
    {
        string Username { get; set; }
        string Password { get; set; }
        string Email { get; set; }

    }

    public interface ICreatePlantDTO : IAddEntityDTO
    {
        string Name { get; set; }
    }

    public interface ICreateGardenDTO : IAddEntityDTO
    {

    }

    public interface IAddPlantDTO : IEventDTO
    {
        Guid PlantId { get; set; }
    }

    public interface IAddRelationshipDTO : IEventDTO
    {
        Guid To { get; set; }
    }

    public interface IAddReferenceScheduleDTO : IAddEntityDTO
    {
        Guid PlantId { get; set; }
        Guid PlantAncestorId { get; set; }
    }

    public interface IAddIntervalScheduleDTO : IAddEntityDTO
    {
        long Interval { get; set; }

    }

    public interface ICreatePlantActionDTO : IAddEntityDTO
    {
        string Note { get; set; }
        Guid PlantId { get; set; }
        Guid PlantAncestorId { get; set; }
        string BlobKey { get; set; }
        MeasurementType MeasurementType { get; set; }
        double Value { get; set; }
        long FBUid { get; set; }
        string FBName { get; set; }
    }


    public interface IAddFBCommentDTO : IAddEntityDTO
    {
        String FbId { get; set; }
        long Uid { get; set; }
        String Name { get; set; }
        string FirstName { get; set; }
        String LastName { get; set; }
        String Note { get; set; }
    }

    #endregion

    public enum MeasurementType
    {
        NOTYPE,
        PH,
        SOIL_HUMIDITY,
        AIR_HUMIDITY,
        LENGTH,
        WEIGHT,
        ILLUMINANCE
    }
}
