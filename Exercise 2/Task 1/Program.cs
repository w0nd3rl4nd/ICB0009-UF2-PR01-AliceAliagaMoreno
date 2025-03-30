using System;
using System.Collections.Generic;
using System.Threading;

class Hospital
{

    // Global list of hosted patients
    static List<Patient> patientList = new List<Patient>();

    //Lock for locking the patient list and avoiding collisions on writing and deleting operations
    static object patientListLock = new object();

    static void Main()
    {
        // Start the monitoring thread
        new Thread(MonitorPatientStatus).Start();

        // Patients arrive...
        SimulatePatientArrival(20);
    }

    static void SimulatePatientArrival(int patientQuantity)
    {

        for (int i = 0; i < patientQuantity; i++)
        {

            // Register new patient 
            lock (patientListLock) {
                patientList = RegisterNewPatient(patientList);
            }

            // Get last registry
            Patient currentPatient = patientList[patientList.Count - 1];

            // Launch a new thread and manage this patient
            new Thread(() => ManagePatient(currentPatient, patientList)).Start();

            Thread.Sleep(2000); // A new patient should only arrive every two seconds
        }
    }

    public static void ManagePatient(Patient currentPatient, List<Patient> patientList) {
        if (currentPatient.RequiresDiagnosis) {
            DiagnosingRooms.AttendPatient(currentPatient, patientList);
        }

        ConsultingRooms.AttendPatient(currentPatient, patientList);
    }

    static void MonitorPatientStatus() {

        // Define a blank list of patients to hold discharged patients that must be removed
        List<Patient> toRemove = new List<Patient>();

        // Variable for exiting the process
        bool exit = false;

        // Define empty start time that we will use to track when no patients are comming
        DateTime? emptyStartTime = null;

        while(!exit) {
            // Clear console and write headers
            Console.Clear();
            Console.WriteLine("\t\t=== Hospital Simulation===");
            Console.WriteLine("ID\tArrival #\tStatus\t\t\tAdditional info");

            // Define current time and a blank list of patients to remove
            DateTime now = DateTime.Now;

            // Empty the list from previous iteration
            toRemove.Clear();

            // For each patient, do
            foreach (Patient patient in patientList) {

                string times = "";
                switch(patient.Status) {
                    case "Awaiting diagnosis":
                        var diagnosisWaitTime = (DateTime.Now - patient.ArrivalTime).TotalSeconds;
                        times = $"Awaiting diagnosis: {Math.Floor(diagnosisWaitTime)}s";
                        break;
                    
                    case "In diagnosis":
                        var diagnosisElapsed = (DateTime.Now - patient.StartDiagnosisTime).TotalSeconds;
                        times = $"\tIn diagnosis for {Math.Floor(diagnosisElapsed)}s";
                        break;
                    
                    case "Awaiting consult":
                        // Ensures to use ArrivalTime if FinishDiagnosisTime is not set, or use FinishDiagnosisTime if it was set
                        DateTime consultWaitStart = patient.FinishDiagnosisTime == default ? patient.ArrivalTime : patient.FinishDiagnosisTime;
                        var consultWaitTime = (DateTime.Now - consultWaitStart).TotalSeconds;
                        times = $"Awaiting consult: {Math.Floor(consultWaitTime)}s";
                        break;

                    case("In consultancy"):
                        var elapsedConsultancyTime = (DateTime.Now - patient.StartConsultancyTime).TotalSeconds;
                        times = $"\tExpected consultancy time of {Math.Floor((double)patient.ConsultancyTime)}s. Currently elapsed {Math.Floor((double)elapsedConsultancyTime)}s";
                        break;
                    
                    case "Discharged":
                        var dischargeDuration = (now - patient.DischargeTime).TotalSeconds;
                        times = $"\tDischarged in {patient.ConsultancyTime}s (Removing in {10 - (int)dischargeDuration}s)";
                        
                        if (dischargeDuration >= 10){
                            toRemove.Add(patient);
                        }

                        break;
                    }

                Console.WriteLine($"{patient.Id}\t{patient.ArrivalNumber}\t\t{patient.Status}\t{times}");

            }

                foreach (Patient patient in toRemove) {
                    lock (patientListLock) {
                        patientList.Remove(patient);
                    }
                }

            // When the patient list is empty
            if (patientList.Count == 0) {

                // If emptyStartTime remains empty, add a new value
                if (!emptyStartTime.HasValue) {
                    emptyStartTime = DateTime.Now;
                }
                // Check if more than 5 seconds have passed
                else if ((DateTime.Now - emptyStartTime.Value).TotalSeconds >= 5) {
                    Console.WriteLine("No patients remaining for 5 seconds. Monitoring stopped.");
                    exit = !exit;
                }
            }
            // Reset if a patient should come in
            else {
                emptyStartTime = null;
            }

            // Update monitor every half a second for proper update latency (with visual glitching)
            // Update monitor every 1 millisecond for close-to perfect visuals
            Thread.Sleep(1);

        }
    }

    // Create a new patient and append it to the global list
    static List<Patient> RegisterNewPatient(List<Patient> patientList)
    {
        // Create a temporary rng generator
        Random random = new Random();

        // Count how many patients there currently are
        int patientQuantity = patientList.Count;

        // Generate Ids until one is unique
        int newId;
        bool idExists;
        do {
            newId = random.Next(1, 101);
            idExists = patientList.Exists(p => p.Id == newId);
        } while (idExists);

        // Initialise rest of properties
        int ArrivalNumber = patientQuantity + 1;
        int ConsultancyTime = random.Next(10, 21);

        // Randomly generate a 50% chance of needing diagnosis
        bool RequiresDiagnosis = (random.Next(100) < 50);

        // Assign status based on diagnosis
        string Status = "";
        if (RequiresDiagnosis) {
            Status = "Awaiting diagnosis";
        } else {
            Status = "Awaiting consult";
        }

        // Create the patient
        Patient newPatient = new Patient(newId, ArrivalNumber, DateTime.Now,ConsultancyTime, RequiresDiagnosis, Status);

        // Add it to the global list
        patientList.Add(newPatient);

        // Return the list
        return patientList;
    }

    // Consulting room is a subclass of hospital because it is a subsection with it's own methods, and is independent to other sections
    class ConsultingRooms {

        // Simulate that we have a number of rooms and doctors. The number of simultaneous attendance can only be as high as the smallest number
        static int availableConsultRooms = 4;
        static int availableDoctors = 4;
        static int maximumSimultaneousConsults = Math.Min(availableConsultRooms, availableDoctors);
        
        // A semaphore to simulate the consulting room availability
        static SemaphoreSlim consultAvailability = new SemaphoreSlim(maximumSimultaneousConsults);

        // Method for attending patients
        public static void AttendPatient(Patient patient, List<Patient> patientList)
        {

            // Occupy a consult. Moved up from status change to remove bug where patients wouldn't await
            consultAvailability.Wait();

            // Set patient status
            patient.StartConsultancyTime = DateTime.Now;
            patient.Status = "In consultancy";

            // Simulate consultation time based on the consultancy time of the patient
            Thread.Sleep(patient.ConsultancyTime * 1000);

            // Set the Status to Discharged
            patient.Status = "Discharged";
            patient.DischargeTime = DateTime.Now;

            // Mark patient as discharged and release the doctor/room (patient removed later by monitor)
            consultAvailability.Release();
        }
    }

    class DiagnosingRooms {

        // Simulate that we have a number of diagnosis rooms and machine operators. The number of simultaneous attendance can only be as high as the smallest number
        static int availableDiagnosingRooms = 2;
        static int availableOperators = 2;
        static int maximumSimultaneousDiagnoses = Math.Min(availableDiagnosingRooms, availableOperators);
        
        // A semaphore to simulate the diagosins room availability
        static SemaphoreSlim diagnosingRoomAvailability = new SemaphoreSlim(maximumSimultaneousDiagnoses);

        public static void AttendPatient(Patient patient, List<Patient> patientList)
        {

            // Occupy a diagnisis room.
            diagnosingRoomAvailability.Wait();

            // Set patient status
            patient.StartDiagnosisTime = DateTime.Now;
            patient.Status = "In diagnosis";

            // Simulate the diagnosis time of 15 seconds
            Thread.Sleep(15000);

            // Set the Status to "Awaiting consultancy
            patient.Status = "Awaiting consult";
            patient.FinishDiagnosisTime = DateTime.Now;

            // Release diagnosis room and update patient status to await consultancy
            diagnosingRoomAvailability.Release();
        }
    }
}

public class Patient
{
    public int Id {get; set;}
    public int ArrivalNumber {get; set;} // Starts with 1 sequentially
    public DateTime ArrivalTime {get;} // Time of arrival to calculate waiting period
    public DateTime StartConsultancyTime {get; set;} // Time of enter consultancy
    public DateTime StartDiagnosisTime {get; set;} // Time of starting diagnosis
    public DateTime FinishDiagnosisTime {get; set;} // Time of finishing diagnosis
    public DateTime DischargeTime {get; set;} // Time of discharge
    public int ConsultancyTime {get; set;} // randint between 5 and 15
    public bool RequiresDiagnosis {get; set;} // Boolean to indicate if the patient requires diagnosis
    public string Status {get; set;} // Awaiting consult | Awaiting diagnosis |  In consultancy | In diagnosis | Discharged

    public Patient (int Id, int ArrivalNumber, DateTime ArrivalTime, int ConsultancyTime, bool RequiresDiagnosis, string Status)
    {
        this.Id = Id;
        this.ArrivalNumber = ArrivalNumber;
        this.ArrivalTime = ArrivalTime;
        this.ConsultancyTime = ConsultancyTime;
        this.RequiresDiagnosis = RequiresDiagnosis;
        this.Status = Status;
    }
}