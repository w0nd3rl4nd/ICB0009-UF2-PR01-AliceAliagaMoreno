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

    // Add sorted set (no duplicates due to set, always minToMax order due to Sorted). We also add a priority comparer and a locker.
    // The custom comparator took quite a long time to understand, but essentially the comparer that auto sorts, will now sort priority first.
    private static readonly SortedSet<Patient> diagnosisQueue = new SortedSet<Patient>(new PatientPriorityComparer());
    private static object queueLock = new object();

    private class PatientPriorityComparer : IComparer<Patient> {
        public int Compare(Patient? x, Patient? y) {
            // Except if they are null to remove Null reference warnings
            if (x is null || y is null) throw new ArgumentException("Patients cannot be null");
            // Return which patient has hicher priority
            return x.Priority == y.Priority ? x.ArrivalNumber.CompareTo(y.ArrivalNumber) : x.Priority.CompareTo(y.Priority);
        }
    }

    // Method for attending patients
    public static void AttendPatient(Patient patient, List<Patient> patientList)
    {

        // Add patient to queue
        lock (queueLock) {
            diagnosisQueue.Add(patient);
        }

        // Set dispatched boolean to false until patient is dispatched
        bool dispatched = false; 

        while (!dispatched) {
            // Now the consult Queue will check the first (smallest) arrival number which will already have been sorted by priority
            lock (queueLock) {
                if (diagnosisQueue.First().ArrivalNumber == patient.ArrivalNumber && diagnosingRoomAvailability.CurrentCount > 0) {
                    // If so, lock the queue, give them the consult and remove them from the queue. Dispatched will be set to true.
                    diagnosingRoomAvailability.Wait();
                    diagnosisQueue.Remove(patient);
                    dispatched = true;
                }
            }

            // Sleep a bit at each check to avoid overload
            if (!dispatched) Thread.Sleep(10);
        }

        // Occupy a diagnisis room.
        patient.StartDiagnosisTime = DateTime.Now;
        patient.Status = "In diagnosis";

        // Simulate the diagnosis time of 15 seconds
        Thread.Sleep(15000);

        // Increment total diagnosis
        Hospital.TotalDiagnoses++;

        // Set the Status to "Awaiting consultancy
        patient.Status = "Awaiting consult";
        patient.FinishDiagnosisTime = DateTime.Now;

        // Release diagnosis room and update patient status to await consultancy
        diagnosingRoomAvailability.Release();
    }
}