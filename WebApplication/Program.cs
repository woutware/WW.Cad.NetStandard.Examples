using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WebApplication {
    /// <summary>
    /// Creates and displays a simple AutoCAD drawing.
    /// 
    /// Important:
    /// 
    /// The Wout Ware license is initialized in <see cref="Startup.Startup(Microsoft.Extensions.Configuration.IConfiguration)"/>.
    /// The comments there explain how to obtain and setup a trial license.
    /// </summary>
    public class Program {
        public static void Main(string[] args) {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
