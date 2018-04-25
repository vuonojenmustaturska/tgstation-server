﻿using Microsoft.AspNetCore;
using Tgstation.Server.Host.Startup;

namespace Tgstation.Server.Host
{
	/// <inheritdoc />
	public sealed class ServerFactory : IServerFactory
	{
		/// <inheritdoc />
		public IServer CreateServer(string[] args) => new Server(WebHost.CreateDefaultBuilder(args));
	}
}