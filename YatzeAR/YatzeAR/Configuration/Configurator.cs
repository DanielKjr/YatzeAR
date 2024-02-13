using YatzeAR.YatzyLogik;

namespace YatzeAR.Configuration
{
    public class Configurator
    {
        /// <summary>
        /// Master configuration method, users input their names upon a marker
        /// </summary>
        /// <returns>List of configured users</returns>
        public static List<User> Configurate(bool markerDetectionUseCamera = true, int camIndex = 1)
        {
            bool allowUndetectedDialog = false;
            List<User> configuredUsers = new List<User>();
            PlayerAR markerDetection = new PlayerAR(markerDetectionUseCamera, camIndex);

            while (true)
            {             
                List<User> unconfiguredUsers = FilterMarkers(configuredUsers, markerDetection.OnFrame());

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
            }

            return configuredUsers;
        }

        /// <summary>
        /// Add name unto existing User
        /// </summary>
        /// <param name="unconfiguredUsers"></param>
        /// <returns>Fully configured user</returns>
        private static User ConfigureMarker(User unconfiguredUsers)
        {
            Console.Write("Input name for found marker: ");
            string inputName = Console.ReadLine() ?? $"Unnamed {unconfiguredUsers.Marker}";

            return new User
            {
                Name = inputName,
                Marker = unconfiguredUsers.Marker,
                Contour = unconfiguredUsers.Contour
            };
        }

        /// <summary>
        /// Whether the program should continue looking for Markers or exit configurator
        /// </summary>
        /// <returns>Continue result as boolean</returns>
        private static bool ContinueConfigurating()
        {
            Console.WriteLine("\nPress 'SPACE' to stop configuring players");
            Console.WriteLine("Press 'ANY' to continue adding players - remember to add new marker!\n\n");

            var key = Console.ReadKey();

            if (key.KeyChar == (char)ConsoleKey.Spacebar) // Stop configurating
            {
                return false;
            }
            else // Continue configurating
            {
                Console.WriteLine("\n\nSearching for new marker!\n\n");

                return true;
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
    }
}