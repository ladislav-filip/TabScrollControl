using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using TabScrollControl;
using TabScrollControl.Annotations;

namespace TabScrollDemo
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        private List<object> _sekce;

        public MainWindowModel()
        {            
            Cislo = 255;

            EditModel = new EditViewModel()
            {
                Nazev = "Pokus",
                Mesto = "Ostrava"
            };
            EditThreeModel = new EditViewModel()
            {
                Nazev = "Pokus",
                Mesto = "Brno"
            };
            EditSecondModel = new BigEditViewModel()
            {
                Nazev = "Druhy",
                Mesto = "Olomouc"
            };
            Sekce = new List<object>(new object[] { EditModel, EditSecondModel, EditThreeModel });
        }

        public List<object> Sekce
        {
            get { return _sekce; }
            set => SetProperty(ref _sekce, value);
        }

        public int Cislo { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public EditViewModel EditModel { get; set; }

        public BigEditViewModel EditSecondModel { get; set; }

        public EditViewModel EditThreeModel { get; set; }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetProperty<T>(ref T backingField, T value, [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (Equals(backingField, value))
            {
                return;
            }

            backingField = value;
            OnPropertyChanged(propertyName);
        }
    }

}