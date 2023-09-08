namespace Rikstoto.Testing.LoadTesting.TestRunner
{
    internal class TestRunSetup
    {
        public int NrOfRunsForReservations { get; set; }
        public int NrOfReservationsPerRun { get; set; }
        public int NrOfRunsForBets { get; set; }
        public int NrOfBetsPerRun { get; set; }
        public int StartingCustomerId { get; set; }
        public string BetDataString { get; set; }
    }
}
