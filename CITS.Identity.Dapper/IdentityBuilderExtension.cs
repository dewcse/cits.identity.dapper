using System;
using System.Collections.Generic;
using System.Text;
using CITS.Identity.Dapper.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CITS.Identity.Dapper
{
public static class IdentityBuilderExtension
	{
		/// <summary>
		/// Adds a Dapper implementation of ASP.NET Core Identity stores.
		/// </summary>
		/// <param name="builder">Helper functions for configuring identity services.</param>
		/// <param name="connectionString">The database connection string.</param>
		/// <returns>The <see cref="IdentityBuilder"/> instance this method extends.</returns>
		public static IdentityBuilder AddDapperStores(this IdentityBuilder builder, string connectionString)
		{
			AddStores(builder.Services, builder.UserType, builder.RoleType, connectionString);
			return builder;
		}

		private static void AddStores(IServiceCollection services, Type userType, Type roleType, string connectionString)
		{
			if (userType != typeof(IdentityUser))
			{
				throw new InvalidOperationException($"{nameof(AddDapperStores)} can only be called with a user that is of type {nameof(IdentityUser)}.");
			}

			if (roleType != null)
			{
				if (roleType != typeof(IdentityRole))
				{
					throw new InvalidOperationException($"{nameof(AddDapperStores)} can only be called with a role that is of type {nameof(IdentityRole)}.");
				}

				services.TryAddScoped<IUserStore<IdentityUser>, UserStore>();
				services.TryAddScoped<IRoleStore<IdentityRole>, RoleStore>();
				//services.TryAddScoped<IDatabaseConnectionFactory>(provider => new SqlConnectionFactory(connectionString));
			}
		}
	}
}
