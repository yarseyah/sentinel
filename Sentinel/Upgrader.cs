namespace Sentinel
{
    using System.Threading.Tasks;

    using Squirrel;

    public class Upgrader
    {
        public static async Task CheckForUpgrades()
        {
            using (var updateManager = new UpdateManager("https://github.com/yarseyah/sentinel/updates"))
            {
                await updateManager.UpdateApp();
            }
        }
    }
}