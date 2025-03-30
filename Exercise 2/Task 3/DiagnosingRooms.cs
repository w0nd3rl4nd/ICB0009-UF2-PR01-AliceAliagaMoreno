using System;
using System.Collections.Generic;
using System.Threading;

class DiagnosingRooms
{
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