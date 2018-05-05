﻿namespace Tgstation.Server.Api.Models
{
	/// <summary>
	/// Indicates a chat channel
	/// </summary>
	public class ChatChannel
	{
		/// <summary>
		/// The IRC channel name
		/// </summary>
		public string IrcChannel { get; set; }

		/// <summary>
		/// The Discord channel ID
		/// </summary>
		public long DiscordChannelId { get; set; }

		/// <summary>
		/// If the <see cref="ChatChannel"/> is an admin channel
		/// </summary>
		public bool IsAdminChannel { get; set; }
	}
}