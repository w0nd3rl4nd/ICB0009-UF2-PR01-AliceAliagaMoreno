using System;
using System.Collections.Generic;
using System.Threading;

class Hospital
{

    // Global list of hosted patients
    static List<Patient> patientList = new List<Patient>();

    static void Main()
    {
        Console.WriteLine("=== Hospital Simulation===");
        // Patients arrive...
        SimulatePatientArrival(4);

    }

    static void SimulatePatientArrival(int patientQuantity)
    {

        for (int i = 0; i < patientQuantity; i++)
        {

            // Register new patient 
            patientList = RegisterNewPatient(patientList);

            // Get last registry
            Patient currentPatient = patientList[patientList.Count - 1];

            // Launch a new thread and attend this patient
            new Thread(() => ConsultingRooms.AttendPatient(currentPatient, patientList)).Start();

            Thread.Sleep(2000); // A new patient should only arrive every two seconds
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
        int ConsultancyTime = random.Next(5, 16);
        string Status = "Awaiting";

        // Create the patient
        Patient newPatient = new Patient(newId, ArrivalNumber, ConsultancyTime, Status);

        // Add it to the global list
        patientList.Add(newPatient);

        // Log the Id and Arrivalnumber
        Console.WriteLine($"[Registration] Patient {newId} registered (Arrival #{ArrivalNumber})");

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

            // We set the current doctor ID as the current available slots (from 4 to 1) then we subtract -4 to return the order backwards (start from ConsultRoom 1 up to 4)
            int doctorId = (4 - consultAvailability.CurrentCount);

            // Set patient status
            patient.Status = "In consultancy";

            // Occupy a consult
            consultAvailability.Wait();
            Console.WriteLine($"[Consultation] Patient {patient.Id} is with Doctor {doctorId + 1}");

            // Simulate consultation time based on the consultancy time of the patient
            Thread.Sleep(patient.ConsultancyTime * 1000);

            // Set the Status to Discharged
            patient.Status = "Discharged";

            // Discharge patient, remove patient then release the doctor and room
            Console.WriteLine($"[Discharge] Patient {patient.Id} and Arrival {patient.ArrivalNumber} has finished with Doctor {doctorId + 1}");
            patientList.RemoveAll(p => p.Id == patient.Id);
            consultAvailability.Release();
        }
    }
}

public class Patient
{
    public int Id {get; set;}
    public int ArrivalNumber {get; set;} // Starts with 0
    public int ConsultancyTime {get; set;} // randint between 5 and 15
    public string Status {get; set;} // Awaiting | In consultancy | Discharged

    public Patient (int Id, int ArrivalNumber, int ConsultancyTime, string Status)  // Added Status parameter
    {
        this.Id = Id;
        this.ArrivalNumber = ArrivalNumber;
        this.ConsultancyTime = ConsultancyTime;
        this.Status = Status;  // Initialize Status
    }
}