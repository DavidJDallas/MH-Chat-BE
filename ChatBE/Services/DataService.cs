using DataStoreApi.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Threading.Tasks;

//A services directory is used to organise and contain classes that provide various services or business logic for the app. E.g. interacting with databases, performing calculations, handling specific aspects. 
//The service file here interacts with a MongoDB database to perform CRUD operations. 
//Models define the structure of the data, whereas services handles the logic for interacting with external resources, e.g. the Mongo database. 

//Models focus on the data representation, and services handle the data access and manipulation logic.


namespace DataStoreApi.Services;

public class DataService
{
    private readonly IMongoCollection<Data> _dataCollection;

    //Constructor below to initialize _dataCollection
    public DataService(IOptions<DatabaseSettings> databaseSettings)
    {
        var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
        Console.WriteLine(mongoClient);

        var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);

        _dataCollection = mongoDatabase.GetCollection<Data>(databaseSettings.Value.DataCollectionName);
    }

    public async Task<List<Data>> GetAsync() => await _dataCollection.Find(_ => true).ToListAsync();

    
    public async Task<Data> GetByIdAsync(string id)
    {
        var filter = Builders<Data>.Filter.Eq(d => d.Id, id);
        return await _dataCollection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(Data newData) => await _dataCollection.InsertOneAsync(newData);

    public async Task DeleteAsync()
    {
        var filter = Builders<Data>.Filter.Empty;
        await _dataCollection.DeleteManyAsync(filter);    
    }
    }