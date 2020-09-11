using Microsoft.AspNetCore.Http;
using System.Net.Sockets;

namespace IP.Services
{
    public interface IAddressService
    {
        string GetAddress(HttpContext context, AddressFamily family);
    }
}
