using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using System.Text.Json.Serialization;

namespace APIComptageVDG.Models.JsonModel
{


    public class IncrementalModel
    {
        public Upsert? upsert { get; set; }
        
        public Delete? delete { get; set; }
    }

    public class Delete
    {
        public object? Obj { get; set; }
    }

    public class Upsert
    {
        public object? Obj { get; set; }
    }


}
