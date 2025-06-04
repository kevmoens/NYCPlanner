
using Microsoft.Azure.Cosmos;
using NYCPlanner.Shared;
namespace NYCPlanner.Services
{


	public class ItineraryRepository
	{
		Container container = null!;
		public ItineraryRepository()
		{
		}
		private readonly List<DropZone> _zones =
		[
			new() { Name = TravelPriority.Must.ToString() },
			new() { Name = TravelPriority.LikeTo.ToString() },
			new() { Name = TravelPriority.Could.ToString() },
			new() { Name = TravelPriority.Out.ToString() }
		];
		private void GetContainer()
		{
			if (container != null)
			{
				return;
			}
			string? connectionString = Environment.GetEnvironmentVariable("NYCPlannerCosmosDBConnectionString");
			CosmosClient client = new(connectionString: connectionString);
			container = client.GetContainer("itinerary", "itinerary");
		}
		public async Task<List<DropZoneItem>> GetItinerary()
		{
			GetContainer();
			List<DropZoneItem> Items = [];
			ItemResponse<Trip> response = await container.ReadItemAsync<Trip>(
			   id: "0",
			   partitionKey: new PartitionKey("0")
			  );
			if (response.Resource.Zones == null)
			{
				// If no zones or items exist, initialize with default items
				// This is for the first run or if the data is empty
				Items = [];
			}
			else if (response.Resource.Zones.Count == 0 || !response.Resource.Zones.SelectMany((z) => z.Items!).Any())
			{
				Items.AddRange([
					new DropZoneItem { Zone = TravelPriority.Must.ToString(), Name = "Statue of Liberty" },
					new DropZoneItem { Zone = TravelPriority.Must.ToString(), Name = "Central Park" },
					new DropZoneItem { Zone = TravelPriority.LikeTo.ToString(), Name = "Brooklyn Bridge" },
					new DropZoneItem { Zone = TravelPriority.LikeTo.ToString(), Name = "Times Square" },
					new DropZoneItem { Zone = TravelPriority.Could.ToString(), Name = "Empire State Building" },
					new DropZoneItem { Zone = TravelPriority.Could.ToString(), Name = "Metropolitan Museum of Art" },
					new DropZoneItem { Zone = TravelPriority.Out.ToString(), Name = "Coney Island" },
					new DropZoneItem { Zone = TravelPriority.Out.ToString(), Name = "High Line" }
					]);
			}
			else
			{
				foreach (var zone in response.Resource.Zones)
				{
					foreach (var item in (zone.Items ?? []).OrderBy((i) => i.Seq))
					{
						Items.Add(item);
					}
				}
			}
			return Items;
		}

		public async Task Save(List<DropZoneItem> items)
		{
			GetContainer();
			try
			{
				var trip = new Trip("0", "0", []);
				foreach (var zone in _zones)
				{
					var newZone = new DropZone
					{
						Name = zone.Name,
						Items = []
					};
					foreach (var item in items.OrderBy(i => i.Seq))
					{
						if (item.Zone == zone.Name)
						{
							newZone.Items.Add(new DropZoneItem() { Zone = zone.Name, Name = item.Name, Link = item.Link, Seq = item.Seq });
						}
					}
					trip.Zones.Add(newZone);
				}
				ItemResponse<Trip> response = await container.UpsertItemAsync<Trip>(
					item: trip,
					partitionKey: new PartitionKey("0")
				);
			}
			catch (CosmosException ex)
			{
				Console.WriteLine($"Cosmos DB error: {ex.Message}");
				throw;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred while saving the itinerary: {ex.Message}");
				throw;

			}
		}
	}

	//	public class ItineraryRepository
	//	{
	//		public ItineraryRepository()
	//		{
	//		}
	//		private readonly List<DropZone> _zones =
	//		[
	//			new() { Name = TravelPriority.Must.ToString() },
	//			new() { Name = TravelPriority.LikeTo.ToString() },
	//			new() { Name = TravelPriority.Could.ToString() },
	//			new() { Name = TravelPriority.Out.ToString() }
	//		];
	//		public Task<List<DropZoneItem>> GetItinerary()
	//		{
	//			List<DropZoneItem> Items = [];
	//			// TODO Call Azure Function to get the trip
	//			//ItemResponse<Trip> response = await container.ReadItemAsync<Trip>(
	//			//   id: "0",
	//			//   partitionKey: new PartitionKey("0")
	//			//  );
	//			//if (response.Resource.Zones == null)
	//			//{
	//			//	// If no zones or items exist, initialize with default items
	//			//	// This is for the first run or if the data is empty
	//			//	Items = new List<DropZoneItem>();
	//			//}
	//			//else if (response.Resource.Zones.Count == 0 || response.Resource.Zones.SelectMany((z) => z.Items!).Count() == 0)
	//			//{
	//			Items.AddRange([
	//					new DropZoneItem { Zone = TravelPriority.Must.ToString(), Name = "Statue of Liberty" },
	//					new DropZoneItem { Zone = TravelPriority.Must.ToString(), Name = "Central Park" },
	//					new DropZoneItem { Zone = TravelPriority.LikeTo.ToString(), Name = "Brooklyn Bridge" },
	//					new DropZoneItem { Zone = TravelPriority.LikeTo.ToString(), Name = "Times Square" },
	//					new DropZoneItem { Zone = TravelPriority.Could.ToString(), Name = "Empire State Building" },
	//					new DropZoneItem { Zone = TravelPriority.Could.ToString(), Name = "Metropolitan Museum of Art" },
	//					new DropZoneItem { Zone = TravelPriority.Out.ToString(), Name = "Coney Island" },
	//					new DropZoneItem { Zone = TravelPriority.Out.ToString(), Name = "High Line" }
	//					]);
	//			//}
	//			//else
	//			//{
	//			//	foreach (var zone in response.Resource.Zones)
	//			//	{
	//			//		foreach (var item in zone.Items ?? new List<DropZoneItem>())
	//			//		{
	//			//			Items.Add(item);
	//			//		}
	//			//	}
	//			//}
	//			return Task.FromResult(Items);
	//		}

	//		public async Task Save(List<DropZoneItem> items)
	//		{
	//			try
	//			{
	//				var trip = new Trip("0", "0", []);
	//				foreach (var zone in _zones)
	//				{
	//					var newZone = new DropZone
	//					{
	//						Name = zone.Name,
	//						Items = []
	//					};
	//					foreach (var item in items)
	//					{
	//						if (item.Zone == zone.Name)
	//						{
	//							newZone.Items.Add(new DropZoneItem() { Zone = zone.Name, Name = item.Name } );
	//						}
	//					}
	//					trip.Zones.Add(newZone);
	//				}

	//				// TODO Call Azure Function to save the trip
	//				//ItemResponse<Trip> response = await container.UpsertItemAsync<Trip>(
	//				//	item: trip,
	//				//	partitionKey: new PartitionKey("0")
	//				//);
	//			}
	//			catch (Exception ex)
	//			{
	//				Console.WriteLine($"An error occurred while saving the itinerary: {ex.Message}");
	//				throw;

	//			}
	//		}
	//	}
}
