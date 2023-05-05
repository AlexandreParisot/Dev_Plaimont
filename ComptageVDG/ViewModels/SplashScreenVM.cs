﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ComptageVDG.ViewModels
{
    public class SplashScreenVM:BaseViewModel
    {
        private string _strLoading = String.Empty;
        public string StrLoading { get => _strLoading; set => SetProperty(ref _strLoading, value); }

        public string lastErreur = String.Empty;

        public async Task<bool> loadApplication()
        {
            StrLoading = "Verfication fichier de configuration ...";
            await Task.Delay(TimeSpan.FromSeconds(1));
            if (!LoadApp.FileIniExist)
            {
                lastErreur = "Vous n'avez pas de fichier ini.";
                return false;
            }

            StrLoading = "Chargement des paramétres de configuration ...";
           
            await Task.Delay(TimeSpan.FromSeconds(1));
            
            if (LoadApp.IniModel == null)
            {
                lastErreur = "Les paramétres de configuration ne sont pas correct.";
                return false;
            }
                

            if(string.IsNullOrEmpty(LoadApp.IniModel.UrlService))
            {
                lastErreur = "L'url du web service est vide.";
                return false;
            }

            StrLoading = $"Connexion API - url : {LoadApp.IniModel.UrlService} ...";
            
            await Task.Delay(TimeSpan.FromSeconds(1));

            
            if (Connexion!.CreateAccess(LoadApp.IniModel.UrlService))
                ServiceCampagne = Connexion!.ServiceCampagne!;
           

            if(Connexion!.Instance != null && Connexion.Instance.IsConnected)
                StrLoading = $"Connexion reussie : {LoadApp.IniModel.UrlService}";
            else
            {
                lastErreur = "La connexion a échoué.";
                return false;
            }

            LoadApp.IniModel.UserName = Connexion.GetUserWindows();

            StrLoading = $"Récupération des droits utilisateur pour : {LoadApp.IniModel.UserName} ...";
             
            await Task.Delay(TimeSpan.FromSeconds(1));

            LoadApp.IniModel.Credentials = Connexion.GetCredentialWindows();


            return true;
        }

    }
}
