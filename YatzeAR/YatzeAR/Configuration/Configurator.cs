﻿using Emgu.CV;
using YatzeAR.DTO;
using YatzeAR.YatzyLogik;

namespace YatzeAR.Configuration
{
    public class Configurator
    {
        private static byte dotIndex = 0;

        /// <summary>
        /// Master configuration method, users input their names upon a marker
        /// </summary>
        /// <returns>List of configured users</returns>
        public static List<User> Configurate(CameraService camService, bool debug = false)
        {
            bool allowUndetectedDialog = false;
            List<User> configuredUsers = new List<User>();
            PlayerAR markerDetection = new PlayerAR();

            while (true)
            {
                CapturedImage capture = GetImage(camService, debug);
                ProcessedMarkers markers = markerDetection.OnFrame(capture.Frame, capture.Frame);

                List<User> unconfiguredUsers = FilterMarkers(configuredUsers, markers.Users);

                if (unconfiguredUsers.Count > 0)
                {
                    User configuredMarker = ConfigureMarker(unconfiguredUsers[0]);

                    configuredUsers.Add(configuredMarker);

                    allowUndetectedDialog = true;
                }
                else if (allowUndetectedDialog)
                {
                    if (!ContinueConfigurating())
                    {
                        break;
                    }

                    allowUndetectedDialog = false;
                }
                else
                {
                    Searching(2);
                }
            }

            return configuredUsers;
        }

        /// <summary>
        /// Add name unto existing User
        /// </summary>
        /// <param name="unconfiguredUser"></param>
        /// <returns>Fully configured user</returns>
        private static User ConfigureMarker(User unconfiguredUser)
        {
            Console.Write($"Input name for marker '{unconfiguredUser.Marker}': ");
            string inputName = Console.ReadLine() ?? default!;

            if (inputName == "") inputName = $"Unnamed {unconfiguredUser.Marker}";

            Console.WriteLine($"'{unconfiguredUser.Marker}' is now: '{inputName}'\n");

            return new User
            {
                Name = inputName,
                Marker = unconfiguredUser.Marker,
                Rectangle = unconfiguredUser.Rectangle
            };
        }

        /// <summary>
        /// Whether the program should continue looking for Markers or exit configurator
        /// </summary>
        /// <returns>Continue result as boolean</returns>
        private static bool ContinueConfigurating()
        {
            Console.WriteLine("\nPress 'ANY KEY' to stop configuring players");
            Console.WriteLine("Press 'SPACE BAR' to continue adding players - remember to add new marker!\n\n");

            var key = Console.ReadKey();

            if (key.KeyChar == (char)ConsoleKey.Spacebar) // Continue configurating
            {
                return true;
            }
            else // Stop configurating
            {
                return false;
            }
        }

        /// <summary>
        /// Intakes lists of found users and removes any which bears the Marker of a configured user
        /// </summary>
        /// <param name="users"></param>
        /// <param name="foundMarkers"></param>
        /// <returns>Usable list of Users</returns>
        private static List<User> FilterMarkers(List<User> users, List<User> foundMarkers)
        {
            List<User> filter = new List<User>(foundMarkers);

            foreach (var marker in foundMarkers)
            {
                foreach (var user in users)
                {
                    if (marker.Marker == user.Marker)
                    {
                        filter.Remove(marker);
                        break;
                    }
                }
            }

            return filter;
        }

        /// <summary>
        /// Returns image from CamService or loads a Debug depending on <paramref name="debug"/> boolean
        /// </summary>
        /// <param name="capturer"></param>
        /// <param name="debug"></param>
        /// <returns></returns>
        private static CapturedImage GetImage(CameraService capturer, bool debug)
        {
            if (debug)
            {
                return capturer.LoadDebugImage("players_png.png");
            }
            else
            {
                return capturer.Capture();
            }
        }

        /// <summary>
        /// Displays a looping 'seaching...' message in console
        /// </summary>
        /// <param name="desiredFPS"></param>
        private static void Searching(int desiredFPS)
        {
            string[] dots = new string[4] { "", ".", ". .", ". . ." };

            Console.Clear();
            Console.Write($"\n\n\nSearching for new player marker {dots[dotIndex]}");
            Console.SetCursorPosition(0, 0);

            dotIndex++;

            if (dotIndex > dots.Length - 1)
            {
                dotIndex = 0;
            }

            CvInvoke.WaitKey((int)1000 / desiredFPS);
        }
    }
}