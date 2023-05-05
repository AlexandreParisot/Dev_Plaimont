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

            var reader = await connection.ExecuteReaderAsync($"select distinct Codlib1 from dbo.sPortailParametre where Prefixe = 'CVDG' order by Codlib1 desc ");

            
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

        public async Task<IEnumerable<PeriodeModel>> AsyncGetPeriode(string year)
        {
            if (connection == null)
                throw new ArgumentNullException("La connexion est null.");

            var periodes = new List<PeriodeModel>();    

            var portailParametres =  await connection.QueryAsync<SPortailParametre>($"select * from dbo.sPortailParametre where Prefixe = 'CVDG' and Codlib1 = '{year}'  ");

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
                        periodeModel.DateDebut = DateTime.ParseExact(portailParametre.Valeur!.Split(" ").ToArray()[0], "dd/MM/yyyy", null);
                        periodeModel.DateFin = DateTime.ParseExact(portailParametre.Valeur!.Split(" ").ToArray()[1], "dd/MM/yyyy", null);

                        periodes.Add(periodeModel);
                    }catch(Exception ex)
                    {
                        Gestion.Erreur($"Erreur sur la recuperation de la période {year}. {ex.Message}");
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
                                sPortailParametre.Valeur = $"{periode?.DateDebut.ToString("dd/MM/yyyy")} {periode?.DateFin.ToString("dd/MM/yyyy")}";
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

        public async Task<IEnumerable<ParcelleModel>> AsyncGetParcellesCampagne(int year)
        {
            throw new NotImplementedException();
        }



        public async Task<IEnumerable<ParcelleModel>> AsyncGetParcelle()
        {
            var result = new List<ParcelleModel>();

            var req = $" select UT.ID_vUniteTravail as id_parcelle " + Environment.NewLine +
            $"                   , UT.UniteTravail_Code ut " + Environment.NewLine +
            $"                   , UT.UniteTravail_Libelle  nameParcelle                                                                                                                                  " + Environment.NewLine +
            $"                   , UT.UniteTravail_Libelle2 nameParcelle2                                                                                                                                 " + Environment.NewLine +
            //$"                   , P.Propriete_Libelle                                                                                                                                                    "+ Environment.NewLine +
            //$"                   , P.ID_vPropriete                                                                                                                                                        "+ Environment.NewLine +
            //$"                   , NOTE.[Note: Appellation de travail] Appellation                                                                                                                        "+ Environment.NewLine +
            //$"                   , TMP.Travail_CepageMajoritaire Cepage                                                                                                                                   "+ Environment.NewLine +
            $"                   , CASE WHEN NOTE.[Note: Site de vendange] <> 'CONDOM' THEN                                                                                                               " + Environment.NewLine +
            $"                                                                                                  (SELECT TOP 1 O2.[Qualité de classement]                                                  " + Environment.NewLine +
            $"                                                                                                  FROM vObservation O2                                                                      " + Environment.NewLine +
            $"                                                                                                                  INNER JOIN vAction A2 ON A2.ID_vAction = O2.ID_vAction                    " + Environment.NewLine +
            $"                                                                                                                  INNER JOIN vActionUniteTravail AUT2 ON AUT2.ID_vAction = A2.ID_vAction    " + Environment.NewLine +
            $"                                                                                                  WHERE O2.ID_vObservationProgramme = 2                                                     " + Environment.NewLine +
            $"                                                                                                                  AND AUT2.ID_vUniteTravail = UT.ID_vUniteTravail                           " + Environment.NewLine +
            $"                                                                                                  ORDER BY A2.Action_Date DESC                                                              " + Environment.NewLine +
            $"                                                                                                  )                                                                                         " + Environment.NewLine +
            $"                                                   ELSE                                                                                                                                     " + Environment.NewLine +
            $"                                                                   (SELECT TOP 1 O2.[Qualité classement COND]                                                                               " + Environment.NewLine +
            $"                                                                                                  FROM vObservation O2                                                                      " + Environment.NewLine +
            $"                                                                                                                  INNER JOIN vAction A2 ON A2.ID_vAction = O2.ID_vAction                    " + Environment.NewLine +
            $"                                                                                                                  INNER JOIN vActionUniteTravail AUT2 ON AUT2.ID_vAction = A2.ID_vAction    " + Environment.NewLine +
            $"                                                                                                  WHERE O2.ID_vObservationProgramme = 126                                                   " + Environment.NewLine +
            $"                                                                                                                  AND AUT2.ID_vUniteTravail = UT.ID_vUniteTravail                           " + Environment.NewLine +
            $"                                                                                                  ORDER BY A2.Action_Date DESC                                                              " + Environment.NewLine +
            $"                                                                                                  )                                                                                         " + Environment.NewLine +
            $"                                                   END qualite                                                                                                                              " + Environment.NewLine +
            //$"                   , NOTE.[Note: Prestataire] Prestataire                                                                                                                                   "+ Environment.NewLine +
            //$"                   , NOTE.[Note: Type de récolte] TypeVendange                                                                                                                              "+ Environment.NewLine +
            //$"                   , NOTE.[Note: Site de vendange] SiteVendange                                                                                                                             "+ Environment.NewLine +
            //$"                   , NOTE.[Note: Vendanges - Totalement vendangée] TotalementVendangee                                                                                                      "+ Environment.NewLine +
            //$"                   , TMP.Travail_Superficie surface                                                                                                                                         "+ Environment.NewLine +
            //$"                   , SUM(R.Recolte_Superficie) as SuperficieVendangee                                                                                                                       "+ Environment.NewLine +
            //$"                   , SUM(R.Recolte_Poids) as PoidsVendange                                                                                                                                  "+ Environment.NewLine +
            $"    from dbo.vUniteTravail UT                                                                                                                                                               " + Environment.NewLine +
            $"                   inner join dbo.vPropriete P on P.ID_vPropriete = UT.ID_vPropriete                                                                                                        " + Environment.NewLine +
            $"                   inner join dbo.pTmpTravailCompoColonne TMP on TMP.ID_vUniteTravail = UT.ID_vUniteTravail                                                                                 " + Environment.NewLine +
            $"                   left  join dbo.pTmpNoteColonne_vUniteTravail NOTE ON NOTE.ID_Target = UT.ID_vUniteTravail                                                                                " + Environment.NewLine +
            $"                   left  join dbo.vRecolte R on R.ID_vUniteTravail = UT.ID_vUniteTravail and year(R.Recolte_Date) = year(getdate())                                                         " + Environment.NewLine +
            $"    where(UT.UniteTravail_Archive = 0 OR UT.UniteTravail_Archive IS NULL)                                                                                                                   " + Environment.NewLine +
            $"        --AND TMP.Travail_Superficie > 0                                                                                                                                                    " + Environment.NewLine +
            $"        and P.Portail = 1                                                                                                                                                                   " + Environment.NewLine +
            $"        and(select count(*) from dbo.vInterlocuteur INT where INT.ID_vPropriete = P.ID_vPropriete and isnull(INT.Interloc_Email, '') not like '') > 0                                       " + Environment.NewLine +
            $"        --and P.ID_vPropriete = 1947                                                                                                                                                        " + Environment.NewLine +
            $"        AND(SELECT COUNT(*)                                                                                                                                                                 " + Environment.NewLine +
            $"                FROM tNote                                                                                                                                                                  " + Environment.NewLine +
            $"                WHERE Note_Fichier = 'vUniteTravail'                                                                                                                                        " + Environment.NewLine +
            $"                    and Note_Categorie = 'Accident climatique'                                                                                                                              " + Environment.NewLine +
            $"                    and Note_Valeur = 'Non production'                                                                                                                                      " + Environment.NewLine +
            $"                    and ID_Target = UT.ID_vUniteTravail                                                                                                                                     " + Environment.NewLine +
            $"                    and DATEPART(year, Note_Date) = DATEPART(year, GETDATE())                                                                                                               " + Environment.NewLine +
            $"            ) = 0                                                                                                                                                                           " + Environment.NewLine +
            $"        AND NOTE.[Note: Statut de la plantation] = 'EN PRODUCTION'                                                                                                                          " + Environment.NewLine +
            $"                   and(                                                                                                                                                                     " + Environment.NewLine +
            $"                (                                                                                                                                                                           " + Environment.NewLine +
            $"                    TMP.Travail_DateDebut IS NULL                                                                                                                                           " + Environment.NewLine +
            $"                        AND                                                                                                                                                                 " + Environment.NewLine +
            $"                    (SELECT TOP 1 Travail_DateDebut                                                                                                                                         " + Environment.NewLine +
            $"                        FROM pTmpTravailCompoColonne AS p2                                                                                                                                  " + Environment.NewLine +
            $"                        WHERE p2.Travail_DateDebut <= getdate()                                                                                                                             " + Environment.NewLine +
            $"                        AND p2.ID_vUniteTravail = TMP.ID_vUniteTravail                                                                                                                      " + Environment.NewLine +
            $"                        ORDER BY Travail_DateDebut DESC                                                                                                                                     " + Environment.NewLine +
            $"                    ) IS NULL                                                                                                                                                               " + Environment.NewLine +
            $"                )                                                                                                                                                                           " + Environment.NewLine +
            $"             OR                                                                                                                                                                             " + Environment.NewLine +
            $"                TMP.Travail_DateDebut = (                                                                                                                                                   " + Environment.NewLine +
            $"                                            SELECT TOP 1 Travail_DateDebut                                                                                                                  " + Environment.NewLine +
            $"                                            FROM pTmpTravailCompoColonne AS p2                                                                                                              " + Environment.NewLine +
            $"                                            WHERE p2.Travail_DateDebut <= getdate()                                                                                                         " + Environment.NewLine +
            $"                                                AND p2.ID_vUniteTravail = TMP.ID_vUniteTravail                                                                                              " + Environment.NewLine +
            $"                                            ORDER BY Travail_DateDebut DESC                                                                                                                 " + Environment.NewLine +
            $"                                        )                                                                                                                                                   " + Environment.NewLine +
            $"            )                                                                                                                                                                               " + Environment.NewLine +
            $"    group by UT.ID_vUniteTravail   " + Environment.NewLine +
            $"                   ,  UT.UniteTravail_Code                                                                                                                                                         " + Environment.NewLine +
            $"                   , UT.UniteTravail_Libelle                                                                                                                                                " + Environment.NewLine +
            $"                   , UT.UniteTravail_Libelle2                                                                                                                                               " + Environment.NewLine +
            //$"                   , P.Propriete_Libelle                                                                                                                                                    "+ Environment.NewLine +
            //$"                   , P.ID_vPropriete                                                                                                                                                        "+ Environment.NewLine +
            // $"                   , NOTE.[Note: Appellation de travail]                                                                                                                                    "+ Environment.NewLine +
            //$"                   , TMP.Travail_CepageMajoritaire                                                                                                                                          "+ Environment.NewLine +
            $"                   , TMP.Travail_Superficie                                                                                                                                                 " + Environment.NewLine +
            $"                   , NOTE.[Note: Qualité]                                                                                                                                                   " + Environment.NewLine 
           // $"                   , NOTE.[Note: Prestataire]                                                                                                                                               "+ Environment.NewLine +
           //  $"                   , NOTE.[Note: Type de récolte]                                                                                                                                           "+ Environment.NewLine +
           // $"                   , NOTE.[Note: Site de vendange]                                                                                                                                          "+ Environment.NewLine +
           // $"                   , NOTE.[Note: Vendanges - Totalement vendangée]                                                                                                                          ";
           ;

            result = connection.Query<ParcelleModel>(req).ToList();

            return result;

        }

        public async Task<IEnumerable<ParcelleModel>> AsyncGetParcelles()
        {
            try
            {
                var result = new List<ParcelleModel>();

                var str = @" 
                    select UT.ID_vUniteTravail as id_parcelle 
                   , UT.UniteTravail_Code ut 
                   , UT.UniteTravail_Libelle  nameParcelle                                                                                                                                  
                   , UT.UniteTravail_Libelle2 nameParcelle2                                                                                                                                 
                   , P.Propriete_Libelle                                                                                                                                                    
                   , P.ID_vPropriete                                                                                                                                                        
                   , NOTE.[Note : Appellation de travail] Appellation                                                                                                                        
                   , TMP.Travail_CepageMajoritaire Cepage                                                                                                                                   
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
                   , NOTE.[Note : Prestataire] Prestataire                                                                                                                                   
                   , NOTE.[Note : Type de récolte] TypeVendange                                                                                                                              
                   , NOTE.[Note : Site de vendange] SiteVendange                                                                                                                             
                   , NOTE.[Note : Vendanges - Totalement vendangée] TotalementVendangee                                                                                                      
                   , TMP.Travail_Superficie surface                                                                                                                                         
                   , SUM(R.Recolte_Superficie) as SuperficieVendangee                                                                                                                       
                   , SUM(R.Recolte_Poids) as PoidsVendange                                                                                                                                  
    from dbo.vUniteTravail UT                                                                                                                                                               
                   inner join dbo.vPropriete P on P.ID_vPropriete = UT.ID_vPropriete                                                                                                        
                   inner join dbo.pTmpTravailCompoColonne TMP on TMP.ID_vUniteTravail = UT.ID_vUniteTravail                                                                                 
                   left  join dbo.pTmpNoteColonne_vUniteTravail NOTE ON NOTE.ID_Target = UT.ID_vUniteTravail                                                                                
                   left  join dbo.vRecolte R on R.ID_vUniteTravail = UT.ID_vUniteTravail and year(R.Recolte_Date) = year(getdate())                                                         
    where(UT.UniteTravail_Archive = 0 OR UT.UniteTravail_Archive IS NULL)                                                                                                                   
        --AND TMP.Travail_Superficie > 0                                                                                                                                                    
        and P.Portail = 1                                                                                                                                                                   
        and(select count(*) from dbo.vInterlocuteur INT where INT.ID_vPropriete = P.ID_vPropriete and isnull(INT.Interloc_Email, '') not like '') > 0                                       
        --and P.ID_vPropriete = 1947                                                                                                                                                        
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
";

                string req = $"SELECT UT.ID_vUniteTravail id_parcelle " + Environment.NewLine+
                             $"   , UT.UniteTravail_Code ut" + Environment.NewLine +
                            // $"   , TMP.Travail_CommuneMajoritaire" + Environment.NewLine +
                            // $"   , UT.UniteTravail_Structure" + Environment.NewLine +
                            // $"   , UT.UniteTravail_Site" + Environment.NewLine +
                            // $"   , P.Propriete_Libelle" + Environment.NewLine +
                            // $"   , A.Designation" + Environment.NewLine +
                             $"   , NOTE.[Note : Qualité] qualite" + Environment.NewLine +
                             $"   , TMP.Travail_Superficie surface" + Environment.NewLine +
                            // $"   , NOTE.[Note : Statut de la plantation]" + Environment.NewLine +
                             $"   , UT.UniteTravail_Libelle nameParcelle" + Environment.NewLine +
                             $"   , UT.UniteTravail_Libelle2 nameParcelle2 " + Environment.NewLine +
                             //$"     , null Campagne" + Environment.NewLine +
                             $"  FROM LAVILOG.administrateur.Vignoble_vUniteTravail UT" + Environment.NewLine +
                             $"  LEFT JOIN dbo.vPropriete P ON P.ID_vPropriete = UT.ID_vPropriete" +   Environment.NewLine +
                             $"  LEFT JOIN dbo.pTmpTravailCompoColonne TMP ON TMP.ID_vUniteTravail = UT.ID_vUniteTravail" + Environment.NewLine +
                             $"  LEFT JOIN dbo.pTmpNoteColonne_vUniteTravail NOTE ON NOTE.ID_Target = UT.ID_vUniteTravail" + Environment.NewLine +
                             $"  LEFT JOIN dbo.pTmpNoteColonne_vPropriete NOTEP ON NOTEP.ID_Target = UT.ID_vPropriete" + Environment.NewLine +
                             $"  LEFT JOIN dbo.pAppellation A ON A.ID_pAppellation = NOTE.[ID_Note : Appellation de travail]" + Environment.NewLine +
                             $"  --and VU.ID_vUniteTravail = 15498" + Environment.NewLine +
                             $"  WHERE (P.Propriete_Archive = 0 OR P.Propriete_Archive IS NULL) " + Environment.NewLine +
                             $"  --and UT.ID_vUniteTravail = 15498" + Environment.NewLine +
                             $"   AND (UT.UniteTravail_Archive = 0 OR UT.UniteTravail_Archive IS NULL)" + Environment.NewLine +
                             $"  --and UT.UniteTravail_Structure <> 'R&D'" + Environment.NewLine +
                             $"  --AND TMP.Travail_Superficie > 0" + Environment.NewLine +
                             $"   AND (" + Environment.NewLine +
                             $"            (" +  Environment.NewLine +
                             $"                TMP.Travail_DateDebut IS NULL " + Environment.NewLine +
                             $"                AND " + Environment.NewLine +
                             $"                (SELECT TOP 1 Travail_DateDebut" + Environment.NewLine +
                             $"                    FROM pTmpTravailCompoColonne AS p2 " + Environment.NewLine +
                             $"                    WHERE p2.Travail_DateDebut <= '18/04/2023 16:04:43'" + Environment.NewLine +
                             $"                    AND p2.ID_vUniteTravail = TMP.ID_vUniteTravail " + Environment.NewLine +
                             $"                    ORDER BY Travail_DateDebut DESC) IS NULL" + Environment.NewLine +
                             $"             )" + Environment.NewLine +
                             $"             OR" +   Environment.NewLine +
                             $"             TMP.Travail_DateDebut = (SELECT TOP 1 Travail_DateDebut " + Environment.NewLine +
                             $"                                        FROM pTmpTravailCompoColonne AS p2 " +   Environment.NewLine +
                             $"                                        WHERE p2.Travail_DateDebut <= '18/04/2023 16:04:43'" + Environment.NewLine +
                             $"                                            AND p2.ID_vUniteTravail = TMP.ID_vUniteTravail " + Environment.NewLine +
                             $"                                        ORDER BY Travail_DateDebut DESC)" + Environment.NewLine +
                             $"      )" + Environment.NewLine +
                             $"  ORDER BY P.Propriete_Code" + Environment.NewLine +
                             $"         , P.Propriete_Libelle" + Environment.NewLine +
                             $"         , UT.UniteTravail_Code" + Environment.NewLine +
                             $"         , UT.UniteTravail_Libelle";


                result = connection.Query<ParcelleModel>(req).ToList();
                                
                return result;
            }
            catch (Exception e)
            {
                Gestion.Erreur($"{e.Message}");
                return null;
            }
           
        }


        public async Task<bool> AsyncSetParcellesCampagne(IEnumerable<ParcelleModel> Parcelles,  int year)
        {

            bool result = true;
            try
            {
                foreach (ParcelleModel parcelleModel in Parcelles)
                {
                    //control si deja dans les notes
                    var reader = await connection.ExecuteReaderAsync($"select 1 from dbo.tNote where Note_Categorie = 'CVDG' and Note_Valeur = '{year}' and Note_Fichier = 'vUniteTravail' and ID_Target = {parcelleModel.id_parcelle}");
                    bool trouve = false;
                    while (reader.Read())
                    {
                        if (reader.GetString(0) == "1")
                            trouve = true;
                    }
                    //si il faut le sotir delete 
                    if (!parcelleModel.inCampage && trouve)
                    {
                        if (await connection.ExecuteAsync($"Delete from dbo.tNote where Note_Categorie = 'CVDG' and Note_Valeur = '{year}' and Note_Fichier = 'vUniteTravail' and ID_Target = {parcelleModel.id_parcelle}") == 0)
                        {
                            Gestion.Erreur($"Err Delete campagne {year} - Parcelle {parcelleModel.id_parcelle} - {parcelleModel.nameParcelle} ");
                            result = false;
                        }
                    }
                    //si il faut l'ajoute insert.
                    else if (parcelleModel.inCampage && !trouve)
                    {
                        var req = $" Insert INTO dbo.tNote ( ID_Target, Note_Valeur, Note_Categorie, Note_Fichier, Note_Date , Note_Type) VALUES";
                        req += $" ({parcelleModel.id_parcelle}, '{year}', 'CVDG','vUniteTravail', {DateTime.Now}, 0)";

                       if(await connection.ExecuteAsync(req) == 0)
                        {
                            Gestion.Erreur($"Err Insert campagne {year} - Parcelle {parcelleModel.id_parcelle} - {parcelleModel.nameParcelle} ");
                            result = false;
                        }
                            
                    }
                }
                return result;
            }catch(Exception ex)
            {
                Gestion.Erreur($"Err  Campagne {year} -  {ex.Message}");
                return false;
            }
        }


        public Task<IEnumerable<long>> AsyncSetCptParcellesCampagne(ParcelleModel Parcelles, int year)
        {
            throw new NotImplementedException();
        }
    }
}
