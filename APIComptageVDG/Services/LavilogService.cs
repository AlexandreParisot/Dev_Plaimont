using APIComptageVDG.Helpers;
using APIComptageVDG.Models;
using APIComptageVDG.Models.ModelSql;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.SignalR;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using static System.Collections.Specialized.BitVector32;
using System.Reflection;
using System.Security.Policy;
using System.Xml.Linq;
using MySqlX.XDevAPI.Common;

namespace APIComptageVDG.Services
{
    public class LavilogService
    {

        //Data Source=PLSERVER03\\SQLLAVILOG;Initial Catalog=LAVILOG_TEST_M3;User=lavilog;Password=***Keepass***; MultipleActiveResultSets = True
        private IDbConnection connection = new SqlConnection();
        
        public bool IsConnected { get {
                try
                {
                    connection.Open();
                    if (connection.State == ConnectionState.Open || connection.State == ConnectionState.Connecting)
                        return true;
                    else return false;
                }
                catch(Exception ex)
                {
                    Gestion.Erreur(ex.Message);
                    return false;
                }
                finally
                {
                    connection.Close(); 
                }
                
            } }   

        public void SetConnexion(string connexionString)
        {
            if (string.IsNullOrEmpty(connexionString))
                throw new ArgumentNullException("La chaine de connexion est null.");

            if (connection == null)
                throw new ArgumentNullException("La connexion est null.");

            if(connection.State != ConnectionState.Open)
                connection.ConnectionString = connexionString;
        }


        public void SetConnexion(IDbConnection connexion)
        {
            if (connection == null)
                throw new ArgumentNullException("L'objet connexion est null.");

            this.connection = connexion;
        }

        public async Task<Dictionary<int,string>> AsyncGetCampagnes()
        {
            if (connection == null)
                throw new ArgumentNullException("La connexion est null.");

            var campagnes = new Dictionary<int,string>();

            var reader = await connection.ExecuteReaderAsync($"select distinct Codlib1 from dbo.sPortailParametre where Prefixe = 'CVDG' and Codlib1 is not null and Codlib2 is not null order by Codlib1 desc ");

            
                while (reader.Read())
                {
                    try
                    {
                        campagnes.Add( int.Parse(reader.GetString(0)),$"Campagne {reader.GetString(0)}");
                    }
                    catch (Exception ex)
                    {
                        Gestion.Erreur($"Erreur sur la recuperation des campagnes. {ex.Message}");
                    }
                }           

            return campagnes;
        }


        public async Task<string> AsyncGetLastSynchro()
        {
            if (connection == null)
                throw new ArgumentNullException("La connexion est null.");

            var strSynchro = string.Empty;
            var reader = await connection.ExecuteReaderAsync($"select Valeur from dbo.sPortailParametre where Prefixe = 'CVDG' and Codlib1 ='last synchro' ");


            while (reader.Read())
            {
                try
                {
                    strSynchro = $"{reader.GetString(0)}";
                }
                catch (Exception ex)
                {
                    Gestion.Erreur($"Erreur sur la recuperation des campagnes. {ex.Message}");
                }
            }

            return strSynchro;
        }

        public async Task<bool> AsyncSetLastSynchro()
        {
            if (connection == null)
                throw new ArgumentNullException("La connexion est null.");

            var strSynchro = string.Empty;
            var reader = await connection.ExecuteReaderAsync($"select  1 from dbo.sPortailParametre where Prefixe = 'CVDG' and Codlib2 is null  and Codlib1 ='last synchro'");
            var trouve = false;
            var result = true;
            var req = string.Empty;
            while (reader.Read())
            {
                try
                {
                    if(reader.GetInt32(0) ==1)
                        trouve = true;
                }
                catch (Exception ex)
                {
                    Gestion.Erreur($"Erreur sur la recuperation des campagnes. {ex.Message}");
                }
            }


            if (trouve)
            {
                req = $" Update dbo.sPortailParametre set Valeur = '{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}' ";
                req += $"  where Prefixe = 'CVDG' and Codlib2 is null  and Codlib1 ='last synchro'";
            }
            else
            {
                req = $" Insert INTO dbo.sPortailParametre (Prefixe,Codlib1 ,Codlib2,Valeur) VALUES";
                req += $" ('CVDG','last synchro','', '{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}')";
            }

            if (!string.IsNullOrEmpty(req) && await connection.ExecuteAsync(req) == 0)
            {
                Gestion.Erreur($"Err Insert last synchro");
                result = false;
            }
            return result;
        }



        public async Task<IEnumerable<PeriodeModel>> AsyncGetPeriode(string year)
        {
            if (connection == null)
                throw new ArgumentNullException("La connexion est null.");

            var periodes = new List<PeriodeModel>();    

            var portailParametres =  await connection.QueryAsync<SPortailParametre>($"select * from dbo.sPortailParametre where Prefixe = 'CVDG' and Codlib1 = '{year}' ");

            if (portailParametres.Any())
            {
                foreach(var portailParametre in portailParametres)
                {
                    try
                    {
                        var periodeModel = new PeriodeModel();
                        periodeModel.Year = int.Parse(year);
                        periodeModel.Id_periode = portailParametre.ID_sPortailParametre;
                        periodeModel.Name = portailParametre.Codlib2!;
                        if (!string.IsNullOrEmpty(portailParametre.Valeur.Trim()))
                        {
                            periodeModel.DateDebut = DateTime.ParseExact(portailParametre.Valeur!.Split(" ").ToArray()[0].Trim(), "dd/MM/yyyy", null);
                            if (!string.IsNullOrEmpty(portailParametre.Valeur.Trim()) && portailParametre.Valeur!.Split(" ").Count() == 2  )
                                periodeModel.DateFin = DateTime.ParseExact(portailParametre.Valeur!.Split(" ").ToArray()[1].Trim(), "dd/MM/yyyy", null);
                        }

                        periodes.Add(periodeModel);
                    }catch(Exception ex)
                    {
                        Gestion.Erreur($"Erreur sur la recuperation de la période {year} - {portailParametre.Codlib2} - {portailParametre.Valeur} / {ex.Message}");
                    }
                }
            }
            Gestion.Info($"Nombre de comptage pour {year} : {periodes.Count()} ");
            return periodes;
        }

        public async Task<List<long>> AsyncSetPeriodes(IEnumerable<PeriodeModel> Periodes)
        {
            if (connection == null)
                throw new ArgumentNullException("La connexion est null.");

            List<long> result = new List<long>();

            if (Periodes.Any())
            {
                result = await Task.Run(async () =>
                {
                    var sPortailParametres = new List<SPortailParametre>();
                    bool insert = false;

                    var PeriodesTest = await AsyncGetPeriode(Periodes!.FirstOrDefault()!.Year.ToString());
                    if (PeriodesTest.Any() && Periodes!.FirstOrDefault()!.Id_periode == 0)
                    {
                        Gestion.Warning($"Il y a déjà une campagne en cours pour {Periodes!.FirstOrDefault()!.Year}. Les informations ne seront pas enregistrées. ");
                    }
                    else
                    {
                        foreach (var periode in Periodes)
                        {
                            try
                            {
                                var sPortailParametre = new SPortailParametre();
                                if (periode?.Id_periode != null && periode.Id_periode > 0)
                                    sPortailParametre.ID_sPortailParametre = periode.Id_periode;
                                else
                                    insert = true;
                                sPortailParametre.Codlib1 = periode?.Year.ToString();
                                sPortailParametre.Valeur = $"{periode?.DateDebut?.ToString("dd/MM/yyyy")} {periode?.DateFin?.ToString("dd/MM/yyyy")}".Trim();
                                sPortailParametre.Prefixe = "CVDG";
                                sPortailParametre.Codlib2 = periode?.Name;

                                sPortailParametres.Add(sPortailParametre);
                            }
                            catch (Exception ex)
                            {
                                Gestion.Erreur($"Erreur dans la creation des paramétres de periode en base. {ex.Message}");
                            }
                        }

                    }

                    if (sPortailParametres.Any())
                    {
                        if (insert)
                        {
                            sPortailParametres.ForEach(x => result.Add( connection.Insert<SPortailParametre>(x)));
                            Gestion.Info($"Creation de la campagne {sPortailParametres[0].Codlib1} - Id : {string.Join(",",result)}");
                        }
                        else
                        {
                            sPortailParametres.ForEach(x => result.Add((connection.Update(x) ? x.ID_sPortailParametre : 0)));
                            Gestion.Info($"Mise à jours de la campagne {sPortailParametres[0].Codlib1} - Id : {string.Join(",", result)}");
                        }
                    }
                    return result;

                });
            }

            return result;
        }

        public async Task<IEnumerable<ParcelleModel>> AsyncGetParcellesInCampagne(int year)
        {
            try
            {
                var result = new List<ParcelleModel>();

                var req = $@" 
                     select  UT.ID_vUniteTravail as id_parcelle 
                   , UT.UniteTravail_Code ut 
                   , UT.UniteTravail_Libelle  nameParcelle                                                                                                                                  
                   , UT.UniteTravail_Libelle2 nameParcelle2                                                                                                                                 
                   , P.Propriete_Libelle  propriete                                                                                                                                                  
                   , P.ID_vPropriete    id_propriete                                                                                                                                                      
                   , NOTE.[Note : Appellation de travail] appellation                                                                                                                        
                   , TMP.Travail_CepageMajoritaire cepage                                                                                                                                   
                   , CASE WHEN NOTE.[Note : Site de vendange] <> 'CONDOM' THEN                                                                                                               
                                                                                                  (SELECT TOP 1 O2.[Qualité de classement]                                                  
                                                                                                  FROM vObservation O2                                                                      
                                                                                                                  INNER JOIN vAction A2 ON A2.ID_vAction = O2.ID_vAction                    
                                                                                                                  INNER JOIN vActionUniteTravail AUT2 ON AUT2.ID_vAction = A2.ID_vAction    
                                                                                                  WHERE O2.ID_vObservationProgramme = 2                                                     
                                                                                                                  AND AUT2.ID_vUniteTravail = UT.ID_vUniteTravail                           
                                                                                                  ORDER BY A2.Action_Date DESC                                                              
                                                                                                  )                                                                                         
                                                   ELSE                                                                                                                                     
                                                                   (SELECT TOP 1 O2.[Qualité classement COND]                                                                               
                                                                                                  FROM vObservation O2                                                                      
                                                                                                                  INNER JOIN vAction A2 ON A2.ID_vAction = O2.ID_vAction                    
                                                                                                                  INNER JOIN vActionUniteTravail AUT2 ON AUT2.ID_vAction = A2.ID_vAction    
                                                                                                  WHERE O2.ID_vObservationProgramme = 126                                                   
                                                                                                                  AND AUT2.ID_vUniteTravail = UT.ID_vUniteTravail                           
                                                                                                  ORDER BY A2.Action_Date DESC                                                              
                                                                                                  )                                                                                         
                                                   END qualite                                                                                                                              
                   , NOTE.[Note : Prestataire] prestataire                                                                                                                                   
                   , NOTE.[Note : Type de récolte] type_vendange                                                                                                                              
                   , NOTE.[Note : Site de vendange] site_vendange                                                                                                                             
                   , NOTE.[Note : Vendanges - Totalement vendangée] totalement_vendangee                                                                                                      
                   , TMP.Travail_Superficie surface                                                                                                                                         
                   , SUM(R.Recolte_Superficie) as superficie_vendangee                                                                                                                       
                   , SUM(R.Recolte_Poids) as poids_vendange  
				  ,ut.UniteTravail_Site site_technique
				  ,CASE tN.Note_Valeur 
						WHEN '{year}' THEN 'true'
						ELSE 'false'
					end inCampagne
				  , null cptGlomerule
				  , null cptPerforation1 
				  , null cptPerforation2
    from [LAVILOG_TEST_M3].dbo.vUniteTravail UT                                                                                                                                                               
                   inner join dbo.vPropriete P on P.ID_vPropriete = UT.ID_vPropriete                                                                                                        
                   inner join dbo.pTmpTravailCompoColonne TMP on TMP.ID_vUniteTravail = UT.ID_vUniteTravail                                                                                 
                   left  join dbo.pTmpNoteColonne_vUniteTravail NOTE ON NOTE.ID_Target = UT.ID_vUniteTravail                                                                                
                   left  join dbo.vRecolte R on R.ID_vUniteTravail = UT.ID_vUniteTravail and year(R.Recolte_Date) = year(getdate()) 
				   inner join dbo.tNote tN on  tN.ID_Target = UT.ID_vUniteTravail and tN.Note_Categorie = 'CVDG' and tN.Note_Valeur = '{year}'
    where(UT.UniteTravail_Archive = 0 OR UT.UniteTravail_Archive IS NULL)                                                                                                                   
        --AND TMP.Travail_Superficie > 0                                                                                                                                                    
        and P.Portail = 1                                                                                                                                                                   
        and(select count(*) from dbo.vInterlocuteur INT where INT.ID_vPropriete = P.ID_vPropriete and isnull(INT.Interloc_Email, '') not like '') > 0                                       
                                                                                                                                                         
        AND(SELECT COUNT(*)                                                                                                                                                                 
                FROM tNote                                                                                                                                                                  
                WHERE Note_Fichier = 'vUniteTravail'                                                                                                                                        
                    and Note_Categorie = 'Accident climatique'                                                                                                                              
                    and Note_Valeur = 'Non production'                                                                                                                                      
                    and ID_Target = UT.ID_vUniteTravail                                                                                                                                     
                    and DATEPART(year, Note_Date) = DATEPART(year, GETDATE())                                                                                                               
            ) = 0                                                                                                                                                                           
        AND NOTE.[Note : Statut de la plantation] = 'EN PRODUCTION'                                                                                                                          
                   and(                                                                                                                                                                     
                (                                                                                                                                                                           
                    TMP.Travail_DateDebut IS NULL                                                                                                                                           
                        AND                                                                                                                                                                 
                    (SELECT TOP 1 Travail_DateDebut                                                                                                                                         
                        FROM pTmpTravailCompoColonne AS p2                                                                                                                                  
                        WHERE p2.Travail_DateDebut <= getdate()                                                                                                                             
                        AND p2.ID_vUniteTravail = TMP.ID_vUniteTravail                                                                                                                      
                        ORDER BY Travail_DateDebut DESC                                                                                                                                     
                    ) IS NULL                                                                                                                                                               
                )                                                                                                                                                                           
             OR                                                                                                                                                                             
                TMP.Travail_DateDebut = (                                                                                                                                                   
                                            SELECT TOP 1 Travail_DateDebut                                                                                                                  
                                            FROM pTmpTravailCompoColonne AS p2                                                                                                              
                                            WHERE p2.Travail_DateDebut <= getdate()                                                                                                         
                                                AND p2.ID_vUniteTravail = TMP.ID_vUniteTravail                                                                                              
                                            ORDER BY Travail_DateDebut DESC                                                                                                                 
                                        )                                                                                                                                                   
            )                                                                                                                                                                               
    group by UT.ID_vUniteTravail   
					,  UT.UniteTravail_Code
                   , UT.UniteTravail_Libelle                                                                                                                                                
                   , UT.UniteTravail_Libelle2                                                                                                                                               
                   , P.Propriete_Libelle                                                                                                                                                    
                   , P.ID_vPropriete                                                                                                                                                        
                   , NOTE.[Note : Appellation de travail]                                                                                                                                   
                  , TMP.Travail_CepageMajoritaire                                                                                                                                          
                   , TMP.Travail_Superficie                                                                                                                                                 
                   , NOTE.[Note : Qualité]                                                                                                                                                 
                   , NOTE.[Note : Prestataire]                                                                                                                                             
                   , NOTE.[Note : Type de récolte]                                                                                                                                        
                   , NOTE.[Note : Site de vendange]                                                                                                                                        
                   , NOTE.[Note : Vendanges - Totalement vendangée]    
				 ,tN.Note_Valeur
				 ,ut.UniteTravail_Site
				  , NOTE.[Note : Site de vendange]";


                result = connection.Query<ParcelleModel>(req).ToList();

                return result;
            }
            catch (Exception e)
            {
                Gestion.Erreur($"{e.Message}");
                return null;
            }

        }

        public async Task<IEnumerable<ParcelleModel>> AsyncGetParcelles(int year)
        {
            try
            {
                var result = new List<ParcelleModel>();

                var req = $@" 
                     select  UT.ID_vUniteTravail as id_parcelle 
                   , UT.UniteTravail_Code ut 
                   , UT.UniteTravail_Libelle  nameParcelle                                                                                                                                  
                   , UT.UniteTravail_Libelle2 nameParcelle2                                                                                                                                 
                   , P.Propriete_Libelle  propriete                                                                                                                                                  
                   , P.ID_vPropriete    id_propriete                                                                                                                                                      
                   , NOTE.[Note : Appellation de travail] appellation                                                                                                                        
                   , TMP.Travail_CepageMajoritaire cepage                                                                                                                                   
                   , CASE WHEN NOTE.[Note : Site de vendange] <> 'CONDOM' THEN                                                                                                               
                                                                                                  (SELECT TOP 1 O2.[Qualité de classement]                                                  
                                                                                                  FROM vObservation O2                                                                      
                                                                                                                  INNER JOIN vAction A2 ON A2.ID_vAction = O2.ID_vAction                    
                                                                                                                  INNER JOIN vActionUniteTravail AUT2 ON AUT2.ID_vAction = A2.ID_vAction    
                                                                                                  WHERE O2.ID_vObservationProgramme = 2                                                     
                                                                                                                  AND AUT2.ID_vUniteTravail = UT.ID_vUniteTravail                           
                                                                                                  ORDER BY A2.Action_Date DESC                                                              
                                                                                                  )                                                                                         
                                                   ELSE                                                                                                                                     
                                                                   (SELECT TOP 1 O2.[Qualité classement COND]                                                                               
                                                                                                  FROM vObservation O2                                                                      
                                                                                                                  INNER JOIN vAction A2 ON A2.ID_vAction = O2.ID_vAction                    
                                                                                                                  INNER JOIN vActionUniteTravail AUT2 ON AUT2.ID_vAction = A2.ID_vAction    
                                                                                                  WHERE O2.ID_vObservationProgramme = 126                                                   
                                                                                                                  AND AUT2.ID_vUniteTravail = UT.ID_vUniteTravail                           
                                                                                                  ORDER BY A2.Action_Date DESC                                                              
                                                                                                  )                                                                                         
                                                   END qualite                                                                                                                              
                   , NOTE.[Note : Prestataire] prestataire                                                                                                                                   
                   , NOTE.[Note : Type de récolte] type_vendange                                                                                                                              
                   , NOTE.[Note : Site de vendange] site_vendange                                                                                                                             
                   , NOTE.[Note : Vendanges - Totalement vendangée] totalement_vendangee                                                                                                      
                   , TMP.Travail_Superficie surface                                                                                                                                         
                   , SUM(R.Recolte_Superficie) as superficie_vendangee                                                                                                                       
                   , SUM(R.Recolte_Poids) as poids_vendange  
				  ,ut.UniteTravail_Site site_technique
				  ,CASE tN.Note_Valeur 
						WHEN '{year}' THEN 'true'
						ELSE 'false'
					end inCampagne
				  , null cptGlomerule
				  , null cptPerforation1 
				  , null cptPerforation2
    from [LAVILOG_TEST_M3].dbo.vUniteTravail UT                                                                                                                                                               
                   inner join dbo.vPropriete P on P.ID_vPropriete = UT.ID_vPropriete                                                                                                        
                   inner join dbo.pTmpTravailCompoColonne TMP on TMP.ID_vUniteTravail = UT.ID_vUniteTravail                                                                                 
                   left  join dbo.pTmpNoteColonne_vUniteTravail NOTE ON NOTE.ID_Target = UT.ID_vUniteTravail                                                                                
                   left  join dbo.vRecolte R on R.ID_vUniteTravail = UT.ID_vUniteTravail and year(R.Recolte_Date) = year(getdate()) 
				   left  join dbo.tNote tN on  tN.ID_Target = UT.ID_vUniteTravail and tN.Note_Categorie = 'CVDG' and tN.Note_Valeur = '{year}'
    where(UT.UniteTravail_Archive = 0 OR UT.UniteTravail_Archive IS NULL)                                                                                                                   
        --AND TMP.Travail_Superficie > 0                                                                                                                                                    
        and P.Portail = 1                                                                                                                                                                   
        and(select count(*) from dbo.vInterlocuteur INT where INT.ID_vPropriete = P.ID_vPropriete and isnull(INT.Interloc_Email, '') not like '') > 0                                       
                                                                                                                                                         
        AND(SELECT COUNT(*)                                                                                                                                                                 
                FROM tNote                                                                                                                                                                  
                WHERE Note_Fichier = 'vUniteTravail'                                                                                                                                        
                    and Note_Categorie = 'Accident climatique'                                                                                                                              
                    and Note_Valeur = 'Non production'                                                                                                                                      
                    and ID_Target = UT.ID_vUniteTravail                                                                                                                                     
                    and DATEPART(year, Note_Date) = DATEPART(year, GETDATE())                                                                                                               
            ) = 0                                                                                                                                                                           
        AND NOTE.[Note : Statut de la plantation] = 'EN PRODUCTION'                                                                                                                          
                   and(                                                                                                                                                                     
                (                                                                                                                                                                           
                    TMP.Travail_DateDebut IS NULL                                                                                                                                           
                        AND                                                                                                                                                                 
                    (SELECT TOP 1 Travail_DateDebut                                                                                                                                         
                        FROM pTmpTravailCompoColonne AS p2                                                                                                                                  
                        WHERE p2.Travail_DateDebut <= getdate()                                                                                                                             
                        AND p2.ID_vUniteTravail = TMP.ID_vUniteTravail                                                                                                                      
                        ORDER BY Travail_DateDebut DESC                                                                                                                                     
                    ) IS NULL                                                                                                                                                               
                )                                                                                                                                                                           
             OR                                                                                                                                                                             
                TMP.Travail_DateDebut = (                                                                                                                                                   
                                            SELECT TOP 1 Travail_DateDebut                                                                                                                  
                                            FROM pTmpTravailCompoColonne AS p2                                                                                                              
                                            WHERE p2.Travail_DateDebut <= getdate()                                                                                                         
                                                AND p2.ID_vUniteTravail = TMP.ID_vUniteTravail                                                                                              
                                            ORDER BY Travail_DateDebut DESC                                                                                                                 
                                        )                                                                                                                                                   
            )                                                                                                                                                                               
    group by UT.ID_vUniteTravail   
					,  UT.UniteTravail_Code
                   , UT.UniteTravail_Libelle                                                                                                                                                
                   , UT.UniteTravail_Libelle2                                                                                                                                               
                   , P.Propriete_Libelle                                                                                                                                                    
                   , P.ID_vPropriete                                                                                                                                                        
                   , NOTE.[Note : Appellation de travail]                                                                                                                                   
                  , TMP.Travail_CepageMajoritaire                                                                                                                                          
                   , TMP.Travail_Superficie                                                                                                                                                 
                   , NOTE.[Note : Qualité]                                                                                                                                                 
                   , NOTE.[Note : Prestataire]                                                                                                                                             
                   , NOTE.[Note : Type de récolte]                                                                                                                                        
                   , NOTE.[Note : Site de vendange]                                                                                                                                        
                   , NOTE.[Note : Vendanges - Totalement vendangée]    
				 ,tN.Note_Valeur
				 ,ut.UniteTravail_Site
				  , NOTE.[Note : Site de vendange]";




                result = connection.Query<ParcelleModel>(req).ToList();
                                
                return result;
            }
            catch (Exception e)
            {
                Gestion.Erreur($"param input : {year} - {e.Message}");
                return null;
            }
           
        }


        public async Task<List<int>> AsyncSetParcellesCampagne(IEnumerable<ParcelleModel> Parcelles,  int year)
        {

            List<int> result = new List<int>();
            try
            {
                foreach (ParcelleModel parcelleModel in Parcelles)
                {
                    //control si deja dans les notes
                    var reader = await connection.ExecuteReaderAsync($"select 1 from dbo.tNote where Note_Categorie = 'CVDG' and Note_Valeur = '{year}' and Note_Fichier = 'vUniteTravail' and ID_Target = {parcelleModel.id_parcelle}");
                    bool trouve = false;
                    while (reader.Read())
                    {
                        if (reader.GetInt32(0) == 1)
                            trouve = true;
                    }
                    //si il faut le sotir delete 
                    if (!parcelleModel.inCampagne && trouve)
                    {
                        if (await connection.ExecuteAsync($"Delete from dbo.tNote where Note_Categorie = 'CVDG' and Note_Valeur = '{year}' and Note_Fichier = 'vUniteTravail' and ID_Target = {parcelleModel.id_parcelle}") == 0)
                        {
                            Gestion.Erreur($"Err Delete campagne {year} - Parcelle {parcelleModel.id_parcelle} - {parcelleModel.nameParcelle} ");
                        }else
                            result.Add(parcelleModel.id_parcelle);
                    }
                    //si il faut l'ajoute insert.
                    else if (parcelleModel.inCampagne && !trouve)
                    {
                        var req = $" Insert INTO dbo.tNote ( ID_Target, Note_Valeur, Note_Categorie, Note_Fichier, Note_Date , Note_Type) VALUES";
                        req += $" ({parcelleModel.id_parcelle}, '{year}', 'CVDG','vUniteTravail', GETDATE(), 0)";

                       if(await connection.ExecuteAsync(req) == 0)
                        {
                            Gestion.Erreur($"Err Insert campagne {year} - Parcelle {parcelleModel.id_parcelle} - {parcelleModel.nameParcelle} ");
                        }
                        else
                         result.Add(parcelleModel.id_parcelle);                            
                    }
                }
                return result;
            }catch(Exception ex)
            {
                Gestion.Erreur($"Err  Campagne {year} -  {ex.Message}");
                return new List<int>();
            }
        }

        public Task<IEnumerable<long>> AsyncSetCptParcellesCampagne(ParcelleModel Parcelles, int year)
        {
            throw new NotImplementedException();
        }
    }
}
