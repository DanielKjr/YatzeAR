using YatzeAR.YatzyLogik;

namespace YatzeAR.Configurator
{
    public class GameConfigurator
    {
        private static List<string> configuredMarkers = new List<string>();

        /// <summary>
        /// Master configuration method, users input their names upon a marker
        /// </summary>
        /// <returns>List of configured users</returns>
        public static List<User> Configurate()
        {
            bool allowUndetectedDialog = false;
            List<User> users = new List<User>();
            bool configurating = true;
            PlayerAR markerDetection = new PlayerAR();

            while (configurating)
            {
                markerDetection.OnFrame();

                var foundMarkerNames = FilterMarkers(configuredMarkers, markerDetection.FoundPlayerMarkers);

                if (foundMarkerNames.Count > 0)
                {
                    users.Add(ConfigureMarker(foundMarkerNames, markerDetection, out allowUndetectedDialog));
                }
                else if (allowUndetectedDialog)
                {
                    configurating = ContinueOrStopConfiguring(out allowUndetectedDialog);
                }
            }

            return users;
        }

        public static User ConfigureMarker(List<string> foundMarkers, PlayerAR markerDetection, out bool allowUndetectedDialog)
        {
            Console.Write("Input name for found marker: ");
            string inputName = Console.ReadLine() ?? "";

            configuredMarkers.Add(foundMarkers[0]);

            allowUndetectedDialog = true;

            return new User
            {
                Name = inputName,
                Marker = foundMarkers[0],
                Contour = markerDetection.FoundPlayerMarkers[0].Contour
            };
        }

        /// <summary>
        /// Intakes lists of found markers and already configured markers in order to remove already configured from incomming found
        /// </summary>
        /// <param name="configuredMarkers"></param>
        /// <param name="foundMarkers"></param>
        /// <returns>Usable list of unconfigured markers</returns>
        public static List<string> FilterMarkers(List<string> configuredMarkers, List<User> foundMarkers)
        {
            List<string> filter = new List<string>();

            foreach (var found in foundMarkers)
            {
                filter.Add(found.Marker);
            }

            foreach (var alreadyConfiguredMarker in configuredMarkers)
            {
                filter.Remove(alreadyConfiguredMarker);
            }

            return filter;
        }

        private static bool ContinueOrStopConfiguring(out bool allowUndetectedDialog)
        {
            Console.WriteLine("\nPress 'SPACE' to stop configuring players");
            Console.WriteLine("Press 'ANY' to continue adding players - remember to add new marker!\n\n");

            var key = Console.ReadKey();

            if (key.KeyChar == (char)ConsoleKey.Spacebar)
            {
                allowUndetectedDialog = false;

                return false;
            }
            else
            {
                allowUndetectedDialog = false;

                Console.WriteLine("\n\nSearching for new marker!\n\n");

                return true;
            }
        }
    }
}