using System;
using WebApi.Models;

namespace WebApi.Common
{
    public class Validator
    {
        public void IsClientOrderValid(ClientRequestInfo cri)
        {
            if (cri.Client_id < 1 || cri.Amount < 20 || cri.Currency.Length != 3) // cri.Currency.Length always 3 characters (ISO 4217)
            {
                throw new FormatException();
            }
        }
    }
}