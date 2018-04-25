﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgstation.Server.Host.Core;
using Tgstation.Server.Host.Models;

namespace Tgstation.Server.Host.Components
{
	/// <inheritdoc />
	sealed class InstanceManager : IInstanceManager, IHostedService
	{
		/// <inheritdoc />
		public bool GracefulShutdown { get; set; }

		/// <summary>
		/// The <see cref="IInstanceFactory"/> for the <see cref="IInstanceManager"/>
		/// </summary>
		readonly IInstanceFactory instanceFactory;
		/// <summary>
		/// The <see cref="IServiceProvider"/> for the <see cref="IInstanceManager"/>
		/// </summary>
		readonly IServiceProvider serviceProvider;
		/// <summary>
		/// The <see cref="IIOManager"/> for the <see cref="IInstanceManager"/>
		/// </summary>
		readonly IIOManager ioManager;
		/// <summary>
		/// Map of <see cref="Api.Models.Instance.Id"/>s to respective <see cref="IInstance"/>s
		/// </summary>
		readonly Dictionary<long, IInstance> instances;

		/// <summary>
		/// Construct an <see cref="InstanceManager"/>
		/// </summary>
		/// <param name="instanceFactory">The value of <see cref="instanceFactory"/></param>
		/// <param name="serviceProvider">The value of <paramref name="serviceProvider"/></param>
		/// <param name="ioManager">The value of <paramref name="ioManager"/></param>
		public InstanceManager(IInstanceFactory instanceFactory, IServiceProvider serviceProvider, IIOManager ioManager)
		{
			this.instanceFactory = instanceFactory ?? throw new ArgumentNullException(nameof(instanceFactory));
			this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			this.ioManager = ioManager ?? throw new ArgumentNullException(nameof(ioManager));
			instances = new Dictionary<long, IInstance>();
		}

		/// <inheritdoc />
		public IInstance GetInstance(Models.Instance metadata)
		{
			lock (this)
			{
				if (!instances.TryGetValue(metadata.Id, out IInstance instance))
					throw new InvalidOperationException("Instance not online!");
				return instance;
			}
		}

		/// <inheritdoc />
		public async Task MoveInstance(Models.Instance instance, string newPath, CancellationToken cancellationToken)
		{
			if (newPath == null)
				throw new ArgumentNullException(nameof(newPath));
			if (instance.Online)
				await OfflineInstance(instance, cancellationToken).ConfigureAwait(false);
			Task instanceOnlineTask = null;
			try
			{
				var oldPath = instance.Path;
				await ioManager.CopyDirectory(oldPath, newPath, null, cancellationToken).ConfigureAwait(false);
				instance.Path = ioManager.ResolvePath(newPath);
				instanceOnlineTask = OnlineInstance(instance, default);
				await ioManager.DeleteDirectory(oldPath, cancellationToken).ConfigureAwait(false);
			}
			finally
			{
				if (instance.Online)
					if (instanceOnlineTask == null)
						await OnlineInstance(instance, default).ConfigureAwait(false);
					else
						await instanceOnlineTask.ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		public async Task OfflineInstance(Models.Instance metadata, CancellationToken cancellationToken)
		{
			IInstance instance;
			lock (this)
			{
				if (!instances.TryGetValue(metadata.Id, out instance))
					throw new InvalidOperationException("Instance not online!");
				instances.Remove(metadata.Id);
			}
			await instance.StopAsync(cancellationToken).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public async Task OnlineInstance(Models.Instance metadata, CancellationToken cancellationToken)
		{
			var instance = instanceFactory.CreateInstance(metadata);
			lock (this)
			{
				if (instances.ContainsKey(metadata.Id))
					throw new InvalidOperationException("Instance already online!");
				instances.Add(metadata.Id, instance);
			}
			await instance.StartAsync(cancellationToken).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public async Task StartAsync(CancellationToken cancellationToken)
		{
			using(var scope = serviceProvider.CreateScope())
			{
				var databaseContext = scope.ServiceProvider.GetRequiredService<IDatabaseContext>();
				await databaseContext.Initialize(cancellationToken).ConfigureAwait(false);
				var dbInstances = databaseContext.Instances.Where(x => x.Online).Include(x => x.RepositorySettings).Include(x => x.ChatSettings).Include(x => x.DreamDaemonSettings).ToAsyncEnumerable();
				var tasks = new List<Task>();
				await dbInstances.ForEachAsync(metadata => tasks.Add(OnlineInstance(metadata, cancellationToken)), cancellationToken).ConfigureAwait(false);
				await Task.WhenAll(tasks).ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		public async Task StopAsync(CancellationToken cancellationToken)
		{
			await Task.WhenAll(instances.Select(x => x.Value.StopAsync(cancellationToken))).ConfigureAwait(false);
			instances.Clear();
		}
	}
}