﻿using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;

namespace YatzeAR.Marker
{
    internal class Player
    {
        public const int PLAYER_GRID_COUNT = 6;
        public const int WARPED_PLAYER_SIZE = 300;

        public static readonly VectorOfPoint PLAYER_SCREEN_COORDS = new VectorOfPoint(new[] {
            new Point(0, 0),
            new Point(WARPED_PLAYER_SIZE, 0),
            new Point(WARPED_PLAYER_SIZE, WARPED_PLAYER_SIZE),
            new Point(0, WARPED_PLAYER_SIZE)
        });

        private static Player[] playerList = new[]
        {
            new Player("player1", new byte[,] {
                { 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 255, 255, 0 },
                { 0, 255, 255, 0, 0, 0 },
                { 0, 255, 0, 0, 0, 0 },
                { 0, 0, 0, 255, 255, 0 },
                { 0, 0, 0, 0, 0, 0 },
            }),
            new Player("player2", new byte[,] {
                { 0, 0, 0, 0, 0, 0 },
                { 0, 255, 255, 255, 0, 0 },
                { 0, 0, 0, 255, 0, 0 },
                { 0, 255, 0, 255, 0, 0 },
                { 0, 0, 255, 255, 0, 0 },
                { 0, 0, 0, 0, 0, 0 },
            }),
            new Player("player3", new byte[,] {
                { 0, 0, 0, 0, 0, 0 },
                { 0, 255, 255, 255, 0, 0 },
                { 0, 255, 255, 0, 255, 0 },
                { 0, 0, 0, 0, 255, 0 },
                { 0, 255, 0, 255, 255, 0 },
                { 0, 0, 0, 0, 0, 0 },
            }),
            new Player("player4", new byte[,] {
                { 0, 0, 0, 0, 0, 0 },
                { 0, 255, 255, 0, 0, 0 },
                { 0, 255, 0, 255, 255, 0 },
                { 0, 255, 0, 0, 0, 0 },
                { 0, 0, 255, 0, 255, 0 },
                { 0, 0, 0, 0, 0, 0 },
            }),
            new Player("player5", new byte[,] {
                { 0, 0, 0, 0, 0, 0 },
                { 0, 255, 0, 0, 0, 0 },
                { 0, 255, 0, 0, 0, 0 },
                { 0, 0, 255, 255, 255, 0 },
                { 0, 255, 0, 255, 255, 0 },
                { 0, 0, 0, 0, 0, 0 },
            }),
            new Player("player6", new byte[,] {
                { 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 255, 0 },
                { 0, 0, 0, 255, 0, 0 },
                { 0, 255, 255, 0, 255, 0 },
                { 0, 0, 255, 255, 255, 0 },
                { 0, 0, 0, 0, 0, 0 },
            }),
        };

        private readonly string name;
        private readonly Matrix<byte>[] playerOrientations = new Matrix<byte>[4];

        private Player(string name, byte[,] playerData)
        {
            playerOrientations[0] = new Matrix<byte>(playerData);

            for (int i = 0; i < 3; i++)
            {
                playerOrientations[i + 1] = new Matrix<byte>(PLAYER_GRID_COUNT, PLAYER_GRID_COUNT);
                CvInvoke.Rotate(playerOrientations[i], playerOrientations[i + 1], RotateFlags.Rotate90CounterClockwise);
            }

            this.name = name;
        }

        public static bool TryFindPlayer(byte[,] playerData, out string playerName, out int orientation)
        {
            orientation = -1;
            playerName = "";

            foreach (Player player in playerList)
            {
                orientation = player.getPlayerOrientation(playerData);
                if (orientation == -1)
                    continue;

                playerName = player.name;
                return true;
            }

            return false;
        }

        private int getPlayerOrientation(byte[,] playerData)
        {
            Matrix<byte> tmp = new Matrix<byte>(playerData);
            for (int i = 0; i < playerOrientations.Length; i++)
            {
                if (playerOrientations[i].Equals(tmp))
                    return i;
            }

            return -1;
        }
    }
}