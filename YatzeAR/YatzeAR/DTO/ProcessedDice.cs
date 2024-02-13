using Emgu.CV;

namespace YatzeAR.DTO
{
    public class ProcessedDice
    {
        public List<Dice> Dices { get; set; } = new List<Dice>();
        public Mat DrawnFrame { get; set; } = new Mat();
    }
}