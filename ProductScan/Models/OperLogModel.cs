using System;
using System.Collections.Generic;

namespace dbModels
{
    public partial class OperLogModel
    {
        public OperLogModel()
        {
        }
        public short? StepByID { get; set; }
        public int ID { get; set;}
        public int? LOTID { get; set;}
        public short? StepID { get; set;}
        public int? PCBID { get; set;}
        public string? StepName { get; set;}
        public string? Result { get; set;}
        public string? UserName { get; set;}
        public string? LineName { get; set;}
        public int? ErrorCodeID { get; set;}
        public DateTime? StepDate { get; set;}
    }
}
