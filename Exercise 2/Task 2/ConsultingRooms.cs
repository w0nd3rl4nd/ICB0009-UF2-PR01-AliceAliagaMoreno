using System;
using System.Collections.Generic;
using System.Threading;

class ConsultingRooms
{
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