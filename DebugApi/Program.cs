using System;
using System.Threading.Tasks;

namespace DebugApi
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().Wait();
        }

        static async Task MainAsync()
        {
            try
            {
                var service = new WvlService();
                await service.PostAudioFile("");
                //await service.PostTest();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
