using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Growthstories.WP8.Models.Commands
{
    class NewPlantCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            throw new NotImplementedException();
        }

        public event EventHandler CanExecuteChanged;

        public PlantDTO plant { get; set; }

        public IHandler receiver { get; set; }

        public void Execute(object parameter)
        {
            receiver.NewPlant(plant);
        }
    }
}
