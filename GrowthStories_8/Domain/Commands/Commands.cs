using Growthstories.WP8.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.WP8.Domain.Commands
{
    public abstract class Command : ICommand
    {
        public Guid Id { get; private set; }

        public Command()
        {
            Id = Guid.NewGuid();
        }
    }

    public class CreateGarden : Command
    {
        public CreateGarden()
            : base()
        {

        }
    }

    public class CreatePlant : Command
    {
        public string Name { get; private set; }

        public CreatePlant(string name)
            : base()
        {
            Name = name;
        }
    }

    public class SetGenus : Command
    {
        public string Genus { get; private set; }

        public SetGenus(string genus)
            : base()
        {
            Genus = genus;
        }
    }




}

