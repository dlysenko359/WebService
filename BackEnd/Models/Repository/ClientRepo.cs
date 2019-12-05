using Dapper;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace BackEnd.Models.Repository
{
    public class ClientRepo
    {
        static readonly IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionToBankDB"].ConnectionString);

        public int PostOrder(Order model)
        {
            var parameter = new DynamicParameters();
            parameter.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameter.Add("@Department_address", model.Department_address);
            parameter.Add("@Amount", model.Amount);
            parameter.Add("@Currency", model.Currency);
            parameter.Add("@Client_id", model.Client_id);
            parameter.Add("@Client_ip", model.Client_ip);
            parameter.Add("@Status", model.Status);

            db.Execute("sp_PostOrder", parameter, commandType: CommandType.StoredProcedure);

            return parameter.Get<int>("@Id");

        }

        public OrderModelFromDb GetOrder(int id)
        {
            var result = db.Query<OrderModelFromDb>("sp_GetOrderById", new { id = id }, commandType: CommandType.StoredProcedure);
            return result.SingleOrDefault();
        }

        public List<OrderModelFromDb> GetOrders(int client_id, string department_address)
        {
            var result = db.Query<OrderModelFromDb>("sp_GetClientOrders", new { client_id = client_id, department_address = department_address }, commandType: CommandType.StoredProcedure);
            return result.ToList();
        }
    }
}