using Dapper.Contrib.Extensions;

namespace APIComptageVDG.Models.ModelSql
{
    [Table("dbo.SPortailParametre")]
    public class SPortailParametre
    {
        //  SELECT TOP(1000) [ID_sPortailParametre]
        //,[Prefixe]
        //,[Codlib1]
        //,[Codlib2]
        //,[Valeur]
        //  FROM[LAVILOG_TEST_M3].[dbo].[sPortailParametre]
        [Key]
        public int ID_sPortailParametre { get; set; }
        public string? Prefixe { get; set; }
        public string? Codlib1 { get; set; }
        public string? Codlib2 { get; set; }        
        public string? Valeur { get; set; }  

    }
}
