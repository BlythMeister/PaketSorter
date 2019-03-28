using McMaster.Extensions.CommandLineUtils;

namespace PaketSorter
{
    internal class Program
    {
        public static int Main(string[] args) => CommandLineApplication.Execute<Runner>(args);
    }
}
