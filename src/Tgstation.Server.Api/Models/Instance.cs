﻿using System.ComponentModel.DataAnnotations;
using Tgstation.Server.Api.Rights;

namespace Tgstation.Server.Api.Models
{
	/// <summary>
	/// Metadata about a server instance
	/// </summary>
	[Model(RightsType.InstanceManager, CanCrud = true, CanList = true)]
	public class Instance
	{
		/// <summary>
		/// The id of the <see cref="Instance"/>. Not modifiable
		/// </summary>
		[Permissions(DenyWrite = true)]
		public long Id { get; set; }

		/// <summary>
		/// The name of the <see cref="Instance"/>
		/// </summary>
		[Permissions(WriteRight = InstanceManagerRights.Rename)]
		[Required]
		public string Name { get; set; }

		/// <summary>
		/// The path to where the <see cref="Instance"/> is located. Changing this will temporarily offline the <see cref="Instance"/> while it moves
		/// </summary>
		[Permissions(WriteRight = InstanceManagerRights.Relocate)]
		[Required]
		public string Path { get; set; }

		/// <summary>
		/// If the <see cref="Instance"/> is online
		/// </summary>
		[Permissions(WriteRight = InstanceManagerRights.SetOnline)]
		public bool Online { get; set; }

		/// <summary>
		/// If <see cref="Configuration"/> can be used on the <see cref="Instance"/>
		/// </summary>
		[Permissions(WriteRight = InstanceManagerRights.SetConfiguration)]
		public bool ConfigurationAllowed { get; set; }
	}
}