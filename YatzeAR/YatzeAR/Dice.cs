using Emgu.CV.Structure;

namespace YatzeAR
{
    public class Dice
    {
        public static List<Dice> Dices { get; set; } = new List<Dice>()
        {
            new Dice{
                DiceNumber = 1,
                Bitmap = new byte[,]{
                    {255, 255, 255},
                    {255,   0, 255},
                    {255, 255, 255}
            }}
            , new Dice{
                DiceNumber = 2,
                Bitmap = new byte[,]{
                    {255, 255,   0},
                    {255, 255, 255},
                    {  0, 255, 255}
            }}, new Dice{
                DiceNumber = 3,
                Bitmap = new byte[,]{
                    {255, 255,   0},
                    {255,   0, 255},
                    {  0, 255, 255}
            }}, new Dice{
                DiceNumber = 4,
                Bitmap = new byte[,]{
                    {  0, 255,   0},
                    {255, 255, 255},
                    {  0, 255,   0}
            }}
            , new Dice{
                DiceNumber = 5,
                Bitmap = new byte[,]{
                    {  0, 255,   0},
                    {255,   0, 255},
                    {  0, 255,   0}
            }}, new Dice{
                DiceNumber = 6,
                Bitmap = new byte[,]{
                    {  0, 255,   0},
                    {  0, 255,   0},
                    {  0, 255,   0}
            }}
        };
        public static readonly MCvPoint3D32f[][] DiceWorldCoors = new[] {
            new MCvPoint3D32f[]{
                new MCvPoint3D32f(0, 0, 0),
                new MCvPoint3D32f(1, 0, 0),
                new MCvPoint3D32f(1, 1, 0),
                new MCvPoint3D32f(0, 1, 0)
            },
            new MCvPoint3D32f[]{
                new MCvPoint3D32f(1, 0, 0),
                new MCvPoint3D32f(1, 1, 0),
                new MCvPoint3D32f(0, 1, 0),
                new MCvPoint3D32f(0, 0, 0)
            },
            new MCvPoint3D32f[]{
                new MCvPoint3D32f(1, 1, 0),
                new MCvPoint3D32f(0, 1, 0),
                new MCvPoint3D32f(0, 0, 0),
                new MCvPoint3D32f(1, 0, 0)
            },
            new MCvPoint3D32f[]{
                new MCvPoint3D32f(0, 1, 0),
                new MCvPoint3D32f(0, 0, 0),
                new MCvPoint3D32f(1, 0, 0),
                new MCvPoint3D32f(1, 1, 0)
            }
        };

        public byte[,]? Bitmap { get; set; }
        public int? DiceNumber { get; set; }
    }
}