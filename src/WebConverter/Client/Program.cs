using KristofferStrube.Blazor.FileSystem;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Thinktecture.Blazor.FileHandling;

namespace PDFtoImage.WebConverter
{
	public class Program
	{
		public static event EventHandler<HandledFileArgs>? FilesHandled;

		public static async Task Main(string[] args)
		{
			var builder = WebAssemblyHostBuilder.CreateDefault(args);
			builder.RootComponents.Add<App>("#app");
			builder.RootComponents.Add<HeadOutlet>("head::after");

			builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
			builder.Services.AddFileHandlingService();

			var host = builder.Build();
			var navigationManager = host.Services.GetService<NavigationManager>()!;

			if (host.Services.GetService<FileHandlingService>() is FileHandlingService service && await service.IsSupportedAsync())
			{
				await service.SetConsumerAsync(async (launchParams) =>
				{
					if (launchParams == null || !launchParams.Files.Any())
						return;

					if (launchParams.Files[0] is FileSystemFileHandle fileSystemFileHandle)
					{
						FilesHandled?.Invoke(null, new HandledFileArgs(await fileSystemFileHandle.GetFileAsync()));
					}
				});
			}

			await host.RunAsync();
		}

		public class HandledFileArgs : EventArgs
		{
			public KristofferStrube.Blazor.FileAPI.File File { get; }

			public HandledFileArgs(KristofferStrube.Blazor.FileAPI.File file)
			{
				File = file;
			}
		}
	}
}