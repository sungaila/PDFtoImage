using Microsoft.AspNetCore.Components.Forms;
using System;
using System.IO;
using System.Threading;

namespace PDFtoImage.WebConverter.Models
{
	public record DummyFile(string Name, DateTimeOffset LastModified, long Size, string ContentType) : IBrowserFile
	{
		public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default) => throw new NotImplementedException();
	}
}
