public class Patient
{
    public int Id {get; set;}
    public int ArrivalNumber {get; set;} // Starts with 1 sequentially
    public int Priority {get; set;} // Add the prioriry attribute. 1 = Emergency, 2 = Urgency, 3 = General
    public DateTime ArrivalTime {get;} // Time of arrival to calculate waiting period
    public DateTime StartConsultancyTime {get; set;} // Time of enter consultancy
    public DateTime StartDiagnosisTime {get; set;} // Time of starting diagnosis
    public DateTime FinishDiagnosisTime {get; set;} // Time of finishing diagnosis
    public DateTime DischargeTime {get; set;} // Time of discharge
    public int ConsultancyTime {get; set;} // randint between 5 and 15
    public bool RequiresDiagnosis {get; set;} // Boolean to indicate if the patient requires diagnosis
    public string Status {get; set;} // Awaiting consult | Awaiting diagnosis |  In consultancy | In diagnosis | Discharged

    public Patient (int Id, int ArrivalNumber, DateTime ArrivalTime, int ConsultancyTime, bool RequiresDiagnosis, string Status, int Priority)
    {
        this.Id = Id;
        this.ArrivalNumber = ArrivalNumber;
        this.Priority = Priority;
        this.ArrivalTime = ArrivalTime;
        this.ConsultancyTime = ConsultancyTime;
        this.RequiresDiagnosis = RequiresDiagnosis;
        this.Status = Status;
    }
}