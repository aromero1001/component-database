using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace YourNamespace
{
    public class Program
    {
        private static IConfiguration Configuration { get; set; }

        public static void Main(string[] args)
        {
            try
            {

                var builder = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json");

                Configuration = builder.Build();

                var client = new MongoClient(Configuration.GetConnectionString("MongoServer"));

                var database = client.GetDatabase(Configuration["database"]);

                var collection = database.GetCollection<NotificationRequest>("NotificationRequests");

                DateTime fechaDesde = DateTime.Now.AddDays(int.Parse(Configuration["cantidadDias"]));

                var pipeline = new List<BsonDocument>
                {
                    BsonDocument.Parse("{ $lookup: { from: 'Notifications', localField: '_id', foreignField: 'RequestId', as: 'notifications' } }"),
                    BsonDocument.Parse("{ $match: { notifications: { $eq: [] }, CreationDate: { $gte: ISODate('" + fechaDesde.ToString("yyyy-MM-ddTHH:mm:ssZ") + "') }, Done: true } }")
                };

                var aggregateOptions = new AggregateOptions
                {
                    AllowDiskUse = true
                };

                var notificationsRequestsNotInNotifications = collection.Aggregate<BsonDocument>(pipeline, aggregateOptions).ToList();
                

                Logger.Log("Actualizar Desde la fecha:" + fechaDesde.ToString("yyyy-MM-ddTHH:mm:ssZ"));

                int countRowsModified = 0;
                foreach (var notificationRequest in notificationsRequestsNotInNotifications)
                {
                    var filter = Builders<NotificationRequest>.Filter.And(
                        Builders<NotificationRequest>.Filter.Eq("_id", notificationRequest.GetValue("_id").AsString),
                        Builders<NotificationRequest>.Filter.Gte("CreationDate", fechaDesde)
                    );

                    var update = Builders<NotificationRequest>.Update
                        .Set("Done", false)
                        .Set("Results", new List<string>());

                    var updateOptions = new UpdateOptions
                    {
                        IsUpsert = false
                    };

                    if (bool.Parse(Configuration["actualizar"]))
                    {
                        Console.WriteLine("Actualizando: " + notificationRequest.GetValue("_id").AsString);

                        if (bool.Parse(Configuration["enabledLog"]))
                        {
                            Logger.Log("Notificacion Actualizada:  " + notificationRequest.GetValue("_id").AsString);
                        }


                        var result = collection.UpdateOne(filter, update, updateOptions);
                        long modifiedCount = result.ModifiedCount;
                        if (modifiedCount > 0)
                        {
                            countRowsModified++;
                        }

                        Logger.Log("Procesado: " + modifiedCount.ToString());

                    }
                    else
                    {
                        if (bool.Parse(Configuration["enabledLog"]))
                        {
                            Logger.Log("Simulando Notificacion Actualizada:  " + notificationRequest.GetValue("_id").AsString);
                        }

                    }
                }

                Logger.Log("Cantidad Total de registros actualizados: " + countRowsModified.ToString());

                Logger.Log("Cantidad de registros encontrados: " + notificationsRequestsNotInNotifications.Count.ToString());
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }
        }

    }
  
    public class NotificationRequest
    {
        public string Id { get; set; }
    }
}
