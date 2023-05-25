using Dapper.Contrib.Extensions;

namespace APIComptageVDG.Models.ModelSql
{
    [Table("dbo.vActionMeta")]
    public class VActionMeta
    {
      //  SELECT TOP(1000) [ID_vActionMeta]
      //,[Meta_DateDebut]
      //,[Meta_DateFin]
      //,[Meta_Type]
      //  FROM[LAVILOG_TEST_M3].[dbo].[vActionMeta]

        [Key]
        public int ID_vActionMeta { get; set; } 
        public DateTime? Meta_DateDebut { get;set; }
        public DateTime? Meta_DateFin { get; set; }
        public int Meta_Type { get; set; }
    }
}
