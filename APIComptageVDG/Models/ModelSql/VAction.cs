using Dapper.Contrib.Extensions;

namespace APIComptageVDG.Models.ModelSql
{
    [Table("dbo.vAction")]
    public class VAction
    {
        //  SELECT TOP(1000) [ID_vAction]
        //,[ID_vActionMeta]
        //,[Action_Type]
        //,[Action_Date]
        //,[Action_Previsionnel]
        //  FROM[LAVILOG_TEST_M3].[dbo].[vAction]

        [Key]
        public int ID_vAction { get; set; }
        public int ID_vActionMeta { get; set; }
        public int Action_Type { get; set; }
        public DateTime Action_Date { get; set; }
        public byte? Action_Previsionnel { get; set; }
    }
}
