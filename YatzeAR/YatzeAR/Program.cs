namespace YatzeAR
{
    public class Program
    {
        static void Main(string[] args)
        {
            var diceAR = new DiceAR(false,1,30);
            //var markerAR = new MarkerAR();
            //markerAR.Run();
            diceAR.Run();
        }
    }
}