using ComptageVDG.Helpers;
using ComptageVDG.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ComptageVDG.Models
{
    public class ParcelleModel: INotifyPropertyChanged
    {

        private bool _inCampagne;
        public bool inCampagne { get => _inCampagne; set{
                SetProperty(ref _inCampagne, value);
                if(id_parcelle > 0 && !string.IsNullOrEmpty(ut))
                    MessageBrokerImpl.Instance.Publish(this, MessageBrokerImpl.Notification("CHANGESTATE", this));
            } }
        public int id_parcelle { get; set; }
        public int id_propriete { get; set; }
        public string? prestataire { get; set; }
        public string? type_vendange { get; set; }
        public string? totalement_vendangee { get; set; }
        public string? ut { get; set; }
        public string? nameParcelle { get; set; }
        public string? nameParcelle2 { get; set; }
        public string? propriete { get; set; }
        public string? appellation { get; set; }
        public string? cepage { get; set; }
        public string? site_technique { get; set; }
        public string? site_vendange { get; set; }
        public decimal? surface { get; set; }
        public decimal? superficie_vendangee { get; set; }
        public decimal? poids_vendanges { get; set; }
        public string? qualite { get; set; }
        public int? cptGlomerule { get; set; }
        public int? cptPerforation1 { get; set; }
        public int? cptPerforation2 { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        
        protected void SetProperty<T>(ref T oldValue, T newValue, [CallerMemberName] string name = null)
        {
            // Todo test egalité
            //ancienne valeur = nouvelle valeur
            oldValue = newValue;
            //PropertyChanged -> nom de la propriété
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
