using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Controllers;

namespace NetMemCache.Umbraco
{
	[PluginController("laykit")]
	public class MemCacheController : UmbracoApiController
	{
		private readonly ILogger<MemCacheController> logger;

		public MemCacheController(ILogger<MemCacheController> logger)
		{
			this.logger = logger;
		}

		[HttpPut]
		[HttpGet]  // For tests with the browser
		public async Task<IActionResult> Set( string key, string value, string store = "DefaultStore" )
		{
			try
			{
				var memCache = new MemCache( Path.Combine( "App_Data", store ) );
				await memCache.Set( key, value );
				return Ok();
			}
			catch (Exception ex)
			{
				logger.LogError( ex, $"{nameof( MemCacheController )}.{nameof( Set )}" );
				return StatusCode( 500 );
			}
		}

		[HttpGet]
		public async Task<IActionResult> Get( string key, string store = "DefaultStore" )
		{
			try
			{
				var memCache = new MemCache( Path.Combine( "App_Data", store ) );

				var result = await memCache.TryGetValue( key );
				if (result.Found)
					return Content( (string) result.Value );
				else
					return NotFound();
			}
			catch (Exception ex)
			{
				logger.LogError( ex, $"{nameof( MemCacheController )}.{nameof( Set )}" );
				return StatusCode( 500 );
			}
		}
	}
}
