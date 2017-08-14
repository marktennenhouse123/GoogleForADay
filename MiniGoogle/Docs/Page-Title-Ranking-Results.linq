<Query Kind="Statements">
  <Connection>
    <ID>6fc9eb97-b4d4-45a9-b7f7-e31b66499617</ID>
    <Persist>true</Persist>
    <Server>ARK-FL-MWS-01\LENOVODB</Server>
    <SqlSecurity>true</SqlSecurity>
    <UserName>testuser</UserName>
    <Password>AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAAnYLfVW8DM06CbEpq5ZDx9AAAAAACAAAAAAADZgAAwAAAABAAAAB9TxqxYuGlufu8jcoTNf4aAAAAAASAAACgAAAAEAAAACOYKHzpsinVDCiHJXs1jUcQAAAAFI2wZCq4uO73pf30P7KDTBQAAAAPYZu6Ut+/pH8RWbmRBzIRu+EWQg==</Password>
    <Database>CloudDB</Database>
    <ShowServer>true</ShowServer>
  </Connection>
</Query>

//var results = from line in Lines
//			  group line by line.ProductCode into g
//			  select new ResultLine
//			  {
//				  ProductName = g.First().Name,
//				  Price = g.Sum(_ => _.Price).ToString(),
//				  Quantity = g.Count().ToString(),
//			  };


string keyword = "is";

var results = (from pg in IndexedPages
			   join pgLinks in PageKeyWords
			   on pg.PageID equals pgLinks.PageID

			   where pgLinks.Keyword.Contains(keyword) || pgLinks.Keyword.StartsWith(keyword)
			   select new {Title = pg.Title, 
			   			   PageURL = pg.PageURL,
						   Ranking = pgLinks.KeywordCount}).ToList();
						   
	//results.Dump();
	
		var final =	 ( from p in results
			  
			   group p by p.PageURL into grup1
			  select new {
				   PageURL = grup1.First().PageURL,
				   Title = grup1.First().Title,
				   Ranking = grup1.Sum(g=>g.Ranking )
			   });
		final.Dump();