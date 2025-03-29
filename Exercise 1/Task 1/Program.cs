using System;
using System.Threading;

class Hospital
{

    static void Main()
    {
        Console.WriteLine("=== Hospital Simulation===");

        // Patients arrive...
        SimulatePatientArrival(4);

    }

    static void SimulatePatientArrival(int patientQuantity) {

        for (int patientId = 1; patientId <= patientQuantity; patientId++)
        {
            // Launch a new thread with the AttendPatient function of ConsultingRooms and attend the specific patient ID
            int currentPatient = patientId;
            new Thread(() => ConsultingRooms.AttendPatient(currentPatient)).Start();
            Thread.Sleep(2000); // A new patient should only arrive every two secconds
        }
    }

    // Consulting room is a subclass of hospital because it is a subsection with it's own methods, and is independent to other sections
    class ConsultingRooms {

        // Simulate that we have a number of rooms and doctors. The number of simultaneous attendance can only be as high as the smallest number
        static int availableConsultRooms = 4;
        static int availableDoctors = 4;
        static int maximumPossibleSimultaneousConsults = Math.Min(availableConsultRooms, availableDoctors);
        
        // A semaphore to simulate the consulting room availability
        static SemaphoreSlim consultAvailability = new SemaphoreSlim(maximumPossibleSimultaneousConsults);

        // Method for attending patients
        public static void AttendPatient(int patientId)
        {
            // Output that a patient has arrived
            Console.WriteLine($"[Arrival] Patient {patientId} has arrived.");

            // We set the current doctor ID as the current available slots (from 4 to 1) then we subtract -4 to return the order backwards (start from ConsultRoom 1 up to 4)
            int doctorId = (4 - consultAvailability.CurrentCount);

            // Occupy a consult
            consultAvailability.Wait();
            Console.WriteLine($"[Consultation] Patient {patientId} is with Doctor {doctorId + 1}");

            // Simulate consultation time of 10 seconds
            Thread.Sleep(10000);

            // Discharge patient and release doctor
            Console.WriteLine($"[Discharge] Patient {patientId} has finished with Doctor {doctorId + 1}");
            consultAvailability.Release();
        }
    }

}