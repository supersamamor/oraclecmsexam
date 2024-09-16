using OracleCMS.CarStocks.Core.CarStocks;
using OracleCMS.CarStocks.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OracleCMS.CarStocks.Infrastructure
{
    public static class SeedData
    {
        public static void Seed(ApplicationContext context)
        {
            // Check if data is already seeded
            if (!context.Dealers.Any())
            {
                // Seed Dealers
                context.Dealers.AddRange(
                    new DealersState { Id = "1", DealerName = "AutoHub", DealerWebsite = "https://www.autohub.com" },
                    new DealersState { Id = "2", DealerName = "CarZone", DealerWebsite = "https://www.carzone.com" },
                    new DealersState { Id = "3", DealerName = "DriveNow", DealerWebsite = "https://www.drivenow.com" }
                );

                // Seed Cars
                context.Cars.AddRange(
                new CarsState { Id = "1", Make = "Audi", Model = "A4", Year = 2018 },
                new CarsState { Id = "2", Make = "BMW", Model = "3 Series", Year = 2019 },
                new CarsState { Id = "3", Make = "Mercedes", Model = "C-Class", Year = 2020 },
                new CarsState { Id = "4", Make = "Toyota", Model = "Camry", Year = 2021 },
                    new CarsState { Id = "5", Make = "Honda", Model = "Accord", Year = 2018 },
                    new CarsState { Id = "6", Make = "Tesla", Model = "Model 3", Year = 2021 }
                );

                // Seed Stocks
                context.Stocks.AddRange(
                // AutoHub's Inventory (DealerID = "1")
                    new StocksState { Id = "1", DealerID = "1", CarID = "1", Quantity = 5 },  // Audi A4
                    new StocksState { Id = "2", DealerID = "1", CarID = "4", Quantity = 10 }, // Toyota Camry
                    new StocksState { Id = "3", DealerID = "1", CarID = "6", Quantity = 3 },  // Tesla Model 3

                // CarZone's Inventory (DealerID = "2")
                    new StocksState { Id = "4", DealerID = "2", CarID = "2", Quantity = 4 },  // BMW 3 Series
                    new StocksState { Id = "5", DealerID = "2", CarID = "5", Quantity = 8 },  // Honda Accord
                    new StocksState { Id = "6", DealerID = "2", CarID = "1", Quantity = 2 },  // Audi A4

                // DriveNow's Inventory (DealerID = "3")
                    new StocksState { Id = "7", DealerID = "3", CarID = "3", Quantity = 7 },  // Mercedes C-Class
                    new StocksState { Id = "8", DealerID = "3", CarID = "4", Quantity = 5 },  // Toyota Camry
                    new StocksState { Id = "9", DealerID = "3", CarID = "2", Quantity = 6 }   // BMW 3 Series
                );

                context.SaveChanges(); // Persist the changes to the database
            }
        }
    }
}
