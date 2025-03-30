using System;
using System.Threading;

class Program
{
    static void Main()
    {
        // Start the monitoring thread
        new Thread(Hospital.MonitorPatientStatus).Start();

        // Patients arrive...
        Hospital.SimulatePatientArrival(50);
    }
}