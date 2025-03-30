using System;
using System.Collections.Generic;
using System.Threading;

class ConsultingRooms
{
    // Simulate that we have a number of rooms and doctors. The number of simultaneous attendance can only be as high as the smallest number
    static int availableConsultRooms = 4;
    static int availableDoctors = 4;
    static int maximumSimultaneousConsults = Math.Min(availableConsultRooms, availableDoctors);

    // Add a consult queue with a lock object
    private static readonly SortedList<int, Patient> consultQueue = new SortedList<int, Patient>();
    private static object queueLock = new object();

    // A semaphore to simulate the consulting room availability
    static SemaphoreSlim consultAvailability = new SemaphoreSlim(maximumSimultaneousConsults);

    // Method for attending patients
    public static void AttendPatient(Patient patient, List<Patient> patientList)
    {
        // Add patient to queue
        lock (queueLock) {
            consultQueue.Add(patient.ArrivalNumber, patient);
        }

        // Set dispatched boolean to false until patient is dispatched
        bool dispatched = false; 

        // While the patient is dispatched check if the first value in the queue matches the patient number
        while (!dispatched) {
            if (consultQueue.Values.First().ArrivalNumber == patient.ArrivalNumber && consultAvailability.CurrentCount > 0) {
                // If so, lock the queue, give them the consult and remove them from the queue. Dispatched will be set to true.
                lock (queueLock) {
                    consultAvailability.Wait();
                    consultQueue.Remove(patient.ArrivalNumber);
                    dispatched = true;
                }
            }

            // Sleep a bit at each check to avoid overload
            if (!dispatched) {
                Thread.Sleep(10);
            }
        }


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