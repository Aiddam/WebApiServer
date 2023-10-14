﻿using Server;
using Server.Handlers;
using WebServer.Repositories;

namespace WebServer
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            WebApiHandler webApiHandler = new(typeof(Program).Assembly);
            webApiHandler.AddTransient<IUserRepository, UserRepository>();

            ServerHost host = new(webApiHandler);
            await host.StartAsync();
        }
    }
}