using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebApplication {
    public class Startup {
        public Startup(IConfiguration configuration) {
            // For this application to work you will need a trial license.
            // The MyAppKeyPair.snk linked in the project is not present in the repository, 
            // you should generate your own strong name key and keep it private.
            //
            // 1) You can generate a strong name key with the following command in the Visual Studio command prompt:
            //     sn -k MyKeyPair.snk
            //
            // 2) The next step is to extract the public key file from the strong name key (which is a key pair):
            //     sn -p MyKeyPair.snk MyPublicKey.snk
            //
            // 3) Display the public key token for the public key: 	
            //     sn -t MyPublicKey.snk
            //
            // 4) Go to the project properties Singing tab, and check the "Sign the assembly" checkbox, 
            //    and choose the strong name key you created.
            //
            // 5) Register and get your trial license from https://www.woutware.com/SoftwareLicenses.
            //    Enter your strong name key public key token that you got at step 3.
            WW.WWLicense.SetLicense("<license string>");

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.Configure<CookiePolicyOptions>(options => {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            } else {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes => {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
