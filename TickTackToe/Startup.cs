using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TickTackToe.Hubs;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace TickTackToe
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            using (var dbContext = new DbContext("localhost:6379"))
            {
                var listOfClubs = new List<Club>()
                {
                    new Club
                    {
                        ClubName = "Barcelona",
                        Players = new List<string>
                        {
                            "Messi",
                            "Ronaldo Nazario",
                            "Figo",
                            "Dembele",
                            "Lewandowski",
                            "Alcantara",
                            "Coutinho",
                            "leka"
                        }
                    },
                    new Club
                    {
                        ClubName = "Man Utd",
                        Players = new List<string>
                        {
                            "Ronaldo",
                            "Rooney",
                            "Pogba",
                            "Sancho",
                            "Januzaj",
                            "Blind",
                            "leka"
                        }
                    },
                    new Club
                    {
                        ClubName = "Real Madrid",
                        Players = new List<string>
                        {
                            "Ronaldo",
                            "Ronaldo Nazario",
                            "Figo",
                            "Bellingham",
                            "Alonso",
                            "leka"


                        }
                    },
                    new Club
                    {
                        ClubName = "Dortmund",
                        Players = new List<string>
                        {
                            "Dembele",
                            "Bellingham",
                            "Lewandowski",
                            "Sancho",
                            "Januzaj",
                            "Emre",
                            "leka"
                        }
                    },
                    new Club
                    {
                        ClubName = "Bayern",
                        Players = new List<string>
                        {
                            "Lewandowski",
                            "Mane",
                            "Coutinho",
                            "Alcantara",
                            "Blind",
                            "leka"
                        }
                    },
                    new Club
                    {
                        ClubName = "Liverpool",
                        Players = new List<string>
                        {
                            "Alcantara",
                            "Mane",
                            "Coutinho",
                            "Alonso",
                            "Emre",
                            "leka"
                        }
                    },
                };

                dbContext.SaveObjects("Clubs", listOfClubs);
            }

            app.UseFileServer();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<TickTackToeHub>("/tickTackToeHub");
            });
        }
    }
}
