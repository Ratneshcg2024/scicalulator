
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SCIMetricAPI.Models
{
    public class Recommendations
    {
        public int Id { get; set; }
        [Column("Recommender_Label")]
        public string RecommenderLabel { get; set; }                
        //public string Title { get; set; }                 
        public string Description { get; set; }  
        [Column("Recommendations")]
        public string Recommendation { get; set; }     
        
        [Column("Config_Template")]
        public string? ConfigTemplate { get; set; }
    }
}
