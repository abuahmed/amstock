using System;
using AMStock.Core.Models;

namespace AMStock.Service.Interfaces
{
    public interface IClientService : IDisposable
    {
        ClientDTO GetClient();
        string InsertOrUpdate(ClientDTO client);
        //string Disable(ClientDTO client);
        //int Delete(string clientId);
    }
}