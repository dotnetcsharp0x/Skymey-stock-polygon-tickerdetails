using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Nancy.Json;
using RestSharp;
using Skymey_main_lib.Models.Prices.Polygon;
using Skymey_main_lib.Models.Tickers.Polygon;
using Skymey_main_lib.TickerDetails.Polygon;
using Skymey_stock_polygon_tickerdetails.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Skymey_stock_polygon_tickerdetails.Actions.TickerDetails
{
    public class GetTickerDetails 
    {
        private RestClient _client;
        private RestRequest _request;
        private MongoClient _mongoClient;
        private ApplicationContext _db;
        private string _apiKey;
        public GetTickerDetails()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            IConfiguration config = builder.Build();

            _apiKey = config.GetSection("ApiKeys:Polygon").Value;
            _mongoClient = new MongoClient("mongodb://127.0.0.1:27017");
            _db = ApplicationContext.Create(_mongoClient.GetDatabase("skymey"));
        }
        public void GetTickerDetailsFromPolygon()
        {
            var all_tickers = (from i in _db.TickerList select i).AsNoTracking();
            foreach (var item in all_tickers)
            {
                _client = new RestClient("https://api.polygon.io/v3/reference/tickers/"+item.ticker+"?apiKey=" + _apiKey);
                _request = new RestRequest("https://api.polygon.io/v3/reference/tickers/"+item.ticker+"?apiKey=" + _apiKey, Method.Get);
                _request.AddHeader("Content-Type", "application/json");
                var r = _client.Execute(_request).Content;
                TickerDetailsList tp = new JavaScriptSerializer().Deserialize<TickerDetailsList>(r);
                if(tp.status == "OK")
                {
                    Console.WriteLine(tp.results.ticker);
                    Skymey_main_lib.TickerDetails.Polygon.TickerDetails? ticker_find = (from i in _db.TickerDetails where i.ticker == tp.results.ticker select i).FirstOrDefault();
                    if (ticker_find == null)
                    {
                        Skymey_main_lib.TickerDetails.Polygon.TickerDetails ticker = new Skymey_main_lib.TickerDetails.Polygon.TickerDetails();
                        ticker = tp.results;
                        if (ticker.name == null) ticker.name = "";
                        if (ticker.market == null) ticker.market = "";
                        if (ticker.locale == null) ticker.locale = "";
                        if (ticker.primary_exchange == null) ticker.primary_exchange = "";
                        if (ticker.type == null) ticker.type = "";
                        if (ticker.active == null) ticker.active = true;
                        if (ticker.currency_name == null) ticker.currency_name = "";
                        if (ticker.currency_name == null) ticker.currency_name = "";
                        if (ticker.cik == null) ticker.cik = "";
                        if (ticker.composite_figi == null) ticker.composite_figi = "";
                        if (ticker.share_class_figi == null) ticker.share_class_figi = "";
                        if (ticker.market_cap == null) ticker.market_cap = 0;
                        if (ticker.phone_number == null) ticker.phone_number = "";
                        if (ticker.address == null) ticker.address = new Skymey_main_lib.TickerDetails.Polygon.Address();
                        if (ticker.address.address1 == null) ticker.address.address1 = "";
                        if (ticker.address.city == null) ticker.address.city = "";
                        if (ticker.address.state == null) ticker.address.state = "";
                        if (ticker.address.postal_code == null) ticker.address.postal_code = "";
                        if (ticker.description == null) ticker.description = "";
                        if (ticker.sic_code == null) ticker.sic_code = "";
                        if (ticker.sic_description == null) ticker.sic_description = "";
                        if (ticker.ticker_root == null) ticker.ticker_root = "";
                        if (ticker.homepage_url == null) ticker.homepage_url = "";
                        if (ticker.total_employees == null) ticker.total_employees = 0;
                        if (ticker.list_date == null) ticker.list_date = "";
                        if (ticker.branding == null) ticker.branding = new Skymey_main_lib.TickerDetails.Polygon.Branding();
                        if (ticker.branding.logo_url == null) ticker.branding.logo_url = "";
                        if (ticker.branding.icon_url == null) ticker.branding.icon_url = "";
                        if (ticker.share_class_shares_outstanding == null) ticker.share_class_shares_outstanding = 0;
                        if (ticker.weighted_shares_outstanding == null) ticker.weighted_shares_outstanding = 0;
                        if (ticker.round_lot == null) ticker.round_lot = 0;
                        ticker._id = ObjectId.GenerateNewId();
                        ticker.Update = DateTime.UtcNow;
                        _db.TickerDetails.Add(ticker);
                    }
                    else
                    {
                        ticker_find.active = tp.results.active;
                        ticker_find.market_cap = tp.results.market_cap;
                        ticker_find.total_employees = tp.results.total_employees;
                        ticker_find.share_class_shares_outstanding = tp.results.share_class_shares_outstanding;
                        ticker_find.weighted_shares_outstanding = tp.results.weighted_shares_outstanding;
                        ticker_find.round_lot = tp.results.round_lot;
                        ticker_find.Update = DateTime.UtcNow;
                        _db.TickerDetails.Update(ticker_find);
                    }
                    _db.SaveChanges();
                }
            }
        }
    }
}
