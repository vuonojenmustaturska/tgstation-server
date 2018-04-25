﻿using Microsoft.IdentityModel.Tokens;
using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Tgstation.Server.Api.Models;

namespace Tgstation.Server.Host.Security
{
	/// <inheritdoc />
	sealed class TokenFactory : ITokenFactory
	{
		public static readonly string TokenAudience = typeof(Token).Assembly.GetName().Name;
		public static readonly string TokenIssuer = Assembly.GetExecutingAssembly().GetName().Name;
		public static readonly byte[] TokenSigningKey = CryptographySuite.GetSecureBytes(256);

		/// <inheritdoc />
		public Token CreateToken(Models.User user)
		{
			if (user == null)
				throw new ArgumentNullException(nameof(user));

			var claims = new Claim[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString(CultureInfo.InvariantCulture)),
				new Claim(JwtRegisteredClaimNames.Exp, $"{DateTimeOffset.Now.AddHours(1).ToUnixTimeSeconds()}"),
				new Claim(JwtRegisteredClaimNames.Nbf, $"{DateTimeOffset.Now.ToUnixTimeSeconds()}"),
				new Claim(JwtRegisteredClaimNames.Iss, TokenIssuer),
				new Claim(JwtRegisteredClaimNames.Aud, TokenAudience)
			};

			var key = new SymmetricSecurityKey(TokenSigningKey);

			var token = new JwtSecurityToken(new JwtHeader(new SigningCredentials(key, SecurityAlgorithms.HmacSha256)), new JwtPayload(claims));
			return new Token { Bearer = new JwtSecurityTokenHandler().WriteToken(token) };
		}
	}
}