using System;
using System.Collections.Generic;
using System.Threading;

class Hospital
{

    // Statistic calculation variables

    // Dictionary of wait times by priority
    public static Dictionary<int, List<double>> WaitTimesByPriority = new Dictionary<int, List<double>>();
    // Dictionary of patient count by priority
    public static Dictionary<int, int> PatientCountByPriority = new Dictionary<int, int>();
    public static int TotalDiagnoses = 0;
    public static DateTime SimulationStartTime;
    public static DateTime SimulationEndTime;

    public static bool started = false;


    // Global list of hosted patients
    static List<Patient> patientList = new List<Patient>();

    //Lock for locking the patient list and avoiding collisions on writing and deleting operations
    static object patientListLock = new object();

    // Global counter for arrivals
    static int nextArrivalNumber = 1;

    public static void SimulatePatientArrival(int patientQuantity)
    {

        for (int i = 0; i < patientQuantity; i++)
        {

            // Register new patient 
            lock (patientListLock) {
                patientList = RegisterNewPatient(patientList);

                if (patientList.Count == 1 && !started) {
                    SimulationStartTime = DateTime.Now; // Start simulation timer when we register the first patient
                    started = !started; // Registers that it's started to avoid bug if the last patient were to be registered as the only one for any reason
                }
            }

            

            // Get last registry
            Patient currentPatient = patientList[patientList.Count - 1];

            // Launch a new thread and manage this patient
            new Thread(() => ManagePatient(currentPatient, patientList)).Start();

            Thread.Sleep(500); // A new patient should only arrive every two seconds
        }
    }

    public static void ManagePatient(Patient currentPatient, List<Patient> patientList) {
        if (currentPatient.RequiresDiagnosis) {
            DiagnosingRooms.AttendPatient(currentPatient, patientList);
        }

        ConsultingRooms.AttendPatient(currentPatient, patientList);
    }

    public static void MonitorPatientStatus() {

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
            Console.WriteLine("ID\tArrival #\tPriority\tStatus\t\t\tAdditional info");

            // Define current time and a blank list of patients to remove
            DateTime now = DateTime.Now;

            // Empty the list from previous iteration
            toRemove.Clear();

            // For each patient, do
            foreach (Patient patient in patientList.ToList()) { // Fix a but where deleting patient from list would cause an exception by temporarily duplicating list

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

                Console.WriteLine($"{patient.Id}\t{patient.ArrivalNumber}\t\t{patient.Priority}\t\t{patient.Status}\t{times}");

            }

                foreach (Patient patient in toRemove) {
                    // Register elapsed times before deleting patient
                    if (patient.RequiresDiagnosis) {
                        double diagnosisWait = (patient.StartDiagnosisTime - patient.ArrivalTime).TotalSeconds;
                        double consultWait = (patient.StartConsultancyTime - patient.FinishDiagnosisTime).TotalSeconds;
                        double totalWait = diagnosisWait + consultWait;
                        AddWaitTime(patient.Priority, totalWait);
                    }
                    else {
                        double consultWait = (patient.StartConsultancyTime - patient.ArrivalTime).TotalSeconds;
                        AddWaitTime(patient.Priority, consultWait);
                    }

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
                    Console.Clear();
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

        SimulationEndTime = DateTime.Now;

        Console.WriteLine("\n=== Simulation statistics ===");
        Console.WriteLine("Attended patients:");
        Console.WriteLine($"- Emergencies: {PatientCountByPriority.GetValueOrDefault(1, 0)}");
        Console.WriteLine($"- Urgencies: {PatientCountByPriority.GetValueOrDefault(2, 0)}");
        Console.WriteLine($"- General consultancies: {PatientCountByPriority.GetValueOrDefault(3, 0)}");

        Console.WriteLine("Average waiting time:");
        PrintMeanWaitTime(1);
        PrintMeanWaitTime(2);
        PrintMeanWaitTime(3);

        double totalDiagnosisTime = TotalDiagnoses * 15;
        double simulationDuration = (SimulationEndTime - SimulationStartTime).TotalSeconds;
        double diagnosticUsage = (totalDiagnosisTime / (2 * simulationDuration)) * 100;
        Console.WriteLine($"Average usage of the diagnostic machinery: {diagnosticUsage:F1}%"); // Use a number with one decimal place with F1 "floating 1 decimal"
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

        // Set priority as a random value between 1 and 3
        int Priority = random.Next(1, 4);

        // Arrival Number assigned with lock to protect from concurrent arrivals
        int ArrivalNumber;
        lock (patientListLock) {
            ArrivalNumber = nextArrivalNumber;
            nextArrivalNumber++;
        }

        // Initialise consultancy time
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
        Patient newPatient = new Patient(newId, ArrivalNumber, DateTime.Now,ConsultancyTime, RequiresDiagnosis, Status, Priority);

        // Add it to the global list
        patientList.Add(newPatient);

        // Return the list
        return patientList;
    }

    // Helper method to print the median wait for a priority
    private static void PrintMeanWaitTime(int priority) {
        var times = WaitTimesByPriority.GetValueOrDefault(priority, new List<double>());

        // If times are 0 print 0s
        if (times.Count == 0) {
            Console.WriteLine($"- {GetPriorityName(priority)}: 0s");
            return;
        }

        // Calculate mean
        double average = times.Average();

        Console.WriteLine($"- {GetPriorityName(priority)}: {average:F1}s");
    }

    // Helper method to get the priority name
    private static string GetPriorityName(int priority) {
        switch (priority)
        {
            case 1: return "Emergencies";
            case 2: return "Urgencies";
            case 3: return "General consultancies";
            default: return "Unknown";
        }
    }

    // Add the wait time to the WaitTimesByPriority and also increment PatientCountByPriority
    private static void AddWaitTime(int priority, double waitTime) {
        if (!WaitTimesByPriority.ContainsKey(priority))
        {
            WaitTimesByPriority[priority] = new List<double>();
        }
        WaitTimesByPriority[priority].Add(waitTime);
        PatientCountByPriority[priority] = PatientCountByPriority.GetValueOrDefault(priority, 0) + 1;
    }

}